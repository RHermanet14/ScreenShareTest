using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShareClient
{
    public partial class Client : Form//, IDisposable
    {
        #region variables
        private bool isFullscreen = false;
        private bool isRunning = false;
        private Connection? connection = null;
        private Rectangle originalPictureBounds; // Store original bounds of pictureBox
        private Rectangle originalFormBounds; // Store original bounds of the form
        private Task? _backgroundTask;
        private CancellationTokenSource? _cts;
        #endregion

        public Client()
        {
            InitializeComponent();
            KeyPreview = true;
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            DisconnectButton.Enabled = true;
            ConnectButton.Enabled = false;
            connection = new Connection(IPTextBox.Text, int.Parse(PortTextBox.Text));
            _cts = new CancellationTokenSource();
            _backgroundTask = Task.Run(() => RunClient(_cts.Token));
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

        private async Task RunClient(CancellationToken ct)
        {
            byte[] bitmap;
            try
            {
                isRunning = connection!.Connect(); // move connection initialization into RunClient?
                if (!isRunning)
                {
                    Invoke(() => {
                        MessageBox.Show("Error: Could not connect to server");
                        DisconnectButton_Click(this, EventArgs.Empty); // Might need Invoke in click function and bool if coming from background thread
                    });
                    return;
                }
                while (isRunning && !ct.IsCancellationRequested)
                {
                    if (connection == null) return;
                    if (!connection.AcceptRequest())
                    {
                        Invoke(() =>
                        {
                            MessageBox.Show("Error: AcceptRequest function returned false");
                        });
                        return;
                    }
                    bitmap = await connection.GetScreen() ?? [0]; // temp fix + change to use _currentSocket
                    SetPictureBox(bitmap);
                    isRunning = connection.StillRunning(); // Needed?
                }
            }
            catch(OperationCanceledException)
            {
                Invoke(() =>
                {
                    MessageBox.Show("Operation was cancelled.");
                });
                // Handle cancellation if needed
                return;
            }
            catch (Exception ex)
            {
                Invoke(() => {
                    MessageBox.Show("Error: " + ex.Message);
                });
                return;
            } 
        }

        private void SetPictureBox(byte[] imageBuffer)
        {
            if (imageBuffer == null || imageBuffer.Length == 0) return;

            try
            {
                pictureBox.Image?.Dispose();
                pictureBox.Image = null;
                using (var ms = new MemoryStream(imageBuffer))
                {
                    using var originalImage = Image.FromStream(ms);
                    pictureBox.Image = new Bitmap(originalImage);
                }
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void DisconnectButton_Click(object sender, EventArgs e)
        {
            DisconnectButton.Enabled = false;
            ConnectButton.Enabled = true;
            Disconnect();
            //StopToken();
            // TODO
        }

        private void Client_Load(object sender, EventArgs e)
        {
            LoadPreferences();
            DisconnectButton.Enabled = false;
            KeyDown += new KeyEventHandler(Client_KeyDown);
            // TODO
        }

        private void Client_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                isFullscreen = false;
                HandleFullscreen();
            }
            else if (e.KeyCode == Keys.F11)
            {
                isFullscreen = !isFullscreen;
                HandleFullscreen();
            }
        }

        private void FullscreenButton_Click(object sender, EventArgs e)
        {
            isFullscreen = true;
            HandleFullscreen();
        }

        private void HandleFullscreen()
        {
            if (isFullscreen)
            {
                originalPictureBounds = pictureBox.Bounds;
                originalFormBounds = Bounds;

                FormBorderStyle = FormBorderStyle.None;
                //WindowState = FormWindowState.Maximized; // Keep or remove?

                pictureBox.BringToFront();
                pictureBox.Dock = DockStyle.Fill;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                //WindowState = FormWindowState.Normal; // Keep or remove?

                pictureBox.Dock = DockStyle.None;
                pictureBox.Bounds = originalPictureBounds;
                pictureBox.SendToBack();
                Bounds = originalFormBounds;
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

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            isRunning = false;
            connection?.Disconnect();
            connection = null;
            StopTokenAsync().Wait(); // Or just StopToken();
        }
    }

    public partial class Connection
    {
        private readonly string IPAddress = "";
        private readonly int Port = 0;
        private Socket? _clientSocket;
        private Socket? _currentSocket;
        private bool _isConnected = false;

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
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = System.Net.IPAddress.Parse(IPAddress);
                IPEndPoint remoteEndPoint = new(ipAddr, Port);
                _clientSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(remoteEndPoint);
                _isConnected = true;
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                _clientSocket?.Close();
                _isConnected = false;
                return false;
            }
        }

        public bool AcceptRequest()
        {
            if (_clientSocket == null || _currentSocket != null) return false;
            try
            {
                if (_clientSocket.Poll(0, SelectMode.SelectRead))
                {
                    _currentSocket = _clientSocket.Accept();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error accepting coordinate client: {ex.Message}");
            }
            return false;
        }

        public async Task<byte[]?> GetScreen()
        {
            if (!_isConnected || _currentSocket == null) return null; // Must act like Kvm Server!!!
            
            try
            {
                byte[] lengthBuffer = new byte[4];
                int bytesRead = 0;

                while (bytesRead < 4)
                {
                    int read = await _currentSocket.ReceiveAsync(
                        new ArraySegment<byte>(
                            lengthBuffer, bytesRead, 4 - bytesRead
                        ), SocketFlags.None
                    );
                    if (read == 0) return null;
                    bytesRead += read;
                }
                int imageLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] imageBuffer = new byte[imageLength];
                bytesRead = 0;
                int bufferSize = Math.Min(65536, imageLength);

                while (bytesRead < imageLength)
                {
                    int toReceive = Math.Min(bufferSize, imageLength - bytesRead);
                    int read = await _currentSocket.ReceiveAsync(
                        new ArraySegment<byte>(imageBuffer, bytesRead, toReceive),
                        SocketFlags.None);
                    if (read == 0) return null;
                    bytesRead += read;
                }
                return imageBuffer;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                //Disconnect();
                return null;
            }  
        }

        public bool StillRunning()
        {
            return _isConnected; // TODO
        }

        private void CloseCurrentClient()
        {
            if (_currentSocket != null)
            {
                try
                {
                    if (_currentSocket.Connected)
                    {
                        _currentSocket.Shutdown(SocketShutdown.Both);
                    }
                    _currentSocket.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing current client: {ex.Message}");
                }
                finally
                {
                    _currentSocket = null;
                    _isConnected = false;
                }
            }
        }

        public void Disconnect()
        {
            // TODO
            CloseCurrentClient();
            _isConnected = false;
            try
            {
                _clientSocket?.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //nothing for now
            }
            _clientSocket?.Close();
        }
    }
}
