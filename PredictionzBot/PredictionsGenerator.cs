using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PredictionzBot
{
    class PredictionsGenerator
    {
        private Db.Database m_dbStuff;
        private int m_mode = 0;
        private string m_serviceAddress; 

        public PredictionsGenerator(Db.Database dbStuff, string mymode, string serviceAddress)
        {
            m_mode              = mymode == "deep" ? 1 : 0;
            m_serviceAddress    = serviceAddress;
            m_dbStuff           = dbStuff;

            Console.WriteLine("PredictionsGenerator running in " + mymode + "  mode");
        }

        internal void Go()
        {
            var ids = m_dbStuff.GetADaysGames(DateTime.Today);
            ids.AddRange(m_dbStuff.GetADaysGames(DateTime.Today + TimeSpan.FromDays(1)));

            //var ids = m_dbStuff.GetFutureGamesWithExceptions( new List<string>() { "1202728" });
            var alreadyPredicted = m_dbStuff.GetIdsFromPredictionTable();

            int goodOnes = 0, badOnes = 0;
            Console.WriteLine("Removing " + ids.RemoveAll(i => alreadyPredicted.Contains(i)) + " games");

            foreach (var id in ids)
            {
                string gameDetails = m_dbStuff.GetGameDetails(id);
                try
                {
                    Console.WriteLine("Good ones: " + goodOnes + " Bad ones " + badOnes + " Total: " + ids.Count());
                    Console.WriteLine(gameDetails);

                    var data = new Dictionary<string, string>();

                    bool allIsWell = true;

                    var goalResp = "";
                    bool httpGetOkay = false;


                    while (httpGetOkay == false)
                    {
                        try
                        {
                            if (m_mode == 0)
                            {
                                goalResp = getGoalsPredictionFromWebService(id);
                            }
                            else
                            {
                                goalResp = getDeepGoalsPredictionFromWebService(id);
                            }

                            httpGetOkay = true;
                        }
                        catch (Exception ce)
                        {
                            Console.WriteLine("HTTP GET Failed. Retrying... [" + ce.GetType() + "]");
                        }
                    }


                    Console.WriteLine("Goal Reponse:");
                    Console.WriteLine(goalResp);

                    allIsWell = ProcessGoalResponse(data, allIsWell, goalResp);

                    if (allIsWell == false)
                    {
                        badOnes++;
                        continue;
                    }

                    var cornerResp = "";
                    httpGetOkay = false;


                    while (httpGetOkay == false)
                    {
                        try
                        {
                            if (m_mode == 0)
                            {
                                cornerResp = getCornersPredictionFromWebService(id);
                            }
                            else
                            {
                                cornerResp = getDeepCornerPredictionFromWebService(id);
                            }

                            httpGetOkay = true;
                        }
                        catch (Exception ce)
                        {
                            Console.WriteLine("HTTP GET Failed. Retrying... [" + ce.GetType() + "]");
                        }
                    }

                    allIsWell = ProcessCornerResponse(id, data, cornerResp);


                    if (allIsWell == false)
                    {
                        badOnes++;
                    }
                    else
                    {
                        goodOnes++;
                    }
                }
                catch (Exception ce)
                {
                    badOnes++;
                    Console.WriteLine("T++++++++++++++++++++++++++++++++++");
                    Console.WriteLine("Thrown exception for id: " + id);
                    Console.WriteLine("T++++++++++++++++++++++++++++++++++");
                    Console.WriteLine(ce);
                }
            }
        }

        private static bool ProcessGoalResponse(Dictionary<string, string> data, bool allIsWell, string goalResp)
        {
            var goalResps = Regex.Split(goalResp, "&#xD").ToList();

            if (goalResps.Count() == 7)
            {
                goalResps.RemoveAt(0);
                goalResps.RemoveAt(goalResps.Count() - 1);

                var moddedGoalResps = new List<string>();
                goalResps.ForEach(x =>
                    moddedGoalResps.Add(x.Split('"').ElementAt(3).Replace(',', '.'))
                );

                data["goalsWinHome"] = moddedGoalResps.ElementAt(1);
                data["goalsWinAway"] = moddedGoalResps.ElementAt(2);
                data["goalsLikelyScoreHome"] = moddedGoalResps.ElementAt(3).Split(' ').First();
                data["goalsLikelyScoreAway"] = moddedGoalResps.ElementAt(3).Split(' ').Last(); ;
                data["goalsLikelyProbability"] = moddedGoalResps.ElementAt(4);
            }
            else
            {
                allIsWell = false;
            }
            return allIsWell;
        }

        private bool ProcessCornerResponse(string id, Dictionary<string, string> data, string cornerResp)
        {
            var cornerResps = Regex.Split(cornerResp, "&#xD").ToList();

            Console.WriteLine("Corner Reponse:");
            Console.WriteLine(cornerResp);

            bool allIsWell = true;

            if (cornerResps.Count() == 7)
            {
                cornerResps.RemoveAt(0);
                cornerResps.RemoveAt(cornerResps.Count() - 1);

                var moddedCornerResps = new List<string>();
                cornerResps.ForEach(x =>
                    moddedCornerResps.Add(x.Split('"').ElementAt(3).Replace(',', '.'))
                );

                data["cornersWinHome"] = moddedCornerResps.ElementAt(1);
                data["cornersWinAway"] = moddedCornerResps.ElementAt(2);
                data["cornersLikelyScoreHome"] = moddedCornerResps.ElementAt(3).Split(' ').First();
                data["cornersLikelyScoreAway"] = moddedCornerResps.ElementAt(3).Split(' ').Last(); ;
                data["cornersLikelyProbability"] = moddedCornerResps.ElementAt(4);

                m_dbStuff.AddPredictionsData(id, data);

                Console.WriteLine("Completed OKAY!!");
            }
            else
            {
                Console.WriteLine("Failed ===========> Trying shallower prediction!!");
                allIsWell = false;
            }
            return allIsWell;
        }

        private string getCornersPredictionFromWebService(string id)
        {
            Console.WriteLine("getCornersPredictionFromWebService =======>");

            WebRequest req = WebRequest.Create("http://" + m_serviceAddress + "/GetCornersPrediction?gameId=" + id);
            req.Timeout = Timeout.Infinite;

            WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }

        private string getGoalsPredictionFromWebService(string id)
        {
            Console.WriteLine("getGoalsPredictionFromWebService =======>");

            WebRequest req = WebRequest.Create("http://" + m_serviceAddress + "/GetGoalsPrediction?gameId=" + id);
            req.Timeout = Timeout.Infinite;

            WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }

        private string getDeepGoalsPredictionFromWebService(string id)
        {
            Console.WriteLine("getDeepGoalsPredictionFromWebService =======>");

            WebRequest req = WebRequest.Create("http://" + m_serviceAddress + "/GetGoalsPredictionWithDepth?gameId=" + id + "&depth=500");
            req.Timeout = Timeout.Infinite;

            WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }

        private string getDeepCornerPredictionFromWebService(string id)
        {
            Console.WriteLine("getDeepCornerPredictionFromWebService =======>");

            WebRequest req = WebRequest.Create("http://" + m_serviceAddress + "/GetCornersPredictionWithDepth?gameId=" + id + "&depth=500");
            req.Timeout = Timeout.Infinite;

            WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }
    }
}
