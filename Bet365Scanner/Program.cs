using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
/*
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Newtonsoft.Json;
*/
namespace BotSpace
{
    using Scanners;
    using Db;
    using WebDriver;
    using System.Collections;

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

    public enum OperationMode
    {
        WilliamHillScan,
        Bet365Scan,
        UploadWilliamHill,
        UploadBet365
    }
    /*
    public class GlobalData
    {
        private static GlobalData instance;
        public Database dbStuff { get; set; }

        private GlobalData() { }

        public static GlobalData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalData();
                }
                return instance;
            }
        }
    }

    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        [WebGet]
        string GetGoalsAndCornersPred(string game_id);

        [OperationContract]
        [WebInvoke]
        string EchoWithPost(string s);
    }

    public class PredRow
    {
        public string RowId  { get; set; }
    };

    public class GameResults
    {
        public string homeTeam;
        public string awayTeam;
        public string homeGoals;
        public string awayGoals;
        public string homeCorners;
        public string awayCorners;
    };

    public class Service : IService
    {
        private static readonly log4net.ILog log
           = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Database dbStuff;
        Service()
        {
            GlobalData gd = GlobalData.Instance;
            dbStuff = gd.dbStuff;
        }
       
        public string GetGoalsAndCornersPred(string game_id)
        {
            string league_id = null;

            //get league id
            dbStuff.RunSQL("SELECT league_id FROM games WHERE id = " + game_id + ";",
                (dr) =>
                {
                    league_id = dr[0].ToString();
                }
            );

            ArrayList results = new ArrayList();

            //get goals, corners from all games in a league_id
            dbStuff.RunSQL("select t1.name, t2.name, max(s.hg), max(s.ag), max(s.hco), max(s.aco)"
            + " from statistics s, games g, teams t1, teams t2"
            + " where g.league_id = "
            + league_id
            + " and s.game_id = g.id and t1.id = g.team1 and t2.id = g.team2 group by s.game_id;",
                (dr) =>
                {
                    GameResults res = new GameResults();
                    res.homeTeam = dr[0].ToString();
                    res.awayTeam = dr[1].ToString();
                    res.homeGoals = dr[2].ToString();
                    res.awayGoals = dr[3].ToString();
                    res.homeCorners = dr[4].ToString();
                    res.awayCorners = dr[4].ToString();
                    results.Add(res);
                }
            );

            log.Debug("Number of games : " + results.Count);

            PredRow row = new PredRow();
            row.RowId = league_id;

            return null; // JsonConvert.SerializeObject(row, Formatting.Indented);
        }

        public string EchoWithPost(string s)
        {
            return "You said " + s;
        }
    }
    */
    public class TheBot
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string willHillUrl = "http://sports.williamhill.com/bet/en-gb/betlive/9";

        static string site              = ConfigurationManager.AppSettings["site"];
        static string connectionString  = ConfigurationManager.AppSettings["connection1"];
        static string dbtype            = ConfigurationManager.AppSettings["dbtype"];
        static string xmlPath           = ConfigurationManager.AppSettings["xmlPath"];
        static string sleepTime         = ConfigurationManager.AppSettings["sleeptime"];

        static OperationMode gOpMode = OperationMode.Bet365Scan;
        static int numBots = 1;
  
        static bool gSkipAddGames = false;

        static void Main(string[] args)
        {
            int r = 0;

            bool phantomMode = false;

          
            foreach (string arg in args)
            {
                log.Info("args[" + r + "] " + arg);

                if (arg.ToLower().Contains("-m:willhill"))
                {
                    gOpMode = OperationMode.WilliamHillScan;
                }

                if (arg.ToLower().Contains("-m:bet365"))
                {
                    gOpMode = OperationMode.Bet365Scan;
                }

                if (arg.ToLower().Contains("-n:"))
                {
                    try
                    {
                        numBots = int.Parse(arg.Substring("-n:".Length));
                    }
                    catch
                    {
                       log.Warn("Couldn't parse number of bots: " + arg);
                    }
                }

                if (arg.ToLower().Contains("-m:uploadwh"))
                {
                    gOpMode = OperationMode.UploadWilliamHill;
                }

                if (arg.ToLower().Contains("-m:upload365"))
                {
                    gOpMode = OperationMode.UploadBet365;
                }

                if (arg.ToLower().Contains("-p:"))
                {
                    xmlPath = arg.Substring("-p:".Length);
                }

                if (arg.ToLower().Contains("-m:skip"))
                {
                    gSkipAddGames = true;
                }

                if (arg.ToLower().Contains("-m:scratch"))
                {
                    ScratchPad();
                    return;
                }

                if (arg.ToLower().Contains("-m:phantom"))
                {
                    phantomMode = true;
                }

                ++r;
            }
            
            log.Info("Bot starting, scanning site : " + gOpMode);
            log.Info("Connection string           : " + connectionString);
            log.Info("Database Type               : " + dbtype);
            log.Info("XML Path                    : " + xmlPath);
            log.Info("Sleep Time                  : " + sleepTime);
            log.Info(" ");

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
                driverCreator = new PhantomDriverCreatorCreatorWait();
            }
            else
            {
                driverCreator = new ChromeDriverCreatorWait();
            }

            Database dbStuff = new Database(dbtype, connectionString, gOpMode);

