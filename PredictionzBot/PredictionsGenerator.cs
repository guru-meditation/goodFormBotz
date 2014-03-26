using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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
           var ids = m_dbStuff.GetGamesForThisDay(DateTime.Today);
           foreach (var id in ids)
           {
               var resp = getPredictionFromWebService(id);
               Console.WriteLine(resp);
           }
        }

        private string getPredictionFromWebService(string id)
        {
            WebRequest req = WebRequest.Create("http://127.0.0.1:8000/GetCornersPrediction?gameId=" + id);
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            return sr.ReadToEnd().Trim();
        }
    }
}
