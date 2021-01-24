using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System.Data;
using AgentServer.Structuring.Item;

namespace AgentServer.Structuring.Map
{
    public static class Map
    {
        public static List<SubTeleport> SubTeleportList { get; } = new List<SubTeleport>();
        public static void LoadTeleportInfo()
        {
            CreateSubTeleport();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getMapTeleport";

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                TeleportRecord teleport = new TeleportRecord
                                {
                                    MapGlobalID = Convert.ToInt32(reader["fdMapGlobalID"]),
                                    MapX = Convert.ToInt32(reader["fdMapX"]),
                                    MapY = Convert.ToInt32(reader["fdMapY"]),
                                    MapTeleportID = Convert.ToInt32(reader["fdMapTeleportID"]),
                                    MapInitX = Convert.ToInt32(reader["fdMapInitX"]),
                                    MapInitY = Convert.ToInt32(reader["fdMapInitY"]),
                                };
                                SubTeleportList.Find(t => t.MapGlobalID == Convert.ToInt32(reader["fdMapGlobalID"])).SubList.Add(teleport);
                            }
                        }
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                    }
                }
            }
        }
        public static void CreateSubTeleport()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getSubTeleportID";

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                SubTeleport teleport = new SubTeleport
                                {
                                    MapGlobalID = Convert.ToInt32(reader["fdMapGlobalID"]),
                                };
                                SubTeleportList.Add(teleport);
                            }
                        }
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                    }
                }
            }
        }
    }
}

