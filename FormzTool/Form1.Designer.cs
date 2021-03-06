﻿namespace FormzTool
{
    partial class FormzDBEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.teamBox1 = new System.Windows.Forms.TextBox();
            this.getButton1 = new System.Windows.Forms.Button();
            this.matchBox = new System.Windows.Forms.ListBox();
            this.idBox2 = new System.Windows.Forms.TextBox();
            this.idLabel = new System.Windows.Forms.Label();
            this.Rename = new System.Windows.Forms.Label();
            this.leagueLabel = new System.Windows.Forms.Label();
            this.leagueBox1 = new System.Windows.Forms.TextBox();
            this.renameTeamButton = new System.Windows.Forms.Button();
            this.renameBox1 = new System.Windows.Forms.TextBox();
            this.setLeague = new System.Windows.Forms.Button();
            this.deleteGame = new System.Windows.Forms.Button();
            this.getLeague = new System.Windows.Forms.Button();
            this.getStats = new System.Windows.Forms.CheckBox();
            this.newLeagueBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.renameLeagueButton = new System.Windows.Forms.Button();
            this.createLeague = new System.Windows.Forms.Button();
            this.todayButton = new System.Windows.Forms.Button();
            this.matchBox2 = new System.Windows.Forms.ListBox();
            this.matchBox3 = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.File = new System.Windows.Forms.ToolStripMenuItem();
            this.specialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDupTeams = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDuplicateMatchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tomorrowsGamesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.specialButtonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixBET365LeaguesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDupMatchesOverMidnightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteOldGamesWithNoStatsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.special2ButtonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.betDeJourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDuplicateLeaguesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.predictionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simpleGoalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simpleCornerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bivariateGoalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bivariateCornerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.doSums = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Old Team:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // teamBox1
            // 
            this.teamBox1.Location = new System.Drawing.Point(87, 38);
            this.teamBox1.Name = "teamBox1";
            this.teamBox1.Size = new System.Drawing.Size(775, 20);
            this.teamBox1.TabIndex = 1;
            // 
            // getButton1
            // 
            this.getButton1.Location = new System.Drawing.Point(868, 38);
            this.getButton1.Name = "getButton1";
            this.getButton1.Size = new System.Drawing.Size(189, 23);
            this.getButton1.TabIndex = 2;
            this.getButton1.Text = "Get Team From Heroku DB";
            this.getButton1.UseVisualStyleBackColor = true;
            this.getButton1.Click += new System.EventHandler(this.getButton1_Click);
            // 
            // matchBox
            // 
            this.matchBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.matchBox.FormattingEnabled = true;
            this.matchBox.ItemHeight = 14;
            this.matchBox.Location = new System.Drawing.Point(21, 220);
            this.matchBox.Name = "matchBox";
            this.matchBox.Size = new System.Drawing.Size(1036, 214);
            this.matchBox.TabIndex = 3;
            this.matchBox.SelectedIndexChanged += new System.EventHandler(this.matchBox_SelectedIndexChanged);
            // 
            // idBox2
            // 
            this.idBox2.Location = new System.Drawing.Point(87, 154);
            this.idBox2.Name = "idBox2";
            this.idBox2.Size = new System.Drawing.Size(70, 20);
            this.idBox2.TabIndex = 4;
            // 
            // idLabel
            // 
            this.idLabel.AutoSize = true;
            this.idLabel.Location = new System.Drawing.Point(26, 158);
            this.idLabel.Name = "idLabel";
            this.idLabel.Size = new System.Drawing.Size(51, 13);
            this.idLabel.TabIndex = 5;
            this.idLabel.Text = "Team ID:";
            // 
            // Rename
            // 
            this.Rename.AutoSize = true;
            this.Rename.Location = new System.Drawing.Point(15, 69);
            this.Rename.Name = "Rename";
            this.Rename.Size = new System.Drawing.Size(62, 13);
            this.Rename.TabIndex = 7;
            this.Rename.Text = "New Team:";
            // 
            // leagueLabel
            // 
            this.leagueLabel.AutoSize = true;
            this.leagueLabel.Location = new System.Drawing.Point(12, 98);
            this.leagueLabel.Name = "leagueLabel";
            this.leagueLabel.Size = new System.Drawing.Size(65, 13);
            this.leagueLabel.TabIndex = 8;
            this.leagueLabel.Text = "Old League:";
            // 
            // leagueBox1
            // 
            this.leagueBox1.Location = new System.Drawing.Point(87, 95);
            this.leagueBox1.Name = "leagueBox1";
            this.leagueBox1.Size = new System.Drawing.Size(591, 20);
            this.leagueBox1.TabIndex = 9;
            // 
            // renameTeamButton
            // 
            this.renameTeamButton.Location = new System.Drawing.Point(868, 89);
            this.renameTeamButton.Name = "renameTeamButton";
            this.renameTeamButton.Size = new System.Drawing.Size(189, 26);
            this.renameTeamButton.TabIndex = 10;
            this.renameTeamButton.Text = "Rename Team";
            this.renameTeamButton.UseVisualStyleBackColor = true;
            this.renameTeamButton.Click += new System.EventHandler(this.renameButton_Click);
            // 
            // renameBox1
            // 
            this.renameBox1.Location = new System.Drawing.Point(87, 66);
            this.renameBox1.Name = "renameBox1";
            this.renameBox1.Size = new System.Drawing.Size(775, 20);
            this.renameBox1.TabIndex = 11;
            // 
            // setLeague
            // 
            this.setLeague.Location = new System.Drawing.Point(247, 151);
            this.setLeague.Name = "setLeague";
            this.setLeague.Size = new System.Drawing.Size(70, 26);
            this.setLeague.TabIndex = 12;
            this.setLeague.Text = "Set League";
            this.setLeague.UseVisualStyleBackColor = true;
            this.setLeague.Click += new System.EventHandler(this.button1_Click);
            // 
            // deleteGame
            // 
            this.deleteGame.Location = new System.Drawing.Point(163, 151);
            this.deleteGame.Name = "deleteGame";
            this.deleteGame.Size = new System.Drawing.Size(78, 26);
            this.deleteGame.TabIndex = 13;
            this.deleteGame.Text = "Delete Game";
            this.deleteGame.UseVisualStyleBackColor = true;
            this.deleteGame.Click += new System.EventHandler(this.deleteGame_Click);
            // 
            // getLeague
            // 
            this.getLeague.Location = new System.Drawing.Point(868, 62);
            this.getLeague.Name = "getLeague";
            this.getLeague.Size = new System.Drawing.Size(189, 26);
            this.getLeague.TabIndex = 15;
            this.getLeague.Text = "Get League From Heroku DB";
            this.getLeague.UseVisualStyleBackColor = true;
            this.getLeague.Click += new System.EventHandler(this.getLeague_Click);
            // 
            // getStats
            // 
            this.getStats.AutoSize = true;
            this.getStats.Location = new System.Drawing.Point(969, 156);
            this.getStats.Name = "getStats";
            this.getStats.Size = new System.Drawing.Size(88, 17);
            this.getStats.TabIndex = 17;
            this.getStats.Text = "Get Statistics";
            this.getStats.UseVisualStyleBackColor = true;
            // 
            // newLeagueBox1
            // 
            this.newLeagueBox1.Location = new System.Drawing.Point(87, 125);
            this.newLeagueBox1.Name = "newLeagueBox1";
            this.newLeagueBox1.Size = new System.Drawing.Size(775, 20);
            this.newLeagueBox1.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "New League:";
            // 
            // renameLeagueButton
            // 
            this.renameLeagueButton.Location = new System.Drawing.Point(868, 118);
            this.renameLeagueButton.Name = "renameLeagueButton";
            this.renameLeagueButton.Size = new System.Drawing.Size(189, 26);
            this.renameLeagueButton.TabIndex = 20;
            this.renameLeagueButton.Text = "Rename League";
            this.renameLeagueButton.UseVisualStyleBackColor = true;
            this.renameLeagueButton.Click += new System.EventHandler(this.renameLeague_Click_1);
            // 
            // createLeague
            // 
            this.createLeague.Location = new System.Drawing.Point(323, 151);
            this.createLeague.Name = "createLeague";
            this.createLeague.Size = new System.Drawing.Size(92, 26);
            this.createLeague.TabIndex = 21;
            this.createLeague.Text = "Create League";
            this.createLeague.UseVisualStyleBackColor = true;
            this.createLeague.Click += new System.EventHandler(this.createLeague_Click);
            // 
            // todayButton
            // 
            this.todayButton.Location = new System.Drawing.Point(684, 90);
            this.todayButton.Name = "todayButton";
            this.todayButton.Size = new System.Drawing.Size(178, 25);
            this.todayButton.TabIndex = 24;
            this.todayButton.Text = "Todays Games";
            this.todayButton.UseVisualStyleBackColor = true;
            this.todayButton.Click += new System.EventHandler(this.todayButton_Click);
            // 
            // matchBox2
            // 
            this.matchBox2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.matchBox2.FormattingEnabled = true;
            this.matchBox2.ItemHeight = 14;
            this.matchBox2.Location = new System.Drawing.Point(21, 442);
            this.matchBox2.Name = "matchBox2";
            this.matchBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.matchBox2.Size = new System.Drawing.Size(1036, 256);
            this.matchBox2.TabIndex = 25;
            // 
            // matchBox3
            // 
            this.matchBox3.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.matchBox3.FormattingEnabled = true;
            this.matchBox3.ItemHeight = 14;
            this.matchBox3.Location = new System.Drawing.Point(21, 704);
            this.matchBox3.Name = "matchBox3";
            this.matchBox3.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.matchBox3.Size = new System.Drawing.Size(1036, 256);
            this.matchBox3.TabIndex = 26;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.File,
            this.specialToolStripMenuItem,
            this.predictionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1074, 24);
            this.menuStrip1.TabIndex = 28;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // File
            // 
            this.File.Name = "File";
            this.File.Size = new System.Drawing.Size(37, 20);
            this.File.Text = "File";
            // 
            // specialToolStripMenuItem
            // 
            this.specialToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeDupTeams,
            this.removeDuplicateMatchesToolStripMenuItem,
            this.tomorrowsGamesToolStripMenuItem,
            this.specialButtonToolStripMenuItem,
            this.fixBET365LeaguesToolStripMenuItem,
            this.removeDupMatchesOverMidnightToolStripMenuItem,
            this.deleteOldGamesWithNoStatsToolStripMenuItem,
            this.special2ButtonToolStripMenuItem,
            this.betDeJourToolStripMenuItem,
            this.analysisToolStripMenuItem,
            this.removeDuplicateLeaguesToolStripMenuItem});
            this.specialToolStripMenuItem.Name = "specialToolStripMenuItem";
            this.specialToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.specialToolStripMenuItem.Text = "Special";
            // 
            // removeDupTeams
            // 
            this.removeDupTeams.Name = "removeDupTeams";
            this.removeDupTeams.Size = new System.Drawing.Size(270, 22);
            this.removeDupTeams.Text = "Remove Duplicate Teams";
            this.removeDupTeams.Click += new System.EventHandler(this.removeDuplicateTeamsToolStripMenuItem_Click);
            // 
            // removeDuplicateMatchesToolStripMenuItem
            // 
            this.removeDuplicateMatchesToolStripMenuItem.Name = "removeDuplicateMatchesToolStripMenuItem";
            this.removeDuplicateMatchesToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.removeDuplicateMatchesToolStripMenuItem.Text = "Remove Duplicate Matches";
            this.removeDuplicateMatchesToolStripMenuItem.Click += new System.EventHandler(this.removeDuplicateMatchesToolStripMenuItem_Click);
            // 
            // tomorrowsGamesToolStripMenuItem
            // 
            this.tomorrowsGamesToolStripMenuItem.Name = "tomorrowsGamesToolStripMenuItem";
            this.tomorrowsGamesToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.tomorrowsGamesToolStripMenuItem.Text = "Tomorrows Games";
            this.tomorrowsGamesToolStripMenuItem.Click += new System.EventHandler(this.tomorrowsGamesToolStripMenuItem_Click);
            // 
            // specialButtonToolStripMenuItem
            // 
            this.specialButtonToolStripMenuItem.Name = "specialButtonToolStripMenuItem";
            this.specialButtonToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.specialButtonToolStripMenuItem.Text = "Special Button";
            this.specialButtonToolStripMenuItem.Click += new System.EventHandler(this.specialButtonToolStripMenuItem_Click);
            // 
            // fixBET365LeaguesToolStripMenuItem
            // 
            this.fixBET365LeaguesToolStripMenuItem.Name = "fixBET365LeaguesToolStripMenuItem";
            this.fixBET365LeaguesToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.fixBET365LeaguesToolStripMenuItem.Text = "Fix BET365 leagues";
            this.fixBET365LeaguesToolStripMenuItem.Click += new System.EventHandler(this.fixBET365LeaguesToolStripMenuItem_Click);
            // 
            // removeDupMatchesOverMidnightToolStripMenuItem
            // 
            this.removeDupMatchesOverMidnightToolStripMenuItem.Name = "removeDupMatchesOverMidnightToolStripMenuItem";
            this.removeDupMatchesOverMidnightToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.removeDupMatchesOverMidnightToolStripMenuItem.Text = "Remove Dup Matches Over Midnight";
            this.removeDupMatchesOverMidnightToolStripMenuItem.Click += new System.EventHandler(this.removeDupMatchesOverMidnightToolStripMenuItem_Click);
            // 
            // deleteOldGamesWithNoStatsToolStripMenuItem
            // 
            this.deleteOldGamesWithNoStatsToolStripMenuItem.Name = "deleteOldGamesWithNoStatsToolStripMenuItem";
            this.deleteOldGamesWithNoStatsToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.deleteOldGamesWithNoStatsToolStripMenuItem.Text = "Delete Old Games With No Stats";
            this.deleteOldGamesWithNoStatsToolStripMenuItem.Click += new System.EventHandler(this.deleteOldGamesWithNoStatsToolStripMenuItem_Click);
            // 
            // special2ButtonToolStripMenuItem
            // 
            this.special2ButtonToolStripMenuItem.Name = "special2ButtonToolStripMenuItem";
            this.special2ButtonToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.special2ButtonToolStripMenuItem.Text = "Special 2 Button";
            this.special2ButtonToolStripMenuItem.Click += new System.EventHandler(this.special2ButtonToolStripMenuItem_Click);
            // 
            // betDeJourToolStripMenuItem
            // 
            this.betDeJourToolStripMenuItem.Name = "betDeJourToolStripMenuItem";
            this.betDeJourToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.betDeJourToolStripMenuItem.Text = "Bet De Jour";
            this.betDeJourToolStripMenuItem.Click += new System.EventHandler(this.betDeJourToolStripMenuItem_Click);
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.Name = "analysisToolStripMenuItem";
            this.analysisToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.analysisToolStripMenuItem.Text = "Analysis";
            this.analysisToolStripMenuItem.Click += new System.EventHandler(this.analysisToolStripMenuItem_Click);
            // 
            // removeDuplicateLeaguesToolStripMenuItem
            // 
            this.removeDuplicateLeaguesToolStripMenuItem.Name = "removeDuplicateLeaguesToolStripMenuItem";
            this.removeDuplicateLeaguesToolStripMenuItem.Size = new System.Drawing.Size(270, 22);
            this.removeDuplicateLeaguesToolStripMenuItem.Text = "Remove Duplicate Leagues";
            this.removeDuplicateLeaguesToolStripMenuItem.Click += new System.EventHandler(this.removeDuplicateLeaguesToolStripMenuItem_Click);
            // 
            // predictionsToolStripMenuItem
            // 
            this.predictionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.simpleGoalToolStripMenuItem,
            this.simpleCornerToolStripMenuItem,
            this.bivariateGoalToolStripMenuItem,
            this.bivariateCornerToolStripMenuItem});
            this.predictionsToolStripMenuItem.Name = "predictionsToolStripMenuItem";
            this.predictionsToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.predictionsToolStripMenuItem.Text = "Predictions";
            // 
            // simpleGoalToolStripMenuItem
            // 
            this.simpleGoalToolStripMenuItem.Name = "simpleGoalToolStripMenuItem";
            this.simpleGoalToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.simpleGoalToolStripMenuItem.Text = "Simple Goal";
            this.simpleGoalToolStripMenuItem.Click += new System.EventHandler(this.simpleGoalToolStripMenuItem_Click);
            // 
            // simpleCornerToolStripMenuItem
            // 
            this.simpleCornerToolStripMenuItem.Name = "simpleCornerToolStripMenuItem";
            this.simpleCornerToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.simpleCornerToolStripMenuItem.Text = "Simple Corner";
            this.simpleCornerToolStripMenuItem.Click += new System.EventHandler(this.simpleCornerToolStripMenuItem_Click);
            // 
            // bivariateGoalToolStripMenuItem
            // 
            this.bivariateGoalToolStripMenuItem.Name = "bivariateGoalToolStripMenuItem";
            this.bivariateGoalToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.bivariateGoalToolStripMenuItem.Text = "Bivariate Goal";
            this.bivariateGoalToolStripMenuItem.Click += new System.EventHandler(this.bivariateGoalPrediction_Click);
            // 
            // bivariateCornerToolStripMenuItem
            // 
            this.bivariateCornerToolStripMenuItem.Name = "bivariateCornerToolStripMenuItem";
            this.bivariateCornerToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.bivariateCornerToolStripMenuItem.Text = "Bivariate Corner";
            this.bivariateCornerToolStripMenuItem.Click += new System.EventHandler(this.bivariateCornerPrediction_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(868, 156);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(94, 17);
            this.checkBox1.TabIndex = 29;
            this.checkBox1.Text = "Do The Maths";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // doSums
            // 
            this.doSums.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.doSums.Location = new System.Drawing.Point(139, 9);
            this.doSums.Name = "doSums";
            this.doSums.Size = new System.Drawing.Size(88, 25);
            this.doSums.TabIndex = 30;
            this.doSums.Text = "Goal Prediction";
            this.doSums.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Location = new System.Drawing.Point(233, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 25);
            this.button1.TabIndex = 31;
            this.button1.Text = "Corners Prediction";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button2.Location = new System.Drawing.Point(482, 9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(131, 25);
            this.button2.TabIndex = 32;
            this.button2.Text = "Deep Corners Prediction";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.doSums);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(24, 180);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1008, 34);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Predictions";
            // 
            // button3
            // 
            this.button3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button3.Location = new System.Drawing.Point(345, 9);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(131, 25);
            this.button3.TabIndex = 33;
            this.button3.Text = "Deep Goals Prediction";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button4.Location = new System.Drawing.Point(619, 9);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(131, 25);
            this.button4.TabIndex = 34;
            this.button4.Text = "Bivariate Goals Prediction";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.bivariateGoalPrediction_Click);
            // 
            // button5
            // 
            this.button5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button5.Location = new System.Drawing.Point(756, 9);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(131, 25);
            this.button5.TabIndex = 35;
            this.button5.Text = "Bivariate Corners Prediction";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.bivariateCornerPrediction_Click);
            // 
            // FormzDBEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1074, 973);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.matchBox3);
            this.Controls.Add(this.matchBox2);
            this.Controls.Add(this.todayButton);
            this.Controls.Add(this.createLeague);
            this.Controls.Add(this.renameLeagueButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.newLeagueBox1);
            this.Controls.Add(this.getStats);
            this.Controls.Add(this.getLeague);
            this.Controls.Add(this.deleteGame);
            this.Controls.Add(this.setLeague);
            this.Controls.Add(this.renameBox1);
            this.Controls.Add(this.renameTeamButton);
            this.Controls.Add(this.leagueBox1);
            this.Controls.Add(this.leagueLabel);
            this.Controls.Add(this.Rename);
            this.Controls.Add(this.idLabel);
            this.Controls.Add(this.idBox2);
            this.Controls.Add(this.matchBox);
            this.Controls.Add(this.getButton1);
            this.Controls.Add(this.teamBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormzDBEditor";
            this.Text = "FormzDBEditor";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox teamBox1;
        private System.Windows.Forms.Button getButton1;
        private System.Windows.Forms.ListBox matchBox;
        private System.Windows.Forms.TextBox idBox2;
        private System.Windows.Forms.Label idLabel;
        private System.Windows.Forms.Label Rename;
        private System.Windows.Forms.Label leagueLabel;
        private System.Windows.Forms.TextBox leagueBox1;
        private System.Windows.Forms.Button renameTeamButton;
        private System.Windows.Forms.TextBox renameBox1;
        private System.Windows.Forms.Button setLeague;
        private System.Windows.Forms.Button deleteGame;
        private System.Windows.Forms.Button getLeague;
        private System.Windows.Forms.CheckBox getStats;
        private System.Windows.Forms.TextBox newLeagueBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button renameLeagueButton;
        private System.Windows.Forms.Button createLeague;
        private System.Windows.Forms.Button todayButton;
        private System.Windows.Forms.ListBox matchBox2;
        private System.Windows.Forms.ListBox matchBox3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem File;
        private System.Windows.Forms.ToolStripMenuItem specialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeDupTeams;
        private System.Windows.Forms.ToolStripMenuItem removeDuplicateMatchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tomorrowsGamesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem specialButtonToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolStripMenuItem fixBET365LeaguesToolStripMenuItem;
        private System.Windows.Forms.Button doSums;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem removeDupMatchesOverMidnightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteOldGamesWithNoStatsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem special2ButtonToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripMenuItem betDeJourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeDuplicateLeaguesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem predictionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simpleGoalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simpleCornerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bivariateGoalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bivariateCornerToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
    }
}

