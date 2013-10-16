using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Db
{
    using BotSpace;

    public class Database
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DbCreator dbCreator = null;
        private List<DbConnection> dbConnectionList = new List<DbConnection>();
        
        private int botIndex = 0;
        private OperationMode opMode;

        public Database(string dbtype, string connectionString, int numBots, OperationMode operationMode)
        {
            opMode = operationMode;

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

            try
            {
                for (int i = 0; i != numBots; ++i)
                {
                    DbConnection connection = dbCreator.newConnection(connectionString);
                    connection.Open();
                    dbConnectionList.Add(connection);
                }
            }
            catch (Exception ex)
            {
                log.Error("Creating new connection " + ex.ToString());
                dbConnectionList = null;
            }
        }

        public int AddTeam(string team)
        {
            int idx = -1;
            bool hasRows = false;

            using (DbCommand findInTeamsTable = dbCreator.newCommand("SELECT id, name FROM teams WHERE name = '" + team + "';", dbConnectionList.ElementAt(botIndex)))
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
                using (DbCommand findInTeamsTable = dbCreator.newCommand("SELECT team_id, name FROM team_associations WHERE name = '" + team + "';", dbConnectionList.ElementAt(botIndex)))
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
                using (DbCommand count = dbCreator.newCommand("select max(id) from teams", dbConnectionList.ElementAt(botIndex)))
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

                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        using (DbCommand insert = dbCreator.newCommand("INSERT into teams ( id, name, created_at, updated_at ) VALUES (" + idx + ", '" + team + "', '" + now + "', '" + now + "');", dbConnectionList.ElementAt(botIndex)))
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
            using (DbCommand find = dbCreator.newCommand("SELECT id, team1, kodate, league_id FROM games WHERE team1 = '" + homeTeamId + "' AND team2 = '" + awayTeamId + "';", dbConnectionList.ElementAt(botIndex)))
            {
                using (DbDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    while (dr.Read())  //bug fix for repeated same game added after rematch
                    {

                        string id = dr[0].ToString();
                        int thisHomeTeam = int.Parse(dr[1].ToString());

                        string thisKoDate = dr[2].ToString();
                        int thisLeagueId = int.Parse(dr[3].ToString());

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

                    if (hasRows == false)
                    {
                        using (DbCommand count = dbCreator.newCommand("select max(id) from games;", dbConnectionList.ElementAt(botIndex)))
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

                                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                using (DbCommand insert = dbCreator.newCommand("INSERT into games (id, league_id, team1, team2, koDate,  created_at, updated_at  ) VALUES (" + idx + ", " + leagueId + ", " + homeTeamId + ", " + awayTeamId + ", '" + koDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + now + "', '" + now + "');", dbConnectionList.ElementAt(botIndex)))
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
            using (DbCommand find = dbCreator.newCommand("SELECT id, name FROM leagues WHERE name = '" + leagueName + "';", dbConnectionList.ElementAt(botIndex)))
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
                using (DbCommand findInTeamsTable = dbCreator.newCommand("SELECT league_id, name FROM league_associations WHERE name = '" + leagueName + "';", dbConnectionList.ElementAt(botIndex)))
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
                using (DbCommand count = dbCreator.newCommand("select max(id) from leagues;", dbConnectionList.ElementAt(botIndex)))
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
                        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        using (DbCommand insert = dbCreator.newCommand("INSERT into leagues ( id, name, league_id, created_at, updated_at  ) VALUES (" + idx + ", '" + leagueName + "', " + idx + ", '" + now + "', '" + now + "');", dbConnectionList.ElementAt(botIndex)))
                        {
                            insert.ExecuteNonQuery();
                        }
                    }
                }
            }

            return idx;
        }

        public bool AddStatistics(List<int> values, int gameId, string minutes, string lastMinute, DateTime seenTime)
        {
            int idx = -1;

            int minutesParsed = ParseMinutes(minutes);

            //check last minute to see if we've seen this game and return quickly
            if (lastMinute != "")
            {
                int lastMinuteParsed = ParseMinutes(lastMinute);

                using (DbCommand find = dbCreator.newCommand("SELECT id, game_id FROM statistics WHERE game_id = " + gameId + " AND gametime = " + lastMinuteParsed + ";", dbConnectionList.ElementAt(botIndex)))
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

            using (DbCommand find = dbCreator.newCommand("SELECT id, game_id FROM statistics WHERE game_id = " + gameId + " AND gametime = " + minutesParsed + ";", dbConnectionList.ElementAt(botIndex)))
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

                        using (DbCommand count = dbCreator.newCommand("select max(id) from statistics;", dbConnectionList.ElementAt(botIndex)))
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

                                string now = DateTime.Now.ToString("yyyy-MM-dd");
                                string valuesAsString = string.Join(", ", values);

                                dbConnectionList.ElementAt(botIndex).CreateCommand();

                                string sql = "";

                                if (opMode == OperationMode.Bet365Scan || opMode == OperationMode.UploadBet365)
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
                                    insert.Connection = dbConnectionList.ElementAt(botIndex);
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

    }
}
