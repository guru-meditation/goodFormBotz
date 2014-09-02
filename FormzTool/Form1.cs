using Npgsql;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Data.Common;


namespace FormzTool
{
    public partial class FormzDBEditor : Form
    {
        static string connectionString = ConfigurationManager.AppSettings["connection1"];
        static NpgsqlConnection pgConnection = null;

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
            if (e.CurrentState == ConnectionState.Broken ||
                e.CurrentState == ConnectionState.Closed)
            {
                MessageBox.Show("Connection = " + e.CurrentState);
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
            public string kodate;


            public string koDate { get; set; }
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

            string sql = "select teams.name, leagues.name, games.id, games.kodate from games join teams on games.team2 = teams.id join leagues on games.league_id = leagues.id where games.id in ( SELECT id FROM games WHERE team1 = '" + id + "') order by games.kodate desc;";

            int maxHomeTeamLength = teamName.Length;
            int maxAwayTeamLength = teamName.Length;

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
            title.koDate = "Date:";

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
                            string koDate = dr[3].ToString();

                            Match m = new Match();
                            m.homeTeam = teamName;
                            m.awayTeam = awayTeam;
                            m.league = league;
                            m.gameId = gameId;
                            m.koDate = koDate;

                            matches.Add(m);
                        }
                    }
                }
            }

            string sql2 = "select teams.name, leagues.name, games.id, games.kodate from games join teams on games.team1 = teams.id join leagues on games.league_id = leagues.id where games.id in ( SELECT id FROM games WHERE team2 = '" + id + "') order by games.kodate desc;";

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
                            string koDate = dr[3].ToString();

                            Match m = new Match();
                            m.homeTeam = homeTeam;
                            m.awayTeam = teamName;
                            m.league = league;
                            m.gameId = gameId;
                            m.koDate = koDate;

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
                var gameIds = matches.Select(x => x.gameId).ToList();
                gameIds.RemoveAt(0);

                string gameIdsString = String.Join(",", gameIds);

                string sql5 = "select g1.id from games g1 left join statistics s1 on g1.id = s1.game_id where s1.id is null and g1.id in  (  " + gameIdsString + " )";
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

                string sql4 = "select hg, ag, hco, aco, gametime, game_id from " +
                              "(select game_id, gametime, hg, ag, hco, aco, max(gametime) over (partition by game_id) max_gameTime from statistics where game_id in  (  " + gameIdsString + " ))" +
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
                        + matches[i].league.PadRight(longestLeagueId) + " "
                        + matches[i].koDate);
                }
            }

            idBox2.Text = id;
        }

        private bool GetLastStat(string gameId, out string hg, out string ag, out string hc, out string ac, out string ls)
        {
            string sql2 = "select min(gametime) from statistics where game_id = '" + gameId + "';";

            string lastTime = "";

            hg = ""; ag = ""; hc = ""; ac = ""; ls = "";

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

            return true;
        }

        private static List<string> GetTeamIds(string teamName)
        {
            return ExecuteSimpleQuery("SELECT id FROM teams WHERE name = '" + teamName + "';");
        }

        private static List<string> GetLeagueIds(string leagueName)
        {
            return ExecuteSimpleQuery("SELECT id FROM leagues WHERE name = '" + leagueName + "';");
        }

        private static string GetTeamId(string teamName)
        {
            var ids = GetTeamIds(teamName);

            if (ids.Count() > 1)
            {
                MessageBox.Show("Warning " + ids.Count() + " teams exist with the name " + teamName);
            }

            return ids.First();
        }

        private static string GetGameCount(string teamId)
        {
            var counts = ExecuteSimpleQuery("select count(*) from games where team1=" + teamId + " or team2=" + teamId + ";");

            return counts.First();
        }

        private static string GetLeagueId(string leagueName)
        {
            var leagueIds = ExecuteSimpleQuery("SELECT id FROM leagues WHERE name = '" + leagueName + "';");

            return leagueIds.First();
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

            bool haveAssociationAlready = false;

            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id FROM team_associations WHERE name = '" + alias + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    haveAssociationAlready = dr.HasRows;
                }
            }

            if (haveAssociationAlready == true)
            {
                MessageBox.Show("Already have association: " + alias + " for team: " + teamName);
            }

            if (aliasId != "")
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE team_associations SET team_id = '" + originalId + "' where team_id = '" + aliasId + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            int idx = -1;

            if (haveAssociationAlready == false)
            {
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
                        //return false;
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

        public string AddLeague(string leagueName)
        {
            int idx = -1;

            bool hasRows = false;
            using (NpgsqlCommand find = new NpgsqlCommand("SELECT id, name FROM leagues WHERE name = '" + leagueName + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
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
                using (NpgsqlCommand findInTeamsTable = new NpgsqlCommand("SELECT league_id, name FROM league_associations WHERE name = '" + leagueName + "';", pgConnection))
                {
                    using (NpgsqlDataReader dr = findInTeamsTable.ExecuteReader())
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
                using (NpgsqlCommand count = new NpgsqlCommand("select max(id) from leagues;", pgConnection))
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
                        string now = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                        using (NpgsqlCommand insert = new NpgsqlCommand("INSERT into leagues ( id, name, league_id, created_at, updated_at  ) VALUES (" + idx + ", '" + leagueName + "', " + idx + ", '" + now + "', '" + now + "');", pgConnection))
                        {
                            insert.ExecuteNonQuery();
                        }
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

        private static List<string> ExecuteSimpleQuery(string sql)
        {
            //check they are consecutive days over midnight
            var vals = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    while (dr.Read() == true)
                    {
                        vals.Add(dr[0].ToString());
                    }
                }
            }

            return vals;
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

        private bool renameLeague(string alias, string properName)
        {
            int test = 0;
            string aliasId = "";

            if (int.TryParse(alias.Trim(), out test))
            {
                //we have an id
                aliasId = alias.Trim();
            }
            else
            {
                aliasId = GetLeagueId(alias);
            }

            string properId = GetLeagueId(properName);

            if (properId == "")
            {
                //add new team;
                properId = AddLeague(properName);
                if (properId == "-1")
                {
                    MessageBox.Show("Failed to create new league:", properName);
                    return false;
                }
            }

            if (test == 0 && AddLeagueAssociation(alias, properName) == false)
            {
                return false;
            }

            string[] games = GetGamesForLeague(aliasId);

            foreach (var id in games)
            {
                using (NpgsqlCommand update = new NpgsqlCommand("UPDATE games SET league_id ='" + properId + "' WHERE id = '" + id + "';", pgConnection))
                {
                    update.ExecuteNonQuery();
                }
            }

            using (NpgsqlCommand delete = new NpgsqlCommand("DELETE from leagues where id = '" + aliasId + "';", pgConnection))
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

        private void todayButton_Click(object sender, EventArgs e)
        {
            matchBox.Items.Clear();

            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Substring(0, 10);
            string sql2 = "select id from games where to_char(kodate, 'YYYY-MM-DD') like '" + now + "%'";

            var ids = OneColumnQuery(sql2);

            Console.WriteLine(ids);

            List<Match> matches = new List<Match>();

            matches.Add(new Match() { homeTeam = "Home:", awayTeam = "Away:", league = "League:", kodate = "Kick Off:", gameId = "Id:" });

            string temp = "select g1.id, t1.name, t2.name, l1.name, g1.kodate from games g1 join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join leagues l1 on g1.league_id = l1.id where g1.id in (" + sql2 + ") order by g1.kodate asc; ";
            
            //string temp = "select g1.id, t1.name, t2.name, l1.name, g1.kodate, p1.\"goalswinhome\", p1.\"goalswinaway\", p1.\"goalslikelyscorehome\", p1.\"goalslikelyscoreaway\", p1.\"goalslikelyprobability\" from games g1 join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join prediction_data p1 on g1.id = p1.id join leagues l1 on g1.league_id = l1.id where g1.id in (" + sql2 + ") order by g1.kodate asc; ";
            //string temp3 = "select g1.id, p1.\"goalswinhome\", p1.\"goalswinaway\", p1.\"goalslikelyscorehome\", p1.\"goalslikelyscoreaway\", p1.\"goalslikelyprobability\" from games g1 join prediction_data p1 on g1.id = p1.id where g1.id in (" + sql2 + ");";

            using (NpgsqlCommand find = new NpgsqlCommand(temp, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var m = new Match()
                        {
                            gameId = dr[0].ToString(),
                            homeTeam = dr[1].ToString(),
                            awayTeam = dr[2].ToString(),
                            league = dr[3].ToString(),
                            kodate = dr[4].ToString()
                        };
                        matches.Add(m);
                    }
                }
            }

            //using (NpgsqlCommand find = new NpgsqlCommand(temp3, pgConnection))
            //{
            //    using (NpgsqlDataReader dr = find.ExecuteReader())
            //    {
            //        while (dr.Read())
            //        {
            //            var m = new Match()
            //            {
            //                gameId = dr[0].ToString(),
            //                homeTeam = dr[1].ToString(),
            //                awayTeam = dr[2].ToString(),
            //                league = dr[3].ToString(),
            //                kodate = dr[4].ToString()
            //            };
            //            matches.Add(m);
            //        }
            //    }
            //}

            int longestHomeTeam = matches.Select(x => x.homeTeam).Max(x => x.Length);
            int longestAwayTeam = matches.Select(x => x.awayTeam).Max(x => x.Length);
            int longestGameId = matches.Select(x => x.gameId).Max(x => x.Length);
            int longestLeague = matches.Select(x => x.league).Max(x => x.Length);

            var tempList = new List<string>();

            for (int i = 0; i < matches.Count(); ++i)
            {
                string temp2 = matches[i].gameId.PadRight(longestGameId + 1) + " " + matches[i].homeTeam.PadRight(longestHomeTeam + 1) + " " + matches[i].awayTeam.PadRight(longestAwayTeam) + " " + matches[i].league.PadRight(longestLeague) + " " + (i == 0 ? matches[i].kodate : matches[i].kodate.Substring(11, 5));
                tempList.Add(temp2);
            }

            string lineOne = tempList.ElementAt(0);
            tempList.RemoveAt(0);
            //tempList.Sort((x, y) => x.Substring(longestGameId).CompareTo(y.Substring(longestGameId)));

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

                try
                {
                    GetOldTeam(awayTeam, 2);
                    GetOldTeam(homeTeam);
                }
                catch (Exception ce)
                {

                    Console.WriteLine(ce);
                }
            }

            if (checkBox1.Checked)
            {
            }

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
            string sql2 = "select name from teams where name like '% Jnrs';";
            var inames = OneColumnQuery(sql2);

            int num = 0;
            foreach (var oldName in inames)
            {
                var newName = oldName.Replace(" Jnrs", " Juniors");

                if (GetTeamId(newName) != "")
                {
                    renameTeam(oldName, newName);
                    num++;

                }
            }

            MessageBox.Show("Performed " + num + " special actions"); ;

        }

        string moreSql = "select distinct team1, team2, EXTRACT(WEEK FROM kodate), count(*) from games group by team1, team2, EXTRACT(WEEK FROM kodate) having count(*) = 2;";

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
                string sql2 = "select id, league_id from games where team1='" + team1s.ElementAt(j)
                            + "' and team2='" + team2s.ElementAt(j)
                            + "' and to_char(kodate, 'DD/MM/YYYY') = '" + kodates.ElementAt(j).Substring(0, 10) + "';";

                var idsAndLeagues = new List<Tuple<string, string>>();

                using (NpgsqlCommand find = new NpgsqlCommand(sql2, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        bool hasRows = dr.HasRows;

                        if (hasRows == true)
                        {
                            while (dr.Read())
                            {
                                idsAndLeagues.Add(new Tuple<string, string>(dr[0].ToString(), dr[1].ToString()));
                            }
                        }
                    }
                }


                var justDelete = new List<string>();

                for (int t = 0; t < idsAndLeagues.Count(); ++t)
                {
                    var id = idsAndLeagues.ElementAt(t).Item1;
                    var counts = OneColumnQuery("select count(*) from statistics where game_id = '" + id + "';");
                    if (counts.ElementAt(0) == "0")
                    {
                        justDelete.Add(id);
                    }
                }

                if (idsAndLeagues.Count() > 1)
                {
                    string primaryId = "";
                    if (idsAndLeagues.Any(x => x.Item2 != "-1"))
                    {
                        primaryId = idsAndLeagues.Where(x => x.Item2 != "-1").First().Item1;
                    }

                    if (primaryId == "")
                    {
                        primaryId = idsAndLeagues.ElementAt(0).Item1;
                    }

                    for (int i = 1; i != idsAndLeagues.Count(); ++i)
                    {
                        string sqlTeam1 = "update statistics set game_id='" + primaryId + "' where game_id='" + idsAndLeagues.ElementAt(i).Item1 + "';";
                        string sqlDeleteTeam = "delete from games where id='" + idsAndLeagues.ElementAt(i).Item1 + "';";
                        if (justDelete.Contains(idsAndLeagues.ElementAt(i).Item1) == false)
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

        private void fixBET365LeaguesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var naughtyLeagues = new List<string>() { "723", "724", "132", "328", "834", "1124", "1343", "-1", "1202831", "3463650" };
            //var naughtyLeagues = new List<string>() {  "-1" };
            var naughtyLeagues  = GetLeagueId("All");

            string sql = "select id, team1, team2, kodate from games where league_id in ( " + String.Join(",", naughtyLeagues) + " ) order by id desc;";
            //string sql = "select id, team1, team2, kodate from games where league_id in ( 723, 1202831, 3463650 ) order by id desc;";

            var bet365games = new List<string>();

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            bet365games.Add(dr[0].ToString() + "," + dr[1].ToString() + "," + dr[2].ToString() + "," + dr[3].ToString());
                        }
                    }
                }
            }

            var fails = 0;
            var successes = 0;

            foreach (var game in bet365games)
            {
                var splits = game.Split(',');

                string team1LeaguesSQL = "select league_id from games where team1 = " + splits[1] + " or team2 = " + splits[1] + " order by kodate desc;";
                string team2LeaguesSQL = "select league_id from games where team1 = " + splits[2] + " or team2 = " + splits[2] + " order by kodate desc;";

                var team1Leagues = OneColumnQuery(team1LeaguesSQL).Distinct().Where(x => naughtyLeagues.Contains(x) == false);
                var team2Leagues = OneColumnQuery(team2LeaguesSQL).Distinct().Where(x => naughtyLeagues.Contains(x) == false); ;

                var common = team1Leagues.Intersect(team2Leagues);

                if (common.Count() != 0)
                {
                    var newLeague = common.First();
                    var sqlUpdate = "update games set league_id = '" + newLeague + "' where id = " + splits[0] + ";";

                    ExecuteNonQuery(sqlUpdate);
                    successes++;
                }
                else
                {
                    fails++;
                }
            }

            Console.WriteLine(fails);
        }


        private void removeDupMatchesOverMidnightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Tuple<string, string>> teams = new List<Tuple<string, string>>();

            string sql = "select distinct team1, team2, EXTRACT(WEEK FROM kodate), count(*) from games group by team1, team2, EXTRACT(WEEK FROM kodate) having count(*) = 2;";

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        while (dr.Read())
                        {
                            var tup = new Tuple<string, string>(dr[0].ToString(), dr[1].ToString());

                            teams.Add(tup);
                        }
                    }
                }
            }

            for (int j = 0; j != teams.Count(); ++j)
            {
                string sql2 = "select id, league_id from games where team1='" + teams.ElementAt(j).Item1
                            + "' and team2='" + teams.ElementAt(j).Item2 + "';";

                var idsAndLeagues = new List<Tuple<string, string>>();

                using (NpgsqlCommand find = new NpgsqlCommand(sql2, pgConnection))
                {
                    using (NpgsqlDataReader dr = find.ExecuteReader())
                    {
                        bool hasRows = dr.HasRows;

                        if (hasRows == true)
                        {
                            while (dr.Read())
                            {
                                idsAndLeagues.Add(new Tuple<string, string>(dr[0].ToString(), dr[1].ToString()));
                            }
                        }
                    }
                }

                if (idsAndLeagues.Count() != 2)
                {
                    continue;
                }
                else
                {
                    var kodates1 = ExecuteSimpleQuery("select kodate from games where id='" + idsAndLeagues.ElementAt(0).Item1 + "'");
                    var kodates2 = ExecuteSimpleQuery("select kodate from games where id='" + idsAndLeagues.ElementAt(1).Item1 + "'");

                    var kodate1 = DateTime.Parse(kodates1.First());
                    var kodate2 = DateTime.Parse(kodates2.First());

                    if ((kodate1 - kodate2).TotalHours > 4)
                    {
                        //MessageBox.Show("Dates too far apart: " + kodate1 + " - " + kodate2);
                        continue;
                    }
                }

                if (idsAndLeagues.Count() == 2)
                {
                    string primaryId = "";

                    if (primaryId == "")
                    {
                        primaryId = idsAndLeagues.ElementAt(0).Item1;
                    }


                    string sqlTeam1 = "update statistics set game_id='" + primaryId + "' where game_id='" + idsAndLeagues.ElementAt(1).Item1 + "';";
                    string sqlDeleteTeam = "delete from games where id='" + idsAndLeagues.ElementAt(1).Item1 + "';";

                    ExecuteNonQuery(sqlTeam1);
                    ExecuteNonQuery(sqlDeleteTeam);
                }
            }

            matchBox2.Items.Clear();


            for (int j = 0; j != teams.Count(); ++j)
            {
                matchBox2.Items.Add(teams.ElementAt(j).Item1 + " v " + teams.ElementAt(j).Item2);
                //    string newName = name.Replace(" Ladies", " Women");
                //    renameTeam(name, newName);
            }
        }

        private void deleteOldGamesWithNoStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ids = ExecuteSimpleQuery("SELECT count(g1.id) FROM games g1 LEFT JOIN statistics s1 ON s1.game_id = g1.id WHERE s1.game_id IS NULL and g1.kodate < current_date;");

        }

        private void special2ButtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ids = ExecuteSimpleQuery("SELECT id from games");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            WebRequest req = WebRequest.Create("http://localhost:8080/GetCornersPredictionWithDepth?gameId=" + id + "&depth=300");
            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            MessageBox.Show(sr.ReadToEnd().Trim());
        }

        public void RunSQL(string sql, Action<DbDataReader> a)
        {
            try
            {
                using (DbCommand cmd = new NpgsqlCommand(sql, pgConnection))
                {
                    using (DbDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            a(dr);
                        }

                        dr.Close();
                    }
                }
            }
            catch (DbException e)
            {
                MessageBox.Show("Exception: " + e);
            }
        }

        class Prediction
        {
            public string id;
            public string team1;
            public string team2;
            public int cornerslikelyscorehome;
            public int cornerslikelyscoreaway;
            public float cornerLine = -1;
            public DateTime koDate;

            internal bool BetDeJour()
            {
                bool retVal = false;

                int totalCorners = cornerslikelyscoreaway + cornerslikelyscorehome;

                if (Math.Abs(totalCorners - cornerLine) > 2)
                    retVal = true;

                return retVal;
            }
        }

        private void betDeJourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sql_preds = "select p1.id, t1.name, t2.name, p1.cornerslikelyscorehome, p1.cornerslikelyscoreaway, a1.cornerline, g1.kodate from prediction_data p1 join games g1 on g1.id = p1.id join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join asiancorners a1 on CAST(a1.game_id AS integer) = g1.id where g1.kodate > current_date order by g1.kodate asc;";
            var preds = new List<Prediction>();

            RunSQL(sql_preds,
                    (dr) =>
                    {
                        Prediction p = new Prediction();
                        p.id = dr[0].ToString();
                        p.team1 = dr[1].ToString();
                        p.team2 = dr[2].ToString();

                        p.cornerslikelyscorehome = int.Parse(dr[3].ToString());
                        p.cornerslikelyscoreaway = int.Parse(dr[4].ToString());

                        p.cornerLine = float.Parse(dr[5].ToString());

                        p.koDate = DateTime.Parse(dr[6].ToString());
                        preds.Add(p);
                    }
                );

            preds.RemoveAll(x => x.cornerLine == -1);
            preds.RemoveAll(x => x.BetDeJour() == false);
            preds.OrderBy(x => x.koDate);

            int longestHomeTeam = preds.Select(x => x.team1).Max(x => x.Length);
            int longestAwayTeam = preds.Select(x => x.team2).Max(x => x.Length);
 
            int longestLines = preds.Select(x => x.cornerLine).Max(x => x.ToString().Length);

            matchBox2.Items.Clear();

            for(int i = 0; i != preds.Count(); ++i)
            {
                matchBox2.Items.Add(preds.ElementAt(i).id + "\t" + preds.ElementAt(i).team1.PadRight(longestHomeTeam) + "\t" + preds.ElementAt(i).team2.PadRight(longestAwayTeam) + "\t\tPredicted: " + preds.ElementAt(i).cornerslikelyscoreaway + "-" + preds.ElementAt(i).cornerslikelyscorehome + "\tLine: " + preds.ElementAt(i).cornerLine.ToString().PadRight(longestLines) + "\t" + preds.ElementAt(i).koDate);
            }
        }

        private void analysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gameIds = ExecuteSimpleQuery("select id from prediction_data");
            var gameIdsString = string.Join(",", gameIds);

            string sql4 = "select hg, ag, hco, aco, gametime, game_id from " +
              "(select game_id, gametime, hg, ag, hco, aco, max(gametime) over (partition by game_id) max_gameTime from statistics where game_id in  (  " + gameIdsString + " ))" +
              " a where game_id in  (  " + gameIdsString + " ) AND gametime = max_GameTime";

            string hg, ag, hc, ac, ls, g_id;

            var matches = new List<Match>();

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

            var sql_preds = "select p1.id, t1.name, t2.name, p1.cornerslikelyscorehome, p1.cornerslikelyscoreaway, a1.cornerline, g1.kodate from prediction_data p1 join games g1 on g1.id = p1.id join teams t1 on g1.team1 = t1.id join teams t2 on g1.team2 = t2.id join asiancorners a1 on CAST(a1.game_id AS integer) = g1.id where g1.kodate > current_date order by g1.kodate asc;";
            var preds = new List<Prediction>();

            RunSQL(sql_preds,
                    (dr) =>
                    {
                        Prediction p = new Prediction();
                        p.id = dr[0].ToString();
                        p.team1 = dr[1].ToString();
                        p.team2 = dr[2].ToString();

                        p.cornerslikelyscorehome = int.Parse(dr[3].ToString());
                        p.cornerslikelyscoreaway = int.Parse(dr[4].ToString());

                        p.cornerLine = float.Parse(dr[5].ToString());

                        p.koDate = DateTime.Parse(dr[6].ToString());
                        preds.Add(p);
                    }
                );
        }

        private void removeDuplicateLeaguesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> badNames = new List<string>();

            string sql = "select name, count(*) from leagues group by name having count(*) > 1;";

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
                var ids = GetLeagueIds(name);

                if (ids.Count() > 1)
                {
                    string primaryId = ids.ElementAt(0);
                    for (int i = 1; i != ids.Count(); ++i)
                    {
                        string sqlLeague1 = "update games set league_id='" + primaryId + "' where league_id='" + ids.ElementAt(i) + "';";
                        string sqlDeleteTeam = "delete from leagues where id='" + ids.ElementAt(i) + "';";
                        ExecuteNonQuery(sqlLeague1);
                        ExecuteNonQuery(sqlDeleteTeam);
                    }

                }
            }

            matchBox2.Items.Clear();

            foreach (var name in badNames)
            {
                matchBox2.Items.Add(name);
            }
        }

        private void simpleGoalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            WebRequest req = WebRequest.Create("http://localhost:8080/GetGoalsPrediction?gameId=" + id);
            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            MessageBox.Show(sr.ReadToEnd().Trim());
        }

        private void simpleCornerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            WebRequest req = WebRequest.Create("http://localhost:8080/GetCornersPrediction?gameId=" + id);
            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            MessageBox.Show(sr.ReadToEnd().Trim());
        }

        private void bivariateGoalPrediction_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            WebRequest req = WebRequest.Create("http://localhost:8080/GetGoalsBiVarPrediction?gameId=" + id);
            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            MessageBox.Show(sr.ReadToEnd().Trim());
        }

        private void bivariateCornerPrediction_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            WebRequest req = WebRequest.Create("http://localhost:8080/GetCornersBiVarPrediction?gameId=" + id);
            req.Timeout = 300000;
            WebResponse resp = req.GetResponse();

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());

            MessageBox.Show(sr.ReadToEnd().Trim());
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
