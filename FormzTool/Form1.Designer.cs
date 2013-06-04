namespace FormzTool
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
            this.button2 = new System.Windows.Forms.Button();
            this.getLeague = new System.Windows.Forms.Button();
            this.getStats = new System.Windows.Forms.CheckBox();
            this.newLeagueBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.renameLeagueButton = new System.Windows.Forms.Button();
            this.createLeague = new System.Windows.Forms.Button();
            this.dupGames = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Old Team:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // teamBox1
            // 
            this.teamBox1.Location = new System.Drawing.Point(81, 6);
            this.teamBox1.Name = "teamBox1";
            this.teamBox1.Size = new System.Drawing.Size(614, 20);
            this.teamBox1.TabIndex = 1;
            // 
            // getButton1
            // 
            this.getButton1.Location = new System.Drawing.Point(701, 4);
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
            this.matchBox.Location = new System.Drawing.Point(15, 148);
            this.matchBox.Name = "matchBox";
            this.matchBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.matchBox.Size = new System.Drawing.Size(878, 256);
            this.matchBox.TabIndex = 3;
            this.matchBox.SelectedIndexChanged += new System.EventHandler(this.matchBox_SelectedIndexChanged);
            // 
            // idBox2
            // 
            this.idBox2.Location = new System.Drawing.Point(81, 122);
            this.idBox2.Name = "idBox2";
            this.idBox2.Size = new System.Drawing.Size(70, 20);
            this.idBox2.TabIndex = 4;
            // 
            // idLabel
            // 
            this.idLabel.AutoSize = true;
            this.idLabel.Location = new System.Drawing.Point(20, 126);
            this.idLabel.Name = "idLabel";
            this.idLabel.Size = new System.Drawing.Size(51, 13);
            this.idLabel.TabIndex = 5;
            this.idLabel.Text = "Team ID:";
            // 
            // Rename
            // 
            this.Rename.AutoSize = true;
            this.Rename.Location = new System.Drawing.Point(9, 37);
            this.Rename.Name = "Rename";
            this.Rename.Size = new System.Drawing.Size(62, 13);
            this.Rename.TabIndex = 7;
            this.Rename.Text = "New Team:";
            // 
            // leagueLabel
            // 
            this.leagueLabel.AutoSize = true;
            this.leagueLabel.Location = new System.Drawing.Point(6, 66);
            this.leagueLabel.Name = "leagueLabel";
            this.leagueLabel.Size = new System.Drawing.Size(65, 13);
            this.leagueLabel.TabIndex = 8;
            this.leagueLabel.Text = "Old League:";
            // 
            // leagueBox1
            // 
            this.leagueBox1.Location = new System.Drawing.Point(81, 63);
            this.leagueBox1.Name = "leagueBox1";
            this.leagueBox1.Size = new System.Drawing.Size(614, 20);
            this.leagueBox1.TabIndex = 9;
            // 
            // renameTeamButton
            // 
            this.renameTeamButton.Location = new System.Drawing.Point(701, 59);
            this.renameTeamButton.Name = "renameTeamButton";
            this.renameTeamButton.Size = new System.Drawing.Size(189, 26);
            this.renameTeamButton.TabIndex = 10;
            this.renameTeamButton.Text = "Rename Team";
            this.renameTeamButton.UseVisualStyleBackColor = true;
            this.renameTeamButton.Click += new System.EventHandler(this.renameButton_Click);
            // 
            // renameBox1
            // 
            this.renameBox1.Location = new System.Drawing.Point(81, 34);
            this.renameBox1.Name = "renameBox1";
            this.renameBox1.Size = new System.Drawing.Size(614, 20);
            this.renameBox1.TabIndex = 11;
            // 
            // setLeague
            // 
            this.setLeague.Location = new System.Drawing.Point(272, 119);
            this.setLeague.Name = "setLeague";
            this.setLeague.Size = new System.Drawing.Size(92, 26);
            this.setLeague.TabIndex = 12;
            this.setLeague.Text = "Set League";
            this.setLeague.UseVisualStyleBackColor = true;
            this.setLeague.Click += new System.EventHandler(this.button1_Click);
            // 
            // deleteGame
            // 
            this.deleteGame.Location = new System.Drawing.Point(175, 119);
            this.deleteGame.Name = "deleteGame";
            this.deleteGame.Size = new System.Drawing.Size(91, 26);
            this.deleteGame.TabIndex = 13;
            this.deleteGame.Text = "Delete Game";
            this.deleteGame.UseVisualStyleBackColor = true;
            this.deleteGame.Click += new System.EventHandler(this.deleteGame_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(563, 119);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(132, 26);
            this.button2.TabIndex = 14;
            this.button2.Text = "Remove Dup Teams";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // getLeague
            // 
            this.getLeague.Location = new System.Drawing.Point(701, 30);
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
            this.getStats.Location = new System.Drawing.Point(469, 125);
            this.getStats.Name = "getStats";
            this.getStats.Size = new System.Drawing.Size(88, 17);
            this.getStats.TabIndex = 17;
            this.getStats.Text = "Get Statistics";
            this.getStats.UseVisualStyleBackColor = true;
            // 
            // newLeagueBox1
            // 
            this.newLeagueBox1.Location = new System.Drawing.Point(81, 93);
            this.newLeagueBox1.Name = "newLeagueBox1";
            this.newLeagueBox1.Size = new System.Drawing.Size(614, 20);
            this.newLeagueBox1.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "New League:";
            // 
            // renameLeagueButton
            // 
            this.renameLeagueButton.Location = new System.Drawing.Point(701, 89);
            this.renameLeagueButton.Name = "renameLeagueButton";
            this.renameLeagueButton.Size = new System.Drawing.Size(189, 26);
            this.renameLeagueButton.TabIndex = 20;
            this.renameLeagueButton.Text = "Rename League";
            this.renameLeagueButton.UseVisualStyleBackColor = true;
            this.renameLeagueButton.Click += new System.EventHandler(this.renameLeague_Click_1);
            // 
            // createLeague
            // 
            this.createLeague.Location = new System.Drawing.Point(370, 119);
            this.createLeague.Name = "createLeague";
            this.createLeague.Size = new System.Drawing.Size(92, 26);
            this.createLeague.TabIndex = 21;
            this.createLeague.Text = "Create League";
            this.createLeague.UseVisualStyleBackColor = true;
            this.createLeague.Click += new System.EventHandler(this.createLeague_Click);
            // 
            // dupGames
            // 
            this.dupGames.Location = new System.Drawing.Point(701, 119);
            this.dupGames.Name = "dupGames";
            this.dupGames.Size = new System.Drawing.Size(186, 26);
            this.dupGames.TabIndex = 22;
            this.dupGames.Text = "Remove Dup Games";
            this.dupGames.UseVisualStyleBackColor = true;
            this.dupGames.Click += new System.EventHandler(this.dupGames_Click);
            // 
            // FormzDBEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(899, 411);
            this.Controls.Add(this.dupGames);
            this.Controls.Add(this.createLeague);
            this.Controls.Add(this.renameLeagueButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.newLeagueBox1);
            this.Controls.Add(this.getStats);
            this.Controls.Add(this.getLeague);
            this.Controls.Add(this.button2);
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
            this.Name = "FormzDBEditor";
            this.Text = "FormzDBEditor";
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
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button getLeague;
        private System.Windows.Forms.CheckBox getStats;
        private System.Windows.Forms.TextBox newLeagueBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button renameLeagueButton;
        private System.Windows.Forms.Button createLeague;
        private System.Windows.Forms.Button dupGames;
    }
}

