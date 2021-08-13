using DiscordRPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Timer = System.Timers.Timer;
using app.modules;
using System.Reflection.Metadata;

namespace app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainAsync().GetAwaiter().GetResult();
        }
        

        public async Task MainAsync()
        {          
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);            
            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:800.\nWaiting for a connection...");

            clientTimer = new Timer();
            clientTimer.Elapsed += ClientTimer_Elapsed;
            clientTimer.Interval = 1000;
            clientTimer.AutoReset = true;
            clientTimer.Start();

            checkTimer = new Timer();
            checkTimer.Elapsed += CheckTimer_Elapsed;
            checkTimer.Interval = 100;
            checkTimer.AutoReset = true;
            checkTimer.Start();


            AddServicesCB();
        }
        private List<CheckBox> services_cb_list;
        private void AddServicesCB() {
            services_cb_list = new List<CheckBox>();
            var xcloud = new CheckBox();
            var gnow = new CheckBox();
            var yt = new CheckBox();
            xcloud.Content = "Xbox Cloud Gaming";
            xcloud.Click += XCloud_Click;
            services_cb_list.Add(xcloud);
            gnow.Content = "GeForce NOW (Web)";
            gnow.Click += GNOW_Click;
            services_cb_list.Add(gnow);
            yt.Content = "YouTube";
            yt.Click += YT_Click;
            services_cb_list.Add(yt);

            foreach (var cb in services_cb_list)
                StackPanel1.Children.Add(cb);

            FilterBox.TextChanged += (v1, v2) =>
            {
                string text = FilterBox.Text.ToLower();
                if (text.Length > 0)
                {
                    foreach (var cb in services_cb_list)
                    {
                        if (!cb.Content.ToString().ToLower().Contains(text))
                            StackPanel1.Children.Remove(cb);
                        else
                        {
                            if (!StackPanel1.Children.Contains(cb))
                                StackPanel1.Children.Add(cb);
                        }
                    }
                }
                else
                {
                    foreach (var cb in services_cb_list)
                    {
                        if (!StackPanel1.Children.Contains(cb))
                            StackPanel1.Children.Add(cb);
                    }
                }
            };
        }


        private async void ClientTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (clientList.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    statusLabel.Content = "Not connected";
                });
            }
            else
            {
                for(int i = 0; i < clientList.Count; i++)
                {
                    if (!clientList[i].Connected)
                        clientList.RemoveAt(i);
                }
                
            }

            if (server.Pending())
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clientList.Add(client);
                Console.WriteLine("A client connected.");
                this.Dispatcher.Invoke(() =>
                {
                    statusLabel.Content = "Connected";
                });
            }
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e) => await Check();

        List<TcpClient> clientList = new List<TcpClient>();
        ServicesModule Services = new ServicesModule();
        TcpListener server;
        Timer checkTimer;
        Timer clientTimer;
        int port = 800;
        data_bundle lastBundle = new data_bundle() {
            title = "",
            url = ""
        };

        private async Task Check()
        {
            if (clientList.Count == 0)
                return;
            foreach (var client in clientList)
            {

                NetworkStream stream = client.GetStream();
                if (stream.DataAvailable)
                {
                    string data = await HandShake(client);
                    if(data != string.Empty)
                    {
                        if (data == "closed")
                        {
                            clientList.Remove(client);
                            return;
                        }
                        try
                        {
                            var bundle = JsonConvert.DeserializeObject<data_bundle>(data);
                            Update(bundle);
                        }
                        catch(Exception) { }
                    }
                }
            }
        }

        void Update(data_bundle bundle)
        {
            if (Services.xcloud && bundle.url.StartsWith("https://www.xbox.com"))
                Services.xCloudHandle(bundle);
            else if (Services.yt && bundle.url.StartsWith("https://www.youtube.com"))
                Services.YTHandle(bundle);
            else if (Services.gnow && bundle.url.StartsWith("https://play.geforcenow.com"))
                Services.GNowHandle(bundle);
            else
                Services.UnsupportedHandle();
            lastBundle = bundle;
        }

        async Task SendTab()
        {
            Update(lastBundle);
        }

        async Task<string> HandShake(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] bytes = new byte[client.Available];
            await stream.ReadAsync(bytes, 0, bytes.Length);
            string data = Encoding.UTF8.GetString(bytes);

            if (Regex.IsMatch(data, "^GET"))
            {
                const string eol = "\r\n"; // end-of-line marker

                Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Connection: Upgrade" + eol
                    + "Upgrade: websocket" + eol
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(
                             Encoding.UTF8.GetBytes(
                                new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + eol
                    + eol);

                stream.Write(response, 0, response.Length);
            }
            else
            {
                bool fin = (bytes[0] & 0b10000000) != 0,
                    mask = (bytes[1] & 0b10000000) != 0; 

                int opcode = bytes[0] & 0b00001111, 
                    msglen = bytes[1] - 128, // & 0111 1111
                    offset = 2;

                if (msglen == 126)
                {
                    msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                    offset = 4;
                }
                
                if (msglen == 0)
                    Console.WriteLine("msglen == 0");
                else if (mask)
                {
                    byte[] decoded = new byte[msglen];
                    byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                    offset += 4;

                    for (int i = 0; i < msglen; ++i)
                        decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                    string text = Encoding.UTF8.GetString(decoded);
                    return text;
                }
                else
                    return "mask bit not set";

                
            }
            return string.Empty;
        }

        private async void XCloud_Click(object sender, RoutedEventArgs e)
        {
            Services.xcloud = !Services.xcloud;
            await SendTab();
        }

        private async void YT_Click(object sender, RoutedEventArgs e)
        {
            Services.yt = !Services.yt;
            await SendTab();
        }

        private async void GNOW_Click(object sender, RoutedEventArgs e)
        {
            Services.gnow = !Services.gnow;
            await SendTab();
        }
        public struct data_bundle
        { 
            public string title { get; set; }
            public string url { get; set; }
        }
    }

    public class User
    {
        public string Service { get; set; }

        public bool Enabled { get; set; }
    }
}
