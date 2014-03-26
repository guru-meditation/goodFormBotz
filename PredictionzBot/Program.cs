using Db;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace PredictionzBot
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string site = ConfigurationManager.AppSettings["site"];
        static string connectionString = ConfigurationManager.AppSettings["connection1"];
        static string dbtype = ConfigurationManager.AppSettings["dbtype"];
        static string sleepTime = ConfigurationManager.AppSettings["sleeptime"];

        static void Main(string[] args)
        {
            Console.WriteLine("Connection string           : " + connectionString);
            Console.WriteLine("Database Type               : " + dbtype);
            Console.WriteLine("Sleep Time                  : " + sleepTime);
            Console.WriteLine(" ");

            int sleep = 2000;

            int.TryParse(sleepTime, out sleep);

            Database dbStuff = new Database(dbtype, connectionString, gOpMode);

            while (dbStuff.Connect() == false)
            {
                log.Warn("Cannot connect to DB... retrying in 10 seconds");
                System.Threading.Thread.Sleep(10000);
            }

            var gen = new PredictionsGenerator(dbStuff);
            gen.Go();
        }
    }
}
