using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BotSpace;
using Db;
using BrowserAutomation;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace Scanners
{

    class Game
    {
        public string competitionName;
        public string team1;
        public string team2;

        public override string ToString()
        {
            return team1 + " v " + team2 + " in " + competitionName + System.Environment.NewLine;
        }
    }

    public class ScanBet365 : Scanner
    {
        private static readonly log4net.ILog log
              = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<Game> competitions = new List<Game>();

        public ScanBet365(DriverCreator creator, Database db, string xml_path, bool skip_games)
            : base(creator, db, xml_path, skip_games)
        {
        }

        private string Chomp(List<string> sectionTexts)
        {
            string retVal = "";

            if (sectionTexts.Count() > 0)
            {
                retVal = sectionTexts.First();
                sectionTexts.RemoveAt(0);
            }

            return retVal;
        }

        private List<string> excludeString = new List<string>() {"COUPONS",
            "1ST HALF ASIANS IN-PLAY",
            "IN-PLAY COUPON",
            "ASIANS IN-PLAY",
            "VIDEO"
        };

        public override void scan(int sleepTime)
        {
            DriverWrapper driverWrapper = null;

            int idx = -1;

            if (driverWrapper == null)
            {
                string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                driverWrapper = DriverFactory.getDriverWaiter(BrowserAutomation.DriverFactory.Browser.Chrome, agentString);
            }

            int badLoopCounter = 0;
            string botID = System.Guid.NewGuid().ToString();

            while (true)
            {
                idx++;
                try
                {
                    if (driverWrapper == null)
                    {
                        string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                        
                        driverWrapper = driverCreator.CreateDriver(agentString);
                        
                        if (driverWrapper == null)
                        {
                            log.Error("Failed to make a Selenium Driver");
                            continue;
                        }
                    }

                    //load the main page
                    driverWrapper.Url = "https://mobile.bet365.com/premium/#type=InPlay;key=1;ip=1;lng=1";
                    //driverWrapper.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
                    bool inPlayElement = false;

                    try
                    {
                        inPlayElement = driverWrapper.WaitUntil(ExpectedBotCondition.VerifyInplayScreen(), 60);
                    }
                    catch (WebDriverTimeoutException)
                    {

                        log.Debug("No games in play, going to sleep for a bit....");
                        driverWrapper.Quit();
                        driverWrapper.Dispose();
                        driverWrapper = null;
                        continue;
                    }

                    List<IWebElement> fixtureElements = null;

                    if (inPlayElement)
                    {
                        fixtureElements = driverWrapper.FindElements(By.ClassName("Fixture")).ToList();
                        int removed = fixtureElements.RemoveAll(x => x.GetAttribute("class") != "Fixture");

                        log.Warn("Fixtures: " + fixtureElements.Count);
                        log.Warn("Removed: " + removed); 

                        var fixtureList = driverWrapper.FindElement(By.ClassName("FixtureList"));
                        var fText = fixtureList.Text;

                        var fixtureSplits = Regex.Split(fText, "\r\n").ToList();

                        fixtureSplits.RemoveAll(x => excludeString.Contains(x.ToUpper()));

                        var competitionName = "";
                        competitions.Clear();

                        while (fixtureSplits.Count() != 0)
                        {
                            var tempBuf = new List<string>();

                            fixtureSplits.RemoveAll(x => x == "VIDEO");

                            Game a = null;

                            while (fixtureSplits.Count() != 0)
                            {
                                if (fixtureSplits.First().StartsWith("  "))
                                {
                                    tempBuf.Add(Chomp(fixtureSplits));
                                    break;
                                }
                                else
                                {
                                    tempBuf.Add(Chomp(fixtureSplits));
                                }
                            }

                            if (tempBuf.Count() == 2)
                            {
                                var team1 = tempBuf[0];
                                var team2 = tempBuf[1];

                                if (team1.Contains(":"))
                                {
                                    team1 = team1.Substring(team1.IndexOf(' '));
                                }

                                a = new Game();
                                a.competitionName = competitionName.Trim();
                                a.team1 = team1.Trim();
                                a.team2 = team2.Trim();

                            }
                            else if (tempBuf.Count() == 3)
                            {
                                competitionName = tempBuf[0];
                                var team1 = tempBuf[1];
                                var team2 = tempBuf[2];

                                if (team1.Contains(":"))
                                {
                                    team1 = team1.Substring(team1.IndexOf(' '));
                                }

                                a = new Game();
                                a.competitionName = competitionName.Trim();
                                a.team1 = team1.Trim();
                                a.team2 = team2.Trim();
                            }
                            else
                            {
                                log.Info("Unexpected number of string in temp buf");
                            }

                            if (a != null)
                            {
                                competitions.Add(a);
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    if (idx == -1)
                    {
                        Random random = new Random();
                        idx = random.Next(0, fixtureElements.Count());
                    }

                    int elementCount = 0;

                    fixtureElements.ForEach(x =>
                    {
                        if (idx == elementCount)
                            log.Warn(x.Text);
                        else { }
                        // log.Debug(x.Text); 
                        ++elementCount;
                    });

                    log.Info("Scanning game " + idx + " of " + fixtureElements.Count() + " games in play at " + DateTime.Now.ToUniversalTime());

                    if (idx < fixtureElements.Count())
                    {
                        var hstats = new Dictionary<string, int>();
                        var astats = new Dictionary<string, int>();

                        int attempts = 3;

                        fixtureElements.ElementAt(idx).Click();

                        string clockText = "";

                        try
                        {
                            bool clockIsOnScreen = driverWrapper.WaitUntil(ExpectedBotCondition.ThereIsAClock(), 20);

                            if (clockIsOnScreen)
                            {
                                var clock = driverWrapper.FindElement(By.Id("mlClock"));
                                if (clock.Text.Contains(':'))
                                {
                                    clockText = clock.Text;
                                }
                            }

                            if (String.IsNullOrEmpty(clockText))
                            {
                                log.Error("No time avaiable!!!!");
                                throw new Exception();
                            }

                        }
                        catch (Exception vr)
                        {
                            log.Warn("cleanScores == null -  " + vr);

                            ++badLoopCounter;

                            if (badLoopCounter == 5)
                            {
                                log.Warn("Bad loop counter reset...");

                                badLoopCounter = 0;
                                driverWrapper.Quit();
                                driverWrapper.Dispose();
                                driverWrapper = null;
                            }

                            continue;
                        }

                        try
                        {
                            driverWrapper.WaitUntil(ExpectedBotCondition.ThereIsAnIdWithAttributeWithValue("arena", "style", "height: 144px;"), 5);
                        }
                        catch
                        {
                            log.Warn("Couldn't find stats areas, continuing....");
                            continue;
                        }

                        System.Threading.Thread.Sleep(400);

                        IJavaScriptExecutor js = driverWrapper.Driver as IJavaScriptExecutor;
                        js.ExecuteScript("document.getElementsByClassName('carousel')[0].setAttribute('style', '-webkit-transform: translate(-50%, 0px);')");

                        System.Threading.Thread.Sleep(400);

                        string hCardsAndCornersText = "";
                        string aCardsAndCornersText = "";

                        Func<IWebDriver, bool> f1 = driver =>
                        {
                            ReadOnlyCollection<IWebElement> elems = null;

                            try
                            {
                                elems = driver.FindElements(By.Id("team1IconStats"));
                            }
                            catch { }

                            if (elems.Count != 0)
                            {
                                hCardsAndCornersText = elems.First().Text;
                            }
                            return hCardsAndCornersText.Split(' ').Count() == 3;
                        };

                        Func<IWebDriver, bool> f2 = driver =>
                        {
                            ReadOnlyCollection<IWebElement> elems = null;

                            try
                            {
                                elems = driver.FindElements(By.Id("team2IconStats"));
                            }
                            catch { }

                            if (elems.Count != 0)
                            {
                                aCardsAndCornersText = elems.First().Text;
                            }
                            return aCardsAndCornersText.Split(' ').Count() == 3;
                        };

                        try
                        {
                            driverWrapper.WaitUntilConditionIsTrue(f1, 2);
                            driverWrapper.WaitUntilConditionIsTrue(f2, 2);
                        }
                        catch
                        {
                            try
                            {
                                js.ExecuteScript("document.getElementsByClassName('carousel')[0].setAttribute('style', '-webkit-transform: translate(-50%, 0px);')");
                                driverWrapper.WaitUntilConditionIsTrue(f1, 3);
                                driverWrapper.WaitUntilConditionIsTrue(f2, 3);
                            }
                            catch { }
                        }

                        if (hCardsAndCornersText == "" ||
                            aCardsAndCornersText == "")
                        {
                            log.Warn("hCardsAndCorners == null");
                            log.Warn("Resetting driver...");

                            ++badLoopCounter;

                            if (badLoopCounter == 5)
                            {
                                badLoopCounter = 0;
                                driverWrapper.Quit();
                                driverWrapper.Dispose();
                                driverWrapper = null;
                            }

                            continue;
                        }

                        var inPlayTitles = driverWrapper.GetValuesByClassName("EventViewTitle", attempts, 1, new char[] { '@' });
                        if (inPlayTitles == null) { log.Warn("inPlayTitles == null"); continue; }

                        bool rballOkay = true;

                        List<string> shotsOnTarget = null;
                        List<string> shotsOffTarget = null;
                        List<string> attacks = null;
                        List<string> dangerousAttacks = null;

                        shotsOnTarget = driverWrapper.GetValuesById("stat1", attempts, 3, "\r\n");

                        if (shotsOnTarget == null)
                        {
                            IWebElement noStats = driverWrapper.FindElement(By.Id("noStats"));
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

                        if (rballOkay == true)
                        {
                            shotsOffTarget = driverWrapper.GetValuesById("stat2", attempts, 3, "\r\n");
                            if (shotsOffTarget == null) { log.Warn("shotsOffTarget == null"); rballOkay = false; }

                            attacks = driverWrapper.GetValuesById("stat3", attempts, 3, "\r\n");
                            if (attacks == null) { log.Warn("attacks == null"); rballOkay = false; }

                            dangerousAttacks = driverWrapper.GetValuesById("stat4", attempts, 3, "\r\n");
                            if (dangerousAttacks == null) { log.Warn("dangerousAttacks == null"); rballOkay = false; }
                        }

                        string inPlayTitle = inPlayTitles.ElementAt(0);
                        
                        if(inPlayTitle.Contains("\r\n"))
                        {
                            inPlayTitle = inPlayTitle.Substring(0, inPlayTitle.IndexOf("\r\n"));
                        }

                        var vals = new List<string>();

                        Action<Dictionary<string, int>, StatAlias, string, int> setStat =
                            (Dictionary<string, int> d, StatAlias alias, string val, int at) =>
                            {
                                string statString = stat(alias);
                                d[statString] = ParseInt(statString, val);
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

                        setStat2(hstats, aliases, hCardsAndCornersText.Split(' ').ToList());
                        setStat2(astats, aliases, aCardsAndCornersText.Split(' ').ToList());

                        if (rballOkay)
                        {
                            aliases = new StatAlias[] { StatAlias.ShotsOnTarget, StatAlias.ShotsOffTarget, StatAlias.Attacks, StatAlias.DangerousAttacks };

                            Func<List<string>, string> h = x => x.ElementAt(0);

                            setStat2(hstats, aliases, new List<string> { h(shotsOnTarget), h(shotsOffTarget), h(attacks), h(dangerousAttacks) });

                            Func<List<string>, string> a = x => x.ElementAt(2);

                            setStat2(astats, aliases,
                                new List<string> { a(shotsOnTarget), a(shotsOffTarget), a(attacks), a(dangerousAttacks) });
                        }

                        var team1score = driverWrapper.FindElement(By.Id("team1score")).Text;
                        var team2score = driverWrapper.FindElement(By.Id("team2score")).Text;
                        setStat(hstats, StatAlias.Goals, team1score, 0);
                        setStat(astats, StatAlias.Goals, team2score, 1);

                        var teams = Regex.Split(inPlayTitle, " v ");

                        string homeTeamName = DoSubstitutions(teams.ElementAt(0));
                        string awayTeamName = DoSubstitutions(teams.ElementAt(1));

                        string today = DateTime.Now.ToUniversalTime().ToString("ddMMyy");

                        Game maybeGame = null;

                        try
                        {
                            maybeGame = competitions.SingleOrDefault(x => x.team1.ToUpper().StartsWith(homeTeamName.ToUpper()) && x.team2.ToUpper().StartsWith(awayTeamName.ToUpper()));
                        }
                        catch (Exception ce)
                        {
                            log.Warn("Exception thrown in your shit code!:" + ce);
                        }

                        string league = "All";

                        if (maybeGame != null)
                        {
                            league = DoSubstitutions(maybeGame.competitionName);
                        }

                        string yesterday = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(1)).ToString("ddMMyy");

                        string finalName = "";

                        try
                        {

                            finalName = Path.Combine(xmlPath, league, homeTeamName + " v " + awayTeamName + "_" + today + ".xml");
                        }
                        catch ( Exception ce )
                        {
                            log.Warn("Another exception thrown in your shit code!:" + ce);
                            throw ce;
                        }

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
                        sd.BeginInvoke(league, bOverMidnight ? DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(1) : DateTime.Now.ToUniversalTime(), homeTeamName, awayTeamName, hstats, astats, clockText, null, null);
                        //SendToWeb(league, bOverMidnight ? DateTime.Today - TimeSpan.FromDays(1) : DateTime.Now, homeTeamName, awayTeamName, hstats, astats, clockText);

                        WriteXmlDelegate wd = new WriteXmlDelegate(WriteXml);
                        wd.BeginInvoke(xmlPath, hstats, astats, homeTeamName, awayTeamName, league, clockText, exists, finalName, null, null);
                    }
                    else
                    {
                        idx = -1;
                    }
                }
                catch (System.Net.WebException we)
                {
                    log.Warn("Caught Web Exception: " + we);
                    continue;

                }
                catch (OpenQA.Selenium.WebDriverException we)
                {
                    log.Error("Exception thrown: " + we);
                    if (driverWrapper != null)
                    {
                        driverWrapper.Quit();
                        driverWrapper.Dispose();
                        driverWrapper = null;
                    }

                }
                catch (Exception we)
                {
                    log.Error("Exception thrown: " + we);
                    if (driverWrapper != null)
                    {
                        driverWrapper.Quit();
                        driverWrapper.Dispose();
                        driverWrapper = null;
                    }
                }
            }
        }

        private void getMyWork(string botID)
        {
            
        }
    }
}
