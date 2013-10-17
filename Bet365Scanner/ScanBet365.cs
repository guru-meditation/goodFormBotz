﻿using OpenQA.Selenium;
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
    using System.Linq.Expressions;
    using WebDriver;

    public abstract class Data
    {
    }

    public class FullTimeResult : Data
    {
        public string One;
        public string X;
        public string Two;
    }

    public class HalfTimeResult : Data
    {
        public string One;
        public string X;
        public string Two;
    }

    public class DoubleChance : Data
    {
        public string OneX;
        public string XTwo;
        public string OneTwo;
    }

    public class NthGoal : Data
    {
        public string team1;
        public string NoGoal;
        public string team2;
    }

    public class Retry
    {
        protected static readonly log4net.ILog log
         = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        int maxRetries = 1;

        public Retry(int max_retries)
        {
            maxRetries = max_retries;
        }

        public void execute(Action f)
        {

            int retries = 0;

            while (true)
            {
                //anything in betting tab might disapear due to refresh of the page
                try
                {
                    f();
                    break; // exit the loop if code completes
                }
                catch (StaleElementReferenceException e)
                {
                    if (++retries >= maxRetries)
                    {
                        log.Warn("Couldn't finish due to StaleElementReferenceException maxRetries reached " + maxRetries);
                        break;
                    }
                    else
                    {
                        log.Info("StaleElementReferenceException retrying " + retries);
                        continue;
                    }
                }
            }
        }
    }

    

    public class Context
    {
        public DriverWrapper driver;
        public IWebElement element = null;
        public List<Data> data = new List<Data>();

        public string team1;
        public string team2;

        public Context(DriverWrapper dr) { driver = dr; }

        public void PushElement(IWebElement el) { element = el; }
        public void PopElement() { element = null; }

        public void PushData(Data data) { this.data.Add(data); }
    }

   
    public class ValidatePage : IDisposable
    {
        protected Context ctx;

        protected virtual void Precondition(Context ctx) { }
        protected virtual void Postcondition(Context ctx) { }

        public ValidatePage(Context ctx) { this.ctx = ctx; Precondition(ctx); }
        public void Dispose() { Postcondition(ctx); }
    }

    public abstract class ExprBase
    {
        protected static readonly log4net.ILog log
           = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public abstract void Interpret(Context ctx);
    }

    public class FullTimeResultExpr : ExprBase
    {
        public override void Interpret(Context ctx)
        {
            log.Info("FullTimeResult parser");
            IWebElement element = ctx.element;
            var cols = element.FindElements(By.ClassName("cpCol3Box"));
            FullTimeResult data = new FullTimeResult();
            foreach ( IWebElement col in cols )
            {
                string text = col.Text;
                
                // "1\r\n 11/10"
                string[] lines = Regex.Split(text, "\r\n");
                string odds_name = lines[0];
                string odds = lines[1];

                log.Debug("Odds: " + odds_name + " at " + odds);
                
                switch (odds_name)
                {
                    case "1" :
                        data.One = odds;
                        break;
                    case "X" :
                        data.X = odds;
                        break;
                    case "2" :
                        data.Two = odds;
                        break;
                    default:
                        log.Error("invalid odds name");
                        break;
                }               
            }

            ctx.PushData(data);
        }
    }

    public class HalfTimeResultExpr : ExprBase
    {
        public override void Interpret(Context ctx)
        {
            log.Info("HalfTimeResult parser");
            IWebElement element = ctx.element;
            var cols = element.FindElements(By.ClassName("cpCol3Box"));
            HalfTimeResult data = new HalfTimeResult();

            foreach (IWebElement col in cols)
            {
                string text = col.Text;

                string[] lines = Regex.Split(text, "\r\n");
                string odds_name = lines[0];
                string odds = lines[1];

                log.Debug("Odds: " + odds_name + " at " + odds);

                switch (odds_name)
                {
                    case "1":
                        data.One = odds;
                        break;
                    case "X":
                        data.X = odds;
                        break;
                    case "2":
                        data.Two = odds;
                        break;
                    default:
                        log.Error("invalid odds name");
                        break;
                }
            }

            ctx.PushData(data);
        }
    }

    public class DoubleChanceExpr : ExprBase
    {
        public override void Interpret(Context ctx)
        {
            log.Info("DoubleChance parser");
            IWebElement element = ctx.element;
            var cols = element.FindElements(By.ClassName("cpCol3Box"));
            DoubleChance data = new DoubleChance();

            foreach (IWebElement col in cols)
            {
                string text = col.Text;

                string[] lines = Regex.Split(text, "\r\n");
                string odds_name = lines[0];
                string odds = lines[1];

                log.Debug("Odds: " + odds_name + " at " + odds);

                switch (odds_name)
                {
                    case "1X":
                        data.OneX = odds;
                        break;
                    case "X2":
                        data.XTwo = odds;
                        break;
                    case "12":
                        data.OneTwo = odds;
                        break;
                    default:
                        log.Error("invalid odds name");
                        break;
                }
            }

            ctx.PushData(data);
        }
    }

    public class NthGoalExpr : ExprBase
    {
        int N = 0;

        public NthGoalExpr(int n) { N = n; }

        public override void Interpret(Context ctx)
        {
            log.Info("NthGoal parser N = "+N);
           
            IWebElement element = ctx.element;

            var cols = element.FindElements(By.ClassName("CouponRow"));
            NthGoal data = new NthGoal();

            foreach (IWebElement col in cols)
            {
                string odds_name = col.Text;
                //odds_name contains odds as well we need to get rid of the odds part
                string odds = col.FindElement(By.ClassName("Odds")).Text;

                odds_name = odds_name.Substring(0, odds_name.IndexOf(odds));

                log.Debug("Odds: " + odds_name + " at " + odds);

                if (odds_name == ctx.team1)
                {
                    data.team1 = odds;
                }
                else if (odds_name == ctx.team2)
                {
                    data.team2 = odds;
                }
                else
                {
                    data.NoGoal = odds;
                }
                
            }
            log.Debug("Odds: " + data.team1 + " " + data.team2 + " " + data.NoGoal);
            
            ctx.PushData(data);
        }
    }

    public class BettingTab : ExprBase
    {
        Dictionary<string, ExprBase> dispatch = new Dictionary<string, ExprBase>();
        public BettingTab()
        {
            dispatch.Add("Fulltime Result", new FullTimeResultExpr());
            dispatch.Add("Half Time Result", new HalfTimeResultExpr());
            dispatch.Add("Double Chance", new DoubleChanceExpr());
            dispatch.Add("1st Goal", new NthGoalExpr(1));
            dispatch.Add("2nd Goal", new NthGoalExpr(2));
            dispatch.Add("3rd Goal", new NthGoalExpr(3));
            
        }

        public override void Interpret(Context ctx)
        {
            Retry r = new Retry(3);

            r.execute(() =>
            {
                var betting_tab = ctx.driver.FindElement(By.Id("BettingTab"));
                var sections = betting_tab.FindElements(By.ClassName("Section"));

                foreach (IWebElement section in sections)
                {
                    IWebElement fixture_descr = section.FindElement(By.ClassName("FixtureDescription"));
                    string name = fixture_descr.Text;

                    //name can be 1st Goal or 2nd Goal or 3rd Goal or 4th Goal .. Nth Goal
                    int pos = name.LastIndexOf("th Goal");

                    ExprBase expr = null;
                    bool found = false;

                    if (pos != -1)
                    {
                        int N = System.Convert.ToInt32(name.Substring(pos));
                        expr = new NthGoalExpr(N);
                        found = true;
                    }
                    else
                    {
                        found = dispatch.TryGetValue(name, out expr);
                    }
                    ctx.PushElement(section);
                    if (found) expr.Interpret(ctx);
                    ctx.PopElement();











                }

            }
               );
        }
    }

    public class InPlayMatchPage : ExprBase
    {
        List<ExprBase> match = new List<ExprBase> { new BettingTab() };

        public override void Interpret(Context ctx)
        {
            using (new ValidatePage(ctx))
            {
                foreach (ExprBase page in match)
                {
                    page.Interpret(ctx);
                }
            }
        }
    }


    public class ScanBet365 : Scanner
    {
        private static readonly log4net.ILog log
              = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScanBet365(DriverCreator creator, Database db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        protected override int addLeague(string league)
        {
            return -1;
        }

        private void GetElementAndClick(DriverWrapper driver, string classType, string findString)
        {
            var element = driver.GetWebElementFromClassAndDivText(classType, findString);

            if (element != null)
            {
                element.Click();
                driver.DirtySleep(dirtySleep);
            }
            else
            {
                log.Error("Couldn't find " + findString + "in classType: " + classType);
                return;
            }
        }

        int dirtySleep = 2000;

        private void AddTodaysMatches(int sleepTime, DriverWrapper driver)
        {
            var foundMatches = new List<aMatch>();

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            dirtySleep = 2000;

            GetElementAndClick(driver, "Level1", "Match Markets");
            GetElementAndClick(driver, "Level2", "Main");
            GetElementAndClick(driver, "genericRow", "Full Time Result");
            // it takes time for genericRow to expand 
            driver.ForceSleep(dirtySleep);

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
            skipGames = true;

            DateTime lastDayGamesUpdated = DateTime.MinValue;

            if (driver == null)
            {
                string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                driver = driverCreator.CreateDriver(agentString);
            }

            if (botIndex == 0 && skipGames == false)
            {
                log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                AddTodaysMatches(sleepTime, driver);
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
                        AddTodaysMatches(sleepTime, driver);
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
                    driver.ForceSleep(2000);


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
                        IWebElement el = elements.ElementAt(idx);
                        el.Click();

                        //log.Info("Element: " + el.Text + " is displayed: " + el.Displayed);

                        var inPlayTitles = driver.GetValuesByClassName("InPlayTitle", attempts, 1, new char[] { '@' });
                        if (inPlayTitles == null) { log.Warn("inPlayTitles == null"); continue; }
                        string inPlayTitle = inPlayTitles.ElementAt(0);
                        var teams = Regex.Split(inPlayTitle, " v ");

                        string homeTeamName = teams.ElementAt(0);
                        string awayTeamName = teams.ElementAt(1);

                        // Collect the odds
                        try
                        {
                            Context ctx = new Context(driver);
                            // Add team names
                            ctx.team1 = homeTeamName;
                            ctx.team2 = awayTeamName;

                            InPlayMatchPage match_page = new InPlayMatchPage();

                            match_page.Interpret(ctx);

                        }
                        catch (Exception e)
                        {
                            log.Error("Exception : " + e);
                        }

                     
                        driver.Wait( () => 
                        {
                            return driver.FindElement(By.ClassName("clock-score")).Displayed &&
                                driver.FindElement(By.ClassName("clock-score")).Enabled &&
                                driver.FindElement(By.Id("arena")).Displayed &&
                                driver.FindElement(By.Id("team1IconStats")).Displayed;
                        } );

                   
                        //XXX: click causes a nice animation that takes some time,
                        //XXX: if we forcesleep for shorter time than animation takes then cleanScores will be null!!
                        //TODO: I changed this to make it faster, but we need to find out a way how to get rid of ForceSleep
                        driver.ForceSleep(2000);

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

                        bool rballOkay = true;
                        // stats are not available for this match
                        var shotsOnTarget = driver.GetValuesById("stat1", attempts, 3, "\r\n");
                        if (shotsOnTarget == null)
                        {

                            IWebElement noStats = driver.FindElement(By.Id("noStats"));
                            if (noStats != null)
                            {
                                log.Debug("shotsOnTarget == null Message: " + noStats.Text);
                            }
                            else
                            {
                                log.Warn("shotsOnTarget == null Expected no statistics but it's not displayed for some other reason");
                            }
                            rballOkay = false;
                        }

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


                        if (String.IsNullOrEmpty(time))
                        {
                            log.Warn("Couldn't get time :(");
                            continue;
                        }

                        var vals = new List<string>();

                        Action<Dictionary<string, int>, StatAlias, List<string>, int> setStat =
                            (Dictionary<string, int> d, StatAlias alias, List<string> list, int at) =>
                            {
                                string statString = stat(alias);
                                d[statString] = ParseInt(statString, list.ElementAt(at));
                            };

                        Action<Dictionary<string, int>, StatAlias[], List<string>> setStat2 =
                            (Dictionary<string, int> d, StatAlias[] alias, List<string> list) =>
                            {
                                for (int i = 0; i < alias.Length; ++i)
                                {
                                    string statString = stat(alias[i]);
                                    d[statString] = ParseInt(statString, list.ElementAt(i));
                                }
                            };

                        StatAlias[] aliases = { StatAlias.RedCards, StatAlias.YellowCards, StatAlias.Corners };

                        setStat2(hstats, aliases, hCardsAndCorners);
                        setStat2(astats, aliases, aCardsAndCorners);

                        if (rballOkay)
                        {
                            aliases = new StatAlias[] { StatAlias.ShotsOnTarget, StatAlias.ShotsOffTarget, StatAlias.Attacks, StatAlias.DangerousAttacks };

                            Func<List<string>, string> h = x => x.ElementAt(0);

                            setStat2(hstats, aliases, new List<string> { h(shotsOnTarget), h(shotsOffTarget), h(attacks), h(dangerousAttacks) });

                            Func<List<string>, string> a = x => x.ElementAt(2);

                            setStat2(astats, aliases,
                                new List<string> { a(shotsOnTarget), a(shotsOffTarget), a(attacks), a(dangerousAttacks) });
                        }

                        setStat(hstats, StatAlias.Goals, cleanScores, 0);
                        setStat(astats, StatAlias.Goals, cleanScores, 1);

                     

                     

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
