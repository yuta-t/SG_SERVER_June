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

namespace AgentServer.Structuring.Game
{
    public static class Trade
    {
        public static List<TradeRecord> TradeList { get; } = new List<TradeRecord>();
        public static int tradeID;

        public static void LoadTradeInfo()
        {
            int pTradeID;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                // 接続後に文字セットを明示的に設定 こうしないとVSで起動しない
                using (var cmd = new MySqlCommand("SET NAMES utf8mb4", con))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getTradeID";

                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    reader.Read();
                    pTradeID = Convert.ToInt32(reader["tradeid"]);
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }
            }
            tradeID = pTradeID;
        }
        public static int GetTradeID()
        {
            tradeID++;
            return tradeID;
        }
        public static int CompleteTrade(TradeRecord tradeRecord)
        {
            try
            {
                //tradeRecord.tradePlayer.Connection.SendAsync(new LoginRemoveMultiItem(tradeRecord.tradePlayer, tradeRecord.TradeItem));
                //tradeRecord.tradedPlayer.Connection.SendAsync(new LoginRemoveMultiItem(tradeRecord.tradedPlayer, tradeRecord.TradedItem));
                
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_tradeComplete";
                    cmd.Parameters.Add("tradeid", MySqlDbType.Int32).Value = tradeRecord.tradeID;
                    /*MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                    reader.Read();
                    string result = reader["result"].ToString();
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                    if (result == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }*/
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int ItemGlobalID = Convert.ToInt32(reader["fdGlobalItemID"]);
                                if(ItemGlobalID == -1)
                                {
                                    return 1;
                                }
                                int ItemOldID = Convert.ToInt32(reader["fdOldItemID"]);
                                int GlobalID = Convert.ToInt32(reader["fdGlobalID"]);
                                int ItemPos = Convert.ToInt32(reader["fdItemPos"]);

                                if (tradeRecord.tradePlayer.GlobalID == GlobalID)
                                {
                                    ItemAttr oldItem = tradeRecord.TradedItem.Find(i => i.ItemGlobalID == ItemOldID);
                                    ItemAttr newItem = (ItemAttr)oldItem.Clone();
                                    newItem.ItemGlobalID = ItemGlobalID;
                                    newItem.ItemPos = ItemPos - 1;
                                    tradeRecord.TradeInsertedItem.Add(newItem);
                                }
                                else if (tradeRecord.tradedPlayer.GlobalID == GlobalID)
                                {
                                    ItemAttr oldItem = tradeRecord.TradeItem.Find(i => i.ItemGlobalID == ItemOldID);
                                    ItemAttr newItem = (ItemAttr)oldItem.Clone();
                                    newItem.ItemGlobalID = ItemGlobalID;
                                    newItem.ItemPos = ItemPos - 1;
                                    tradeRecord.TradedInsertedItem.Add(newItem);
                                }
                            }
                        }
                        //string result = reader["result"].ToString();
                        cmd.Dispose();
                        reader.Close();
                        con.Close();
                
                        /*if(result == "1")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }*/
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 2;
            }
        }
        public static bool AddTrade(TradeRecord tradeRecord)
        {
            tradeRecord.tradeID = GetTradeID();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_addTradeTemplate";
                    cmd.Parameters.Add("tradeid", MySqlDbType.Int32).Value = tradeRecord.tradeID;
                    cmd.Parameters.Add("tradePlayerID", MySqlDbType.Int32).Value = tradeRecord.tradePlayer.GlobalID;
                    cmd.Parameters.Add("tradedPlayerID", MySqlDbType.Int32).Value = tradeRecord.tradedPlayer.GlobalID;

                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    reader.Read();
                    string result = reader["result"].ToString();
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                    if (result == "1")
                    {
                        TradeList.Add(tradeRecord);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public static bool UpdateTrade(TradeRecord trade, int updatePlayer, int tradelock)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_updateTrade";
                    cmd.Parameters.Add("tradeid", MySqlDbType.Int32).Value = trade.tradeID;
                    if (updatePlayer == 0)
                    {
                        cmd.Parameters.Add("tradePlayer", MySqlDbType.Int32).Value = trade.tradePlayer.GlobalID;
                        for(int i = 0; i < trade.TradeItem.Count; i++)
                        {
                            cmd.Parameters.Add("tradeItem" + (i + 1).ToString(), MySqlDbType.Int32).Value = trade.TradeItem[i].ItemGlobalID;
                        }
                        for (int i = trade.TradeItem.Count; i < 12; i++)
                        {
                            cmd.Parameters.Add("tradeItem" + (i + 1).ToString(), MySqlDbType.Int32).Value = 0;
                        }
                        cmd.Parameters.Add("tradeZula", MySqlDbType.Int32).Value = trade.tradeZula;
                    }
                    else
                    {
                        cmd.Parameters.Add("tradePlayer", MySqlDbType.Int32).Value = trade.tradedPlayer.GlobalID;
                        for (int i = 0; i < trade.TradedItem.Count; i++)
                        {
                            cmd.Parameters.Add("tradeItem" + (i + 1).ToString(), MySqlDbType.Int32).Value = trade.TradedItem[i].ItemGlobalID;
                        }
                        for (int i = trade.TradedItem.Count; i < 12; i++)
                        {
                            cmd.Parameters.Add("tradeItem" + (i + 1).ToString(), MySqlDbType.Int32).Value = 0;
                        }
                        cmd.Parameters.Add("tradeZula", MySqlDbType.Int32).Value = trade.tradedZula;
                    }
                    cmd.Parameters.Add("tradelock", MySqlDbType.Int32).Value = tradelock;

                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    reader.Read();
                    string result = reader["result"].ToString();
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                    if (result == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public static bool UpdateLock(TradeRecord trade, int updatePlayer, int tradelock)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_updateTradeLock";
                    cmd.Parameters.Add("tradeid", MySqlDbType.Int32).Value = trade.tradeID;
                    if (updatePlayer == 0)
                    {
                        cmd.Parameters.Add("tradePlayer", MySqlDbType.Int32).Value = trade.tradePlayer.GlobalID;
                    }
                    else
                    {
                        cmd.Parameters.Add("tradePlayer", MySqlDbType.Int32).Value = trade.tradedPlayer.GlobalID;
                    }
                    cmd.Parameters.Add("tradelock", MySqlDbType.Int32).Value = tradelock;

                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    reader.Read();
                    string result = reader["result"].ToString();
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                    if (result == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public static void RemoveTrade(TradeRecord tradeRecord)
        {
            TradeList.Remove(tradeRecord);
        }
    }
}
