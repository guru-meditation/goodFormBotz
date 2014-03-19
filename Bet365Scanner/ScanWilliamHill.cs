using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Scanners
{
    using BotSpace;
    using Db;

    public class ScanWilliamHill : Scanner
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScanWilliamHill(DriverCreator creator, Database db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        public override void scan(int sleepTime = 0)
        {
            DriverWrapper driver = null;

            ScoreBoardFinder sbf = new ScoreBoardFinder(sleepTime);
            sbf.Start();

            while (true)
            {
                try
                {
                    if (driver == null)
                    {
                        driver = driverCreator.CreateDriver("");
                    }

                    var hstats = new Dictionary<string, int>();
                    var astats = new Dictionary<string, int>();

                    var copyOfTheList = sbf.GetWebBlobs();

                    log.Debug("There are " + copyOfTheList.Count() + " games in the blob list");
                    if (copyOfTheList.Count() == 0)
                    {
                        var downTime = 20000;
                        log.Debug("Going to sleep for " + downTime + " miiliseconds");
                        driver.ForceSleep(downTime);
                        continue;
                    }

                    foreach (var blob in copyOfTheList)
                    {
                        log.Debug("On the blob loop...");

                        string homeTeamName = blob.HomeTeam;
                        string awayTeamName = blob.AwayTeam;
                        string league = blob.League;

                        string scoreUrl = blob.Url;

                        driver.Url = scoreUrl;
                        driver.DirtySleep(sleepTime);

                        string text = driver.GetElementText("//*[@id=\"commentaryContent\"]");

                        int totalDAs = -1;
                        int totalAs = -1;
                        int totalCs = -1;
                        int totalBs = -1;

                        int homeDAs = -1;
                        int homeAs = -1;
                        int homeCs = -1;
                        int homeBs = -1;
                        int homeSonTs = -1;
                        int homeSoffTs = -1;

                        int awayDAs = -1;
                        int awayAs = -1;
                        int awayCs = -1;
                        int awayBs = -1;
                        int awaySonTs = -1;
                        int awaySoffTs = -1;

                        if (string.IsNullOrEmpty(text) == false)
                        {
                            var splits = text.Split('\n').ToList();

                            splits.RemoveAll(x => String.IsNullOrEmpty(x));
                            splits.RemoveAll(x => x[0] == ' ');
                            splits.RemoveAll(x => char.IsDigit(x[0]));

                            var distinct = splits.Distinct();

                            var query = distinct.Select(g => new { Name = g, Count = g.Count() });

                            splits.ForEach(x => x.Trim());

                            totalDAs = splits.Count(x => x.StartsWith("Dangerous Attack by"));
                            totalAs = splits.Count(x => x.StartsWith("Attack by"));
                            totalCs = splits.Count(x => x.StartsWith("Clearance by"));
                            totalBs = splits.Count(x => x.StartsWith("Blocked Shot for"));

                            homeDAs = splits.Count(x => x.StartsWith("Dangerous Attack by " + homeTeamName));
                            homeAs = splits.Count(x => x.StartsWith("Attack by " + homeTeamName));
                            homeCs = splits.Count(x => x.StartsWith("Clearance by " + homeTeamName));
                            homeBs = splits.Count(x => x.StartsWith("Blocked Shot for " + homeTeamName));
                            homeSonTs = splits.Count(x => x.StartsWith("Shot On Target for " + homeTeamName));
                            homeSoffTs = splits.Count(x => x.StartsWith("Shot Off Target for " + homeTeamName));

                            awayDAs = splits.Count(x => x.StartsWith("Dangerous Attack by " + awayTeamName));
                            awayAs = splits.Count(x => x.StartsWith("Attack by " + awayTeamName));
                            awayCs = splits.Count(x => x.StartsWith("Clearance by " + awayTeamName));
                            awayBs = splits.Count(x => x.StartsWith("Blocked Shot for " + awayTeamName));
                            awaySonTs = splits.Count(x => x.StartsWith("Shot On Target for " + awayTeamName));
                            awaySoffTs = splits.Count(x => x.StartsWith("Shot Off Target for " + awayTeamName));

                            if (homeDAs + awayDAs != totalDAs)
                            {
                                if (homeDAs == 0) homeDAs = totalDAs - awayDAs;
                                if (awayDAs == 0) awayDAs = totalDAs - homeDAs;
                            }

                            if (homeAs + awayAs != totalAs)
                            {
                                if (homeAs == 0) homeAs = totalAs - awayAs;
                                if (awayAs == 0) awayAs = totalAs - homeAs;
                            }

                            if (homeCs + awayCs != totalCs)
                            {
                                if (homeCs == 0) homeCs = totalCs - awayCs;
                                if (awayCs == 0) awayCs = totalCs - homeCs;
                            }

                            if (homeBs + awayBs != totalBs)
                            {
                                if (homeBs == 0) homeBs = totalBs - awayBs;
                                if (awayBs == 0) awayBs = totalBs - homeBs;
                            }
                        }

                        log.Debug("Game:\t\t" + homeTeamName + " v " + awayTeamName);

                        if (String.IsNullOrEmpty(scoreUrl) == false)
                        {

                            string previewText = driver.GetElementText("//*[@id=\"previewContents\"]");
                            string time = driver.GetElementText("//*[@id=\"time\"]");
                            string period = driver.GetElementText("//*[@id=\"period\"]");

                            if (string.IsNullOrEmpty(time))
                            {
                                time = period;
                            }

                            //"Ivory Coast\r\nTogo\r\n56%\r\n44%"
                            //"TotalNormal Time1st Half2nd Half\r\nIvory Coast 2 0 5 5 1 5 18 22 1 0\r\nTogo 1 0 4 9 0 6 16 21 3 0"
                            bool clickedOk = driver.ClickElement("//*[@id=\"statisticsTab\"]");

                            if (!clickedOk)
                            {
                                log.Error("======> Click failed");
                            }

                            var previewSplits = Regex.Split(previewText, "\r\n").ToList();
                            string aPossession = previewSplits.Last().Replace("%", "");
                            previewSplits.RemoveAt(previewSplits.Count() - 1);
                            string hPossession = previewSplits.Last().Replace("%", "");

                            string statsText = driver.GetElementText("//*[@id=\"statsTable\"]/tbody");

                            string homeStatsText = Regex.Split(statsText, "\r\n").ToList().ElementAt(0);
                            string awayStatsText = Regex.Split(statsText, "\r\n").ToList().ElementAt(1);

                            var homeStatsList = Regex.Split(homeStatsText, " ").ToList();
                            var justHomeStats = homeStatsList.GetRange(homeStatsList.Count() - 10, 10);

                            var awayStatsList = Regex.Split(awayStatsText, " ").ToList();
                            var justAwayStats = awayStatsList.GetRange(awayStatsList.Count() - 10, 10);

                            hstats[statType[0]] = ParseInt(statType[0], hPossession);

                            for (int i = 0; i != 10; ++i)
                            {
                                int parseResult = ParseInt(statType[i + 1], justHomeStats[i]);
                                if (parseResult == -1 && (i == 0 || i == 5 || i == 8 || i == 9))
                                {
                                    hstats[statType[i + 1]] = 0;
                                }
                                else
                                {
                                    hstats[statType[i + 1]] = parseResult;
                                }
                            }

                            if (hstats[statType[3]] == -1)
                            {
                                hstats[statType[3]] = homeSonTs;
                            }

                            if (hstats[statType[4]] == -1)
                            {
                                hstats[statType[4]] = homeSoffTs;

                            }

                            hstats[statType[11]] = homeAs;
                            hstats[statType[12]] = homeDAs;
                            hstats[statType[13]] = homeBs;
                            hstats[statType[14]] = homeCs;

                            astats[statType[0]] = ParseInt(statType[0], aPossession);

                            for (int i = 0; i != 10; ++i)
                            {
                                int parseResult = ParseInt(statType[i + 1], justAwayStats[i]);
                                if (parseResult == -1 && (i == 0 || i == 5 || i == 8 || i == 9))
                                {
                                    astats[statType[i + 1]] = 0;
                                }
                                else
                                {
                                    astats[statType[i + 1]] = parseResult;
                                }
                            }

                            if (astats[statType[3]] == -1)
                            {
                                astats[statType[3]] = awaySonTs;
                            }

                            if (astats[statType[4]] == -1)
                            {
                                astats[statType[4]] = awaySoffTs;
                            }

                            astats[statType[11]] = awayAs;
                            astats[statType[12]] = awayDAs;
                            astats[statType[13]] = awayBs;
                            astats[statType[14]] = awayCs;

                            homeTeamName = DoSubstitutions(homeTeamName);
                            awayTeamName = DoSubstitutions(awayTeamName);
                            league = DoSubstitutions(league);

                            bool homeTeamLongest = homeTeamName.Length > awayTeamName.Length;

                            log.Info("League:\t\t" + league + " at " + time);
                            log.Info(homeTeamName.PadRight(homeTeamLongest ? homeTeamName.Length + 1 : awayTeamName.Length + 1) + String.Join(" ", hstats.Values));
                            log.Info(awayTeamName.PadRight(homeTeamLongest ? homeTeamName.Length + 1 : awayTeamName.Length + 1) + String.Join(" ", astats.Values));

                            if (hstats.Keys.Any(x => x == "-1") || astats.Keys.Any(x => x == "-1"))
                            {
                                log.Warn("Bad Stat detected.... skipping");
                                continue;
                            }

                            string today = DateTime.Now.ToUniversalTime().ToString("ddMMyy");
                            string yesterday = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(1)).ToString("ddMMyy");
                            string finalName = Path.Combine(xmlPath, league, homeTeamName + " v " + awayTeamName + "_" + today + ".xml");

                            bool exists = File.Exists(finalName);

                            //edge case of games going over midnight
                            bool bOverMidnight = false;
                            if (exists == false)
                            {
                                string anotherName = Path.Combine(xmlPath, league, homeTeamName + " v " + awayTeamName + "_" + yesterday + ".xml");
                                if (File.Exists(anotherName))
                                {
                                    finalName = anotherName;
                                    exists = true;
                                    bOverMidnight = true;
                                }
                            }

                            SendToWebDelegate sd = new SendToWebDelegate(SendToWeb);
                            sd.BeginInvoke(league, bOverMidnight ? DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(1) : DateTime.Now.ToUniversalTime(), homeTeamName, awayTeamName, hstats, astats, time, null, null);

                            WriteXmlDelegate wd = new WriteXmlDelegate(WriteXml);
                            wd.BeginInvoke(xmlPath, hstats, astats, homeTeamName, awayTeamName, league, time, exists, finalName, null, null);
                        }
                    }
                }
                catch (Exception ce)
                {
                    log.Error("Exception caught: " + ce);
                    if (driver != null)
                    {
                        driver.Quit();
                        driver.Dispose();
                        driver = null;
                    }
                }
            }
        }
    }

}
