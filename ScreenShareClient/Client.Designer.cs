namespace ScreenShareClient
{
    partial class Client
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PreferencesButton = new Button();
            ConnectButton = new Button();
            DisconnectButton = new Button();
            IPLabel = new Label();
            IPTextBox = new TextBox();
            PortLabel = new Label();
            PortTextBox = new TextBox();
            FullscreenButton = new Button();
            pictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // PreferencesButton
            // 
            PreferencesButton.Location = new Point(225, 415);
            PreferencesButton.Name = "PreferencesButton";
            PreferencesButton.Size = new Size(134, 23);
            PreferencesButton.TabIndex = 1;
            PreferencesButton.Text = "Save Preferences";
            PreferencesButton.UseVisualStyleBackColor = true;
            PreferencesButton.Click += PreferencesButton_Click;
            // 
            // ConnectButton
            // 
            ConnectButton.Location = new Point(492, 415);
            ConnectButton.Name = "ConnectButton";
            ConnectButton.Size = new Size(77, 23);
            ConnectButton.TabIndex = 2;
            ConnectButton.Text = "Connect";
            ConnectButton.UseVisualStyleBackColor = true;
            ConnectButton.Click += ConnectButton_Click;
            // 
            // DisconnectButton
            // 
            DisconnectButton.Location = new Point(575, 415);
            DisconnectButton.Name = "DisconnectButton";
            DisconnectButton.Size = new Size(77, 23);
            DisconnectButton.TabIndex = 3;
            DisconnectButton.Text = "Disconnect";
            DisconnectButton.UseVisualStyleBackColor = true;
            DisconnectButton.Click += DisconnectButton_Click;
            // 
            // IPLabel
            // 
            IPLabel.AutoSize = true;
            IPLabel.Location = new Point(12, 385);
            IPLabel.Name = "IPLabel";
            IPLabel.Size = new Size(77, 15);
            IPLabel.TabIndex = 4;
            IPLabel.Text = "IPv4 Address:";
            // 
            // IPTextBox
            // 
            IPTextBox.Location = new Point(95, 382);
            IPTextBox.Name = "IPTextBox";
            IPTextBox.Size = new Size(124, 23);
            IPTextBox.TabIndex = 5;
            IPTextBox.KeyPress += IPTextBox_KeyPress;
            // 
            // PortLabel
            // 
            PortLabel.AutoSize = true;
            PortLabel.Location = new Point(12, 415);
            PortLabel.Name = "PortLabel";
            PortLabel.Size = new Size(32, 15);
            PortLabel.TabIndex = 6;
            PortLabel.Text = "Port:";
            // 
            // PortTextBox
            // 
            PortTextBox.Location = new Point(95, 412);
            PortTextBox.Name = "PortTextBox";
            PortTextBox.Size = new Size(124, 23);
            PortTextBox.TabIndex = 7;
            PortTextBox.KeyPress += PortTextBox_KeyPress;
            // 
            // FullscreenButton
            // 
            FullscreenButton.Font = new Font("Arial Black", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            FullscreenButton.Location = new Point(633, 378);
            FullscreenButton.Name = "FullscreenButton";
            FullscreenButton.Size = new Size(19, 22);
            FullscreenButton.TabIndex = 8;
            FullscreenButton.Text = "⛶";
            FullscreenButton.UseVisualStyleBackColor = true;
            FullscreenButton.Click += FullscreenButton_Click;
            // 
            // pictureBox
            // 
            pictureBox.BackColor = SystemColors.ControlLight;
            pictureBox.BorderStyle = BorderStyle.Fixed3D;
            pictureBox.Location = new Point(12, 12);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(640, 360);
            pictureBox.TabIndex = 9;
            pictureBox.TabStop = false;
            // 
            // Client
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 501);
            Controls.Add(pictureBox);
            Controls.Add(FullscreenButton);
            Controls.Add(PortTextBox);
            Controls.Add(PortLabel);
            Controls.Add(DisconnectButton);
            Controls.Add(IPTextBox);
            Controls.Add(IPLabel);
            Controls.Add(ConnectButton);
            Controls.Add(PreferencesButton);
            Name = "Client";
            Text = "Client";
            FormClosing += Client_FormClosing;
            Load += Client_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button PreferencesButton;
        private Button ConnectButton;
        private Button DisconnectButton;
        private Label IPLabel;
        private TextBox IPTextBox;
        private Label PortLabel;
        private TextBox PortTextBox;
        private Button FullscreenButton;
        private PictureBox pictureBox;
    }
}
