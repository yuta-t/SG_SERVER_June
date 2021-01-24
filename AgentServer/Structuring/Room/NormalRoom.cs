using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet;
using AgentServer.Packet.Send;
using AgentServer.Structuring.Map;
using AgentServer.Structuring.Room;
using LocalCommons.Logging;
using LocalCommons.Network;
using LocalCommons.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentServer.Structuring
{
    public class NormalRoom : IDisposable
    {
        public int ID;
        public string Name;
        public string Password;
        public int IsTeamPlay;
        public int ItemType;
        public bool IsStepOn;
        public int MapNum = 1;
        public int RoomKindID;
        public byte MaxPlayersCount = 8;
        public byte SlotCount = 8;
        public bool HasBuff = false;
        public List<Account> Players = new List<Account>();
        public bool HasPiero = false;
        public bool HasAfreeca = false;
        public bool HasPassword;
        public List<byte> PosList = new List<byte>();// t = new List<byte> { 0, 1, 2, 3, 4, 5, 6, 7 };
        public List<byte> RelayPosList = new List<byte>();
        public int PosWeight = 0;
        public bool is8Player = true;
        public byte RoomMasterIndex;
        public bool isPlaying = false;
        //public long StartTime;
        public bool isGoal = false;
        public int MatchTime = 1;
        public Dictionary<Account, DateTime> KickedList = new Dictionary<Account, DateTime>();
        public List<DropList> DropItem = new List<DropList>();
        public List<Account> LevelUPPlayerList = new List<Account>();
        public byte Rank = 1;
        public DateTime ChangeMapTime = DateTime.Now;
        public int PlayingMapNum = 0;
        public byte Survival;
        public int GameMode;
        public int Channel;
        public int RuleType;
        public int CatchTime = 200;//捉魚次數，由200開始計算
        public int CapsuleNum = 200;
        public Dictionary<int, int> RegCapsule = new Dictionary<int, int>();
        public float MapMaxDistance;
        public Dictionary<int, short> RewardGroupList = new Dictionary<int, short>();
        public byte[] Result;

        //魔書解密
        public bool recvRandGameOver = false;
        public List<byte> GameOverRank = new List<byte>();
        public Dictionary<int, float> RacelengthList = new Dictionary<int, float>();

        //八人同心
        public int CorunBossHP;
        public Dictionary<byte, int> ClearAreaTime = new Dictionary<byte, int>();
        public int StartKillBossRemainTime;
        public byte[] CorunModeResult;
        public List<Account> RideCattlePlayer = new List<Account>();
        public ConcurrentDictionary<long, ObjectBoss> ObjectBoss = new ConcurrentDictionary<long, ObjectBoss>();

        //阿努比斯
        public ConcurrentDictionary<long, ObjectBoss> AnubisObjectBoss = new ConcurrentDictionary<long, ObjectBoss>();

        //物品登錄
        public int ItemNum = -1;
        public long Storage_Id = -1;
        public int isOrderBy = 0;
        public int SendRank = 0;
        public bool isPublic = false;

        public byte Round = 0;
        public HashSet<byte> RespwanList = new HashSet<byte>();

        private Stopwatch _Stopwatch = new Stopwatch();
        private Timer _ChangeRoomMasterTimer;
        private readonly object syncRoot = new object();
        private readonly object playerLock = new object();
        private readonly object goalLock = new object();
        private readonly object gameoverLock = new object();
        private readonly object enterroomLock = new object();

        public int FarmIndex;

        public int PlayerCount()
        {
            return Players.Count(p => p.Attribute != 3);
        }
        public List<Account> PlayerList()
        {
            lock (playerLock)
            {
                return new List<Account>(Players);
            }
        }
        public void setID(int id)
        {
            ID = id;
        }
        public void setName(string name)
        {
            Name = name;
        }
        public void setPassword(string password)
        {
            HasPassword = password != string.Empty;
            Password = password;
        }
        public void setItemType(int itemtype)
        {
            ItemType = itemtype;
        }
        public void setIsStepOn(bool isstepon)
        {
            IsStepOn = isstepon;
        }
        public void setRoomKindID(int roomkindid)
        {
            RoomKindID = roomkindid;
        }
        public void setIsTeamPlay(int isteamplay)
        {
            IsTeamPlay = isteamplay;
        }
        public void setMaxPlayersCount(byte maxplayerscount)
        {
            MaxPlayersCount = maxplayerscount;
        }
        public void setSlotCount(byte count)
        {
            SlotCount = count;
        }
        public void setPosList(byte maxplayerscount)
        {
            for (byte i = 0; i < maxplayerscount; i++)
            {
                PosList.Add(i);
            }
        }
        public void setPosWeight(byte roompos, bool isOff)
        {
            if (isOff)
            {
                PosWeight += 1 << roompos;
                PosList.Remove(roompos);
                SlotCount -= 1;
            }
            else
            {
                PosWeight -= 1 << roompos;
                PosList.Add(roompos);
                SlotCount += 1;
            }
        }
        public void setRelayPosWeight(byte teamslot, bool isOff)
        {
            int begin = teamslot * 3 - 3;
            if (isOff)
            {
                for (int i = begin; i < begin + 3; i++)
                {
                    PosWeight += 1 << i;
                    RelayPosList.Remove((byte)(i + 3));
                }
            }
            else
            {
                for (int i = begin; i < begin + 3; i++)
                {
                    PosWeight -= 1 << i;
                    RelayPosList.Add((byte)(i + 3));
                }
   
            }
        }
        public void setRoomMasterIndex(byte index)
        {
            RoomMasterIndex = index;
        }
        public void addMatchTime()
        {
            MatchTime++;
        }
        public void addKickedPlayer(Account User)
        {
            KickedList.Add(User, DateTime.Now);
        }
        public void setMapNum(int mapid)
        {
            MapNum = mapid;
            ChangeMapTime = DateTime.Now;
        }
        public void RegisterItem(int itemnum, long storage_id, int isorderby, int sendrank, bool ispublic)
        {
            //Log.Info("Register Item - ItemNum: {0}, Storage_Id: {1}, isOrderBy: {2}, SendRank: {3}, isPublic: {4}", itemnum, storage_id, isorderby, sendrank, ispublic);
            ItemNum = itemnum;
            Storage_Id = storage_id;
            isOrderBy = isorderby;
            SendRank = sendrank;
            isPublic = ispublic;
        }
        public void setGameMode(RoomKindInfo roomkindinfo)
        {
            GameMode = roomkindinfo.GameMode;
            Channel = roomkindinfo.Channel;
            byte Maxplayer = 0;
            switch (GameMode)
            {
                case 1: //8人
                    Maxplayer = 8;
                    is8Player = true;
                    break;
                case 2: //30人
                    Maxplayer = ServerSettingHolder.ServerSettings.SurvivalMaxUserNum;
                    is8Player = false;
                    break;
                case 3: //接力
                    Maxplayer = 20;
                    is8Player = false;
                    setRelayPosList();
                    break;
                case 5: //八心
                    Maxplayer = 8;
                    is8Player = true;
                    break;
                case 14://阿努比斯
                    Maxplayer = 8;
                    is8Player = true;
                    break;
                case 16://公園
                    Maxplayer = 50;
                    is8Player = false;
                    break;
                case 17://農場
                    Maxplayer = 20;
                    is8Player = false;
                    break;
                case 38: //小遊戲
                    Maxplayer = 4;
                    is8Player = true;
                    break;
                default:
                    Maxplayer = 8;
                    is8Player = true;
                    break;
            }
            setMaxPlayersCount(Maxplayer);
            setPosList(Maxplayer);
            setSlotCount(Maxplayer);
        }
        private void setRelayPosList()
        {
            for (byte i = 3; i < 21; i++)
            {
                RelayPosList.Add(i);
            }
        }


        public void BroadcastToAllExclude(NetPacket np, Account User)
        {
            foreach (Account RoomPlayer in PlayerList())
            {
                if (RoomPlayer.GlobalID != User.GlobalID)
                {
                    RoomPlayer.Connection.SendAsync(np);
                }
            }
            /*Players.ForEach((x) => {
                x.Connection.SendAsync(np);
            });*/
        }
        public void BroadcastToAll(NetPacket np)
        {
            foreach (Account RoomPlayer in PlayerList())
            {
                RoomPlayer.Connection.SendAsync(np);
            }
            /*Players.ForEach((x) => {
                x.Connection.SendAsync(np);
            });*/
        }
        public bool EnterRoomCheck(Account User, string pw, out byte ErrorCode)
        {
            ErrorCode = 1;
            if (isPlaying)
            {
                ErrorCode = 5;
                return false;
            }
            if (PlayerCount() < SlotCount || User.Attribute == 3)
            {            
                if (KickedList.TryGetValue(User, out var kickedtime))
                {
                    bool cantenter = DateTime.Compare(DateTime.Now, kickedtime.AddSeconds(60)) < 0;
                    if (cantenter)
                    {
                        ErrorCode = 6;
                        return false;
                    }
                    KickedList.Remove(User);
                }
                if (Password != pw && User.Attribute == 0)
                {
                    ErrorCode = 7;
                    return false;
                }
                if (Players.Exists(p => p.Attribute == 3) && User.Attribute == 3)
                {
                    ErrorCode = 9;
                    return false;
                }
                if (Channel == 1)
                {
                    if (User.Exp > ServerSettingHolder.ServerSettings.NewbieOnlyChannelLimitExp && User.Attribute == 0)
                    {
                        ErrorCode = 12;
                        return false;
                    }
                }
            }
            else
            {
                ErrorCode = 1;
                return false;
            }
            return true;
        }
        public bool CheckCanStart(out int ErrorCode)
        {
            ErrorCode = 0;
            bool justchange = DateTime.Compare(DateTime.Now, ChangeMapTime.AddSeconds(3)) < 0;
            if (justchange)
            {
                ErrorCode = 9;
                return false;
            }
            if (isOrderBy == 1 && SendRank > PlayerCount())
            {
                ErrorCode = 8; //不符合商品登錄條件，遊戲無法開始。
                return false;
            }
            if (IsTeamPlay == 2)
            {
                int redteamcount = Players.Count(p => p.Team == 1 && p.Attribute != 3);
                int blueteamcount = Players.Count(p => p.Team == 2 && p.Attribute != 3);
                if (redteamcount > blueteamcount + 1 || redteamcount + 1 < blueteamcount) //多過另一隊2人不能開始
                {
                    ErrorCode = 3; //團隊不適合
                    return false;
                }
            }
            return true;
        }
        public bool CheckReadyPlayerNum(out int ErrorCode)
        {
            ErrorCode = 0;
            //如果是八人房間的話，則檢查除房主以外的所有人是否有準備
            switch (GameMode)
            {
                case 1:
                case 14://阿努比斯
                case 38:
                    if (!Players.FindAll(p => p.RoomPos != RoomMasterIndex).All(player => player.IsReady))
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    //所有Map最少2人
                    if ((Players.Count(p => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1) < 2)
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    break;
                case 2: //生存
                    bool isSurvival = RuleType == 2 || RuleType == 4 || RuleType == 8;
                    if (isSurvival && (Players.Count(p => p.IsReady && p.Attribute != 3) + 1) < ServerSettingHolder.ServerSettings.SurvivalMinUserNum) //包括場主
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    //所有Map最少2人
                    if ((Players.Count(p => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1) < 2)
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    break;
                case 3: //接力
                    var readysount = Players.Count(w => w.RelayTeamPos > 2 && w.RelayTeamPos < 21);
                    var roommaster = Players.Find(p => p.RoomPos == RoomMasterIndex);
                    bool roommasterready = roommaster.Attribute == 3 ? true : roommaster.IsReady;
                    if (readysount < 6 || !roommasterready) //人數不足2隊/場主冇入隊
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    for (byte i = 1; i < 7; i++)
                    {
                        int teamcount = Players.Count(w => w.RelayTeam == i);
                        if (teamcount == 0)
                            continue;
                        if (teamcount != 3)//人數不足
                        {
                            ErrorCode = 1; //1 = 現在無法開始
                            return false;
                        }
                    }
                    break;
                case 5: //八心
                    if (!Players.FindAll(p => p.RoomPos != RoomMasterIndex).All(player => player.IsReady))
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    if ((Players.Count(p => p.IsReady && p.RoomPos != RoomMasterIndex && p.Attribute != 3) + 1) < ServerSettingHolder.ServerSettings.corunModeMinPlayerNum)
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    break;
                default:
                    if (!Players.FindAll(p => p.RoomPos != RoomMasterIndex).All(player => player.IsReady))
                    {
                        ErrorCode = 1; //1 = 現在無法開始
                        return false;
                    }
                    break;
            }
            return true;
        }

        public void SetRewardGroupList()
        {
            RewardGroupList.Clear();
            var type1 = GameRewardHolder.GroupInfos.FirstOrDefault(f => f.Value.GroupType == 1 && f.Value.Argument == RoomKindID);
            if (type1.Value != null)
                RewardGroupList.Add(type1.Key, type1.Value.GroupType);
            var type2 = GameRewardHolder.GroupInfos.FirstOrDefault(f => f.Value.GroupType == 2 && f.Value.Argument == PlayingMapNum);
            if (type2.Value != null)
            {
                RewardGroupList.Add(type2.Key, type2.Value.GroupType);
                if (type2.Value.ChildGroupNum > 0)
                {
                    GameRewardHolder.GroupInfos.TryGetValue(type2.Value.ChildGroupNum, out var childgp);
                    RewardGroupList.Add(type2.Value.ChildGroupNum, childgp.GroupType);
                }
            }
        }
        public void StartGame()
        {
            _Stopwatch.Start();
        }
        public int GetCurrentTime()
        {
            return (int)_Stopwatch.ElapsedMilliseconds;
        }

        public void TeleportPlayers()
        {
            /***Strugarden Code Block***/
            foreach (var roomUser in PlayerList())
            {
                BroadcastToAll(new LoginTeleportCharAppear_0x3E8_00(roomUser));
            }
        }
        public void EnterRoom(ClientConnection Client, string pw, byte last)
        {
            /***Strugarden Code Block***/
            Account User = Client.CurrentAccount;
            Players.Add(User);
            foreach (var roomUser in PlayerList())
            {
                if (roomUser.GlobalID != User.GlobalID)
                {
                    Client.SendAsync(new LoginCharAppear_0x3E8_00(roomUser));
                }
            }
            BroadcastToAll(new LoginCharAppear_0x3E8_00(User));
                /*Account User = Client.CurrentAccount;
                lock (enterroomLock)
                {
                    if (EnterRoomCheck(User, pw, out var Err))
                    {
                        User.InGame = true;
                        User.CurrentRoomId = ID;
                        User.IsReady = false;

                        //取得當前第一個位置id
                        User.RoomPos = (byte)(User.Attribute == 3 ? 100 : PosList.FirstOrDefault());
                        PosList.Remove(User.RoomPos);

                        Client.SendAsync(new GameRoom_GoodsInfo(this, last));
                        if (RoomKindID != 75)
                            Client.SendAsync(new GameRoom_SendRoomInfo(this, last, User.RoomPos));
                        else
                            Client.SendAsync(new SendFarmInfo(User, this, FarmIndex, last));

                        bool isRelayMode = GameMode == 3;
                        bool isTeamPlay = IsTeamPlay == 2;
                        if (isTeamPlay)
                        {
                            int redcount = Players.Count(p => p.Team == 1);
                            int blueteam = Players.Count(p => p.Team == 2);
                            User.Team = (byte)(redcount <= blueteam ? 1 : 2);
                        }
                        if (isRelayMode)
                        {
                            int pos1count = Players.Count(p => p.RelayTeamPos == 1);
                            int pos2count = Players.Count(p => p.RelayTeamPos == 2);
                            byte selectpos = (byte)(pos1count <= pos2count ? 1 : 2);
                            User.SelectRelayTeam(selectpos);
                        }
                        if (User.Attribute == 3)
                        {
                            setRoomMasterIndex(100);
                        }

                        Players.Add(User);

                        foreach (var roomUser in PlayerList())
                        {
                            //Send roomuser info俾自己
                            Client.SendAsync(new GameRoom_SendPlayerInfo(roomUser, last));
                            Client.SendAsync(new GameRoom_RoomPosReady(roomUser.RoomPos, roomUser.IsReady, last));
                            if (isTeamPlay)
                                Client.SendAsync(new GameRoom_RoomPosTeam(roomUser, last));
                            if (isRelayMode)
                                Client.SendAsync(new GameRoom_RoomPosRelayTeam(roomUser, last));
                            if (User.Attribute == 3)
                                roomUser.Connection.SendAsync(new GameRoom_GetRoomMaster(RoomMasterIndex, last));

                            //Send自己info俾其他roomuser
                            if (roomUser.RoomPos != User.RoomPos)
                            {
                                roomUser.Connection.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                                if (isTeamPlay)
                                    roomUser.Connection.SendAsync(new GameRoom_RoomPosTeam(User, last));
                                if (isRelayMode)
                                    roomUser.Connection.SendAsync(new GameRoom_RoomPosRelayTeam(User, last));
                            }
                        }

                        if (User.Attribute != 3)
                        {
                            byte roommasterpos = RoomMasterIndex;
                            Client.SendAsync(new GameRoom_GetRoomMaster(roommasterpos, last));
                        }
                    }
                    else
                    {
                        Client.SendAsync(new GameRoom_EnterRoomError(User, Err, (byte)RoomKindID, last));
                    }
                }*/
            }

        public void GameOver(Account User, byte last)
        {
            lock (gameoverLock)
            {
                if (User.GameEndType == 0 && !User.GameOver)
                {
                    Survival -= 1;
                    User.GameEndType = 4;
                    User.GameOver = true;
                    BroadcastToAll(new GameRoom_GameOver(User.RoomPos, last));
                    Log.Info("{0} GameOver, Survival:{1}", User.NickName, Survival);
                    switch (RuleType)
                    {
                        case 2: //生存
                            if (Survival == 1 && GameMode != 3)
                            {
                                var aliveplayer = Players.Find(f => f.GameEndType == 0 && f.Attribute != 3);
                                BroadcastToAll(new GameRoom_Alive(aliveplayer.RoomPos, last));
                                aliveplayer.GameEndType = 2;
                                aliveplayer.LapTime = GetCurrentTime();
                                aliveplayer.ServerLapTime = User.LapTime;
                                long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                Task.Run(() => GameRoomEvent.Execute_GameEnd(this, EndTime, last));
                            }
                            else if (GameMode == 3)
                            {
                                var teamplayer = Players.Where(w => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum);
                                foreach (var p in teamplayer)
                                {
                                    Survival -= 1;
                                    p.GameEndType = 4;
                                    p.GameOver = true;
                                }
                                var survivalteam = Players.Where(w => w.RelayTeam != User.RelayTeam && w.GameEndType == 0);
                                if (survivalteam.Select(s => s.RelayTeam).Distinct().Count() == 1)
                                {
                                    foreach (var p in survivalteam)
                                    {
                                        BroadcastToAll(new GameRoom_Alive(p.RoomPos, last));
                                        p.GameEndType = 2;
                                        p.LapTime = GetCurrentTime();
                                        p.ServerLapTime = User.LapTime;
                                    }
                                    long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                    Task.Run(() => GameRoomEvent.Execute_GameEnd(this, EndTime, last));
                                }
                            }
                            break;
                        case 3: //Hardcore
                        case 4: //跑步吧
                        case 8: //衝呀!!
                        case 64://八心
                            if (Survival == 0)
                            {
                                long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                Task.Run(() => GameRoomEvent.Execute_GameEnd(this, EndTime, last));
                            }
                            break;
                        /*case 20480:
                            //MiniGame
                            break;*/
                        default:
                            break;
                    }
                }
            }
        }
        public void GoalIn(Account User, int laptime, int servertime, byte last)
        {
            lock (goalLock)
            {
                User.LapTime = laptime;
                User.ServerLapTime = servertime;
                //User.Rank = Rank++;
                if (!isGoal)
                {
                    MapHolder.MapInfos.TryGetValue(PlayingMapNum, out MapInfo mapinfo);
                    if (GameMode == 5)
                    {
                        if (CorunBossHP > 0)
                            return; //hack
                    }
                    if (mapinfo.GoalInLimitTime * 1000 < servertime)
                    {
                        Console.WriteLine("GoalInOK");
                        isGoal = true;
                        foreach (var RoomPlayer in PlayerList())
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 0, last));
                            RoomPlayer.Connection.SendAsync(new GameRoom_StartTimeOutCount(User.LapTime + 2000, last));
                            //FF 7C 03 E1 38 02 00 0A
                        }
                        if (GameMode == 5)
                        {
                            int i = 0;
                            foreach (var p in Players.OrderBy(o => o.RoomPos))
                            {
                                p.LapTime = laptime + i;
                                p.ServerLapTime = servertime + i;
                                i++;
                            }
                        }
                        long EndTime = Utility.CurrentTimeMilliseconds() + 15000;
                        Task.Run(() => GameRoomEvent.Execute_GameEnd(this, EndTime, last));
                    }
                    else
                    {
                        Console.WriteLine("GoalInError");
                        User.Connection.SendAsync(new DisconnectPacket(User, 258, last));
                    }
                }
                else
                {
                    BroadcastToAll(new GameRoom_GoalInData(User.RoomPos, User.LapTime, 0, last));
                }
            }
        }

        public void DisconnectDropList(Account User)
        {
            var i = DropItem.Find(f => f.UserNum == User.UserNum);
            User.GameEndType = 4;
            if (i == null)
            {
                DropList dropList = new DropList
                {
                    UserNum = User.UserNum,
                    TotalEXP = User.Exp,
                    RaceDistance = 0,
                    ServerLapTime = 1000000000,
                    LapTime = 1000000000,
                    Pos = User.RoomPos,
                    Team = User.Team,
                    RelayTeam = User.RelayTeam,
                    RelayTeamPos = User.RelayTeamPos,
                    BounsTR = 0,
                    BounsEXP = 0,
                    TR = 0,
                    EXP = 0,
                    MiniGamePoint = 0,
                    MiniGameStarPoint = 0,
                    Rank = 99,
                    isLevelUP = false,
                    CardID = new List<int> { 0 }
                };
                DropItem.Add(dropList);
            }
            else
            {
                i.RaceDistance = 0;
                i.ServerLapTime = 1000000000;
                i.LapTime = 1000000000;
                i.BounsTR = 0;
                i.BounsEXP = 0;
                i.TR = 0;
                i.EXP = 0;
                i.MiniGamePoint = 0;
                i.MiniGameStarPoint = 0;
                i.Rank = 99;
            }
            if (GameMode == 3)
            {
                User.GameEndType = 4;
                foreach (var teamp in Players.Where(w => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum))
                {
                    var j = DropItem.Find(f => f.UserNum == teamp.UserNum);
                    if (j == null)
                    {
                        DropList dropList = new DropList
                        {
                            UserNum = teamp.UserNum,
                            TotalEXP = User.Exp,
                            RaceDistance = 0,
                            ServerLapTime = 1000000000,
                            LapTime = 1000000000,
                            Pos = teamp.RoomPos,
                            Team = teamp.Team,
                            RelayTeam = teamp.RelayTeam,
                            RelayTeamPos = teamp.RelayTeamPos,
                            BounsTR = 0,
                            BounsEXP = 0,
                            TR = 0,
                            EXP = 0,
                            MiniGamePoint = 0,
                            MiniGameStarPoint = 0,
                            Rank = 99,
                            isLevelUP = false,
                            CardID = new List<int> { 0 }
                        };
                        DropItem.Add(dropList);
                    }
                    else
                    {
                        j.RaceDistance = 0;
                        j.ServerLapTime = 1000000000;
                        j.LapTime = 1000000000;
                        j.BounsTR = 0;
                        j.BounsEXP = 0;
                        j.TR = 0;
                        j.EXP = 0;
                        j.MiniGamePoint = 0;
                        j.MiniGameStarPoint = 0;
                        j.Rank = 99;
                    }
                    teamp.GameEndType = 4;
                }
            }
        }
        public void GameEndSetNewRoomMaster()
        {
            //重新設定房主
            if (!Players.Exists(p => p.Attribute == 3))
            {
                var newRoomMaster = Players.Exists(p => p.Attribute == 1) ? Players.FirstOrDefault(p => p.Attribute == 1) : Players.OrderBy(p => p.Rank).FirstOrDefault();
                RoomMasterIndex = newRoomMaster.RoomPos;
                BroadcastToAll(new GameRoom_GetRoomMaster(newRoomMaster.RoomPos, 0x1));
            }
        }
        public void GameEndReset()
        {
            Rank = 1;
            Result = null;
            DropItem.Clear();
            LevelUPPlayerList.Clear();
            RespwanList.Clear();
            isGoal = false;
            isPlaying = false;
            PlayingMapNum = 0;
            RuleType = 0;
            CatchTime = 200;
            foreach (Account RoomPlayer in Players)
            {
                RoomPlayer.IsReady = false;
                RoomPlayer.EndLoading = false;
                RoomPlayer.GameEndType = 0;
                RoomPlayer.GameOver = false;
                RoomPlayer.Partner = 8;
                RoomPlayer.Fatigue = 0f;
                RoomPlayer.TeamLeader = false;
                RoomPlayer.RequestChange = false;
            }
            RegCapsule.Clear();
            _Stopwatch.Reset();
            ClearAreaTime.Clear();
            ObjectBoss.Clear();
            AnubisObjectBoss.Clear();
        }

        #region 定時自動更換場主
        public void StartAutoChangeRoomMaster()
        {
            _ChangeRoomMasterTimer = new Timer((e) =>
            {
                ChangeRoomMaster();
            }, null, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        }
        public void ResetAutoChangeRoomMaster()
        {
            _ChangeRoomMasterTimer.Change(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
        }
        private void ChangeRoomMaster()
        {
            if (Players.Find(p => p.RoomPos == RoomMasterIndex).Attribute == 0 && Players.Count > 1 && !isPlaying)
            {
                bool hasPiero = Players.Exists(p => p.Attribute == 1);
                Account newRoomMaster;
                if (hasPiero)
                {
                    newRoomMaster = Players.FindAll(p => p.Attribute == 1).FirstOrDefault();
                }
                else
                {
                    newRoomMaster = Players.Find(f => f.RoomPos > RoomMasterIndex);
                    if (newRoomMaster == null)
                        newRoomMaster = Players.OrderBy(o => o.RoomPos).FirstOrDefault();
                }
                RoomMasterIndex = newRoomMaster.RoomPos;
                BroadcastToAll(new GameRoom_GetRoomMaster(newRoomMaster.RoomPos, 0x1));
            }
        } 
        #endregion

        #region 踢除60秒內沒有完成載入的玩家
        public void StartWaitAllSyncThread()
        {
            Task.Run(() => WaitAllSync());
        }
        private void WaitAllSync()
        {
            long nowtime = Utility.CurrentTimeMilliseconds();
            bool isAllSync = false;
            _ChangeRoomMasterTimer.Dispose();
            while (Utility.CurrentTimeMilliseconds() < nowtime + ServerSettingHolder.ServerSettings.LoadingTimeOutMilliSeconds)
            {
                Thread.Sleep(2000);
                if (Players.All(p => p.EndLoading))
                {
                    Thread.Sleep(500);
                    BroadcastToAll(new GameRoom_AllSync(0x1));
                    isAllSync = true;
                    break;
                }
            }
            Console.WriteLine("isAllSync: {0}", isAllSync);
            if (!isAllSync)
            {
                Account[] Users = Players.FindAll(player => !player.EndLoading).ToArray();
                foreach (Account KickedPlayer in Users)
                {
                    GameRoomEvent.KickPlayer(KickedPlayer, this, 0x1);
                    if (KickedPlayer.RoomPos == RoomMasterIndex)
                    {
                        if (ItemNum != -1)
                        {
                            KickedPlayer.Connection.SendAsync(new GameRoom_LockKeepItem(this, true, 0x1));//解鎖之前保管了的物品
                            RegisterItem(-1, -1, 0, 0, false);
                        }
                        Account newRoomMaster = Players.FirstOrDefault();
                        RoomMasterIndex = newRoomMaster.RoomPos;
                        BroadcastToAll(new GameRoom_GetRoomMaster(newRoomMaster.RoomPos, 0x1));
                    }
                }
                if (Players.Count == 0)
                {
                    //Rooms.NormalRoomList.Remove(this);
                    Rooms.RemoveRoom(ID);
                    Dispose();
                }
                else
                {
                    BroadcastToAll(new GameRoom_AllSync(0x1));
                }
            }
        }
        #endregion

        #region 開始倒計時超時
        public void StartTimeoutCountDownThread()
        {
            Task.Run(() => TimeoutCountDown());
        }
        private void TimeoutCountDown()
        {
            long nowtime = Utility.CurrentTimeMilliseconds();
            while (Utility.CurrentTimeMilliseconds() < nowtime + 10000)
            {
                Thread.Sleep(1000);
                if (PlayingMapNum != 0 || !isPlaying)
                    break;
            }
            if (PlayingMapNum == 0 || !isPlaying)
            {
                BroadcastToAll(new GameRoom_Hex("FF0903", 0x1));
                isPlaying = false;
            }
        }
        #endregion

        public void StartHareAndTortoiseThread()
        {
            Task.Run(() => HareAndTortoiseStatus());
        }
        private void HareAndTortoiseStatus()
        {
            int start = 0;
            bool send = false;
            while (isPlaying && PlayingMapNum == 1606)
            {
                start++;
                Thread.Sleep(1000);
                send = start % 4 == 0;
                foreach (Account RoomUser in Players)
                {
                    float fatigue = RoomUser.Fatigue;
                    if (RoomUser.TeamLeader)
                    {
                        if (fatigue <= 97f)
                            RoomUser.Fatigue += ServerSettingHolder.ServerSettings.RABBIT_TURTLE_FATIGUE_INC;
                        else
                            RoomUser.Fatigue = 100f;
                    }
                    else
                    {
                        if (fatigue >= 3f)
                            RoomUser.Fatigue -= ServerSettingHolder.ServerSettings.RABBIT_TURTLE_FATIGUE_DEC;
                        else
                            RoomUser.Fatigue = 0f;
                    }
                    if(send)
                        RoomUser.Connection.SendAsync(new TeamStatus(RoomUser, this, 1));
                }
            }
        }

        public void Dispose()
        {
            _ChangeRoomMasterTimer.Dispose();
        }
    }
}
