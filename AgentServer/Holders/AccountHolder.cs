using AgentServer.Structuring;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Holders
{
    public static class AccountHolder
    {
        public static HashSet<long> LevelInfo { get; } = new HashSet<long>();

        public static void LoadLevelInfo()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM essenlevelinfo WHERE fdLevelKind = 1", con))
                {
                    cmd.Parameters.Clear();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LevelInfo.Add(Convert.ToInt64(reader["fdExp"]));
                        }
                    }
                }
            }
            Log.Info("Load Levels Count: {0}", LevelInfo.Count());
        }
    }
}
