namespace ScreenShareClient
{
    public partial class Client : Form
    {
        private bool isFullscreen = false;

        public Client()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            DisconnectButton.Enabled = true;
            ConnectButton.Enabled = false;
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            DisconnectButton.Enabled = false;
            ConnectButton.Enabled = true;
        }

        private void Client_Load(object sender, EventArgs e)
        {
            LoadPreferences();
            DisconnectButton.Enabled = false;
            KeyDown += new KeyEventHandler(Client_KeyDown);
        }

        private void Client_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                isFullscreen = false;
            }
            else if (e.KeyCode == Keys.F11)
            {
                isFullscreen = !isFullscreen;
            }
        }

        private void LoadPreferences()
        {
            // TODO
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            // TODO
        }

        private void FullscreenButton_Click(object sender, EventArgs e)
        {
            // TODO
            // pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
    }
}
