using AgentServer.Packet.Send;
using AgentServer.Structuring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using AgentServer.Holders;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using LocalCommons.Network;
using LocalCommons.Logging;
using LocalCommons.Cryptography;
using AgentServer.Network.Connections;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.GameReward;

namespace AgentServer.Packet
{
    public class GameRoomEvent
    {
        public static void Execute_GameEnd(NormalRoom room, long EndTime, byte last)
        {
            room.addMatchTime();
            while (Utility.CurrentTimeMilliseconds() < EndTime)
            {
                if (room.Players.Where(p => p.Attribute != 3).All(p => p.GameEndType > 0))
                    break;
                Thread.Sleep(1000);
            };

            if (room.Players.Exists(p => p.GameEndType == 0 && p.Attribute != 3))
            {
                foreach (Account player in room.Players.Where(p => p.GameEndType == 0 && p.Attribute != 3).OrderBy(o => o.RaceDistance))
                {
                    player.LapTime = room.GetCurrentTime() + 300000;
                    player.ServerLapTime = player.LapTime;
                    player.Rank = 99;//99 = TIME OVER, 98 = GAME OVER
                    player.GameEndType = 3;
                }
            }

            if (room.GameMode != 3)
            {
                foreach (var player in room.Players.ToList().OrderBy(o => o.ServerLapTime).ThenByDescending(o => o.RaceDistance))
                {
                    if (player.GameEndType == 1)//goalin
                    {
                        player.Rank = room.Rank++;
                    }
                    if (player.GameEndType == 2)//alive
                    {
                        player.Rank = room.Rank++;
                    }
                    else if (player.GameEndType == 3)//timeover
                    {
                        player.LapTime = room.GetCurrentTime() + 220000;
                        player.ServerLapTime = player.LapTime;
                        player.Rank = 99;
                    }
                    else if (player.GameEndType == 4)//gameover
                    {
                        player.LapTime = room.GetCurrentTime() + 240000;
                        player.ServerLapTime = player.LapTime;
                        switch (room.RuleType)
                        {
                            case 4:
                                player.Rank = 99;
                                break;
                            default:
                                player.Rank = 98;
                                break;
                        }
                    }
                    else if (player.GameEndType == 5)//footstep
                    {
                        player.Rank = room.Rank++;
                    }
                    //Calc_DropItem(player, room, player.Rank, last);
                    //Thread.Sleep(100);
                }
            }
            else
            {
                foreach (var player in room.Players.ToList().Where(w => w.RelayTeamPos == 5 || w.RelayTeamPos == 8 || w.RelayTeamPos == 11 || w.RelayTeamPos == 14 || w.RelayTeamPos == 17 || w.RelayTeamPos == 20)
                    .OrderBy(o => o.ServerLapTime).ThenByDescending(o => o.RaceDistance))
                {
                    if (player.GameEndType == 1)//goalin
                    {
                        player.Rank = room.Rank++;
                        int i = 1;
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                        {
                            teamplayer.LapTime = player.LapTime + i;
                            teamplayer.ServerLapTime = player.ServerLapTime + i;
                            teamplayer.Rank = room.Rank++;
                            i++;
                            //Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                        }
                    }
                    if (player.GameEndType == 2)//alive
                    {
                        player.Rank = room.Rank++;
                        int i = 1;
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                        {
                            teamplayer.LapTime = player.LapTime + i;
                            teamplayer.ServerLapTime = player.ServerLapTime + i;
                            teamplayer.Rank = room.Rank++;
                            i++;
                            //Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                        }
                    }
                    else if (player.GameEndType == 3)//timeover
                    {
                        player.LapTime = room.GetCurrentTime() + 220000;
                        player.ServerLapTime = player.LapTime;
                        player.Rank = 99;
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                        {
                            teamplayer.LapTime = player.LapTime;
                            teamplayer.ServerLapTime = player.ServerLapTime;
                            teamplayer.Rank = player.Rank;
                            //Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                        }
                    }
                    else if (player.GameEndType == 4)//gameover
                    {
                        player.LapTime = room.GetCurrentTime() + 240000;
                        player.ServerLapTime = player.LapTime;
                        switch (room.RuleType)
                        {
                            case 4:
                                player.Rank = 99;
                                break;
                            default:
                                player.Rank = 98;
                                break;
                        }
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                        {
                            teamplayer.LapTime = player.LapTime;
                            teamplayer.ServerLapTime = player.ServerLapTime;
                            teamplayer.Rank = player.Rank;
                            //Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                        }
                    }
                    else if (player.GameEndType == 5)//footstep
                    {
                        player.Rank = room.Rank++;
                        int i = 1;
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == player.RelayTeam && w.UserNum != player.UserNum))
                        {
                            teamplayer.LapTime = player.LapTime + i;
                            teamplayer.ServerLapTime = player.ServerLapTime + i;
                            teamplayer.Rank = room.Rank++;
                            i++;
                            //Calc_DropItem(teamplayer, room, teamplayer.Rank, last);
                        }
                    }
                    //Calc_DropItem(player, room, player.Rank, last);
                    //Thread.Sleep(100);
                }
            }
            float rankmultiply = 1;
            if (room.GameMode == 5)
                CorunModeResult(room, out rankmultiply);
            foreach (var player in room.Players.ToList().OrderBy(p => p.Rank))
            {
                Calc_DropItem(player, room, player.Rank, rankmultiply, last);
                Thread.Sleep(200);
            }
            Thread.Sleep(3000);
            room.Result = GenResult(room, last);
            room.RegisterItem(-1, -1, 2, 0x1049F00C, true);
            List<Account> playerlist = room.PlayerList();
            bool isRelayMode = room.GameMode == 3;
            foreach (Account Player in playerlist)
            {
                Player.IsReady = false;
                if (isRelayMode)
                {
                    int pos1count = room.Players.Count(p => p.RelayTeamPos == 1);
                    int pos2count = room.Players.Count(p => p.RelayTeamPos == 2);
                    byte selectpos = (byte)(pos1count <= pos2count ? 1 : 2);
                    Player.SelectRelayTeam(selectpos);
                }
            }
            try
            {
                foreach (var lvupplayer in room.LevelUPPlayerList)
                    lvupplayer.Connection.SendAsync(new GetUserEXPInfo2(1, lvupplayer.Level, lvupplayer.Exp, last));
                foreach (Account RoomPlayer in playerlist) //更新自己EXP
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameUpdateEXP(RoomPlayer, last));
                }
                foreach (Account RoomPlayer in playerlist)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameResult2(RoomPlayer, room.Result));
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex("FF0102", last));
                    foreach (Account Player in room.Players.ToList())
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosReady(Player.RoomPos, Player.IsReady, last));
                        if (isRelayMode)
                            RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosRelayTeam(Player, last));
                    }
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(room, last));
                }
                if(room.LevelUPPlayerList.Count > 0)
                {
                    foreach (Account RoomPlayer in playerlist)
                    {
                        foreach (var lvupplayer in room.LevelUPPlayerList)
                            RoomPlayer.Connection.SendAsync(new GameRoom_LevelUP(1, lvupplayer.Exp, lvupplayer.RoomPos, last));
                    }
                }
                foreach (Account RoomPlayer in playerlist)
                {
                    //RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF2D02000000000000000000000000000800000000000000000000", last)); //FF2D02A3220000C4C11400CB110000DB0800000E0C00001E0F0000
                    RoomPlayer.Connection.SendAsync(new GameRoom_UpdateIndividualGameRecord(RoomPlayer, last));
                }
                foreach (Account RoomPlayer in playerlist)
                {
                    var rewarditem = room.DropItem.FirstOrDefault(f => f.UserNum == RoomPlayer.UserNum).RewardItemID;
                    if (rewarditem.Count > 0)
                        RoomPlayer.Connection.SendAsync(new GameRoom_RewardResult(rewarditem, last));
                }
                Thread.Sleep(6000);
                foreach (Account RoomPlayer in playerlist)
                {
                    //MoveToGameRoom
                    RoomPlayer.Connection.SendAsync(new MoveToGameRoom(last)); //9704
                }

                room.GameEndSetNewRoomMaster();
                room.StartAutoChangeRoomMaster();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //reset
            room.GameEndReset();
        }

        public static void Calc_DropItem(Account User, NormalRoom room, byte rank, float rankmultiply, byte last)
        {
            bool isExists = room.DropItem.Exists(e => e.UserNum == User.UserNum);
            if (User.Attribute != 3 && !isExists)
            {
                //int PlayerCount = room.Players.FindAll(p => p.Attribute != 3).Count;
                int TR = 0;
                int EXP = 0;
                //rank = rank > 10 ? (byte)10 : rank;
                
                TR = Calc_TR(User, room, rank > 10 ? (byte)10 : rank, rankmultiply, out short BounsTR);
                EXP = Calc_EXP(User, room, rank > 10 ? (byte)10 : rank, rankmultiply, out short BounsEXP);
                int oldlevel = User.Level;
                addEXP(User.UserNum, EXP);
                addTR(User.UserNum, TR);
                User.Exp += EXP;
                User.TR += TR;
                User.GetMyLevel();
                if (User.Level != oldlevel)
                    room.LevelUPPlayerList.Add(User);
                
                GetCard(room.PlayingMapNum, (float)User.Luck, out var takecard);
                foreach (var card in takecard)
                {
                    if (card != 0)
                        giveCard(User.UserNum, card);
                }
                GetGameReward(User, room, rank, out var rewardresult);
                
                DropList dropList = new DropList
                {
                    UserNum = User.UserNum,
                    TotalEXP = User.Exp,
                    RaceDistance = User.RaceDistance,
                    ServerLapTime = User.ServerLapTime,
                    LapTime = User.LapTime,
                    Pos = User.RoomPos,
                    Team = User.Team,
                    RelayTeam = User.RelayTeam,
                    RelayTeamPos = User.RelayTeamPos,
                    BounsTR = BounsTR,
                    BounsEXP = BounsEXP,
                    TR = TR,
                    EXP = EXP,
                    Rank = User.Rank,
                    isLevelUP = User.Level != oldlevel,
                    CardID = takecard,
                    RewardItemID = rewardresult,
                    MiniGamePoint = 0,
                    MiniGameStarPoint = 0
                };
                Log.Info("Drop Item - Nickname: {0}, TR: {1}, EXP: {2}, Rank: {3}", User.NickName, TR, EXP, rank);
                room.DropItem.Add(dropList);
            }
        }

        public static void addEXP(int usernum, int exp)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_addExpByusernum";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
                    cmd.Parameters.Add("exp", MySqlDbType.Int32).Value = exp;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void addTR(int usernum, int tr)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_addmoneybyusernum";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
                    cmd.Parameters.Add("gamemoney", MySqlDbType.Int32).Value = tr;
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void giveCard(int usernum, int itemdescnum)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_alchemist_giveCard";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = usernum;
                cmd.Parameters.Add("itemdescnum", MySqlDbType.Int32).Value = itemdescnum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }

        public static int Calc_TR(Account User, NormalRoom room, byte rank, float rankmultiply, out short BounsTR)
        {
            int PlayerCount = room.PlayerCount();
            float ServerMultiplyTR = ServerSettingHolder.ServerSettings.MultiplyTR;
            int inittr = room.GameMode != 5 ? (int)Math.Round(12 * PlayerCount * (float)(1 - (((decimal)rank / 10) - (decimal)0.1)), MidpointRounding.AwayFromZero) : 12 * PlayerCount;
            int trtotal = 0;
            float trpercent = 0, trnormal = 0;
            int MaxTR = 20000;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var a in User.AllItemAttr) 
            {
                switch (a.Key)
                {
                    case 1:
                        trpercent += a.Value;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            trpercent += a.Value;
                        break;
                    case 30://個人賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 32://團體賽，接力賽 TR 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            trpercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 37:
                        trnormal += a.Value;
                        break;
                    case 51:
                        trnormal += rnd.Next(Convert.ToInt32(a.Value));
                        break;
                    case 91://簡易頻道 +N%
                        if (room.Channel == 2)
                            trpercent += a.Value;
                        break;
                    case 93://普通頻道 +N%
                        if (room.Channel == 3)
                            trpercent += a.Value;
                        break;
                    case 103://簡易頻道 +N
                        if (room.Channel == 2)
                            trnormal += a.Value;
                        break;
                    case 105://普通頻道 +N
                        if (room.Channel == 3)
                            trnormal += a.Value;
                        break;
                    case 131://個人戰頭3名以內過關時TR+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            trpercent += a.Value;
                        break;
                    default:
                        break;
                }
            }
            if (User.AvatarItems.ContainsKey(47635)) //優越童話通行証BUFF
            {
                trpercent += 1;
            }
            trtotal  = (int)Math.Round(((inittr + (inittr * trpercent) + trnormal) * rankmultiply) * ServerMultiplyTR, MidpointRounding.AwayFromZero);
            trtotal = trtotal > MaxTR ? MaxTR : trtotal;
            BounsTR = (short)(trtotal - inittr);
            return trtotal;
        }    
        public static int Calc_EXP(Account User, NormalRoom room, byte rank, float rankmultiply, out short BounsEXP)
        {
            int PlayerCount = room.PlayerCount();
            float ServerMultiplyEXP = ServerSettingHolder.ServerSettings.MultiplyEXP;
            int initexp = room.GameMode != 5 ? (int)Math.Round(12 * PlayerCount * (float)(1 - (((decimal)rank / 10) - (decimal)0.1)), MidpointRounding.AwayFromZero) : 12 * PlayerCount;
            int  exptotal = 0;
            float exppercent = 0, expnormal = 0;
            int MaxEXP = 20000;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var a in User.AllItemAttr)
            {
                switch (a.Key)
                {
                    case 2:
                        exppercent += a.Value;
                        break;
                    case 14://得到個人賽 前3名時 TR，經驗值+%
                        if (room.IsTeamPlay == 0 && rank <= 3)
                            exppercent += a.Value;
                        break;
                    case 31://個人賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay == 0 && room.GameMode != 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 33://團體賽，接力賽 經驗值 隨機獎勵 0~N%
                        if (room.IsTeamPlay != 0 || room.GameMode == 3)
                            exppercent += (float)rnd.Next(Convert.ToInt32(a.Value)) / 100;
                        break;
                    case 38:
                        expnormal += a.Value;
                        break;
                    case 52:
                        expnormal += rnd.Next(Convert.ToInt32(a.Value));
                        break;
                    case 92://簡易頻道 +N%
                        if (room.Channel == 2)
                            exppercent += a.Value;
                        break;
                    case 94://普通頻道 +N%
                        if (room.Channel == 3)
                            exppercent += a.Value;
                        break;
                    case 104://簡易頻道 +N
                        if (room.Channel == 2)
                            expnormal += a.Value;
                        break;
                    case 106://普通頻道 +N
                        if (room.Channel == 3)
                            expnormal += a.Value;
                        break;
                    case 132://個人戰頭3名以內過關時EXP+%
                        if (room.IsTeamPlay == 0 && rank <= 3 && room.GameMode != 3)
                            exppercent += a.Value;
                        break;
                    case 264://最大獲得經驗值增加 +
                        MaxEXP += (int)a.Value;
                        break;
                    default:
                        break;
                }
            }
            if (User.AvatarItems.ContainsKey(47635)) //優越童話通行証BUFF
            {
                exppercent += (float)1.5;
                MaxEXP += 10000;
            }
            exptotal = (int)Math.Round(((initexp + (initexp * exppercent) + expnormal) * rankmultiply) * ServerMultiplyEXP, MidpointRounding.AwayFromZero);
            exptotal = exptotal > MaxEXP ? MaxEXP : exptotal;
            BounsEXP = (short)(exptotal - initexp);
            return exptotal;
        }
        private static void GetCard(int mapnum, float lucky, out List<int> takecard)
        {
            takecard = new List<int>();
            if (MapCardHolder.MapCardRateInfos.TryGetValue(mapnum, out var randcard))
            {
                int GetTwoCardChance = (int)(lucky / 10.0);
                if (GetTwoCardChance >= 0)
                {
                    if (GetTwoCardChance > 30)
                        GetTwoCardChance = 30;
                }
                else
                    GetTwoCardChance = 0;
                int rand = new Random(Guid.NewGuid().GetHashCode()).Next();
                int rand2 = rand % 100 + 1;
                int getnum = 1;
                if (rand2 <= GetTwoCardChance)
                    getnum = 2;
                for (int i = 0; i < getnum; i++)
                    takecard.Add(randcard.NextWithReplacement());
            }
            else
            {
                takecard.Add(0);
            }

            /*if (MapCardHolder.MapCardRateInfos.TryGetValue(mapnum, out var cardrateinfo))
             {
                 Random rand = new Random(Guid.NewGuid().GetHashCode());
                 double r = rand.NextDouble() * MapCardHolder.GetSumWeight(cardrateinfo);
                 double min = 0, max = 0;
                 foreach (var card in cardrateinfo.OrderBy(o => o.RateKind))
                 {
                     max += MapCardHolder.GetWeight(card.RateKind);
                     if (min <= r && r < max)
                     {
                         takecard.Add(card.CardNum);
                         break;
                     }
                     min = max;
                 }
             }
             else
                 takecard.Add(0);*/
        }
        private static void GetGameReward(Account User, NormalRoom room, int rank, out List<GameRewardResult> rewardresult)
        {
            //List<GameRewardGroupRate> getitem = new List<GameRewardGroupRate>();
            string rewardGroupNumList = string.Empty;
            string rewardSubGroupList = string.Empty;
            string rewardTypeList = string.Empty;
            string rewardIDList = string.Empty;
            string rewardAmountList = string.Empty;

            Random rand = new Random(Guid.NewGuid().GetHashCode());
            foreach (var group in room.RewardGroupList)
            {
                if (GameRewardHolder.GroupRateInfos.TryGetValue(group.Key, out var grouprateinfos))
                {
                    var subgroup = grouprateinfos;
                    if (group.Value == 1) //type
                    {
                        GameRewardHolder.SubGroupInfos.TryGetValue(group.Key, out var subgroupinfos);
                        int subgroupnum = 0;
                        double r0 = rand.NextDouble() * subgroupinfos.Values.Where(w => w.SubGroupType != 100).Sum(s => s.SubGroupRate);
                        double min0 = 0, max0 = 0;
                        foreach (var igroup in subgroupinfos.Values.Where(w => w.SubGroupType != 100).OrderBy(o => o.SubGroupRate))
                        {
                            max0 += igroup.SubGroupRate;
                            if (min0 <= r0 && r0 < max0)
                            {
                                subgroupnum = igroup.SubGroup;
                                break;
                            }
                            min0 = max0;
                        }
                        subgroup = grouprateinfos.Where(w => w.SubGroup == subgroupnum).ToList();
                    }
                    if (subgroup.Count == 0)
                        continue;
                    double r = rand.NextDouble() * subgroup.Sum(s => s.Rate);
                    double min = 0, max = 0;
                    foreach (var item in subgroup.OrderBy(o => o.Rate))
                    {
                        max += item.Rate;
                        if (min <= r && r < max)
                        {
                            if (GameRewardHolder.SubGroupInfos.TryGetValue(group.Key, item.SubGroup, out var subgroupinfo))
                            {
                                if (User.RaceDistance >= room.MapMaxDistance * (subgroupinfo.RaceRate / 100))
                                {
                                    rewardGroupNumList += string.Format("{0},", item.GroupNum);
                                    rewardSubGroupList += string.Format("{0},", item.SubGroup);
                                    rewardTypeList += string.Format("{0},", item.RewardType);
                                    rewardIDList += string.Format("{0},", item.RewardID);
                                    rewardAmountList += string.Format("{0},", item.Amount);
                                }
                            }
                            break;

                        }
                        min = max;
                    }
     
                }
            }
            rewardresult = new List<GameRewardResult>();
            if (!string.IsNullOrEmpty(rewardTypeList))
            {
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(string.Empty, con))
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "usp_gameReward_GiveReward";
                        cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                        cmd.Parameters.Add("rewardGroupNumList", MySqlDbType.VarString).Value = rewardGroupNumList;
                        cmd.Parameters.Add("rewardSubGroupList", MySqlDbType.VarString).Value = rewardSubGroupList;
                        cmd.Parameters.Add("rewardTypeList", MySqlDbType.VarString).Value = rewardTypeList;
                        cmd.Parameters.Add("rewardIDList", MySqlDbType.VarString).Value = rewardIDList;
                        cmd.Parameters.Add("rewardAmountList", MySqlDbType.VarString).Value = rewardAmountList;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GameRewardResult info = new GameRewardResult
                                {
                                    RewardType = Convert.ToInt16(reader["rewardType"]),
                                    RewardID = Convert.ToInt32(reader["rewardID"]),
                                    RewardAmount = Convert.ToInt32(reader["rewardAmount"])
                                };
                                rewardresult.Add(info);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] GenResult(NormalRoom room, byte last)
        {
            PacketWriter ns = new PacketWriter();
            ns = PacketWriter.CreateInstance(16, true);
            ns.Write((byte)0xFF);
            ns.Write((short)0x381);
            ns.Write((byte)room.RoomKindID);

            bool isTeamPlay = room.IsTeamPlay != 0;
            bool isCorunMode = room.GameMode == 5;
            ns.Fill(8);
            ns.Write(isCorunMode);//9
            if (isCorunMode)
            {
                ns.Write(room.CorunModeResult, 0, room.CorunModeResult.Length);
                room.CorunModeResult = null;
            }
            ns.Write((byte)0);//A
            ns.Write(isTeamPlay);//B
            if (isTeamPlay)
            {
                int winteam = room.Players.OrderBy(p => p.Rank).ThenByDescending(p => p.RaceDistance).FirstOrDefault().Team;
                int loseteam = winteam == 1 ? 2 : 1;
                ns.Write(2);
                ns.Write(winteam);
                ns.Write(loseteam);
                ns.Write(winteam);
            }

            int playercount = room.DropItem.Count;
            ns.Write((byte)playercount); //count?
            //ns.Fill(0x6);
            ns.Write(0);
            var rankplayer = room.DropItem.OrderBy(p => p.Rank).ThenByDescending(p => p.RaceDistance).ThenBy(p => p.ServerLapTime).ThenBy(o => o.Pos);
            var lastrankplayer = rankplayer.Last();
            foreach (var p in rankplayer)
            {
                ns.Write((short)0);
                ns.Write(p.ServerLapTime);
                ns.Write(p.LapTime);
                ns.Write(p.Rank);
                ns.Write(p.Pos);
                ns.Write(p.EXP);
                ns.Write(p.MiniGameStarPoint);
                ns.Write(p.TR);
                ns.Write(0);
                ns.Write(0);
                ns.Write(0); //TR Bouns
                if (room.IsTeamPlay != 0)
                    ns.Write((int)p.Team);
                ns.Write((int)p.RelayTeamPos);
                ns.Write(p.BounsEXP);
                ns.Write(p.BounsTR);
                short bounsweight = 0;
                if (p.BounsTR > 0)
                    bounsweight += 4;
                if (p.BounsEXP > 0)
                    bounsweight += 8;
                ns.Write(bounsweight); // 0C 00 / 1C 00 bouns
                ns.Write((short)0); //20 00  02=情侶? 0x2 =通行證?
                ns.Write(playercount); //02 00 00 00 player count?
                int count = p.CardID.Count;
                ns.Write(count); //card count
                foreach (var card in p.CardID)
                {
                    ns.Write(card);
                } 
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B");
                //ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B00000000");
                if (!p.Equals(lastrankplayer))
                    ns.Write(0);
            }

            bool hasgavegoods = SendRegisterGoods(room, out byte[] getgoodsnickname, last);
            if (hasgavegoods)
            {
                Account roomMaster = room.Players.Find(p => p.RoomPos == room.RoomMasterIndex);
                roomMaster.Connection.SendAsync(new GameRoom_DeleteKeepItem(roomMaster, room, last));
            }

            //ns.Write((short)0);
            //04 02 00 00 00 03 00 00 00 00
            //03 02 00 00 00 05 03 00 00 00 04 00 00 00 00 C E D
            //01 00 00 00 02 02 00 00 00 04 03 00 00 00 03 04 00 00 00 06 05 00 00 00 01 06 00 00 00 05 00 00 00 00  B D C F A E
            if (room.GameMode == 3)
            {
                int rank = 1;//startrank?
                var relayteamrank = room.DropItem.OrderBy(o => o.Rank).Select(s => s.RelayTeam).Distinct();
                ns.Write(relayteamrank.Count());
                foreach (var relayteam in relayteamrank)
                {
                    ns.Write(rank);
                    ns.Write(relayteam);
                    rank++;
                }
            }
            else
                ns.Write(0);
            ns.Write(0);
            ns.Write(2);
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
            ns.Write(last);
            byte[] ret = ns.ToArray();
            PacketWriter.ReleaseInstance(ns);
            ns = null;
            //Console.WriteLine("Result packet: " + Utility.ByteArrayToString(ret));
            return ret;
        }
        private static void CorunModeResult(NormalRoom room, out float rankmultiply)
        {
            PacketWriter ns = new PacketWriter();
            ns = PacketWriter.CreateInstance(16, true);

            GameModeHolder.CorunModeInfos.TryGetValue(room.PlayingMapNum, out var mapresultinfos);
            bool isFinish = room.Players.Any(a => a.GameEndType == 1);
            int count = room.ClearAreaTime.Count + (isFinish ? 4 : 3);
            ns.Write(count);

            short totalpoint = 0;
            //EssenCorunModeResultPoint
            ns.Write(0); //reward type 0 = 剩下人數
            ns.Write((int)room.Survival);
            int survialpoint = room.Survival * 10;
            totalpoint += (short)survialpoint;
            ns.Write(survialpoint);//Point

            room.ClearAreaTime.TryGetValue(100, out var bosstime);
            int remaintime = isFinish ? room.StartKillBossRemainTime - bosstime : 0;
            var resulttype1 = GameModeHolder.GetResultInfo(mapresultinfos, 1, remaintime);
            ns.Write(resulttype1.ResultType); //reward type 1 = 遊戲剩下時間
            ns.Write(remaintime);//剩餘時間
            totalpoint += resulttype1.ResultPoint;
            ns.Write((byte)resulttype1.ResultPoint);//0x64 = 100 result point
            ns.Write((ushort)0xEAF0);
            ns.Write((byte)0x2E);

            int finishtime = 0;
            foreach (var i in room.ClearAreaTime)
            {
                int resulttype = i.Key != 100 ? i.Key + 10 : 2;
                /*if (i.Equals(room.ClearAreaTime.Last()) && isFinish)
                    resulttype = 2;*/
                finishtime += i.Value;
                var resulttypei = GameModeHolder.GetResultInfo(mapresultinfos, resulttype, i.Value);

                ns.Write(resulttypei.ResultType);//result type 2 = BOSS關  11-14 = 每關時間
                ns.Write(i.Value);//通關時間
                totalpoint += resulttypei.ResultPoint;
                ns.Write((byte)resulttypei.ResultPoint);//0x32 = 50 result point SS
                ns.Write((ushort)0xCBE5);
                ns.Write((byte)0xF);
            }

            if (isFinish)
            {
                ns.Write(250);//result type 0xFA/250 = 完成時間
                ns.Write(finishtime);//完成時間
                ns.Write((byte)0);
                ns.Write((ushort)0xCBE5);
                ns.Write((byte)0xF);
            }

            var resulttype3 = GameModeHolder.GetResultInfo(mapresultinfos, 3, 0);
            ns.Write(resulttype3.ResultType); //result type 3 = MapPoint
            ns.Write(room.PlayingMapNum);
            totalpoint += resulttype3.ResultPoint;
            ns.Write((byte)resulttype3.ResultPoint); //result point
            ns.Write((ushort)0xCBE4);
            ns.Write((byte)0xF);

            ns.Write(totalpoint);//Total point 
            short rank = 0;
            rankmultiply = 1;
            if (totalpoint >= 501)
            {
                rank = 5;
                rankmultiply = 3;
            }
            else if (totalpoint >= 401)
            {
                rank = 4;
                rankmultiply = 2.5f;
            }
            else if (totalpoint >= 301)
            {
                rank = 3;
                rankmultiply = 2f;
            }
            else if (totalpoint >= 201)
            {
                rank = 2;
                rankmultiply = 1.5f;
            }
            else if (totalpoint >= 151)
            {
                rank = 1;
                rankmultiply = 1f;
            }
            else if (totalpoint >= 0)
            {
                rank = 0;
                rankmultiply = 0.5f;
            }
            if (!isFinish)
            {
                rankmultiply = 0.2f;
            }
            ns.Write(rank);//rank? 5=SS  0=F

            room.CorunModeResult = ns.ToArray();

            PacketWriter.ReleaseInstance(ns);
            ns = null;
        }
        public static bool SendRegisterGoods(NormalRoom room, out byte[] nickname, byte last)
        {
            if (room.ItemNum != -1)
            {
                if (room.isOrderBy == 1)
                {
                    List<Account> playerlist = room.PlayerList();
                    foreach (Account RoomUser in playerlist)
                    {
                        if (RoomUser.Rank == room.SendRank)
                        {
                            if (RoomUser.RoomPos == room.RoomMasterIndex)
                            {
                                RoomUser.Connection.SendAsync(new GameRoom_LockKeepItem(room, true, last));
                                nickname = null;
                                return false;
                            }
                            else
                            {
                                //TODO: 派野比人
                                StorageGiveReward(RoomUser.UserNum, room.ItemNum, room.Storage_Id);
                                nickname = Encoding.GetEncoding("Big5").GetBytes(RoomUser.NickName);
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    Account RoomUser = room.Players.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                    if (RoomUser.RoomPos == room.RoomMasterIndex)
                    {
                        RoomUser.Connection.SendAsync(new GameRoom_LockKeepItem(room, true, last));
                        nickname = null;
                        return false;
                    }
                    else
                    {
                        //TODO: 派野比人
                        StorageGiveReward(RoomUser.UserNum, room.ItemNum, room.Storage_Id);
                        nickname = Encoding.GetEncoding("Big5").GetBytes(RoomUser.NickName);
                        return true;
                    }
                }
            }
            nickname = null;
            return false;
        }
        private static bool StorageGiveReward(int recvUserNum, int ItemNum, long uniqueNum)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_giveroomreward";
                cmd.Parameters.Add("recvUserNum", MySqlDbType.Int32).Value = recvUserNum;
                cmd.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
                cmd.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = uniqueNum;
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
        public static void EnterRoom(ClientConnection Client, NormalRoom room, string pw, byte last)
        {
            Account User = Client.CurrentAccount;

            #region bak
            /*if (room.isPlaying)
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 5, (byte)room.RoomKindID, last));
                return;
            }
            if (room.Channel == 1)
            {
                if (User.Exp > ServerSettingHolder.ServerSettings.NewbieOnlyChannelLimitExp && User.Attribute == 0)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 12, (byte)room.RoomKindID, last));
                    return;
                }
            }
            if (room.PlayerCount() < room.SlotCount || User.Attribute == 3)
            {
                //room.Players.Add(User);
                // User.IsRoomMaster = false;                
                if (room.Players.Exists(p => p.Attribute == 3) && User.Attribute == 3)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 9, (byte)room.RoomKindID, last));
                    return;
                }
                if (room.Password != pw && User.Attribute == 0)
                {
                    Client.SendAsync(new GameRoom_EnterRoomError(User, 7, (byte)room.RoomKindID, last));
                    return;
                }
                DateTime kickedtime;
                if (room.KickedList.TryGetValue(User, out kickedtime))
                {
                    bool cantenter = DateTime.Compare(DateTime.Now, kickedtime.AddSeconds(60)) < 0;
                    if (cantenter)
                    {
                        Client.SendAsync(new GameRoom_EnterRoomError(User, 6, (byte)room.RoomKindID, last));
                        return;
                    }
                    room.KickedList.Remove(User);
                }
                User.InGame = true;
                User.CurrentRoomId = room.ID;
                User.IsReady = false;

                //取得當前第一個位置id
                User.RoomPos = (byte)(User.Attribute == 3 ? 100 : room.PosList.FirstOrDefault());
                room.PosList.Remove(User.RoomPos);

                Client.SendAsync(new GameRoom_GoodsInfo(room, last));
                Client.SendAsync(new GameRoom_SendRoomInfo(room, last, User.RoomPos));

                bool isRelayMode = room.GameMode == 3;
                bool isTeamPlay = room.IsTeamPlay == 2;
                if (isTeamPlay)
                {
                    int redcount = room.Players.Count(p => p.Team == 1);
                    int blueteam = room.Players.Count(p => p.Team == 2);
                    User.Team = (byte)(redcount <= blueteam ? 1 : 2);
                }
                if (isRelayMode)
                {
                    int pos1count = room.Players.Count(p => p.RelayTeamPos == 1);
                    int pos2count = room.Players.Count(p => p.RelayTeamPos == 2);
                    byte selectpos = (byte)(pos1count <= pos2count ? 1 : 2);
                    User.SelectRelayTeam(selectpos);
                }
                if (User.Attribute == 3)
                {
                    room.setRoomMasterIndex(100);
                    //byte africapos = room.RoomMasterIndex;
                }

                room.Players.Add(User);

                foreach (var roomUser in room.PlayerList())
                {
                    //Send roomuser info俾自己
                    Client.SendAsync(new GameRoom_SendPlayerInfo(roomUser, last));
                    Client.SendAsync(new GameRoom_RoomPosReady(roomUser.RoomPos, roomUser.IsReady, last));
                    if (isTeamPlay)
                        Client.SendAsync(new GameRoom_RoomPosTeam(roomUser, last));
                    if (isRelayMode)
                        Client.SendAsync(new GameRoom_RoomPosRelayTeam(roomUser, last));
                    if (User.Attribute == 3)
                        roomUser.Connection.SendAsync(new GameRoom_GetRoomMaster(room.RoomMasterIndex, last));

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

                //Send自己info俾其他roomuser
                //foreach (var roomUser in room.PlayerList())
                //{
                //    if (roomUser.RoomPos != User.RoomPos)
                //    {
                //        roomUser.Connection.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                //        if (isTeamPlay)
                //        {
                //            roomUser.Connection.SendAsync(new GameRoom_RoomPosTeam(User, last));
                //        }
                //        if (isRelayMode)
                //        {
                //            roomUser.Connection.SendAsync(new GameRoom_RoomPosRelayTeam(User, last));
                //        }
                //    }
                //}

                if (User.Attribute != 3)
                {
                    byte roommasterpos = room.RoomMasterIndex;
                    Client.SendAsync(new GameRoom_GetRoomMaster(roommasterpos, last));
                }
            }
            else
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 1, (byte)room.RoomKindID, last));
            } */
            #endregion

            if (room.EnterRoomCheck(User, pw, out var Err))
            {
                User.InGame = true;
                User.CurrentRoomId = room.ID;
                User.IsReady = false;

                //取得當前第一個位置id
                User.RoomPos = (byte)(User.Attribute == 3 ? 100 : room.PosList.FirstOrDefault());
                room.PosList.Remove(User.RoomPos);

                Client.SendAsync(new GameRoom_GoodsInfo(room, last));
                Client.SendAsync(new GameRoom_SendRoomInfo(room, last, User.RoomPos));

                bool isRelayMode = room.GameMode == 3;
                bool isTeamPlay = room.IsTeamPlay == 2;
                if (isTeamPlay)
                {
                    int redcount = room.Players.Count(p => p.Team == 1);
                    int blueteam = room.Players.Count(p => p.Team == 2);
                    User.Team = (byte)(redcount <= blueteam ? 1 : 2);
                }
                if (isRelayMode)
                {
                    int pos1count = room.Players.Count(p => p.RelayTeamPos == 1);
                    int pos2count = room.Players.Count(p => p.RelayTeamPos == 2);
                    byte selectpos = (byte)(pos1count <= pos2count ? 1 : 2);
                    User.SelectRelayTeam(selectpos);
                }
                if (User.Attribute == 3)
                {
                    room.setRoomMasterIndex(100);
                }

                room.Players.Add(User);

                foreach (var roomUser in room.PlayerList())
                {
                    //Send roomuser info俾自己
                    Client.SendAsync(new GameRoom_SendPlayerInfo(roomUser, last));
                    Client.SendAsync(new GameRoom_RoomPosReady(roomUser.RoomPos, roomUser.IsReady, last));
                    if (isTeamPlay)
                        Client.SendAsync(new GameRoom_RoomPosTeam(roomUser, last));
                    if (isRelayMode)
                        Client.SendAsync(new GameRoom_RoomPosRelayTeam(roomUser, last));
                    if (User.Attribute == 3)
                        roomUser.Connection.SendAsync(new GameRoom_GetRoomMaster(room.RoomMasterIndex, last));

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
                    byte roommasterpos = room.RoomMasterIndex;
                    Client.SendAsync(new GameRoom_GetRoomMaster(roommasterpos, last));
                }
            }
            else
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, Err, (byte)room.RoomKindID, last));
            }
        }
        public static void KickPlayer(Account KickedPlayer, NormalRoom room, byte last)
        {
            byte KickedPlayerIndex = KickedPlayer.RoomPos;
            room.BroadcastToAll(new GameRoom_KickPlayer(KickedPlayerIndex, last));
            room.Players.Remove(KickedPlayer);
            room.PosList.Add(KickedPlayerIndex);
            KickedPlayer.LeaveRoomReset();
            KickedPlayer.Connection.SendAsync(new GameRoom_Hex("FF6405F703000002000000", last));
            KickedPlayer.Connection.SendAsync(new GameRoom_LeaveRoomUser_0XA9(KickedPlayer, KickedPlayerIndex, last));
            KickedPlayer.Connection.SendAsync(new GameRoom_Hex("FFA80500000000", last));
            KickedPlayer.Connection.SendAsync(new GameRoom_KickPlayer(KickedPlayerIndex, last));
            if (KickedPlayer.Attribute == 0)
            {
                room.addKickedPlayer(KickedPlayer);
            }
            if (room.Players.Count == 0)
            {
                Rooms.RemoveRoom(room.ID);
                return;
            }
            room.BroadcastToAll(new GameRoom_RemoveRoomUser(KickedPlayerIndex, last));
            room.BroadcastToAll(new GameRoom_Hex("FFA80500000000", last));
        }
        public static void LeaveRoom(ClientConnection Client, bool isDisconnect, byte last)
        {
            Account User = Client.CurrentAccount;
            if (User.CurrentRoomId != 0 && User.InGame)
            {
                byte roompos = User.RoomPos;
                //NormalRoom normalRoom = Rooms.NormalRoomList.Find(room => room.ID == User.CurrentRoomId);
                NormalRoom normalRoom = Rooms.GetRoom(User.CurrentRoomId);
                bool isRemoved = true;
                if (normalRoom.Players.Count == 1)
                {
                    if (normalRoom.ItemNum != -1 && !isDisconnect)
                    {
                        Client.SendAsync(new GameRoom_LockKeepItem(normalRoom, true, last));//解鎖之前保管了的物品
                    }
                    Rooms.RemoveRoom(User.CurrentRoomId);
                }
                else
                {
                    //加回位置id
                    normalRoom.Players.Remove(User);
                    normalRoom.PosList.Add(roompos);
                    isRemoved = false;
                    if (normalRoom.isPlaying)
                    {
                        normalRoom.DisconnectDropList(User);
                        normalRoom.Survival -= 1;
                        switch (normalRoom.RuleType)
                        {
                            case 2: //生存
                                if (normalRoom.Survival == 1 && normalRoom.GameMode != 3)
                                {
                                    var aliveplayer = normalRoom.Players.Find(f => f.GameEndType == 0 && f.Attribute != 3);
                                    normalRoom.BroadcastToAll(new GameRoom_Alive(aliveplayer.RoomPos, last));
                                    aliveplayer.GameEndType = 2;
                                    aliveplayer.LapTime = normalRoom.GetCurrentTime();
                                    aliveplayer.ServerLapTime = User.LapTime;
                                    long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                    Task.Run(() => GameRoomEvent.Execute_GameEnd(normalRoom, EndTime, last));
                                }
                                else if (normalRoom.GameMode == 3)
                                {
                                    var teamplayer = normalRoom.Players.Where(w => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum);
                                    foreach (var p in teamplayer)
                                    {
                                        normalRoom.Survival -= 1;
                                        p.GameEndType = 4;
                                        p.GameOver = true;
                                    }
                                    var survivalteam = normalRoom.Players.Where(w => w.RelayTeam != User.RelayTeam && w.GameEndType == 0);
                                    if (survivalteam.Select(s => s.RelayTeam).Distinct().Count() == 1)
                                    {
                                        foreach (var p in survivalteam)
                                        {
                                            normalRoom.BroadcastToAll(new GameRoom_Alive(p.RoomPos, last));
                                            p.GameEndType = 2;
                                            p.LapTime = normalRoom.GetCurrentTime();
                                            p.ServerLapTime = User.LapTime;
                                        }
                                        long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                        Task.Run(() => Execute_GameEnd(normalRoom, EndTime, last));
                                    }
                                }
                                break;
                            case 3: //Hardcore
                            case 4: //跑步吧
                            case 8: //衝呀!!
                            case 64://八心
                                if (normalRoom.Survival == 0)
                                {
                                    long EndTime = Utility.CurrentTimeMilliseconds() + 5000;
                                    Task.Run(() => Execute_GameEnd(normalRoom, EndTime, last));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                byte RoomMasterIndex = normalRoom.RoomMasterIndex;
                if (!isDisconnect)
                {
                    User.LeaveRoomReset();
                    Client.SendAsync(new GameRoom_LeaveRoomUser_0XA9(User, roompos, last));
                    Client.SendAsync(new GameRoom_Hex("FFA80500000000", last));
                }

                //Client.SendAsync(new GameRoom_Hex(User, "FF6405F703000002000000", last));
                if (!isRemoved)
                {
                    normalRoom.BroadcastToAll(new GameRoom_RemoveRoomUser(roompos, last));

                    if (roompos == RoomMasterIndex)
                    {
                        if (normalRoom.ItemNum != -1)
                        {
                            if (!isDisconnect)
                                Client.SendAsync(new GameRoom_LockKeepItem(normalRoom, true, last));//解鎖之前保管了的物品
                            normalRoom.RegisterItem(-1, -1, 0, 0, false);
                            normalRoom.BroadcastToAll(new GameRoom_GoodsInfo(normalRoom, last));
                        }
                        Account newRoomMaster = normalRoom.Players.Exists(p => p.Attribute == 1) ? normalRoom.Players.FindAll(p => p.Attribute == 1).FirstOrDefault() : normalRoom.Players.FirstOrDefault();
                        normalRoom.RoomMasterIndex = newRoomMaster.RoomPos;
                        normalRoom.BroadcastToAll(new GameRoom_GetRoomMaster(newRoomMaster.RoomPos, last));
                    }
                }
                else
                {
                    normalRoom.Dispose();
                }
            }
        }
    }
}
