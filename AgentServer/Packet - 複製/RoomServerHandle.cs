using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using AgentServer.Database;
using LocalCommons.Utilities;
using System.Text;
using System.Collections.Generic;
using LocalCommons.Logging;
using System.Threading;
using System.Threading.Tasks;
using AgentServer.Structuring.Map;

namespace AgentServer.Packet
{
    public class RoomServerHandle
    {

        public static void Handle_SlotControl(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            if (isRoomMaster && room.is8Player)
            {
                byte roompos = reader.ReadByte();
                bool isOff = reader.ReadBoolean();

                //可用位置
                if (isOff)
                    room.PosList.Remove(roompos);
                else
                    room.PosList.Add(roompos);

                room.setPosWeight(roompos, isOff);
                room.setSlotCount((byte)(room.PosList.Count() + 1));

                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_ControlRoomPos(RoomPlayer, roompos, isOff, last));
                }

            }
        }
        public static void Handle_ChangeSetting(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            if (isRoomMaster || room.RoomKindID != 0x4A)
            {
                int namelength = reader.ReadLEInt32();
                string name = reader.ReadBig5StringSafe(namelength);
                int passwordlength = reader.ReadLEInt32();
                string password = string.Empty;
                if (passwordlength > 0)
                    password = reader.ReadBig5StringSafe(passwordlength);
                bool isStepOn = reader.ReadBoolean();
                int itemtype = reader.ReadLEInt32();

                room.setName(name);
                room.setPassword(password);
                room.setItemType(itemtype);
                room.setIsStepOn(isStepOn);

                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_ChangeSetting_FFD902(RoomPlayer, room, last));
                }
            }
        }
        public static void Handle_Ready(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            //bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            bool isReady = reader.ReadBoolean();
            User.IsReady = isReady;
            byte readyroompos = User.RoomPos;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosReady(RoomPlayer, readyroompos, isReady, last));
            }
        }
        public static void Handle_ChangeMap(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;
            if (isRoomMaster)
            {
                int mapid = reader.ReadLEInt32();
                if (!MapHolder.MapInfos.TryGetValue(mapid, out MapInfo mapinfo))
                {
                    Console.WriteLine("Unknown mapid: {0}", mapid);
                    return;
                }
                //room.MapNum = mapid;
                room.setMapNum(mapid);
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF6405F70300000000000000000000", last));
                    RoomPlayer.Connection.SendAsync(new GameRoom_ChangeMap_FF6605(RoomPlayer, mapid, last));
                    RoomPlayer.Connection.SendAsync(new GameRoom_ChangeMap_FFEA02(RoomPlayer, mapid, last));
                }
            }
        }
        public static void Handle_RoomChat(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            reader.Offset += 1; //FF
            short len = reader.ReadLEInt16();
            byte[] text = reader.ReadByteArray(len);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_RoomChat(User, text, last));
            }
        }
        public static void Handle_ChangeStatus(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF C9 02 0A 00 00 00 02
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            int code = reader.ReadLEInt32();
            if (User.InGame)
            {                    
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_ChangeStatus(RoomPlayer, User.RoomPos, code, last));
                }
            }
        }

        public static void Handle_StartGame(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            bool isAllReady = room.is8Player ? room.Players.FindAll(player => player.RoomPos != room.RoomMasterIndex).All(player => player.IsReady) : true;//如果是八人房間的話，則檢查除房主以外的所有人是否有準備
            if (isRoomMaster && isAllReady)
            {
                bool justchange = DateTime.Compare(DateTime.Now, room.ChangeMapTime.AddSeconds(3)) < 0;
                if (justchange)
                {
                    User.Connection.SendAsync(new GameRoom_StartError(User, 9, last));
                    return;
                }
                if (room.isOrderBy == 1 && room.SendRank > room.Players.Count(p => p.Attribute != 3))
                {
                    User.Connection.SendAsync(new GameRoom_StartError(User, 8, last));//不符合商品登錄條件，遊戲無法開始。
                    return;
                }
                if (room.IsTeamPlay == 2)
                {
                    int redteamcount = room.Players.FindAll(p => p.Team == 1).Count;
                    int blueteamcount = room.Players.FindAll(p => p.Team == 2).Count;
                    if (redteamcount != blueteamcount)
                    {
                        User.Connection.SendAsync(new GameRoom_Hex(User, "FF0C0303", last));
                        User.Connection.SendAsync(new GameRoom_StartError(User, 3, last));//團隊不適合
                        return;
                    }
                }
                Log.Info("Start game CountDown");
                room.MapNum = reader.ReadLEInt32(); //mapid
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF0703", last)); //307
                }
                Task.Run(() => GameRoomEvent.TimeoutLoading(room, room.MatchTime));
            }
        }

        public static void Handle_StartLoading(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            //bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            int mapid = reader.ReadLEInt32(); //mapid
            int randseed = reader.ReadLEInt32(); //00 92 D1 00  randseed
            room.PlayingMapNum = mapid;
            reader.ReadByte(); //00
            Log.Info("startLoading(randseed = {0})", randseed);
            if (MapHolder.MapInfos.TryGetValue(mapid, out MapInfo mapinfo))
            {
                foreach (Account RoomPlayer in room.Players)
                {
                    //FF 0B 03 89 13 00 00 92 D1 00 00 00 04
                    RoomPlayer.Connection.SendAsync(new GameRoom_StartLoading_FF0B03(RoomPlayer, mapid, randseed, last));
                }
                room.isGoal = false;
                room.isPlaying = true;
                room.RuleType = mapinfo.RuleType;
            }
        }
        public static void Handle_EndLoading(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            //bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            //Log.Info("EndLoading"); //onRecvEndLoading
            User.EndLoading = true;

            //send 自己end loading
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_EndLoading_FF1103(RoomPlayer, User.RoomPos, last));
            }

            if (room.Players.All(p => p.EndLoading == true))
            {
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF1203", last));
                }
            }
        }

        public static void Handle_GameStart(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            //.sendStartGame(65), currentTick=0
            //FF C0 02 FF 13 03 41 00 00 00 60 6C 32 48 3B 04 00 00 C8 00 00 00 6E 00 00 00 80
            int iGameStartTick = reader.ReadLEInt32();
            reader.ReadLEInt32(); //unk1
            reader.ReadLEInt32(); //unk2
            int iNumberOfItem = reader.ReadLEInt32();
            reader.ReadLEInt32(); //unk3

            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            //onRecvStartGame: iGameStartTick=65, iNumberOfItem=200 (CurTick=2)
            room.StartTime = Utility.CurrentTimeMilliseconds();
            room.Survival = (byte)room.Players.Count(p => p.Attribute != 3);
            Log.Info("Room StartTime {0}", room.StartTime);
            foreach (Account RoomPlayer in room.Players)
            {
                //RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF140341000000C800000099AC0900", last));
                RoomPlayer.Connection.SendAsync(new GameRoom_START_GAME_RES(RoomPlayer, iGameStartTick, iNumberOfItem, last));
                RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FFA80500000000", last));
            }
        }

        public async static void Handle_GoalInData(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            if (room.GameMode == 38) //小遊戲
                return;
            //FF C0 02 FF 52 03
            User.LapTime = reader.ReadLEInt32();//11 31 02 00 goal time
            User.ServerLapTime = (int)(Utility.CurrentTimeMilliseconds() - room.StartTime);
            reader.ReadLEUInt32(); //54 6A 02 00 
            //User.RaceDistance = reader.ReadLESingle(); //6F C3 DE 47 RaceDistance float 114054.867188
            //00 00 00 00 flag?
            long EndTime = Utility.CurrentTimeMilliseconds() + 13000;

            User.Rank = room.Rank++;
            if (!room.isGoal)
            {
                MapHolder.MapInfos.TryGetValue(room.PlayingMapNum, out MapInfo mapinfo);
                if (mapinfo.GoalInLimitTime*1000 < User.ServerLapTime)
                {
                    Console.WriteLine("GoalIn");
                    foreach (Account RoomPlayer in room.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_GoalInData(User, User.LapTime, last));
                        RoomPlayer.Connection.SendAsync(new GameRoom_StartTimeOutCount(RoomPlayer, User.LapTime + 2000, last)); //FF 7C 03 E1 38 02 00 0A
                        room.isGoal = true;
                    }
                    //await Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                    Task calctask = Task.Run(() => GameRoomEvent.Calc_DropItem(User, room, User.Rank, last));
                    await Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                }
                else
                {
                    Console.WriteLine("GoalInError");
                    Client.SendAsync(new GameRoom_GoalInError(User, last));
                }
            }
            else
            {
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoalInData(User, User.LapTime, last));
                }
                //GameRoomEvent.Calc_DropItem(User, room, User.Rank, last);
                Task calctask = Task.Run(() => GameRoomEvent.Calc_DropItem(User, room, User.Rank, last));

            }
        }
        public static void Handle_MapControl(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF CA 02 14 00 44 00 00 00 00 91 EE 01 00 00 00 00 00 00 EE 01 00 00 00 00 01
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            short len = reader.ReadLEInt16(); //0C 00
            byte[] unk = reader.ReadByteArray(len);//39 8C F0 01 01 00 EB 00 05 00 04 00
            //Console.WriteLine("MapControl: {0}", Utility.ByteArrayToString(reader.Buffer));
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MapControl(User, len, unk, last));
            }
        }
        public static void Handle_TriggerMapEvent(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            //byte unk1 = reader.ReadByte();
            //int unk2 = reader.ReadInt32();
            //byte unk3 = reader.ReadByte();
            short eventnum = reader.ReadLEInt16();
            int eventlaptime = reader.ReadLEInt32();
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_TriggerMapEvent(User, eventlaptime, last));
            }
        }    
        public static void Handle_GiveUpItem(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            User.Connection.SendAsync(new GameRoom_GiveUpItem(User, last));
        }
        public static void Handle_DrawItem(ClientConnection Client,PacketReader reader,byte last)
        {
            //FF C0 02 FF 1A 03 51 34 00 00 00 00 00 00 01 00 FF FF FF FF FF FF FF FF 02
            Account User = Client.CurrentAccount;
            int unk1 = reader.ReadLEInt32(); //random?
            int unk2 = reader.ReadLEInt32(); //00 00 00 00 id?
            short rank = reader.ReadLEInt16(); //01 00
            int unk3 = reader.ReadLEInt32(); //固定道具?
            int unk4 = reader.ReadLEInt32();
            Console.WriteLine("unk1: {0}, unk2: {1}, unk3: {2}, unk4: {3}", unk1, unk2, unk3, unk4);
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            int getitem = unk3 == -1 ? RandItem(room): unk3;
            User.Connection.SendAsync(new GameRoom_DrawItem(User, unk1, unk2, getitem, last));
        }
        public static void Handle_UseItem(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF C0 02 FF 1D 03 D0 00 00 00 A0 F3 00 00 0B 00 00
             00 45 00 D0 00 00 00 0B 00 00 00 A0 F3 00 00 00 00
             C6 5E F3 C3 B6 2C 38 C5 9C 59 2A 44 00 00 00 00 00
             00 00 00 00 00 00 00 F3 04 B5 B9 00 00 00 00 00 00
             00 00 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00
             00 00 00 00 40*/
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            int unk1 = reader.ReadLEInt32(); //CF 00 00 00 current id
            int rand = reader.ReadLEInt32(); //random?
            int itemid = reader.ReadLEInt32();
            short bytesize = reader.ReadLEInt16();
            byte[] bytes = reader.ReadByteArray(bytesize);
            Console.WriteLine("unk1: {0}, rand: {1}", unk1, rand);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_UseItem(RoomPlayer, User.RoomPos, rand, itemid, bytes, last));
            }
        }
        public static void Handle_ChangeTeam(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            if (!User.IsReady)
            {
                NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
                byte team = reader.ReadByte();
                User.Team = team;
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosTeam(RoomPlayer, last));
                }
            }
        }
        public static void Handle_StepOnButton(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            byte[] unk = reader.ReadByteArray(12);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_StepOnButton(RoomPlayer, unk, last));
            }
        }
        public static void Handle_RegisterItem(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);

            //FF C0 02 FF 56 05 CB A3 00 00 7E 41 09 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 04
            int itemnum = reader.ReadLEInt32();
            long storage_id = reader.ReadLEInt64();
            int unk = reader.ReadLEInt32();//01 00 00 00
            int isOrderBy = reader.ReadLEInt32();
            int sendcount = reader.ReadLEInt32();
            bool isPublic = reader.ReadBoolean();

            if (room.ItemNum != -1)
            {
                User.Connection.SendAsync(new GameRoom_LockKeepItem(User, room, true, last));//解鎖之前保管了的物品
            }

            if (CheckRegisterItem(User.UserNum, storage_id))
            {
                room.RegisterItem(itemnum, storage_id, isOrderBy, sendcount, isPublic);

                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_RegisterSuccess(RoomPlayer, itemnum, storage_id, last));
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(RoomPlayer, room, last));
                }
                //FF 57 05 03 00 00 00 80 <- Register too many times

                if (itemnum != -1)
                {
                    User.Connection.SendAsync(new GameRoom_LockKeepItem(User, room, false, last));//鎖定保管物品
                }
            }
        }

        public static void Handle_MaxSpeedTube(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            byte[] unk = reader.ReadByteArray(0xC);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MaxSpeedTube(RoomPlayer, unk, last));
            }
        }

        private static int RandItem(NormalRoom room)
        {
            if (room.ItemType == 1)
            {
                if (room.IsTeamPlay == 0)
                {
                    return MapItemHolder.PersonalItem1.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                }
                else
                {
                    return MapItemHolder.TeamItem1.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                }
            }
            else
            {
                if (room.IsTeamPlay == 0)
                {
                    return MapItemHolder.PersonalItem2.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                }
                else
                {
                    return MapItemHolder.TeamItem2.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                }
            }
        }
        private static bool CheckRegisterItem(int usernum, long storageid)
        {
            if (storageid == -1)
                return true;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_checkroomreward";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = usernum;
                cmd.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = storageid;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                byte ret = Convert.ToByte(reader["retval"]);
                if (ret == 0)
                    return true;
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return false;
        }
    }
}
