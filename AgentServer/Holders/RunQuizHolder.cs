using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Holders
{
    public static class RunQuizHolder
    {
        public static Dictionary<int, string> RunQuizInfo = new Dictionary<int, string>();
        public static void LoadRunQuizInfo()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand("select * from essenrunquizmodedesc;", con))
                {
                    cmd.Parameters.Clear();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            RunQuizInfo.Add(Convert.ToInt32(reader["fdRunQuizNum"]), Convert.ToString(reader["fdName"]));
                        }
                    }
                }
            }
            Log.Info("Load RunQuizInfo Count: {0}", RunQuizInfo.Count());
        }

    }
}
