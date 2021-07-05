using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GetSteamGameServers
{
    public class SteamServerClient
    {
        UdpClient client;
        /// <summary>
        /// Gets all of the players in the specified server
        /// </summary>
        /// <param name="IP">Ip of the server</param>
        /// <param name="endPointPort">Port of the server</param>
        /// <param name="TimoutTime">Client recieve timout time(ms)</param>
        /// <returns></returns>
        public byte[] UDP_GetPlayers(string IP, int endPointPort,int TimoutTime)
        {
            DisposeClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IP), endPointPort);
            client = new UdpClient(endPointPort);
            client.Connect(endPoint);
            string msg = "ÿÿÿÿUÿÿÿÿ";
            client.Send(Encoding.Default.GetBytes(msg), msg.Length);
            client.Client.ReceiveTimeout = TimoutTime;
            byte[] recivedmsg = client.Receive(ref endPoint);
            string encmsg = Encoding.Default.GetString(recivedmsg);
            string[] split = encmsg.Split('A');

            msg = "ÿÿÿÿU" + split[1];
            client.Send(Encoding.Default.GetBytes(msg), msg.Length);

            return client.Receive(ref endPoint);
        }
        /// <summary>
        /// Gets all the information of the server
        /// </summary>
        /// <param name="IP">Ip of the server</param>
        /// <param name="endPointPort">Port of the server</param>
        /// <param name="TimoutTime">Client recieve timout time(ms)</param>
        /// <returns></returns>
        public byte[] UDP_GetServerInfo(string IP, int endPointPort, int TimoutTime)
        {
            DisposeClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IP), endPointPort);
            client = new UdpClient(endPointPort);
            client.Connect(endPoint);
            string msg = "ÿÿÿÿTSource Engine Query\0";
            client.Send(Encoding.Default.GetBytes(msg), msg.Length);
            client.Client.ReceiveTimeout = TimoutTime;
            return client.Receive(ref endPoint);
        }
        /// <summary>
        /// Gets all the rules of the server
        /// </summary>
        /// <param name="IP">Ip of the server</param>
        /// <param name="endPointPort">Port of the server</param>
        /// <param name="TimoutTime">Client recieve timout time(ms)</param>
        /// <returns></returns>
        public byte[] UDP_GetServerRules(string IP, int endPointPort, int TimoutTime)
        {
            DisposeClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IP), endPointPort);
            client = new UdpClient(endPointPort);
            client.Connect(endPoint);
            string msg = "ÿÿÿÿVÿÿÿÿ";
            client.Send(Encoding.Default.GetBytes(msg), msg.Length);
            client.Client.ReceiveTimeout = TimoutTime;
            byte[] recivedmsg = client.Receive(ref endPoint);
            int header = BitConverter.ToInt32(new byte[] { recivedmsg[0], recivedmsg[1], recivedmsg[2], recivedmsg[3] },0);
            string encmsg = Encoding.Default.GetString(recivedmsg);
            if (header == -1)
            {
                string[] split = encmsg.Split('A');
                msg = "ÿÿÿÿV" + split[1];
                client.Send(Encoding.Default.GetBytes(msg), msg.Length);
            }
            client.Client.ReceiveTimeout = 500;
            byte[] second = new byte[0];
            List<byte[]> bytes = new List<byte[]>();
            while (true)
            {
                try 
                { 
                    bytes.Add(client.Receive(ref endPoint)); 
                } 
                catch 
                { 
                    break; 
                }
            }

            for (int i = 0; i < bytes.Count; i++)
                second = Combine(bytes[(bytes.Count - 1) - i].Skip(19 - (i == (bytes.Count-1) ? 0 : 7)).ToArray(), second);

            return second;
        }
        byte[] Combine(byte[] first, byte[] second)
        {
            byte[] bytes = new byte[(first.Length) + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length,second.Length);
            return bytes;
        }

        /// <summary>
        /// Closes / Disposes the client
        /// </summary>
        public void DisposeClient()
        {
            if (client != null) { client.Close(); client.Dispose(); }
        }
    }
}
