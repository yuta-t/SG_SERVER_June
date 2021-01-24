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
using AgentServer.Structuring.Item;
using NestedDictionaryLib;

namespace AgentServer.Packet.Send
{
    public sealed class EnchantItemInfo : NetPacket
    {
        public EnchantItemInfo(int ItemNum, NestedDictionary<int, byte, byte, int, List<ItemAttr>> infos, byte last)
        {
            //FF 8B 05 01 00 00 00 FF FF FF FF 00 00 00 00 10
            /*FF 8B 05 01 00 00 00 FF FF FF FF 02 00 00 00
             35 02 00 00 01 01 04 05 D2 00 00 02 02 00 8F
             C2 F5 3C 7B 17 00 00 80 3F 77 E5 00 00 03 01
             04 61 D1 00 00 01 25 00 00 00 80 3F 02 09 00
             00 00 00 00 03 09 00 00 00 00 00 08*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x58B);
            ns.Write(1);
            ns.Write(ItemNum);
            ns.Write(infos.Count);
            if (infos.Count == 0)
                goto End;
            foreach (var y in infos)
            {
                ns.Write(y.Key); //ItemNum
                ns.Write((byte)y.Value.Count);//socket count
                foreach (var i in y.Value)
                {
                    ns.Write(i.Key); //SeqNum
                    foreach (var j in i.Value)
                    {
                        ns.Write(j.Key); //SocketNum
                        foreach (var k in j.Value)
                        {
                            ns.Write(k.Key); //StoneNum
                            ns.Write((byte)k.Value.Count(c => c.AttrValue > 0)); //attr count
                            foreach (var x in k.Value.Where(c => c.AttrValue > 0))
                            {
                                ns.Write(x.Attr); //attr 6011 = point
                                ns.Write(x.AttrValue);
                            }
                        }
                    }
                }
            }
            End:
            ns.Write(last);
        }
    }

    public sealed class HardeningDone : NetPacket
    {
        public HardeningDone(long TR, int StoneNum, int ResultStoneNum, byte last)
        {
            //FF 95 05 01 00 00 00 00 00 00 00 3F 89 6A 03 00 00 00 00 61 D1 00 00 55 D3 00 00 40
            //FF 95 05 01 00 00 00 00 00 00 00 13 88 6A 03 00 00 00 00 61 D1 00 00 85 D1 00 00 01
            //FF 95 05 01 00 00 00 00 00 00 00 0B 81 6A 03 00 00 00 00 61 D1 00 00 6D D1 00 00 10
            //FF 95 05 01 00 00 00 00 00 00 00 B3 7E 6A 03 00 00 00 00 61 D1 00 00 6D D1 00 00 80

            //FF 95 05 01 00 00 00 00 00 00 00 47 77 6A 03 00 00 00 00 69 D1 00 00 00 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x595);
            ns.Write(1);
            ns.Write(0);
            ns.Write(TR);
            ns.Write(StoneNum);
            ns.Write(ResultStoneNum);
            ns.Write(last);
        }
    }
    public sealed class StoneMountSuccess : NetPacket
    {
        public StoneMountSuccess(long TR, byte SeqNum, int ItemNum, int StoneNum, NestedDictionary<byte, byte, int, List<ItemAttr>> infos, byte last)
        {
            /*FF 8D 05 01 00 00 00 00 00 00 00 5F 73 6A 03 
             00 00 00 00 03 77 E5 00 00 01 03 09 0D D2 00
             00 02 02 00 CD CC 4C 3D 7B 17 00 00 40 40 80*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x58D);
            ns.Write(1);//err
            ns.Write(0);
            ns.Write(TR);
            ns.Write(SeqNum);
            ns.Write(ItemNum);
            ns.Write((byte)infos.Count);
            foreach (var i in infos)
            {
                ns.Write(i.Key); //SeqNum
                foreach (var j in i.Value)
                {
                    ns.Write(j.Key); //SocketNum
                    foreach (var k in j.Value)
                    {
                        ns.Write(k.Key); //StoneNum
                        ns.Write((byte)k.Value.Count(c => c.AttrValue > 0)); //attr count
                        foreach (var x in k.Value.Where(c => c.AttrValue > 0))
                        {
                            ns.Write(x.Attr); //attr 6011 = point
                            ns.Write(x.AttrValue);
                        }
                    }
                }
            }
            ns.Write(last);
        }
    }
    public sealed class StoneMountFail : NetPacket
    {
        public StoneMountFail(byte last)
        {
            //FF 8D 05 0F 00 00 00 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x58D);
            ns.Write(0xF);//err
            ns.Write(last);
        }
    }

    public sealed class StoneRemoveSuccess : NetPacket
    {
        public StoneRemoveSuccess(int ReturnStoneNum, int ItemNum, NestedDictionary<byte, byte, int, List<ItemAttr>> infos, byte last)
        {
            /*FF 91 05 01 00 00 00 01 41 D2 00 00 00 00 00 00 
             77 E5 00 00 03 01 04 61 D1 00 00 01 25 00 00 00
             80 3F 02 09 00 00 00 00 00 03 09 0D D2 00 00 02
             02 00 CD CC 4C 3D 7B 17 00 00 40 40 10*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x591);
            ns.Write(1);
            ns.Write((byte)1);
            ns.Write(ReturnStoneNum);
            ns.Write(0);
            ns.Write(ItemNum);
            ns.Write((byte)infos.Count);
            foreach (var i in infos)
            {
                ns.Write(i.Key); //SeqNum
                foreach (var j in i.Value)
                {
                    ns.Write(j.Key); //SocketNum
                    foreach (var k in j.Value)
                    {
                        ns.Write(k.Key); //StoneNum
                        ns.Write((byte)k.Value.Count(c => c.AttrValue > 0)); //attr count
                        foreach (var x in k.Value.Where(c => c.AttrValue > 0))
                        {
                            ns.Write(x.Attr); //attr 6011 = point
                            ns.Write(x.AttrValue);
                        }
                    }
                }
            }
            ns.Write(last);
        }
    }
}
