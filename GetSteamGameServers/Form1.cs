using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TEST.PacketBuilder;

namespace GetSteamGameServers
{
    public partial class Form1 : Form
    {
        enum EnumRegions
        {
            UsEastCoast = 0x00,
            UsWestCoast = 0x01,
            SouthAmerica = 0x02,
            Europe = 0x03,
            Asia = 0x04,
            Australia = 0x05,
            MiddleEast = 0x06,
            Africa = 0x07,
            All = 0xFF
        }
        enum SourceEngineGames
        {
            Half_Life2 = 220,
            Counter_Strike_Source = 240,
            Day_Of_Defeat = 300,
            Half_Life2_Deathmatch = 320,
            Team_Fortress_2 = 440,
            Left_4_Dead_2 = 500,
            Counter_Strike_Global_Offensive = 730,
            Garrys_Mod = 4000,
            Insurgency = 17700
        }

        public string BuildPacket(string first,string region,string appid)
        {
            int a = (int)Enum.Parse(typeof(EnumRegions), region);
            return $"1{(char)a}{first}\0\\appid\\{appid}\\empty\\1\\map\\gm_genesis\0"; //
        }

        public Form1()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
            InitializeComponent();
        }
        Size formSize;

        readonly Logger log = new Logger();
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox10.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            log.LogTextBox = textBox1;
            formSize = this.Size;
            Regions.Text = "All";
            Games.Text = "Garrys_Mod";
        }
        //https://developer.valvesoftware.com/wiki/Master_Server_Query_Protocol

        readonly IpFilter filter = new IpFilter();
        readonly Invokers invokers = new Invokers();
        UdpClient client;
        Thread GetSteamServersThread;
        Thread GetRulesThread;
        string SelectedGame;
        string ServerRegion;
        private void GetServers_Click(object sender, EventArgs e)
        {
            if (client != null) { client.Close(); client.Dispose(); }
            if (GetSteamServersThread != null)
                if (GetSteamServersThread.IsAlive)
                    GetSteamServersThread.Abort();

            dataGridView1.Rows.Clear();

            SelectedGame = Games.SelectedItem.ToString();
            ServerRegion = Regions.SelectedItem.ToString();

            int appid = (int)Enum.Parse(typeof(SourceEngineGames), SelectedGame);

            filter.AppId = appid.ToString();
            filter.Map = textBox4.Text;
            filter.HostName = textBox5.Text;
            filter.IsNotEmpty = checkBox2.Checked;

            GetSteamServersThread = new Thread(GetServer_Thread);
            GetSteamServersThread.Start();
        }

        IPEndPoint Hl2_MasterServerEndPoint = new IPEndPoint(Dns.GetHostAddresses("hl2master.steampowered.com")[0], 27011);
        private void GetServer_Thread()
        {
            ServerInfoParser serverInfoParser = new ServerInfoParser();
            IpParser ipParser = new IpParser();

            client = new UdpClient(Hl2_MasterServerEndPoint.Port);

            string first = $"0.0.0.0:0";
            string last = first;
            int NextBatch = 0;
            int Bacthes = (int)numericUpDown1.Value-1;
            int Index = 1;
            serverInfoParser.TimoutTime = (int)numericUpDown2.Value;
            
            invokers.LabelTextInvoker(label1, "Status : Busy");

            client.Connect(Hl2_MasterServerEndPoint);
            while (true)
            {
                log.Log($"[*] Getting Servers From : {last} -> {Hl2_MasterServerEndPoint.Address}:{Hl2_MasterServerEndPoint.Port}");
                string message = new PacketBuilder2().BuildPacket(last,ServerRegion,filter); //BuildPacket(last, ServerRegion, appid.ToString());

                client.Send(Encoding.Default.GetBytes(message), message.Length);
                byte[] recivedmsg = client.Receive(ref Hl2_MasterServerEndPoint);
                var ParsedMessage = ipParser.Parse(recivedmsg);

                if (ParsedMessage != null || ParsedMessage.Count !=0)
                {
                    if (ParsedMessage[0].Address.ToString() != "0.0.0.0")
                    {
                        foreach (var item in ParsedMessage)
                        {
                            serverInfoParser.GetServerName(item.Address.ToString(), item.Port);
                            if (serverInfoParser.Valid)
                            {
                                invokers.AddDataItem(new string[] {
                                    Index++.ToString("00000.#####"),
                                    serverInfoParser.ServerName,
                                    $"{serverInfoParser.Players}/{serverInfoParser.MaxPlayers}",
                                    serverInfoParser.Bots.ToString(),
                                    item.Address.ToString(), item.Port.ToString()
                                }, dataGridView1);
                            }
                        }
                        log.Log($"[*] Done With Batch: {NextBatch + 1}");

                        if (ParsedMessage.Count == 0)
                            invokers.AddDataItem(new string[] { "n/a", "No Server Found", "n/a", "n/a", "n/a", "n/a" }, dataGridView1);
                        else
                            last = $"{ParsedMessage.Last().Address}:{ParsedMessage.Last().Port}";

                        ParsedMessage.Clear();
                        if (last == first | NextBatch == Bacthes) //last == first
                            break;
                        NextBatch++;
                    }
                    else { invokers.AddDataItem(new string[] { "n/a", "No Server Found", "n/a", "n/a", "n/a", "n/a" }, dataGridView1); break; }
                }
                else { invokers.AddDataItem(new string[] { "n/a", "No Server Found", "n/a", "n/a", "n/a", "n/a" }, dataGridView1); break; }
            }
            client.Close();
            client.Dispose();
            invokers.LabelTextInvoker(label1, "Status : Idle");
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var dt = dataGridView1;
                dataGridView2.Rows.Clear();
                if (dt.Rows.Count > 0)
                {
                    string ip = dt.Rows[e.RowIndex].Cells[4].Value.ToString();
                    string port = dt.Rows[e.RowIndex].Cells[5].Value.ToString();
                    string index = dt.Rows[e.RowIndex].Cells[0].Value.ToString();

                    textBox6.Text = ip;
                    textBox7.Text = port.ToString();
                    textBox2.Text = index;
                    textBox3.Text = $"{ip}:{port}";
                    new Thread(() =>
                    {
                        ServerPlayersParser playersParser = new ServerPlayersParser();
                        var play = playersParser.GetPlayers(ip, int.Parse(port));
                        if (play != null)
                        {
                            if (play.Count() != 0)
                            {
                                foreach (var item in play)
                                {
                                    int Hour = 0;
                                    int Minutes = 0;
                                    int Seconds = 0;
                                    try
                                    {
                                        var Time = TimeSpan.FromSeconds(float.IsNaN(item.Duration) ? 0.0f : item.Duration);
                                        Hour = Time.Hours;
                                        Minutes = Time.Minutes;
                                        Seconds = Time.Seconds;
                                    }
                                    catch { }
                                    invokers.AddDataItem(new string[] {
                                        item.Name,
                                        item.Score.ToString(),
                                        $"{(Hour == 0 ? $"" : $"{Hour}h " )}{((Minutes == 0 & Hour == 0) ? $"" : $"{Minutes}m " )}{((Seconds == 0 & Minutes == 0 & Hour == 0) ? $"" : $"{Seconds}s " )}"
                                            }, dataGridView2);
                                }
                            }
                        }
                        else
                        {
                            invokers.AddDataItem(new string[] {
                                    "No Players Found / Error",
                                    "n/a",
                                    "n/a"
                                        }, dataGridView2);
                        }
                    }).Start();
                }
            }
            catch
            {

            }
        }

        Rcon rcon = new Rcon();

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GetSteamServersThread != null)
                if (GetSteamServersThread.IsAlive)
                    GetSteamServersThread.Abort();

            if (GetRulesThread != null)
                if (GetRulesThread.IsAlive)
                    GetRulesThread.Abort();

            if(rcon.Connected)
                rcon.Disconnect();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            this.Size = formSize;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (GetSteamServersThread != null)
                if (GetSteamServersThread.IsAlive)
                    GetSteamServersThread.Abort();
            if (client != null) { client.Close(); client.Dispose(); }
            invokers.LabelTextInvoker(label1, "Status : Idle");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (GetRulesThread != null)
                if (GetRulesThread.IsAlive)
                    GetRulesThread.Abort();

            dataGridView3.Rows.Clear();
            GetRulesThread = new Thread(GetRuleThread);
            GetRulesThread.Start();
        }
        private void GetRuleThread()
        {
            ServerRulesParser serverRulesParser = new ServerRulesParser();
            string ip = textBox6.Text;
            int port = int.Parse(textBox7.Text);
            var rules = serverRulesParser.GetServerRules(ip,port);
            foreach (var item in rules)
            {
                invokers.AddDataItem(new string[] {
                    item.command,
                    item.value,
                }, dataGridView3);
            }
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://developer.valvesoftware.com/wiki/Master_Server_Query_Protocol");
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://developer.valvesoftware.com/wiki/Server_queries");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://developer.valvesoftware.com/wiki/Source_RCON_Protocol");
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            label14.Text = "Status : Connecting";
            string ip = textBox8.Text;
            int port = int.Parse(textBox9.Text);
            string pass = textBox11.Text;
            if (ip != "")
            {
                new Thread(() =>
                {
                    invokers.EnableObject(button4, false);
                    invokers.EnableObject(button6, false);
                    if (rcon.Connect(ip, port, pass))
                    {

                        invokers.EnableObject(textBox10, true);
                        invokers.EnableObject(button5, true);
                        invokers.LabelTextInvoker(label14, "Status : Connected");
                        invokers.TextboxAddText(textBox12, $"Connected To {ip}:{port}");
                        invokers.EnableObject(button4, true);
                        invokers.EnableObject(button6, true);
                    }
                    else
                    {
                        invokers.EnableObject(textBox10, false);
                        invokers.EnableObject(button5, false);
                        invokers.LabelTextInvoker(label14, "Status : Failed To Connect");
                        invokers.TextboxAddText(textBox12, $"Failed To Connected To {ip}:{port}");
                    }
                    invokers.EnableObject(button4, true);
                }).Start();
            }
            else
                label14.Text = "Status : Insert IP";
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            label14.Text = "Status : Sending";
            string cmnd = textBox10.Text;
            if (cmnd != "")
            {
                new Thread(() =>
                {
                    var rcn = rcon.SendCommand(cmnd);
                    if (rcn.valid)
                    {
                        invokers.LabelTextInvoker(label14, "Status : Send Successful");
                        invokers.TextboxAddText(textBox12, rcn.Out);
                    }
                    else
                    {
                        invokers.LabelTextInvoker(label14, "Status : Send Failed");
                        invokers.TextboxAddText(textBox12, "Send Failed");
                    }
                }).Start();
            }
            else
            {
                label14.Text = "Status : Insert Command";
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            if (rcon.Connected)
            {
                if (rcon.Disconnect())
                {
                    label14.Text = "Status : Disconnected";
                    textBox10.Enabled = false;
                    button5.Enabled = false;
                    button6.Enabled = false;
                    textBox12.Text += $"Disconnected\r\n";
                }
                else
                {
                    label14.Text = "Status : Disconnect Failed";
                    textBox12.Text += $"Disconnected Failed\r\n";
                }
            }
        }
    }
}
