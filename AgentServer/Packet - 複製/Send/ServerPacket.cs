using AgentServer.Structuring;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
    public sealed class NoticePacket : NetPacket
    {
        public NoticePacket(Account User, string content, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0x77);
            ns.Write(0);
            ns.Write(1);
            ns.WriteBIG5Fixed_intSize(content);
            ns.Write(last);
        }
    }
}
