using LocalCommons.Network;
using LocalCommons.Utilities;
using LoadBalanceServer.Network.Connections;

namespace LoadBalanceServer.Packet.Send
{
    public sealed class NP_Hex : NetPacket
    {
        public NP_Hex(string value) : base(3, 0)
        {
            ns.WriteHex(value);
        }
    }
    public sealed class NP_Byte : NetPacket
    {
        public NP_Byte(byte[] value) : base(3, 0)
        {
            ns.Write(value, 0, value.Length);
        }
    }

    public sealed class HashCheckOK : NetPacket
    {
        public HashCheckOK() : base(3, 0)
        {
            /*1B 00 02 0F 00 00 00 32 31 30 2E 32 34 32 2E 32 30 32 2E 31 35 32 C1 23 00 00 01*/

            ns.Write((byte)0x02); //op
            ns.WriteBIG5Fixed_intSize(Conf.ServerIP);
            ns.Write(Conf.AgentPort);
            ns.Write((byte)0x01);
        }
    }

    public sealed class HashCheckFail : NetPacket
    {
        public HashCheckFail() : base(3, 0)
        {
            /*1B 00 02 0F 00 00 00 32 31 30 2E 32 34 32 2E 32 30 32 2E 31 35 32 C1 23 00 00 01*/

            ns.Write((byte)0x02); //op
            ns.Write(0L);
            ns.Write((byte)0x00);
        }
    }


}
