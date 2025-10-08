using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Drawing.Imaging;

namespace ScreenShareServer
{
    public partial class Server : Form
    {
        private Connection? connection = null;
        public Server()
        {
            InitializeComponent();
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            IPLabel.Visible = !IPLabel.Visible;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            connection = new Connection(int.Parse(PortTextBox.Text));
            connection?.Connect();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = true;
            StopButton.Enabled = false;
            KillServer();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            KillServer();
        }

        private void KillServer()
        {
            connection?.Disconnect();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            StopButton.Enabled = false;
            IPLabel.Visible = false;
            PortTextBox.Text = Properties.Settings.Default.Port;
            try
            {
                IPLabel.Text = GetIPAddress();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static string GetIPAddress()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    var ipProperties = networkInterface.GetIPProperties();

                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            return ip.Address.ToString();
                    }
                }
            }
            throw new Exception("No network adapters with an IPv$ address in the system!");
        }

        private void IPLabel_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(IPLabel.Text);
            MessageBox.Show("IP Address Was Copied!");
        }

        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8) // char 8 = Backspace
            {
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Port = PortTextBox.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("Preference Was Saved.");
        }

        private void IPLabel_MouseMove(object sender, MouseEventArgs e)
        {
            toolTip1.SetToolTip(IPLabel, "Click To Copy IP To Clipboard.");
        }
    }

    public partial class Connection
    {
        private Socket? serverSocket;
        private readonly int Port;
        private bool isConnected = false;

        public Connection(int port)
        {
            Port = port;
        }

        public void Connect() 
        {
            IPAddress ip = IPAddress.Any; // Listen on all available network interfaces
            IPEndPoint localEndPoint = new IPEndPoint(ip, Port);
            serverSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(); // Max pending connections
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            isConnected = false;
            try
            {
                serverSocket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //nothing for now
            }

            serverSocket?.Close();
        }

        public byte[] GetScreen()
        {
            if (!isConnected) return [0];

            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using Bitmap bitmap = new(bounds.Width, bounds.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
    }
}
