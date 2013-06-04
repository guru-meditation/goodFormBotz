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
        string connectionString = "Database=d7menjp3rap4ts;Server=ec2-54-235-155-182.compute-1.amazonaws.com;Port=5432;User Id=leupjwfvjinxsi;Password=HACn2POfVhsUY9S5HUsV7DhgS_;SSL=true";
        //string connectionString = "Database=deg5ivhqu73n1i;Server=ec2-54-243-181-184.compute-1.amazonaws.com;Port=5432;User Id=mjkscoveqvuszj;Password=qj1TBKCPuVxeCAR2sT79uIHAqT;SSL=true";

        public FormzDBEditor()
        {
            InitializeComponent();

            try
            {
                pgConnection = new NpgsqlConnection(connectionString);
                pgConnection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
                pgConnection = null;
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
            GetOldTeam();
        }

        private void GetOldTeam()
        {
            string teamName = teamBox1.Text;

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

            matchBox.Items.Clear();

            if (id == "")
            {
                MessageBox.Show(teamName + " does not exist");
                return;
            }

            string sql = "select teams.name, leagues.name, games.id from games join teams on games.team2 = teams.id join leagues on games.league_id = leagues.id where games.id in ( SELECT id FROM games WHERE team1 = '" + id + "');";

            int maxHomeTeamLength = teamName.Length;
            int maxAwayTeamLength = teamName.Length;

            List<string> homeTeams = new List<string>() { "Home:" };
            List<string> awayTeams = new List<string>() { "Away:" };
            List<string> leagues = new List<string>() { "League:" };
            List<string> gameIds = new List<string>() { "Id:" };
            List<string> hgsList = new List<string>() { "HG:" };
            List<string> agsList = new List<string>() { "AG:" };
            List<string> hcsList = new List<string>() { "HC:" };
            List<string> acsList = new List<string>() { "AC:" };
            List<string> seenTimes = new List<string>() { "Seen:" };

            using (NpgsqlCommand find = new NpgsqlCommand(sql, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    bool hasRows = dr.HasRows;

                    if (hasRows == true)
                    {
                        string matchIds = "";
                        while (dr.Read())
                        {
                            string awayTeam = dr[0].ToString();
                            string league = dr[1].ToString();
                            string gameId = dr[2].ToString();

                            homeTeams.Add(teamName);
                            awayTeams.Add(awayTeam);
                            leagues.Add(league);
                            gameIds.Add(gameId);
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
                        string matchIds = "";
                        while (dr.Read())
                        {
                            string homeTeam = dr[0].ToString();
                            string league = dr[1].ToString();
                            string gameId = dr[2].ToString();

                            homeTeams.Add(homeTeam);
                            awayTeams.Add(teamName);
                            leagues.Add(league);
                            gameIds.Add(gameId);
                        }
                    }
                }
            }

            if (getStats.Checked)
            {
                foreach (var gameId in gameIds)
                {
                    string hg, ag, hc, ac, ls;
                    if (gameId == gameIds[0])
                    {
                        continue;
                    }

                    if (GetLastStat(gameId, out hg, out ag, out hc, out ac, out ls))
                    {
                        hgsList.Add(hg);
                        agsList.Add(ag);
                        hcsList.Add(hc);
                        acsList.Add(ac);
                        seenTimes.Add(ls);
                    }
                }
            }

            int longestHomeTeam = homeTeams.Max(x => x.Length);
            int longestAwayTeam = awayTeams.Max(x => x.Length);
            int longestGameId = gameIds.Max(x => x.Length);
            int longestLeagueId = leagues.Max(x => x.Length);
            int longesthg = hgsList.Max(x => x.Length);
            int longestag = agsList.Max(x => x.Length);
            int longesthc = hcsList.Max(x => x.Length);
            int longestac = acsList.Max(x => x.Length);

            for (int i = 0; i < homeTeams.Count(); ++i)
            {
                if (getStats.Checked)
                {
                    matchBox.Items.Add(gameIds[i].PadRight(longestGameId + 1) + " "
                        + homeTeams[i].PadRight(longestHomeTeam + 1) + " "
                        + awayTeams[i].PadRight(longestAwayTeam) + " "
                        + leagues[i].PadRight(longestLeagueId + 2) + " "
                        + hgsList[i].PadRight(longesthg + 1) + " "
                        + agsList[i].PadRight(longestag + 1) + " "
                        + hcsList[i].PadRight(longesthc + 1) + " "
                        + acsList[i].PadRight(longestac + 1) + " "
                        + seenTimes[i]);
                }
                else
                {
                    matchBox.Items.Add(gameIds[i].PadRight(longestGameId + 1) + " "
                        + homeTeams[i].PadRight(longestHomeTeam + 1) + " "
                        + awayTeams[i].PadRight(longestAwayTeam) + " "
                        + leagues[i]);
                }
            }

            idBox2.Text = id;
        }

        private bool GetLastStat(string gameId, out string hg, out string ag, out string hc, out string ac, out string ls)
        {
            string sql2 = "select min(gametime) from statistics where game_id = '" + gameId + "';";

            string lastTime = "";

            hg = ""; ag = ""; hc = ""; ac = ""; ls = "";

            using (NpgsqlCommand find = new NpgsqlCommand(sql2, pgConnection))
            {
                using (NpgsqlDataReader dr = find.ExecuteReader())
                {
                    if (dr.HasRows == true)
                    {
                        dr.Read();
                        lastTime = dr[0].ToString();
                    }
                }
            }

            if (lastTime != "")
            {
                if (lastTime != "-2")
                {
                    string sql3 = "select max(gametime) from statistics where game_id = '" + gameId + "';";

                    using (NpgsqlCommand find = new NpgsqlCommand(sql3, pgConnection))
                    {
                        using (NpgsqlDataReader dr = find.ExecuteReader())
                        {
                            if (dr.HasRows == true)
                            {
                                dr.Read();
                                lastTime = dr[0].ToString();
                            }
                        }
                    }
                }

                string sql4 = "select hg, ag, hco, aco from statistics where game_id = '" + gameId + "' AND gametime = '" + lastTime + "';";

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
                GetOldTeam();
            }
        }

        private bool AddTeamAssociation(string alias, string teamName)
        {
            string id = "";

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
            string id = "";

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
            int numItems = matchBox.Items.Count;

            for (int i = 0; i != numItems; ++i)
            {
                if (matchBox.GetSelected(i) == true)
                {
                    string selectedText = "";
                    try
                    {
                        selectedText = matchBox.GetItemText(matchBox.Items[i]);
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
                GetOldTeam();
            }
        }

        private void deleteGame_Click(object sender, EventArgs e)
        {
            string selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
            string id = Regex.Split(selectedText, " ").ElementAt(0);

            using (NpgsqlCommand insert = new NpgsqlCommand("DELETE from games WHERE id = '" + id + "';", pgConnection))
            {
                using (NpgsqlDataReader dr = insert.ExecuteReader())
                {
                    insert.ExecuteNonQuery();
                }
            }

            GetOldTeam();
        }

        private void button2_Click(object sender, EventArgs e)
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

            matchBox.Items.Clear();


            foreach (var name in badNames)
            {
                matchBox.Items.Add(name);
                //    string newName = name.Replace(" Ladies", " Women");
                //    renameTeam(name, newName);
            }
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

            matchBox.Items.Clear();

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
                matchBox.Items.Add(gameIds[i].PadRight(longestGameId + 1) + " " + homeTeams[i].PadRight(longestHomeTeam + 1) + " " + awayTeams[i].PadRight(longestAwayTeam) + " " + leagues[i]);
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

        private void matchBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedText = "";
            try
            {
                selectedText = matchBox.GetItemText(matchBox.Items[matchBox.SelectedIndex]);
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
            List<string> team1s = new List<string>();
            List<string> team2s = new List<string>();
            List<string> kodates = new List<string>();

            string sql = "select team1, team2, kodate, count(*) from games group by team1, team2, kodate having count(*) > 1;";

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
                            + "' and kodate='" + kodates.ElementAt(j) + "';";

                var ids = OneColumnQuery(sql2);

                var justDelete = new List<string>();

                foreach (var id in ids)
                {
                    var counts = OneColumnQuery("select count(*) from statistics where game_id = '" + id + "';");
                    if(counts.ElementAt(0) == "0")
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

            matchBox.Items.Clear();


            for (int j = 0; j != team1s.Count(); ++j)
            {
                matchBox.Items.Add(kodates.ElementAt(j) + " " + team1s.ElementAt(j) + " v " + team2s.ElementAt(j));
                //    string newName = name.Replace(" Ladies", " Women");
                //    renameTeam(name, newName);
            }
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

    }
}
