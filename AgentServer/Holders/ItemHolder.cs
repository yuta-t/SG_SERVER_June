using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Shu;
using LocalCommons.Logging;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentServer.Holders
{
    public static class ItemHolder
    {
        public static ConcurrentDictionary<int, ItemCPK> ItemCPKInfos { get; } = new ConcurrentDictionary<int, ItemCPK>();
        public static ConcurrentDictionary<int, List<ItemAttr>> ItemAttrCollections { get; } = new ConcurrentDictionary<int, List<ItemAttr>>();
        public static ConcurrentDictionary<int, ItemShopInfo> ItemShopInfos { get; } = new ConcurrentDictionary<int, ItemShopInfo>();
        public static ConcurrentDictionary<int, int> PetMaxEXP { get; } = new ConcurrentDictionary<int, int>();
        public static ConcurrentDictionary<int, bool> ExchangeSystemInfo { get; } = new ConcurrentDictionary<int, bool>();
        public static ConcurrentDictionary<int, List<int>> ItemSetDesc { get; } = new ConcurrentDictionary<int, List<int>>();
        public static ConcurrentDictionary<int, List<ItemSetAttr>> ItemSetAttr { get; } = new ConcurrentDictionary<int, List<ItemSetAttr>>();
        public static NestedDictionary<ushort, byte, ushort, int> ItemPCKDict = new NestedDictionary<ushort, byte, ushort, int>();

        public static ConcurrentDictionary<int, ShuItemCPK> ShuItemCPKInfos { get; } = new ConcurrentDictionary<int, ShuItemCPK>();

        public static void LoadItemInfo()
        {
            string fileName = @"iteminfo\\tblavataritemdesc.txt";
            var lines = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            //int Count = 0;
            foreach (var line in lines)
            {
                //Count++;
                //Console.WriteLine(line);
                string[] iteminfo = line.Split(',');
                int itemid = Convert.ToInt32(iteminfo[0]);
                byte ItemChar = Convert.ToByte(iteminfo[3]);
                ushort ItemPosition = Convert.ToUInt16(iteminfo[4]);
                ushort ItemKind = Convert.ToUInt16(iteminfo[5]);
                if (iteminfo[2] == "1" || iteminfo[2] == "2" || iteminfo[2] == "4")
                {
                    ItemCPK ItemInfo = new ItemCPK
                    {
                        ItemChar = ItemChar,
                        ItemPosition = ItemPosition,
                        ItemKind = ItemKind
                    };
                    ItemCPKInfos.TryAdd(itemid, ItemInfo);
                    ItemPCKDict[ItemPosition][ItemChar][ItemKind] = itemid;
                }
                //shopinfo
                bool canbuy = Convert.ToBoolean(iteminfo[21]); //fdPurchasable
                ItemShopInfo ItemShop = new ItemShopInfo
                {
                    CanBuy = canbuy,
                    ItemPosition = Convert.ToUInt16(iteminfo[4]),
                    NotDeleteWhenExpired = Convert.ToBoolean(iteminfo[16])
                };
                ItemShopInfos.TryAdd(itemid, ItemShop);
            }
            Log.Info("Load ItemCPKInfo Count: {0}", ItemCPKInfos.Count);
            Log.Info("Load ItemShopInfos Count: {0}", ItemShopInfos.Count);
            Log.Info("Load ItemPCKDict Count: {0}", ItemPCKDict.Count);
            //Console.WriteLine(ItemCPKInfos.FirstOrDefault(i => (i.Value.ItemChar == 1 || i.Value.ItemChar == 0) && i.Value.ItemPosition == 6 && i.Value.ItemKind == 0).Key);
            LoadShuItemCPKInfo();
        }
        public static void LoadItemAttrInfo()
        {
            string fileName = @"iteminfo\\tblavataritemdescattr.txt";
            var lines = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            foreach (var line in lines)
            {

                string[] iteminfo = line.Split(',');
                ItemAttr ItemAttr = new ItemAttr
                {
                    Attr = Convert.ToUInt16(iteminfo[2]),
                    AttrValue = Convert.ToSingle(iteminfo[3])
                };
                int itemid = Convert.ToInt32(iteminfo[1]);

                ItemAttrCollections.AddOrUpdate(itemid, new List<ItemAttr> { ItemAttr }, (k, v) => { v.Add(ItemAttr); return v; });
            }

            Log.Info("Load ItemAttrInfo Count: {0}", ItemAttrCollections.Count());
            /*ItemAttrCollections.TryGetValue(61819, out List<ItemAttr> value);
            Console.WriteLine(value[3].AttrValue);*/
        }

        public static void LoadPetExpInfo()
        {
            string fileName = @"iteminfo\\essenmaxexpfrompetlevel.txt";
            var lines = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            foreach (var line in lines)
            {
                string[] iteminfo = line.Split(',');
                PetMaxEXP.TryAdd(Convert.ToInt32(iteminfo[0]), Convert.ToInt32(iteminfo[1]));
            }

            Log.Info("Load PetExpInfo Count: {0}", PetMaxEXP.Count());
            /*ItemAttrCollections.TryGetValue(61819, out List<ItemAttr> value);
            Console.WriteLine(value[3].AttrValue);*/
        }

        public static void LoadExchangeSystemInfo()
        {
            string fileName = @"iteminfo\\essenexchangesystemnameinfo.txt";
            var lines = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            foreach (var line in lines)
            {
                string[] iteminfo = line.Split(',');
                ExchangeSystemInfo.TryAdd(Convert.ToInt32(iteminfo[0]), Convert.ToBoolean(iteminfo[2]));
            }

            Log.Info("Load ExchangeSystemInfo Count: {0}", ExchangeSystemInfo.Count());
        }

        public static void LoadItemSetInfo()
        {
            string fileName = @"iteminfo\\tblavataritemsetdesc.txt";
            var lines = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            foreach (var line in lines)
            {
                string[] iteminfo = line.Split(',');
                string active = iteminfo[2];
                if (active == "1")
                {
                    int groupid = Convert.ToInt32(iteminfo[0]);
                    int memberid = Convert.ToInt32(iteminfo[1]);
                    ItemSetDesc.AddOrUpdate(groupid, new List<int> { memberid }, (k, v) => { v.Add(memberid); return v; });
                }
            }
            Log.Info("Load ItemSetDesc Count: {0}", ItemSetDesc.Count());

            fileName = @"iteminfo\\tblavataritemsetattr.txt";
            var lines2 = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            foreach (var line in lines2)
            {
                string[] iteminfo = line.Split(',');
                int groupid = Convert.ToInt32(iteminfo[1]);
                ItemSetAttr ItemAttr = new ItemSetAttr
                {
                    Attr = Convert.ToUInt16(iteminfo[3]),
                    AttrValue = Convert.ToSingle(iteminfo[4]),
                    ApplyTarget = Convert.ToByte(iteminfo[5])
                };
                ItemSetAttr.AddOrUpdate(groupid, new List<ItemSetAttr> { ItemAttr }, (k, v) => { v.Add(ItemAttr); return v; });
            }
            Log.Info("Load ItemSetAttr Count: {0}", ItemSetAttr.Count());
        }

        public static void LoadShuItemCPKInfo()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_shu_getItemInfo";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ShuItemCPK item = new ShuItemCPK
                            {
                                character = Convert.ToInt16(reader["character"]),
                                position = Convert.ToInt32(reader["position"]),
                                kind = Convert.ToInt16(reader["kind"])
                            };
                            int itemnum = Convert.ToInt32(reader["avatarItemNum"]);
                            ShuItemCPKInfos.TryAdd(itemnum, item);
                        }
                    }
                }
            }
            Log.Info("Load ShuItemCPKInfo Count: {0}", ShuItemCPKInfos.Count());

        }

        /*public static void LoadItemShopInfo()
        {
            ItemShopInfos.Clear();
            string fileName = @"iteminfo\\tblavataritemdesc.txt";
            var lines = File.ReadLines(fileName, Encoding.GetEncoding(1200));
            foreach (var line in lines)
            {

                string[] iteminfo = line.Split(',');
                int itemid = Convert.ToInt32(iteminfo[0]);
                bool canbuy = Convert.ToBoolean(iteminfo[21]); //fdPurchasable
                ItemShopInfo ItemShop = new ItemShopInfo
                {
                   CanBuy = canbuy,
                   ItemPosition = Convert.ToUInt16(iteminfo[4])
                };

                ItemShopInfos.TryAdd(itemid, ItemShop);
            }

            Log.Info("Load ItemShopInfos Count: {0}", ItemShopInfos.Count());
            ItemShopInfos.TryGetValue(44, out ItemShopInfo value);
            Console.WriteLine(value.CanBuy);
        }*/

    }
}
