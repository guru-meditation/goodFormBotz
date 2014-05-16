using BotSpace;
using Db;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OddsBot
{
    class OddScanner
    {
        public OddScanner(Database db)
        {
            m_dbStuff = db;
        }

        public int m_sleepTime = 2000;

        private static readonly log4net.ILog log
           = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Database m_dbStuff;

        protected static Dictionary<string, string> m_subs = new Dictionary<string, string>()
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

        protected string DoSubstitutions(string aString)
        {
            foreach (var sub in m_subs)
            {
                if (aString.Contains(sub.Key))
                {
                    aString = Regex.Replace(aString, sub.Key, sub.Value);
                }
            }
            return aString;
        }

        public void AddTodaysMatches(int sleepTime, DriverWrapper driver)
        {
            var foundMatches = new List<aMatch>();

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";

            driver.ForceSleep(sleepTime);
            driver.GetElementAndClick("splashSection", "MATCH MARKETS");
            driver.ForceSleep(sleepTime);
            driver.GetElementAndClick("splashSection", "MAIN");
            driver.ForceSleep(sleepTime);
            driver.GetElementAndClick("genericRow", "Full Time Result");
            driver.ForceSleep(sleepTime);

            // it takes time for genericRow to expand 
            GetPreMatchData(driver, foundMatches);
            /////////////////////

            int longestTeam1 = foundMatches.Select(x => x.team1).Max(x => x.Length);
            int longestTeam2 = foundMatches.Select(x => x.team2).Max(x => x.Length);
            int longestLeague = foundMatches.Select(x => x.league).Max(x => x.Length);

            foreach (aMatch m in foundMatches)
            {
                Console.WriteLine(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " at " + m.koDateTime.TimeOfDay + " in " + m.league);
                int leagueId = m_dbStuff.AddLeague(m.league);
                int hTeamId = m_dbStuff.AddTeam(m.team1);
                int aTeamId = m_dbStuff.AddTeam(m.team2);
                int gameId = m_dbStuff.AddGame(hTeamId, aTeamId, leagueId, m.koDateTime);
                m.id = gameId;
            }

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.ForceSleep(sleepTime);

            driver.GetElementAndClick("splashSection", "CORNERS");
            driver.ForceSleep(sleepTime);

            //driver.GetElementAndClick("genericRow", "Race To 3 Corners");
            //driver.ForceSleep(sleepTime);

            //GetRaceToCornerData(driver, foundMatches, 3);

            ///////////////////////

            //driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";

            //driver.ForceSleep(sleepTime);
            //driver.GetElementAndClick("genericRow", "Race To 5 Corners");
            //driver.ForceSleep(sleepTime);

            //GetRaceToCornerData(driver, foundMatches, 5);

            ///////////////////////

            //driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";

            //driver.ForceSleep(sleepTime);
            //driver.GetElementAndClick("genericRow", "Race To 7 Corners");
            //driver.ForceSleep(sleepTime);

            //GetRaceToCornerData(driver, foundMatches, 7);

            ///////////////////////

            //driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";

            //driver.ForceSleep(sleepTime);
            //driver.GetElementAndClick("genericRow", "Race To 9 Corners");
            //driver.ForceSleep(sleepTime);

            //GetRaceToCornerData(driver, foundMatches, 9);
            /////////////////////

            //driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            //driver.ForceSleep(sleepTime);

            driver.GetElementAndClick("genericRow", "Asian Total Corners");
            
            System.Threading.Thread.Sleep(sleepTime);
            driver.ForceSleep(sleepTime);

            GetAsianCornerData(driver, foundMatches);

            /////////////////////

            foreach (aMatch m in foundMatches)
            {

                Console.WriteLine(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " corner line: " + m.cornerLine + " " + m.homeAsianCornerPrice + "//" + m.awayAsianCornerPrice);

                if (String.IsNullOrEmpty(m.cornerLine) == false)
                {
                    m_dbStuff.AddCornerData(m.id, m.cornerLine, m.homeAsianCornerPrice, m.awayAsianCornerPrice);
                }
            
            //    if (String.IsNullOrEmpty(m.homeRaceTo3CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.awayRaceTo3CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.neitherRaceTo3CornersPrice) == false)
            //    {
            //        m_dbStuff.AddRaceToCornerData(m.id, 3, m.homeRaceTo3CornersPrice, m.awayRaceTo3CornersPrice, m.neitherRaceTo3CornersPrice);
            //    }

            //    if (String.IsNullOrEmpty(m.homeRaceTo5CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.awayRaceTo5CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.neitherRaceTo5CornersPrice) == false)
            //    {
            //        m_dbStuff.AddRaceToCornerData(m.id, 5, m.homeRaceTo5CornersPrice, m.awayRaceTo5CornersPrice, m.neitherRaceTo5CornersPrice);
            //    }

            //    if (String.IsNullOrEmpty(m.homeRaceTo7CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.awayRaceTo7CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.neitherRaceTo7CornersPrice) == false)
            //    {
            //        m_dbStuff.AddRaceToCornerData(m.id, 7, m.homeRaceTo7CornersPrice, m.awayRaceTo7CornersPrice, m.neitherRaceTo7CornersPrice);
            //    }

            //    if (String.IsNullOrEmpty(m.homeRaceTo9CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.awayRaceTo9CornersPrice) == false &&
            //        String.IsNullOrEmpty(m.neitherRaceTo9CornersPrice) == false)
            //    {
            //        m_dbStuff.AddRaceToCornerData(m.id, 9, m.homeRaceTo9CornersPrice, m.awayRaceTo9CornersPrice, m.neitherRaceTo9CornersPrice);
            //    }

                if (String.IsNullOrEmpty(m.homeWinPrice) == false &&
                    String.IsNullOrEmpty(m.drawPrice) == false &&
                    String.IsNullOrEmpty(m.awayWinPrice) == false)
                {
                    m_dbStuff.AddFinalResultPrices(m.id, m.homeWinPrice, m.drawPrice, m.awayWinPrice);
                }
            }
            ///////////////////////

            Console.WriteLine("Finished");
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
                        Console.WriteLine("Can't find item at index: " + i);
                        break;
                    }

                    string leagueText = genItem.Text.Trim();
                    genItem.Click();
                    driver.ForceSleep(m_sleepTime);

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
                                Console.WriteLine("Exception caught trying to click at index: " + j);
                            }

                            driver.ForceSleep(500);
                        }
                        else
                        {
                            Console.WriteLine("Can't find item at index: " + j);
                            break;
                        }
                    }


                    sectionCount = driver.FindElements(By.ClassName("Section")).Count();

                    for (int j = 0; j < sectionCount; ++j)
                    {
                        var sections = driver.FindElements(By.ClassName("Section"));

                        if (j > sections.Count())
                        {
                            Console.WriteLine("Can't find item at index: " + j);
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
                                if (bits.ElementAt(2).Contains("  "))
                                {
                                    if (raceToValue == 3) thisMatch.homeRaceTo3CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                    if (raceToValue == 5) thisMatch.homeRaceTo5CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                    if (raceToValue == 7) thisMatch.homeRaceTo7CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                    if (raceToValue == 9) thisMatch.homeRaceTo9CornersPrice = GetUkOddsPrice(bits.ElementAt(2)).ToString();
                                }

                                if (bits.ElementAt(3).Contains("  "))
                                {
                                    if (raceToValue == 3) thisMatch.awayRaceTo3CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                    if (raceToValue == 5) thisMatch.awayRaceTo5CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                    if (raceToValue == 7) thisMatch.awayRaceTo7CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                    if (raceToValue == 9) thisMatch.awayRaceTo9CornersPrice = GetUkOddsPrice(bits.ElementAt(3)).ToString();
                                }

                                if (bits.ElementAt(4).Contains("  "))
                                {
                                    if (raceToValue == 3) thisMatch.neitherRaceTo3CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                    if (raceToValue == 5) thisMatch.neitherRaceTo5CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                    if (raceToValue == 7) thisMatch.neitherRaceTo7CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                    if (raceToValue == 9) thisMatch.neitherRaceTo9CornersPrice = GetUkOddsPrice(bits.ElementAt(4)).ToString();
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error occurred");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Can't get match from " + matchText);
                        }

                    }
                }
                catch (Exception ce)
                {
                    Console.WriteLine("Exception caught: " + ce);
                }

                IJavaScriptExecutor js = driver.Driver as IJavaScriptExecutor;
                js.ExecuteScript("document.getElementById('HeaderBack').click()");

                driver.ForceSleep(m_sleepTime);
            }
        }

        private static float GetUkOddsPrice(string bit)
        {
            string price = Regex.Split(bit, "  ").Last();
            string numerator = Regex.Split(price, "/").First();
            string denominator = Regex.Split(price, "/").Last();

            return (float.Parse(numerator) / float.Parse(denominator)) + 1;
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
                        Console.WriteLine("Can't find item at index: " + i);
                        break;
                    }

                    string leagueText = genItem.Text.Trim();
                    genItem.Click();
                    driver.ForceSleep(m_sleepTime);

                    ReadInformation(driver, foundMatches, leagueText, " OVER UNDER");
                }
                catch (Exception ce)
                {
                    Console.WriteLine("Exception caught: " + ce);
                }

                IJavaScriptExecutor js = driver.Driver as IJavaScriptExecutor;
                js.ExecuteScript("document.getElementById('HeaderBack').click()");

                driver.ForceSleep(m_sleepTime);
            }
        }

        private void GetPreMatchData(DriverWrapper driver, List<aMatch> foundMatches)
        {
            var rowsCount = driver.FindElements(By.ClassName("genericRow")).Count();

            for (int i = 0; i < rowsCount; ++i)
            {
                var genItems = driver.FindElements(By.ClassName("genericRow"));

                IWebElement genItem = null;
                if (i < genItems.Count())
                {
                    genItem = genItems.ElementAt(i);

                    string leagueText = genItem.Text.Trim();
                    genItem.Click();
                    driver.ForceSleep(m_sleepTime);

                    ReadInformation(driver, foundMatches, leagueText, " 1 X 2");

                    IJavaScriptExecutor js = driver.Driver as IJavaScriptExecutor;
                    js.ExecuteScript("document.getElementById('HeaderBack').click()");

                    driver.ForceSleep(m_sleepTime);
                }
                else
                {
                    Console.WriteLine("Can't find item at index: " + i);
                    break;
                }
            }
        }

        private void ReadInformation(DriverWrapper driver, List<aMatch> foundMatches, string leagueText, string marker)
        {
            var sections = driver.FindElements(By.ClassName("Section"));
            var sectionTexts = Regex.Split(sections.First().Text, "\r\n").ToList();
            sectionTexts.RemoveAt(0);

            var sectionCount = sectionTexts.Where(x => x.Contains(marker)).Count();

            log.Debug(sectionTexts.Count());

            for (int j = 0; j < sectionCount; ++j)
            {
                var txt = Chomp(sectionTexts);

                if (txt.Contains(marker))
                {
                    DateTime koDate;

                    bool parsed = DateTime.TryParse(txt.Substring(0, txt.IndexOf(marker)), out koDate);

                    while (true)
                    {
                        if (sectionTexts.Count() == 0)
                        {
                            break;
                        }

                        if (sectionTexts.First().Contains(marker) == true)
                        {
                            break;
                        }

                        var team1 = DoSubstitutions(Chomp(sectionTexts));
                        var team2 = DoSubstitutions(Chomp(sectionTexts));

                        //search for teams already in matches list here.
                        var m = foundMatches.SingleOrDefault(x => x.team1 == team1 && x.team2 == team2);
                        
                        bool needToAdd = false;

                        if (m == null)
                        {
                            m = new aMatch();
                            needToAdd = true;
                        }

                        m.koDateTime = koDate;
                        m.league = leagueText;

                        m.team1 = team1;
                        m.team2 = team2;

                        var kodate = Chomp(sectionTexts);
                        var hours = kodate.Substring(0, 2);
                        var mins = kodate.Substring(3, 2);

                        try
                        {
                            m.koDateTime = m.koDateTime.AddHours(Double.Parse(hours));
                            m.koDateTime = m.koDateTime.AddMinutes(Double.Parse(mins));
                        }
                        catch (Exception ce)
                        {
                            Console.WriteLine("Exception trying to parse hours: [" + hours + "] + minutes [" + mins + "]");
                        }

                        //goals
                        if (marker == " 1 X 2")
                        {
                            var pricesText = Chomp(sectionTexts);
                            if (char.IsDigit(pricesText[0]))
                            {
                                var prices = pricesText.Split(' ');

                                m.homeWinPrice = GetUkOddsPrice(prices[0]).ToString();
                                m.drawPrice = GetUkOddsPrice(prices[1]).ToString();
                                m.awayWinPrice = GetUkOddsPrice(prices[2]).ToString();

                                if (needToAdd)
                                {
                                    foundMatches.Add(m);
                                }

                                Console.WriteLine("Found: " + m.team1 + " v " + m.team2 + " in " + m.league + " at " + m.koDateTime + " Prices: " + pricesText);
                            }
                            else
                            {
                                Console.WriteLine("Unrecognised price format: " + pricesText);
                            }
                        }
                        //corners
                        else 
                        {
                            var pricesText1 = Chomp(sectionTexts);
                            if (char.IsDigit(pricesText1[0]))
                            {
                                var prices = pricesText1.Split(' ');

                                m.homeAsianCornerPrice = prices[0];
                                m.cornerLine = prices[1];

                                var pricesText2 = Chomp(sectionTexts);
                                m.awayAsianCornerPrice = pricesText2;

                                if (needToAdd)
                                {
                                    foundMatches.Add(m);
                                }

                                Console.WriteLine("Found: " + m.team1 + " v " + m.team2 + " in " + m.league + " at " + m.koDateTime + " Corner Prices: " + pricesText1 + " " + pricesText2);
                            }
                            else
                            {
                                Console.WriteLine("Unrecognised price format: " + pricesText1);
                            }
                        }
                    }
                }
            }
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
    }
}
