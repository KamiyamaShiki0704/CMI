namespace CMI
{
    sealed partial class AddEventForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddEventForm));
            this.label1 = new System.Windows.Forms.Label();
            this.eventNameTextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.soundPathTextbox = new System.Windows.Forms.TextBox();
            this.soundPathBrowseButton = new System.Windows.Forms.Button();
            this.pointer1Textbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pointer2Textbox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.startbitTextbox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.typeTextbox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.fadeInSecondsTextbox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.fadeOutSecondsTextbox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.addEventOKButton = new System.Windows.Forms.Button();
            this.fadeIntoNextTrackCheckbox = new System.Windows.Forms.CheckBox();
            this.loopCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Event Name:";
            // 
            // eventNameTextbox
            // 
            this.eventNameTextbox.Location = new System.Drawing.Point(120, 14);
            this.eventNameTextbox.Name = "eventNameTextbox";
            this.eventNameTextbox.Size = new System.Drawing.Size(124, 20);
            this.eventNameTextbox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "SoundPath:";
            // 
            // soundPathTextbox
            // 
            this.soundPathTextbox.Location = new System.Drawing.Point(120, 45);
            this.soundPathTextbox.Name = "soundPathTextbox";
            this.soundPathTextbox.Size = new System.Drawing.Size(124, 20);
            this.soundPathTextbox.TabIndex = 3;
            // 
            // soundPathBrowseButton
            // 
            this.soundPathBrowseButton.Location = new System.Drawing.Point(250, 43);
            this.soundPathBrowseButton.Name = "soundPathBrowseButton";
            this.soundPathBrowseButton.Size = new System.Drawing.Size(67, 23);
            this.soundPathBrowseButton.TabIndex = 4;
            this.soundPathBrowseButton.Text = "Browse";
            this.soundPathBrowseButton.UseVisualStyleBackColor = true;
            this.soundPathBrowseButton.Click += new System.EventHandler(this.SoundPathBrowseButton_Click);
            // 
            // pointer1Textbox
            // 
            this.pointer1Textbox.Location = new System.Drawing.Point(120, 76);
            this.pointer1Textbox.Name = "pointer1Textbox";
            this.pointer1Textbox.Size = new System.Drawing.Size(124, 20);
            this.pointer1Textbox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Pointer1:";
            // 
            // pointer2Textbox
            // 
            this.pointer2Textbox.Location = new System.Drawing.Point(120, 107);
            this.pointer2Textbox.Name = "pointer2Textbox";
            this.pointer2Textbox.Size = new System.Drawing.Size(124, 20);
            this.pointer2Textbox.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Pointer2:";
            // 
            // startbitTextbox
            // 
            this.startbitTextbox.Location = new System.Drawing.Point(120, 138);
            this.startbitTextbox.Name = "startbitTextbox";
            this.startbitTextbox.Size = new System.Drawing.Size(124, 20);
            this.startbitTextbox.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Startbit:";
            // 
            // typeTextbox
            // 
            this.typeTextbox.Location = new System.Drawing.Point(120, 169);
            this.typeTextbox.Name = "typeTextbox";
            this.typeTextbox.Size = new System.Drawing.Size(124, 20);
            this.typeTextbox.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 172);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Type:";
            // 
            // fadeInSecondsTextbox
            // 
            this.fadeInSecondsTextbox.Location = new System.Drawing.Point(120, 200);
            this.fadeInSecondsTextbox.Name = "fadeInSecondsTextbox";
            this.fadeInSecondsTextbox.Size = new System.Drawing.Size(124, 20);
            this.fadeInSecondsTextbox.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 203);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(85, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "FadeInSeconds:";
            // 
            // fadeOutSecondsTextbox
            // 
            this.fadeOutSecondsTextbox.Location = new System.Drawing.Point(120, 231);
            this.fadeOutSecondsTextbox.Name = "fadeOutSecondsTextbox";
            this.fadeOutSecondsTextbox.Size = new System.Drawing.Size(124, 20);
            this.fadeOutSecondsTextbox.TabIndex = 16;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 234);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(93, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "FadeOutSeconds:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 266);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(102, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "FadeIntoNextTrack:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(143, 266);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Loop:";
            // 
            // addEventOKButton
            // 
            this.addEventOKButton.Location = new System.Drawing.Point(15, 290);
            this.addEventOKButton.Name = "addEventOKButton";
            this.addEventOKButton.Size = new System.Drawing.Size(314, 23);
            this.addEventOKButton.TabIndex = 21;
            this.addEventOKButton.Text = "OK";
            this.addEventOKButton.UseVisualStyleBackColor = true;
            this.addEventOKButton.Click += new System.EventHandler(this.AddEventOKButton_Click);
            // 
            // fadeIntoNextTrackCheckbox
            // 
            this.fadeIntoNextTrackCheckbox.AutoSize = true;
            this.fadeIntoNextTrackCheckbox.Location = new System.Drawing.Point(120, 266);
            this.fadeIntoNextTrackCheckbox.Name = "fadeIntoNextTrackCheckbox";
            this.fadeIntoNextTrackCheckbox.Size = new System.Drawing.Size(15, 14);
            this.fadeIntoNextTrackCheckbox.TabIndex = 22;
            this.fadeIntoNextTrackCheckbox.UseVisualStyleBackColor = true;
            // 
            // loopCheckbox
            // 
            this.loopCheckbox.AutoSize = true;
            this.loopCheckbox.Location = new System.Drawing.Point(183, 266);
            this.loopCheckbox.Name = "loopCheckbox";
            this.loopCheckbox.Size = new System.Drawing.Size(15, 14);
            this.loopCheckbox.TabIndex = 23;
            this.loopCheckbox.UseVisualStyleBackColor = true;
            // 
            // AddEventForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 327);
            this.Controls.Add(this.loopCheckbox);
            this.Controls.Add(this.fadeIntoNextTrackCheckbox);
            this.Controls.Add(this.addEventOKButton);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.fadeOutSecondsTextbox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.fadeInSecondsTextbox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.typeTextbox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.startbitTextbox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pointer2Textbox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pointer1Textbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.soundPathBrowseButton);
            this.Controls.Add(this.soundPathTextbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.eventNameTextbox);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AddEventForm";
            this.Text = "Add Event";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox eventNameTextbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox soundPathTextbox;
        private System.Windows.Forms.Button soundPathBrowseButton;
        private System.Windows.Forms.TextBox pointer1Textbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox pointer2Textbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox startbitTextbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox typeTextbox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox fadeInSecondsTextbox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox fadeOutSecondsTextbox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button addEventOKButton;
        private System.Windows.Forms.CheckBox fadeIntoNextTrackCheckbox;
        private System.Windows.Forms.CheckBox loopCheckbox;
    }
}