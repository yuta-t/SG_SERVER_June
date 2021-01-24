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

    public sealed class GameRoom_Hex : NetPacket
    {
        public GameRoom_Hex(Account User, string bytes, byte last) : base(1, User.EncryptKey)
        {
            //FF5805FFFFFFFFFFFFFFFF000000000000000D01
            ns.WriteHex(bytes);
            //ns.Write(bytes, 0, bytes.Length);
            ns.Write(last); //end
        }
    }

    public sealed class GameRoom_SendRoomInfo : NetPacket
    {
        public GameRoom_SendRoomInfo(Account User, NormalRoom room, byte last, byte roompos = 0) : base(1, User.EncryptKey)
        {
            byte roomkindid = (byte)room.RoomKindID;
            string name = room.Name;
            string password = room.Password;
            int isTeamPlay = room.IsTeamPlay;
            bool isStepOn = room.IsStepOn;
            int itemtype = room.ItemType;

            //byte header = 0xA5;
            ns.Write((byte)0xA5); //opcode
            ns.Write(0);
            ns.Write(roomkindid);
            ns.WriteBIG5Fixed_intSize(name);
            ns.Write(room.MaxPlayersCount);
            ns.WriteBIG5Fixed_intSize(password);
            int unk1 = 9;
            ns.Write(unk1);
            int roomid = room.ID;
            ns.Write(roomid);
            //byte[] unk2 = { 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 };
            //ns.Write(unk2, 0, unk2.Length);
            ns.Write(roompos);
            ns.Write(room.MapNum); //mapnum?
            ns.Write(10);
            ns.Write(isTeamPlay);
            ns.Write(isStepOn);
            ns.Write(itemtype);
            ns.Write(0);
            ns.Write(Utility.CurrentTimeMilliseconds());
            ns.Write(room.PosWeight); //位
            ns.Write(last); //end
        }
    }

    public sealed class GameRoom_SendPlayerInfo : NetPacket
    {
        public GameRoom_SendPlayerInfo(Account User, byte last) : base(1, User.EncryptKey)
        {
            //Account User = Client.CurrentAccount;
            ns.Write((byte)0xA6); //op code
            ns.Write(User.Session); //User session
            ns.Write(User.RoomPos);
            ns.Write((short)0x2); //port length?
            byte[] clientport = BitConverter.GetBytes(User.UDPPort).Reverse().ToArray();
            byte[] clientip = BitConverter.GetBytes(Utility.IPToInt(User.LastIp)).Reverse().ToArray();
            ns.Write(clientport, 0, 2);
            ns.Write(clientip, 0, 4);
            ns.Write(0L);
            ns.Write((short)0x2); //port length?
            ns.Write(clientport, 0, 2); //內網ip
            ns.Write(clientip, 0, 4);
            ns.Write(0L);
            ns.Write((short)0x2);
            byte[] relayserverport = BitConverter.GetBytes(Conf.RelayPort).Reverse().ToArray();
            ns.Write(relayserverport, 0, 2);
            ns.Write(BitConverter.GetBytes(Utility.IPToInt(Conf.ServerIP)).Reverse().ToArray(), 0, 4);
            int unk5 = 0x78485E2C;
            ns.Write(unk5);
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(User.NickName);
            ns.Fill(0x10);

            ns.Write((short)0x64); //avatarInfoHeader
            for (int i = 0; i < 15; i++)
            {
                ns.Write(User.CurrentAvatarInfo[i]);
            }
            ns.Fill(0x88); //0x88
            for (int i = 15; i < 30; i++)
            {
                ns.Write(User.CurrentAvatarInfo[i]);
            }
            ns.Fill(0x88); //0x88
            ns.Write(User.costumeMode);
            ns.Write((short)1);//00 00

            ns.Write(User.Exp);
            ns.Write((short)0);
            ns.Write(User.RoomPos);
            ns.Fill(0x14);
            //login packet
            ns.Write(-1);
            ns.Write(0L);
            ns.Write(0x00068BB6671178CB);
            ns.Write(0x00068BB6671178CB);
            ns.Write(0x00068BB6671178CB);
            ns.Write(-1L);
            ns.Write(0xFFFF);
            ns.Write(-1);
            ns.Write((byte)0);
            ns.Fill(0x10);

            ns.Write(1);
            /*long countpos = ns.Position;
            short count = 0;
            ns.Write(count);//count
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getActiveFuncItem";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("position", MySqlDbType.Int32).Value = -1;
                cmd.Parameters.Add("expiredcheck", MySqlDbType.Int32).Value = 0;
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ns.Write(0x8C7C98); //98 7C 8C 00
                    ns.Write(Convert.ToInt16(reader["character"]));
                    ns.Write(Convert.ToUInt16(reader["position"]));
                    ns.Write(Convert.ToInt16(reader["kind"]));
                    ns.Write(Convert.ToInt32(reader["itemdescnum"]));//4 bytes
                    ns.Write(Convert.IsDBNull(reader["expireTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                    ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                    ns.Write(Convert.ToInt32(reader["count"])); //4 bytes
                    ns.Write(Convert.ToInt32(reader["exp"]));
                    ns.Write((byte)0);
                    ns.Write(Convert.ToBoolean(reader["using"]));
                    //ns.Write((short)0x100); //unknown?? 00 01 / 01 00 / 01 01 / 00 00
                    count++;
                }
                ns.Write((short)0);//  daily buff count?
                long itemendpos = ns.Position;
                ns.Seek((int)countpos, SeekOrigin.Begin);
                ns.Write(count);
                ns.Seek((int)itemendpos, SeekOrigin.Begin);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/
            ns.Write((short)User.AvatarItems.Count); //count
            foreach (var item in User.AvatarItems)
            {
                ns.Write(0x8C7C98); //98 7C 8C 00
                ns.Write(item.character);
                ns.Write(item.position);
                ns.Write(item.kind);
                ns.Write(item.itemdescnum);
                ns.Write(item.expireTime);
                ns.Write(item.gotDateTime);
                ns.Write(item.count);
                ns.Write(item.exp);
                ns.Write((byte)0);
                ns.Write(item.use);
            }
            ns.Write((short)0); //  daily buff count?
            //ns.Write((short)0);//  daily buff count?
            ns.Write(User.UserItemAttr.Count);
            foreach (var item in User.UserItemAttr)
            {
                ns.Write(item.Key);
                ns.Write(item.Value.Count);
                foreach (var attr in item.Value)
                {
                    ns.Write(attr.Attr);
                    ns.Write(attr.AttrValue);
                }
            }
            ns.Fill(0x51); //守護靈 0x51
            ns.Write(User.TopRank);
            ns.Write(0);
            //時裝
            ns.Write(1); //count
            ns.Write(17); //17:光光
            ns.Write((byte)0); //時裝on off?
            ns.Write((byte)0); //時裝on off?
            /*ns.Fill(0x5F); //守護靈
            ns.Write(-1);
            ns.Fill(0x14);*/
            int unk8 = 0x3F13A127;//27 A1 13 3F
            ns.Write(unk8);
            ns.Write((byte)1);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_ControlRoomPos : NetPacket
    {
        public GameRoom_ControlRoomPos(Account User, byte roompos, bool isOff, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0X301);
            ns.Write(roompos);
            ns.Write(isOff);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_KickPlayer : NetPacket
    {
        public GameRoom_KickPlayer(Account User, byte roompos, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0X302);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GetRoomList : NetPacket
    {
        public GameRoom_GetRoomList(Account User, List<NormalRoom> rooms, byte roomkindid, int page, byte last) : base(1, User.EncryptKey)
        {
            List<NormalRoom> pagedRooms = rooms.Skip(page * 16).Take(16).ToList();
            int maxpage = Convert.ToInt32(Math.Ceiling(rooms.Count / Convert.ToDouble(16))) - 1;
            ns.Write((byte)0x98);
            ns.Write(roomkindid);
            ns.Write(maxpage);
            ns.Write(page);
            ns.Write((short)pagedRooms.Count);
            foreach (NormalRoom room in pagedRooms)
            {
                byte MaxPlayer;
                if (room.is8Player)
                    MaxPlayer = room.SlotCount;
                else
                    MaxPlayer = room.MaxPlayersCount;
                bool hasPiero = room.Players.Exists(player => player.Attribute == 1);
                bool hasAfreecaTV = room.Players.Exists(player => player.Attribute == 3);
                ns.Write(room.ID);
                ns.WriteBIG5Fixed_intSize(room.Name);
                ns.Write(!room.HasPassword);
                ns.Write((byte)room.Players.FindAll(p => p.Attribute != 3).Count);
                ns.Write(MaxPlayer);
                ns.Write(!room.isPlaying);
                ns.Write(room.IsStepOn);
                ns.Write(room.ItemType);
                ns.Write(room.MapNum);
                ns.Write(6);//default 17
                ns.Write((byte)(room.IsTeamPlay == 2 ? 1 : 0));
                ns.Write(hasAfreecaTV);//非洲電視
                ns.Write(hasPiero);//小丑房間
                ns.Write((byte)0);//助理圍巾
                ns.Write((byte)0);//男數量
                ns.Write((byte)0);//女數量
                ns.Write((byte)1);
                ns.Write(-1);
                ns.Write((byte)0);//buff
                ns.Write(room.ItemNum);//房間獎勵
                ns.Write(0);//BONUS STAGE Level
                ns.Write((byte)0);
            }
            ns.Seek(ns.Position - 1, SeekOrigin.Begin);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RemoveRoomUser : NetPacket
    {
        public GameRoom_RemoveRoomUser(Account User, byte roompos, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xA8);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_LeaveRoomUser_0XA9 : NetPacket
    {
        public GameRoom_LeaveRoomUser_0XA9(Account User, byte roompos, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xA9);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RoomPosReady : NetPacket
    {
        public GameRoom_RoomPosReady(Account User, byte roompos, bool isReady, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2E7);
            ns.Write(roompos);
            ns.Write(isReady);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GetRoomMaster : NetPacket
    {
        public GameRoom_GetRoomMaster(Account User, byte roompos, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2DD); //opcode
            ns.Write((byte)roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeMap_FF6605 : NetPacket
    {
        public GameRoom_ChangeMap_FF6605(Account User, int mapid, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x566);
            ns.Write(mapid);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeMap_FFEA02 : NetPacket
    {
        public GameRoom_ChangeMap_FFEA02(Account User, int mapid, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2EA);
            ns.Write(mapid);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeSetting_FFD902 : NetPacket
    {
        public GameRoom_ChangeSetting_FFD902(Account User, NormalRoom room, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2D9);
            ns.WriteBIG5Fixed_intSize(room.Name);
            ns.WriteBIG5Fixed_intSize(room.Password);
            ns.Write(room.IsStepOn);
            ns.Write(room.ItemType);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_EndLoading_FF1103 : NetPacket
    {
        public GameRoom_EndLoading_FF1103(Account User, byte roompos, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x311);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StartLoading_FF0B03 : NetPacket
    {
        public GameRoom_StartLoading_FF0B03(Account User, int mapid, int randseed, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x30B);
            ns.Write(mapid);
            ns.Write(randseed);
            ns.Write((byte)0);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_START_GAME_RES : NetPacket
    {
        public GameRoom_START_GAME_RES(Account User, int iGameStartTick, int iNumberOfItem, byte last) : base(1, User.EncryptKey)
        {
            //FF 14 03 41 00 00 00 C8 00 00 00 99 AC 09 00
            ns.Write((byte)0xFF);
            ns.Write((short)0x314);
            ns.Write(iGameStartTick);
            ns.Write(iNumberOfItem);
            ns.Write(0x816A4); //A4 16 08 00  //99 AC 09 00
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GoalInData : NetPacket
    {
        public GameRoom_GoalInData(Account User, int LapTime, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x353);
            ns.Write(User.RoomPos);
            ns.Write(LapTime);
            ns.Write(0);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GameEndData : NetPacket
    {
        public GameRoom_GameEndData(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3E3);
            ns.Write(User.RoomPos);
            ns.Write(2);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_StartTimeOutCount : NetPacket
    {
        public GameRoom_StartTimeOutCount(Account User, int LapTime, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x37C);
            ns.Write(LapTime); //dwGametime=LapTime+2000
            ns.Write((byte)10); //nTimeOutSeconds=10
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RoomChat : NetPacket
    {
        public GameRoom_RoomChat(Account User, byte[] text, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2DE);
            ns.Write(User.RoomPos);
            short len = (short)text.Length;
            ns.Write(len);
            ns.Write(text, 0, len);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeStatus : NetPacket
    {
        public GameRoom_ChangeStatus(Account User, byte pos, int code, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2C9);
            ns.Write(pos);
            ns.Write(code);
            ns.Write(last);
        }
    }
    /*
    public sealed class GameRoom_GameResult : NetPacket
    {
        public GameRoom_GameResult(Account User, NormalRoom room, byte last) : base(1, User.EncryptKey)
        {
            /*FF 81 03 04 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00
             00 00 00 21 CA 00 00 36 CA 00 00 01 00 55 00 00 00 00 00
             00 00 23 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 3F 00 17 00 0C 00 00 00 02 00 00 00 01 00 00 00
             00 00 00 00 00 F7 98 1C 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 00 00 00 E4 D5 1D 00 00 00 00 00 00
             5C D5 00 00 48 D5 00 00 02 01 10 00 00 00 00 00 00 00 09
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 70 02 00
             00 00 F7 98 1C 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 E6 D5 1D 00 00 00 00 00 00 00 00 02
             00 00 00 EA 03 00 00 11 00 00 00 FF FF FF FF FF FF FF FF
             FF FF FF FF 00 01 00 00 00 F6 03 00 00 04 00 00 00 00 00
             00 00 10

            ns.Write((byte)0xFF);
            ns.Write((short)0x381);
            ns.Write((byte)4);
            ns.Fill(0xB);
            ns.Write((byte)room.Players.FindAll(p => p.Attribute != 3).Count); //count?
            ns.Fill(0x6);
            //ns.Write(User.ServerLapTime); //len: 5C
            //ns.Write(User.LapTime);
            //rank
            //index
            //exp
            //tr
            //card
            foreach (var p in room.Players.Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { player = p, droplist = d }).OrderBy(o => o.droplist.Rank))
            {
                ns.Write(p.player.ServerLapTime);
                ns.Write(p.player.LapTime);
                ns.Write(p.droplist.Rank);
                ns.Write(p.player.RoomPos);
                ns.Write(p.droplist.EXP);
                ns.Write(p.droplist.TR);
                ns.Fill(0xC);
                ns.Write(p.droplist.BounsEXP);
                ns.Write(p.droplist.BounsTR);
                ns.Write(0x4); // 0C 00 00 00
                ns.Write(0x2); //card
                int count = p.droplist.CardID.Count;
                ns.Write(count); //card count
                if (count > 0) {
                    foreach (var card in p.droplist.CardID)
                    {
                        ns.Write(card);
                    }
                }
                else
                {
                    ns.Write(0);
                }
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B000000000000");
            }
            ns.WriteHex("000002000000EA03000011000000FFFFFFFFFFFFFFFFFFFFFFFF0001000000F60300000400000000000000");
            ns.Write(last);
        }
    }
    */
    public sealed class GameRoom_GameResult2 : NetPacket
    {
        public GameRoom_GameResult2(Account User, byte[] result) : base(1, User.EncryptKey)
        {
            ns.Write(result, 0, result.Length);
        }
    }

    public sealed class GameRoom_GameOver : NetPacket
    {
        public GameRoom_GameOver(Account User, byte pos, byte last) : base(1, User.EncryptKey)
        {
            //FF 3E 03 1A 02 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x33E); //opcode
            ns.Write(pos);
            ns.Write(2); //02 00 00 00
            ns.Write(last);
        }
    }
    public sealed class GameRoom_alive : NetPacket
    {
        public GameRoom_alive(Account User, byte pos, byte last) : base(1, User.EncryptKey)
        {
            //FF 3F 03 01 00 00 00 16 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x33F); //opcode
            ns.Write(1); //01 00 00 00
            ns.Write(pos);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GameUpdateEXP : NetPacket
    {
        public GameRoom_GameUpdateEXP(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xAA);
            ns.Write(0x2710);//ladder point?
            ns.Write(User.Exp);
            ns.Write(0x259); //601
            ns.Write(0L);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_UpdateIndividualGameRecord : NetPacket
    {
        public GameRoom_UpdateIndividualGameRecord(Account User, byte last) : base(1, User.EncryptKey)
        {
            int clear = 0, first = 0, second = 0, third = 0;
            int playCount = 0, distance = 0, clearCount = 0, firstCount = 0, secondCount = 0, thirdCount = 0;
            if (User.Rank < 4) {
                first = User.Rank == 1 ? 1 : 0;
                second = User.Rank == 2 ? 1 : 0;
                third = User.Rank == 3 ? 1 : 0;
            }
            clear = User.GameEndType == 1 ? 1 : 0;

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_IndividualRecordUpdateGame";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("clear", MySqlDbType.Int32).Value = clear;
                    cmd.Parameters.Add("distance", MySqlDbType.Int32).Value = User.RaceDistance;
                    cmd.Parameters.Add("first", MySqlDbType.Int32).Value = first;
                    cmd.Parameters.Add("second", MySqlDbType.Int32).Value = second;
                    cmd.Parameters.Add("third", MySqlDbType.Int32).Value = third;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        playCount = Convert.ToInt32(reader["playCount"]);
                        distance = Convert.ToInt32(reader["distance"]);
                        clearCount = Convert.ToInt32(reader["clearCount"]);
                        firstCount = Convert.ToInt32(reader["firstCount"]);
                        secondCount = Convert.ToInt32(reader["secondCount"]);
                        thirdCount = Convert.ToInt32(reader["thirdCount"]);
                    }
                }
            }
            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_IndividualRecordUpdateGame";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("clear", MySqlDbType.Int32).Value = clear;
                cmd.Parameters.Add("distance", MySqlDbType.Int32).Value = User.RaceDistance;
                cmd.Parameters.Add("first", MySqlDbType.Int32).Value = first;
                cmd.Parameters.Add("second", MySqlDbType.Int32).Value = second;
                cmd.Parameters.Add("third", MySqlDbType.Int32).Value = third;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                Console.WriteLine("UpdateIndividualGameRecord1.2");
                playCount = Convert.ToInt32(reader["playCount"]);
                distance = Convert.ToInt32(reader["distance"]);
                clearCount = Convert.ToInt32(reader["clearCount"]);
                firstCount = Convert.ToInt32(reader["firstCount"]);
                secondCount = Convert.ToInt32(reader["secondCount"]);
                thirdCount = Convert.ToInt32(reader["thirdCount"]);
                Console.WriteLine("UpdateIndividualGameRecord1.3");
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x22D);
            ns.Write(playCount);
            ns.Write(distance);
            ns.Write(clearCount);
            ns.Write(firstCount);
            ns.Write(secondCount);
            ns.Write(thirdCount);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_MapControl : NetPacket
    {
        public GameRoom_MapControl(Account User, short len, byte[] unk, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2CB);
            ns.Write(len);
            ns.Write(unk, 0, unk.Length);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_TriggerMapEvent : NetPacket
    {
        public GameRoom_TriggerMapEvent(Account User, int eventlaptime, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x35D);
            ns.Write((byte)0x2);
            ns.Write(eventlaptime);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GiveUpItem : NetPacket
    {
        public GameRoom_GiveUpItem(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x321);
            ns.Write(User.RoomPos);
            ns.Write((byte)0x1);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_DrawItem : NetPacket
    {
        public GameRoom_DrawItem(Account User, int unk1, int unk2, int itemid, byte last) : base(1, User.EncryptKey)
        {
            //FF 1B 03 00 00 00 00 00 51 34 00 00 0B 00 00 00 CF 00 00 00 00 02
            ns.Write((byte)0xFF);
            ns.Write((short)0x31B);
            ns.Write(User.RoomPos);
            ns.Write(unk2);
            ns.Write(unk1);
            ns.Write(itemid);//item id
            ns.Write(0xCF); //next id??
            ns.Write((byte)0x0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_UseItem : NetPacket
    {
        public GameRoom_UseItem(Account User, byte pos, int rand, int itemid, byte[] bytes, byte last) : base(1, User.EncryptKey)
        {
            /*FF 1E 03 00 A0 F3 00 00 0B 00 00 00 45 00 D0 00
             00 00 0B 00 00 00 A0 F3 00 00 00 00 C6 5E F3 C3 
             B6 2C 38 C5 9C 59 2A 44 00 00 00 00 00 00 00 00
             00 00 00 00 F3 04 B5 B9 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00
             00 00 00 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x31E);
            ns.Write(pos);
            ns.Write(rand);
            ns.Write(itemid);
            ns.Write(bytes, 0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RoomPosTeam : NetPacket
    {
        public GameRoom_RoomPosTeam(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2ED);
            ns.Write(User.RoomPos);
            ns.Write(User.Team);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StartError : NetPacket
    {
        public GameRoom_StartError(Account User, int error, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x308);
            ns.Write(error);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RegisterSuccess : NetPacket
    {
        public GameRoom_RegisterSuccess(Account User, int itemnum, long storage_id, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x557);
            ns.Write(0);
            ns.Write(itemnum);
            ns.Write(storage_id);
            ns.Write(0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GoodsInfo : NetPacket
    {
        public GameRoom_GoodsInfo(Account User, NormalRoom room, byte last) : base(1, User.EncryptKey)
        {
            //FF5805FFFFFFFF00000000000000000000000000
            ns.Write((byte)0xFF);
            ns.Write((short)0x558);
            ns.Write(room.ItemNum);
            ns.Write(0);
            ns.Write(room.isOrderBy);
            ns.Write(room.SendRank);
            ns.Write(room.isPublic);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StepOnButton : NetPacket
    {
        public GameRoom_StepOnButton(Account User, byte[] unk, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2FF);
            ns.Write(unk, 0, 0xC);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_LockKeepItem : NetPacket
    {
        public GameRoom_LockKeepItem(Account User, NormalRoom room, bool isCancel, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x555);
            ns.Write(1);
            ns.Write(2L);
            ns.Write(room.Storage_Id);
            ns.Write(room.ItemNum);
            ns.Write(Utility.ConvertToTimestamp(DateTime.Now));//TODO: 讀取保管日期
            ns.Write(1);
            ns.Write(1);
            ns.Write(isCancel ? 0 : (float)1);
            ns.Write(0L); 
            /*ns.Write((short)0);
            ns.Write(isCancel ? 0 : 0x3F80);//80 3F 00 00 00 00 00 00
            ns.Write(0);
            ns.Write((short)0);*/
            ns.Write(last);
        }
    }
    public sealed class GameRoom_DeleteKeepItem : NetPacket
    {
        public GameRoom_DeleteKeepItem(Account User, NormalRoom room, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x555);
            ns.Write(1);
            ns.Write(1L);
            ns.Write(room.Storage_Id);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_SendRoomMaster : NetPacket
    {
        public GameRoom_SendRoomMaster(Account User, int mapnum, byte roommasterindex, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((byte)0xB9);
            ns.Write((byte)0x1);
            ns.Write(mapnum);
            ns.Write(roommasterindex);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_CreateRoomError : NetPacket
    {
        public GameRoom_CreateRoomError(Account User, int errorid, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0x9E);
            ns.Write(errorid);//5: Level limited, 8: No room server, 9: map incorrect
            ns.Write(last);
        }
    }
    public sealed class GameRoom_EnterRoomError : NetPacket
    {
        public GameRoom_EnterRoomError(Account User, byte errorid, byte roomkindid, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xA5);
            ns.Write(0x41);
            ns.Write(errorid);
            ns.Write(roomkindid);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GoalInError : NetPacket
    {
        public GameRoom_GoalInError(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3E0);
            ns.Write(0x102);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_MaxSpeedTube : NetPacket
    {
        public GameRoom_MaxSpeedTube(Account User, byte[] unk, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x370);
            ns.Write(unk, 0, 0xC);
            ns.Write(last);
        }
    }
}
