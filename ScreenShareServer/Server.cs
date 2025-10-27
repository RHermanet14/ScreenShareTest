using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Drawing.Imaging;

namespace ScreenShareServer
{
    public partial class Server : Form
    {
        #region variables
        private volatile bool isRunning = false;
        private Connection? connection = null;
        private Task? _backgroundTask;
        private CancellationTokenSource? _cts;
        #endregion

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
            _cts = new CancellationTokenSource();
            _backgroundTask = Task.Run(() => RunServer(_cts.Token));
        }

        private void StopToken()
        {
            _cts?.Cancel();
        }

        private async Task StopTokenAsync()
        {
            _cts?.Cancel();
            if (_backgroundTask != null)
            {
                try
                {
                    await _backgroundTask;
                }
                catch (OperationCanceledException)
                {
                    //
                }
            }
            _cts?.Dispose();
            _cts = null;
        }

        private async Task RunServer(CancellationToken ct)
        {
            byte[] arr;
            try
            {
                isRunning = connection!.Connect();
                if (!isRunning)
                {
                    Invoke(() => { // So UI will appear even though its on background thread
                        MessageBox.Show("Error: Connect function returned false.");
                        StopButton_Click(this, EventArgs.Empty);
                    });
                    return;
                }

                if (!await connection.WaitForClient())
                {
                    Invoke(() => {
                        MessageBox.Show("Error: Failed to accept client connection.");
                        StopButton_Click(this, EventArgs.Empty);
                    });
                    return;
                }

                while (isRunning && !ct.IsCancellationRequested)
                {
                    if (connection == null) return;
                    arr = connection.GetScreen();
                    await connection.SendScreen(arr);
                    
                    // Small delay to prevent overwhelming the client
                    await Task.Delay(100, ct);
                }
            }
            catch (OperationCanceledException)
            {
                Invoke(() =>
                {
                    MessageBox.Show("Operation was cancelled.");
                });
                return;
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    MessageBox.Show("Error: " + ex.Message);
                });
                return;
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = true;
            StopButton.Enabled = false;
            KillServer();
            isRunning = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            KillServer();
        }

        private void KillServer()
        {
            isRunning = false;
            connection?.Disconnect();
            connection = null;
            StopTokenAsync().Wait(); // Or just StopToken();
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
        private Socket? clientSocket;
        private readonly int Port;
        private volatile bool _isConnected = false;

        public Connection(int port)
        {
            Port = port;
        }

        public bool Connect() 
        {
            IPAddress ip = IPAddress.Any; // Listen on all available network interfaces
            IPEndPoint localEndPoint = new IPEndPoint(ip, Port);
            serverSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(1); // Max pending connections
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                serverSocket?.Close();
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> WaitForClient()
        {
            if (!_isConnected || serverSocket == null) return false;
            
            try
            {
                clientSocket = await serverSocket.AcceptAsync();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting client: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            try
            {
                clientSocket?.Shutdown(SocketShutdown.Both);
                clientSocket?.Close();
            }
            catch
            {
                //nothing for now
            }
            
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
            if (!_isConnected) return [0];

            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using Bitmap bitmap = new(bounds.Width, bounds.Height);
            using Graphics g = Graphics.FromImage(bitmap);
            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            using MemoryStream stream = new();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        public async Task<bool> SendScreen(byte[] screen)
        {
            if (!_isConnected || clientSocket == null) return false;

            try
            {
                byte[] lengthBuffer = BitConverter.GetBytes(screen.Length);
                int bytesSent = 0;
                while (bytesSent < 4)
                {
                    int sent = await clientSocket.SendAsync(
                        new ArraySegment<byte>(lengthBuffer, bytesSent, 4 - bytesSent),
                        SocketFlags.None
                    );
                    if (sent == 0) return false;
                    bytesSent += sent;
                }
                // Send image data
                bytesSent = 0;
                int bufferSize = Math.Min(65536, screen.Length);
                while (bytesSent < screen.Length)
                {
                    int toSend = Math.Min(bufferSize, screen.Length - bytesSent);
                    int sent = await clientSocket.SendAsync(
                        new ArraySegment<byte>(screen, bytesSent, toSend),
                        SocketFlags.None
                    );
                    if (sent == 0) return false;
                    bytesSent += sent;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                //Disconnect();
                return false;
            }
        }
    }
}
