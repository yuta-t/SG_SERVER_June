using AgentServer.Structuring.Map;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace AgentServer.Holders
{
    public static class MapHolder
    {
        public static ConcurrentDictionary<int, MapInfo> MapInfos { get; } = new ConcurrentDictionary<int, MapInfo>();

        public static void LoadMapInfo()
        {
            MapInfos.Clear();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getMapInfo";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MapInfo map = new MapInfo
                            {
                                CanTimeAttack = Convert.ToBoolean(reader["cantimeattack"]),
                                RuleType = Convert.ToInt32(reader["ruletype"]),
                                GoalInLimitTime = Convert.ToInt32(reader["goalInLimitLapTime"])
                            };
                            int mapnum = Convert.ToInt32(reader["mapnum"]);
                            MapInfos.TryAdd(mapnum, map);
                        }
                    }
                }
            }
            Log.Info("Load MapInfo Count: {0}", MapInfos.Count());
        }
    }
}
