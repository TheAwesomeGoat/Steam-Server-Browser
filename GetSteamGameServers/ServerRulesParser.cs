using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Steam.Query;

namespace GetSteamGameServers
{
    class ServerRulesParser
    {
        public struct Commands
        {
            public string command;
            public string value;
        }
        public List<Commands> GetServerRules(string IP, int port)
        {
            List<Commands> commands = new List<Commands>();
            SteamServerClient client = new SteamServerClient();

            byte[] recivedmsg = client.UDP_GetServerRules(IP,port,5000);

            var parser = new ResponseParser(recivedmsg);

            while (parser.CurrentPosition != recivedmsg.Length)
            {
                commands.Add(new Commands
                {
                    command = parser.GetStringToTermination(),
                    value = parser.GetStringToTermination()
                });
            }
            client.DisposeClient();
            return commands;
        }
    }
}
