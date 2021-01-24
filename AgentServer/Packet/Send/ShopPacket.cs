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
using System.Collections.Concurrent;

namespace AgentServer.Packet.Send
{

    public sealed class ShopBuyItem_ACK : NetPacket
    {
        public ShopBuyItem_ACK(Account User, int itemid, int unk3, int unk4, int unk5, byte last)
        {
            /*FF 6A 01 00 00 00 00 01 00 00 00 47 96 00 00 
            AC E3 4B 01 FF FF FF FF FF FF FF FF 00 00 00 00 
            00 01 15 01 FF FF FF FF FF FF FF FF 00 00 00 00 
            00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
            EF 08 00 00 00 00 00 00 00 01 00 00 00 47 96 00 
            00 00 00 00 00 00 00 00 00 20*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x16A); // op code
            ns.Write(0);
            ns.Write(1);
            ns.Write(itemid);
            ns.Write(unk3);
            ns.Write((long)-1);
            ns.Write(0);
            ns.Write(unk4);
            ns.Write((long)-1);
            ns.Fill(0x14);
            ns.Write(unk5);
            ns.Write(0);
            ns.Write((byte)0);
            ns.Write(1);
            ns.Write(itemid);
            ns.Write(0L);

            ns.Write(last);  //end

        }
    }
    public sealed class ShopBuyItemFail : NetPacket
    {
        public ShopBuyItemFail(Account User, int itemid, int unk3, int unk4, int unk5, byte last)
        {
            /*FF 6A 01 48 00 00 00 00 00 00 00 00 01 
             00 00 00 66 12 00 00 AC E3 4B 01 FF FF
             FF FF FF FF FF FF 01 00 00 00 00 01 13 
             01 FF FF FF FF FF FF FF FF 00 00 00 00 
             00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 85 09 00 00 00 00 00 00 00 00 
             00 00 04*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x16A); // op code
            ns.Write((byte)0x48);
            ns.Write(0L);
            ns.Write(1);
            ns.Write(itemid);
            ns.Write(unk3);
            ns.Write((long)-1);
            ns.Write(0);
            ns.Write(unk4);
            ns.Write((long)-1);
            ns.Fill(0x14);
            ns.Write(unk5);
            ns.Write(0L);
            ns.Write(last);  //end

        }
    }
    public sealed class ShopBuyFreePassUpdate : NetPacket
    {
        public ShopBuyFreePassUpdate(Account User, byte last)
        {
            //FF 52 05 01 00 00 00 98 F3 A4 3D 68 01 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x552); // op code
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_freepass_getUserDesc";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ns.Write(Convert.ToInt32(reader["type"]));
                ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);  //end
        }
    }
    public sealed class ShopBuyFreePassUpdate2 : NetPacket
    {
        public ShopBuyFreePassUpdate2(Account User, byte last)
        {
            //FF 4F 05 01 00 00 00 98 F3 A4 3D 68 01 00 00 14 14 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x54F); // op code
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_storage_getUserDesc";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ns.Write(Convert.ToInt32(reader["type"]));
                ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                ns.Write(Convert.ToByte(reader["maxSavableCount"]));
                ns.Write(Convert.ToByte(reader["maxReceivableCount"]));
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);  //end
        }
    }

    public sealed class ShopGiftItemOK_ACK : NetPacket
    {
        public ShopGiftItemOK_ACK(Account User, int itemid, string nickname, byte last)
        {

            ns.Write((byte)0xFF);
            ns.Write((short)0x171); // op code
            ns.Write(0);
            ns.Write(itemid);
            ns.Write(User.TR);
            ns.Write(0x1F4); //unknown
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(last);  //end

        }
    }
    public sealed class ShopGiftItemError_ACK : NetPacket
    {
        public ShopGiftItemError_ACK(Account User, byte errorid, byte last)
        {

            ns.Write((byte)0xFF);
            ns.Write((short)0x171); // op code
            ns.Write(0x49);
            ns.Write(errorid);
            ns.Write(last);  //end

        }
    }

    public sealed class ShopBuyItemInfo_ACK : NetPacket
    {
        public ShopBuyItemInfo_ACK(Account User, List<AvatarItemInfo> iteminfos, byte last)
        {
            //FF 48 01 00 00 00 00 01 00 00 00 
            //B8 C1 9B 00 01 00 05 00 05 00 45 96 00 00 30 2E B1 3B 68 01 00 00 
            //C0 40 3E D0 67 01 00 00 01 00 00 00 00 00 00 00 01 01 01 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x148); // op code
            ns.Write(0);
            ns.Write(iteminfos.Count);
            foreach (var item in iteminfos)
            {
                ns.Write(0x9CC1D0); //D0 C1 9C 00
                ns.Write(item.character);
                ns.Write(item.position);
                ns.Write(item.kind);
                ns.Write(item.itemdescnum);
                ns.Write(item.expireTime);
                ns.Write(item.gotDateTime);
                ns.Write(item.count);
                ns.Write(item.exp);
                ns.Write((byte)0);
                ns.Write(item.use);
            }
            ns.Write((byte)1);
            ns.Write(last);  //end
        }
    }

    public sealed class CurrentGameMoney_ACK : NetPacket
    {
        public CurrentGameMoney_ACK(Account User, byte last)
        {
            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCurrentGameMoney";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                User.TR = Convert.ToInt64(reader["fdGameMoney"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x17C); // op code
            ns.Write(User.TR); 
            ns.Write(last);  //end

        }
    }

}
