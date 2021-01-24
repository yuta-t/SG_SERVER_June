using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;

namespace RelayServer.Network.Packet.Send
{
    public sealed class Connect_04FFD403 : NetPacket
    {
        public Connect_04FFD403(IPEndPoint endPoint) : base(2, 0)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3D5);
            ns.Write((short)0x02); //02 00 port length
            short port = (short)endPoint.Port;
            string ip = endPoint.Address.ToString();
            byte[] clientport = BitConverter.GetBytes(port).Reverse().ToArray();
            byte[] clientip = BitConverter.GetBytes(Utility.IPToInt(ip)).Reverse().ToArray();
            ns.Write(clientport, 0, 2);
            ns.Write(clientip, 0, 4);
            ns.Fill(8);
        }
    }

    public sealed class Connect_04FFD803 : NetPacket
    {
        public Connect_04FFD803() : base(2, 0)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3D9);
        }
    }

    public sealed class Connect_04FFD603 : NetPacket
    {
        public Connect_04FFD603(byte[] array) : base(2, 0)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3D7);
            ns.Write(array, 0, array.Length);
        }
    }

}
