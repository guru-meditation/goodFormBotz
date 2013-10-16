using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BotSpace
{
    public class ScanBet365 : Scanner
    {
        private static readonly log4net.ILog log
              = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScanBet365(DriverCreator creator, DbStuff db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        protected override int addLeague(string league)
        {
            return -1;
        }
        private void AddTodaysBet365Matches(int sleepTime, DriverWrapper driver)
        {
            var foundMatches = new List<aMatch>();

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            int dirtySleep = 2000;

            var matchMarket = driver.GetWebElementFromClassAndDivText("Level1", "Match Markets");

            if (matchMarket != null)
            {
                matchMarket.Click();
                driver.DirtySleep(dirtySleep);
            }
            else
            {
                log.Error("Couldn't find Match Markets");
                return;
            }

            var mainGroup = driver.GetWebElementFromClassAndDivText("Level2", "Main");

            if (mainGroup != null)
            {
                mainGroup.Click();
                driver.DirtySleep(dirtySleep);
            }
            else
            {
                log.Error("Couldn't find Main");
                return;
            }

            var fullTimeResult = driver.GetWebElementFromClassAndDivText("genericRow", "Full Time Result");

            if (fullTimeResult != null)
            {
                fullTimeResult.Click();
                driver.ForceSleep(dirtySleep);
            }
            else
            {
                log.Error("Couldn't find Full Time Result");
                return;
            }

            int numGenRows = driver.FindElements(By.ClassName("genericRow")).Count();

            for (int i = 0; i != numGenRows; ++i)
            {
                var genItem = driver.FindElements(By.ClassName("genericRow")).ElementAt(i);

                string leagueText = genItem.Text.Trim();
                genItem.Click();
                driver.DirtySleep(dirtySleep);

                var matches = driver.FindElements(By.ClassName("FixtureDescription"));

                var kickOffs = driver.FindElements(By.ClassName("FixtureStartTime"));

                for (int j = 0; j < matches.Count(); ++j)
                {
                    var m = new aMatch();

                    string matchText = matches.ElementAt(j).Text;

                    if (matchText.Contains("\r\n"))
                    {
                        matchText = matchText.Substring(0, matchText.IndexOf("\r\n"));
                    }

                    if (matchText.Contains(" v "))
                    {
                        var teamSplits = Regex.Split(matchText, " v ");

                        m.team1 = teamSplits.ElementAt(0);
                        m.team2 = teamSplits.ElementAt(1);

                        m.team1 = DoSubstitutions(m.team1);
                        m.team2 = DoSubstitutions(m.team2);
                        m.league = DoSubstitutions(leagueText);

                        m.koDateTime = DateTime.ParseExact(kickOffs.ElementAt(j).Text, "dd MMM HH:mm", CultureInfo.InvariantCulture);

                        foundMatches.Add(m);
                    }
                    else
                    {
                        log.Error("Can't get match from " + matchText);
                    }
                }

                var back = driver.FindElement(By.Id("HeaderBack"));
                back.Click();
                driver.DirtySleep(dirtySleep);

            }

            int longestTeam1 = foundMatches.Select(x => x.team1).Max(x => x.Length);
            int longestTeam2 = foundMatches.Select(x => x.team2).Max(x => x.Length);
            int longestLeague = foundMatches.Select(x => x.league).Max(x => x.Length);

            foreach (aMatch m in foundMatches)
            {
                log.Debug(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " at " + m.koDateTime.TimeOfDay + " in " + m.league);
                int leagueId = dbStuff.AddLeague(m.league);
                int hTeamId = dbStuff.AddTeam(m.team1);
                int aTeamId = dbStuff.AddTeam(m.team2);
                int gameId = dbStuff.AddGame(hTeamId, aTeamId, leagueId, m.koDateTime);
            }

            log.Debug("");
        }

        public override void scan(int sleepTime)
        {
            DriverWrapper driver = null;

            //TODO: this was always set to 0 in original code
            int botIndex = 0;
            int idx = -1;
            bool firstTime = true;

            DateTime lastDayGamesUpdated = DateTime.MinValue;

            if (driver == null)
            {
                string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                driver = driverCreator.CreateDriver(agentString);
            }

            if (botIndex == 0 && skipGames == false)
            {
                log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                AddTodaysBet365Matches(sleepTime, driver);
            }

            lastDayGamesUpdated = DateTime.Today;

            int badLoopCounter = 0;

            while (true)
            {
                idx++;
                try
                {
                    if (driver == null)
                    {
                        string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                        driver = driverCreator.CreateDriver(agentString);
                    }

                    if (DateTime.Today.Equals(lastDayGamesUpdated) == false)
                    {
                        if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(3))
                        {
                            lastDayGamesUpdated = DateTime.Today;
                        }

                        log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                        AddTodaysBet365Matches(sleepTime, driver);
                    }
                    else
                    {
                        log.Info("Already scanned todays games for " + lastDayGamesUpdated.Date);
                    }

                    driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
                    driver.DirtySleep(sleepTime);

                    //string inPlayXPath = "//*[@id=\"sc_0_L1_1-1-5-0-0-0-0-1-1-0-0-0-0-0-1-0-0-0-0\"]";
                    IWebElement inPlayElement = driver.GetWebElementFromClassAndDivText("Level1", "In-Play");

                    driver.ClickElement(inPlayElement);

                    var elements = driver.FindElements(By.ClassName("IPScoreTitle"));

                    if (elements.Count() == 0)
                    {
                        log.Debug("No games in play, going to sleep for a bit....");
                        driver.DirtySleep(20000);
                        driver.Quit();
                        driver.Dispose();
                        driver = null;
                        firstTime = true;
                        continue;
                    }

                    if (firstTime)
                    {
                        Random random = new Random();
                        idx = random.Next(0, elements.Count());
                        firstTime = false;
                    }

                    log.Info("Scanning game " + idx + " of " + elements.Count() + " games in play");

                    if (idx < elements.Count())
                    {
                        var hstats = new Dictionary<string, int>();
                        var astats = new Dictionary<string, int>();

                        int attempts = 3;

                        //*[@id="rw_spl_sc_1-1-5-24705317-2-0-0-1-1-0-0-0-0-0-1-0-0_101"]/div[1]
                        elements.ElementAt(idx).Click();
                        //XXX: click causes a nice animation that takes some time,
                        //XXX: if we forcesleep for shorter time than animation takes then cleanScores will be null!!
                        //TODO: I changed this to make it faster, but we need to find out a way how to get rid of ForceSleep
                        driver.ForceSleep(6000);


                        var cleanScores = driver.GetValuesByClassName("clock-score", attempts, 3, new char[] { ' ', '-', '\r', '\n' });

                        if (cleanScores == null)
                        {
                            log.Warn("cleanScores == null");

                            ++badLoopCounter;

                            if (badLoopCounter == 5)
                            {
                                log.Warn("Bad loop counter reset...");

                                badLoopCounter = 0;
                                driver.Quit();
                                driver.Dispose();
                                driver = null;
                            }

                            continue;
                        }

                        driver.ClickElement("//*[@id=\"arena\"]");
                        driver.DirtySleep(2000);

                        var hCardsAndCorners = driver.GetValuesById("team1IconStats", attempts, 3, " ");

                        if (hCardsAndCorners == null)
                        {
                            log.Warn("hCardsAndCorners == null");
                            log.Warn("Resetting driver...");

                            ++badLoopCounter;

                            if (badLoopCounter == 5)
                            {
                                badLoopCounter = 0;
                                driver.Quit();
                                driver.Dispose();
                                driver = null;
                            }

                            continue;
                        }

                        //*[@id="team1IconStats"]
                        var aCardsAndCorners = driver.GetValuesById("team2IconStats", attempts, 3, " ");
                        if (aCardsAndCorners == null) { log.Warn("aCardsAndCorners == null"); continue; }

                        var inPlayTitles = driver.GetValuesByClassName("InPlayTitle", attempts, 1, new char[] { '@' });
                        if (inPlayTitles == null) { log.Warn("inPlayTitles == null"); continue; }

                        bool rballOkay = true;

                        var shotsOnTarget = driver.GetValuesById("stat1", attempts, 3, "\r\n");
                        if (shotsOnTarget == null) { log.Warn("shotsOnTarget == null"); rballOkay = false; }

                        List<string> shotsOffTarget = null;
                        List<string> attacks = null;
                        List<string> dangerousAttacks = null;

                        if (rballOkay == true)
                        {
                            shotsOffTarget = driver.GetValuesById("stat2", attempts, 3, "\r\n");
                            if (shotsOffTarget == null) { log.Warn("shotsOffTarget == null"); rballOkay = false; }

                            attacks = driver.GetValuesById("stat3", attempts, 3, "\r\n");
                            if (attacks == null) { log.Warn("attacks == null"); rballOkay = false; }

                            dangerousAttacks = driver.GetValuesById("stat4", attempts, 3, "\r\n");
                            if (dangerousAttacks == null) { log.Warn("dangerousAttacks == null"); rballOkay = false; }
                        }

                        cleanScores.RemoveAll(x => String.IsNullOrEmpty(x));
                        string time = cleanScores.ElementAt(2);
                        string inPlayTitle = inPlayTitles.ElementAt(0);

                        if (String.IsNullOrEmpty(time))
                        {
                            log.Warn("Couldn't get time :(");
                            continue;
                        }

                        var vals = new List<string>();
                        hstats[statType[10]] = ParseInt(statType[10], hCardsAndCorners.ElementAt(0));
                        hstats[statType[9]] = ParseInt(statType[9], hCardsAndCorners.ElementAt(1));
                        hstats[statType[6]] = ParseInt(statType[6], hCardsAndCorners.ElementAt(2));

                        astats[statType[10]] = ParseInt(statType[10], aCardsAndCorners.ElementAt(0));
                        astats[statType[9]] = ParseInt(statType[9], aCardsAndCorners.ElementAt(1));
                        astats[statType[6]] = ParseInt(statType[6], aCardsAndCorners.ElementAt(2));

                        if (rballOkay)
                        {
                            hstats[statType[3]] = ParseInt(statType[3], shotsOnTarget.ElementAt(0));
                            astats[statType[3]] = ParseInt(statType[3], shotsOnTarget.ElementAt(2));

                            hstats[statType[4]] = ParseInt(statType[4], shotsOffTarget.ElementAt(0));
                            astats[statType[4]] = ParseInt(statType[4], shotsOffTarget.ElementAt(2));

                            hstats[statType[11]] = ParseInt(statType[11], attacks.ElementAt(0));
                            astats[statType[11]] = ParseInt(statType[11], attacks.ElementAt(2));

                            hstats[statType[12]] = ParseInt(statType[12], dangerousAttacks.ElementAt(0));
                            astats[statType[12]] = ParseInt(statType[12], dangerousAttacks.ElementAt(2));
                        }

                        hstats[statType[1]] = ParseInt(statType[1], cleanScores.ElementAt(0));
                        astats[statType[1]] = ParseInt(statType[1], cleanScores.ElementAt(1));

                        var teams = Regex.Split(inPlayTitle, " v ");

                        string homeTeamName = teams.ElementAt(0);
                        string awayTeamName = teams.ElementAt(1);

                        string today = DateTime.Now.ToString("ddMMyy");
                        string league = "All";

                        string yesterday = (DateTime.Today - TimeSpan.FromDays(1)).ToString("ddMMyy");
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
                        sd.BeginInvoke(league, bOverMidnight ? DateTime.Today - TimeSpan.FromDays(1) : DateTime.Now, homeTeamName, awayTeamName, hstats, astats, time, null, null);
                        //SendToWeb(league, bOverMidnight ? DateTime.Today - TimeSpan.FromDays(1) : DateTime.Now, homeTeamName, awayTeamName, hstats, astats, time);

                        WriteXmlDelegate wd = new WriteXmlDelegate(WriteXml);
                        wd.BeginInvoke(xmlPath, hstats, astats, homeTeamName, awayTeamName, league, time, exists, finalName, null, null);
                    }
                    else
                    {
                        idx = -1;
                    }
                }
                catch (OpenQA.Selenium.WebDriverException we)
                {
                    log.Error("Exception thrown: " + we);
                    driver.Quit();
                    driver.Dispose();
                    driver = null;

                }
                catch (Exception we)
                {
                    log.Error("Exception thrown: " + we);
                    driver.Quit();
                    driver.Dispose();
                    driver = null;
                }
            }
        }

    }

}
