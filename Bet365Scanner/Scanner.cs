using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BotSpace
{
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
             {"Phillipine", "Philippine"}
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

        protected DriverCreator driverCreator = null;
        protected DbStuff dbStuff = null;
        protected int keyClashRetries = 5;
        protected bool skipGames = false;
        protected string xmlPath;

        public Scanner(DriverCreator creator, DbStuff db, string xml_path, bool skip_games)
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
            snap.SetAttributeValue("Seen", DateTime.Now.ToString("MM/dd/yy H:mm:ss"));

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
            int hTeamId = dbStuff.AddTeam(homeTeam);
            int aTeamId = dbStuff.AddTeam(awayTeam);
            int gameId = dbStuff.AddGame(hTeamId, aTeamId, leagueId, koDate);

            List<int> allStats = new List<int>();
            allStats.AddRange(hstats.Values);
            allStats.AddRange(astats.Values);

            int retries = 0;

            log.Debug("Adding " + homeTeam + " [" + hTeamId + "] v " + awayTeam + " [" + aTeamId + "] in league [" + leagueId + "] with game id: " + gameId + " at time: " + time);

            log.Debug("Goals Corners");
            log.Debug(hstats[statType[1]].ToString().PadRight(6) + hstats[statType[6]].ToString());
            log.Debug(astats[statType[1]].ToString().PadRight(6) + astats[statType[6]].ToString());

            while (retries < keyClashRetries)
            {
                try
                {
                    dbStuff.AddStatistics(allStats, gameId, time, "", koDate);
                }
                catch (DbException ne)
                {
                    log.Warn("Retrying....");
                    retries += 1;
                }
                break;
            }
        }

        protected virtual int addLeague(string league)
        {
            return dbStuff.AddLeague(league);
        }
        public virtual void scan(int sleepTime = 0) { }
    }

    public class UpdateFromXmlToWeb : Scanner
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateFromXmlToWeb(DriverCreator creator, DbStuff db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        private void Upload(string league, DateTime koDate, string homeTeam, string awayTeam, IEnumerable<XElement> snaps)
        {
            int leagueId = addLeague(league);      

            int hTeamId = dbStuff.AddTeam(homeTeam);
            int aTeamId = dbStuff.AddTeam(awayTeam);
            int gameId = dbStuff.AddGame(hTeamId, aTeamId, leagueId, koDate);

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
                            result = dbStuff.AddStatistics(values, gameId, time, lastTime, seenTime);
                        }
                        catch (DbException ne)
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
                            dbStuff.AddStatistics(values, gameId, time, "", seenTime);
                        }
                        catch (DbException ne)
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
        public UploadBet365(DriverCreator creator, DbStuff db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        protected override int addLeague(string league)
        {
            return -1;
        }
    }

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
            System.Threading.Thread.Sleep(sleepTime);

            int dirtySleep = 2000;

            var matchMarket = driver.GetWebElementFromClassAndDivText("Level1", "Match Markets");

            if (matchMarket != null)
            {
                matchMarket.Click();
                System.Threading.Thread.Sleep(dirtySleep);
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
                System.Threading.Thread.Sleep(dirtySleep);
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
                System.Threading.Thread.Sleep(dirtySleep);
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
                System.Threading.Thread.Sleep(dirtySleep);

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
                System.Threading.Thread.Sleep(dirtySleep);

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
                    System.Threading.Thread.Sleep(sleepTime);

                    //string inPlayXPath = "//*[@id=\"sc_0_L1_1-1-5-0-0-0-0-1-1-0-0-0-0-0-1-0-0-0-0\"]";
                    IWebElement inPlayElement = driver.GetWebElementFromClassAndDivText("Level1", "In-Play");

                    driver.ClickElement(inPlayElement);

                    var elements = driver.FindElements(By.ClassName("IPScoreTitle"));

                    if (elements.Count() == 0)
                    {
                        log.Debug("No games in play, going to sleep for a bit....");
                        System.Threading.Thread.Sleep(20000);
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
                        System.Threading.Thread.Sleep(10000);

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
                        System.Threading.Thread.Sleep(2000);

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

    public class ScanWilliamHill : Scanner
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScanWilliamHill(DriverCreator creator, DbStuff db, string xml_path, bool skip_games)
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
                        System.Threading.Thread.Sleep(downTime);
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
                        System.Threading.Thread.Sleep(sleepTime);

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

                            string today = DateTime.Now.ToString("ddMMyy");
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
