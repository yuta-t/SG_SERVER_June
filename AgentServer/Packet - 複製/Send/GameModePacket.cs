using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Network.Connections;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;

namespace AgentServer.Packet.Send
{


    public sealed class GameRoom_LapTimeCountdwon : NetPacket
    {
        public GameRoom_LapTimeCountdwon(Account User, short second, byte round, byte last) : base(1, User.EncryptKey)
        {
            //FF 16 03 01 06 00 00 01
            //FF 16 03 01 06 00 01 20
            //FF 16 03 01 06 00 02 20
            ns.Write((byte)0xFF);
            ns.Write((short)0x316);
            ns.Write((byte)0x1);
            ns.Write(second);
            ns.Write(round);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_LapTimeCountdwon2 : NetPacket
    {
        public GameRoom_LapTimeCountdwon2(Account User, short second, byte round, byte last) : base(1, User.EncryptKey)
        {
            //FF 17 03 00 06 00 01 02 00 00 00 00 00 00 00 02 00 00 00 02
            //FF 17 03 01 06 00 01 02 00 00 00 00 00 00 00 01 00 00 00 80
            //FF 17 03 02 06 00 01 02 00 00 00 00 00 00 00 02 00 00 00 80
            //FF 17 03 02 06 00 01 02 00 00 00 00 00 00 00 01 00 00 00 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x317);
            ns.Write(round);
            ns.Write(second);
            ns.Write((byte)0x1);
            ns.Write(2);
            ns.Write(0);
            //int remainround = 2 - round;
            ns.Write(2);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_MiniGame_RoundTime : NetPacket
    {
        public GameRoom_MiniGame_RoundTime(Account User, int currentround, int isnextround, float RoundTime, byte last) : base(1, User.EncryptKey)
        {
            //FF 60 05 00 00 00 00 00 00 00 00 00 00 A0 41 04 //0

            //FF 60 05 02 00 00 00 00 00 00 00 00 00 20 42 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x560);
            ns.Write(currentround);
            ns.Write(isnextround);
            ns.Write(RoundTime);
            ns.Write(last);

            //FF 60 05 00 00 00 00 01 00 00 00 00 00 80 3F 08 //1
            //FF 60 05 01 00 00 00 01 00 00 00 00 00 80 3F 08 //2
            //FF 60 05 02 00 00 00 01 00 00 00 00 00 80 3F 10 //3
        }
    }
    public sealed class GameRoom_MiniGame_GetPoint : NetPacket
    {
        public GameRoom_MiniGame_GetPoint(Account User, int nowpoint, int getpoint, byte last) : base(1, User.EncryptKey)
        {
            //FF 5C 05 00 15 00 00 00 14 00 00 00 00 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x55C);
            ns.Write(User.RoomPos);
            ns.Write(nowpoint);
            ns.Write(getpoint);
            ns.Write(0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_UpdatePoint : NetPacket
    {
        public GameRoom_MiniGame_UpdatePoint(Account User, NormalRoom room, byte last) : base(1, User.EncryptKey)
        {
            //FF 5D 05 02 00 00 00 00 00 00 00 00 01 00 00 00 00 08
            //FF 5D 05 02 00 00 00 00 00 00 00 00 02 00 00 00 00 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x55D);
            ns.Write(room.Players.Count(p => p.Attribute != 3));
            foreach (var p in room.Players.Where(p => p.Attribute != 3)
                           .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { p, d })
                                .OrderBy(o => o.d.MiniGamePoint).ThenBy(o => o.p.RoomPos))
            {

                ns.Write(p.p.RoomPos);
                ns.Write(p.d.MiniGamePoint);
            }
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_55F : NetPacket
    {
        public GameRoom_MiniGame_55F(Account User, int round, byte last) : base(1, User.EncryptKey)
        {
            //FF 5F 05 00 00 00 00 00 00 00 00 00 00 00 00 04 場主only?

            //FF 5F 05 02 00 00 00 00 00 00 00 00 00 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x55F);
            ns.Write(round);
            ns.Write(0L);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_GameOver : NetPacket
    {
        public GameRoom_MiniGame_GameOver(Account User, byte pos, byte last) : base(1, User.EncryptKey)
        {
            //FF 3E 03 00 02 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x33E);
            ns.Write(pos);
            ns.Write(2);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_Respawn : NetPacket
    {
        public GameRoom_MiniGame_Respawn(Account User, int pos, byte last) : base(1, User.EncryptKey)
        {
            //FF 5A 05 00 00 00 00 00 00 00 00 01 00 00 00 20
            //FF 5A 05 00 00 00 00 00 00 00 00 00 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x55A);
            ns.Write(0L);
            ns.Write(pos);
            ns.Write(last);
        }
    }



}
