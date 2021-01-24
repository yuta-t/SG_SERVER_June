using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Network.Connections;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using AgentServer.Holders;
using LocalCommons.Logging;
using AgentServer.Structuring.Shu;
using System.Collections.Concurrent;

namespace AgentServer.Packet.Send
{
    public sealed class Shu_GetUserItemInfo : NetPacket
    {
        public Shu_GetUserItemInfo(int kind, List<ShuItemInfo> iteminfos, byte last)
        {
            /*FF 2E 05 05 00 00 00 00 00 00 00 00 00 00 00
             03 00 00 00 48 AA 00 00 D1 E3 34 00 00 00 00
             00 90 67 13 B8 68 01 00 00 A3 15 00 00 00 00
             00 00 4A AA 00 00 D2 E3 34 00 00 00 00 00 90
             67 13 B8 68 01 00 00 F2 07 00 00 00 00 00 00
             5B AA 00 00 95 EF 34 00 00 00 00 00 48 97 31
             3B 67 01 00 00 8D 00 00 00 00 00 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(5); //sub opcode
            ns.Write(0);
            ns.Write(kind);
            ns.Write(iteminfos.Count);
            foreach (var i in iteminfos)
            {
                ns.Write(i.itemdescnum);
                ns.Write(i.itemID);
                ns.Write(i.gotDateTime);
                ns.Write(i.count);
                ns.Write(i.state);
            }
            ns.Write(last);  //end
        }
    }
    public sealed class Shu_GetItemInfoStr : NetPacket
    {
        public Shu_GetItemInfoStr(List<ShuItemInfo> iteminfos, byte last)
        {
            /*FF 2E 05 03 00 00 00 00 00 00 00 01 00 00 00
             48 AA 00 00 88 1C 34 00 00 00 00 00 80 EF CB
             8B 69 01 00 00 C4 00 00 00 00 00 00 00 10*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(3); //sub opcode
            ns.Write(0);
            ns.Write(iteminfos.Count);
            foreach (var i in iteminfos)
            {
                ns.Write(i.itemdescnum);
                ns.Write(i.itemID);
                ns.Write(i.gotDateTime);
                ns.Write(i.count);
                ns.Write(i.state);
            }
            ns.Write(last);  //end
        }
    }

    public sealed class Shu_HatchOK : NetPacket
    {
        public Shu_HatchOK(long eggitemid, DBShuInfo infos, byte last)//DataTable shuinfo, DataTable shuavatarinfo, DataTable shustatus, byte last)
        {
            /*FF 2E 05 01 00 00 00 00 00 00 00 65 B8 37 00 00 00 00 00
             7F B8 37 00 00 00 00 00 04 00 00 00 00 00 00 00 65 B8 37
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 3A AA 00 00 7F B8 37 00 00 00 00 00 20 89 70 DB 68
             01 00 00 01 00 00 00 01 00 00 00 3B AA 00 00 80 B8 37 00
             00 00 00 00 20 89 70 DB 68 01 00 00 01 00 00 00 01 00 00
             00 3D AA 00 00 81 B8 37 00 00 00 00 00 20 89 70 DB 68 01
             00 00 01 00 00 00 01 00 00 00 00 00 00 00 01 00 00 00 7F
             B8 37 00 00 00 00 00 7F B8 37 00 00 00 00 00 3A AA 00 00
             30 00 7F B8 37 00 00 00 00 00 80 B8 37 00 00 00 00 00 81
             B8 37 00 00 00 00 00 FF FF FF FF FF FF FF FF FF FF FF FF
             FF FF FF FF FF FF FF FF FF FF FF FF 10 00 F0 00 00 00 64
             00 00 00 E8 03 00 00 00 00 00 00 00 04 04 00 00 00 00 00
             00 00 00 00 00 00 00 00 06 00 00 00 A6 75 C5 40 C6 46 00
             00 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(1); //sub opcode
            ns.Write(0);
            ns.Write(eggitemid);
            ns.Write(infos.characterItemID[0]);
            int count = infos.shuitems.Values.Sum(s => s.Count);
            bool haveegg = false;
            if (infos.shuitems.TryGetValue(0, out var eggitem))
                haveegg = true;
            else
                count += 1;
            ns.Write(count);
            if (haveegg)
            {
                ns.Write(0);//itemdescnum
                ns.Write(eggitemid);
                ns.Write(eggitem[0].gotDateTime);
                ns.Write(eggitem[0].count);
                ns.Write(eggitem[0].state);
                //ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(egginfo.Rows[0]["gotDateTime"])));
                //ns.Write((int)egginfo.Rows[0]["count"]);
                //ns.Write((int)egginfo.Rows[0]["state"]);
            }
            else
            {
                ns.Write(0);//itemdescnum
                ns.Write(eggitemid);
                ns.Fill(0x10);
            }

            foreach (var i in infos.shuitems.Where(w => w.Key!= 0))
            {
                foreach (var j in i.Value)
                {
                    ns.Write(j.itemdescnum);
                    ns.Write(j.itemID);
                    ns.Write(j.gotDateTime);
                    ns.Write(j.count);
                    ns.Write(j.state);
                }
            }

            ns.Write(0);

            ns.Write(infos.characterItemID.Count);
            foreach (var charid in infos.characterItemID)
            {
                infos.shuchars.TryGetValue(charid, out var shuchar);
                infos.shuavatars.TryGetValue(charid, out var shuavatar);
                infos.shustatus.TryGetValue(charid, out var shustate);
                ns.Write(charid);
                ns.Write(charid);
                ns.Write(shuchar.avatarItemNum);

                ns.Write((short)0x30);//size
                foreach (var i in shuavatar.OrderBy(o => o.Position))
                {
                    ns.Write(i.itemID);
                }

                ns.Write((short)0x10);//size
                foreach (var i in shustate)
                {
                    ns.Write(i.value);
                }
                ns.Write(shuchar.MotionList);
                ns.Write(shuchar.PurchaseMotionList);
                ns.WriteBIG5Fixed_intSize(shuchar.Name);
                ns.Write(shuchar.state);
            }

            ns.Write(last);  //end
            //Console.WriteLine("Shu_Hatch: {0}", Utility.ByteArrayToString(ns.ToArray()));
        }
    }

    public sealed class Shu_GetUserCharacterItemList : NetPacket
    {
        public Shu_GetUserCharacterItemList(DBShuInfo infos, byte last)//List<ShuItemInfo> iteminfos, DataTable shuinfo, DataTable shuavatarinfo, DataTable shustatus, byte last)
        {
            /*FF 2E 05 02 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 03 00 00 00 3A AA 00 00 AA 10 34 00 00
             00 00 00 28 38 57 97 64 01 00 00 01 00 00 00 01
             00 00 00 3B AA 00 00 AB 10 34 00 00 00 00 00 28
             38 57 97 64 01 00 00 01 00 00 00 01 00 00 00 3D
             AA 00 00 AC 10 34 00 00 00 00 00 28 38 57 97 64
             01 00 00 01 00 00 00 01 00 00 00 00 00 00 00 01
             00 00 00 AA 10 34 00 00 00 00 00 AA 10 34 00 00
             00 00 00 3A AA 00 00 30 00 AA 10 34 00 00 00 00
             00 AB 10 34 00 00 00 00 00 AC 10 34 00 00 00 00
             00 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
             FF FF FF FF FF FF FF FF FF 10 00 00 00 00 00 64
             00 00 00 64 00 00 00 10 03 06 00 1F FE 2F 00 00
             00 00 00 00 40 00 00 00 00 00 00 06 00 00 00 A6
             75 C5 40 C6 46 00 00 00 00 04*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(2); //sub opcode
            ns.Write(0);
            ns.Write(0L);
            int count = infos.shuitems.Values.Sum(s => s.Count);
            ns.Write(count);
            if (count > 0)
            {
                foreach (var i in infos.shuitems.Values)
                {
                    foreach (var j in i)
                    {
                        ns.Write(j.itemdescnum);
                        ns.Write(j.itemID);
                        ns.Write(j.gotDateTime);
                        ns.Write(j.count);
                        ns.Write(j.state);
                    }
                }
                /*foreach (var i in iteminfos)
                {
                    ns.Write(i.itemdescnum);
                    ns.Write(i.itemID);
                    ns.Write(i.gotDateTime);
                    ns.Write(i.count);
                    ns.Write(i.state);
                }*/
                ns.Write(0);
                ns.Write(infos.characterItemID.Count);
                foreach (var charid in infos.characterItemID)
                {
                    infos.shuchars.TryGetValue(charid, out var shuchar);
                    infos.shuavatars.TryGetValue(charid, out var shuavatar);
                    infos.shustatus.TryGetValue(charid, out var shustate);
                    ns.Write(charid);
                    ns.Write(charid);
                    ns.Write(shuchar.avatarItemNum);

                    ns.Write((short)0x30);//size
                    foreach (var i in shuavatar.OrderBy(o => o.Position))
                    {
                        ns.Write(i.itemID);
                    }

                    ns.Write((short)0x10);//size
                    foreach (var i in shustate)
                    {
                        ns.Write(i.value);
                    }
                    ns.Write(shuchar.MotionList);
                    ns.Write(shuchar.PurchaseMotionList);
                    ns.WriteBIG5Fixed_intSize(shuchar.Name);
                    ns.Write(shuchar.state);
                }
            }
            else
            {
                ns.Write(0L);
            }
            ns.Write(last);  //end
            //Console.WriteLine("Shu_GetUserCharacterItemList: {0}", Utility.ByteArrayToString(ns.ToArray()));
        }
    }

    public sealed class Shu_GetUserStatusInfo : NetPacket
    {
        public Shu_GetUserStatusInfo(ConcurrentDictionary<long, List<int>> status, byte last)
        {
            /*FF 2E 05 08 00 00 00 00 00 00 00 01 00 00 
             00 AA 10 34 00 00 00 00 00 10 00 00 00 00
             00 64 00 00 00 64 00 00 00 10 03 06 00 08*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(8); //sub opcode
            ns.Write(0);
            ns.Write(status.Count);
            foreach (var i in status)
            {
                ns.Write(i.Key);
                ns.Write((short)0x10);//size
                foreach (var j in i.Value)
                {
                    ns.Write(j);
                }
            }
            ns.Write(last);  //end
        }
    }

    public sealed class Shu_ChangeCurrentShu : NetPacket
    {
        public Shu_ChangeCurrentShu(long beforeCharacterItemID, long shuitemid, DBShuInfo infos, byte last)
        {
            /*FF 2E 05 0C 00 00 00 00 00 00 00 FF FF FF FF FF 
             FF FF FF 7F B8 37 00 00 00 00 00 00 00 00 00 01
             00 00 00 7F B8 37 00 00 00 00 00 7F B8 37 00 00
             00 00 00 3A AA 00 00 30 00 7F B8 37 00 00 00 00
             00 80 B8 37 00 00 00 00 00 81 B8 37 00 00 00 00
             00 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF
             FF FF FF FF FF FF FF FF FF 10 00 E4 00 00 00 64
             00 00 00 E8 03 00 00 00 00 00 00 00 04 04 00 00
             00 00 00 00 00 00 00 00 00 00 00 06 00 00 00 A6
             75 C5 40 C6 46 01 00 00 00 04*/
            /*FF 2E 05 0C 00 00 00 00 00 00 00 7F B8 37 00 00
             00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00
             00 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xC); //sub opcode
            ns.Write(0);
            ns.Write(beforeCharacterItemID);
            ns.Write(shuitemid);

            ns.Write(0);

            if (shuitemid != -1)
            {
                ns.Write(infos.characterItemID.Count);
                foreach (var charid in infos.characterItemID)
                {
                    infos.shuchars.TryGetValue(charid, out var shuchar);
                    infos.shuavatars.TryGetValue(charid, out var shuavatar);
                    infos.shustatus.TryGetValue(charid, out var shustate);
                    ns.Write(charid);
                    ns.Write(charid);
                    ns.Write(shuchar.avatarItemNum);

                    ns.Write((short)0x30);//size
                    foreach (var i in shuavatar.OrderBy(o => o.Position))
                    {
                        ns.Write(i.itemID);
                    }

                    ns.Write((short)0x10);//size
                    foreach (var i in shustate)
                    {
                        ns.Write(i.value);
                    }
                    ns.Write(shuchar.MotionList);
                    ns.Write(shuchar.PurchaseMotionList);
                    ns.WriteBIG5Fixed_intSize(shuchar.Name);
                    ns.Write(shuchar.state);
                }
            }
            else
            {
                ns.Write(0);
            }

            ns.Write(last);  //end
        }
    }

    public sealed class Shu_ManagerAction : NetPacket
    {
        public Shu_ManagerAction(int actionType, long shuitemid, DBShuActionInfo infos, byte last)
        {
            /*FF 2E 05 09 00 00 00 00 00 00 00 AE C1 1E 00 00
             00 00 00 00 00 00 00 22 00 00 00 01 00 00 00 00
             00 00 00 24 00 00 00 01 00 00 00 AE C1 1E 00 00
             00 00 00 10 00 24 00 00 00 63 00 00 00 90 01 00
             00 18 A6 05 00 02*/
            /*FF 2E 05 09 00 00 00 00 00 00 00 AE C1 1E 00 00
             00 00 00 02 00 00 00 20 00 00 00 02 00 00 00 01
             00 00 00 14 00 00 00 02 00 00 00 C8 00 00 00 01
             00 00 00 AE C1 1E 00 00 00 00 00 10 00 48 00 00
             00 64 00 00 00 58 02 00 00 18 A6 05 00 02*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0x9); //sub opcode
            ns.Write(0);
            ns.Write(shuitemid);
            ns.Write(actionType);

            ns.Write(infos.remainMP);
            ns.Write(infos.ActionResult.Count);
            foreach (var i in infos.ActionResult)
            {
                ns.Write(i.statusType);
                ns.Write(i.giveValue);
            }
            ns.Write(infos.shustatus.Count);
            foreach (var shustate in infos.shustatus)
            {
                ns.Write(shustate.Key);
                ns.Write((short)0x10);//size
                foreach (var i in shustate.Value)
                {
                   ns.Write(i.value);
                }
            }
            ns.Write(last);  //end
            //Console.WriteLine("Shu_ManagerAction: {0}", Utility.ByteArrayToString(ns.ToArray()));
        }
    }

    public sealed class Shu_BuyItemOK : NetPacket
    {
        public Shu_BuyItemOK(int itemnum, int unk3, int unk4, int unk5, long itemid, byte last)
        {
            /*FF 6A 01 00 00 00 00 01 00 00 00 40 AA 00 00 B8
             3E 22 73 FF FF FF FF FF FF FF FF 00 00 00 00 00
             01 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 18
             14 13 01 01 00 00 00 99 BE 37 00 00 00 00 00 40
             AA 00 00 00 01 00 00 00 40 AA 00 00 00 00 00 00
             00 00 00 00 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x16A); // op code
            ns.Write(0);
            ns.Write(1);
            ns.Write(itemnum);
            ns.Write(unk3);
            ns.Write((long)-1);
            ns.Write(0);
            ns.Write(unk4);
            ns.Write((long)-1);
            ns.Fill(0x14);
            ns.Write(unk5);

            ns.Write(1);
            ns.Write(itemid);
            ns.Write(itemnum);

            ns.Write((byte)0);
            ns.Write(1);
            ns.Write(itemnum);
            ns.Write(0L);

            ns.Write(last);  //end
        }
    }
    public sealed class Shu_BuyItemOK2 : NetPacket
    {
        public Shu_BuyItemOK2(List<ShuItemInfo> infos, byte last)
        {
            /*FF 2E 05 03 00 00 00 00 00 00 00 01 00
             00 00 40 AA 00 00 99 BE 37 00 00 00 00
             00 58 DD F1 E1 68 01 00 00 01 00 00 00
             00 00 00 00 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); // op code
            ns.Write(3);
            ns.Write(0);
            ns.Write(infos.Count);//count
            foreach (var i in infos)
            {
                ns.Write(i.itemdescnum);
                ns.Write(i.itemID);
                ns.Write(i.gotDateTime);
                ns.Write(i.count);
                ns.Write(i.state);
            }
            /*ns.Write(itemnum);
            ns.Write(itemid);
            ns.Write(gotdatetime);
            ns.Write(1);
            ns.Write(0);*/
            ns.Write(last);  //end
        }
    }

    public sealed class Shu_ChangeAvatarInfo : NetPacket
    {
        public Shu_ChangeAvatarInfo(long shuitemid, DBShuChangeAVInfo infos, byte last)
        {
            /*FF 2E 05 0B 00 00 00 00 00 00 00 AE C1 1E 00 00 00 00 00
             02 00 00 00 45 25 22 00 00 00 00 00 00 00 00 00 46 25 22
             00 00 00 00 00 01 00 00 00 01 00 00 00 AE C1 1E 00 00 00
             00 00 30 00 AE C1 1E 00 00 00 00 00 AF C1 1E 00 00 00 00
             00 B0 C1 1E 00 00 00 00 00 FF FF FF FF FF FF FF FF 46 25
             22 00 00 00 00 00 FF FF FF FF FF FF FF FF 20*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xB); //sub opcode
            ns.Write(0);
            ns.Write(shuitemid);
  

            ns.Write(infos.AvatarState.Count);
            foreach (var i in infos.AvatarState)
            {
                ns.Write(i.itemID);
                ns.Write(i.state);
            }
            ns.Write(infos.shuavatars.Count);
            foreach (var shuavatar in infos.shuavatars)
            {
                ns.Write(shuavatar.Key);
                ns.Write((short)0x30);//size
                foreach (var i in shuavatar.Value.OrderBy(o => o.Position))
                {
                    ns.Write(i.itemID);
                }
            }
            ns.Write(last);  //end
            //Console.WriteLine("Shu_ChangeAvatarInfo: {0}", Utility.ByteArrayToString(ns.ToArray()));
        }
    }

    public sealed class Shu_ChangeNameOK : NetPacket
    {
        public Shu_ChangeNameOK(long shuitemid, string name, byte last)
        {
            /*FF 2E 05 0A 00 00 00 00 00 00 00 AA 10 34 00
             00 00 00 00 0C 00 00 00 61 61 77 61 62 72 77
             61 72 62 77 61 08*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xA); //sub opcode
            ns.Write(0);
            ns.Write(shuitemid);
            ns.WriteBIG5Fixed_intSize(name);
            ns.Write(last);  //end
        }
    }
    public sealed class Shu_ExploreStartOK : NetPacket
    {
        public Shu_ExploreStartOK(ExploreInfo info, byte last)
        {
            /*FF 2E 05 10 00 00 00 00 00 00 00 01 
             FF 28 05 0F B2 4B 00 88 B1 56 E7 68 
             01 00 00 AA 10 34 00 00 00 00 00 02*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0x10); //sub opcode
            ns.Write(0);
            ns.Write(info.zoneNum);
            ns.Write((byte)0xFF);
            ns.Write((short)0x528);
            ns.Write(0x4BB20F);
            ns.Write(info.endDateTime);
            ns.Write(info.characterItemID);
            ns.Write(last);  //end
        }
    }
    public sealed class Shu_ExploreStopOK : NetPacket
    {
        public Shu_ExploreStopOK(byte zoneid, long shuitemid, byte last)
        {
            //FF 2E 05 11 00 00 00 00 00 00 00 01 AA 10 34 00 00 00 00 00 10
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0x11); //sub opcode
            ns.Write(0);
            ns.Write(zoneid);
            ns.Write(shuitemid);
            ns.Write(last);  //end
        }
    }
    public sealed class Shu_ExploreCheck : NetPacket
    {
        public Shu_ExploreCheck(List<ExploreInfo> infos, byte last)
        {
            //FF 2E 05 0F 00 00 00 00 00 00 00 00 00 00 00 40
            /*FF 2E 05 0F 00 00 00 00 00 00 00 01 00 00 00 01
             17 00 00 00 00 00 00 88 45 68 E7 68 01 00 00 AA
             10 34 00 00 00 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xF); //sub opcode
            ns.Write(0);
            ns.Write(infos.Count);
            foreach (var i in infos) 
            {
                ns.Write(i.zoneNum);
                ns.Fill(0x7);
                ns.Write(i.endDateTime);
                ns.Write(i.characterItemID);
            }
            ns.Write(last);  //end
        }
    }
    public sealed class Shu_ExploreReward : NetPacket
    {
        public Shu_ExploreReward(byte zonenum, long shuitemid, List<ShuRewardInfo> infos, byte last)
        {
            /*FF 2E 05 12 00 00 00 00 00 00 00 01 AA 10 34 00
             00 00 00 00 01 00 00 00 C8 00 00 00 00 00 00 00
             20 03 00 00 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0x12); //sub opcode
            ns.Write(0);
            ns.Write(zonenum);
            ns.Write(shuitemid);

            ns.Write(infos.Count);
            foreach (var i in infos)
            {
                ns.Write(i.rewardType);
                ns.Write(i.rewardItem);
                ns.Write(i.rewardCount);
            }
            ns.Write(last);  //end
        }
    }
    public sealed class Shu_GetGiftOK : NetPacket
    {
        public Shu_GetGiftOK(long shuitemid, int exp, List<ShuRewardInfo> infos, byte last)
        {
            /*FF 2E 05 0E 00 00 00 00 00 00 00 AA 10 34 00 00
             00 00 00 01 00 00 00 64 00 00 00 E8 98 00 00 01
             00 00 00 80 9D 05 00 20*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xE); //sub opcode
            ns.Write(0);
            ns.Write(shuitemid);

            ns.Write(infos.Count);
            foreach (var i in infos)
            {
                ns.Write(i.rewardType);
                ns.Write(i.rewardItem);
                ns.Write(i.rewardCount);
            }
            ns.Write(exp);
            ns.Write(last);  //end
        }
    }

    public sealed class Shu_UseItem : NetPacket
    {
        public Shu_UseItem(int position, long shucharitemid, long shuitemid, int itemnum, int usecount, DBShuUseItemInfo infos, byte last)
        {
            /*FF 2E 05 0D 00 00 00 00 00 00 00 AA 10 34 00 00 
             00 00 00 D1 E3 34 00 00 00 00 00 48 AA 00 00 01
             00 00 00 D1 07 00 00 01 00 00 00 AA 10 34 00 00
             00 00 00 10 00 3C 00 00 00 5C 00 00 00 20 03 00
             00 54 B5 05 00 01 00 00 00 48 AA 00 00 D1 E3 34
             00 00 00 00 00 90 67 13 B8 68 01 00 00 9E 15 00
             00 00 00 00 00 04*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xD); //sub opcode
            ns.Write(0);
            ns.Write(shucharitemid);
            ns.Write(shuitemid);
            ns.Write(itemnum);
            ns.Write(usecount);
            ns.Write(position);

            if (position == 2001)
            {
                ns.Write(infos.shustatus.Count);
                foreach (var i in infos.shustatus)
                {
                    ns.Write(i.Key);
                    ns.Write((short)0x10);//size
                    foreach (var j in i.Value)
                    {
                        ns.Write(j.value);
                    }
                }
            }
            else if (position == 2002)
            {
                ns.Write(infos.remainMP);
            }
            else if (position == 2003)
            {
                ns.Write(0);
                ns.Write(infos.characterItemID.Count);
                foreach (var charid in infos.characterItemID)
                {
                    infos.shuchars.TryGetValue(charid, out var shuchar);
                    infos.shuavatars.TryGetValue(charid, out var shuavatar);
                    infos.shustatus.TryGetValue(charid, out var shustate);
                    ns.Write(charid);
                    ns.Write(charid);
                    ns.Write(shuchar.avatarItemNum);

                    ns.Write((short)0x30);//size
                    foreach (var i in shuavatar.OrderBy(o => o.Position))
                    {
                        ns.Write(i.itemID);
                    }

                    ns.Write((short)0x10);//size
                    foreach (var i in shustate)
                    {
                        ns.Write(i.value);
                    }
                    ns.Write(shuchar.MotionList);
                    ns.Write(shuchar.PurchaseMotionList);
                    ns.WriteBIG5Fixed_intSize(shuchar.Name);
                    ns.Write(shuchar.state);
                }
            }

            int count = infos.ItemInfos.Count;
            if (count > 0)
            {
                ns.Write(infos.ItemInfos.Count);
                foreach (var i in infos.ItemInfos)
                {
                    ns.Write(i.itemdescnum);
                    ns.Write(i.itemID);
                    ns.Write(i.gotDateTime);
                    ns.Write(i.count);
                    ns.Write(i.state);
                }
            }
            else
            {
                ns.Write(1);
                ns.Write(0);
                ns.Write(shuitemid);
                ns.Fill(0x10);
            }

            ns.Write(last);  //end
        }
    }
    public sealed class Shu_UseItemFail : NetPacket
    {
        public Shu_UseItemFail(long shucharitemid, long shuitemid, int itemnum, int usecount, byte last)
        {
            /*FF 2E 05 0D 00 00 00 13 00 00 00 AA 10 34 00 00 
             00 00 00 95 EF 34 00 00 00 00 00 5B AA 00 00 01 00 00 00 08*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0xD); //sub opcode
            ns.Write(0x13);
            ns.Write(shucharitemid);
            ns.Write(shuitemid);
            ns.Write(itemnum);
            ns.Write(usecount);
            ns.Write(last);  //end
        }
    }

    public sealed class Shu_LevelUP : NetPacket
    {
        public Shu_LevelUP(long shucharitemid, int beforelv, int afterlv, byte last)
        {
            /*FF 2E 05 14 00 00 00 00 00 00 00 01 00 00 00
             02 00 00 00 9F C5 37 00 00 00 00 00 20*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x52E); //opcode
            ns.Write(0x14); //sub opcode
            ns.Write(0);
            ns.Write(beforelv);
            ns.Write(afterlv);
            ns.Write(shucharitemid);
            ns.Write(last);  //end
        }
    }
}
