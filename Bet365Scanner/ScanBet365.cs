using OpenQA.Selenium;
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
    using OpenQA.Selenium.Support.UI;
    using System.Linq.Expressions;
    using WebDriver;

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
            DriverWrapper driverWrapper = null;

            //TODO: this was always set to 0 in original code
            int botIndex = 0;
            int idx = -1;
            bool firstTime = true;

            DateTime lastDayGamesUpdated = DateTime.MinValue;

            if (driverWrapper == null)
            {
                string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                driverWrapper = driverCreator.CreateDriver(agentString);
            }

            if (botIndex == 0 && skipGames == false)
            {
                log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                AddTodaysMatches(sleepTime, driverWrapper);
            }

            lastDayGamesUpdated = DateTime.Today;

            int badLoopCounter = 0;

            while (true)
            {
                idx++;
                try
                {
                    if (driverWrapper == null)
                    {
                        string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                        driverWrapper = driverCreator.CreateDriver(agentString);
                    }

                    if (DateTime.Today.Equals(lastDayGamesUpdated) == false)
                    {
                        if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(3))
                        {
                            lastDayGamesUpdated = DateTime.Today;
                        }

                        log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                        AddTodaysMatches(sleepTime, driverWrapper);
                    }
                    else
                    {
                        log.Info("Already scanned todays games for " + lastDayGamesUpdated.Date);
                    }

                    driverWrapper.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
                    driverWrapper.DirtySleep(sleepTime);

                    //string inPlayXPath = "//*[@id=\"sc_0_L1_1-1-5-0-0-0-0-1-1-0-0-0-0-0-1-0-0-0-0\"]";
                    IWebElement inPlayElement = driverWrapper.GetWebElementFromClassAndDivText("Level1", "In-Play");

                    driverWrapper.ClickElement(inPlayElement, 10);

                    var elements = driverWrapper.FindElements(By.ClassName("genericRow")).ToList();

                    log.Warn("Generic Rows: " + elements.Count);

                    int firstNonMatch = elements.IndexOf(elements.First(x => x.Text.Contains(" v ") == false));

                    if (firstNonMatch != -1)
                    {
                        elements.RemoveRange(firstNonMatch, elements.Count() - firstNonMatch);
                    }

                    log.Warn("Generic Rows Inplay: " + elements.Count);

                    foreach (var ele in elements)
                    {
                        log.Warn(ele.Text);
                    }

                    if (elements.Count() == 0)
                    {
                        log.Debug("No games in play, going to sleep for a bit....");
                        driverWrapper.DirtySleep(20000);
                        driverWrapper.Quit();
                        driverWrapper.Dispose();
                        driverWrapper = null;
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

                        var wait = new WebDriverWait(driverWrapper, TimeSpan.FromSeconds(20));
                        var clockText = "";

                        try
                        {
                            wait.Until<Boolean>((d) =>
                            {
                                bool retVal = false;
                                var clocks = d.FindElements(By.Id("mlClock"));
                                if (clocks.Count != 0)
                                {
                                    clockText = clocks[0].Text;
                                    retVal =  clockText.Contains(':');
                                }
                                return retVal;
                            });
                        }
                        catch (Exception)
                        {
                            log.Warn("cleanScores == null");

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

                        wait.Until<Boolean>((d) =>
                        {
                            return d.FindElement(By.Id("arena")).GetAttribute("style") == "height: 144px;";
                        });

                        IJavaScriptExecutor js = driverWrapper.Driver as IJavaScriptExecutor;
                        js.ExecuteScript("document.getElementsByClassName('carousel')[0].setAttribute('style', '-webkit-transform: translate(-50%, 0px);')");

                        string hCardsAndCornersText = "";
                        string aCardsAndCornersText = "";

                        wait.Until<Boolean>((d) =>
                        {
                            var elems = d.FindElements(By.Id("team1IconStats"));
                            if( elems.Count != 0)
                            {
                                hCardsAndCornersText = elems.First().Text; 
                            }
                            return hCardsAndCornersText.Split(' ').Count() == 3;
                        });

                        wait.Until<Boolean>((d) =>
                        {
                            var elems = d.FindElements(By.Id("team2IconStats"));
                            if (elems.Count != 0)
                            {
                                aCardsAndCornersText = elems.First().Text;
                            }
                            return aCardsAndCornersText.Split(' ').Count() == 3;
                        });

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

                        var inPlayTitles = driverWrapper.GetValuesByClassName("InPlayTitle", attempts, 1, new char[] { '@' });
                        if (inPlayTitles == null) { log.Warn("inPlayTitles == null"); continue; }

                        bool rballOkay = true;
                        // stats are not available for this match

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

                        log.Warn("Rball 1 = " + rballOkay);

                        if (rballOkay == true)
                        {
                            shotsOffTarget = driverWrapper.GetValuesById("stat2", attempts, 3, "\r\n");
                            if (shotsOffTarget == null) { log.Warn("shotsOffTarget == null"); rballOkay = false; }

                            attacks = driverWrapper.GetValuesById("stat3", attempts, 3, "\r\n");
                            if (attacks == null) { log.Warn("attacks == null"); rballOkay = false; }

                            dangerousAttacks = driverWrapper.GetValuesById("stat4", attempts, 3, "\r\n");
                            if (dangerousAttacks == null) { log.Warn("dangerousAttacks == null"); rballOkay = false; }
                        }

                        log.Warn("Rball 2 = " + rballOkay);

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

                        log.Warn("Rball 3 = " + rballOkay);

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

                        log.Warn("Rball 4 = " + rballOkay);

                        //SendToWebDelegate sd = new SendToWebDelegate(SendToWeb);
                        //sd.BeginInvoke(league, bOverMidnight ? DateTime.Today - TimeSpan.FromDays(1) : DateTime.Now, homeTeamName, awayTeamName, hstats, astats, time, null, null);
                        SendToWeb(league, bOverMidnight ? DateTime.Today - TimeSpan.FromDays(1) : DateTime.Now, homeTeamName, awayTeamName, hstats, astats, clockText);
                        
                        WriteXmlDelegate wd = new WriteXmlDelegate(WriteXml);
                        wd.BeginInvoke(xmlPath, hstats, astats, homeTeamName, awayTeamName, league, clockText, exists, finalName, null, null);
                    }
                    else
                    {
                        idx = -1;
                    }
                }
                catch (OpenQA.Selenium.WebDriverException we)
                {
                    log.Error("Exception thrown: " + we);
                    driverWrapper.Quit();
                    driverWrapper.Dispose();
                    driverWrapper = null;

                }
                catch (Exception we)
                {
                    log.Error("Exception thrown: " + we);
                    driverWrapper.Quit();
                    driverWrapper.Dispose();
                    driverWrapper = null;
                }
            }
        }

    }

}
