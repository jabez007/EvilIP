using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EvilPacket
{
    public class EvilPacket
    {
        /*
         *  // IP Header
            0x45,                   // ip[0:1] = version and header length
            0x00,                   // ip[1:1] = type of service
            0x00, 0x1c,             // ip[2:2] = total length (number of bytes in entire datagram)  
            0x00, 0x00,             // ip[4:2] = Identification
            0x80, 0x00,             // ip[6:2] = R, DF, MF, fragment offset
            0xff,                   // ip[8:1] = TTL (255 max)
            0x01,                   // ip[9:1] = protocol (ICMP)
            0x00, 0x00,             // ip[10:2] = header checksum
            0x00, 0x00, 0x00, 0x00, // ip[12:4] = source address
            0x00, 0x00, 0x00, 0x00, // ip[16:4] = destination address
                                    // ip[20:40] = options (only if header length is greater than 5)
            // Protocol Header
            0x08,                   // icmp[0:1] = Type (echo request)
            0x00,                   // icmp[1:1] = Code
            0x00, 0x00,             // icmp[2:2] = header checksum
            0x00, 0x00, 0x00, 0x00  // icmp[4:4] = other message specific information
            // Data
         */

        public ushort id { get; }
        public IPAddress src { get; }
        public IPAddress dst { get; }
        public byte hiddenData { get; }
        public bool evil { get; } = true;

        public EvilPacket(string destination, ushort messageId, byte data)
        {
            id = messageId;

            dst = GetAddress(destination);
            if (IPAddress.IsLoopback(dst))
            {
                src = IPAddress.Loopback;
            }
            else
            {
                src = GetAddress(Environment.MachineName);
            }

            hiddenData = data;
        }

        public EvilPacket(byte[] receivedPacket)
        {
            if (receivedPacket[6] != 0x80 && receivedPacket[20] != 0x08) { evil = false; }

            // id = receivedPacket[4] + receivedPacket[5]
            id = BitConverter.ToUInt16(receivedPacket, 4);

            // src = receivedPacket[12:4]
            src = new IPAddress(((receivedPacket.ToList()).GetRange(12, 4)).ToArray());

            // dst = receivedPacket[16:4]
            dst = new IPAddress(((receivedPacket.ToList()).GetRange(16, 4)).ToArray());

            hiddenData = receivedPacket[21];
        }

        public override string ToString()
        {
            List<string> thisString = new List<string>();

            StringBuilder thisStringRow = new StringBuilder();
            foreach (string hex in BitConverter.ToString(ToBytes()).Split('-'))
            {
                thisStringRow.Append(hex);

                if (thisStringRow.Length == 11)
                {
                    thisString.Add(thisStringRow.ToString());
                    thisStringRow.Clear();
                }
                else
                {
                    thisStringRow.Append(" ");
                }
            }

            return string.Join("\n", thisString);
        }

        public byte[] ToBytes()
        {
            List<byte> datagram = new List<byte>();

            #region IP Header
            List<byte> ip = new List<byte>();

            ip.Add(0x45);  // version and header length
            ip.Add(0x00);  // type of service

            // total datagram length place holders
            ip.Add(0x00);
            ip.Add(0x00);

            foreach (byte b in BitConverter.GetBytes(id).Reverse())  // should be just two bytes
            {
                ip.Add(b); // Identification
            }

            ip.Add(0x80);  // Evil bit set
            ip.Add(0x00);  // fragment offset
            ip.Add(0xff);  // TTL (maxed)
            ip.Add(0x01);  // ICMP
            // IP header checksum place holders
            ip.Add(0x00);
            ip.Add(0x00);

            foreach (byte b in src.GetAddressBytes())  // should be 4 bytes
            {
                ip.Add(b);  // source address
            }

            foreach (byte b in dst.GetAddressBytes())  // should be 4 bytes
            {
                ip.Add(b);  // destination address
            }
            #endregion

            #region ICMP Header
            List<byte> icmp = new List<byte>();

            icmp.Add(0x08);  // Type (echo request)
            icmp.Add(hiddenData);  // hide our data here in the code of the echo request
            // ICMP checksum place holders
            icmp.Add(0x00);
            icmp.Add(0x00);
            // Other message specific information
            icmp.Add(0x00);
            icmp.Add(0x00);
            icmp.Add(0x00);
            icmp.Add(0x00);

            // calculate the ICMP checksum
            ushort _icmpChecksum = CalculateChecksum(icmp.ToArray());
            byte[] _icmpChecksumBytes = BitConverter.GetBytes(_icmpChecksum);
            icmp[2] = _icmpChecksumBytes[1];
            icmp[3] = _icmpChecksumBytes[0];
            #endregion

            // calculate total datagram length and update the IP header
            byte[] _totalLength = BitConverter.GetBytes(ip.Count + icmp.Count);  // should be just two bytes
            ip[2] = _totalLength[1];
            ip[3] = _totalLength[0];

            // and set the checksum for the IP header now that we have all the info
            ushort _ipChecksum = CalculateChecksum(ip.ToArray());
            byte[] _ipChecksumBytes = BitConverter.GetBytes(_ipChecksum);
            ip[10] = _ipChecksumBytes[1];
            ip[11] = _ipChecksumBytes[0];

            // put our packet together
            datagram.AddRange(ip);
            datagram.AddRange(icmp);

            return datagram.ToArray();
        }

        private IPAddress GetAddress(string address)
        {
            IPAddress resolved;

            try
            {
                IPAddress[] addressList = Dns.GetHostAddresses(address);
                resolved = addressList[0];
            }
            catch (SocketException e)  // bad string passed to GetHostAddresses
            {
                throw new EvilPacketException(string.Format("Unable to resolve {0} ( {1} )", address, e.Message));
            }

            return resolved;
        }

        /// <summary>
        /// method for computing the 16-bit one's complement checksum of a byte buffer.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private static ushort CalculateChecksum(byte[] header)
        {
            long sum = 0;

            ushort word16;
            for (int i = 0; i < header.Length; i += 2)
            {
                word16 = (ushort)(((header[i] << 8) & 0xFF00)
                + (header[i + 1] & 0xFF));
                sum += word16;
            }

            while ((sum >> 16) != 0)
            {
                sum = (sum & 0xFFFF) + (sum >> 16);
            }

            sum = ~sum;

            return (ushort)sum;
        }
    }

    public class EvilPacketException : Exception
    {
        public EvilPacketException(string message):
            base(message)
        {

        }
    }
}
