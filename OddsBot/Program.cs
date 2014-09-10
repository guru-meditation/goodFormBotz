using Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using BotSpace;

namespace OddsBot
{
    public enum OperationMode
    {
        WilliamHillScan,
        Bet365Scan,
        UploadWilliamHill,
        UploadBet365
    }

    public class aMatch
    {
        public int id;
        public string team1;
        public string team2;
        public string league;
        public string cornerLine;
        public string homeAsianCornerPrice;
        public string awayAsianCornerPrice;
        public string homeRaceTo3CornersPrice;
        public string awayRaceTo3CornersPrice;
        public string neitherRaceTo3CornersPrice;
        public string homeRaceTo5CornersPrice;
        public string awayRaceTo5CornersPrice;
        public string neitherRaceTo5CornersPrice;
        public string homeRaceTo7CornersPrice;
        public string awayRaceTo7CornersPrice;
        public string neitherRaceTo7CornersPrice;
        public string homeRaceTo9CornersPrice;
        public string awayRaceTo9CornersPrice;
        public string neitherRaceTo9CornersPrice;
        public string homeWinPrice;
        public string drawPrice;
        public string awayWinPrice;

        public DateTime koDateTime;

        public override string ToString()
        {
            return team1 + " v " + team2 + " at " + koDateTime + " in " + league;
        }
    }

    class Program
    {
        private static readonly log4net.ILog log
           = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static OperationMode gOpMode = OperationMode.Bet365Scan;
        static string site = ConfigurationManager.AppSettings["site"];
        static string connectionString = ConfigurationManager.AppSettings["connection1"];
        static string dbtype = ConfigurationManager.AppSettings["dbtype"];
        static string xmlPath = ConfigurationManager.AppSettings["xmlPath"];
        static string sleepTime = ConfigurationManager.AppSettings["sleeptime"];

        static void Main(string[] args)
        {

            int r = 0;

            bool phantomMode = false;

            gOpMode = OperationMode.Bet365Scan;

            foreach (string arg in args)
            {
                Console.WriteLine("args[" + r + "] " + arg);

                if (arg.ToLower().Contains("-p:"))
                {
                    xmlPath = arg.Substring("-p:".Length);
                }

                ++r;
            }

            Console.WriteLine("Bot starting, scanning site : " + gOpMode);
            Console.WriteLine("Connection string           : " + connectionString);
            Console.WriteLine("Database Type               : " + dbtype);
            Console.WriteLine("XML Path                    : " + xmlPath);
            Console.WriteLine("Sleep Time                  : " + sleepTime);
            Console.WriteLine(" ");

            int sleep = 2000;

            int.TryParse(sleepTime, out sleep);

            if (Directory.Exists(xmlPath) == false)
            {
                log.Error("Directory " + xmlPath + " does not exist :(");
                return;
            }

            DriverCreator driverCreator = null;

            if (phantomMode)
            {
                driverCreator = new PhantomDriverCreator();
            }
            else
            {
                driverCreator = new ChromeDriverCreator();
            }

            Database dbStuff = new Database(DbCreator.Create(dbtype));

            while (dbStuff.Connect(connectionString) == false)
            {
                log.Warn("Cannot connect to DB... retrying in 10 seconds");
                System.Threading.Thread.Sleep(10000);
            }

            string agentString = "--user-agent=\"Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533/1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1\"";

            using (var driverWrapper = driverCreator.CreateDriver(agentString))
            {

                if (driverWrapper == null)
                {
                    log.Error("Failed to make a Selenium Driver");
                }

                var scanner = new OddScanner(dbStuff);
                scanner.AddTodaysMatches(2000, driverWrapper);
            }
        }
    }
}
