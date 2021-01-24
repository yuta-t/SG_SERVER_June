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
using System.Windows.Forms;

namespace AgentServer.Packet
{
    public class RoomServerHandle
    {

        public static void Handle_SlotControl(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            if (isRoomMaster && room.is8Player)
            {
                byte roompos = reader.ReadByte();
                bool isOff = reader.ReadBoolean();
                room.setPosWeight(roompos, isOff);
                room.BroadcastToAll(new GameRoom_ControlRoomPos(roompos, isOff, last));
            }
        }
        public static void Handle_ChangeSetting(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);

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

                room.BroadcastToAll(new GameRoom_ChangeSetting(room, last));
            }
        }
        public static void Handle_Ready(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            bool isReady = reader.ReadBoolean();
            if (room.isPlaying && !isReady)
                return;
            User.IsReady = isReady;
            byte readyroompos = User.RoomPos;
            room.BroadcastToAll(new GameRoom_RoomPosReady(readyroompos, isReady, last));
        }
        public static void Handle_ChangeMap(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
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
                room.RuleType = mapinfo.RuleType;
                foreach (var RoomPlayer in room.PlayerList())
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex("FF6405F70300000000000000000000", last));
                    RoomPlayer.Connection.SendAsync(new GameRoom_ChangeMap_FF6605(RoomPlayer, mapid, last));
                    RoomPlayer.Connection.SendAsync(new GameRoom_ChangeMap_FFEA02(RoomPlayer, mapid, last));
                }
            }
        }
        public static void Handle_RoomChat(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF DE 02 FF 13 00 00 2F 61 64 76 65 72 74 6F 70 65 6E 20 3A 31 32 33 35 00 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            reader.Offset += 1; //FF
            short len = reader.ReadLEInt16();
            byte[] text = reader.ReadByteArray(len);
            room.BroadcastToAll(new GameRoom_RoomChat(User.RoomPos, text, last));
            if (room.RoomMasterIndex == User.RoomPos && !room.isPlaying)
                room.ResetAutoChangeRoomMaster();
        }
        public static void Handle_ChangeStatus(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF C9 02 0A 00 00 00 02
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int code = reader.ReadLEInt32();
            if (User.InGame)
            {
                room.BroadcastToAll(new GameRoom_ChangeStatus(User.RoomPos, code, last));
            }
        }

        public static void Handle_StartGame(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            bool isRoomMaster = User.RoomPos == room.RoomMasterIndex;

            if (!room.CheckReadyPlayerNum(out int err))
            {
                if (err > 0)
                    Client.SendAsync(new GameRoom_StartError(err, last));
                return;
            }

            bool CanStart = room.CheckCanStart(out int ErrorCode);
            if (isRoomMaster && CanStart)
            {
                Log.Info("Start game CountDown");
                room.MapNum = reader.ReadLEInt32(); //mapid
                room.isPlaying = true;
                room.BroadcastToAll(new GameRoom_Hex("FF0703", last)); //307
                if (!room.is8Player)
                {
                    room.StartTimeoutCountDownThread();
                }
            }
            else if (isRoomMaster && !CanStart)
            {
                if (ErrorCode == 3)
                    Client.SendAsync(new GameRoom_CannotStart((byte)ErrorCode, last));
                if (ErrorCode > 0 && ErrorCode != 1)
                    Client.SendAsync(new GameRoom_StartError(ErrorCode, last));
                if (ErrorCode == 1)
                    Client.SendAsync(new GameRoom_CannotStart((byte)ErrorCode, last));
            }
        }
        public static void Handle_StartLoading(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int mapid = reader.ReadLEInt32(); //mapid
            int randseed = reader.ReadLEInt32(); //00 92 D1 00  randseed
            reader.ReadByte(); //00

            //Log.Info("startLoading(randseed = {0})", randseed);
            if (MapHolder.MapInfos.TryGetValue(mapid, out MapInfo mapinfo))
            {
                room.RuleType = mapinfo.RuleType;
                if (!room.is8Player && !room.CheckReadyPlayerNum(out int err))
                {
                    if (err > 0)
                        Client.SendAsync(new GameRoom_StartError(err, last));
                    room.isPlaying = false;
                    return;
                }

                if (room.GameMode == 2)
                {
                    List<Account> notReadyPlayers = room.Players.FindAll(p => !p.IsReady && p.RoomPos != room.RoomMasterIndex);
                    foreach (Account notReadyPlayer in notReadyPlayers)
                    {
                        GameRoomEvent.KickPlayer(notReadyPlayer, room, last);
                    }
                }
                else if (room.GameMode == 3)
                {
                    List<Account> notReadyPlayers = room.Players.FindAll(p => !p.IsReady);
                    foreach (Account notReadyPlayer in notReadyPlayers)
                    {
                        GameRoomEvent.KickPlayer(notReadyPlayer, room, last);
                    }
                }
                room.PlayingMapNum = mapid;
                room.BroadcastToAll(new GameRoom_StartLoading(mapid, randseed, last));
                room.StartWaitAllSyncThread();
                room.SetRewardGroupList();
            }
        }
        public static void Handle_EndLoading(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            User.EndLoading = true;
            Log.Debug("{0} EndLoading", User.NickName);
            //send 自己end loading
            room.BroadcastToAll(new GameRoom_EndLoading(User.RoomPos, last));
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

            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //onRecvStartGame: iGameStartTick=65, iNumberOfItem=200 (CurTick=2)
            //room.StartTime = Utility.CurrentTimeMilliseconds();
            room.Survival = (byte)room.PlayerCount();
            room.CapsuleNum = iNumberOfItem;
            room.StartGame();
            room.Players.ForEach((RoomPlayer) => {
                RoomPlayer.Connection.SendAsync(new GameRoom_START_GAME_RES(RoomPlayer, iGameStartTick, iNumberOfItem, last));
                RoomPlayer.Connection.SendAsync(new GameRoom_Hex("FFA80500000000", last));
                RoomPlayer.GameEndType = 0;
                RoomPlayer.GameOver = false;
            });
            if (room.PlayingMapNum == 1606)
            {
                for (byte i = 0; i <= 7; i++)
                {
                    Account Hare = room.Players.Find(p => p.RoomPos == i);
                    if (Hare.Partner > 7)
                    {
                        Account Tortoise = room.Players.Where(p => p.Team == Hare.Team && p.Partner > 7 && p.RoomPos != i).OrderBy(_ => Guid.NewGuid()).Take(1).ToArray()[0];
                        Hare.Animal = 0;
                        Hare.Partner = Tortoise.RoomPos;
                        Hare.TeamLeader = true;
                        Tortoise.Animal = 1;
                        Tortoise.Partner = Hare.RoomPos;
                        Tortoise.TeamLeader = false;
                    }
                }
                room.BroadcastToAll(new AllocatePartner(room, last));
                room.StartHareAndTortoiseThread();
            }

            if (room.RuleType == 8) 
            {
                int startlaptime = 100 * 1000;
                room.Players.ForEach((RoomPlayer) => {
                    RoomPlayer.CurrentLapTime = startlaptime;
                    RoomPlayer.LastLapTime = 0;
                    RoomPlayer.Connection.SendAsync(new Amsan_LapTimeControl(0, startlaptime, startlaptime, false, last));
                });
            }
        }

        public static void Handle_GoalInData(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            if (room.GameMode == 38) //小遊戲
                return;
            int laptime = reader.ReadLEInt32();
            //User.LapTime = laptime;
            //FF C0 02 FF 52 03 11 31 02 00
            int servertime = room.GetCurrentTime();
            //User.ServerLapTime = servertime;
            //reader.ReadLEUInt32(); //54 6A 02 00 
            //User.RaceDistance = reader.ReadLESingle(); //6F C3 DE 47 RaceDistance float 114054.867188
            //00 00 00 00 flag?
            //User.Rank = room.Rank++;
            room.GoalIn(User, laptime, servertime, last);
            /*
            if (!room.isGoal)
            {
                MapHolder.MapInfos.TryGetValue(room.PlayingMapNum, out MapInfo mapinfo);
                if (room.GameMode == 5)
                {
                    if (room.CorunBossHP > 0)
                        return; //hack
                    if (room.Players.Any(a => a.GameEndType == 1))
                        return;
                }
                if (mapinfo.GoalInLimitTime * 1000 < servertime)
                {
                    Console.WriteLine("GoalInOK");
                    room.isGoal = true;
                    room.Players.ForEach((RoomPlayer) => {
                        RoomPlayer.Connection.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 0, last));
                        RoomPlayer.Connection.SendAsync(new GameRoom_StartTimeOutCount(User.LapTime + 2000, last)); 
                        //FF 7C 03 E1 38 02 00 0A
                    });
                    if (room.GameMode == 5)
                    {
                        int i = 0;
                        foreach (var p in room.Players.OrderBy(o => o.RoomPos))
                        {
                            //p.GameEndType = 1;
                            p.LapTime = laptime + i;
                            p.ServerLapTime = servertime + i;
                            i++;
                            //Console.WriteLine("p {0}, {1}, {2}, {3}", p.NickName, p.ServerLapTime, p.LapTime, p.RoomPos);
                        }
                    }
                    long EndTime = Utility.CurrentTimeMilliseconds() + 15000;
                    Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                }
                else
                {
                    Console.WriteLine("GoalInError");
                    Client.SendAsync(new DisconnectPacket(User, 258, last));
                }
            }
            else
            {
                room.BroadcastToAll(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 0, last));
            }*/
        }
        public static void Handle_MapControl(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF CA 02 14 00 44 00 00 00 00 91 EE 01 00 00 00 00 00 00 EE 01 00 00 00 00 01
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            short len = reader.ReadLEInt16(); //0C 00
            byte[] unk = reader.ReadByteArray(len);//39 8C F0 01 01 00 EB 00 05 00 04 00
            //Console.WriteLine("MapControl: {0}", Utility.ByteArrayToString(reader.Buffer));
            room.BroadcastToAll(new GameRoom_MapControl(len, unk, last));
        }
        public static void Handle_TriggerMapEvent(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 5C 03 00 01 D0 78 02 00 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            reader.Offset += 1; //00
            byte eventnum = reader.ReadByte();
            int eventlaptime = reader.ReadLEInt32();
            room.BroadcastToAll(new GameRoom_TriggerMapEvent(eventnum, eventlaptime, last));
        }    
        public static void Handle_GiveUpItem(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GameRoom_GiveUpItem(User, last));
        }
        public static void Handle_DrawItem(ClientConnection Client,PacketReader reader,byte last)
        {
            //FF C0 02 FF 1A 03 51 34 00 00 00 00 00 00 01 00 FF FF FF FF FF FF FF FF 02
            //FF C0 02 FF 1A 03 85 1F 00 00 C9 00 00 00 01 00 FF FF FF FF FF FF FF FF 02
            //FF C0 02 FF 1A 03 13 A5 00 00 CE 00 00 00 01 01 1F 00 00 00 FF FF FF FF 10
            Account User = Client.CurrentAccount;
            int time = reader.ReadLEInt32(); //random?
            int CapsuleID = reader.ReadLEInt32(); //00 00 00 00 Capsule id?
            byte rank = reader.ReadByte(); //01
            byte flag = reader.ReadByte();
            int fixitem = reader.ReadLEInt32(); //固定道具?
            int unk4 = reader.ReadLEInt32();
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //int getitem = fixitem == -1 ? RandItem(room, rank) : fixitem;
            if (room.RegCapsule.TryGetValue(CapsuleID, out var regitemid))
            {
                Console.WriteLine("DrawRegItem - CapsuleID: {0}, itemid: {1}, unk4: {2}", CapsuleID, regitemid, unk4);
                room.BroadcastToAll(new GameRoom_DrawItem(User.RoomPos, time, CapsuleID, regitemid, 0, flag, last));
                room.RegCapsule.Remove(CapsuleID);
            }
            else
            {
                Console.WriteLine("DrawItem - CapsuleID: {0}, Fixitem: {1}, unk4: {2}", CapsuleID, fixitem, unk4);
                int getitem = fixitem == -1 ? RandItem(room, rank) : fixitem;
                room.BroadcastToAll(new GameRoom_DrawItem(User.RoomPos, time, CapsuleID, getitem, room.CapsuleNum, flag, last));
                room.CapsuleNum++;
            }
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
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int CapsuleID = reader.ReadLEInt32(); //CF 00 00 00 current id
            int time = reader.ReadLEInt32(); //random?
            int itemid = reader.ReadLEInt32();
            short bytesize = reader.ReadLEInt16();
            byte[] bytes = reader.ReadByteArray(bytesize);
            Console.WriteLine("UseItem - CapsuleID: {0}, Itemid: {1}", CapsuleID, itemid);
            room.BroadcastToAll(new GameRoom_UseItem(User.RoomPos, time, itemid, bytes, last));
        }
        public static void Handle_RegItem(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF C0 02 FF 27 03 AE 05 00 00 39 00 00 00 25 00 09 00 00 00 
             78 AC 05 00 00 F3 03 00 00 00 00 00 00 1B 3F 28 C4 29 E2 64
             C5 98 D9 18 44 00 00 00 00 00 E0 B4 B9 20*/
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int time = reader.ReadLEInt32(); //random?
            int itemid = reader.ReadLEInt32();
            short bytesize = reader.ReadLEInt16();
            byte[] bytes = reader.ReadByteArray(bytesize);
            Console.WriteLine("RegItem - CapsuleID: {0}, Itemid: {1}", room.CapsuleNum, itemid);
            room.RegCapsule.Add(room.CapsuleNum, itemid);
            room.BroadcastToAll(new GameRoom_RegItem(time, itemid, room.CapsuleNum, bytes, last));
            room.CapsuleNum++;
        }
        public static void Handle_RegItem2(ClientConnection Client, PacketReader reader, byte last)
        {
            /*FF C0 02 FF 26 03 0E 0A 00 00 1B 00 00 00 12 00 00 00 
             1D 00 07 00 00 00 00 00 00 00 00 12 00 00 00 1B 00 00
             00 68 BA 05 43 4E 8E 17 47 B3 A1 1C 46 08*/
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int time = reader.ReadLEInt32();
            int itemid = reader.ReadLEInt32();
            int unk = reader.ReadLEInt32();
            short arraylen = reader.ReadLEInt16();
            byte[] array = reader.ReadByteArray(arraylen);
            room.RegCapsule.Add(room.CapsuleNum, itemid);
            room.BroadcastToAll(new GameRoom_RegItem(time, itemid, room.CapsuleNum, array, last));
            room.CapsuleNum++;
            //Console.WriteLine("RegItem2 - CapsuleID: {0}, Itemid: {1}", room.CapsuleNum, itemid);
        }
        public static void Handle_ChangeTeam(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            byte team = reader.ReadByte();
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            bool isReady = User.RoomPos == room.RoomMasterIndex ? false : User.IsReady;
            if (!isReady && User.Team != team)
            {
                User.Team = team;
                room.BroadcastToAll(new GameRoom_RoomPosTeam(User, last));
            }
        }
        public static void Handle_StepOnButton(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte[] unk = reader.ReadByteArray(12);
            room.BroadcastToAll(new GameRoom_StepOnButton(unk, last));
        }
        public static void Handle_RegisterItem(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);

            //FF C0 02 FF 56 05 CB A3 00 00 7E 41 09 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 04
            int itemnum = reader.ReadLEInt32();
            long storage_id = reader.ReadLEInt64();
            int unk = reader.ReadLEInt32();//01 00 00 00
            int isOrderBy = reader.ReadLEInt32();
            int sendcount = reader.ReadLEInt32();
            bool isPublic = reader.ReadBoolean();

            if (room.ItemNum != -1)
            {
                Client.SendAsync(new GameRoom_LockKeepItem(room, true, last));//解鎖之前保管了的物品
            }

            if (CheckRegisterItem(User.UserNum, storage_id))
            {
                room.RegisterItem(itemnum, storage_id, isOrderBy, sendcount, isPublic);

                room.Players.ForEach((RoomPlayer) => {
                    RoomPlayer.Connection.SendAsync(new GameRoom_RegisterSuccess(itemnum, storage_id, last));
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(room, last));
                });
                //FF 57 05 03 00 00 00 80 <- Register too many times

                if (itemnum != -1)
                {
                    Client.SendAsync(new GameRoom_LockKeepItem(room, false, last));//鎖定保管物品
                }
            }
        }

        public static void Handle_jw_Loading(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.BroadcastToAll(new GameRoom_jw_Loading(last));
        }

        public static void Handle_ChangeRelayTeam(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF EF 02 03 10
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            if (room.isPlaying || room.GameMode != 3)
                return;
            byte relayteam = reader.ReadByte();
            if(room.Players.Exists(e => e.RelayTeamPos == relayteam && relayteam > 2))
                return;
            User.SelectRelayTeam(relayteam);
            room.BroadcastToAll(new GameRoom_RoomPosRelayTeam(User, last));
        }
        public static void Handle_RandomChooseRelayTeam(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            if (room.isPlaying)
                return;
            var id = room.RelayPosList.Where(w => !room.Players.Any(s => s.RelayTeamPos == w))
                            .OrderBy(_ => Guid.NewGuid());
            if (id.Count() == 0)
                return;
            User.SelectRelayTeam(id.FirstOrDefault());
            room.BroadcastToAll(new GameRoom_RoomPosRelayTeam(User, last));
        }
        public static void Handle_ChangeSlotStateRelay(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF F3 02 02 01 20
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte slotid = reader.ReadByte();
            bool isOFF = reader.ReadBoolean();
            room.setRelayPosWeight(slotid, isOFF);
            room.BroadcastToAll(new GameRoom_RelayChangeSlotState(slotid, isOFF, last));
        }
        /*public static void Handle_PreparePassBaton(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF CE 02 FF 04 00 05 00 00 00 80
            //FF C0 02 FF CE 02 01 08 00 03 00 00 00 02 00 00 00 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte unk1 = reader.ReadByte();
            short len = reader.ReadLEInt16();
            //nt unk3 = reader.ReadLEInt32();
            byte[] bytes = reader.ReadByteArray(len);
            //Console.WriteLine("0x2CE {0}", Utility.ByteArrayToString(reader.Buffer));
            room.BroadcastToAll(new GameRoom_PreparePassBaton(User.RoomPos, len, bytes, last));
        }*/
        public static void Handle_WaitPassBaton(ClientConnection Client, byte last)
        {
            //FF C0 02 FF F6 02 01
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.BroadcastToAll(new GameRoom_WaitPassBaton(User, last));
        }
        public static void Handle_WaitPassBaton2(ClientConnection Client, byte last)
        {
            //FF C0 02 FF F8 02 01
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.BroadcastToAll(new GameRoom_WaitPassBaton2(User, last));
        }
        public static void Handle_PassBaton(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte teampos = reader.ReadByte();
            int unk = reader.ReadLEInt32();
            room.BroadcastToAll(new GameRoom_PassBaton(User, teampos, unk, last));
        }
        public static void Handle_StartPassBaton(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF FA 02 0A 22 01 00 01
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int unk = reader.ReadLEInt32();
            room.BroadcastToAll(new GameRoom_StartPassBaton(User, last));
        }

        private static int RandItem(NormalRoom room, int rank)
        {
            int resultitem = 13;
            int rankgroup = 0;
            int survival = room.Survival;
            if (room.ItemType == 2)
                resultitem = 1001;
            if (rank > survival)
                goto label1;
            if (rank == 1)
            {
                rankgroup = 0;
            }
            else if (survival > 4)
            {
                if (rank == survival)
                {
                    rankgroup = 11;
                }
                else
                {
                    double v1 = 10.0 / (survival - 2);
                    rankgroup = (int)((rank - 2) * v1) + 1;
                }
            }
            else
            {
                if (survival < 2)
                    survival = 2;
                double v1 = 10.0 / (survival - 1);
                rankgroup = (int)((rank - 2) * v1) + 1;
            }
            label1:
            if (rankgroup >= 0 && rankgroup < 12)
            {
                if (room.ItemType == 1)
                {
                    if (room.IsTeamPlay != 0)
                        rankgroup += 100;
                }
                else if (room.ItemType == 2)
                {
                    if (room.IsTeamPlay == 0)
                        rankgroup += 200;
                    else
                        rankgroup += 300;
                }

                MapItemHolder.MapCapsuleItems.TryGetValue(rankgroup, out var randitem);
                resultitem = randitem.NextWithReplacement().GameItemNum;
            }
            else
            {
                Log.Error("group_num < 0 || group_num >= eRankCategory_COUNT, group_num : {0}", rankgroup);
            }
            return resultitem;
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
