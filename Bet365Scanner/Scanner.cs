﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Scanners
{
    using BotSpace;
    using Db;

    public class Scanner
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static Dictionary<string, string> subs = new Dictionary<string, string>()
        {
             {"%28", ""},
             {"%29", ""},
             {"%2e", "."},
             {"%2", "."},
             {"%26", "&"},
             {"%7c", ""},
             {"1.", ""},
             {"1. ", ""},
             {"1.e ", ""},
             {"1.e", ""},
             {".e", "."},
             {".6", "&"},
             {"'", ""},
             {"/", " "},
             {"Phillipine", "Philippine"}
        };


        protected enum StatAlias
        {
            Possession = 0,
            Goals = 1,
            Penalties = 2,
            ShotsOnTarget = 3,
            ShotsOffTarget = 4,
            Woodwork = 5,
            Corners = 6,
            FreeKicks = 7,
            ThrowIns = 8,
            YellowCards = 9,
            RedCards = 10,
            Attacks = 11,
            DangerousAttacks = 12,
            BlockedShots = 13,
            Clearances = 14
        };

        protected static string[] statType = { "Possession", 
                                              "Goals", 
                                              "Penalties", 
                                              "ShotsOnTarget", 
                                              "ShotsOffTarget", 
                                              "Woodwork", 
                                              "Corners", 
                                              "FreeKicks", 
                                              "ThrowIns", 
                                              "YellowCards", 
                                              "RedCards", 
                                              "Attacks", 
                                              "DangerousAttacks", 
                                              "BlockedShots", 
                                              "Clearances" };

        protected string stat(StatAlias alias)
        {
            return statType[(int)alias];
        }

        protected DriverCreator driverCreator = null;
        protected Database dbStuff = null;
        protected int keyClashRetries = 5;
        protected bool skipGames = false;
        protected string xmlPath;

        public Scanner(DriverCreator creator, Database db, string xml_path, bool skip_games)
        {
            driverCreator = creator;
            dbStuff = db;
            xmlPath = xml_path;
            skipGames = skip_games;
        }

        protected int ParseInt(string statType, string valToParse)
        {
            int result = -1;
            try
            {
                result = int.Parse(valToParse);

            }
            catch (Exception)
            {
                log.Warn("Failed to parse " + statType + " input: " + valToParse);
            }

            return result;
        }
        protected string DoSubstitutions(string aString)
        {
            foreach (var sub in subs)
            {
                if (aString.Contains(sub.Key))
                {
                    aString = Regex.Replace(aString, sub.Key, sub.Value);
                }
            }
            return aString;
        }

        public delegate void SendToWebDelegate(string league, DateTime koDate, string homeTeam, string awayTeam, Dictionary<string, int> hstats, Dictionary<string, int> astats, string time);
        public delegate void WriteXmlDelegate(string path, Dictionary<string, int> hstats, Dictionary<string, int> astats, string homeTeamName, string awayTeamName, string league, string time, bool exists, string finalName);

        protected void WriteXml(string path, Dictionary<string, int> hstats, Dictionary<string, int> astats, string homeTeamName, string awayTeamName, string league, string time, bool exists, string finalName)
        {
            if (Directory.Exists(Path.Combine(path, league)) == false)
            {
                Directory.CreateDirectory(Path.Combine(path, league));
            }

            XElement home = new XElement("Home",
               from keyValue in hstats
               select new XElement(keyValue.Key, keyValue.Value.ToString())
           );

            XElement away = new XElement("Away",
                from keyValue in astats
                select new XElement(keyValue.Key, keyValue.Value.ToString())
            );

            XElement snap = new XElement("Snap", home, away);
            snap.SetAttributeValue("Time", time);
            snap.SetAttributeValue("Seen", DateTime.Now.ToUniversalTime().ToString("MM/dd/yy H:mm:ss"));

            if (exists == false)
            {
                XElement game = new XElement("Game", snap);
                game.SetAttributeValue("Home", homeTeamName);
                game.SetAttributeValue("Away", awayTeamName);

                var xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                xdoc.Add(game);

                // save the document
                xdoc.Save(finalName);
            }
            else
            {
                var xdoc = XDocument.Load(finalName);
                var xGame = xdoc.Descendants("Game").First();
                string lastSnapTime = xGame.Descendants("Snap").Last().FirstAttribute.Value;

                if (lastSnapTime.Split(':').First() == time.Split(':').First())
                {
                    log.Info("Already seen this minute of the " + homeTeamName + " vs " + awayTeamName + " game!");
                }
                else
                {
                    xGame.Add(snap);
                    xdoc.Save(finalName);
                }
            }
        }

        protected void SendToWeb(string league, DateTime koDate, string homeTeam, string awayTeam, Dictionary<string, int> hstats, Dictionary<string, int> astats, string time)
        {
            int leagueId = addLeague(league);
            int hTeamId = AddTeam.DoIt(dbStuff, homeTeam);
            int aTeamId = AddTeam.DoIt(dbStuff, awayTeam);
            int gameId = AddGame.DoIt(dbStuff, hTeamId, aTeamId, leagueId, koDate);

            List<int> allStats = new List<int>();
            allStats.AddRange(hstats.Values);
            allStats.AddRange(astats.Values);

            int retries = 0;

            log.Debug("Adding " + homeTeam + " [" + hTeamId + "] v " + awayTeam + " [" + aTeamId + "] in league [" + leagueId + "] with game id: " + gameId + " at time: " + time);

            log.Debug("Goals Corners");

            var line1 = hstats[statType[1]].ToString().PadRight(6) + hstats[statType[6]].ToString();
            var line2 = astats[statType[1]].ToString().PadRight(6) + astats[statType[6]].ToString();

            log.Debug(line1);
            log.Debug(line2);

            while (retries < keyClashRetries)
            {
                try
                {
                    AddStatistics.DoIt(dbStuff, isBet365(), allStats, gameId, time, "", koDate);
                }
                catch (DbException)
                {
                    log.Warn("Retrying....");
                    retries += 1;
                }
                break;
            }
        }

        virtual protected bool isBet365()
        {
            return true;
        }

        protected virtual int addLeague(string league)
        {
            return AddLeague.DoIt(dbStuff, league);
        }
        public virtual void scan(int sleepTime = 0) { }
    }

    public class UpdateFromXmlToWeb : Scanner
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateFromXmlToWeb(DriverCreator creator, Database db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        protected override bool isBet365()
        {
            return false;
        }

        private void Upload(string league, DateTime koDate, string homeTeam, string awayTeam, IEnumerable<XElement> snaps)
        {
            int leagueId = addLeague(league);

            int hTeamId = AddTeam.DoIt(dbStuff, homeTeam);
            int aTeamId = AddTeam.DoIt(dbStuff, awayTeam);
            int gameId = AddGame.DoIt(dbStuff, hTeamId, aTeamId, leagueId, koDate);

            log.Debug("Adding " + homeTeam + " [" + hTeamId + "] v " + awayTeam + " [" + aTeamId + "] in league [" + leagueId + "] with game id: " + gameId);
            SendStats(snaps, gameId);
        }

        private void SendStats(IEnumerable<XElement> snaps, int gameId)
        {
            var lastSnap = snaps.Last();
            bool firstSnap = true;

            foreach (var snap in snaps)
            {
                string time = snap.Attribute("Time").Value;

                //<Snap Time="Full Time" Seen="01/27/13 5:30:04">
                //<Snap Time="Half Time" Seen="01/27/13 4:23:48">
                //<Snap Time="90:19" Seen="01/27/13 5:23:57">
                DateTime seenTime = GetSeenTime(snap);

                var hstats = snap.Descendants("Home").First();
                var astats = snap.Descendants("Away").First();

                Dictionary<string, int> statsStore = new Dictionary<string, int>();
                List<int> values = new List<int>();

                foreach (var hstat in hstats.Descendants())
                {
                    statsStore["Home" + hstat.Name.ToString()] = int.Parse(hstat.Value);
                    values.Add(int.Parse(hstat.Value));
                }

                foreach (var astat in astats.Descendants())
                {
                    statsStore["Away" + astat.Name.ToString()] = int.Parse(astat.Value);
                    values.Add(int.Parse(astat.Value));
                }

                if (firstSnap == true)
                {
                    firstSnap = false;
                    string lastTime = lastSnap.Attribute("Time").Value;

                    int retries = 0;
                    bool result = false;

                    while (retries < keyClashRetries)
                    {
                        try
                        {
                            result = AddStatistics.DoIt(dbStuff, isBet365(), values, gameId, time, lastTime, seenTime);
                        }
                        catch (DbException)
                        {
                            retries += 1;
                        }
                        break;
                    }

                    if (result == false)
                    {
                        break;
                    }
                }
                else
                {
                    int retries = 0;

                    while (retries < keyClashRetries)
                    {
                        try
                        {
                            AddStatistics.DoIt(dbStuff, isBet365(), values, gameId, time, "", seenTime);
                        }
                        catch (DbException)
                        {
                            retries += 1;
                        }
                        break;
                    }
                }
            }
        }
        private DateTime GetSeenTime(XElement snap)
        {
            string seen = snap.Attribute("Seen").Value;
            string[] bits = Regex.Split(seen, " ");
            string[] dates = bits[0].Split('/');
            string[] times = bits[1].Split(':');

            return new DateTime(int.Parse(dates[2]) + 2000, int.Parse(dates[0]), int.Parse(dates[1]), int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2])); ;
        }

        public override void scan(int sleepTime = 0)
        {
            try
            {
                string path = xmlPath;

                log.Info("Program starting from " + path);

                var dirs = Directory.GetDirectories(path);

                foreach (var dir in dirs)
                {
                    string league = Path.GetFileName(dir);

                    var xmls = Directory.GetFiles(Path.Combine(path, dir)).Where(x => x.EndsWith(".xml"));

                    foreach (var xml in xmls)
                    {
                        log.Info("Loading: " + xml);
                        XDocument xdoc = null;

                        try
                        {
                            xdoc = XDocument.Load(xml);
                        }
                        catch (Exception ce)
                        {
                            log.Error("Failed to load: " + ce);
                            File.Move(xml, xml + ".malformed");
                        }

                        if (xdoc != null)
                        {
                            var game = xdoc.Descendants("Game");
                            string koDateString = xml.Substring(xml.Length - 10, 6);
                            DateTime koDate = DateTime.ParseExact(koDateString, "ddMMyy", null);

                            string homeTeam = game.First().Attribute("Home").Value;
                            string awayTeam = game.First().Attribute("Away").Value;

                            homeTeam = DoSubstitutions(homeTeam);
                            awayTeam = DoSubstitutions(awayTeam);
                            league = DoSubstitutions(league);

                            log.Debug(league);
                            log.Debug(homeTeam + " v " + awayTeam + " " + koDateString);

                            var snaps = game.Descendants("Snap");

                            var hstats = new Dictionary<string, int>();
                            var astats = new Dictionary<string, int>();

                            Upload(league, koDate, homeTeam, awayTeam, snaps);

                            bool renamedOk = false;
                            int qwert = 0;
                            while (renamedOk == false)
                            {
                                string uploadedFileName = qwert == 0 ? xml + ".uploaded" : xml + "." + qwert + ".uploaded";
                                try
                                {
                                    File.Move(xml, uploadedFileName);
                                    renamedOk = true;
                                }
                                catch
                                {
                                    log.Warn("cannot overwrite: " + uploadedFileName);
                                    qwert += 1;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                log.Error("Exception: " + e);
            }
            finally
            {
                log.Debug("Finally");
            }
        }
    }

    public class UploadBet365 : UpdateFromXmlToWeb
    {
        public UploadBet365(DriverCreator creator, Database db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        protected override int addLeague(string league)
        {
            return -1;
        }
    }



}
