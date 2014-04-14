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

            ids.RemoveAll(i => alreadyPredicted.Contains(i));

            foreach (var id in ids)
            {
                try
                {
                    var data = new Dictionary<string, string>();

                    var goalResp = getGoalsPredictionFromWebService(id);
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
                    }

                    foreach (var respText in cornerResps)
                    {
                        Console.WriteLine(cornerResp);
                    }
                }
                catch (Exception ce)
                {
                    Console.WriteLine("Threw exception for id: " + id);
                    Console.WriteLine(ce);
                }

            }
        }

        private string getCornersPredictionFromWebService(string id)
        {
            WebRequest req = WebRequest.Create("http://127.0.0.1:8000/GetCornersPrediction?gameId=" + id);

            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }

        private string getGoalsPredictionFromWebService(string id)
        {
            WebRequest req = WebRequest.Create("http://127.0.0.1:8000/GetGoalsPrediction?gameId=" + id);
            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }
    }
}
