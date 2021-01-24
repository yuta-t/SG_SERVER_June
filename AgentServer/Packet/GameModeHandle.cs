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
    public class GameModeHandle
    {

        public static void GameMode_LapTimeCountdwon(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 15 03 06 00 00 01 01
            //FF C0 02 FF 15 03 06 00 01 01 20
            //FF C0 02 FF 15 03 06 00 02 01 20
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine("LapTimeCountdwon: {0}", Utility.ByteArrayToString(reader.Buffer));
            short second = reader.ReadLEInt16();
            byte round = reader.ReadByte(); //round?
            room.BroadcastToAll(new GameRoom_LapTimeCountdwon(second, round, last));
            if (room.GameMode == 38) //小遊戲
            {
                Task.Run(() => Task.Delay(second * 1000))
                    .ContinueWith((t) =>
                    {
                        room.Players.ForEach((RoomPlayer) => {
                            RoomPlayer.Connection.SendAsync(new GameRoom_LapTimeCountdwon2(second, round, last));
                            RoomPlayer.GameEndType = 0;
                        });
                        room.Round = round;
                        if (room.Round == 0) //init pointlist
                        {
                            foreach (Account Player in room.Players)
                            {
                                DropList dropList = new DropList
                                {
                                    UserNum = Player.UserNum,
                                    RaceDistance = Player.RaceDistance,
                                    ServerLapTime = Player.ServerLapTime,
                                    LapTime = Player.LapTime,
                                    Pos = Player.RoomPos,
                                    Team = Player.Team,
                                    BounsTR = 0,
                                    BounsEXP = 0,
                                    TR = 0,
                                    EXP = 0,
                                    Rank = 0,
                                    CardID = new List<int> { 0 },
                                    MiniGamePoint = 0,
                                    MiniGameStarPoint = 0
                                };
                                room.DropItem.Add(dropList);
                            }
                        }
                    });
            }

        }

        public static void GameMode_MiniGame_RoundTime(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 5E 05 00 00 00 00 00 00 00 00 00 00 A0 41 04
            //FF C0 02 FF 5E 05 02 00 00 00 00 00 00 00 00 00 20 42 01
            Account User = Client.CurrentAccount;
            int round = reader.ReadLEInt32(); //current round
            int nextround = reader.ReadLEInt32(); //nextround?
            float RoundTime = reader.ReadLESingle();
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_RoundTime(RoomPlayer, round, nextround, RoundTime, last));
                RoomPlayer.GameOver = false;
            }
            room.Survival = (byte)room.PlayerCount();
            long RoundEndTime = Utility.CurrentTimeMilliseconds() + (long)(RoundTime * 1000);
            Task.Run(() => GameMode_MiniGame_RoundThread(RoundEndTime, room));
            //Console.WriteLine("room.Round: {0}", room.Round);
            Client.SendAsync(new GameRoom_MiniGame_55F(User, round, last));
            room.RespwanList.Clear();
        }
        public static void GameMode_MiniGame_RoundThread(long RoundTime, NormalRoom room)
        {
            while(Utility.CurrentTimeMilliseconds() < RoundTime)
            {
                if (room.Survival == 0) //next round
                    break;
                //wait all gameover or time
            }

            int point = 100 + 50 * room.Round;
            if (room.PlayingMapNum != 40006)
            {
                room.Players.Where(w => w.Attribute != 3 && !w.GameOver)
                    .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { p, d })
                        .ToList().ForEach(f => f.d.MiniGamePoint += point);
            }

            int isnextround = 1;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_RoundTime(RoomPlayer, room.Round, isnextround, 1, 0x1));
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_UpdatePoint(room, 0x1));
            }

            int nextround = room.Round + 1;
            int maxround = room.PlayingMapNum == 40003 ? 1 : 3;
            if (nextround == maxround) //endgame
            {
                int laptime = 100;

                foreach (var p in room.Players.Where(p => p.Attribute != 3)
                           .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new {p, d })
                                .OrderByDescending(o => o.d.MiniGamePoint).ThenBy(o => o.p.RoomPos))
                {
                    if (p.d.MiniGamePoint > 0)
                    {
                        p.d.Rank = room.Rank++;
                        int mylabtime = laptime++;
                        p.p.ServerLapTime = mylabtime;
                        p.p.LapTime = mylabtime;
                        p.d.MiniGameStarPoint = (int)Math.Round(p.d.MiniGamePoint / 4.5, MidpointRounding.AwayFromZero);
                        UpdateUserPoint(p.p.UserNum, 2400, p.d.MiniGameStarPoint);
                    }
                    else
                    {
                        p.d.Rank = 98;
                        p.p.ServerLapTime = 100000;
                        p.p.LapTime = 100000;
                    }
                }

                room.Result = GenResult_ForMiniGameMode(room, 0x1);
                room.RegisterItem(-1, -1, 2, 0x1049F00C, true);

                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameUpdateEXP(RoomPlayer, 0x1));
                }
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameResult2(RoomPlayer, room.Result));
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex("FF0102", 0x1));
                    foreach (Account Player in room.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosReady(Player.RoomPos, false, 0x1));
                    }
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(room, 0x1));
                }
                foreach (Account RoomPlayer in room.Players)
                {
                     //FF2D02A3220000C4C11400CB110000DB0800000E0C00001E0F0000
                    RoomPlayer.Connection.SendAsync(new GameRoom_UpdateIndividualGameRecord(RoomPlayer, 0x1));
                }
                Thread.Sleep(5000);
                foreach (Account RoomPlayer in room.Players)
                {
                    //MoveToGameRoom
                    RoomPlayer.Connection.SendAsync(new MoveToGameRoom(0x1)); //9704
                }

                room.GameEndSetNewRoomMaster();
                room.addMatchTime();
                room.StartAutoChangeRoomMaster();
                //reset
                room.GameEndReset();
            }

        }

        public static void GameMode_MiniGame_GetPoint(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 5B 05 00 14 00 00 00 80
            Account User = Client.CurrentAccount;
            byte pos = reader.ReadByte();
            int point = reader.ReadLEInt32();
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int nowpoint = room.DropItem.Find(f => f.UserNum == User.UserNum).MiniGamePoint += point;
            room.BroadcastToAll(new GameRoom_MiniGame_UpdatePoint(room, last));
            Client.SendAsync(new GameRoom_MiniGame_GetPoint(User, nowpoint, point, last));
        }
        public static void GameMode_GameOver(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 3D 03 15 91 E5 39 10 00 A0 42 00 00 00 00 00 40
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.GameOver(User, last);
            /*if (User.GameEndType == 0)
            {
                Log.Info("{0} GameOver", User.NickName);
                room.Survival -= 1;
                User.GameEndType = 4;
                User.GameOver = true;
                room.BroadcastToAll(new GameRoom_GameOver(User.RoomPos, last));
                switch (room.RuleType)
                {
                    case 2: //生存
                        if (room.Survival == 1)
                        {
                            byte alivepos = room.Players.Find(f => f.GameEndType == 0 && f.Attribute != 3).RoomPos;
                            room.BroadcastToAll(new GameRoom_Alive(alivepos, last));
                        }
                        break;
                    case 3: //Hardcore
                    case 4: //跑步吧
                    case 8: //衝呀!!
                    case 64://八心
                        if (room.Survival == 0)
                        {
                            long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                            Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                        }
                        break;
                    case 20480:
                        //MiniGame
                        break;
                    default:
                        break;
                }
            }*/
        }
        public static void GameMode_MiniGame_Respawn(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 59 05 00 00 00 00 40
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.RespwanList.Add(User.RoomPos);
            if (room.Players.Count(p => p.Attribute != 3 && p.GameOver) == room.RespwanList.Count)
            {
                foreach (Account RoomPlayer in room.Players)
                {
                    foreach (int pos in room.RespwanList)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_Respawn(RoomPlayer, pos, last));
                    }
                }
            }
        }

        public static void GameMode_FootStep_GoalIn(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 4E 03 BA 52 02 00 01 01
            Console.WriteLine("FootStep_GoalIn");
            Account User = Client.CurrentAccount;
            if (User.GameEndType == 0)
            {
                NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                int laptime = reader.ReadLEInt32();
                byte unk = reader.ReadByte();//flag?
                User.LapTime = laptime;
                User.ServerLapTime = room.GetCurrentTime();
                //User.Rank = room.Rank++;
                if (!room.isGoal)
                {
                    room.isGoal = true;
                    if (room.RuleType == 8)
                    {
                        foreach (Account RoomPlayer in room.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
                            RoomPlayer.Connection.SendAsync(new Amsan_LapTimeControl(room.GetCurrentTime(), 10000, 0, false, last));
                            RoomPlayer.LastLapTime = User.ServerLapTime;
                            RoomPlayer.CurrentLapTime = 10000;
                        }
                        Client.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
                    }
                    else
                    {
                        foreach (Account RoomPlayer in room.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new FootStep_GoalIn_CountDown(laptime, 10000, last));
                            RoomPlayer.Connection.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
                            RoomPlayer.Connection.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
                            RoomPlayer.Connection.SendAsync(new GameRoom_StartTimeOutCount(User.LapTime + 2000, last));
                        }
                    }
                    long EndTime = Utility.CurrentTimeMilliseconds() + 15000;
                    Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                    //Task.Run(() => GameRoomEvent.Calc_DropItem(User, room, User.Rank, last));
                }
                else
                {
                    if (room.RuleType == 8)
                    {
                        foreach (Account RoomPlayer in room.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
                        }
                        Client.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
                    }
                    else
                    {
                        foreach (Account RoomPlayer in room.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new FootStep_GoalIn(User.RoomPos, 1, last));
                            RoomPlayer.Connection.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 1, last));
                        }
                    }
                    //Task.Run(() => GameRoomEvent.Calc_DropItem(User, room, User.Rank, last));
                }
            }
        }

        public static void GameMode_Amsan_LapTime(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF C1 02 00 00 00 00 00 20
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine(" GameMode_Amsan_2C1: {0}", Utility.ByteArrayToString(reader.Buffer));
            int unk = reader.ReadLEInt32();
            byte round = reader.ReadByte(); //round?
            room.BroadcastToAll(new Amsan_LapTime(unk, round, User.RoomPos, last));
        }
        public static void GameMode_Amsan_StepButton(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 39 03 27 00 00 00 08
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine("Amsan_Step_Button");
            int btnid = reader.ReadLEInt32();
            room.BroadcastToAll(new Amsan_Step_Button(btnid, User.RoomPos, last));
        }
        public static void GameMode_Amsan_StepButton_Push(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 39 03 27 00 00 00 08
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine("Amsan_Step_Button_Push");
            room.BroadcastToAll(new Amsan_Step_Button_Push(last));
        }
        public static void GameMode_Amsan_FinalButton(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 7F 03 40
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine("Amsan_Goal_Button");
            //User.LapTime = laptime;
            /*User.ServerLapTime = (int)(Utility.CurrentTimeMilliseconds() - room.StartTime);
            User.LapTime = User.LapTime;*/
            room.BroadcastToAll(new Amsan_Goal_Button(last));
        }
        public static void GameMode_Amsan_LapTimeControl(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 4C 03 E8 B2 00 00 00 00 00 00 01 20
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine("GameMode_Amsan_LapTimeControl: {0}", Utility.ByteArrayToString(reader.Buffer));
            int currenttime = reader.ReadLEInt32();
            int area = reader.ReadLEInt32();
            bool isCorrect = reader.ReadBoolean();
            int addtime = 15000;
            int remaintime = User.CurrentLapTime - (currenttime - User.LastLapTime);
            if (isCorrect)
                User.CurrentLapTime = remaintime + (room.isGoal ? 0 : addtime);
            else
                User.CurrentLapTime = remaintime - addtime;
            User.LastLapTime = currenttime;
            Client.SendAsync(new Amsan_LapTimeControl(currenttime, User.CurrentLapTime, addtime, isCorrect, last));
        }

        public static void GameMode_RandomGameOver(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 43 03 01 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            if (room.recvRandGameOver)
                return;
            room.recvRandGameOver = true;
            //Console.WriteLine("Random_GameOver: {0}", Utility.ByteArrayToString(reader.Buffer));
            byte area = reader.ReadByte();
            //List<byte> dieranklist = new List<byte>();
            if (room.PlayerCount() > 7)
            {
                var r = Enumerable.Range(1, room.Survival)
                            .Select(i => (byte)i).OrderBy(_ => Guid.NewGuid()).Take(1).ToList();
                room.GameOverRank.AddRange(r);
                //dieranklist.AddRange(r);
            }
            room.BroadcastToAll(new RandomGameOver(area, User.RoomPos, room.GameOverRank, last));
        }
        public static void GameMode_RandomGameOver_Die(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 45 03 06 00 5B 9F 3C 80
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte area = reader.ReadByte();
            float racelen = reader.ReadLESingle();//race length
            room.RacelengthList.Add(User.Session, racelen);
            //Client.SendAsync(new RandomGameOver_Die1(area, last));
            //Client.SendAsync(new RandomGameOver_Die2(area, last));

            if (room.RacelengthList.Count == room.Survival)
            {
                //var sortedDict = from entry in room.RacelengthList orderby entry.Value descending select entry;
                var sortedDict = room.RacelengthList.OrderByDescending(o => o.Value).Select(s => s);
                foreach (var rank in room.GameOverRank)
                {
                    var session = sortedDict.ElementAt(rank - 1).Key;
                    ClientConnection.CurrentAccounts.TryGetValue(session, out var ac);
                    ac.Connection.SendAsync(new RandomGameOver_Die1(area, last));
                    ac.Connection.SendAsync(new RandomGameOver_Die2(area, last));
                }
                room.GameOverRank.Clear();
                room.RacelengthList.Clear();
                room.recvRandGameOver = false;
            }
        }

        public static void GameMode_CatchFish(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int time = reader.ReadLEInt32();
            int fish = reader.ReadLEInt32();//0x1C 銀色魚，0x1D 銅色魚
            room.BroadcastToAll(new GameRoom_CatchFish(User, room, time, fish, last));
            room.CatchTime++;
        }

        public static void RunQuizMode_RequestQuizList(ClientConnection Client, byte last)
        {
            //eRoom_RUN_QUIZMODE_REQUEST_QUIZ_LIST_REQ
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.BroadcastToAll(new RequestQuizList_Ack(last));
        }

        public static void CorunMode_SetClearLimitTime(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_SET_CLEAR_LIMIT_TIME_REQ
            //FF C0 02 FF 64 03 64 00 00 00 10
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int sec = reader.ReadLEInt32();
            room.BroadcastToAll(new SetClearLimitTime_Ack(sec, last));
        }
        public static void CorunMode_ClearTimeSection(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_CLEAR_TIME_SECTION_REQ
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte area = reader.ReadByte();
            int ClearTime = reader.ReadLEInt32();
            int sec = reader.ReadLEInt32();
            if (room.GameMode == 5)
            {
                if (!room.ClearAreaTime.ContainsKey(area))
                    room.ClearAreaTime.Add(area, ClearTime);
            }
            room.BroadcastToAll(new ClearTimeSection_Ack(area, ClearTime, sec, last));
            //Log.Debug("RunQuizTimeControl area:{0} ClearTime:{1} sec:{2}", area, ClearTime, sec);
        }
        public static void CorunMode_EnterTimeSection(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_ENTER_TIME_SECTION_REQ
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            byte area = reader.ReadByte();
            int remaintime = reader.ReadLEInt32();
            int addtime = reader.ReadLEInt32();
            room.BroadcastToAll(new EnterTimeSection_Ack(area, remaintime, addtime, last));
            if (area == 100)
                room.StartKillBossRemainTime = remaintime + addtime * 1000;
            //Log.Debug("RunQuizAreaCountDown area:{0} remaintime:{1} addtime:{2}", area, remaintime, addtime);
        }

        public static void GameMode_TurtleEatItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF E8 05 00 00 00 00 20
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int itemtype = reader.ReadLEInt32();
            float fatigue = User.Fatigue;
            if (itemtype == 0)
            {
                if (fatigue >= 10f)
                    User.Fatigue -= ServerSettingHolder.ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_DEC;
                else
                    User.Fatigue = 0f;
            }
            else if (itemtype == 1)
            {
                if (fatigue <= 90f)
                    User.Fatigue += ServerSettingHolder.ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_INC;
                else
                    User.Fatigue = 100f;
            }

            Client.SendAsync(new TeamStatus(User, room, last));
        }
        public static void GameMode_ReqChangeTeamLeader(ClientConnection Client, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            Account Partner = room.Players.Find((Account p) => p.RoomPos == User.Partner);
            bool teamLeader = User.TeamLeader;
            if (teamLeader)
            {
                User.TeamLeader = false;
                Partner.TeamLeader = true;
                Client.SendAsync(new ReqChangeTeamLeader(0, last));
                Partner.Connection.SendAsync(new ReqChangeTeamLeader(0, last));
                Client.SendAsync(new NewTeamLeader(Partner.RoomPos, last));
                Partner.Connection.SendAsync(new NewTeamLeader(Partner.RoomPos, last));
            }
            else
            {
                Client.SendAsync(new ReqChangeTeamLeader(4, last));
                Partner.Connection.SendAsync(new ReqChangeTeamLeader(4, last));
            }
        }

        public static void CorunMode_SetBossEnergy(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_SET_BOSS_ENERGY_REQ[%d]
            //FF C0 02 FF 6A 03 10 27 00 00 FF FF FF FF 40
            int bosshp = reader.ReadLEInt32();
            int boss = reader.ReadLEInt32();
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.CorunBossHP = bosshp / ServerSettingHolder.ServerSettings.corunModeDecreaseEnergyRatio;
            room.BroadcastToAll(new CorunMode_SetBossEnergy_Ack(boss, last));
        }
        public static void CorunMode_DecreaseBossEnergy(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_DECREASE_BOSS_ENERGY_REQ
            //FF C0 02 FF 6D 03 64 00 00 00 FF FF FF FF 02
            int reducehp = reader.ReadLEInt32();
            int boss = reader.ReadLEInt32();
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int remainhp = room.CorunBossHP - reducehp;
            room.CorunBossHP = remainhp < 0 ? 0 : remainhp;
            room.BroadcastToAll(new CorunMode_DecreaseBossEnergy_Ack(boss, room.CorunBossHP, last));
        }
        public static void CorunMode_SetObjectBossEnergy(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_SET_OBJECT_BOSS_ENERGY_REQ
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int boss = reader.ReadLEInt32();
            long bossid = reader.ReadLEInt64();
            int bosshp = reader.ReadLEInt32();
            int inithp = bosshp / ServerSettingHolder.ServerSettings.corunModeDecreaseEnergyRatio;
            ObjectBoss objectBoss = new ObjectBoss();
            objectBoss.HP = inithp;
            objectBoss.MaxHP = inithp;
            room.ObjectBoss.TryAdd(bossid, objectBoss);
            room.BroadcastToAll(new CorunMode_SetObjectBossEnergy_Ack(boss, bossid, last));
        }
        public static void CorunMode_DecreaseObjectBossEnergy(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_DECREASE_OBJECT_BOSS_ENERGY_REQ
            int boss = reader.ReadLEInt32();
            long bossid = reader.ReadLEInt64();
            int reducehp = reader.ReadLEInt32();
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //int bossindex = room.ObjectBoss.FindIndex(o => o.BossId == bossid);
            //var objectBoss = room.ObjectBoss.Find(o => o.BossId == bossid);
            //int remainhp = objectBoss.HP - reducehp;
            //objectBoss.HP = remainhp < 0 ? 0 : remainhp;
            //room.ObjectBoss[bossindex] = objectBoss;
            int remainhp = room.ObjectBoss[bossid].HP - reducehp;
            room.ObjectBoss[bossid].HP = remainhp < 0 ? 0 : remainhp;
            room.BroadcastToAll(new CorunMode_DecreaseObjectBossEnergy_Ack(boss, bossid, room.ObjectBoss[bossid].HP, last));
        }
        public static void CorunMode_IncreaseObjectBossEnergy(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int boss = reader.ReadLEInt32();
            long bossid = reader.ReadLEInt64();
            int addhp = reader.ReadLEInt32();
            /*int bossindex = room.ObjectBoss.FindIndex(o => o.BossId == bossid);
            var objectBoss = room.ObjectBoss.Find(o => o.BossId == bossid);
            int remainhp = objectBoss.HP + addhp;
            objectBoss.HP = remainhp > objectBoss.MaxHP ? objectBoss.HP : remainhp;
            room.ObjectBoss[bossindex] = objectBoss;*/
            int remainhp = room.ObjectBoss[bossid].HP + addhp;
            room.ObjectBoss[bossid].HP = remainhp > room.ObjectBoss[bossid].MaxHP ? room.ObjectBoss[bossid].MaxHP : remainhp;
            room.BroadcastToAll(new CorunMode_IncreaseObjectBossEnergy_Ack(boss, bossid, room.ObjectBoss[bossid].HP, last));
        }
        public static void CorunMode_TriggerObjectEvent(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_TRIGGER_OBJECT_EVENT_REQ RideCattle
            //FF C0 02 FF 5E 03 14 00 00 00 01 00 00 00 00 00 00 40
            //FF C0 02 FF 5E 03 08 00 00 00 01 02 DB 00 00 00 00 40
            int unk = reader.ReadLEInt32();
            byte unk2 = reader.ReadByte();
            byte isLeave = reader.ReadByte();
            int unk3 = reader.ReadLEInt32();
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            if (isLeave == 0)
                room.RideCattlePlayer.Add(User);
            else if (isLeave == 1)
                room.RideCattlePlayer.Remove(User);
            if (room.RideCattlePlayer.Count == room.PlayerCount() || isLeave == 2)
            {
                room.BroadcastToAll(new TriggerObjectEvent_Ack(unk, isLeave, unk3, last));
                room.RideCattlePlayer.Clear();
            }
        }
        public static void CorunMode_TriggerCheckInObjectEvent(ClientConnection Client, PacketReader reader, byte last)
        {
            //eRoom_CORUN_MODE_TRIGGER_CHECK_IN_OBJECT_EVENT_REQ takecar
            //FF C0 02 FF 60 03 08 00 00 00 01 00 00 00 00 00 DB 00 00 00 40
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int needcount = reader.ReadLEInt32();
            int tookcount = reader.ReadLEInt32();
            short unk = reader.ReadLEInt16();
            int unk2 = reader.ReadLEInt32();
            room.BroadcastToAll(new TriggerCheckInObjectEvent_Ack(User.RoomPos, needcount, tookcount, unk2, last));
        }

        public static void Anubis_GetPlayerHP(ClientConnection Client, byte last)
        {
            //FF C0 02 FF 75 04 01 00 00 00 10
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            room.BroadcastToAll(new SendPlayerHP(room, last));
        }

        public static void Anubis_SetObjectBoss(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 72 04 03 00 00 00 90 89 1D 01 00 00 00 00 96 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 01 00 00 00 96 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 02 00 00 00 96 00 00 00 01 00 00 00 00 00 00 00 40
            //FF C0 02 FF 72 04 08 00 00 00 90 89 1D 01 03 00 00 00 FA 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 04 00 00 00 78 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 05 00 00 00 78 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 06 00 00 00 78 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 09 00 00 00 78 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 0A 00 00 00 78 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 0B 00 00 00 FA 00 00 00 01 00 00 00 00 00 00 00 90 89 1D 01 10 00 00 00 78 00 00 00 01 00 00 00 00 00 00 00 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int bosscount = reader.ReadLEInt32();
            for (int i = 1; i <= bosscount; i++)
            {
                int unk3 = reader.ReadLEInt32();
                int bossid = reader.ReadLEInt32();
                int hp = reader.ReadLEInt32();
                int unk = reader.ReadLEInt32();
                int unk2 = reader.ReadLEInt32();

                ObjectBoss objectBoss = new ObjectBoss();
                objectBoss.HP = hp;
                objectBoss.MaxHP = hp;

                room.AnubisObjectBoss.TryAdd(bossid, objectBoss);
            }
            room.BroadcastToAll(new AnubisObjectBossInit(room, last));
            //room.BroadcastToAll(new AnubisObjectBossInitFail(room, last));
        }

        public static void Anubis_DecreaseObjectBossHP(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 7C 04 FF FF FF FF 01 00 00 00 00 8F 00 00 00 10
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);

            int unk = reader.ReadLEInt32();
            int bossid = reader.ReadLEInt32();
            byte unk2 = reader.ReadByte();
            int reducehp = reader.ReadLEInt32();

            int remainhp = room.AnubisObjectBoss[bossid].HP - reducehp;
            room.AnubisObjectBoss[bossid].HP = remainhp < 0 ? 0 : remainhp;

            room.BroadcastToAll(new AnubisDecreasObjectBossHP(room, unk, bossid, reducehp, last));
        }

        public static void Anubis_DecreaseMyHP(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 77 04 02 00 00 00 05 00 00 00 01 00 00 00 00 00 00 00 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            int unk = reader.ReadLEInt32();
            int reducehp = reader.ReadLEInt32();
            int remainhp = User.HP - reducehp;
            User.HP = remainhp < 0 ? 0 : remainhp;
            room.BroadcastToAll(new AnubisDecreaseMyHP(User, last));
        }

        public static void Anubis_Rebirth(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 8D 04 01 01 00 00 00 01
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //TODO: 扣減復活石數量

            User.HP = User.MaxHP;
            Client.SendAsync(new AnubisRemainRebirthStone(User, last));
            room.BroadcastToAll(new AnubisRebirth(User, last));
        }

        private static void UpdateUserPoint(int UserNum, int rewardGroup, int deltaPoint)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_updateUserPoint";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
                    cmd.Parameters.Add("rewardGroup", MySqlDbType.Int32).Value = rewardGroup;
                    cmd.Parameters.Add("deltaPoint", MySqlDbType.Int32).Value = deltaPoint;
                    cmd.Parameters.Add("resultpoint", MySqlDbType.Int32);
                    cmd.Parameters["resultpoint"].Direction = ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                    /*using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        //reader.Read();
                    }*/
                }
            }
        }
        private static byte[] GenResult_ForMiniGameMode(NormalRoom room, byte last)
        {
            PacketWriter ns = new PacketWriter();
            ns = PacketWriter.CreateInstance(16, true);
            ns.Write((byte)0xFF);
            ns.Write((short)0x381);
            ns.Write((byte)room.RoomKindID);
            ns.Fill(room.IsTeamPlay == 0 ? 0xB : 0xA);
            if (room.IsTeamPlay != 0)
            {
                int winteam = room.Players.OrderBy(p => p.Rank).FirstOrDefault().Team;
                int loseteam = winteam == 1 ? 2 : 1;
                ns.Write((byte)0x1);
                ns.Write(2);
                ns.Write(winteam);
                ns.Write(loseteam);
                ns.Write(winteam);
            }
            int playercount = room.DropItem.Count;
            ns.Write((byte)playercount); //count?
            ns.Fill(0x6);
            foreach (var p in room.DropItem.OrderBy(o => o.Rank).ThenBy(o => o.Pos))
            {
                ns.Write(p.ServerLapTime);
                ns.Write(p.LapTime);
                ns.Write(p.Rank);
                ns.Write(p.Pos);
                ns.Write(p.EXP);
                ns.Write(p.MiniGameStarPoint);
                ns.Write(p.TR);
                ns.Write(0);
                ns.Write(0L);
                if (room.IsTeamPlay != 0)
                    ns.Write((int)p.Team);
                ns.Write(0);
                ns.Write(p.BounsEXP);
                ns.Write(p.BounsTR);
                int bounsweight = 0;
                if (p.BounsTR > 0)
                    bounsweight += 4;
                if (p.BounsEXP > 0)
                    bounsweight += 8;
                ns.Write(bounsweight); // 0C 00 00 00
                ns.Write(playercount); //02 00 00 00 player count?
                int count = p.CardID.Count;
                ns.Write(count); //card count

                foreach (var card in p.CardID)
                {
                    ns.Write(card);
                }
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B000000000000");
            }

            //minigame part
            //00 00 03 00 00 00 03 00 00 00 16 00 00 00
            //00 00 00 00 00 02 C2 01 00 00 02 00 00 00 01 00
            ns.Write((short)0);
            ns.Write(3);
            ns.Write(3);
            ns.Write(12 + playercount * 5);
            foreach (var p in room.DropItem.OrderBy(o => o.Pos))
            {
                ns.Write(p.Pos);
                ns.Write(p.MiniGamePoint);
            }
            ns.Write(playercount); //player count?
            ns.Write(1);
            ns.Write(4);
            //minigame part end

            bool hasgavegoods = GameRoomEvent.SendRegisterGoods(room, out byte[] getgoodsnickname, last);
            if (hasgavegoods)
            {
                Account roomMaster = room.Players.Find(p => p.RoomPos == room.RoomMasterIndex);
                roomMaster.Connection.SendAsync(new GameRoom_DeleteKeepItem(roomMaster, room, last));
            }

            ns.Write(0x3EA);
            ns.Write(!hasgavegoods ? 0x11 : 0x11 + getgoodsnickname.Length);
            ns.Write(room.ItemNum);

            if (!hasgavegoods)
            {
                ns.Write(-1L);
                ns.Write((byte)0);
                ns.Write(1);
            }
            else
            {
                ns.Write(0x3b1c32);//32 1C 3B 00
                ns.Write(2);
                ns.Write(getgoodsnickname, 0, getgoodsnickname.Length);//B5 A3 B8 DC B6 5D A4 48
                ns.Write((byte)0);
                ns.Write(getgoodsnickname.Length + 1);
            }
            ns.Write(0x3F6);
            ns.Write(4);
            ns.Write(0);
            ns.Write(0xC);
            ns.Write(last);
            byte[] ret = ns.ToArray();
            PacketWriter.ReleaseInstance(ns);
            ns = null;
            //Console.WriteLine("Result packet: " + Utility.ByteArrayToString(ret));
            return ret;
        }
    }
}
