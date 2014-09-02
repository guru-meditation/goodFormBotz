using Db;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace BestBet
{
    class aMatch
    {
        public string id;
        public string homeWinPrice;
        public string drawPrice;
        public string awayWinPrice;
    }

    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static OperationMode gOpMode = OperationMode.Bet365Scan;
        static string site = ConfigurationManager.AppSettings["site"];
        static string connectionString = ConfigurationManager.AppSettings["connection1"];
        static string dbtype = ConfigurationManager.AppSettings["dbtype"];
        static string sleepTime = ConfigurationManager.AppSettings["sleeptime"];

        static void Main(string[] args)
        {
            log.Info("Connection string           : " + connectionString);
            log.Info("Database Type               : " + dbtype);
            log.Info("Sleep Time                  : " + sleepTime);
            log.Info(" ");
            log.Info("Ids:");
            int sleep = 2000;

            int.TryParse(sleepTime, out sleep);

            Database dbStuff = new Database(dbtype, connectionString, gOpMode);

            while (dbStuff.Connect() == false)
            {
                log.Warn("Cannot connect to DB... retrying in 10 seconds");
                System.Threading.Thread.Sleep(10000);
            }

            List<aMatch> matches = new List<aMatch>();

            dbStuff.RunSQL("SELECT id FROM games"
                         + " WHERE date_part('DAY', kodate) = date_part('DAY', now())"
                         + " AND date_part('month', kodate) = date_part('month', now())"
                         + " AND date_part('year', kodate) = date_part('year', now())"
                         // + "kodate > now()"
                         ,
                (dr) =>
                {
                    var match = new aMatch();
                    match.id = dr[0].ToString();
                    matches.Add(match);
                }
                );

            //134510, 134511

            

            log.Info("Getting odds: ");
            foreach (aMatch match in matches) {
             log.Info("match: " + match.id);
             dbStuff.RunSQL("SELECT homeprice, drawprice, awayprice"
                   + " FROM fulltimeprice"
                   + " WHERE game_id = '" + match.id + "';"
                   ,
                   (dr) =>
                   {
                       match.homeWinPrice = dr[0].ToString();
                       match.drawPrice = dr[1].ToString();
                       match.awayWinPrice = dr[2].ToString();
                   }
             );
             log.Info("ods for match: homeWin: " + match.homeWinPrice);
            }
        }
    }
}
