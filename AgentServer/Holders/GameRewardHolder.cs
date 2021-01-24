using AgentServer.Structuring.GameReward;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using NestedDictionaryLib;
using System.Linq;

namespace AgentServer.Holders
{
    public static class GameRewardHolder
    {
        public static ConcurrentDictionary<int, GameRewardGroupInfo> GroupInfos { set; get; } = new ConcurrentDictionary<int, GameRewardGroupInfo>();
        public static ConcurrentDictionary<int, List<GameRewardGroupRate>> GroupRateInfos { set; get; } = new ConcurrentDictionary<int, List<GameRewardGroupRate>>();
        public static NestedDictionary<int , int, GameRewardSubGroupInfo> SubGroupInfos { set; get; } = new NestedDictionary<int, int, GameRewardSubGroupInfo>();

        public static void LoadGameRewardInfo()
        {
            GroupInfos.Clear();
            GroupRateInfos.Clear();
            SubGroupInfos.Clear();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_gameReward_GetGroupInfo";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int Key = Convert.ToInt32(reader["groupNum"]);
                            GameRewardGroupInfo info = new GameRewardGroupInfo
                            {
                                GroupType = Convert.ToInt16(reader["groupType"]),
                                Argument = Convert.ToInt32(reader["arg"]),
                                ChildGroupNum = Convert.ToInt32(reader["childGroupNum"]),
                                SpecialRewardRate = Convert.ToSingle(reader["specialRewardRate"])
                            };
                            GroupInfos.TryAdd(Key, info);
                        }
                    }
                }
            }

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_gameReward_GetGroupRate";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int Key = Convert.ToInt32(reader["groupNum"]);
                            GameRewardGroupRate info = new GameRewardGroupRate
                            {
                                GroupNum = Key,
                                SubGroup = Convert.ToInt32(reader["subGroupNum"]),
                                Rate = Convert.ToSingle(reader["rate"]),
                                RewardType = Convert.ToInt16(reader["rewardType"]),
                                RewardID = Convert.ToInt32(reader["rewardID"]),
                                Amount = Convert.ToInt32(reader["amount"])
                            };
                            if (info.RewardID != 46235 && (info.RewardID < 27685 || info.RewardID > 27709)) //unknown item
                                GroupRateInfos.AddOrUpdate(Key, new List<GameRewardGroupRate> { info }, (k, v) => { v.Add(info); return v; });
                        }
                    }
                }
            }
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_gameReward_GetSubGroupInfo";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int Key1 = Convert.ToInt32(reader["groupNum"]);
                            int Key2 = Convert.ToInt32(reader["subGroupNum"]);
                            GameRewardSubGroupInfo info = new GameRewardSubGroupInfo
                            {
                                SubGroup = Key2,
                                SubGroupType = Convert.ToInt32(reader["subGroupType"]),
                                SubGroupRate = Convert.ToSingle(reader["subGroupRate"]),
                                StartRank = Convert.ToByte(reader["startRank"]),
                                EndRank = Convert.ToByte(reader["endRank"]),
                                RaceRate = Convert.ToInt32(reader["raceRate"]),
                                OnePlusOneRate = Convert.ToSingle(reader["onePlusOneRate"])
                            };
                            SubGroupInfos.Add(Key1, Key2, info);
                        }
                    }
                }
            }
            Log.Info("Load GameRewardInfo Done!");
            /*Random rnd = new Random();
            int rand = rnd.Next() % 1000000;
            //Log.Info("rnd {0}", rnd.Next() % 1000000);
            Dictionary<int, short> RewardGroupList = new Dictionary<int, short>();

            var type1 = GameRewardHolder.GroupInfos.FirstOrDefault(f => f.Value.GroupType == 1 && f.Value.Argument == 1);
            if (type1.Value != null)
                RewardGroupList.Add(type1.Key, type1.Value.GroupType);
            var type2 = GameRewardHolder.GroupInfos.FirstOrDefault(f => f.Value.GroupType == 2 && f.Value.Argument == 4040);
            if (type2.Value != null)
            {
                RewardGroupList.Add(type2.Key, type2.Value.GroupType);
                if (type2.Value.ChildGroupNum > 0)
                {
                    GameRewardHolder.GroupInfos.TryGetValue(type2.Value.ChildGroupNum, out var childgp);
                    RewardGroupList.Add(type2.Value.ChildGroupNum, childgp.GroupType);
                }
            }

            string rewardGroupNumList = string.Empty;
            string rewardSubGroupList = string.Empty;
            string rewardTypeList = string.Empty;
            string rewardIDList = string.Empty;
            string rewardAmountList = string.Empty;

            Random rand = new Random(Guid.NewGuid().GetHashCode());
            foreach (var group in RewardGroupList)
            {
                if (GameRewardHolder.GroupRateInfos.TryGetValue(group.Key, out var grouprateinfos))
                {
                    var subgroup = grouprateinfos;
                    if (group.Value == 1) //type
                    {
                        GameRewardHolder.SubGroupInfos.TryGetValue(group.Key, out var subgroupinfos);
                        int subgroupnum = 0;
                        double r0 = rand.NextDouble() * subgroupinfos.Values.Sum(s => s.SubGroupRate);
                        double min0 = 0, max0 = 0;
                        foreach (var igroup in subgroupinfos.Values.OrderBy(o => o.SubGroupRate))
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

                    double r = rand.NextDouble() * subgroup.Sum(s => s.Rate);
                    double min = 0, max = 0;
                    foreach (var item in subgroup.OrderBy(o => o.Rate))
                    {
                        max += item.Rate;
                        if (min <= r && r < max)
                        {
                            if (GameRewardHolder.SubGroupInfos.TryGetValue(group.Key, item.SubGroup, out var subgroupinfo))
                            {
                                //getitem.Add(item);
                                rewardGroupNumList += string.Format("{0},", item.GroupNum);
                                rewardSubGroupList += string.Format("{0},", item.SubGroup);
                                rewardTypeList += string.Format("{0},", item.RewardType);
                                rewardIDList += string.Format("{0},", item.RewardID);
                                rewardAmountList += string.Format("{0},", item.Amount);
                            }
                            break;

                        }
                        min = max;
                    }

                }
            }

            Log.Info("rewardIDList {0}", rewardIDList);
            */
        }
    }
}