            //GlobalData gd = GlobalData.Instance;
            //gd.dbStuff = dbStuff;

            while (dbStuff.Connect() == false)
            {
                log.Warn("Cannot connect to DB... retrying in 10 seconds");
                System.Threading.Thread.Sleep(10000);
            }


            /*
             * WebServiceHost host = new WebServiceHost(typeof(Service), new Uri("http://localhost:8000/"));

            try
            {
                ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
                host.Open();
                using (ChannelFactory<IService> cf = new ChannelFactory<IService>(new WebHttpBinding(), "http://localhost:8000"))
                {
                    cf.Endpoint.Behaviors.Add(new WebHttpBehavior());

                    IService channel = cf.CreateChannel();
                }
            }
            catch (CommunicationException cex)
            {
                Console.WriteLine("An exception occurred: {0}", cex.Message);
                host.Abort();
            }
            */

            Scanner scanner = null;

            switch (gOpMode)
            {
                case OperationMode.Bet365Scan:
                    scanner = new ScanBet365(driverCreator, dbStuff, xmlPath, gSkipAddGames);
                    break;
                case OperationMode.WilliamHillScan:
                    scanner = new ScanWilliamHill(driverCreator, dbStuff, xmlPath, gSkipAddGames);
                    break;
                case OperationMode.UploadBet365:
                    scanner = new UploadBet365(driverCreator, dbStuff, xmlPath, gSkipAddGames);
                    break;
                default:
                    scanner = new UpdateFromXmlToWeb(driverCreator, dbStuff, xmlPath, gSkipAddGames);
                    break;

            }

            scanner.scan(sleep);
            //host.Close();
        }     
     
        class Game
        {
            public string teamA;
            public string teamB;
            public int hgoals;
            public int agoals;
            public int hCorners;
            public int aCorners;
        }


        private static void ScratchPad()
        {
            var games = new List<Game>();
            string teamA = "Manisaspor Reserves";
            string teamB = "Kasimpasa Reserves";
            try
            {
                string path = "C:\\Users\\user\\Google Drive\\WillHillData_Processed\\Turkish A2 Ligi";
                var xmls = Directory.GetFiles(path);

                System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\users\\user\\TurkeyA2.txt");
                file.WriteLine("HomeTeam,AwayTeam,HomeGoals,AwayGoals,HomeCorners,AwayCorners");

                foreach (var xml in xmls)
                {
                    XDocument xdoc = null;
                    try
                    {

                        xdoc = XDocument.Load(xml);
                    }
                    catch (Exception )
                    {
                        log.Error("Exception: " + xml);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(xml);

                    var splits = Regex.Split(fileName, " v ");
                    string homeTeam = splits.ElementAt(0);
                    string awayTeam = splits.ElementAt(1).Substring(0, splits.ElementAt(1).IndexOf("_"));

                    var game = xdoc.Descendants("Game");
                    string koDateString = xml.Substring(xml.Length - 10, 6);
                    DateTime koDate = DateTime.ParseExact(koDateString, "ddMMyy", null);

                    var snaps = game.Descendants("Snap");

                    //foreach (var snap in snaps)
                    var snap = snaps.Last();


                    string time = snap.Attribute("Time").Value;

                    //<Snap Time="Full Time" Seen="01/27/13 5:30:04">
                    //<Snap Time="Half Time" Seen="01/27/13 4:23:48">
                    //<Snap Time="90:19" Seen="01/27/13 5:23:57">
                    string seen = snap.Attribute("Seen").Value;
                    string[] bits = Regex.Split(seen, " ");
                    string[] dates = bits[0].Split('/');
                    string[] times = bits[1].Split(':');

                    DateTime seenTime = new DateTime(int.Parse(dates[2]) + 2000, int.Parse(dates[0]), int.Parse(dates[1]), int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]));

                    var hG = int.Parse(snap.Descendants("Home").First().Descendants("Goals").First().Value);
                    var aG = int.Parse(snap.Descendants("Away").First().Descendants("Goals").First().Value);
                    var hC = int.Parse(snap.Descendants("Home").First().Descendants("Corners").First().Value);
                    var aC = int.Parse(snap.Descendants("Away").First().Descendants("Corners").First().Value);



                    string line = String.Format("{0},{1},{2},{3},{4},{5}", homeTeam, awayTeam, hG, aG, hC, aC);

                    file.WriteLine(line);

                    games.Add(new Game() { teamA = homeTeam, teamB = awayTeam, hCorners = hC, aCorners = aC, agoals = aG, hgoals = hG });

                }
                file.Close();
            }
            catch (Exception e)
            {
                log.Error("Exception: " + e);
            }
            finally
            {
                log.Debug("Finally");
            }

            var rematchesHome = games.Where(x => x.teamA == teamA && x.teamB == teamB);
            var rematchesAway = games.Where(x => x.teamB == teamA && x.teamA == teamB);

            var gamesWhereTeamAIsHome = games.Where(x => x.teamA == teamA);
            var gamesWhereTeamAIsAway = games.Where(x => x.teamA == teamA);
            var gamesWhereTeamBIsHome = games.Where(x => x.teamA == teamB);
            var gamesWhereTeamBIsAway = games.Where(x => x.teamB == teamB);

            foreach (var g in gamesWhereTeamAIsHome)
            {
                string thisAwayTeam = g.teamB;

            }
        }
    }
}
