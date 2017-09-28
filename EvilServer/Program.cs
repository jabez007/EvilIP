using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EvilServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket receiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            IPEndPoint destination = new IPEndPoint(IPAddress.Loopback, 0);
            receiveSocket.Bind(destination);
            receiveSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4];

            receiveSocket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

            EndPoint source = new IPEndPoint(IPAddress.Any, 0);
            byte[] received = new byte[4096];
            int bytesRead;
            EvilPacket.EvilPacket receivedPacket;
            while (true)
            {
                bytesRead = receiveSocket.ReceiveFrom(received, ref source);
                receivedPacket = new EvilPacket.EvilPacket(received);
                if (receivedPacket.evil)
                {
                    Console.WriteLine("EvilServer received {0} from {1}", Encoding.UTF8.GetString(new[] { receivedPacket.hiddenData }), source);
                }   
            }
        }
    }
}
