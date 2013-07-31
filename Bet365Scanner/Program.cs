
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using System.Globalization;
//using MySql.Data.MySqlClient;


namespace BotSpace
{
    public class aMatch
    {
        public string team1;
        public string team2;
        public string league;
        public DateTime koDateTime;

        public override string ToString()
        {
            return team1 + " v " + team2 + " at " + koDateTime + " in " + league;
        }
    }

    public class TheBot
    {
        public static string[] statType = { "Possession", 
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

        public static string willHillUrl = "http://sports.williamhill.com/bet/en-gb/betlive/9";

        static string site = ConfigurationSettings.AppSettings["site"];
        static string connectionString = ConfigurationSettings.AppSettings["connection1"];
        static string dbtype = ConfigurationSettings.AppSettings["dbtype"];
        static string xmlPath = ConfigurationSettings.AppSettings["xmlPath"];
        static string sleepTime = ConfigurationSettings.AppSettings["sleeptime"];

        private static String GetElementText(IWebDriver driver, string xpath)
        {
            String result = String.Empty;

            try
            {
                IWebElement iwe = driver.FindElement(By.XPath(xpath));
                result = iwe.Text;
            }
            catch (Exception)
            {
                Console.WriteLine("=========> Exception thrown trying to find element: " + xpath);
            }

            return result;
        }

        private static bool ClickElement(IWebDriver driver, IWebElement iwe)
        {
            bool result = false;

            try
            {
                if (iwe != null)
                {
                    iwe.Click();
                    result = true;
                }
                else
                {
                    Console.WriteLine("Couldn't click NULL web element");
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine("=========> Exception thrown trying to click element: " + iwe.TagName + " [" + ce + "]");
            }

            return result;
        }

        private static bool ClickElement(IWebDriver driver, string xpath)
        {
            bool result = false;

            try
            {
                IWebElement iwe = driver.FindElement(By.XPath(xpath));
                if (iwe != null)
                {
                    iwe.Click();
                    result = true;
                }
                else
                {
                    Console.WriteLine("Couldn't find " + xpath + " to click");
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine("=========> Exception thrown trying to click element: " + xpath + "[" + ce + "]");
            }

            return result;
        }


        private static int ParseInt(string statType, string valToParse)
        {
            int result = -1;
            try
            {
                result = int.Parse(valToParse);

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to parse " + statType + " input: " + valToParse);
            }

            return result;
        }

        enum OperationMode
        {
            WilliamHillScan,
            Bet365Scan,
            UploadWilliamHill,
            UploadBet365
        }

        static OperationMode                        gOpMode = OperationMode.WilliamHillScan;
        static int                                  numBots = 1;
        static int                                  botIndex = 0;
        static bool                                 gSkipAddGames = false;
        static int                                  gKeyClashRetries = 5;
        static List<NpgsqlConnection>               pgConnectionList = new List<NpgsqlConnection>();
        //static MySqlConnection  msConnection = null;

        static void Main(string[] args)
        {
            int r = 0;

            foreach (string arg in args)
            {
                Console.WriteLine("args[" + r + "] " + arg);

                if (arg.ToLower().Contains("-m:willhill"))
                {
                    gOpMode = OperationMode.WilliamHillScan;
                }

                if (arg.ToLower().Contains("-m:bet365"))
                {
                    gOpMode = OperationMode.Bet365Scan;
                }

                if (arg.ToLower().Contains("-n:"))
                {
                    try
                    {
                        numBots = int.Parse(arg.Substring("-n:".Length));
                    }
                    catch 
                    {
                        Console.WriteLine("Warning: Couldn't parse number of bots: " + arg);
                    }
                }

                if (arg.ToLower().Contains("-m:uploadwh"))
                {
                    gOpMode = OperationMode.UploadWilliamHill;
                }

                if (arg.ToLower().Contains("-m:upload365"))
                {
                    gOpMode = OperationMode.UploadBet365;
                }

                if (arg.ToLower().Contains("-p:"))
                {
                    xmlPath = arg.Substring("-p:".Length);
                }

                if (arg.ToLower().Contains("-m:skip"))
                {
                    gSkipAddGames = true;
                }

                if (arg.ToLower().Contains("-m:scratch"))
                {
                    ScratchPad();
                    return;
                }
                ++r;
            }

            Console.WriteLine("Bot starting, scanning site : " + gOpMode);
            Console.WriteLine("Connection string           : " + connectionString);
            Console.WriteLine("Database Type               : " + dbtype);
            Console.WriteLine("XML Path                    : " + xmlPath);
            Console.WriteLine("Sleep Time                  : " + sleepTime);
            Console.WriteLine(" ");

            int sleep = 2000;

            int.TryParse(sleepTime, out sleep);

            if (Directory.Exists(xmlPath) == false)
            {
                Console.WriteLine("Directory " + xmlPath + " does not exist :(");
                return;
            }

            if (dbtype == "pg")
            {
                try
                {
                    for (int i = 0; i != numBots; ++i)
                    {
                        var pgConnection = new NpgsqlConnection(connectionString);
                        pgConnection.Open();
                        pgConnectionList.Add(pgConnection);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.ToString());
                    pgConnectionList = null;
                }
            }

            if (dbtype == "mysql")
            {
                //try
                //{
                //    msConnection = new MySqlConnection(connectionString);
                //    msConnection.Open();
                //    Console.WriteLine("MySQL version : {0}", msConnection.ServerVersion);

                //}
                //catch (MySqlException ex)
                //{
                //    Console.WriteLine("Error: {0}", ex.ToString());
                //    msConnection = null;
                //}
            }

            if (gOpMode == OperationMode.Bet365Scan)
            {
                ScanBet365(sleep, 0);
            }
            else if (gOpMode == OperationMode.WilliamHillScan)
            {
                ScanWilliamHill(sleep);
            }
            else
            {
                UpdateFromXmlToWeb();
            }

        }

        private static void ScanWilliamHill(int sleepTime)
        {
            IWebDriver driver = null;

            ScoreBoardFinder sbf = new ScoreBoardFinder(sleepTime);
            sbf.Start();

            while (true)
            {
                try
                {
                    if (driver == null)
                    {
                        driver = GetChromeDriver("");
                    }

                    var hstats = new Dictionary<string, int>();
                    var astats = new Dictionary<string, int>();

                    var copyOfTheList = sbf.GetWebBlobs();

                    Console.WriteLine("There are " + copyOfTheList.Count() + " games in the blob list");
                    if (copyOfTheList.Count() == 0)
                    {
                        var downTime = 20000;
                        Console.WriteLine("Going to sleep for " + downTime + " miiliseconds");
                        System.Threading.Thread.Sleep(downTime);
                        continue;
                    }

                    foreach (var blob in copyOfTheList)
                    {
                        Console.WriteLine("On the blob loop...");

                        string homeTeamName = blob.HomeTeam;
                        string awayTeamName = blob.AwayTeam;
                        string league = blob.League;

                        string scoreUrl = blob.Url;

                        driver.Url = scoreUrl;
                        System.Threading.Thread.Sleep(sleepTime);

                        string text = GetElementText(driver, "//*[@id=\"commentaryContent\"]");

                        int totalDAs = -1;
                        int totalAs = -1;
                        int totalCs  = -1;
                        int totalBs  = -1;

                        int homeDAs = -1;
                        int homeAs  = -1;
                        int homeCs = -1;
                        int homeBs = -1;
                        int homeSonTs  = -1;
                        int homeSoffTs = -1;

                        int awayDAs = -1;
                        int awayAs = -1;
                        int awayCs  = -1;
                        int awayBs = -1;
                        int awaySonTs  = -1;
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

                        Console.WriteLine("Game:\t\t" + homeTeamName + " v " + awayTeamName);

                        if (String.IsNullOrEmpty(scoreUrl) == false)
                        {

                            string previewText = GetElementText(driver, "//*[@id=\"previewContents\"]");
                            string time = GetElementText(driver, "//*[@id=\"time\"]");
                            string period = GetElementText(driver, "//*[@id=\"period\"]");

                            if (string.IsNullOrEmpty(time))
                            {
                                time = period;
                            }

                            //"Ivory Coast\r\nTogo\r\n56%\r\n44%"
                            //"TotalNormal Time1st Half2nd Half\r\nIvory Coast 2 0 5 5 1 5 18 22 1 0\r\nTogo 1 0 4 9 0 6 16 21 3 0"
                            bool clickedOk = ClickElement(driver, "//*[@id=\"statisticsTab\"]");

                            if (!clickedOk)
                            {
                                Console.WriteLine("======> Click failed");
                            }

                            var previewSplits = Regex.Split(previewText, "\r\n").ToList();
                            string aPossession = previewSplits.Last().Replace("%", "");
                            previewSplits.RemoveAt(previewSplits.Count() - 1);
                            string hPossession = previewSplits.Last().Replace("%", "");

                            string statsText = GetElementText(driver, "//*[@id=\"statsTable\"]/tbody");

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
                                int parseResult =  ParseInt(statType[i + 1], justAwayStats[i]);
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

                            Console.WriteLine("League:\t\t" + league + " at " + time);
                            Console.WriteLine(homeTeamName.PadRight(homeTeamLongest ? homeTeamName.Length + 1 : awayTeamName.Length + 1) + String.Join(" ", hstats.Values));
                            Console.WriteLine(awayTeamName.PadRight(homeTeamLongest ? homeTeamName.Length + 1 : awayTeamName.Length + 1) + String.Join(" ", astats.Values));

                            if (hstats.Keys.Any(x => x == "-1") || astats.Keys.Any(x => x == "-1"))
                            {
                                Console.WriteLine("Bad Stat detected.... skipping");
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
                    Console.WriteLine("Exception caught: " + ce);
                    if (driver != null)
                    {
                        driver.Quit();
                        driver.Dispose();
                        driver = null;
                    }
                }
            }
        }

        private static void ScanBet365(int sleepTime, int botIndex)
        {
            IWebDriver driver = null;
            int idx = -1;
            bool firstTime = true;

            DateTime lastDayGamesUpdated = DateTime.MinValue;

            if (driver == null)
            {
                string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                driver = GetChromeDriver(agentString);
            }

            if (botIndex == 0 && gSkipAddGames == false)
            {
                Console.WriteLine("Scanning today's games for " + lastDayGamesUpdated.Date);
                AddTodaysBet365Matches(sleepTime, driver);
            }

            lastDayGamesUpdated = DateTime.Today;

            while (true)
            {
                idx++;
                try
                {
                    if (driver == null)
                    {
                        string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                        driver = GetChromeDriver(agentString);
                    }

                    if (DateTime.Today.Equals(lastDayGamesUpdated) == false)
                    {
                        if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(3))
                        {
                            lastDayGamesUpdated = DateTime.Today;
                        }

                        Console.WriteLine("Scanning today's games for " + lastDayGamesUpdated.Date);
                        AddTodaysBet365Matches(sleepTime, driver);
                    }
                    else
                    {
                        Console.WriteLine("Already scanned todays games for " + lastDayGamesUpdated.Date);
                    }

                    driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
                    System.Threading.Thread.Sleep(sleepTime);
                    
                    //string inPlayXPath = "//*[@id=\"sc_0_L1_1-1-5-0-0-0-0-1-1-0-0-0-0-0-1-0-0-0-0\"]";
                    IWebElement inPlayElement = GetWebElementFromClassAndDivText(driver, "Level1", "In-Play");

                    ClickElement(driver, inPlayElement);

                    var elements = driver.FindElements(By.ClassName("IPScoreTitle"));

                    if (firstTime || elements.Count() == 0)
                    {
                        inPlayElement = GetWebElementFromClassAndDivText(driver, "Level1", "In-Play");
                        ClickElement(driver, inPlayElement);
                        elements = driver.FindElements(By.ClassName("IPScoreTitle"));
                    }

                    if (elements.Count() == 0)
                    {
                        Console.WriteLine("No games in play, going to sleep for a bit....");
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

                    if (idx < elements.Count())
                    {
                        var hstats = new Dictionary<string, int>();
                        var astats = new Dictionary<string, int>();

                        int attempts = 3;

                        //*[@id="rw_spl_sc_1-1-5-24705317-2-0-0-1-1-0-0-0-0-0-1-0-0_101"]/div[1]
                        elements.ElementAt(idx).Click();
                        System.Threading.Thread.Sleep(sleepTime);

                        var hCardsAndCorners = GetValuesById(driver, "team1IconStats", attempts, 3, " ");
                        if (hCardsAndCorners == null) { 
                            Console.WriteLine("hCardsAndCorners == null");
                            Console.WriteLine("Resetting driver...");
                            driver.Close();
                            driver.Dispose();
                            driver = null;
                            continue;
                        }

                        var aCardsAndCorners = GetValuesById(driver, "team2IconStats", attempts, 3, " ");
                        if (aCardsAndCorners == null) { Console.WriteLine("aCardsAndCorners == null"); continue; }

                        var inPlayTitles = GetValuesByClassName(driver, "InPlayTitle", attempts, 1, new char[] { '@' });
                        if (inPlayTitles == null) { Console.WriteLine("inPlayTitles == null"); continue; }

                        var cleanScores = GetValuesByClassName(driver, "clock-score", attempts, 3, new char[] { ' ', '-', '\r', '\n' });
                        if (cleanScores == null) { Console.WriteLine("cleanScores == null"); continue; }

                        //var matchdetails = driver.FindElement(By.Id("match-details"));
                        //matchdetails.Click();
                        //System.Threading.Thread.Sleep(1000);
                        //var statButton = driver.FindElement(By.ClassName("StatsSelectIcon"));
                        //statButton.Click();
                        //System.Threading.Thread.Sleep(3000);
                        //var popUpMatchStats = GetValuesById(driver, "PopUpMatchStats", attempts, 0, "\r\n");                    
                        //Console.WriteLine(popUpMatchStats);

                        bool rballOkay = true;

                        var shotsOnTarget = GetValuesById(driver, "stat1", attempts, 3, "\r\n");
                        if (shotsOnTarget == null) { Console.WriteLine("shotsOnTarget == null"); rballOkay = false; }

                        List<string> shotsOffTarget = null;
                        List<string> attacks = null;
                        List<string> dangerousAttacks = null;

                        if (rballOkay == true)
                        {
                            shotsOffTarget = GetValuesById(driver, "stat2", attempts, 3, "\r\n");
                            if (shotsOffTarget == null) { Console.WriteLine("shotsOffTarget == null"); rballOkay = false; }

                            attacks = GetValuesById(driver, "stat3", attempts, 3, "\r\n");
                            if (attacks == null) { Console.WriteLine("attacks == null"); rballOkay = false; }

                            dangerousAttacks = GetValuesById(driver, "stat4", attempts, 3, "\r\n");
                            if (dangerousAttacks == null) { Console.WriteLine("dangerousAttacks == null"); rballOkay = false; }
                        }

                        cleanScores.RemoveAll(x => String.IsNullOrEmpty(x));
                        string time = cleanScores.ElementAt(2);
                        string inPlayTitle = inPlayTitles.ElementAt(0);

                        if (String.IsNullOrEmpty(time))
                        {
                            Console.WriteLine("Couldn't get time :(");
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
                    Console.WriteLine("Exception thrown: " + we);
                    driver.Quit();
                    driver.Dispose();
                    driver = null;

                }
                catch (Exception we)
                {
                    Console.WriteLine("Exception thrown: " + we);
                    driver.Quit();
                    driver.Dispose();
                    driver = null;
                }
            }
        }

        private static IWebElement GetWebElementFromClassAndDivText(IWebDriver driver, string classType, string findString)
        {
            IWebElement retVal = null;
            var thisTypes = driver.FindElements(By.ClassName(classType));

            foreach (var level1 in thisTypes)
            {
                if (level1.Text.Trim().Equals(findString))
                {
                    retVal = level1;
                    break;
                }
            }

            return retVal;
        }

        private static void AddTodaysBet365Matches(int sleepTime, IWebDriver driver)
        {
            var foundMatches = new List<aMatch>();

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            System.Threading.Thread.Sleep(sleepTime);

            int dirtySleep = 2000;

            var matchMarket = GetWebElementFromClassAndDivText(driver, "Level1", "Match Markets");

            if (matchMarket != null)
            {
                matchMarket.Click();
                System.Threading.Thread.Sleep(dirtySleep);
            }
            else
            {
                Console.WriteLine("Couldn't find Match Markets");
                return;
            }

            var mainGroup = GetWebElementFromClassAndDivText(driver, "Level2", "Main");

            if (mainGroup != null)
            {
                mainGroup.Click();
                System.Threading.Thread.Sleep(dirtySleep);
            }
            else
            {
                Console.WriteLine("Couldn't find Main");
                return;
            }

            var fullTimeResult = GetWebElementFromClassAndDivText(driver, "genericRow", "Full Time Result");

            if (fullTimeResult != null)
            {
                fullTimeResult.Click();
                System.Threading.Thread.Sleep(dirtySleep);
            }
            else
            {
                Console.WriteLine("Couldn't find Full Time Result");
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

                //if (matches.Count() > 1)
                //{
                //    for (int k = 1; k != matches.Count(); ++k)
                //    {
                //        matches.ElementAt(k).Click();
                //        System.Threading.Thread.Sleep(dirtySleep);
                //    }
                //}

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
                        Console.WriteLine("Can't get match from " + matchText);
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
                Console.WriteLine(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " at " + m.koDateTime.TimeOfDay + " in " + m.league);
                int leagueId = AddLeague(m.league);
                int hTeamId = AddTeam(m.team1);
                int aTeamId = AddTeam(m.team2);
                int gameId = AddGame(hTeamId, aTeamId, leagueId, m.koDateTime);
            }

            Console.WriteLine("");
        }

        private static IWebDriver GetChromeDriver(string agentString)
        {
            IWebDriver driver = null;

            try
            {

                if (string.IsNullOrEmpty(agentString) == false)
                {
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument(agentString);
                    driver = new ChromeDriver(options);
                }
                else
                {
                    driver = new ChromeDriver();
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine("Exception: " + ce);
            }


            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
            return driver;
        }

        private static List<string> GetValuesById(IWebDriver driver, string searchId, int attempts, int expected, string seperator)
        {
            while (attempts-- != 0)
            {
                var data = Regex.Split(driver.FindElement(By.Id(searchId)).Text, seperator);
                var dataList = data.ToList();
                dataList.RemoveAll(x => String.IsNullOrWhiteSpace(x));

                if (dataList.Count() == expected || expected == 0)
                {
                    return dataList;
                }
            }

            return null;
        }

        private static List<string> GetValuesByClassName(IWebDriver driver, string searchId, int attempts, int expected, char[] seperators)
        {
            while (attempts-- != 0)
            {
                var data = driver.FindElement(By.ClassName(searchId)).Text.Split(seperators);
                var dataList = data.ToList();
                dataList.RemoveAll(x => String.IsNullOrEmpty(x));

                if (dataList.Count() == expected)
                {
                    return dataList;
                }
            }

            return null;
        }

        public delegate void SendToWebDelegate(string league, DateTime koDate, string homeTeam, string awayTeam, Dictionary<string, int> hstats, Dictionary<string, int> astats, string time);
        public delegate void WriteXmlDelegate(string path, Dictionary<string, int> hstats, Dictionary<string, int> astats, string homeTeamName, string awayTeamName, string league, string time, bool exists, string finalName);

        public static Dictionary<string, string> subs = new Dictionary<string, string>()
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

        private static string DoSubstitutions(string aString)
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

        private static void WriteXml(string path, Dictionary<string, int> hstats, Dictionary<string, int> astats, string homeTeamName, string awayTeamName, string league, string time, bool exists, string finalName)
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
                    Console.WriteLine("Already seen this minute of the " + homeTeamName + " vs " + awayTeamName + " game!");
                }
                else
                {
                    xGame.Add(snap);
                    xdoc.Save(finalName);
                }
            }
        }

        private static void SendToWeb(string league, DateTime koDate, string homeTeam, string awayTeam, Dictionary<string, int> hstats, Dictionary<string, int> astats, string time)
        {
            int leagueId = -1;

            if (gOpMode != OperationMode.Bet365Scan)
            {
                leagueId = AddLeague(league);
            }

            int hTeamId = AddTeam(homeTeam);
            int aTeamId = AddTeam(awayTeam);
            int gameId = AddGame(hTeamId, aTeamId, leagueId, koDate);

            List<int> allStats = new List<int>();
            allStats.AddRange(hstats.Values);
            allStats.AddRange(astats.Values);

            int retries = 0;

            Console.WriteLine("Adding " + homeTeam + " [" + hTeamId + "] v " + awayTeam + " [" + aTeamId + "] in league [" + leagueId + "] with game id: " + gameId + " at time: " + time);
 
            Console.WriteLine("Goals Corners");
            Console.WriteLine(hstats[statType[1]].ToString().PadRight(6) + hstats[statType[6]].ToString());
            Console.WriteLine(astats[statType[1]].ToString().PadRight(6) + astats[statType[6]].ToString());

            while (retries < gKeyClashRetries)
            {
                try
                {
                    AddStatistics(allStats, gameId, time, "", koDate);
                }
                catch (NpgsqlException ne)
                {
                    Console.WriteLine("Retrying....");
                    retries += 1;
                }
                break;
            }
        }

        private static void Upload(string league, DateTime koDate, string homeTeam, string awayTeam, IEnumerable<XElement> snaps)
        {
            int leagueId = -1;

            if (gOpMode != OperationMode.UploadBet365)
            {
                leagueId = AddLeague(league);
            }

            int hTeamId = AddTeam(homeTeam);
            int aTeamId = AddTeam(awayTeam);
            int gameId = AddGame(hTeamId, aTeamId, leagueId, koDate);

            Console.WriteLine("Adding " + homeTeam + " [" + hTeamId + "] v " + awayTeam + " [" + aTeamId + "] in league [" + leagueId + "] with game id: " + gameId);
            SendStats(snaps, gameId);
        }

        public static int AddTeam(string team)
        {
            int idx = -1;
            bool hasRows = false;

            using (NpgsqlCommand findInTeamsTable = new NpgsqlCommand("SELECT id, name FROM teams WHERE name = '" + team + "';", pgConnectionList.ElementAt(botIndex)))
            {
                using (NpgsqlDataReader dr = findInTeamsTable.ExecuteReader())
                {
                    hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        string id = dr[0].ToString();
                        string name = dr[1].ToString();

                        if (name == team)
                        {
                            idx = int.Parse(id);
                        }
                    }
                    dr.Close();
                }
            }

            if (hasRows == false)
            {
                //see if it exists in the team_associations
                using (NpgsqlCommand findInTeamsTable = new NpgsqlCommand("SELECT team_id, name FROM team_associations WHERE name = '" + team + "';", pgConnectionList.ElementAt(botIndex)))
                {
                    using (NpgsqlDataReader dr = findInTeamsTable.ExecuteReader())
                    {
                        hasRows = dr.HasRows;

                        if (hasRows == true)
                        {
                            dr.Read();
                            string id = dr[0].ToString();
                            string name = dr[1].ToString();

                            if (name == team)
                            {
                                idx = int.Parse(id);
                            }
                        }

                        dr.Close();
                    }
                }
            }

            if (hasRows == false)
            {
                using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from teams", pgConnectionList.ElementAt(botIndex)))
                {
                    using (NpgsqlDataReader dr2 = count.ExecuteReader())
                    {
                        dr2.Read();

                        try
                        {
                            int rows = int.Parse(dr2[0].ToString());
                            idx = rows + 1;
                        }
                        catch (Exception)
                        {
                            idx = 1;
                        }

                        dr2.Close();

                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into teams ( id, name, created_at, updated_at ) VALUES (" + idx + ", '" + team + "', '" + now + "', '" + now + "');", pgConnectionList.ElementAt(botIndex)))
                        {
                            insert.ExecuteNonQuery();
                        }
                    }
                }
            }
            return idx;
        }

        public static bool AddStatistics(List<int> values, int gameId, string minutes, string lastMinute, DateTime seenTime)
        {
            int idx = -1;

            int minutesParsed = ParseMinutes(minutes);

            //check last minute to see if we've seen this game and return quickly
            if (lastMinute != "")
            {
                int lastMinuteParsed = ParseMinutes(lastMinute);

                using (NpgsqlCommand find = new NpgsqlCommand("SELECT id, game_id FROM statistics WHERE game_id = " + gameId + " AND gametime = " + lastMinuteParsed + ";", pgConnectionList.ElementAt(botIndex)))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        bool hasRows = dr.HasRows;

                        if (hasRows)
                        {
                            Console.WriteLine("Already seen the minute " + lastMinuteParsed + " of this game");
                            dr.Close();
                            return false;
                        }

                        dr.Close();
                    }
                }
            }

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id, game_id FROM statistics WHERE game_id = " + gameId + " AND gametime = " + minutesParsed + ";", pgConnectionList.ElementAt(botIndex)))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        string id = dr[0].ToString();

                        int gCheck = int.Parse(dr[1].ToString());

                        if (gCheck == gameId)
                        {
                            idx = int.Parse(id);
                        }
                    }

                    dr.Close();

                    if (hasRows == false)
                    {
                        Console.WriteLine("Uploading game time: " + minutesParsed);

                        using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from statistics;", pgConnectionList.ElementAt(botIndex)))
                        {
                            using (NpgsqlDataReader dr2 = count.ExecuteReader())
                            {
                                dr2.Read();

                                try
                                {
                                    int rows = int.Parse(dr2[0].ToString());
                                    idx = rows + 1;
                                }
                                catch (Exception)
                                {
                                    idx = 1;
                                }

                                dr2.Close();

                                string now = DateTime.Now.ToString("yyyy-MM-dd");
                                string valuesAsString = string.Join(", ", values);

                                pgConnectionList.ElementAt(botIndex).CreateCommand();

                                string sql = "";

                                if (gOpMode == OperationMode.Bet365Scan ||
                                    gOpMode == OperationMode.UploadBet365)
                                {
                                    if (values.Count() == 8)
                                    {
                                        sql = "INSERT into statistics ( " +
                                       "id, gametime, game_id, seentime, hrc, hyc, hco, hg, arc, ayc, aco, ag, created_at, updated_at ) " +
                                       " VALUES " +
                                       "( " + idx + ", '" +
                                       minutesParsed + "', " +
                                       gameId + ", '" +
                                       seenTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                       valuesAsString + ", '" + now + "', '" + now + "');";
                                    }
                                    else
                                    {
                                        sql = "INSERT into statistics ( " +
                                        "id, gametime, game_id, seentime, hrc, hyc, hco, hsont, hsofft, ha, hda, hg, arc, ayc, aco, asont, asofft, aa, ada, ag, created_at, updated_at ) " +
                                        " VALUES " +
                                        "( " + idx + ", '" +
                                        minutesParsed + "', " +
                                        gameId + ", '" +
                                        seenTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                        valuesAsString + ", '" + now + "', '" + now + "');";
                                    }
                                }
                                else
                                {
                                    sql = "INSERT into statistics ( " +
                                    "id, gametime, game_id, seentime, hpn, hg, hpen, hsont, hsofft, hw, hco, hfk, ht, hyc, hrc, ha, hda, hbs, hcl, apn, ag, apen, asont, asofft, aw, aco, afk, at, ayc, arc, aa, ada, abs, acl, created_at, updated_at ) " +
                                    " VALUES " +
                                    "( " + idx + ", '" +
                                    minutesParsed + "', " +
                                    gameId + ", '" +
                                    seenTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                    valuesAsString + ", '" + now + "', '" + now + "');";
                                }


                                using (NpgsqlCommand insert = new NpgsqlCommand(sql))
                                {
                                    insert.Connection = pgConnectionList.ElementAt(botIndex);
                                    insert.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Already seen minute " + minutesParsed + " of this game");
                    }
                }

            }

            return true;
        }

        private static int ParseMinutes(string time)
        {
            int minutes = 0;

            if (time.ToLower().StartsWith("half"))
            {
                minutes = -1;
            }
            else if (time.ToLower().StartsWith("full") || time.Trim().StartsWith("End Of Normal Time"))
            {
                minutes = -2;
            }
            else if (time.Contains(":"))
            {
                string mins = Regex.Split(time, ":").ElementAt(0);
                minutes = int.Parse(mins);
            }
            return minutes;
        }

        public static int AddGame(int homeTeamId, int awayTeamId, int leagueId, DateTime koDate)
        {
            int idx = -1;
            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id, team1, kodate, league_id FROM games WHERE team1 = '" + homeTeamId + "' AND team2 = '" + awayTeamId + "';", pgConnectionList.ElementAt(botIndex)))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    while (dr.Read())  //bug fix for repeated same game added after rematch
                    {
                        
                        string id = dr[0].ToString();
                        int thisHomeTeam = int.Parse(dr[1].ToString());

                        string thisKoDate = dr[2].ToString();
                        int thisLeagueId = int.Parse(dr[3].ToString());

                        DateTime dt = DateTime.Parse(thisKoDate);

                        if (dt.Date == koDate.Date &&
                            thisHomeTeam == homeTeamId)
                        {
                            idx = int.Parse(id);
                            hasRows = true;
                            break;
                        }
                        else
                        {
                            hasRows = false;
                        }
                    }

                    dr.Close();

                    if (hasRows == false)
                    {
                        using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from games;", pgConnectionList.ElementAt(botIndex)))
                        {
                            using (NpgsqlDataReader dr2 = count.ExecuteReader())
                            {
                                dr2.Read();
                                try
                                {
                                    int rows = int.Parse(dr2[0].ToString());
                                    idx = rows + 1;
                                }
                                catch (Exception)
                                {
                                    idx = 1;
                                }

                                dr2.Close();

                                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into games (id, league_id, team1, team2, koDate,  created_at, updated_at  ) VALUES (" + idx + ", " + leagueId + ", " + homeTeamId + ", " + awayTeamId + ", '" + koDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + now + "', '" + now + "');", pgConnectionList.ElementAt(botIndex)))
                                {
                                    insert.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

            }
            return idx;
        }

        public static int AddLeague(string leagueName)
        {
            int idx = -1;

            bool hasRows = false;
            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id, name FROM leagues WHERE name = '" + leagueName + "';", pgConnectionList.ElementAt(botIndex)))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        string id = dr[0].ToString();
                        string name = dr[1].ToString();

                        if (name == leagueName)
                        {
                            idx = int.Parse(id);
                        }
                    }

                    dr.Close();
                }
            }

            if (hasRows == false)
            {
                //see if it exists in the team_associations
                using (NpgsqlCommand findInTeamsTable = new NpgsqlCommand("SELECT league_id, name FROM league_associations WHERE name = '" + leagueName + "';", pgConnectionList.ElementAt(botIndex)))
                {
                    using (NpgsqlDataReader dr = findInTeamsTable.ExecuteReader())
                    {
                        hasRows = dr.HasRows;

                        if (hasRows == true)
                        {
                            dr.Read();
                            string id = dr[0].ToString();
                            string name = dr[1].ToString();

                            if (name == leagueName)
                            {
                                idx = int.Parse(id);
                            }
                        }

                        dr.Close();
                    }
                }
            }

            if (hasRows == false)
            {
                using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from leagues;", pgConnectionList.ElementAt(botIndex)))
                {
                    using (NpgsqlDataReader dr2 = count.ExecuteReader())
                    {
                        dr2.Read();
                        try
                        {
                            int rows = int.Parse(dr2[0].ToString());
                            idx = rows + 1;
                        }
                        catch (Exception)
                        {
                            idx = 1;
                        }

                        dr2.Close();
                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into leagues ( id, name, league_id, created_at, updated_at  ) VALUES (" + idx + ", '" + leagueName + "', " + idx + ", '" + now + "', '" + now + "');", pgConnectionList.ElementAt(botIndex)))
                        {
                            insert.ExecuteNonQuery();
                        }
                    }
                }
            }

            return idx;
        }

        private static void UpdateFromXmlToWeb()
        {

            try
            {
                string path = xmlPath;

                Console.WriteLine("Program starting from " + path);

                var dirs = Directory.GetDirectories(path);

                foreach (var dir in dirs)
                {
                    string league = Path.GetFileName(dir);

                    var xmls = Directory.GetFiles(Path.Combine(path, dir)).Where(x => x.EndsWith(".xml"));

                    foreach (var xml in xmls)
                    {
                        Console.WriteLine("Loading: " + xml);
                        XDocument xdoc = null;

                        try
                        {
                            xdoc = XDocument.Load(xml);
                        }
                        catch (Exception ce)
                        {
                            Console.WriteLine("Failed to load: " + ce);
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

                            Console.WriteLine(league);
                            Console.WriteLine(homeTeam + " v " + awayTeam + " " + koDateString);

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
                                    Console.WriteLine("Warning: cannot overwrite: " + uploadedFileName);
                                    qwert += 1;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Finally");
            }
        }

        private static void SendStats(IEnumerable<XElement> snaps, int gameId)
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

                    while (retries < gKeyClashRetries)
                    {
                        try
                        {
                            result = AddStatistics(values, gameId, time, lastTime, seenTime);
                        }
                        catch (NpgsqlException ne)
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

                    while (retries < gKeyClashRetries)
                    {
                        try
                        {
                            AddStatistics(values, gameId, time, "", seenTime);
                        }
                        catch (NpgsqlException ne)
                        {
                            retries += 1;
                        }
                        break;
                    }
                }
            }
        }

        private static DateTime GetSeenTime(XElement snap)
        {
            string seen = snap.Attribute("Seen").Value;
            string[] bits = Regex.Split(seen, " ");
            string[] dates = bits[0].Split('/');
            string[] times = bits[1].Split(':');

            return new DateTime(int.Parse(dates[2]) + 2000, int.Parse(dates[0]), int.Parse(dates[1]), int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2])); ;
        }



        //public static int CorrectTeamName(string oldTeam, string newTeam)
        //{
        //    int idx = -1;
        //    using (NpgsqlCommand find = new NpgsqlCommand("SELECT id, name FROM teams WHERE name = '" + oldTeam + "';", pgConnection))
        //    {

        //        using (NpgsqlDataReader dr = find.ExecuteReader())
        //        {
        //            bool hasRows = dr.HasRows;

        //            if (hasRows == true)
        //            {
        //                dr.Read();
        //                string id = dr[0].ToString();
        //                string name = dr[1].ToString();

        //                if (name == oldTeam)
        //                {
        //                    idx = int.Parse(id);
        //                }
        //            }
        //        }
        //    }


        //    if (idx != -1)
        //    {
        //        //old team exists
        //        using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM games WHERE team1 = '" + idx + "' OR team2 = '" + idx + "';", pgConnection))
        //        {

        //            using (NpgsqlDataReader dr = find.ExecuteReader())
        //            {
        //            }
        //        }
        //    }
        //}

        class Game
        {
            public string teamA;
            public string teamB;
            public int hgoals;
            public int agoals;
            public int hCorners;
            public int aCorners;
        }


        private static void ScratchPad()
        {
            var games = new List<Game>();
            string teamA = "Manisaspor Reserves";
            string teamB = "Kasimpasa Reserves";
            try
            {
                //string pat2h = "C:\\Users\\user\\Google Drive\\WillHillXML - Copy\\Turkish A2 Ligi";

                //Console.WriteLine("Program starting from " + pat2h);


                //var xml2s = Directory.GetFiles(pat2h);

                //foreach (string xml in xml2s)
                //{
                //    if (xml.Contains("Res ") || xml.Contains("Res_"))
                //    {
                //        string newXml = xml.Replace("Res", "Reserves");
                //        File.Move(xml, newXml);

                //    }

                //}

                //return;

                string path = "C:\\Users\\user\\Google Drive\\WillHillData_Processed\\Turkish A2 Ligi";
                var xmls = Directory.GetFiles(path);

                System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\user\\TurkeyA2.txt");
                file.WriteLine("HomeTeam,AwayTeam,HomeGoals,AwayGoals,HomeCorners,AwayCorners");

                foreach (var xml in xmls)
                {
                    XDocument xdoc = null;
                    try
                    {

                        xdoc = XDocument.Load(xml);
                    }
                    catch (Exception ce)
                    {
                        Console.WriteLine("error: " + xml);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(xml);

                    var splits = Regex.Split(fileName, " v ");
                    string homeTeam = splits.ElementAt(0);
                    string awayTeam = splits.ElementAt(1).Substring(0, splits.ElementAt(1).IndexOf("_"));

                    var game = xdoc.Descendants("Game");
                    string koDateString = xml.Substring(xml.Length - 10, 6);
                    DateTime koDate = DateTime.ParseExact(koDateString, "ddMMyy", null);

                    var snaps = game.Descendants("Snap");

                    //foreach (var snap in snaps)
                    var snap = snaps.Last();


                    string time = snap.Attribute("Time").Value;

                    //<Snap Time="Full Time" Seen="01/27/13 5:30:04">
                    //<Snap Time="Half Time" Seen="01/27/13 4:23:48">
                    //<Snap Time="90:19" Seen="01/27/13 5:23:57">
                    string seen = snap.Attribute("Seen").Value;
                    string[] bits = Regex.Split(seen, " ");
                    string[] dates = bits[0].Split('/');
                    string[] times = bits[1].Split(':');

                    DateTime seenTime = new DateTime(int.Parse(dates[2]) + 2000, int.Parse(dates[0]), int.Parse(dates[1]), int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]));

                    var hG = int.Parse(snap.Descendants("Home").First().Descendants("Goals").First().Value);
                    var aG = int.Parse(snap.Descendants("Away").First().Descendants("Goals").First().Value);
                    var hC = int.Parse(snap.Descendants("Home").First().Descendants("Corners").First().Value);
                    var aC = int.Parse(snap.Descendants("Away").First().Descendants("Corners").First().Value);



                    string line = String.Format("{0},{1},{2},{3},{4},{5}", homeTeam, awayTeam, hG, aG, hC, aC);

                    file.WriteLine(line);

                    games.Add(new Game() { teamA = homeTeam, teamB = awayTeam, hCorners = hC, aCorners = aC, agoals = aG, hgoals = hG });

                }
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Finally");
            }



            int confidenceIdx = 0;

            var rematchesHome = games.Where(x => x.teamA == teamA && x.teamB == teamB);
            var rematchesAway = games.Where(x => x.teamB == teamA && x.teamA == teamB);

            var gamesWhereTeamAIsHome = games.Where(x => x.teamA == teamA);
            var gamesWhereTeamAIsAway = games.Where(x => x.teamA == teamA);
            var gamesWhereTeamBIsHome = games.Where(x => x.teamA == teamB);
            var gamesWhereTeamBIsAway = games.Where(x => x.teamB == teamB);

            foreach (var g in gamesWhereTeamAIsHome)
            {
                string thisAwayTeam = g.teamB;

            }
        }
    }
}
