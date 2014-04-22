using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Db
{
    using Npgsql;
    using NpgsqlTypes;
    using System.Data;
    
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


    public class Database
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DbCreator dbCreator = null;
        private List<DbConnection> dbConnectionList = new List<DbConnection>();
        
        private OperationMode m_opMode;

        public Database(string dbtype, string connectionString, OperationMode operationMode)
        {
            m_dbConnectionString = connectionString;
            m_opMode = operationMode;

            switch (dbtype)
            {
                case "pg":
                    dbCreator = new NpgsqlCreator();
                    break;
                case "sqlite":
                    dbCreator = new SQLiteCreator();
                    break;
                default:
                    dbCreator = new SQLiteCreator();
                    break;
            } 
        }

        public bool Connect()
        {
            bool retVal = false;

            DbConnection connection = null;
            
            try
            {
                    connection = dbCreator.newConnection(m_dbConnectionString);

                    if (connection != null)
                    {
                        connection.Open();
                        
                        retVal = true;
                    }
            }
            catch (Exception ex)
            {
                log.Error("Creating new connection " + ex.ToString());

                if (connection != null)
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                    {
                        log.Debug("Closing DB Connection...");
                        connection.Close();
                    }
                   
                    connection.Dispose();
                }
            }

            if(retVal) dbConnectionList.Add(connection);

            return retVal;
        }


        public void RunSQL(string sql, Action<DbDataReader> a)
        {
            using (DbCommand cmd = dbCreator.newCommand(sql, dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())  //bug fix for repeated same game added after rematch
                    {
                       a(dr);  
                    }

                    dr.Close();
                }
            }
        }

        public int AddTeam(string team)
        {
            int idx = -1;
            bool hasRows = false;

            using (DbCommand findInTeamsTable = dbCreator.newCommand("SELECT id, name FROM teams WHERE name = '" + team + "';", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = findInTeamsTable.ExecuteReader())
                {
                    hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        string id = dr[0].ToString();
                        string name = dr[1].ToString();

                        if (name == team)
                        {
                            idx = int.Parse(id);
                        }
                    }
                    dr.Close();
                }
            }

            if (hasRows == false)
            {
                //see if it exists in the team_associations
                using (DbCommand findInTeamsTable = dbCreator.newCommand("SELECT team_id, name FROM team_associations WHERE name = '" + team + "';", dbConnectionList.ElementAt(0)))
                {
                    using (DbDataReader dr = findInTeamsTable.ExecuteReader())
                    {
                        hasRows = dr.HasRows;

                        if (hasRows == true)
                        {
                            dr.Read();
                            string id = dr[0].ToString();
                            string name = dr[1].ToString();

                            if (name == team)
                            {
                                idx = int.Parse(id);
                            }
                        }

                        dr.Close();
                    }
                }
            }

            if (hasRows == false)
            {
                using (DbCommand count = dbCreator.newCommand("select max(id) from teams", dbConnectionList.ElementAt(0)))
                {
                    using (DbDataReader dr2 = count.ExecuteReader())
                    {
                        dr2.Read();

                        try
                        {
                            int rows = int.Parse(dr2[0].ToString());
                            idx = rows + 1;
                        }
                        catch (Exception)
                        {
                            idx = 1;
                        }

                        dr2.Close();

                        string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

                        using (DbCommand insert = dbCreator.newCommand("INSERT into teams ( id, name, created_at, updated_at ) VALUES (" + idx + ", '" + team + "', '" + now + "', '" + now + "');", dbConnectionList.ElementAt(0)))
                        {
                            insert.ExecuteNonQuery();
                        }
                    }
                }
            }
            return idx;
        }

        public int AddGame(int homeTeamId, int awayTeamId, int leagueId, DateTime koDate)
        {
            int idx = -1;
            int thisLeagueId = -2;

            using (DbCommand find = dbCreator.newCommand("SELECT id, team1, kodate, league_id FROM games WHERE team1 = '" + homeTeamId + "' AND team2 = '" + awayTeamId + "';", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    while (dr.Read())  //bug fix for repeated same game added after rematch
                    {
                        string id           = dr[0].ToString();
                        int thisHomeTeam    = int.Parse(dr[1].ToString());
                        string thisKoDate   = dr[2].ToString();
                        thisLeagueId        = int.Parse(dr[3].ToString());

                        if (thisLeagueId == -1)
                        {
                            log.Debug("thisHomeTeam: " + thisHomeTeam);
                        }

                        DateTime dt = DateTime.Parse(thisKoDate);

                        if (dt.Date == koDate.Date &&
                            thisHomeTeam == homeTeamId)
                        {
                            idx = int.Parse(id);
                            hasRows = true;
                            break;
                        }
                        else
                        {
                            hasRows = false;
                        }
                    }

                    dr.Close();

                    string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

                    if (idx != -1 && thisLeagueId == -1)
                    {
                        using (DbCommand update = dbCreator.newCommand("update games set league_id = " + leagueId + " where id = " + idx + ";", dbConnectionList.ElementAt(0)))
                        {
                            update.ExecuteNonQuery();
                        }
                    }

                    if (hasRows == false)
                    {
                        using (DbCommand count = dbCreator.newCommand("select max(id) from games;", dbConnectionList.ElementAt(0)))
                        {
                            using (DbDataReader dr2 = count.ExecuteReader())
                            {
                                dr2.Read();
                                try
                                {
                                    int rows = int.Parse(dr2[0].ToString());
                                    idx = rows + 1;
                                }
                                catch (Exception)
                                {
                                    idx = 1;
                                }

                                dr2.Close();

                                using (DbCommand insert = dbCreator.newCommand("INSERT into games (id, league_id, team1, team2, koDate,  created_at, updated_at  ) VALUES (" + idx + ", " + leagueId + ", " + homeTeamId + ", " + awayTeamId + ", '" + koDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + now + "', '" + now + "');", dbConnectionList.ElementAt(0)))
                                {
                                    insert.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

            }
            return idx;
        }

        public int AddLeague(string leagueName)
        {
            int idx = -1;

            bool hasRows = false;
            using (DbCommand find = dbCreator.newCommand("SELECT id, name FROM leagues WHERE name = '" + leagueName + "';", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        string id = dr[0].ToString();
                        string name = dr[1].ToString();

                        if (name == leagueName)
                        {
                            idx = int.Parse(id);
                        }
                    }

                    dr.Close();
                }
            }

            if (hasRows == false)
            {
                //see if it exists in the team_associations
                using (DbCommand findInTeamsTable = dbCreator.newCommand("SELECT league_id, name FROM league_associations WHERE name = '" + leagueName + "';", dbConnectionList.ElementAt(0)))
                {
                    using (DbDataReader dr = findInTeamsTable.ExecuteReader())
                    {
                        hasRows = dr.HasRows;

                        if (hasRows == true)
                        {
                            dr.Read();
                            string id = dr[0].ToString();
                            string name = dr[1].ToString();

                            if (name == leagueName)
                            {
                                idx = int.Parse(id);
                            }
                        }

                        dr.Close();
                    }
                }
            }

            if (hasRows == false)
            {
                using (DbCommand count = dbCreator.newCommand("select max(id) from leagues;", dbConnectionList.ElementAt(0)))
                {
                    using (DbDataReader dr2 = count.ExecuteReader())
                    {
                        dr2.Read();
                        try
                        {
                            int rows = int.Parse(dr2[0].ToString());
                            idx = rows + 1;
                        }
                        catch (Exception)
                        {
                            idx = 1;
                        }

                        dr2.Close();
                        string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                        using (DbCommand insert = dbCreator.newCommand("INSERT into leagues ( id, name, league_id, created_at, updated_at  ) VALUES (" + idx + ", '" + leagueName + "', " + idx + ", '" + now + "', '" + now + "');", dbConnectionList.ElementAt(0)))
                        {
                            insert.ExecuteNonQuery();
                        }
                    }
                }
            }

            return idx;
        }

        public int GetActiveBotStates(IEnumerable<string> games)
        {
            String lastGameScanned = "";
            String nextGameToScan = "";
            DataSet ds = new DataSet();
            DbTransaction transaction = null;
            int idx = -2;

            transaction = dbCreator.newTransaction(dbConnectionList.ElementAt(0));

            using (var da = dbCreator.newAdapter("select * from bots", dbConnectionList.ElementAt(0)))
            {
                da.InsertCommand = dbCreator.newCommand("insert into bots(bot_id, updated_at, created_at) " +
                                                        " values (:a, :b, :c)", dbConnectionList.ElementAt(0));
                try
                {
                    da.InsertCommand.Parameters.Add(new NpgsqlParameter("a", NpgsqlDbType.Varchar));
                    da.InsertCommand.Parameters.Add(new NpgsqlParameter("b", NpgsqlDbType.Timestamp));
                    da.InsertCommand.Parameters.Add(new NpgsqlParameter("c", NpgsqlDbType.Timestamp));
                    da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
                    da.InsertCommand.Parameters[1].Direction = ParameterDirection.Input;
                    da.InsertCommand.Parameters[2].Direction = ParameterDirection.Input;
                    da.InsertCommand.Parameters[0].SourceColumn = "bot_id";
                    da.InsertCommand.Parameters[1].SourceColumn = "updated_at";
                    da.InsertCommand.Parameters[2].SourceColumn = "created_at";

                    da.InsertCommand.Transaction = transaction;

                    da.Fill(ds);

                    DataTable dt = ds.Tables[0];

                    da.DeleteCommand = dbCreator.newCommand("TRUNCATE TABLE bots", dbConnectionList.ElementAt(0));
                    da.DeleteCommand.Transaction = transaction;
                    da.DeleteCommand.ExecuteNonQuery();

                    var gameColumn = dt.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray();
                    var tsColumn = dt.Rows.Cast<DataRow>().Select(row => row[1].ToString()).ToArray();

                    idx = Array.FindIndex(tsColumn, row => row.ToString() == DateTime.MinValue.ToString());

                    if (tsColumn.Count() == idx)
                    {
                        idx = -1;
                    }

                    var scanNextGame = lastGameScanned == ""; //cheeky!

                    int counter = 0;
                    foreach (var game in games)
                    {
                        DataRow dr = dt.NewRow();

                        dr["bot_id"] = game;
                        dr["updated_at"] = counter == idx + 1 ? DateTime.MinValue : DateTime.Now;
                        dr["created_at"] = counter == idx + 1 ? DateTime.MinValue : DateTime.Now;

                        dt.Rows.Add(dr);
                        ++counter;
                    }

                    DataSet ds2 = ds.GetChanges();
                    da.Update(ds2);
                    ds.Merge(ds2);
                    ds.AcceptChanges();
                }
                catch (Exception ce)
                {
                    log.Warn("FAILED to get a LOCK on the DB:(");

                    if (transaction != null)
                    {
                        transaction.Rollback();
                        transaction = null;
                    }
                    idx = -2;
                }
                finally
                {
                    if(transaction != null)
                        transaction.Commit();
                }
            }
            
            return idx + 1;
        }

        public bool AddStatistics(List<int> values, int gameId, string minutes, string lastMinute, DateTime seenTime)
        {
            int idx = -1;

            int minutesParsed = ParseMinutes(minutes);

            //check last minute to see if we've seen this game and return quickly
            if (lastMinute != "")
            {
                int lastMinuteParsed = ParseMinutes(lastMinute);

                using (DbCommand find = dbCreator.newCommand("SELECT id, game_id FROM statistics WHERE game_id = " + gameId + " AND gametime = " + lastMinuteParsed + ";", dbConnectionList.ElementAt(0)))
                {
                    using (DbDataReader dr = find.ExecuteReader())
                    {
                        bool hasRows = dr.HasRows;

                        if (hasRows)
                        {
                            log.Info("Already seen the minute " + lastMinuteParsed + " of this game");
                            dr.Close();
                            return false;
                        }

                        dr.Close();
                    }
                }
            }

            using (DbCommand find = dbCreator.newCommand("SELECT id, game_id FROM statistics WHERE game_id = " + gameId + " AND gametime = " + minutesParsed + ";", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        string id = dr[0].ToString();

                        int gCheck = int.Parse(dr[1].ToString());

                        if (gCheck == gameId)
                        {
                            idx = int.Parse(id);
                        }
                    }

                    dr.Close();

                    if (hasRows == false)
                    {
                        log.Info("Uploading game time: " + minutesParsed);

                        using (DbCommand count = dbCreator.newCommand("select max(id) from statistics;", dbConnectionList.ElementAt(0)))
                        {
                            using (DbDataReader dr2 = count.ExecuteReader())
                            {
                                dr2.Read();

                                try
                                {
                                    int rows = int.Parse(dr2[0].ToString());
                                    idx = rows + 1;
                                }
                                catch (Exception)
                                {
                                    idx = 1;
                                }

                                dr2.Close();

                                string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd");
                                string valuesAsString = string.Join(", ", values);

                                dbConnectionList.ElementAt(0).CreateCommand();

                                string sql = "";

                                if (m_opMode == OperationMode.Bet365Scan || m_opMode == OperationMode.UploadBet365)
                                {
                                    if (values.Count() == 8)
                                    {
                                        sql = "INSERT into statistics ( " +
                                       "id, gametime, game_id, seentime, hrc, hyc, hco, hg, arc, ayc, aco, ag, created_at, updated_at ) " +
                                       " VALUES " +
                                       "( " + idx + ", '" +
                                       minutesParsed + "', " +
                                       gameId + ", '" +
                                       seenTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                       valuesAsString + ", '" + now + "', '" + now + "');";
                                    }
                                    else
                                    {
                                        sql = "INSERT into statistics ( " +
                                        "id, gametime, game_id, seentime, hrc, hyc, hco, hsont, hsofft, ha, hda, hg, arc, ayc, aco, asont, asofft, aa, ada, ag, created_at, updated_at ) " +
                                        " VALUES " +
                                        "( " + idx + ", '" +
                                        minutesParsed + "', " +
                                        gameId + ", '" +
                                        seenTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                        valuesAsString + ", '" + now + "', '" + now + "');";
                                    }
                                }
                                else
                                {
                                    sql = "INSERT into statistics ( " +
                                    "id, gametime, game_id, seentime, hpn, hg, hpen, hsont, hsofft, hw, hco, hfk, ht, hyc, hrc, ha, hda, hbs, hcl, apn, ag, apen, asont, asofft, aw, aco, afk, at, ayc, arc, aa, ada, abs, acl, created_at, updated_at ) " +
                                    " VALUES " +
                                    "( " + idx + ", '" +
                                    minutesParsed + "', " +
                                    gameId + ", '" +
                                    seenTime.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                                    valuesAsString + ", '" + now + "', '" + now + "');";
                                }


                                using (DbCommand insert = dbCreator.newCommand(sql))
                                {
                                    insert.Connection = dbConnectionList.ElementAt(0);
                                    insert.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    else
                    {
                        log.Info("Already seen minute " + minutesParsed + " of this game");
                    }
                }

            }

            return true;
        }

        private int ParseMinutes(string time)
        {
            int minutes = 0;

            if (time.ToLower().StartsWith("half"))
            {
                minutes = -1;
            }
            else if (time.ToLower().StartsWith("full") || time.Trim().StartsWith("End Of Normal Time"))
            {
                minutes = -2;
            }
            else if (time.Contains(":"))
            {
                string mins = Regex.Split(time, ":").ElementAt(0);
                minutes = int.Parse(mins);
            }
            return minutes;
        }


        public string m_dbConnectionString { get; set; }

        public void AddCornerData(int gameID, string cornerline, string homeprice, string awayprice)
        {
            bool alreadyGotThis = false;

            using (DbCommand find = dbCreator.newCommand("SELECT game_id, cornerline, homeprice, awayprice, created_at FROM asiancorners WHERE game_id like '" + gameID + "' order by created_at desc;", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows)
                    {
                        dr.Read();
                        if (dr[1].ToString() == cornerline &&
                           dr[2].ToString() == homeprice &&
                           dr[3].ToString() == awayprice)
                        {
                            log.Info("Already got latest corner price!");
                            alreadyGotThis = true;
                        }
                    }

                    dr.Close();
                }
            }

            if (alreadyGotThis == false)
            {
                string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                using (DbCommand insert = dbCreator.newCommand("INSERT into asiancorners ( idx, game_id, cornerline, homeprice, awayprice, created_at, updated_at  ) VALUES ('" + 1 + "', '" + gameID + "', '" + cornerline + "', '" + homeprice + "', '" + awayprice + "', '" + now + "', '" + now + "');", dbConnectionList.ElementAt(0)))
                {
                    insert.ExecuteNonQuery();
                }
            }   
        }

        public void AddPredictionsData(string gameId, Dictionary<string, string> data)
        {

            string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string sql = "INSERT into prediction_data ( id, \"gameid\", \"goalswinhome\", \"goalswinaway\", \"goalslikelyscorehome\", \"goalslikelyscoreaway\", \"goalslikelyprobability\", " +
                         "\"cornerswinhome\", \"cornerswinaway\", \"cornerslikelyscorehome\", \"cornerslikelyscoreaway\", \"cornerslikelyprobability\", created_at, updated_at  ) " + 
                         " VALUES (" + 
                         gameId + ", " + 
                         gameId + ", " + 
                         data["goalsWinHome"] + ", "  +
                         data["goalsWinAway"] + ", " +
                         data["goalsLikelyScoreHome"] + ", " +
                         data["goalsLikelyScoreAway"] + ", " +
                         data["goalsLikelyProbability"] + ", " +
                         data["cornersWinHome"] + ", " +
                         data["cornersWinAway"] + ", " +
                         data["cornersLikelyScoreHome"] + ", " +
                         data["cornersLikelyScoreAway"] + ", " +
                         data["cornersLikelyProbability"] + ", '" +
                         now + "', '" + now + "');";

            bool alreadyGotThis = false;

            using (DbCommand find = dbCreator.newCommand("SELECT \"gameid\" FROM predictions WHERE \"gameid\" = " + gameId + ";", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    alreadyGotThis = dr.HasRows;
                    dr.Close();
                }
            }

            //just add it again
            //if (alreadyGotThis == false)
            {
                log.Info(sql);

                using (DbCommand insert = dbCreator.newCommand(sql, dbConnectionList.ElementAt(0)))
                {
                    insert.ExecuteNonQuery();
                }
            }
        }

        public void AddRaceToCornerData(int gameId, int cornerTarget, string homeprice, string awayprice, string neitherprice)
        {
            string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string sql = "INSERT into racetocorners ( idx, game_id, cornertarget, homeprice, awayprice, neitherprice, created_at, updated_at  ) VALUES ('" + 1 + "', '" + gameId + "', '" + cornerTarget + "', '" + homeprice + "', '" + awayprice + "', '" + neitherprice + "', '" + now + "', '" + now + "');";

            bool alreadyGotThis = false;

            using (DbCommand find = dbCreator.newCommand("SELECT homeprice, awayprice, neitherprice, created_at FROM racetocorners WHERE game_id like '" + gameId + "' AND cornertarget like '" + cornerTarget + "' order by created_at desc;", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows)
                    {
                        dr.Read();
                        if (dr[0].ToString() == homeprice &&
                            dr[1].ToString() == awayprice &&
                            dr[2].ToString() == neitherprice)
                        {
                            log.Info("Already got latest race to corner price!");
                            alreadyGotThis = true;
                        }
                        else
                        {
                            log.Info("Adding new corner info!!!");
                        }
                    }

                    dr.Close();
                }
            }

            if (alreadyGotThis == false)
            {
                log.Info(sql);

                using (DbCommand insert = dbCreator.newCommand(sql, dbConnectionList.ElementAt(0)))
                {
                    insert.ExecuteNonQuery();
                }
            }
        }

        public void AddFinalResultPrices(int gameId, string homeprice, string drawprice, string awayprice)
        {
            string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string sql = "INSERT into fulltimeprice ( idx, game_id, homeprice, drawprice, awayprice, created_at, updated_at  ) VALUES ('" + 1 + "', '" + gameId + "', '" + homeprice + "', '" + drawprice + "', '" + awayprice + "', '" + now + "', '" + now + "');";

            bool alreadyGotThis = false;

            using (DbCommand find = dbCreator.newCommand("SELECT homeprice, drawprice, awayprice, created_at FROM fulltimeprice WHERE game_id like '" + gameId + "' order by created_at desc;", dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows)
                    {
                        dr.Read();
                        if (dr[0].ToString() == homeprice &&
                            dr[1].ToString() == drawprice &&
                            dr[2].ToString() == awayprice)
                        {
                            log.Info("Already got latest fulltime price!");
                            alreadyGotThis = true;
                        }
                        else
                        {
                            log.Info("Adding new fulltime info!!!");
                        }
                    }

                    dr.Close();
                }
            }

            if (alreadyGotThis == false)
            {
                log.Info(sql);

                using (DbCommand insert = dbCreator.newCommand(sql, dbConnectionList.ElementAt(0)))
                {
                    insert.ExecuteNonQuery();
                }
            }
        }

        public string GetGameDetails(string id)
        {
            var sql = "select g1.id, t1.name, t2.name, l1.name, g1.kodate from games g1 join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join leagues l1 on l1.id = g1.league_id where g1.id =" + id;

            string retVal = "";

            using (DbCommand find = dbCreator.newCommand(sql, dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows)
                    {
                        dr.Read();
                        retVal = dr[0].ToString() + " " + dr[1].ToString() + " " + dr[2].ToString() + " " + dr[3].ToString() + " " + dr[4].ToString();
                    }

                    dr.Close();
                }
            }

            return retVal;
        }

        public List<string> GetGamesForThisDay(DateTime thisDay)
        {
            string day = thisDay.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            return OneColumnQuery("select id from games where to_char(kodate, 'YYYY-MM-DD') like '" + day + "%'");
        }

        public List<string> GetFurureGames()
        {
            return OneColumnQuery("select id from games where kodate > current_date");
        }

        public List<string> GetIdsFromPredictionTable()
        {
            return OneColumnQuery("select id from prediction_data");
        }

        private List<string> OneColumnQuery(string sql)
        {
            var ids = new List<string>();

            using (DbCommand find = dbCreator.newCommand(sql, dbConnectionList.ElementAt(0)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            ids.Add(dr[0].ToString());
                        }
                    }
                }
            }

            return ids;
        }
    }
}
