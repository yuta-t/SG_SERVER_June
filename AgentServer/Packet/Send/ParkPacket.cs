using AgentServer.Holders;
using AgentServer.Structuring.Park;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using AgentServer.Structuring.Item;

namespace AgentServer.Packet.Send
{
    public sealed class GetMachineInfo : NetPacket
    {
        public GetMachineInfo(Account User, int MachineNum, CapsuleMachineInfo MachineInfo,  byte last)
        {
            /*FF 74 05 7C DF 00 00 62 00 00 00 FF FF FF FF 01 00 00 00 00 
             0A 00 00 00 62 F2 00 00 00 00 00 00 05 00 00 00 62 F2 00 
             00 02 66 F2 00 00 01 00 00 00 02 00 00 00 66 F2 00 00 01 
             6A F2 00 00 03 00 00 00 14 00 00 00 6A F2 00 00 03 6E F2 
             00 00 03 00 00 00 14 00 00 00 6E F2 00 00 03 72 F2 00 00 
             08 00 00 00 1E 00 00 00 72 F2 00 00 03 76 F2 00 00 03 00 
             00 00 1E 00 00 00 76 F2 00 00 03 8A F2 00 00 42 00 00 00 
             78 00 00 00 8A F2 00 00 04 8B F2 00 00 46 00 00 00 78 00 
             00 00 8B F2 00 00 04 8C F2 00 00 8E 00 00 00 96 00 00 00 
             8C F2 00 00 05 8F F2 00 00 1C 01 00 00 2F 01 00 00 8F F2 
             00 00 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
             00 00 00 00 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x574);
            ns.Write(MachineInfo.RealMachineNum);
            ns.Write(MachineInfo.RealMachineNumKind);
            if(MachineNum != MachineInfo.RealMachineNum)
                ns.Write(MachineNum);
            else
                ns.Write(-1); //FF FF FF FF
            ns.Write((byte)1);
            ns.Write(0);
            CapsuleMachineHolder.CapsuleMachineItems.TryGetValue(MachineNum, out List<CapsuleMachineItem> MachineItems);
            ns.Write(MachineItems.Count);
            foreach (var item in MachineItems)
            {
                ns.Write(item.ItemNum);
                ns.Write((int)item.ItemCount);
                ns.Write((int)item.ItemMax);
                ns.Write(item.ItemNum);
                ns.Write(item.Level);
            }
            ns.Fill(0x14);
            ns.Write(last);
            //Console.WriteLine("ns: {0}",Utility.ByteArrayToString(ns.ToArray()));
        }

    }
    public sealed class GetMachineInfoFail : NetPacket
    {
        public GetMachineInfoFail(Account User, int MachineItemNum, int MachineID, byte last)
        {
            /*FF 74 05 85 4E 00 00 09 00 00 00 FF FF FF FF 00 
             02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
             60 BC 87 06 00 00 00 00 00 00 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x574);
            ns.Write(MachineItemNum);
            ns.Write(MachineID);
            ns.Write(-1); //FF FF FF FF
            ns.Write((byte)0);
            ns.Write(2);
            ns.Write(0);
            ns.Fill(0x14);
            ns.Write(last);
        }

    }
    public sealed class GetMachineInfoResetting : NetPacket
    {
        public GetMachineInfoResetting(Account User, int MachineNum, CapsuleMachineInfo MachineInfo, byte last)
        {
            /*FF 74 05 17 52 00 00 0E 00 00 00 FF FF FF FF 00 
             09 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
             00 60 BC 87 06 00 00 00 00 00 00 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x574);
            ns.Write(MachineInfo.RealMachineNum);
            ns.Write(MachineInfo.RealMachineNumKind);
            if (MachineNum != MachineInfo.RealMachineNum)
                ns.Write(MachineNum);
            else
                ns.Write(-1); //FF FF FF FF
            ns.Write((byte)0);
            ns.Write(9);
            ns.Write(0);
            ns.Fill(0x14);
            ns.Write(last);
        }

    }
    public sealed class GetMachineSelectItem : NetPacket
    {
        public GetMachineSelectItem(Account User, int MachineItemNum, int ResultItemNum, byte last)
        {
            /*FF 32 02 66 F1 00 00 84 E7 00 00 01*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x232);
            ns.Write(MachineItemNum);
            ns.Write(ResultItemNum);
            ns.Write(last);
        }
    }

    public sealed class GetMachineSelectItemFail : NetPacket
    {
        public GetMachineSelectItemFail(int MachineItemNum, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x233);
            ns.Write(MachineItemNum);
            ns.Write((byte)1);//error id: 1=DB Error, 15=not enough required item/point
            ns.Write(last);
        }
    }

    public sealed class MachineGiveItem : NetPacket
    {
        public MachineGiveItem(Account User, int ResultItemNum, bool isGift, string NickName, byte last)
        {
            /*FF 32 02 66 F1 00 00 84 E7 00 00 01*/
            //FF 38 02 01 D1 0D 00 00 06 00 00 00 41 6C 74 4B 6F 59 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x238);
            ns.Write(isGift);
            ns.Write(ResultItemNum);
            if (isGift)
                ns.WriteBIG5Fixed_intSize(NickName);
            ns.Write(last);
        }
    }
    public sealed class MachineGiveItemFail : NetPacket
    {
        public MachineGiveItemFail(byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x239);
            ns.Write((byte)0x5);
            ns.Write(last);
        }
    }
    public sealed class MachineKeepItem : NetPacket
    {
        public MachineKeepItem(Account User, bool isSuccess, long uniqueNum, int itemNum, long dateTime, byte last)
        {
            /*FF 48 05 00 00 00 00 01 00 00 00 26 02 09 
             00 00 00 00 00 2E 1A 00 00 F8 D3 15 FB 
             67 01 00 00 00 00 00 00 20*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x548);
            ns.Write(isSuccess ? 0 : 3);
            ns.Write(1); //type?
            ns.Write(uniqueNum);
            ns.Write(itemNum);
            ns.Write(dateTime);
            ns.Write(0);
            ns.Write(last);
        }
    }

    public sealed class CapsuleMachineNotice : NetPacket
    {
        public CapsuleMachineNotice(Account User, byte level, int MachineItemNum, int ItemNum, string NickName)
        {
            //FF 3A 02 01 53 AB 00 00 AC 91 00 00 00 00 00 00 06 00 00 00 B9 5D B8 4C B8 4C 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x23A);
            ns.Write(level); //type?
            ns.Write(MachineItemNum);
            ns.Write(ItemNum);
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(NickName);
            ns.Write((byte)1); //last
        }
    }
    public sealed class RoatateMachineNotice : NetPacket
    {
        public RoatateMachineNotice(Account User, string MachineItemNum)
        {
            //77 03 00 00 00 03 00 00 00 05 00 00 00 34 33 38 35 36 01
            ns.Write((byte)0x77);
            ns.Write(3);
            ns.Write(3);
            ns.WriteBIG5Fixed_intSize(MachineItemNum);
            ns.Write((byte)1); //last
        }
    }

    public sealed class AlchemistMix : NetPacket
    {
        public AlchemistMix(Account User, int resultitem, List<ItemAttr> Attrs, byte last)
        {
            /*FF AD 01 00 00 00 00 35 02 00 00 04 00 00 00
             02 00 29 5C 8F 3D 13 00 00 00 80 3F 2C 00 00
             00 48 43 32 00 9A 99 19 3E 80*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x1AD);
            ns.Write(0);
            ns.Write(resultitem);
            ns.Write(Attrs.Count);
            foreach(var attr in Attrs)
            {
                ns.Write(attr.Attr);
                ns.Write(attr.AttrValue);
            }
           
            ns.Write(last); //end
        }
    }

    public sealed class AlchemistUpgrade : NetPacket
    {
        public AlchemistUpgrade(Account User, int resultitem, List<ItemAttr> Attrs, byte last)
        {
            /*FF AF 01 00 00 00 00 00 35 02 00 00 04 00 00
             00 02 00 29 5C 8F 3D 13 00 00 00 80 3F 2C 00
             00 00 48 43 32 00 8F C2 F5 3D 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x1AF);
            ns.Write((byte)0);
            ns.Write(0);
            ns.Write(resultitem);
            ns.Write(Attrs.Count);
            foreach (var attr in Attrs)
            {
                ns.Write(attr.Attr);
                ns.Write(attr.AttrValue);
            }

            ns.Write(last); //end
        }
    }

}
