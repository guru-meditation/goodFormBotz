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
    using System.Drawing.Imaging;
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
                driver.ClickElement(element);
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

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            GetElementAndClick(driver, "Level1", "Match Markets");
            System.Threading.Thread.Sleep(dirtySleep);
            GetElementAndClick(driver, "Level2", "Main");
            System.Threading.Thread.Sleep(dirtySleep);
            GetElementAndClick(driver, "genericRow", "Full Time Result");
            System.Threading.Thread.Sleep(dirtySleep);

            // it takes time for genericRow to expand 
            driver.ForceSleep(dirtySleep);

            GetPreMatchData(driver, foundMatches);

            /////////////////////

            int longestTeam1 = foundMatches.Select(x => x.team1).Max(x => x.Length);
            int longestTeam2 = foundMatches.Select(x => x.team2).Max(x => x.Length);
            int longestLeague = foundMatches.Select(x => x.league).Max(x => x.Length);

            int counter = 0;
            foreach (aMatch m in foundMatches)
            {
                log.Debug(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " at " + m.koDateTime.TimeOfDay + " in " + m.league);
                int leagueId    = dbStuff.AddLeague(m.league);
                int hTeamId     = dbStuff.AddTeam(m.team1);
                int aTeamId     = dbStuff.AddTeam(m.team2);
                int gameId =  dbStuff.AddGame(hTeamId, aTeamId, leagueId, m.koDateTime);
                m.id = gameId;
            }

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            GetElementAndClick(driver, "Level2", "Corners");
            driver.DirtySleep(sleepTime);
            GetElementAndClick(driver, "genericRow", "Race To 3 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 3);

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            GetElementAndClick(driver, "genericRow", "Race To 5 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 5);

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            GetElementAndClick(driver, "genericRow", "Race To 7 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 7);

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            GetElementAndClick(driver, "genericRow", "Race To 9 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 9);
            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            GetElementAndClick(driver, "genericRow", "Asian Total Corners");
            System.Threading.Thread.Sleep(dirtySleep);

            driver.ForceSleep(dirtySleep);

            GetAsianCornerData(driver, foundMatches);

            /////////////////////

            foreach (aMatch m in foundMatches)
            {
               
                    log.Debug(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " corner line: " + m.cornerLine + " " + m.homeAsianCornerPrice + "//" + m.awayAsianCornerPrice );

                    if (String.IsNullOrEmpty(m.cornerLine) == false)
                    {
                        dbStuff.AddCornerData(m.id, m.cornerLine, m.homeAsianCornerPrice, m.awayAsianCornerPrice);
                    }
                    
                    if (String.IsNullOrEmpty(m.homeRaceTo3CornersPrice) == false &&
                        String.IsNullOrEmpty(m.awayRaceTo3CornersPrice) == false &&
                        String.IsNullOrEmpty(m.neitherRaceTo3CornersPrice) == false)
                    {
                        dbStuff.AddRaceToCornerData(m.id, 3, m.homeRaceTo3CornersPrice, m.awayRaceTo3CornersPrice, m.neitherRaceTo3CornersPrice);
                    }

                    if (String.IsNullOrEmpty(m.homeRaceTo5CornersPrice) == false &&
                        String.IsNullOrEmpty(m.awayRaceTo5CornersPrice) == false &&
                        String.IsNullOrEmpty(m.neitherRaceTo5CornersPrice) == false)
                    {
                        dbStuff.AddRaceToCornerData(m.id, 5, m.homeRaceTo5CornersPrice, m.awayRaceTo5CornersPrice, m.neitherRaceTo5CornersPrice);
                    }

                    if (String.IsNullOrEmpty(m.homeRaceTo7CornersPrice) == false &&
                        String.IsNullOrEmpty(m.awayRaceTo7CornersPrice) == false &&
                        String.IsNullOrEmpty(m.neitherRaceTo7CornersPrice) == false)
                    {
                        dbStuff.AddRaceToCornerData(m.id, 7, m.homeRaceTo7CornersPrice, m.awayRaceTo7CornersPrice, m.neitherRaceTo7CornersPrice);
                    }

                    if (String.IsNullOrEmpty(m.homeRaceTo9CornersPrice) == false &&
                        String.IsNullOrEmpty(m.awayRaceTo9CornersPrice) == false &&
                        String.IsNullOrEmpty(m.neitherRaceTo9CornersPrice) == false)
                    {
                        dbStuff.AddRaceToCornerData(m.id, 9, m.homeRaceTo9CornersPrice, m.awayRaceTo9CornersPrice, m.neitherRaceTo9CornersPrice);
                    }

                    if (String.IsNullOrEmpty(m.homeWinPrice) == false &&
                        String.IsNullOrEmpty(m.drawPrice) == false &&
                        String.IsNullOrEmpty(m.awayWinPrice) == false)
                    {
                        dbStuff.AddFinalResultPrices(m.id, m.homeWinPrice, m.drawPrice, m.awayWinPrice);
                    }
                
                
                
            }
            /////////////////////

            log.Debug("");
        }

        private void GetRaceToCornerData(DriverWrapper driver, List<aMatch> foundMatches, int raceToValue)
        {
            var rowsCount = driver.FindElements(By.ClassName("genericRow")).Count();

            for (int i = 0; i < rowsCount; ++i)
            {
                try
                {
                    var genItems = driver.FindElements(By.ClassName("genericRow"));

                    IWebElement genItem = null;
                    if (i < genItems.Count())
                    {
                        genItem = genItems.ElementAt(i);
                    }
                    else
                    {
                        log.Error("Can't find item at index: " + i);
                        break;
                    }

                    string leagueText = genItem.Text.Trim();
                    genItem.Click();
                    driver.DirtySleep(dirtySleep);

                     var sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                    for (int j = 1; j < sectionCount; ++j)
                    {
                        var sections = driver.FindElements(By.ClassName("Section"));
                        if (j < sections.Count())
                        {
                            try
                            {
                                sections.ElementAt(j).Click();
                            }
                            catch (Exception ce)
                            {
                                log.Error("Exception caught trying to click at index: " + j);
                            }

                            driver.DirtySleep(500);
                        }
                        else
                        {
                            log.Error("Can't find item at index: " + j);
                            break;
                        }
                    }


                    sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                    for (int j = 0; j < sectionCount; ++j)
                    {
                        var sections = driver.FindElements(By.ClassName("Section"));

                        if (j > sections.Count())
                        {
                            log.Error("Can't find item at index: " + j);
                            break;
                        }

                        var section = sections.ElementAt(j);

                        string sectionText = section.Text;

                        var bits = Regex.Split(sectionText, "\r\n").ToList();
                        bits.ForEach(x => x.Trim());

                        string matchText = bits.ElementAt(0);

                        if (matchText.Contains("\r\n"))
                        {
                            matchText = matchText.Substring(0, matchText.IndexOf("\r\n"));
                        }

                        if (matchText.Contains(" v "))
                        {
                            var teamSplits = Regex.Split(matchText, " v ");

                            var m = new aMatch();
                            m.team1 = teamSplits.ElementAt(0);
                            m.team2 = teamSplits.ElementAt(1);

                            m.team1 = DoSubstitutions(m.team1);
                            m.team2 = DoSubstitutions(m.team2);
                            m.league = DoSubstitutions(leagueText);

                            var thisMatch = foundMatches.SingleOrDefault(x => x.team1 == m.team1 && x.team2 == m.team2);

                            if (thisMatch != null && bits.Count() == 5)
                            {
                                if(bits.ElementAt(2).Contains("  "))
                                {
                                    if (raceToValue == 3) thisMatch.homeRaceTo3CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                    if (raceToValue == 5) thisMatch.homeRaceTo5CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                    if (raceToValue == 7) thisMatch.homeRaceTo7CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                    if (raceToValue == 9) thisMatch.homeRaceTo9CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                }

                                if(bits.ElementAt(3).Contains("  "))
                                {
                                    if (raceToValue == 3) thisMatch.awayRaceTo3CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                    if (raceToValue == 5) thisMatch.awayRaceTo5CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                    if (raceToValue == 7) thisMatch.awayRaceTo7CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                    if (raceToValue == 9) thisMatch.awayRaceTo9CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                }

                                if(bits.ElementAt(4).Contains("  "))
                                {
                                    if (raceToValue == 3) thisMatch.neitherRaceTo3CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                    if (raceToValue == 3) thisMatch.neitherRaceTo5CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                    if (raceToValue == 3) thisMatch.neitherRaceTo7CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                    if (raceToValue == 3) thisMatch.neitherRaceTo9CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                }
                            }
                            else
                            {
                                log.Error("Error occurred");
                            }

                        }
                        else
                        {
                            log.Error("Can't get match from " + matchText);
                        }

                    }
                }
                catch (Exception ce)
                {
                    log.Error("Exception caught: " + ce);
                }

                IJavaScriptExecutor js = driver.Driver as IJavaScriptExecutor;
                js.ExecuteScript("document.getElementById('HeaderBack').click()");

                driver.DirtySleep(dirtySleep);
            }
        }

        private static float GetUkOddsPrice(string bit)
        {
            string price = Regex.Split(bit, "  ").Last();
            string numerator = Regex.Split(price, "/").First();
            string denominator = Regex.Split(price, "/").Last();

            return ( float.Parse(numerator) / float.Parse(denominator)) + 1;
        }

        private void GetAsianCornerData(DriverWrapper driver, List<aMatch> foundMatches)
        {
            var rowsCount = driver.FindElements(By.ClassName("genericRow")).Count();

            for (int i = 0; i < rowsCount; ++i)
            {
                try
                {

                var genItems = driver.FindElements(By.ClassName("genericRow"));

                IWebElement genItem = null;
                if (i < genItems.Count())
                {
                    genItem = genItems.ElementAt(i);
                }
                else
                {
                    log.Error("Can't find item at index: " + i);
                    break;
                }

                string leagueText = genItem.Text.Trim();
                genItem.Click();
                driver.DirtySleep(dirtySleep);

                var sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                for (int j = 1; j < sectionCount; ++j)
                {
                    var sections = driver.FindElements(By.ClassName("Section"));
                    if (j < sections.Count())
                    {
                        try
                        {
                            sections.ElementAt(j).Click();
                        }
                        catch (Exception ce)
                        {
                            log.Error("Exception caught trying to click at index: " + j);
                        }

                        driver.DirtySleep(500);
                    }
                    else
                    {
                        log.Error("Can't find item at index: " + j);
                        break;
                    }
                }

                
                    sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                    for (int j = 0; j < sectionCount; ++j)
                    {
                        var sections = driver.FindElements(By.ClassName("Section"));
                        
                        if (j > sections.Count())
                        {
                            log.Error("Can't find item at index: " + j);
                            break;
                        } 
                        
                        var section = sections.ElementAt(j);

                        string sectionText = section.Text;

                        var bits = Regex.Split(sectionText, "\r\n").ToList();
                        bits.ForEach(x => x.Trim());


                        string matchText = bits.ElementAt(0);

                        if (matchText.Contains("\r\n"))
                        {
                            matchText = matchText.Substring(0, matchText.IndexOf("\r\n"));
                        }

                        if (matchText.Contains(" v "))
                        {
                            var teamSplits = Regex.Split(matchText, " v ");

                            var m = new aMatch();
                            m.team1 = teamSplits.ElementAt(0);
                            m.team2 = teamSplits.ElementAt(1);

                            m.team1 = DoSubstitutions(m.team1);
                            m.team2 = DoSubstitutions(m.team2);
                            m.league = DoSubstitutions(leagueText);

                            var thisMatch = foundMatches.SingleOrDefault(x => x.team1 == m.team1 && x.team2 == m.team2);

                            if (thisMatch != null && bits.Count() == 6)
                            {
                                var temp = bits.ElementAt(2);
                                if (temp.StartsWith("O "))
                                {
                                    thisMatch.cornerLine = temp.Substring(2);
                                }
                                else
                                {
                                    log.Debug("temp: " + temp);
                                }

                                thisMatch.homeAsianCornerPrice = bits.ElementAt(3).Trim();
                                thisMatch.awayAsianCornerPrice = bits.ElementAt(5).Trim();

                            }
                            else
                            {
                                log.Error("Error occurred");
                            }

                        }
                        else
                        {
                            log.Error("Can't get match from " + matchText);
                        }

                    }
                }
                catch (Exception ce)
                {
                    log.Error("Exception caught: " + ce);
                }

                IJavaScriptExecutor js = driver.Driver as IJavaScriptExecutor;
                js.ExecuteScript("document.getElementById('HeaderBack').click()");

                driver.DirtySleep(dirtySleep);
            }
        }

        private void GetPreMatchData(DriverWrapper driver, List<aMatch> foundMatches)
        {
            var rowsCount = driver.FindElements(By.ClassName("genericRow")).Count();

            for(int i = 0; i < rowsCount; ++i)
            {
                var genItems = driver.FindElements(By.ClassName("genericRow"));
                
                IWebElement genItem = null;
                if (i < genItems.Count())
                {
                    genItem = genItems.ElementAt(i);
                }
                else
                {
                    log.Error("Can't find item at index: " + i);
                    break;
                }

                string leagueText = genItem.Text.Trim();
                genItem.Click();
                driver.DirtySleep(dirtySleep);

                var sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                for (int j = 1; j < sectionCount; ++j)
                {
                    var sections = driver.FindElements(By.ClassName("Section"));
                    if (j < sections.Count())
                    {
                        try
                        {
                            sections.ElementAt(j).Click();
                        }
                        catch (Exception ce)
                        {
                            log.Error("Exception caught trying to click at index: " + j);
                        }

                        driver.DirtySleep(500);
                    }
                    else
                    {
                        log.Error("Can't find item at index: " + j);
                        break;
                    }
                }

                sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                for (int j = 0; j < sectionCount; ++j)
                {
                    var m = new aMatch();
                    
                    var sections = driver.FindElements(By.ClassName("Section"));

                    if (j > sections.Count())
                    {
                        log.Error("Can't find item at index: " + j);
                        break;
                    } 

                    var sectionTexts = Regex.Split(sections.ElementAt(j).Text, "\r\n");

                    if (sectionTexts.Count() == 8)
                    {
                        string matchText = sectionTexts.ElementAt(0);

                        if (matchText.Contains(" v "))
                        {
                            var teamSplits = Regex.Split(matchText, " v ");

                            m.team1 = teamSplits.ElementAt(0);
                            m.team2 = teamSplits.ElementAt(1);

                            m.team1 = DoSubstitutions(m.team1);
                            m.team2 = DoSubstitutions(m.team2);
                            m.league = DoSubstitutions(leagueText);

                            try
                            {
                                m.koDateTime = DateTime.ParseExact(sectionTexts.ElementAt(1).Substring(0,12) , "dd MMM HH:mm", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ce)
                            {
                                log.Error("Couldn't parse a date out of :" + sectionTexts.ElementAt(1));

                            }

                            m.homeWinPrice  = GetUkOddsPrice(sectionTexts.ElementAt(3)).ToString();
                            m.drawPrice = GetUkOddsPrice(sectionTexts.ElementAt(5)).ToString();
                            m.awayWinPrice = GetUkOddsPrice(sectionTexts.ElementAt(7)).ToString();

                            foundMatches.Add(m);
                        }
                        else
                        {
                            log.Error("Can't get match from " + matchText);
                        }
                    }
                    else
                    {
                        log.Error("Wrong text in section: " + sections.ElementAt(j));
                    }
                }

                IJavaScriptExecutor js = driver.Driver as IJavaScriptExecutor;
                js.ExecuteScript("document.getElementById('HeaderBack').click()");

                driver.DirtySleep(dirtySleep);
            }
        }

        public override void scan(int sleepTime)
        {
            DriverWrapper driverWrapper = null;

            int idx = -1;

            DateTime lastDayGamesUpdated = DateTime.MinValue;

            if (driverWrapper == null)
            {
                string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";
                driverWrapper = driverCreator.CreateDriver(agentString);
            }

            if (skipGames == false)
            {
                log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                AddTodaysMatches(sleepTime, driverWrapper);
            }

            lastDayGamesUpdated = DateTime.Today.ToUniversalTime();
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

                    if (DateTime.Today.ToUniversalTime().Equals(lastDayGamesUpdated) == false)
                    //if(true)
                    {
                        if (DateTime.Now.ToUniversalTime().TimeOfDay > TimeSpan.FromHours(3))
                        {
                            lastDayGamesUpdated = DateTime.Today.ToUniversalTime();
                        }

                        log.Info("Scanning today's games for " + lastDayGamesUpdated.Date);
                        AddTodaysMatches(sleepTime, driverWrapper);
                    }
                    else
                    {
                        log.Info("Already scanned todays games for " + lastDayGamesUpdated.Date);
                    }

                    //load the main page
                    driverWrapper.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";

                    var waiter = new WebDriverWait(driverWrapper, TimeSpan.FromSeconds(60));
                    IWebElement inPlayElement = null;

                    try
                    {

                        inPlayElement = waiter.Until(ExpectedBotCondition.GetDivContainingText("In-Play"));
                        //inPlayElement = waiter.Until(ExpectedBotCondition.PageHasClassWithText("Level1", "In-Play"));
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

                        bool inPlayGamesOnScreen = waiter.Until(ExpectedBotCondition.PageHasClassContainingString("genericRow", " v "));
                        
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
                    //int attemtps = 3;
                    //do
                    //{
                    //    idx = dbStuff.GetActiveBotStates(gamesAsText);
                    //    --attemtps;
                    //}
                    //while (idx == -1 && attemtps != 0);

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
                        
                        waiter = new WebDriverWait(driverWrapper, TimeSpan.FromSeconds(20));
                        var clockText = "";

                        try
                        {
                            waiter.Until<Boolean>((d) =>
                            {
                                bool retVal = false;
                                var clocks = d.FindElements(By.Id("mlClock"));
                                if (clocks.Count != 0)
                                {
                                    clockText = clocks[0].Text;
                                    retVal = clockText.Contains(':');
                                }
                                return retVal;
                            });
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

                        waiter.Until<Boolean>((d) =>
                        {
                            return d.FindElement(By.Id("arena")).GetAttribute("style") == "height: 144px;";
                        });


                        IJavaScriptExecutor js = driverWrapper.Driver as IJavaScriptExecutor;
                        js.ExecuteScript("document.getElementsByClassName('carousel')[0].setAttribute('style', '-webkit-transform: translate(-50%, 0px);')");

                        string hCardsAndCornersText = "";
                        string aCardsAndCornersText = "";

                        waiter.Until<Boolean>((d) =>
                        {
                            var elems = d.FindElements(By.Id("team1IconStats"));
                            if (elems.Count != 0)
                            {
                                hCardsAndCornersText = elems.First().Text;
                            }
                            return hCardsAndCornersText.Split(' ').Count() == 3;
                        });

                        waiter.Until<Boolean>((d) =>
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
