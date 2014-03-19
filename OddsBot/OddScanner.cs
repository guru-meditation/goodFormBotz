using BotSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OddsBot
{
    class OddScanner
    {

        public void AddTodaysMatches(int sleepTime, DriverWrapper driver)
        {
            var foundMatches = new List<aMatch>();

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            driver.GetElementAndClick("Level1", "Match Markets");
            System.Threading.Thread.Sleep(dirtySleep);
            driver.GetElementAndClick("Level2", "Main");
            System.Threading.Thread.Sleep(dirtySleep);
            driver.GetElementAndClick("genericRow", "Full Time Result");
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
                int leagueId = dbStuff.AddLeague(m.league);
                int hTeamId = dbStuff.AddTeam(m.team1);
                int aTeamId = dbStuff.AddTeam(m.team2);
                int gameId = dbStuff.AddGame(hTeamId, aTeamId, leagueId, m.koDateTime);
                m.id = gameId;
            }

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            driver.GetElementAndClick("Level2", "Corners");
            driver.DirtySleep(sleepTime);
            driver.GetElementAndClick("genericRow", "Race To 3 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 3);

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            driver.GetElementAndClick("genericRow", "Race To 5 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 5);

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            driver.GetElementAndClick("genericRow", "Race To 7 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 7);

            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            driver.GetElementAndClick("genericRow", "Race To 9 Corners");
            driver.ForceSleep(dirtySleep);

            GetRaceToCornerData(driver, foundMatches, 9);
            /////////////////////

            driver.Url = "https://mobile.bet365.com/premium/#type=Splash;key=1;ip=0;lng=1";
            driver.DirtySleep(sleepTime);

            driver.GetElementAndClick("genericRow", "Asian Total Corners");
            System.Threading.Thread.Sleep(dirtySleep);

            driver.ForceSleep(dirtySleep);

            GetAsianCornerData(driver, foundMatches);

            /////////////////////

            foreach (aMatch m in foundMatches)
            {

                log.Debug(m.team1.PadRight(longestTeam1 + 1) + " " + m.team2.PadRight(longestTeam2 + 1) + " corner line: " + m.cornerLine + " " + m.homeAsianCornerPrice + "//" + m.awayAsianCornerPrice);

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

            for (int i = 0; i < rowsCount; ++i)
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
                                m.koDateTime = DateTime.ParseExact(sectionTexts.ElementAt(1).Substring(0, 12), "dd MMM HH:mm", CultureInfo.InvariantCulture);
                            }
                            catch (Exception ce)
                            {
                                log.Error("Couldn't parse a date out of :" + sectionTexts.ElementAt(1));

                            }

                            m.homeWinPrice = GetUkOddsPrice(sectionTexts.ElementAt(3)).ToString();
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

    }
}
