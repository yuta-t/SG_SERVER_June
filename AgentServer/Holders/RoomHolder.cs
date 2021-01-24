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

namespace AgentServer.Holders
{
    public static class RoomHolder
    {
        public static ConcurrentDictionary<int, RoomKindInfo> RoomKindInfos { get; } = new ConcurrentDictionary<int, RoomKindInfo>();

        public static void LoadRoomKindInfo()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getRoomKindID";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RoomKindInfo roomkindinfo = new RoomKindInfo
                            {
                                GameMode = Convert.ToInt32(reader["GameMode"]),
                                Channel = Convert.ToInt32(reader["Channel"])
                            };
                            RoomKindInfos.TryAdd(Convert.ToInt32(reader["RoomKindID"]), roomkindinfo);
                        }
                    }
                }
            }
            Log.Info("Load RoomKindInfo Count: {0}", RoomKindInfos.Count());
        }
    }
}
