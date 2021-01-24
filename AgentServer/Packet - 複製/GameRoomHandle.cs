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
using AgentServer.Structuring.Room;

namespace AgentServer.Packet
{
    public class GameRoomHandle
    {
        public static void Handle_CreateGameRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            if (!User.InGame)
            {
                if (User.Attribute == 3)
                {
                    Client.SendAsync(new GameRoom_CreateRoomError(User, 8, last));
                    return;
                }
                byte roomkindid = reader.ReadByte();
                byte unk = reader.ReadByte();
                int namelength = reader.ReadLEInt32();
                string name = reader.ReadBig5StringSafe(namelength);
                int passwordlength = reader.ReadLEInt32();
                string password = string.Empty;
                if (passwordlength > 0)
                    password = reader.ReadBig5StringSafe(passwordlength);
                int isTeamPlay = reader.ReadLEInt32();
                bool isStepOn = reader.ReadBoolean();
                int itemtype = reader.ReadLEInt32();
                int unk2 = reader.ReadLEInt32();
                int unk3 = reader.ReadLEInt32();
                //byte footer = binaryReader.ReadByte();

                NormalRoom room = new NormalRoom();
                room.setID(Rooms.RoomID);
                room.setName(name);
                room.setPassword(password);
                room.setItemType(itemtype);
                room.setIsStepOn(isStepOn);
                room.setRoomKindID(roomkindid);
                room.setIsTeamPlay(isTeamPlay);
                room.Players.Add(User);
                Console.WriteLine("fdRoomKindID={0}", roomkindid);
                if (RoomHolder.RoomKindInfos.TryGetValue(roomkindid, out int GameMode))
                    room.setGameMode(GameMode);
                else
                    room.setGameMode(1);

                /*if (!DBHelp.CheckTable(string.Format("select * from essenroomkindid where fdRoomKindID={0}", roomkindid)))
                {
                    Log.Error("DataBase沒有此RoomKind: {0}", roomkindid);
                    return;
                }

                if (DBHelp.CheckTable(string.Format("select * from essenroomkindid where fdRoomKindID={0} and fdGameMode=1", roomkindid)))
                {
                    room.setMaxPlayersCount(8);
                    room.setPosList(8);
                }
                else if (DBHelp.CheckTable(string.Format("select * from essenroomkindid where fdRoomKindID={0} and fdGameMode=2", roomkindid)))
                {
                    room.setMaxPlayersCount(30);
                    room.setPosList(30);
                    room.is8Player = false;
                }
                else if (DBHelp.CheckTable(string.Format("select * from essenroomkindid where fdRoomKindID={0} and fdGameMode=3", roomkindid)))
                {
                    room.setMaxPlayersCount(20);
                    room.setPosList(20);
                    room.is8Player = false;
                }*/


                //取得第一個位置 0
                User.RoomPos = (byte)(User.Attribute == 3 ? 100 : room.PosList.First());
                room.PosList.Remove(User.RoomPos);
                room.setRoomMasterIndex(User.RoomPos);


                //User.IsRoomMaster = true;
                User.InGame = true;
                User.CurrentRoomId = Rooms.RoomID;
                Rooms.RoomID += 1;

                Log.Info("Room name: {0}, isTeamPlay={1}, isStepOn={2}, ItemType={3}, HasPassword={4}, RoomKindID={5}", name, isTeamPlay, isStepOn, itemtype, passwordlength > 0 ? true : false, roomkindid);

                //Client.SendAsync(new GameRoom_Hex(User, "FF5805FFFFFFFFFFFFFFFF000000000000000D01", last));
                Client.SendAsync(new GameRoom_GoodsInfo(User, room, last));
                Client.SendAsync(new GameRoom_Hex(User, "A3", last));

                Client.SendAsync(new GameRoom_SendRoomInfo(User, room, last));
                Client.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                Client.SendAsync(new GameRoom_GetRoomMaster(User, room.RoomMasterIndex, last)); //場主位置

                //Client.SendAsync(new GameRoom_Hex(User, "FFB9010100000000", last));
                //Client.SendAsync(new GameRoom_SendRoomMaster(User, room.RoomMasterIndex, last));
                Client.SendAsync(new GameRoom_SendRoomMaster(User, room.MapNum, room.RoomMasterIndex, last));

                Client.SendAsync(new GameRoom_Hex(User, "FF6405F70300000000000000000000", last));
                Client.SendAsync(new GameRoom_Hex(User, "FF660501000000", last));

                if (isTeamPlay == 2)
                {
                    User.Team = 1;
                    Client.SendAsync(new GameRoom_RoomPosTeam(User, last));
                }
                /*Client.SendAsync(new GameRoom_Hex(User, "FF6405F70300000000000000000000", last));
                Client.SendAsync(new GameRoom_Hex(User, "FF660501000000", last));*/
                Rooms.NormalRoomList.Add(room);
            }
        }

