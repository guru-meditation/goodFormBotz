﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormzTool
{
    public partial class FormzDBEditor : Form
    {
        static NpgsqlConnection pgConnection = null;
        string connectionString = "Database=d7menjp3rap4ts;Server=ec2-54-235-155-182.compute-1.amazonaws.com;Port=5432;User Id=leupjwfvjinxsi;Password=HACn2POfVhsUY9S5HUsV7DhgS_;SSL=true;CommandTimeout=600;Timeout=600";
        //string connectionString = "Database=deg5ivhqu73n1i;Server=ec2-54-243-181-184.compute-1.amazonaws.com;Port=5432;User Id=mjkscoveqvuszj;Password=qj1TBKCPuVxeCAR2sT79uIHAqT;SSL=true";

        public FormzDBEditor()
        {
            InitializeComponent();

            try
            {
                pgConnection = new NpgsqlConnection(connectionString);
                pgConnection.Open();
                pgConnection.StateChange += pgConnection_StateChange;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
                pgConnection = null;
            }
        }

        void pgConnection_StateChange(object sender, StateChangeEventArgs e)
        {
            if(e.CurrentState == ConnectionState.Broken || 
                e.CurrentState == ConnectionState.Closed)
            {
                MessageBox.Show("Connection = "+ e.CurrentState);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void getButton1_Click(object sender, EventArgs e)
        {
            GetOldTeam(teamBox1.Text);
        }

        private class Match
        {
            public string homeTeam;
            public string awayTeam;
            public string league;
            public string gameId;
            public string hgs;
            public string ags;
            public string hco;
            public string aco;
            public string seenTime;
        }

        private void GetOldTeam(string teamName, int box = 1)
        {
            int test = 0;
            string id = "";

            if (int.TryParse(teamName.Trim(), out test))
            {
                //we have an id
                id = teamName;
            }
            else
            {
                id = GetTeamId(teamName);
            }

            var tMatchBox = matchBox2;

            if (box != 1)
            {
                tMatchBox = matchBox3;
            }

            tMatchBox.Items.Clear();

            if (id == "")
            {
                MessageBox.Show(teamName + " does not exist");
                return;
            }

            string sql = "select teams.name, leagues.name, games.id from games join teams on games.team2 = teams.id join leagues on games.league_id = leagues.id where games.id in ( SELECT id FROM games WHERE team1 = '" + id + "');";

            int maxHomeTeamLength = teamName.Length;
            int maxAwayTeamLength = teamName.Length;

            //List<string> homeTeams = new List<string>() { "Home:" };
            //List<string> awayTeams = new List<string>() { "Away:" };
            //List<string> leagues = new List<string>() { "League:" };
            //List<string> gameIds = new List<string>() { "Id:" };
            //List<string> hgsList = new List<string>() { "HG:" };
            //List<string> agsList = new List<string>() { "AG:" };
            //List<string> hcsList = new List<string>() { "HC:" };
            //List<string> acsList = new List<string>() { "AC:" };
            //List<string> seenTimes = new List<string>() { "Seen:" };
            //List<string> respGameIds = new List<string>() { "RespGameIds:" };
            List<Match> matches = new List<Match>();

            Match title = new Match();
            title.homeTeam = "Home:";
            title.awayTeam = "Away:";
            title.league = "League:";
            title.gameId = "Id:";
            title.hgs = "HG:";
            title.ags = "AG:";
            title.hco = "HC:";
            title.aco = "AC:";
            title.seenTime = "Seen:";

            matches.Add(title);

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            string awayTeam = dr[0].ToString();
                            string league = dr[1].ToString();
                            string gameId = dr[2].ToString();

                            Match m = new Match();
                            m.homeTeam = teamName;
                            m.awayTeam = awayTeam;
                            m.league = league;
                            m.gameId = gameId;

                            matches.Add(m);
                        }
                    }
                }
            }

            string sql2 = "select teams.name, leagues.name, games.id from games join teams on games.team1 = teams.id join leagues on games.league_id = leagues.id where games.id in ( SELECT id FROM games WHERE team2 = '" + id + "');";

            using (NpgsqlCommand find = new NpgsqlCommand(sql2, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            string homeTeam = dr[0].ToString();
                            string league = dr[1].ToString();
                            string gameId = dr[2].ToString();

                            Match m = new Match();
                            m.homeTeam = homeTeam;
                            m.awayTeam = teamName;
                            m.league = league;
                            m.gameId = gameId;

                            matches.Add(m);
                        }
                    }
                }
            }

            if (matches.Count() == 1)
            {
                MessageBox.Show("No matches found with data");
            }
            else if (getStats.Checked)
            {
                string sql5 = "select g1.id from games g1 left join statistics s1 on g1.id = s1.game_id where s1.id is null";
                var empties = new List<string>();

                using (NpgsqlCommand find = new NpgsqlCommand(sql5, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        while (dr.Read() == true)
                        {
                            empties.Add(dr[0].ToString());
                        }
                    }
                }

                matches.RemoveAll(x => empties.Contains(x.gameId));

                string hg, ag, hc, ac, ls, g_id;

                var gameIds = matches.Select(x => x.gameId);

                var copyOfGameIds = gameIds.ToArray().ToList();
                copyOfGameIds.RemoveAt(0);
                string gameIdsString = String.Join(",", copyOfGameIds);

                string sql4 = "select hg, ag, hco, aco, gametime, game_id from " +
                              "(select game_id, gametime, hg, ag, hco, aco, max(gametime) over (partition by game_id) max_gameTime from statistics where game_id in  (  " + gameIdsString + " ) )" +
                              " a where game_id in  (  " + gameIdsString + " ) AND gametime = max_GameTime";

                using (NpgsqlCommand find = new NpgsqlCommand(sql4, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        while (dr.Read() == true)
                        {
                            hg = dr[0].ToString();
                            ag = dr[1].ToString();
                            hc = dr[2].ToString();
                            ac = dr[3].ToString();
                            ls = dr[4].ToString();
                            g_id = (dr[5].ToString());

                            Match m = matches.SingleOrDefault(x => x.gameId == g_id);
                            if (m == null)
                            {
                                MessageBox.Show("null match!!");
                            }
                            else
                            {
                                m.aco = ac;
                                m.hco = hc;
                                m.hgs = hg;
                                m.ags = ag;
                                m.seenTime = ls;
                            }
                        }
                    }
                }
            }

            int longestHomeTeam = matches.Select(x => x.homeTeam).Max(x => x.Length);
            int longestAwayTeam = matches.Select(x => x.awayTeam).Max(x => x.Length);
            int longestGameId = matches.Select(x => x.gameId).Max(x => x.Length);
            int longestLeagueId = matches.Select(x => x.league).Max(x => x != null ? x.Length : 0);
            int longesthg = matches.Select(x => x.hgs).Max(x => x != null ? x.Length : 0);
            int longestag = matches.Select(x => x.ags).Max(x => x != null ? x.Length : 0);
            int longesthc = matches.Select(x => x.hco).Max(x => x != null ? x.Length : 0);
            int longestac = matches.Select(x => x.aco).Max(x => x != null ? x.Length : 0);

            for (int i = 0; i < matches.Count(); ++i)
            {
                if (getStats.Checked)
                {
                        tMatchBox.Items.Add(matches[i].gameId.PadRight(longestGameId + 1) + " "
                            + matches[i].homeTeam.PadRight(longestHomeTeam + 1) + " "
                            + matches[i].awayTeam.PadRight(longestAwayTeam) + " "
                            + matches[i].league.PadRight(longestLeagueId + 2) + " "
                            + matches[i].hgs.PadRight(longesthg + 1) + " "
                            + matches[i].ags.PadRight(longestag + 1) + " "
                            + matches[i].hco.PadRight(longesthc + 1) + " "
                            + matches[i].aco.PadRight(longestac + 1) + " "
                            + matches[i].seenTime);
                    
                }
                else
                {
                    tMatchBox.Items.Add(matches[i].gameId.PadRight(longestGameId + 1) + " "
                        + matches[i].homeTeam.PadRight(longestHomeTeam + 1) + " "
                        + matches[i].awayTeam.PadRight(longestAwayTeam) + " "
                        + matches[i].league);
                }
            }

            //var homeGoals = matches.Select(x => x.hgs);
            //var homeGoalsAsInts = new List<int>();
            //homeGoals.ToList().ForEach(x => homeGoalsAsInts.Add(int.Parse(x)));
            //int totalHGs = homeGoalsAsInts.Sum();

            //var awayGoals = matches.Select(x => x.ags);
            //var awayGoalsAsInts = new List<int>();
            //awayGoals.ToList().ForEach(x => awayGoalsAsInts.Add(int.Parse(x)));
            //int totalAGs = awayGoalsAsInts.Sum();

            //int totalScore = totalHGs + totalAGs;
            //int totalHomeGames = homeGoals.Count();
            //int totalAwayGames = awayGoals.Count();
            //int totalGames = totalHomeGames + totalAwayGames;

            //var tempList = new List<int>();
            //tempList.AddRange( homeGoalsAsInts);
            //tempList.AddRange( awayGoalsAsInts );

            //double meanGs = tempList.Average();

            //int avGoalsAtHome = 1;
            //int avGoalsAtAway = 1;
            //list int attackStrength = 3; /// Total number of goals scored by each team, divided by the average number of goals expected by any team
            //list int defenceWeakness = 3; /// Total number of goals conceded by each team, divided by the average number of goals expected by any team

            //var teams = new List<string>();
            //teams.AddRange(matches.Select(x => x.homeTeam));
            //teams.AddRange(matches.Select(x => x.awayTeam));
            //var distinctTeams = teams.Distinct();

            //var GoalsH = new int[,] {};
            //var GoalsA = new int[,] {};

            //int hIdx = 0;
            //foreach (var hTeam in distinctTeams)
            //{
            //    int aIdx = 0;

            //    foreach (var aTeam in distinctTeams)
            //    {
            //        GoalsH[hIdx, aIdx] = avGoalsAtHome * 
            //        GoalsA[hIdx, aIdx] = 1;        
            //    }
            //}
   
            idBox2.Text = id;
        }

        private bool GetLastStat(string gameId, out string hg, out string ag, out string hc, out string ac, out string ls)
        {
            string sql2 = "select min(gametime) from statistics where game_id = '" + gameId + "';";

            string lastTime = "";

            hg = ""; ag = ""; hc = ""; ac = ""; ls = "";

            //using (NpgsqlCommand find = new NpgsqlCommand(sql2, pgConnection))
            //{
            //    using (NpgsqlDataReader dr = find.ExecuteReader())
            //    {
            //        if (dr.HasRows == true)
            //        {
            //            dr.Read();
            //            lastTime = dr[0].ToString();
            //        }
            //    }
            //}

            //if (lastTime != "")
            {
                //if (lastTime != "-2")
                //{
                //    string sql3 = "select max(gametime) from statistics where game_id = '" + gameId + "';";

                //    using (NpgsqlCommand find = new NpgsqlCommand(sql3, pgConnection))
                //    {
                //        using (NpgsqlDataReader dr = find.ExecuteReader())
                //        {
                //            if (dr.HasRows == true)
                //            {
                //                dr.Read();
                //                lastTime = dr[0].ToString();
                //            }
                //        }
                //    }
                //}

                string sql4 = "select hg, ag, hco, aco from statistics where game_id = '" + gameId + "' AND ( gametime=-2 OR gametime = (select max(gametime) from statistics where game_id = '" + gameId + "'));";

                using (NpgsqlCommand find = new NpgsqlCommand(sql4, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        if (dr.HasRows == true)
                        {
                            dr.Read();
                            hg = dr[0].ToString();
                            ag = dr[1].ToString();
                            hc = dr[2].ToString();
                            ac = dr[3].ToString();
                        }

                    }
                }

                ls = lastTime == "-2" ? "Full Time" : lastTime;
            }

            return true;

        }

        private static List<string> GetTeamIds(string teamName)
        {
            List<string> ids = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM teams WHERE name = '" + teamName + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    while (dr.Read() == true)
                    {
                        ids.Add(dr[0].ToString());
                    }
                }
            }

            return ids;
        }

        private static string GetTeamId(string teamName)
        {
            string id = "";
            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM teams WHERE name = '" + teamName + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        id = dr[0].ToString();
                    }

                    if (dr.Read() == true)
                    {
                        MessageBox.Show("Warning two teams exist with the name " + teamName);
                    }

                }
            }

            return id;
        }

        private static string GetLeagueId(string leagueName)
        {
            string id = "";
            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM leagues WHERE name = '" + leagueName + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        id = dr[0].ToString();

                    }
                }
            }
            return id;
        }

        private static string GetLeagueName(string leagueId)
        {
            string id = "";
            using (NpgsqlCommand find = new NpgsqlCommand("SELECT name FROM leagues WHERE id = '" + leagueId + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        dr.Read();
                        id = dr[0].ToString();

                    }
                }
            }
            return id;
        }

        private bool renameTeam(string oldName, string newName)
        {

            int test = 0;
            string oldId = "";

            if (int.TryParse(oldName.Trim(), out test))
            {
                //we have an id
                oldId = oldName.Trim();
            }
            else
            {
                oldId = GetTeamId(oldName);
            }

            string newId = GetTeamId(newName);

            if (newId == "")
            {
                //add new team;
                newId = AddTeam(newName);
                if (newId == "-1")
                {
                    MessageBox.Show("Failed to create new team:", newName);
                    return false;
                }
            }

            if (test == 0 && AddTeamAssociation(oldName, newName) == false)
            {
                return false;
            }

            string[] homeOldGames = GetHomeGames(oldId);
            string[] awayOldGames = GetAwayGames(oldId);

            foreach (var id in homeOldGames)
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE games SET team1='" + newId + "' WHERE id = '" + id + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            foreach (var id in awayOldGames)
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE games SET team2='" + newId + "' WHERE id = '" + id + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            using (NpgsqlCommand delete = new NpgsqlCommand("DELETE from teams where id = '" + oldId + "';", pgConnection))
            {
                delete.ExecuteNonQuery();
            }

            return true;
        }

        private void renameButton_Click(object sender, EventArgs e)
        {
            if (renameTeam(teamBox1.Text, renameBox1.Text))
            {
                teamBox1.Text = renameBox1.Text;
                GetOldTeam(teamBox1.Text);
            }
        }

        private bool AddTeamAssociation(string alias, string teamName)
        {
            string originalId = GetTeamId(teamName);
            string aliasId = GetTeamId(alias);

            if (originalId == "")
            {
                originalId = GetTeamId(alias);
            }

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM team_associations WHERE name = '" + alias + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;
                    if (hasRows == true)
                    {
                        MessageBox.Show("Already have association for this team");
                        return false;
                    }
                }
            }

            if (aliasId != "")
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE team_associations SET team_id = '" + originalId + "' where team_id = '" + aliasId + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            int idx = -1;
            using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from team_associations", pgConnection))
            {
                using (NpgsqlDataReader dr2 = count.ExecuteReader())
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

                    using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into team_associations ( id, team_id, name, created_at, updated_at ) VALUES ('" + idx + "', '" + originalId + "', '" + alias + "', '" + now + "', '" + now + "');", pgConnection))
                    {
                        insert.ExecuteNonQuery();
                    }
                }
            }

            return true;
        }

        private bool AddLeagueAssociation(string alias, string leagueName)
        {
            string originalId = GetLeagueId(leagueName);
            string aliasId = GetLeagueId(alias);

            if (originalId == "")
            {
                originalId = GetLeagueId(alias);
            }

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM league_associations WHERE name = '" + alias + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;
                    if (hasRows == true)
                    {
                        MessageBox.Show("Already have association for this league");
                        return false;
                    }
                }
            }

            if (aliasId != "")
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE league_associations SET league_id = '" + originalId + "' where league_id = '" + aliasId + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            int idx = -1;
            using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from league_associations", pgConnection))
            {
                using (NpgsqlDataReader dr2 = count.ExecuteReader())
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

                    using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into league_associations ( id, league_id, name, created_at, updated_at ) VALUES ('" + idx + "', '" + originalId + "', '" + alias + "', '" + now + "', '" + now + "');", pgConnection))
                    {
                        insert.ExecuteNonQuery();
                    }
                }
            }

            return true;
        }

        private string AddTeam(string team)
        {
            int idx = -1;

            using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from teams", pgConnection))
            {
                count.CommandTimeout = 10000;
                using (NpgsqlDataReader dr2 = count.ExecuteReader())
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

                    using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into teams ( id, name, created_at, updated_at ) VALUES (" + idx + ", '" + team + "', '" + now + "', '" + now + "');", pgConnection))
                    {
                        insert.CommandTimeout = 10000;
                        insert.ExecuteNonQuery();
                    }
                }
            }

            return idx.ToString();
        }

        private string[] GetGamesForLeague(string league_id)
        {
            List<string> gameIds = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM games WHERE league_id = '" + league_id + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            gameIds.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return gameIds.ToArray();
        }

        private string[] GetHomeGames(string id)
        {
            List<string> gameIds = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM games WHERE team1 = '" + id + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            gameIds.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return gameIds.ToArray();
        }

        private string[] GetAwayGames(string id)
        {
            List<string> gameIds = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM games WHERE team2 = '" + id + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            gameIds.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return gameIds.ToArray();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            int numItems = matchBox2.Items.Count;

            for (int i = 0; i != numItems; ++i)
            {
                if (matchBox2.GetSelected(i) == true)
                {
                    string selectedText = "";
                    try
                    {
                        selectedText = matchBox2.GetItemText(matchBox2.Items[i]);
                    }
                    catch
                    {
                        MessageBox.Show("Select a game!");
                        return;
                    }

                    string id = Regex.Split(selectedText, " ").ElementAt(0);

                    string leagueID = "";

                    using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM leagues WHERE name = '" + leagueBox1.Text + "';", pgConnection))
                    {
                        using (NpgsqlDataReader dr = find.ExecuteReader())
                        {
                            bool hasRows = dr.HasRows;

                            if (hasRows == true)
                            {
                                while (dr.Read())
                                {
                                    leagueID = dr[0].ToString();
                                }
                            }
                        }
                    }

                    if (leagueID == "")
                    {
                        MessageBox.Show("Couldn't find league " + leagueBox1.Text);
                        return;
                    }

                    using (NpgsqlCommand insert = new NpgsqlCommand("UPDATE games SET league_id = '" + leagueID + "' WHERE id = '" + id + "';", pgConnection))
                    {
                        using (NpgsqlDataReader dr = insert.ExecuteReader())
                        {
                            insert.ExecuteNonQuery();
                        }
                    }
                }
            }

            if (teamBox1.Text != "")
            {
                GetOldTeam(teamBox1.Text);
            }
        }

        private void deleteGame_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox2.GetItemText(matchBox2.Items[matchBox2.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            using (NpgsqlCommand insert = new NpgsqlCommand("DELETE from games WHERE id = '" + id + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = insert.ExecuteReader())
                {
                    insert.ExecuteNonQuery();
                }
            }

            GetOldTeam(teamBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private static void ExecuteNonQuery(string sql)
        {
            using (NpgsqlCommand insert = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = insert.ExecuteReader())
                {
                    insert.ExecuteNonQuery();
                }
            }
        }

        private void getLeague_Click(object sender, EventArgs e)
        {
            GetOldLeague();
        }

        private void GetOldLeague()
        {
            int test = 0;
            string leagueText = leagueBox1.Text;
            string leagueId = "";

            matchBox2.Items.Clear();

            if (int.TryParse(leagueText.Trim(), out test))
            {
                //we have an id
                leagueId = leagueText.Trim();
                leagueText = GetLeagueName(leagueId);

                if (leagueText == "")
                {
                    MessageBox.Show("No league with ID: " + leagueId);
                    return;
                }
            }
            else
            {
                leagueId = GetLeagueId(leagueText);
            }

            List<string> homeTeams = new List<string>() { "Home:" };
            List<string> awayTeams = new List<string>() { "Away:" };
            List<string> leagues = new List<string>() { "League:" };
            List<string> gameIds = new List<string>() { "Id:" };


            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM games WHERE league_id = '" + leagueId + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        gameIds.Add(dr[0].ToString());
                        leagues.Add(leagueText);
                    }
                }
            }

            if (gameIds.Count() == 0)
            {
                MessageBox.Show("No games found for league: " + leagueBox1.Text);
                return;
            }

            using (NpgsqlCommand find = new NpgsqlCommand("select t1.name from games g1 join teams t1 on g1.team1 = t1.id where g1.league_id = '" + leagueId + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        homeTeams.Add(dr[0].ToString());
                    }
                }
            }

            using (NpgsqlCommand find = new NpgsqlCommand("select t1.name from games g1 join teams t1 on g1.team2 = t1.id where g1.league_id = '" + leagueId + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        awayTeams.Add(dr[0].ToString());
                    }
                }
            }

            int longestHomeTeam = homeTeams.Max(x => x.Length);
            int longestAwayTeam = awayTeams.Max(x => x.Length);
            int longestGameId = gameIds.Max(x => x.Length);

            for (int i = 0; i < homeTeams.Count(); ++i)
            {
                matchBox2.Items.Add(gameIds[i].PadRight(longestGameId + 1) + " " + homeTeams[i].PadRight(longestHomeTeam + 1) + " " + awayTeams[i].PadRight(longestAwayTeam) + " " + leagues[i]);
            }
        }

        private void renameLeague_Click_1(object sender, EventArgs e)
        {
            if (renameLeague(leagueBox1.Text, newLeagueBox1.Text))
            {
                leagueBox1.Text = newLeagueBox1.Text;
                GetOldLeague();
            }
        }

        private bool renameLeague(string oldName, string newName)
        {
            int test = 0;
            string oldId = "";

            if (int.TryParse(oldName.Trim(), out test))
            {
                //we have an id
                oldId = oldName.Trim();
            }
            else
            {
                oldId = GetLeagueId(oldName);
            }

            string newId = GetLeagueId(newName);

            if (newId == "")
            {
                //add new team;
                newId = GetLeagueId(newName);
                if (newId == "-1")
                {
                    MessageBox.Show("Failed to create new league:", newName);
                    return false;
                }
            }

            if (test == 0 && AddLeagueAssociation(oldName, newName) == false)
            {
                return false;
            }

            string[] games = GetGamesForLeague(oldId);

            foreach (var id in games)
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE games SET league_id ='" + newId + "' WHERE id = '" + id + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            using (NpgsqlCommand delete = new NpgsqlCommand("DELETE from leagues where id = '" + oldId + "';", pgConnection))
            {
                delete.ExecuteNonQuery();
            }

            return true;
        }

        private void createLeague_Click(object sender, EventArgs e)
        {
            if (newLeagueBox1.Text != "")
            {
                string newId = GetLeagueId(newLeagueBox1.Text);
            }
            else
            {
                MessageBox.Show("No league in newLeagueBox1");
            }
        }

        private void matchBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = "";
            try
            {
                selectedText = matchBox2.GetItemText(matchBox2.Items[matchBox2.SelectedIndex]);
            }
            catch
            {
                MessageBox.Show("Select a game!");
                return;
            }

            string id = Regex.Split(selectedText, "  ").ElementAt(0);
            string teamsA = Regex.Split(selectedText, "  ").ElementAt(1);

        }

        private void dupGames_Click(object sender, EventArgs e)
        {
            
        }

        private static List<string> OneColumnQuery(string sql2)
        {
            var ids = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand(sql2, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
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

        private void specialButton_Click(object sender, EventArgs e)
        {



        }

        private void todayButton_Click(object sender, EventArgs e)
        {
            matchBox.Items.Clear();

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            string sql2 = "select id from games where to_char(kodate, 'YYYY-MM-DD') like '" + now + "%';";

            var ids = OneColumnQuery(sql2);

            Console.WriteLine(ids);

            List<string> homeTeams = new List<string>() { "Home:" };
            List<string> awayTeams = new List<string>() { "Away:" };
            List<string> leagues = new List<string>() { "League:" };
            List<string> kickOffTimes = new List<string>() { "Kick Off:" };
            List<string> gameIds = new List<string>() { "Id:" };
            gameIds.AddRange(ids);

            string someIds = String.Join(",", ids);

            //foreach (var id in ids)
            {
                //2013-03-22 00:00:00 -0700

                string temp = "select t1.name, t2.name, l1.name, g1.kodate from games g1 join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join leagues l1 on g1.league_id = l1.id where g1.id in (" + someIds + "); ";
                using (NpgsqlCommand find = new NpgsqlCommand(temp, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            homeTeams.Add(dr[0].ToString());
                            awayTeams.Add(dr[1].ToString());
                            leagues.Add(dr[2].ToString());
                            kickOffTimes.Add(dr[3].ToString());
                        }
                    }
                }
            }

            int longestHomeTeam = homeTeams.Max(x => x.Length);
            int longestAwayTeam = awayTeams.Max(x => x.Length);
            int longestGameId = gameIds.Max(x => x.Length);
            int longestLeague = leagues.Max(x => x.Length);

            var tempList = new List<string>();

            for (int i = 0; i < homeTeams.Count(); ++i)
            {
                string temp = gameIds[i].PadRight(longestGameId + 1) + " " + homeTeams[i].PadRight(longestHomeTeam + 1) + " " + awayTeams[i].PadRight(longestAwayTeam) + " " + leagues[i].PadRight(longestLeague) + " " + (i == 0 ? kickOffTimes[i] : kickOffTimes[i].Substring(11, 5));
                tempList.Add(temp);
            }

            string lineOne = tempList.ElementAt(0);
            tempList.RemoveAt(0);
            tempList.Sort((x, y) => x.Substring(longestGameId).CompareTo(y.Substring(longestGameId)));

            tempList.Insert(0, lineOne);

            foreach (var t in tempList)
            {
                matchBox.Items.Add(t);
            }
        }

        private void matchBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            int numItems = matchBox.Items.Count;
            int selected = -1;

            for (int i = 0; i != numItems; ++i)
            {
                if (matchBox.GetSelected(i) == true)
                {
                    selected = i;
                    break;
                }
            }

            if (selected != -1)
            {
                string selectedText = matchBox.GetItemText(matchBox.Items[selected]);
                string header = matchBox.GetItemText(matchBox.Items[0]);
                int homeIdx = header.IndexOf("Home:");
                int awayIdx = header.IndexOf("Away:");
                int leagueIdx = header.IndexOf("League:");

                string homeTeam = selectedText.Substring(homeIdx, awayIdx - homeIdx).Trim();
                string awayTeam = selectedText.Substring(awayIdx, leagueIdx - awayIdx).Trim();

                GetOldTeam(awayTeam, 2);
                GetOldTeam(homeTeam);
            }

            if (checkBox1.Checked)
            {
            }

        }

        private void tomo_button1_Click(object sender, EventArgs e)
        {
        }

        private void removeDuplicateTeamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> badNames = new List<string>();

            string sql = "select name, count(*) from teams group by name having count(*) > 1;";

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            badNames.Add(dr[0].ToString());

                        }
                    }
                }
            }

            foreach (var name in badNames)
            {
                var ids = GetTeamIds(name);

                if (ids.Count() > 1)
                {
                    string primaryId = ids.ElementAt(0);
                    for (int i = 1; i != ids.Count(); ++i)
                    {
                        string sqlTeam1 = "update games set team1='" + primaryId + "' where team1='" + ids.ElementAt(i) + "';";
                        string sqlTeam2 = "update games set team2='" + primaryId + "' where team2='" + ids.ElementAt(i) + "';";
                        string sqlDeleteTeam = "delete from teams where id='" + ids.ElementAt(i) + "';";
                        ExecuteNonQuery(sqlTeam1);
                        ExecuteNonQuery(sqlTeam2);
                        ExecuteNonQuery(sqlDeleteTeam);
                    }

                }
            }

            matchBox2.Items.Clear();


            foreach (var name in badNames)
            {
                matchBox2.Items.Add(name);
                //    string newName = name.Replace(" Ladies", " Women");
                //    renameTeam(name, newName);
            }

        }

        private void specialButtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sql2 = "select id from games where team1='9706' and team2='9709';";

            var ids = OneColumnQuery(sql2);

            ids.RemoveAt(0);


            if (ids.Count() > 1)
            {
                string primaryId = ids.ElementAt(0);
                for (int i = 1; i != ids.Count(); ++i)
                {
                    string sqlTeam1 = "update statistics set game_id='" + primaryId + "' where game_id='" + ids.ElementAt(i) + "';";
                    string sqlDeleteTeam = "delete from games where id='" + ids.ElementAt(i) + "';";

                    ExecuteNonQuery(sqlTeam1);

                    ExecuteNonQuery(sqlDeleteTeam);
                }

            }

            matchBox2.Items.Clear();
        }

        private void removeDuplicateMatchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> team1s = new List<string>();
            List<string> team2s = new List<string>();
            List<string> kodates = new List<string>();

            string sql = "select team1, team2, kodate::date, count(*) from games group by team1, team2, kodate::date having count(*) > 1;";

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            team1s.Add(dr[0].ToString());
                            team2s.Add(dr[1].ToString());
                            kodates.Add(dr[2].ToString());
                        }
                    }
                }
            }

            for (int j = 0; j != team1s.Count(); ++j)
            {
                string sql2 = "select id from games where team1='" + team1s.ElementAt(j)
                            + "' and team2='" + team2s.ElementAt(j)
                            + "' and to_char(kodate, 'DD/MM/YYYY') = '" + kodates.ElementAt(j).Substring(0, 10) + "';";

                var ids = OneColumnQuery(sql2);

                var justDelete = new List<string>();

                foreach (var id in ids)
                {
                    var counts = OneColumnQuery("select count(*) from statistics where game_id = '" + id + "';");
                    if (counts.ElementAt(0) == "0")
                    {
                        justDelete.Add(id);
                    }
                }

                if (ids.Count() > 1)
                {
                    string primaryId = ids.ElementAt(0);
                    for (int i = 1; i != ids.Count(); ++i)
                    {
                        string sqlTeam1 = "update statistics set game_id='" + primaryId + "' where game_id='" + ids.ElementAt(i) + "';";
                        string sqlDeleteTeam = "delete from games where id='" + ids.ElementAt(i) + "';";
                        if (justDelete.Contains(ids.ElementAt(i)) == false)
                        {
                            ExecuteNonQuery(sqlTeam1);
                        }
                        ExecuteNonQuery(sqlDeleteTeam);
                    }

                }
            }

            matchBox2.Items.Clear();


            for (int j = 0; j != team1s.Count(); ++j)
            {
                matchBox2.Items.Add(kodates.ElementAt(j) + " " + team1s.ElementAt(j) + " v " + team2s.ElementAt(j));
                //    string newName = name.Replace(" Ladies", " Women");
                //    renameTeam(name, newName);
            }
        }

        private void tomorrowsGamesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            matchBox.Items.Clear();

            string now = (DateTime.Today + TimeSpan.FromDays(1)).ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            string sql2 = "select id from games where to_char(kodate, 'YYYY-MM-DD') like '" + now + "%';";

            var ids = OneColumnQuery(sql2);

            Console.WriteLine(ids);

            List<string> homeTeams = new List<string>() { "Home:" };
            List<string> awayTeams = new List<string>() { "Away:" };
            List<string> leagues = new List<string>() { "League:" };
            List<string> kickOffTimes = new List<string>() { "Kick Off:" };
            List<string> gameIds = new List<string>() { "Id:" };
            gameIds.AddRange(ids);

            string someIds = String.Join(",", ids);

            //foreach (var id in ids)
            {
                //2013-03-22 00:00:00 -0700

                string temp = "select t1.name, t2.name, l1.name, g1.kodate from games g1 join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join leagues l1 on g1.league_id = l1.id where g1.id in (" + someIds + "); ";
                using (NpgsqlCommand find = new NpgsqlCommand(temp, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            homeTeams.Add(dr[0].ToString());
                            awayTeams.Add(dr[1].ToString());
                            leagues.Add(dr[2].ToString());
                            kickOffTimes.Add(dr[3].ToString());
                        }
                    }
                }
            }

            int longestHomeTeam = homeTeams.Max(x => x.Length);
            int longestAwayTeam = awayTeams.Max(x => x.Length);
            int longestGameId = gameIds.Max(x => x.Length);
            int longestLeague = leagues.Max(x => x.Length);

            var tempList = new List<string>();

            for (int i = 0; i < homeTeams.Count(); ++i)
            {
                string temp = gameIds[i].PadRight(longestGameId + 1) + " " + homeTeams[i].PadRight(longestHomeTeam + 1) + " " + awayTeams[i].PadRight(longestAwayTeam) + " " + leagues[i].PadRight(longestLeague) + " " + (i == 0 ? kickOffTimes[i] : kickOffTimes[i].Substring(11, 5));
                tempList.Add(temp);
            }

            string lineOne = tempList.ElementAt(0);
            tempList.RemoveAt(0);
            tempList.Sort((x, y) => x.Substring(longestGameId).CompareTo(y.Substring(longestGameId)));

            tempList.Insert(0, lineOne);

            foreach (var t in tempList)
            {
                matchBox.Items.Add(t);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

    }
}
