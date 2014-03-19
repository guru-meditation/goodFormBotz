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
                    driverWrapper.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
                    IWebElement inPlayElement = null;

                    try
                    {
                        inPlayElement = driverWrapper.WaitUntil(ExpectedBotCondition.GetDivContainingText("In-Play"), 60);
                    }
                    catch (WebDriverTimeoutException)
                    {

                        log.Debug("No games in play, going to sleep for a bit....");
                        driverWrapper.Quit();
                        driverWrapper.Dispose();
                        driverWrapper = null;
                        continue;
                    }

                    List<IWebElement> genericRowElements = null;

                    if (inPlayElement != null)
                    {
                        driverWrapper.ClickElement(inPlayElement);

                        bool inPlayGamesOnScreen = driverWrapper.WaitUntil(ExpectedBotCondition.PageHasClassContainingString("genericRow", " v "), 20);
                        
                        genericRowElements = driverWrapper.FindElements(By.ClassName("genericRow")).ToList();

                        log.Warn("Generic Rows: " + genericRowElements.Count);

                        int firstNonMatch = genericRowElements.IndexOf(genericRowElements.First(x => x.Text.Contains(" v ") == false));
                        
                        if (firstNonMatch != -1)
                        {
                            genericRowElements.RemoveRange(firstNonMatch, genericRowElements.Count() - firstNonMatch);
                        }

                        log.Warn("Generic Rows Inplay: " + genericRowElements.Count);
                    }

                    IEnumerable<string> gamesAsText = genericRowElements.Select(x => x.Text);

                    if (idx == -1)
                    {
                        Random random = new Random();
                        idx = random.Next(0, genericRowElements.Count());
                    }

                    int elementCount = 0;

                    genericRowElements.ForEach(x =>
                    {
                        if (idx == elementCount)
                            log.Warn(x.Text);
                        else { }
                        // log.Debug(x.Text); 
                        ++elementCount;
                    });

                    log.Info("Scanning game " + idx + " of " + genericRowElements.Count() + " games in play at " + DateTime.Now.ToUniversalTime());

                    if (idx < genericRowElements.Count())
                    {
                        var hstats = new Dictionary<string, int>();
                        var astats = new Dictionary<string, int>();

                        int attempts = 3;

                        //*[@id="rw_spl_sc_1-1-5-24705317-2-0-0-1-1-0-0-0-0-0-1-0-0_101"]/div[1]
                        genericRowElements.ElementAt(idx).Click();

                        string clockText = "";

                        try
                        {
                            clockText = driverWrapper.WaitUntil(ExpectedBotCondition.ThereIsAClock(), 20);
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
                        string league = "All";

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
