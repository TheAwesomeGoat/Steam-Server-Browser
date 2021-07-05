using Steam.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetSteamGameServers
{
    public class Rcon
    {
        public TcpClient client;

        public bool Connected { get; set; }
        public bool Connect(string IP, int port,string pswrd)
        {
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
                UTF8Encoding utf = new UTF8Encoding();
                client = new TcpClient();

                int serverdata = 3;
                int reqid = 2;
                byte[] bstring1 = utf.GetBytes(pswrd);
                byte[] bstring2 = utf.GetBytes(string.Empty);

                int length = (4 + 4 + 4 + bstring1.Length + 1 + bstring2.Length + 1) - 4;

                string msg = $"{(char)length}\0\0\0{(char)reqid}\0\0\0{(char)serverdata}\0\0\0{pswrd}\0\0";
                client.Connect(endPoint);
                
                client.Client.Send(Encoding.Default.GetBytes(msg));

                byte[] a = new byte[14];
                byte[] b = new byte[14];
                int can = client.Client.Receive(a);
                int can2 = client.Client.Receive(b);
                int Auth = BitConverter.ToInt32(new byte[] { b[4], b[5], b[6], b[7], }, 0);
                if (Auth == -1)
                    return false;

                Connected = true;
                return true;
                
            }
            catch
            {
                Connected = false;
                return false;
            }
        }
        public struct Command
        {
            public string Out;
            public bool valid;
        }
        public Command SendCommand(string cmnd)
        {
            try
            {
                UTF8Encoding utf = new UTF8Encoding();
                int serverdata = 2;
                int reqid = 2;
                byte[] bstring1 = utf.GetBytes(cmnd);
                byte[] bstring2 = utf.GetBytes(string.Empty);

                int length = (4 + 4 + 4 + bstring1.Length + 1 + bstring2.Length + 1) - 4;

                string msg = $"{(char)length}\0\0\0{(char)reqid}\0\0\0{(char)serverdata}\0\0\0{cmnd}\0\0";

                client.Client.Send(Encoding.Default.GetBytes(msg));

                byte[] a = new byte[client.ReceiveBufferSize];
                client.Client.Receive(a);

                ResponseParser parser = new ResponseParser(a);

                parser.CurrentPosition = 12;
                string output = parser.GetStringToTermination();

                return new Command { Out = output, valid = true };
            }
            catch
            {
                return new Command { Out = "", valid = false };
            }
        }
        public bool Disconnect()
        {
            if (client.Connected)
            {
                try
                {
                    client.Client.Disconnect(false);
                    client.Close();
                    client.Dispose();
                    Connected = false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }

}
