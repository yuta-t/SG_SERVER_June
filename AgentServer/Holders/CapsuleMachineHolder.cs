using AgentServer.Structuring;
using AgentServer.Structuring.Park;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentServer.Holders
{
    public static class CapsuleMachineHolder
    {
        public static ConcurrentDictionary<int, CapsuleMachineInfo> CapsuleMachineInfos { set; get; } = new ConcurrentDictionary<int, CapsuleMachineInfo>();
        public static ConcurrentDictionary<int, List<CapsuleMachineItem>> CapsuleMachineItems { set; get; } = new ConcurrentDictionary<int, List<CapsuleMachineItem>>();
       // public static ConcurrentDictionary<int, int> CapsuleMachineKinds { set; get; } = new ConcurrentDictionary<int, int> ();


        public static void LoadCapsuleMachineInfo()
        {
            CapsuleMachineInfos.Clear();
            CapsuleMachineItems.Clear();
            int RotateMachineNum = 0;
            int RealMachineNum = 0;
            bool isRotate = false;
            //int TotalItemCount = 0;
            //short ResetCount = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineGetMachineInfo";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = 0;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    CapsuleMachineItem Item = new CapsuleMachineItem
                    {
                        ItemNum = Convert.ToInt32(reader["fdItemNum"]),
                        ItemCount = Convert.ToInt16(reader["fdItemCount"]),
                        ItemMax = Convert.ToInt16(reader["fdItemMax"]),
                        Level = Convert.ToByte(reader["fdLevel"])
                    };
                    RotateMachineNum = Convert.ToInt32(reader["fdRotateGroupNum"]);
                    RealMachineNum = Convert.ToInt32(reader["fdMachineNum"]);
                    if (RotateMachineNum == 0)
                    {
                        RotateMachineNum = RealMachineNum;
                        isRotate = false;
                    }
                    else
                        isRotate = true;

                    ItemHolder.ItemCPKInfos.TryGetValue(RealMachineNum, out Structuring.Item.ItemCPK cpk);
                    CapsuleMachineInfo MachineInfo = new CapsuleMachineInfo
                    {
                        RealMachineNum = RealMachineNum,
                        RealMachineNumKind = cpk.ItemKind,
                        isRotate = isRotate,
                        TotalItemCount = 0,
                        ResetCount = Convert.ToInt16(reader["fdResetCount"]),
                        LastResetTime = DateTime.Now
                    };

                    CapsuleMachineInfos.TryAdd(RotateMachineNum,  MachineInfo);
                    CapsuleMachineItems.AddOrUpdate(RotateMachineNum, new List<CapsuleMachineItem> { Item }, (k, v) => { v.Add(Item); return v; });
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

            foreach (var item in CapsuleMachineItems)
            {
                int TotalCount = 0;
                foreach (var items in item.Value)
                {
                    TotalCount += items.ItemMax;
                }
                CapsuleMachineInfos.TryGetValue(item.Key, out CapsuleMachineInfo oldvalue);
                CapsuleMachineInfo MachineInfo = new CapsuleMachineInfo
                {
                    RealMachineNum = oldvalue.RealMachineNum,
                    RealMachineNumKind = oldvalue.RealMachineNumKind,
                    isRotate = oldvalue.isRotate,
                    TotalItemCount = TotalCount,
                    ResetCount = oldvalue.ResetCount,
                    LastResetTime = oldvalue.LastResetTime
                };
                CapsuleMachineInfos.TryUpdate(item.Key, MachineInfo, oldvalue);
            }

            Log.Info("Load CapsuleMachineInfo Done!");
            //CapsuleMachineHolder.CapsuleMachineItems.TryGetValue(57212, out List<CapsuleMachineItem> MachineItems);
            //CapsuleMachineHolder.CapsuleMachineInfos.TryGetValue(43861, out CapsuleMachineInfo infos);
            //Console.WriteLine("DrawItem: {0}", infos.RealMachineNum);
            //Console.WriteLine("DrawItem2: {0}", infos.RealMachineNumKind);

        }

        public static string UpdateCapsuleMachineInfo(int MachineNum, int RealNachineNum, bool isRotate)
        {
            CapsuleMachineItems[MachineNum].Clear();
            CapsuleMachineInfos.TryGetValue(MachineNum, out CapsuleMachineInfo old);
            int RotateMachineNum = 0;
            int RealMachineNum = 0;
            int TotalItemCount = 0;
            short ResetCount = 0;
            Structuring.Item.ItemCPK cpk = new Structuring.Item.ItemCPK();
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineGetMachineInfo";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = RealNachineNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CapsuleMachineItem Item = new CapsuleMachineItem
                    {
                        ItemNum = Convert.ToInt32(reader["fdItemNum"]),
                        ItemCount = Convert.ToInt16(reader["fdItemCount"]),
                        ItemMax = Convert.ToInt16(reader["fdItemMax"]),
                        Level = Convert.ToByte(reader["fdLevel"])
                    };
                    RotateMachineNum = Convert.ToInt32(reader["fdRotateGroupNum"]);
                    RealMachineNum = Convert.ToInt32(reader["fdMachineNum"]);
                    if (RotateMachineNum == 0)
                    {
                        RotateMachineNum = RealMachineNum;
                        isRotate = false;
                    }
                    else
                        isRotate = true;

                    TotalItemCount += Convert.ToInt16(reader["fdItemMax"]);
                    ResetCount = Convert.ToInt16(reader["fdResetCount"]);
                    ItemHolder.ItemCPKInfos.TryGetValue(RealMachineNum, out cpk);
                    CapsuleMachineItems.AddOrUpdate(RotateMachineNum, new List<CapsuleMachineItem> { Item }, (k, v) => { v.Add(Item); return v; });
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }


            CapsuleMachineInfo MachineInfo = new CapsuleMachineInfo
            {
                RealMachineNum = RealMachineNum,
                RealMachineNumKind = cpk.ItemKind,
                isRotate = isRotate,
                TotalItemCount = TotalItemCount,
                ResetCount = ResetCount,
                LastResetTime = DateTime.Now.AddSeconds(10)
            };
            CapsuleMachineInfos.TryUpdate(RotateMachineNum, MachineInfo, old);

            return RealMachineNum.ToString();
        }

        public static int CapsuleMachineGetItem(CapsuleMachineInfo infos, List<CapsuleMachineItem> CapsuleMachineItem)
        {
            int TotalCount = infos.TotalItemCount;
            int itemnum = 0;
            foreach (var item in CapsuleMachineItem.OrderBy(o => o.ItemMax))
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                int rndnum = rnd.Next(TotalCount + 1);
                if (rndnum <= item.ItemMax)
                {
                    if(item.ItemCount <= 0)
                    {
                        itemnum = CapsuleMachineItem.Where(w => w.ItemCount > 0 && w.Level != 1).OrderBy(_ => Guid.NewGuid()).Select(s => s.ItemNum).FirstOrDefault();
                    }
                    else
                    {
                        itemnum = item.ItemNum;
                    }
                    break;
                }
                else
                {
                    TotalCount -= item.ItemMax;
                }
            }
            return itemnum;
        }

    }
}
