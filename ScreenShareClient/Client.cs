using System.Net;
using System.Net.Sockets;

namespace ScreenShareClient
{
    public partial class Client : Form
    {
        private bool isFullscreen = false;
        private bool isRunning = false;
        private Connection? connection = null;

        public Client()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            isRunning = true;
            DisconnectButton.Enabled = true;
            ConnectButton.Enabled = false;
            ConnectTimer.Enabled = true;
            connection = new Connection(IPTextBox.Text, int.Parse(PortTextBox.Text));
            // TODO
        }

        private void ConnectTimer_Tick(object? sender, EventArgs e)
        {
            MessageBox.Show("It would run right now");
            // Check if connected
            // If so, isRunning = true
            // If not, try again
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            isRunning = false;
            DisconnectButton.Enabled = false;
            ConnectButton.Enabled = true;
            ConnectTimer.Enabled = false;
            connection?.Disconnect();
            //connection = null;
            // TODO
        }

        private void Client_Load(object sender, EventArgs e)
        {
            LoadPreferences();
            DisconnectButton.Enabled = false;
            KeyDown += new KeyEventHandler(Client_KeyDown);
            ConnectTimer.Tick += ConnectTimer_Tick;
            // TODO
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
            IPTextBox.Text = Properties.Settings.Default.IP;
            PortTextBox.Text = Properties.Settings.Default.Port;
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IP = IPTextBox.Text;
            Properties.Settings.Default.Port = PortTextBox.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Preferences Successfully Saved");
        }

        private void FullscreenButton_Click(object sender, EventArgs e)
        {
            isFullscreen = true;
            // TODO
            // pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            // Or just handle it where ever the image is drawn
        }

        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8) // char 8 = Backspace
            {
                e.Handled = true;
            }
        }

        private void IPTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8 && e.KeyChar != 46) // char 8 = Backspace, 46 = period
            {
                e.Handled = true;
            }
        }
    }

    public partial class Connection
    {
        private readonly string IPAddress = "";
        private readonly int Port = 0;
        private Socket? clientSocket;
        private bool isConnected = false;

        public Connection() { }
        public Connection(string ipAddress, int port)
        {
            IPAddress = ipAddress;
            Port = port;
        }

        public bool Connect()
        {
            try
            {
                isConnected = true;
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = System.Net.IPAddress.Parse(IPAddress);
                IPEndPoint remoteEndPoint = new(ipAddr, Port);
                clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteEndPoint);
                isConnected = true;
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                clientSocket?.Close();
                isConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            // TODO
            isConnected = false;
            try
            {
                clientSocket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //nothing for now
            }
            clientSocket?.Close();
        }
    }
}
