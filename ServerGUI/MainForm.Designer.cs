namespace fCraft.ServerGUI {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.uriDisplay = new System.Windows.Forms.TextBox();
            this.playerList = new System.Windows.Forms.ListBox();
            this.playerListLabel = new System.Windows.Forms.Label();
            this.console = new fCraft.ServerGUI.ConsoleBox();
            this.SuspendLayout();
            // 
            // logBox
            // 
            this.logBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logBox.BackColor = System.Drawing.Color.White;
            this.logBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.logBox.HideSelection = false;
            this.logBox.Location = new System.Drawing.Point(12, 39);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.Size = new System.Drawing.Size(519, 384);
            this.logBox.TabIndex = 7;
            this.logBox.Text = "";
            this.logBox.TextChanged += new System.EventHandler(this.logBox_TextChanged);
            // 
            // uriDisplay
            // 
            this.uriDisplay.BackColor = System.Drawing.Color.Gainsboro;
            this.uriDisplay.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uriDisplay.Location = new System.Drawing.Point(12, 13);
            this.uriDisplay.Name = "uriDisplay";
            this.uriDisplay.Size = new System.Drawing.Size(519, 20);
            this.uriDisplay.TabIndex = 7;
            this.uriDisplay.Text = "https://minecraft.net/classic/play/76bafebaedefdb169acb05af350017a7";
            // 
            // playerList
            // 
            this.playerList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playerList.BackColor = System.Drawing.Color.Gainsboro;
            this.playerList.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerList.ForeColor = System.Drawing.Color.Black;
            this.playerList.FormattingEnabled = true;
            this.playerList.IntegralHeight = false;
            this.playerList.ItemHeight = 14;
            this.playerList.Location = new System.Drawing.Point(540, 39);
            this.playerList.Name = "playerList";
            this.playerList.Size = new System.Drawing.Size(144, 384);
            this.playerList.TabIndex = 4;
            this.playerList.DoubleClick += new System.EventHandler(this.playerList_SelectedIndexChanged);
            // 
            // playerListLabel
            // 
            this.playerListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.playerListLabel.AutoSize = true;
            this.playerListLabel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerListLabel.Location = new System.Drawing.Point(537, 20);
            this.playerListLabel.Name = "playerListLabel";
            this.playerListLabel.Size = new System.Drawing.Size(48, 14);
            this.playerListLabel.TabIndex = 6;
            this.playerListLabel.Text = "Players";
            // 
            // console
            // 
            this.console.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.console.Enabled = false;
            this.console.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.console.Location = new System.Drawing.Point(13, 430);
            this.console.Name = "console";
            this.console.Size = new System.Drawing.Size(671, 20);
            this.console.TabIndex = 0;
            this.console.Text = "Please wait, starting server...";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(696, 461);
            this.Controls.Add(this.console);
            this.Controls.Add(this.playerListLabel);
            this.Controls.Add(this.playerList);
            this.Controls.Add(this.uriDisplay);
            this.Controls.Add(this.logBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 150);
            this.Name = "MainForm";
            this.Text = "AtomicCraft Server";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.TextBox uriDisplay;
        private System.Windows.Forms.ListBox playerList;
        private System.Windows.Forms.Label playerListLabel;
        private ConsoleBox console;
    }
}

