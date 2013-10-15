using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace BotSpace
{
    public class WebDownloader : WebClient
    {
        private int _timeout;
        /// <summary>
        /// Time in milliseconds
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public WebDownloader(int timeout)
        {
            this._timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = this._timeout;
            return result;
        }
    }

    class ScoreBoardFinder
    {
        WebClient client = null;
        List<WebBlob> _webBlobStore = null;

        public ScoreBoardFinder(int timeout = 3000)
        {
            _webBlobStore = new List<WebBlob>();

            if (client == null)
            {
                client = new WebDownloader(timeout);
            }
        }

        public void Start()
        {
            Thread newThread = new Thread(new ThreadStart(Run));
            newThread.Start();
        }

        public void Run()
        {
            while (true)
            {
                string downloadString = GetMainPage();

                string hRefPattern = "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";
                var urls = RegexGet(downloadString, hRefPattern).Where(x => x.Contains("%2dv%2d")).Distinct().Select(x => Regex.Replace(x, "%2d", "-"));

                //get home and away teams
                var hts = new List<string>();
                var ats = new List<string>();

                Console.WriteLine("Found " + urls.Count() + " URLs at: " + DateTime.Now);

                if(urls.Count() == 0)
                {
                    Console.WriteLine("Sleeping for a bit....");
                    Thread.Sleep(10000);
                }

                foreach (var url in urls)
                {
                    string htmlFileName = Path.GetFileNameWithoutExtension(url);
                    htmlFileName = htmlFileName.Replace("-", " ");

                    var teams = Regex.Split(htmlFileName, " v ");

                    hts.Add(teams[0]);
                    ats.Add(teams[1]);
                }

                //remove those that are not being played anymore
                _webBlobStore.RemoveAll(x => hts.Any(y => y == x.HomeTeam) == false);

                foreach (var url in urls)
                {
                    string htmlFileName = Path.GetFileNameWithoutExtension(url);
                    htmlFileName = htmlFileName.Replace("-", " ");

                    var teams = Regex.Split(htmlFileName, " v ");

                    string homeTeamName = teams[0];
                    string awayTeamName = teams[1];

                    try
                    {
                        downloadString = client.DownloadString(url);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Http RAAAAAAAAAAAR");
                        continue;
                    }

                    string league = GetLeague(downloadString);

                    if (league != "BAD_LEAGUE")
                    {

                        string iFramePattern = "iframe id=\"scoreboard_frame\" class=\"scoreboardCollapsed\" src\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";
                        var iFrames = RegexGet(downloadString, iFramePattern);

                        if (iFrames.Count() == 0)
                        {
                            //Console.WriteLine("No game iframe found for " + homeTeamName + " v " + awayTeamName);
                            continue;
                        }
                        else
                        {
                            if (_webBlobStore.Any(x => x.HomeTeam == homeTeamName && x.AwayTeam == awayTeamName) == false)
                            {
                                WebBlob wb = new WebBlob();
                                wb.HomeTeam = homeTeamName;
                                wb.AwayTeam = awayTeamName;
                                wb.League = league;
                                wb.Url = iFrames[0];
                                _webBlobStore.Add(wb);
                            }
                        }
                    }
                }
            }
        }

        private string GetLeague(string url)
        {
            string league = "Unknown";

            string iLeaguePattern = "<meta name=\"description\" content\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

            var iLeague = RegexGet(url, iLeaguePattern);

            if (iLeague.Count() == 0)
            {
                Console.WriteLine("No league content found");
                return "BAD_LEAGUE";
            }

            string content = iLeague[0];

            int startLeague = content.IndexOf(" in ") + " in ".Length;
            int endLeague = content.IndexOf(" - ");


            try
            {
                league = content.Substring(startLeague, endLeague - startLeague);
            }
            catch (Exception)
            {
                Console.WriteLine(" =========> No league in [" + content + "]");
            }

            return league;
        }

        public WebBlob[] GetWebBlobs()
        {
            return _webBlobStore.ToArray();
        }

        private string GetMainPage()
        {
            string downloadString = "";

            try
            {
                downloadString = client.DownloadString(TheBot.willHillUrl);
            }
            catch (Exception)
            {
                Console.WriteLine("Http RAAAAAAAAAAAR");
            }

            return downloadString;
        }

        public static List<string> RegexGet(string inputString, string hRefPattern)
        {
            List<string> hrefs = new List<string>();
            Match m;

            try
            {
                m = Regex.Match(inputString, hRefPattern);

                while (m.Success)
                {
                    hrefs.Add(m.Groups[1].ToString());
                    m = m.NextMatch();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("The matching operation timed out.");
            }

            return hrefs;
        }
    }
}
