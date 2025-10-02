using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShareClient
{
    public partial class Client : Form
    {
        #region variables
        private bool isFullscreen = false;
        private bool isRunning = false;
        private Connection? connection = null;
        private Rectangle originalPictureBounds; // Store original bounds of pictureBox
        private Rectangle originalFormBounds; // Store original bounds of the form
        #endregion

        public Client()
        {
            InitializeComponent();
            KeyPreview = true;
        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            isRunning = true;
            DisconnectButton.Enabled = true;
            ConnectButton.Enabled = false;
            connection = new Connection(IPTextBox.Text, int.Parse(PortTextBox.Text));
            connection.Connect(); // Forget using the timer and block until connected
            MessageBox.Show("Connected to Server");
            await Task.Run(RunClient);
        }

        private async void RunClient()
        {
            while (isRunning)
            {
                // TODO
                await Task.Delay(100); // Prevents high CPU usage, adjust as necessary
                // Receive and display the screen data here
                // connection?.GetScreen();
                // connection?.StillRunning();
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
                    using (var originalImage = Image.FromStream(ms))
                    {
                        if (originalImage.Width > 3840 || originalImage.Height > 2160)
                        {
                            int maxWidth = Math.Min(1920, pictureBox.Width * 2);
                            int maxHeight = Math.Min(1080, pictureBox.Height * 2);

                            double scaleX = (double)maxWidth / originalImage.Width;
                            double scaleY = (double)maxHeight / originalImage.Height;
                            double scale = Math.Min(scaleX, scaleY);

                            int newWidth = (int)(originalImage.Width * scale);
                            int newHeight = (int)(originalImage.Height * scale);

                            pictureBox.Image = new Bitmap(originalImage, newWidth, newHeight);
                        }
                        else
                        {
                            pictureBox.Image = new Bitmap(originalImage);
                        }
                    }
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

        public async Task<byte[]?> GetScreen()
        {
            if (!isConnected || clientSocket == null) return null; // Must act like Kvm Server!!!
            
            try
            {
                byte[] lengthBuffer = new byte[4];
                int bytesRead = 0;

                while (bytesRead < 4)
                {
                    int read = await clientSocket.ReceiveAsync(
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
                    int read = await clientSocket.ReceiveAsync(
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
            return isConnected; // TODO
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
