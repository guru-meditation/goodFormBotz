﻿using Db;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace PredictionzBot
{
    public enum OperationMode
    {
        WilliamHillScan,
        Bet365Scan,
        UploadWilliamHill,
        UploadBet365
    }

    class Program
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string site                  = ConfigurationManager.AppSettings["site"];
        static string connectionString      = ConfigurationManager.AppSettings["connection1"];
        static string dbtype                = ConfigurationManager.AppSettings["dbtype"];
        static string sleepTime             = ConfigurationManager.AppSettings["sleeptime"];
        static string mode                  = ConfigurationManager.AppSettings["mode"];
        static string predictionServiceHost = ConfigurationManager.AppSettings["predictionservicehost"];
        static string predictionServicePort = ConfigurationManager.AppSettings["predictionserviceport"];

        static void Main(string[] args)
        {
            log.Info("Connection string           : " + connectionString);
            Console.WriteLine("Database Type               : " + dbtype);
            Console.WriteLine("Sleep Time                  : " + sleepTime);
            Console.WriteLine("Service Host                : " + predictionServiceHost + ":" + predictionServicePort);
            Console.WriteLine(" ");

            int sleep = 2000;

            int.TryParse(sleepTime, out sleep);
            
            Database dbStuff = new Database(DbCreator.Create(dbtype));

            int cnt = 0;
            int maxWait = 10;

            while (true)
            {
                try
                {
                    dbStuff.Connect(connectionString);
                    break;
                }
                catch (Exception e)
                {
                    log.Error(e);
                    log.Warn("Cannot connect to DB... retrying in 10 seconds");
                    if (cnt++ < maxWait)
                    {
                        System.Threading.Thread.Sleep(10000);
                    }
                    else
                    {
                        break;
                    }
                }
            }


            var gen = new PredictionsGenerator(dbStuff, mode, predictionServiceHost + ":" + predictionServicePort);
            gen.Go();
        }
    }
}
