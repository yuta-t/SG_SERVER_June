using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Map;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Weighted_Randomizer;

namespace AgentServer.Holders
{
    public static class MapItemHolder
    {
        public static ConcurrentDictionary<int, IWeightedRandomizer<MapCapsuleItemInfo>> MapCapsuleItems { get; } = new ConcurrentDictionary<int, IWeightedRandomizer<MapCapsuleItemInfo>>();

        public static void LoadMapCapsuleItemInfo()
        {
            ConcurrentDictionary<int, List<MapCapsuleItemInfo>> dbmapiteminfos = new ConcurrentDictionary<int, List<MapCapsuleItemInfo>>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_loadCapsuleItemInfo";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MapCapsuleItemInfo item = new MapCapsuleItemInfo
                            {
                                PresentRuleType = Convert.ToInt32(reader["fdPresentRuleType"]),
                                Argument = Convert.ToInt32(reader["fdArgument"]),
                                GameItemNum = Convert.ToInt32(reader["fdGameItemNum"]),
                                Rate = Convert.ToInt32(reader["fdRate"])
                            };
                            int GroupNum = Convert.ToInt32(reader["fdGroupNum"]);
                            dbmapiteminfos.AddOrUpdate(GroupNum, new List<MapCapsuleItemInfo> { item }, (k, v) => { v.Add(item); return v; });
                        }
                    }
                }
            }
            //Log.Info("Load dbmapiteminfos Count: {0}", dbmapiteminfos.Count());
            foreach (var i in dbmapiteminfos)
            {
                IWeightedRandomizer<MapCapsuleItemInfo> randomizer = new StaticWeightedRandomizer<MapCapsuleItemInfo>();
                foreach (var j in i.Value)
                {
                    randomizer.Add(j, j.Rate);
                }
                MapCapsuleItems.TryAdd(i.Key, randomizer);
                //Log.Info("i.Key: {0}", i.Key);
            }
            Log.Info("Load MapCapsuleItems Count: {0}", MapCapsuleItems.Count());
        }

    }
}
