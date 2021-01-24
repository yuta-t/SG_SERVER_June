using AgentServer.Structuring;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
    public sealed class NoticePacket : NetPacket
    {
        public NoticePacket(Account User, string content, byte last)
        {
            ns.Write((byte)0x77);
            ns.Write(0);
            ns.Write(1);
            ns.WriteBIG5Fixed_intSize(content);
            ns.Write(last);
        }
    }
    public sealed class DisconnectPacket : NetPacket
    {
        public DisconnectPacket(Account User, int msgid, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3E0);
            ns.Write(msgid);//257=Other player try to login this account, 258=goalin too fast
            ns.Write(last);
        }
    }
}