        public static void Handle_LeaveRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            if (User.CurrentRoomId != 0 && User.InGame)
            {
                byte roompos = User.RoomPos;
                NormalRoom normalRoom = Rooms.NormalRoomList.Find(room => room.ID == User.CurrentRoomId);
                bool isRemoved = true;
                if (normalRoom.Players.Count == 1)
                {
                    if (normalRoom.ItemNum != -1)
                    {
                        User.Connection.SendAsync(new GameRoom_LockKeepItem(User, normalRoom, true, last));//解鎖之前保管了的物品
                    }
                    Rooms.NormalRoomList.Remove(normalRoom);
                }
                else
                {
                    //加回位置id
                    normalRoom.Players.Remove(User);
                    normalRoom.PosList.Add(roompos);
                    isRemoved = false;
                }

                byte RoomMasterIndex = normalRoom.RoomMasterIndex;
                User.InGame = false;
                User.CurrentRoomId = 0;
                User.EndLoading = false;
                User.RoomPos = 0;

                Client.SendAsync(new GameRoom_LeaveRoomUser_0XA9(User, roompos, last));
                Client.SendAsync(new GameRoom_Hex(User, "FFA80500000000", last));

                //Client.SendAsync(new GameRoom_Hex(User, "FF6405F703000002000000", last));
                if (!isRemoved)
                {
                    foreach (Account RoomPlayer in normalRoom.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RemoveRoomUser(RoomPlayer, roompos, last));
                    }

                    if (roompos == RoomMasterIndex)
                    {
                        if (normalRoom.ItemNum != -1)
                        {
                            User.Connection.SendAsync(new GameRoom_LockKeepItem(User, normalRoom, true, last));//解鎖之前保管了的物品
                            normalRoom.RegisterItem(-1, -1, 0, 0, false);

                            foreach (Account RoomPlayer in normalRoom.Players)
                            {
                                //RoomPlayer.Connection.SendAsync(new GameRoom_RegisterSuccess(User, -1, -1, last));
                                RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(User, normalRoom, last));
                            }
                        }
                        Account newRoomMaster = normalRoom.Players.FirstOrDefault();
                        normalRoom.RoomMasterIndex = newRoomMaster.RoomPos;
                        foreach (Account RoomPlayer in normalRoom.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_GetRoomMaster(RoomPlayer, newRoomMaster.RoomPos, last));
                        }
                    }
                    bool isAllSync = normalRoom.Players.All(player => player.EndLoading);
                    if (isAllSync)
                    {
                        foreach (Account RoomPlayer in normalRoom.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF1203", last));
                        }
                    }
                }
            }
        }

        public static void Handle_RoomControl(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            reader.Offset += 1; //FF
            short subopcode = reader.ReadLEInt16();
            switch (subopcode)
            {
                case 0x300: //開關欄位
                    RoomServerHandle.Handle_SlotControl(Client, reader, last);
                    break;
                case 0x2D8: //更改設定
                    RoomServerHandle.Handle_ChangeSetting(Client, reader, last);
                    break;
                case 0x2E6: //準備/取消準備
                    RoomServerHandle.Handle_Ready(Client, reader, last);
                    break;
                case 0x2E9: //轉地圖
                    RoomServerHandle.Handle_ChangeMap(Client, reader, last);
                    break;
                case 0x306: //Start game
                    RoomServerHandle.Handle_StartGame(Client, reader, last);
                    break;
                case 0x2C9:
                    RoomServerHandle.Handle_ChangeStatus(Client, reader, last);
                    break;
                case 0x30A:
                    RoomServerHandle.Handle_StartLoading(Client, reader, last);
                    break;
                case 0x310:
                    RoomServerHandle.Handle_EndLoading(Client, reader, last);
                    break;
                case 0x313:
                    RoomServerHandle.Handle_GameStart(Client, reader, last);
                    break;
                case 0x352:
                    RoomServerHandle.Handle_GoalInData(Client, reader, last);
                    break;
                case 0x37D:
                    Console.WriteLine("time over?alive?");
                    break;
                case 0x2DE:
                    RoomServerHandle.Handle_RoomChat(Client, reader, last);
                    break;
                case 0x2CA:
                    RoomServerHandle.Handle_MapControl(Client, reader, last);
                    break;
                case 0x315: //start lap countdown
                    GameModeHandle.GameMode_LapTimeCountdwon(Client, reader, last);
                    break;
                case 0x35C://天使神殿·外傳開門
                    //Log.Debug("Map event: {0}", Utility.ByteArrayToString(reader.Buffer));
                    RoomServerHandle.Handle_TriggerMapEvent(Client, reader, last);
                    break;
                case 0x2FE://四心踩制
                    RoomServerHandle.Handle_StepOnButton(Client, reader, last);
                    break;
                case 0x556://登陸商品
                    RoomServerHandle.Handle_RegisterItem(Client, reader, last);
                    break;
                case 0x559:
                    GameModeHandle.GameMode_MiniGame_Respawn(Client, reader, last);
                    break;
                case 0x55B:
                    GameModeHandle.GameMode_MiniGame_GetPoint(Client, reader, last);
                    break;
                case 0x55E:
                    GameModeHandle.GameMode_MiniGame_RoundTime(Client, reader, last);
                    break;
                case 0x33D:
                    GameModeHandle.GameMode_MiniGame_GameOver(Client, reader, last);
                    break;

                case 0x320://放棄之前抽到的道具（重抽道具用）
                    RoomServerHandle.Handle_GiveUpItem(Client, reader, last);
                    break;
                case 0x31A://抽道具
                    RoomServerHandle.Handle_DrawItem(Client, reader, last);
                    break;
                case 0x31D://使用道具
                    RoomServerHandle.Handle_UseItem(Client, reader, last);
                break;
                case 0x2EC://change team
                    RoomServerHandle.Handle_ChangeTeam(Client, reader, last);
                    break;
                case 0x36F:
                    RoomServerHandle.Handle_MaxSpeedTube(Client, reader, last);
                    break;
                default:
                    Log.Info("Unhandle room opcode: {0}", Utility.ByteArrayToString(reader.Buffer));
                    break;
            }
        }

        public async static void Handle_GameEndInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            User.RaceDistance = reader.ReadLESingle();
            float MapDistance = reader.ReadLESingle();
            User.GameEndType = reader.ReadLEInt16();
            long CurrentTime = Utility.CurrentTimeMilliseconds();

            Log.Debug("Game End - Nickname: {0}, GameEndType: {1}, RaceDistance: {2}", User.NickName, User.GameEndType, User.RaceDistance);

            if (User.GameEndType > 1)
            {
                NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
                if (room.GameMode == 38) //小遊戲
                    return;
                //GameEndType 2 = alive
                //GameEndType 3 = timeover
                //GameEndType 4 = gameover
                if (User.GameEndType == 2)
                {
                    User.LapTime = (int)(CurrentTime - room.StartTime);
                    User.ServerLapTime = User.LapTime;
                    User.Rank = room.Rank++;
                    Task calctask = Task.Run(() => GameRoomEvent.Calc_DropItem(User, room, User.Rank, last));
                    long EndTime = Utility.CurrentTimeMilliseconds() + 2000;
                    await Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                }
                else if (User.GameEndType == 3)
                {
                    User.LapTime = (int)(CurrentTime + 200000 - room.StartTime);
                    User.ServerLapTime = User.LapTime;
                }
                else if (User.GameEndType == 4)
                {
                    User.LapTime = (int)(CurrentTime + 240000 - room.StartTime);
                    User.ServerLapTime = User.LapTime;
                    room.Survival -= 1;
                    byte mypos = User.RoomPos;
                    foreach (Account RoomPlayer in room.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_GameOver(RoomPlayer, mypos, last));
                        if (room.Survival == 1 && room.RuleType == 2) //生存mode
                        {
                            byte alivepos = room.Players.Find(f => f.GameEndType == 0 && f.Attribute !=3).RoomPos;
                            RoomPlayer.Connection.SendAsync(new GameRoom_alive(RoomPlayer, alivepos, last));
                        }
                    }

                    if (room.Survival == 0 && room.RuleType == 3) //Hardcore
                    {
                        long EndTime = Utility.CurrentTimeMilliseconds() + 2000;
                        await Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                    }
                }
            }
        }

        public static void Handle_FF6C01(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            //NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            Client.SendAsync(new GameRoom_Hex(User, "FFC5020200000000000000000100000000", last));
            Client.SendAsync(new GameRoom_Hex(User, "FF270500000000", last));

        }

        public static void Handle_GetRoomList(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            byte roomkindid = reader.ReadByte();
            int unk1 = reader.ReadLEInt32();
            int page = reader.ReadLEInt32();
            byte unk2 = reader.ReadByte();

            List<NormalRoom> rooms = Rooms.NormalRoomList.FindAll(room => room.RoomKindID == roomkindid);

            Client.SendAsync(new GameRoom_GetRoomList(User, rooms, roomkindid, page, last));


        }

        public static void Handle_EnterRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int roomid = reader.ReadLEInt32();
            int pwlen = reader.ReadLEInt32();
            string pw = string.Empty;
            if (pwlen != 0)
                pw = reader.ReadBig5StringSafe(pwlen);
            if (User.CurrentRoomId != 0)
            {
                GameRoomEvent.DisconnectRoom(User);
            }
            bool isExist = Rooms.NormalRoomList.Exists(rm => rm.ID == roomid);
            if (isExist)
            {
                NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == roomid);
                GameRoomEvent.EnterRoom(Client, room, pw, last);
            }
            else
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 0x1, 0x9B, last));
            }
        }

        public static void Handle_KickPlayer(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            bool isExist = room.Players.Exists(p => p.NickName == nickname);
            if (isExist)
            {
                Account KickedPlayer = room.Players.Find(p => p.NickName == nickname);
                byte KickedPlayerIndex = KickedPlayer.RoomPos;
                if (room.RoomMasterIndex == User.RoomPos && room.RoomKindID != 0x4A && KickedPlayer.Attribute == 0)//room.RoomKindID != 0x4A <-----防公園踢人外掛, KickedPlayer.Attribute == 0 <-----只能踢普通玩家
                {
                    GameRoomEvent.KickPlayer(KickedPlayer, room, last);
                }
                if (User.Attribute != 0)
                {
                    GameRoomEvent.KickPlayer(KickedPlayer, room, last);
                }
            }
        }

        public static void Handle_FF9704(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GameRoom_Hex(User, "FF98044E00", last));
        }

        public static void Handle_RandomEnterRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            byte roomkindid = reader.ReadByte();
            //int empty = reader.ReadLEInt32();

            if (roomkindid == 0x4A)
            {
                bool isExist = Rooms.NormalRoomList.Any(rm => rm.RoomKindID == 0x4A && rm.Players.Count < rm.MaxPlayersCount);
                if (!isExist)
                {
                    NormalRoom room = new NormalRoom();
                    room.setID(Rooms.RoomID);
                    room.setName("Park");
                    room.setPassword("");
                    room.setItemType(1);
                    room.setIsStepOn(true);
                    room.setRoomKindID(0x4A);
                    room.setIsTeamPlay(0);

                    room.Players.Add(User);
                    room.setMaxPlayersCount(30);
                    room.setPosList(30);

                    User.RoomPos = room.PosList.First();
                    room.PosList.Remove(User.RoomPos);
                    room.setRoomMasterIndex(User.RoomPos);

                    User.CurrentRoomId = Rooms.RoomID;
                    User.InGame = true;

                    Client.SendAsync(new GameRoom_SendRoomInfo(User, room, last));
                    Client.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                    Client.SendAsync(new GameRoom_GetRoomMaster(User, room.RoomMasterIndex, last)); //場主位置

                    Client.SendAsync(new GameRoom_SendRoomMaster(User, room.MapNum, room.RoomMasterIndex, last));

                    Rooms.NormalRoomList.Add(room);
                    Rooms.RoomID += 1;

                }
                else
                {
                    NormalRoom room = Rooms.NormalRoomList.FirstOrDefault(rm => rm.RoomKindID == 0x4A && rm.Players.Count < rm.MaxPlayersCount);
                    User.CurrentRoomId = room.ID;
                    User.InGame = true;

                    //取得當前第一個位置id
                    User.RoomPos = room.PosList.FirstOrDefault();
                    room.PosList.Remove(User.RoomPos);

                    Client.SendAsync(new GameRoom_Hex(User, "FF5805FFFFFFFF00000000000000000000000000", last));
                    Client.SendAsync(new GameRoom_SendRoomInfo(User, room, last, User.RoomPos));

                    //Send自己info俾其他roomuser
                    foreach (Account roomUser in room.Players)
                    {
                        roomUser.Connection.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                    }

                    room.Players.Add(User);

                    //Send roomuser info俾自己
                    foreach (Account roomUser in room.Players)
                    {
                        Client.SendAsync(new GameRoom_SendPlayerInfo(roomUser, last));
                    }

                    byte roommasterpos = room.RoomMasterIndex; //room.Players.Find(player => player.IsRoomMaster).RoomPos;
                    Client.SendAsync(new GameRoom_GetRoomMaster(User, roommasterpos, last));
                }
            }
            else
            {
                bool isExist = Rooms.NormalRoomList.Any(rm => rm.RoomKindID == roomkindid && rm.Players.Count < rm.MaxPlayersCount && !rm.isPlaying && !rm.HasPassword && !rm.Players.Exists(p => p.Attribute == 3));
                if (!isExist)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 0xB, roomkindid, last));
                }
                else
                {
                    int count = Rooms.NormalRoomList.Count(rm => rm.RoomKindID == roomkindid && rm.Players.Count < rm.MaxPlayersCount && !rm.isPlaying && !rm.HasPassword && !rm.Players.Exists(p => p.Attribute == 3));
                    NormalRoom room = Rooms.NormalRoomList.FindAll(rm => rm.RoomKindID == roomkindid && rm.Players.Count < rm.MaxPlayersCount && !rm.isPlaying && !rm.HasPassword && !rm.Players.Exists(p => p.Attribute == 3))[new Random().Next(count)];
                    GameRoomEvent.EnterRoom(Client, room, string.Empty, last);
                }
            }
        }

        public static void Handle_PlayTogether(ClientConnection Client, PacketReader packet, byte last)
        {
            Account User = Client.CurrentAccount;
            int unk = packet.ReadLEInt32();
            int roomid = packet.ReadLEInt32();
            byte roomkindid = packet.ReadByte();
            int pwlen = packet.ReadLEInt32();
            string pw = string.Empty;
            if (pwlen > 0)
                pw = packet.ReadBig5StringSafe(pwlen);
            bool isExist = Rooms.NormalRoomList.Exists(rm => rm.ID == roomid);
            if (!isExist)
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 0x1, roomkindid, last));
            }
            else
            {
                NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == roomid);
                GameRoomEvent.EnterRoom(Client, room, pw, last);
            }
        }
    }
}
