using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Steam.Query;

namespace GetSteamGameServers
{
    public class ServerInfoParser
    {
        public int Protocol { get; set; }
        public string ServerName { get; set; }
        public string Map { get; set; }
        public string Folder { get; set; }
        public string Game { get; set; }
        public int ID { get; set; }
        public byte Players { get; set; }
        public byte MaxPlayers { get; set; }
        public byte Bots { get; set; }
        public string ServerType { get; set; }
        public string Environment { get; set; }
        public byte Visibility { get; set; }
        public string Version { get; set; }
        public string ServerIP { get; set; }
        public int Port { get; set; }
        public bool Valid { get; set; }

        public int TimoutTime { get; set; }

        public void GetServerName(string IP, int endPointPort)
        {
            SteamServerClient client = new SteamServerClient();
            try
            {
                byte[] recivedmsg = client.UDP_GetServerInfo(IP,endPointPort, TimoutTime);

                var parser = new ResponseParser(recivedmsg);
                parser.CurrentPosition += 5;
                Protocol = parser.GetByte();
                ServerName = parser.GetStringToTermination();
                Map = parser.GetStringToTermination();
                Folder = parser.GetStringToTermination();
                Game = parser.GetStringToTermination();
                ID = parser.GetShort();
                Players = parser.GetByte();
                MaxPlayers = parser.GetByte();
                Bots = parser.GetByte();
                ServerType = parser.GetStringOfByte();
                Environment = parser.GetStringOfByte();
                Visibility = parser.GetByte();
                Version = parser.GetStringToTermination();
                ServerIP = IP;

                client.DisposeClient();

                Valid = true;
            }
            catch (System.Net.Sockets.SocketException)
            {
                ServerIP = IP;
                Port = endPointPort;
                Valid = false;
                client.DisposeClient();
            }
        }
    }
}
