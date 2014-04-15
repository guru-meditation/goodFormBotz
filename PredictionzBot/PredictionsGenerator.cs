using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace PredictionzBot
{
    class PredictionsGenerator
    {
        private Db.Database m_dbStuff;

        public PredictionsGenerator(Db.Database dbStuff)
        {

            this.m_dbStuff = dbStuff;
        }

        internal void Go()
        {
            var ids = m_dbStuff.GetFurureGames();
            var alreadyPredicted = m_dbStuff.GetIdsFromPredictionTable();

            int goodOnes = 0, badOnes = 0;
            Console.WriteLine("Removing " + alreadyPredicted.Count() + " games");

            ids.RemoveAll(i => alreadyPredicted.Contains(i));

            foreach (var id in ids)
            {
                string gameDetails = m_dbStuff.GetGameDetails(id);
                try
                {
                    Console.WriteLine("Good ones: " + goodOnes + " Bad ones " + badOnes + " Total: " + ids.Count());
                    Console.WriteLine(gameDetails);

                    var data = new Dictionary<string, string>();

                    var goalResp = getGoalsPredictionFromWebService(id);

                    Console.WriteLine("Goal Reponse:");
                    Console.WriteLine(goalResp);

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

                    var cornerResp = getCornersPredictionFromWebService(id);
                    var cornerResps = Regex.Split(cornerResp, "&#xD").ToList();

                    Console.WriteLine("Corner Reponse:");
                    Console.WriteLine(cornerResp);

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
                        goodOnes++;
                    }

                    foreach (var respText in cornerResps)
                    {
                        Console.WriteLine(cornerResp);
                    }
                }
                catch (Exception ce)
                {
                    badOnes++;
                    Console.WriteLine("T++++++++++++++++++++++++++++++++++");
                    Console.WriteLine("Thrown exception for id: " + id);
                    Console.WriteLine("T++++++++++++++++++++++++++++++++++");
                    Console.WriteLine(ce);

                    System.IO.StreamWriter file = new System.IO.StreamWriter("Fails.txt", true);
                    file.WriteLine(gameDetails);

                    file.Close();
                }

            }
        }

        private string getCornersPredictionFromWebService(string id)
        {
            WebRequest req = WebRequest.Create("http://127.0.0.1:8000/GetCornersPrediction?gameId=" + id);

            req.Timeout = 3000000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }

        private string getGoalsPredictionFromWebService(string id)
        {
            WebRequest req = WebRequest.Create("http://127.0.0.1:8000/GetGoalsPrediction?gameId=" + id);
            req.Timeout = 3000000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }
    }
}
