using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EvilClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            IPEndPoint source = new IPEndPoint(IPAddress.Loopback, 0);
            sendSocket.Bind(source);
            sendSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            IPEndPoint destination = new IPEndPoint(IPAddress.Loopback, 0);

            EvilPacket.EvilPacket packet;
            foreach (byte b in Encoding.UTF8.GetBytes("Hello world"))
            {
                packet = new EvilPacket.EvilPacket("localhost", ushort.MaxValue, b);
                sendSocket.SendTo(packet.ToBytes(), destination);
            }            
        }
    }
}
