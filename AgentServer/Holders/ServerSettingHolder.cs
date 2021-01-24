using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Structuring.Map;
using LocalCommons.Logging;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace AgentServer.Holders
{
    public static class ServerSettingHolder
    {
        public static List<SettingInfo> ServerSettingList { get; } = new List<SettingInfo>();
        public static ServerSetting ServerSettings { get; private set; }

        public static void LoadServerSettingInfo()
        {
            ServerSettingList.Clear();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand("SELECT * FROM tblserversettinginfo", con))
                {
                    cmd.Parameters.Clear();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SettingInfo info = new SettingInfo
                            {
                                Key = reader["fdKey"].ToString().Replace(" ",""),
                                Value = reader["fdValue"].ToString(),
                                OnlyServerSetting = Convert.ToBoolean(reader["fdOnlyServerSetting"])
                            };
                            ServerSettingList.Add(info);
                        }
                    }
                }
            }

            PacketWriter ns = new PacketWriter();
            ns = PacketWriter.CreateInstance(16, true);
            ns.Write((byte)0x15);
            ns.Write(ServerSettingList.Count(c => !c.OnlyServerSetting));
            foreach(var i in ServerSettingList.Where(w => !w.OnlyServerSetting))
            {
                ns.WriteBIG5Fixed_intSize(i.Key);
                ns.WriteBIG5Fixed_intSize(i.Value);
            }
            ns.Write((byte)0x1);
            DBInit.GameServerSetting = ns.ToArray();
            PacketWriter.ReleaseInstance(ns);
            ns = null;
            LoadDBInfo();
            Log.Info("Load ServerSetting Count: {0}", ServerSettingList.Count());
        }
        private static void LoadDBInfo()
        {
            ServerSettings = null;
            ServerSettings = new ServerSetting();
            foreach (var i in ServerSettingList)
            {
                switch (i.Key)
                {
                    case "MultiplyTR":
                        ServerSettings.MultiplyTR = Convert.ToSingle(i.Value);
                        break;
                    case "MultiplyEXP":
                        ServerSettings.MultiplyEXP = Convert.ToSingle(i.Value);
                        break;
                    case "SurvivalMaxUserNum":
                        ServerSettings.SurvivalMaxUserNum = Convert.ToByte(i.Value);
                        break;
                    case "SurvivalMinUserNum":
                        ServerSettings.SurvivalMinUserNum = Convert.ToByte(i.Value);
                        break;
                    case "NewbieOnlyChannelLimitExp":
                        ServerSettings.NewbieOnlyChannelLimitExp = Convert.ToInt64(i.Value);
                        break;
                    case "GateNoticeURL":
                        ServerSettings.GateNoticeURL = i.Value;
                        break;
                    case "QuitConfirmDialogURL":
                        ServerSettings.QuitConfirmDialogURL = i.Value;
                        break;
                    case "cashFillUpURL":
                        ServerSettings.cashFillUpURL = i.Value;
                        break;
                    case "EveryDayEventURL":
                        ServerSettings.EveryDayEventURL = i.Value;
                        break;
                    case "LoadingTimeOutMilliSeconds":
                        ServerSettings.LoadingTimeOutMilliSeconds = Convert.ToInt32(i.Value);
                        break;
                    case "RABBIT_TURTLE_FATIGUE_DEC":
                        ServerSettings.RABBIT_TURTLE_FATIGUE_DEC = Convert.ToSingle(i.Value);
                        break;
                    case "RABBIT_TURTLE_FATIGUE_INC":
                        ServerSettings.RABBIT_TURTLE_FATIGUE_INC = Convert.ToSingle(i.Value);
                        break;
                    case "RABBIT_TURTLE_ITEM_FATIGUE_DEC":
                        ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_DEC = Convert.ToSingle(i.Value);
                        break;
                    case "RABBIT_TURTLE_ITEM_FATIGUE_INC":
                        ServerSettings.RABBIT_TURTLE_ITEM_FATIGUE_INC = Convert.ToSingle(i.Value);
                        break;
                    case "corunModeMinPlayerNum":
                        ServerSettings.corunModeMinPlayerNum = Convert.ToByte(i.Value);
                        break;
                    case "corunModeDecreaseEnergyRatio":
                        ServerSettings.corunModeDecreaseEnergyRatio = Convert.ToInt32(i.Value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
