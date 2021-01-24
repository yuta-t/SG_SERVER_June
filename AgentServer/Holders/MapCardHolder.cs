using AgentServer.Packet;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
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
    public static class MapCardHolder
    {
        //public static ConcurrentDictionary<int, List<MapCardRateInfo>> MapCardRateInfos { get; } = new ConcurrentDictionary<int, List<MapCardRateInfo>>();
        public static ConcurrentDictionary<int, IWeightedRandomizer<int>> MapCardRateInfos { get; } = new ConcurrentDictionary<int, IWeightedRandomizer<int>>();

        /*public static void LoadMapCardInfo()
        {
            string fileName = @"iteminfo\\tblalchemist_mapcardinfo.txt";
            var lines = File.ReadLines(fileName);
            int Count = 0;
            foreach (var line in lines)
            {
                Count++;
                string[] mapcard = line.Split(',');
                MapCardInfo mapCardInfo = new MapCardInfo
                {
                    MapNum = Convert.ToInt32(mapcard[1]),
                    CardNum = Convert.ToInt32(mapcard[2])
                };
                MapCardInfos.Add(Count, mapCardInfo);
            }
            Log.Info("Load MapCardInfo Count: {0}", MapCardInfos.Count());
        }*/
        public static void LoadMapCardRateInfo()
        {
            ConcurrentDictionary<int, List<MapCardRateInfo>> _mapCardRateInfos = new ConcurrentDictionary<int, List<MapCardRateInfo>>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_alchemist_getCardRateKindMapInfo";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MapCardRateInfo mapCardInfo = new MapCardRateInfo
                            {
                                CardNum = Convert.ToInt32(reader["carditemdesc"]),
                                RateKind = Convert.ToInt32(reader["getratekind"])
                            };
                            _mapCardRateInfos.AddOrUpdate(Convert.ToInt32(reader["mapnum"]), new List<MapCardRateInfo> { mapCardInfo }, (k, v) => { v.Add(mapCardInfo); return v; });
                        }
                    }
                }
            }
            //Log.Info("Load MapCardRateInfo Count: {0}", _mapCardRateInfos.Count());

            foreach (var i in _mapCardRateInfos)
            {
                IWeightedRandomizer<int> randomizer = new StaticWeightedRandomizer<int>();
                foreach (var j in i.Value)
                {
                    int rate = GetWeight(j.RateKind);
                    randomizer.Add(j.CardNum, rate);
                }
                MapCardRateInfos.TryAdd(i.Key, randomizer);
            }
            Log.Info("Load MapCardRateInfo Count: {0}", MapCardRateInfos.Count());
        }

        public static int GetWeight(int ratekind)
        {
            int weight = 0;
            switch (ratekind)
            {
                case 1:
                    weight = 50;
                    break;
                case 2:
                    weight = 40;
                    break;
                case 3:
                    weight = 30;
                    break;
                case 4:
                    weight = 20;
                    break;
                case 5:
                    weight = 10;
                    break;
            }
            return weight;
        }
        /*
        public static int GetSumWeight(List<MapCardRateInfo> rateinfo)
        {
            int weight = 0;
            foreach (var i in rateinfo)
            {
                switch (i.RateKind)
                {
                    case 1:
                        weight += 50;
                        break;
                    case 2:
                        weight += 40;
                        break;
                    case 3:
                        weight += 30;
                        break;
                    case 4:
                        weight += 20;
                        break;
                    case 5:
                        weight += 10;
                        break;
                }
            }
            return weight;
        }
        */
    }
}
