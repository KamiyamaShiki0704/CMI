namespace CMI
{
    partial class CMI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMI));
            this.statusLogGroupBox = new System.Windows.Forms.GroupBox();
            this.statusLogTextBox = new System.Windows.Forms.TextBox();
            this.playerGroupBox = new System.Windows.Forms.GroupBox();
            this.uiSoundPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            this.gameStateTimer = new System.Windows.Forms.Timer(this.components);
            this.eventActivationIcons = new System.Windows.Forms.ImageList(this.components);
            this.soundEventsGroupBox = new System.Windows.Forms.GroupBox();
            this.addEventButton = new System.Windows.Forms.Button();
            this.soundEventsListBox = new System.Windows.Forms.TreeView();
            this.soundEventFadeTimer = new System.Windows.Forms.Timer(this.components);
            this.soundEventRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.soundEventDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundEventEditMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusLogGroupBox.SuspendLayout();
            this.playerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiSoundPlayer)).BeginInit();
            this.soundEventsGroupBox.SuspendLayout();
            this.soundEventRightClickMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusLogGroupBox
            // 
            this.statusLogGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLogGroupBox.Controls.Add(this.statusLogTextBox);
            this.statusLogGroupBox.Location = new System.Drawing.Point(12, 7);
            this.statusLogGroupBox.Name = "statusLogGroupBox";
            this.statusLogGroupBox.Size = new System.Drawing.Size(776, 123);
            this.statusLogGroupBox.TabIndex = 0;
            this.statusLogGroupBox.TabStop = false;
            this.statusLogGroupBox.Text = "Status Log";
            // 
            // statusLogTextBox
            // 
            this.statusLogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLogTextBox.Location = new System.Drawing.Point(6, 19);
            this.statusLogTextBox.Multiline = true;
            this.statusLogTextBox.Name = "statusLogTextBox";
            this.statusLogTextBox.ReadOnly = true;
            this.statusLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.statusLogTextBox.Size = new System.Drawing.Size(764, 98);
            this.statusLogTextBox.TabIndex = 0;
            // 
            // playerGroupBox
            // 
            this.playerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playerGroupBox.Controls.Add(this.uiSoundPlayer);
            this.playerGroupBox.Location = new System.Drawing.Point(12, 272);
            this.playerGroupBox.Name = "playerGroupBox";
            this.playerGroupBox.Size = new System.Drawing.Size(776, 71);
            this.playerGroupBox.TabIndex = 3;
            this.playerGroupBox.TabStop = false;
            this.playerGroupBox.Text = "Player";
            // 
            // uiSoundPlayer
            // 
            this.uiSoundPlayer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uiSoundPlayer.Enabled = true;
            this.uiSoundPlayer.Location = new System.Drawing.Point(6, 19);
            this.uiSoundPlayer.Name = "uiSoundPlayer";
            this.uiSoundPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("uiSoundPlayer.OcxState")));
            this.uiSoundPlayer.Size = new System.Drawing.Size(764, 45);
            this.uiSoundPlayer.TabIndex = 2;
            // 
            // eventActivationIcons
            // 
            this.eventActivationIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("eventActivationIcons.ImageStream")));
            this.eventActivationIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.eventActivationIcons.Images.SetKeyName(0, "blank_icon.ico");
            this.eventActivationIcons.Images.SetKeyName(1, "deactivated_icon.png");
            this.eventActivationIcons.Images.SetKeyName(2, "activated_icon.png");
            // 
            // soundEventsGroupBox
            // 
            this.soundEventsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundEventsGroupBox.Controls.Add(this.addEventButton);
            this.soundEventsGroupBox.Controls.Add(this.soundEventsListBox);
            this.soundEventsGroupBox.Location = new System.Drawing.Point(12, 136);
            this.soundEventsGroupBox.Name = "soundEventsGroupBox";
            this.soundEventsGroupBox.Size = new System.Drawing.Size(776, 130);
            this.soundEventsGroupBox.TabIndex = 0;
            this.soundEventsGroupBox.TabStop = false;
            this.soundEventsGroupBox.Text = "Sound Events";
            // 
            // addEventButton
            // 
            this.addEventButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addEventButton.Location = new System.Drawing.Point(695, 9);
            this.addEventButton.Name = "addEventButton";
            this.addEventButton.Size = new System.Drawing.Size(75, 23);
            this.addEventButton.TabIndex = 1;
            this.addEventButton.Text = "Add Event";
            this.addEventButton.UseVisualStyleBackColor = true;
            this.addEventButton.Click += new System.EventHandler(this.AddEventButton_Click);
            // 
            // soundEventsListBox
            // 
            this.soundEventsListBox.AllowDrop = true;
            this.soundEventsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundEventsListBox.ImageIndex = 0;
            this.soundEventsListBox.ImageList = this.eventActivationIcons;
            this.soundEventsListBox.Location = new System.Drawing.Point(6, 33);
            this.soundEventsListBox.Margin = new System.Windows.Forms.Padding(2);
            this.soundEventsListBox.Name = "soundEventsListBox";
            this.soundEventsListBox.SelectedImageIndex = 0;
            this.soundEventsListBox.Size = new System.Drawing.Size(764, 91);
            this.soundEventsListBox.TabIndex = 0;
            this.soundEventsListBox.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.SoundEventsListBox_ItemDrag);
            this.soundEventsListBox.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SoundEventsListBox_AfterSelect);
            this.soundEventsListBox.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.SoundEventsListBox_NodeMouseClick);
            this.soundEventsListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.SoundEventsListBox_DragDrop);
            this.soundEventsListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.SoundEventsListBox_DragEnter);
            this.soundEventsListBox.DragOver += new System.Windows.Forms.DragEventHandler(this.SoundEventsListBox_DragOver);
            // 
            // soundEventFadeTimer
            // 
            this.soundEventFadeTimer.Interval = 10;
            // 
            // soundEventRightClickMenu
            // 
            this.soundEventRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.soundEventEditMenuItem,
            this.soundEventDeleteMenuItem});
            this.soundEventRightClickMenu.Name = "soundEventRightClickMenu";
            this.soundEventRightClickMenu.Size = new System.Drawing.Size(181, 70);
            // 
            // soundEventDeleteMenuItem
            // 
            this.soundEventDeleteMenuItem.Name = "soundEventDeleteMenuItem";
            this.soundEventDeleteMenuItem.Size = new System.Drawing.Size(180, 22);
            this.soundEventDeleteMenuItem.Text = "Delete";
            this.soundEventDeleteMenuItem.Click += new System.EventHandler(this.SoundEventDeleteMenuItem_Click);
            // 
            // soundEventEditMenuItem
            // 
            this.soundEventEditMenuItem.Name = "soundEventEditMenuItem";
            this.soundEventEditMenuItem.Size = new System.Drawing.Size(180, 22);
            this.soundEventEditMenuItem.Text = "Edit";
            this.soundEventEditMenuItem.Click += new System.EventHandler(this.SoundEventEditMenuItem_Click);
            // 
            // CMI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 351);
            this.Controls.Add(this.soundEventsGroupBox);
            this.Controls.Add(this.playerGroupBox);
            this.Controls.Add(this.statusLogGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(516, 303);
            this.Name = "CMI";
            this.Text = "Custom Music Injector";
            this.Shown += new System.EventHandler(this.CMI_Shown);
            this.Click += new System.EventHandler(this.CMI_Click);
            this.statusLogGroupBox.ResumeLayout(false);
            this.statusLogGroupBox.PerformLayout();
            this.playerGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiSoundPlayer)).EndInit();
            this.soundEventsGroupBox.ResumeLayout(false);
            this.soundEventRightClickMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox statusLogGroupBox;
        private System.Windows.Forms.TextBox statusLogTextBox;
        private AxWMPLib.AxWindowsMediaPlayer uiSoundPlayer;
        private System.Windows.Forms.GroupBox playerGroupBox;
        private System.Windows.Forms.Timer gameStateTimer;
        private System.Windows.Forms.ImageList eventActivationIcons;
        private System.Windows.Forms.GroupBox soundEventsGroupBox;
        private System.Windows.Forms.TreeView soundEventsListBox;
        private System.Windows.Forms.Timer soundEventFadeTimer;
        private System.Windows.Forms.Button addEventButton;
        private System.Windows.Forms.ContextMenuStrip soundEventRightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem soundEventDeleteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem soundEventEditMenuItem;
    }
}

