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
    public class ServerPlayersParser
    {
        public struct PlayerInfo
        {
            public int Index;
            public string Name;
            public int Score;
            public float Duration;
            public bool valid;
        }
        public List<PlayerInfo> GetPlayers(string IP, int endPointPort)
        {
            List<PlayerInfo> players = new List<PlayerInfo>();
            SteamServerClient client = new SteamServerClient();
            try
            {
                byte[] recivedmsg = client.UDP_GetPlayers(IP, endPointPort, 5000);

                var parser = new ResponseParser(recivedmsg);
                parser.CurrentPosition += 5;
                byte playersCount = parser.GetByte();
                try
                {
                    for (int i = 0; i < recivedmsg.Count(); i += 4)
                    {
                        players.Add(new PlayerInfo {
                            Index = parser.GetByte(),
                            Name = parser.GetStringToTermination(),
                            Score = (int)parser.GetLong(),
                            Duration = parser.GetFloat(),
                            valid = true
                        });
                    }
                }
                catch (Exception e){ var a = e; client.DisposeClient(); }
                client.DisposeClient();
                if (players.Count != 0)
                    return players;
                else
                {
                    client.DisposeClient();
                    return null;
                }
            }
            catch (Exception)
            {
                client.DisposeClient();
                return null;
            }
        }
    }
}
