using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using AgentServer.Database;
using LocalCommons.Utilities;
using AgentServer.Structuring.Park;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AgentServer.Structuring.Item;

namespace AgentServer.Packet
{
    public class ExchangeHandle
    {
        public static void Handle_GetExchangeSystemInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF BD 04 2D 00 00 00 01
            Account User = Client.CurrentAccount;
            int SystemNum = reader.ReadLEInt32();
            if (ItemHolder.ExchangeSystemInfo.TryGetValue(SystemNum, out var canuse) && canuse)
            {
                Client.SendAsync(new GetExchangeSystemOK(User, SystemNum, last));
            }
        }

        public static void Handle_ExchangeItem(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF BF 04 6D 00 00 00 70 0E 00 00 08
            Account User = Client.CurrentAccount;
            int SystemNum = reader.ReadLEInt32();
            int ExchangeID = reader.ReadLEInt32();
            if (ItemHolder.ExchangeSystemInfo.TryGetValue(SystemNum, out var canuse) && canuse)
            {
                if (ExchangeItem(User.UserNum, ExchangeID, out var exinfo))
                {
                    Client.SendAsync(new ExchangeItem(User, exinfo, last));
                }
            }
        }

        private static bool ExchangeItem(int Usernum, int ExchangeID, out List<ExchangeItemInfo> exinfo)
        {
            exinfo = new List<ExchangeItemInfo>();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_exchangeSystem_Exchange";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = Usernum;
                    cmd.Parameters.Add("exchangeID", MySqlDbType.Int32).Value = ExchangeID;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ExchangeItemInfo exi = new ExchangeItemInfo
                                {
                                    type = Convert.ToInt32(reader["type"]),
                                    id = Convert.ToInt32(reader["id"]),
                                    count = Convert.ToInt32(reader["count"])
                                };
                                exinfo.Add(exi);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

    }
}
