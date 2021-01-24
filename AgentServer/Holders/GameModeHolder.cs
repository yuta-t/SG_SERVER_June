using AgentServer.Structuring;
using AgentServer.Structuring.Map;
using AgentServer.Structuring.Room;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace AgentServer.Holders
{
    public static class GameModeHolder
    {
        public static ConcurrentDictionary<int, ConcurrentDictionary<int, List<CorunModeResult>>> CorunModeInfos { get; } = new ConcurrentDictionary<int, ConcurrentDictionary<int, List<CorunModeResult>>>();

        public static void LoadCorunModeResultInfo()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getCorunModeResultPoint";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        ConcurrentDictionary<int, List<CorunModeResult>> mapinfo = new ConcurrentDictionary<int, List<CorunModeResult>>();
                        mapinfo.Clear();
                        int CurMapNum = 0;
                        int start = 0;
                        while (reader.Read())
                        {
                            int BeforeMapNum = CurMapNum;
                            CurMapNum = Convert.ToInt32(reader["fdMapNum"]);
                            int ResultType = Convert.ToInt32(reader["fdResultType"]);
                            if (BeforeMapNum != CurMapNum && BeforeMapNum != 0)
                            {
                                var nd0 = new ConcurrentDictionary<int, List<CorunModeResult>>(mapinfo);
                                CorunModeInfos.TryAdd(BeforeMapNum, nd0);
                                mapinfo.Clear();
                            }
                            CorunModeResult resultinfo = new CorunModeResult
                            {
                                ResultType = Convert.ToInt32(reader["fdResultType"]),
                                ResultPoint = Convert.ToByte(reader["fdResultPoint"]),
                                TimeFrom = Convert.ToInt32(reader["fdTimeFrom"]) * 1000,
                                TimeTo = Convert.ToInt32(reader["fdTimeTo"]) * 1000
                            };
                            mapinfo.AddOrUpdate(ResultType, new List<CorunModeResult> { resultinfo }, (k, v) => { v.Add(resultinfo); return v; });
                        }
                        //Log.Info("CurMapNum: {0}", CurMapNum);
                        var nd = new ConcurrentDictionary<int, List<CorunModeResult>>(mapinfo);
                        CorunModeInfos.TryAdd(CurMapNum, nd);
                        mapinfo.Clear();
                    }
                }
            }
            foreach (var i in CorunModeInfos)
            {
                foreach (var j in i.Value)
                {
                    foreach (var k in j.Value)
                    {
                        if (j.Key == 1) //redult type = 1
                        {
                            if (k.Equals(j.Value.Last()))
                                k.TimeTo = 999999;
                        }
                        else if (j.Key != 1 && j.Key != 3)
                        {
                            if (k.Equals(j.Value.First()))
                                k.TimeTo = 999999;
                        }

                    }
                }
            }
            Log.Info("Load CorunModeResultInfo Count: {0}", CorunModeInfos.Count());
        }

        public static CorunModeResult GetResultInfo(ConcurrentDictionary<int, List<CorunModeResult>> mapresultinfos, int resulttype, int time)
        {
            mapresultinfos.TryGetValue(resulttype, out var rewardtype);

            if (resulttype == 3)
                return rewardtype.FirstOrDefault();
            else
                return rewardtype.Where(w => w.TimeFrom <= time && time < w.TimeTo).FirstOrDefault();
        }
    }
}
