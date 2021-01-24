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

namespace AgentServer.Packet.Send
{
    public sealed class ShowPage : NetPacket
    {
        public ShowPage(Account User, byte type, byte last) : base(1, User.EncryptKey)
        {
            string url = string.Empty;
            ns.Write((byte)115); //0x73 op code
            ns.Write(type);//0 lobby page, 1 charge page, 2 升級系統, 6 endgame page, 
            if (type == 6)
            {
                url = "http://gameweb.mjonline.com.hk/talesrunner/activities/tr_ingame_end.html";
            }
            else if (type == 0)
            {
                url = "http://gameweb.mjonline.com.hk/talesrunner/tr_ingame.html";
            }
            else
            {
                url = "https://www.renewcreation.com";
            }
            ns.WriteASCIIFixed_intSize(url);
            ns.Write(last);
        }
    }
    public sealed class PingTime_0X41 : NetPacket
    {
        public PingTime_0X41(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0x41);
            ns.Write(Utility.CurrentTimeMilliseconds());
            ns.Write(last);
        }
    }
    public sealed class SinglePlay_0X1D4 : NetPacket
    {
        public SinglePlay_0X1D4(Account User, int mapnum, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x1D4);
            ns.Write((0));
            ns.Write(mapnum);
            ns.Write(last);
        }
    }

    public sealed class GetUserInfo : NetPacket
    {
        public GetUserInfo(Account User, string nickname, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0x91); //op code
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(nickname);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getUserInfo";
                cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ns.Write(Convert.ToByte(reader["attribute"]));
                ns.Write(Convert.ToInt64(reader["fdExp"]));
                ns.Write(Convert.ToInt16(reader["character"]));
                ns.Write(Convert.ToInt16(reader["head"]));
                ns.Write(Convert.ToInt16(reader["topBody"]));
                ns.Write(Convert.ToInt16(reader["downBody"]));
                ns.Write(Convert.ToInt16(reader["foot"]));
                ns.Write(Convert.ToInt16(reader["acHead"]));
                ns.Write(Convert.ToInt16(reader["acHand"]));
                ns.Write(Convert.ToInt16(reader["acFace"]));
                ns.Write(Convert.ToInt16(reader["acBack"]));
                ns.Write(Convert.ToInt16(reader["acNeck"]));
                ns.Write(Convert.ToInt16(reader["pet"]));
                ns.Write(Convert.ToInt16(reader["expansion"]));
                ns.Write(Convert.ToInt16(reader["acWrist"]));
                ns.Write(Convert.ToInt16(reader["acBooster"]));
                ns.Write(Convert.ToInt16(reader["acTail"]));
                ns.Fill(0x88);
                ns.Write(Convert.ToInt16(reader["cos_character"]));
                ns.Write(Convert.ToInt16(reader["cos_head"]));
                ns.Write(Convert.ToInt16(reader["cos_topBody"]));
                ns.Write(Convert.ToInt16(reader["cos_downBody"]));
                ns.Write(Convert.ToInt16(reader["cos_foot"]));
                ns.Write(Convert.ToInt16(reader["cos_acHead"]));
                ns.Write(Convert.ToInt16(reader["cos_acHand"]));
                ns.Write(Convert.ToInt16(reader["cos_acFace"]));
                ns.Write(Convert.ToInt16(reader["cos_acBack"]));
                ns.Write(Convert.ToInt16(reader["cos_acNeck"]));
                ns.Write(Convert.ToInt16(reader["cos_pet"]));
                ns.Write(Convert.ToInt16(reader["cos_expansion"]));
                ns.Write(Convert.ToInt16(reader["cos_acWrist"]));
                ns.Write(Convert.ToInt16(reader["cos_acBooster"]));
                ns.Write(Convert.ToInt16(reader["cos_acTail"]));
                ns.Fill(0x88);
                ns.Write(Convert.ToInt16(reader["fdCostumeMode"]));
                if (Convert.IsDBNull(reader["fdGuildName"]))
                {
                    ns.Write(0);
                    ns.WriteBIG5Fixed_intSize("http://0");
                }
                ns.Write(Convert.ToInt32(reader["couplenum"]));
                ns.Write(0);
                ns.WriteBIG5Fixed_intSize(reader["matename"].ToString());
                ns.Write(0x68BB6671178CB);
                ns.Write(0L);
                ns.Write(0x68BB6671178CB);
                ns.Write(-1L);
                ns.Write(0);
                ns.Write(-1);
                ns.Fill(0x11);
                ns.Write(Convert.ToInt32(reader["emblemCount"]));
                ns.Fill(0x10);
                ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["lastLogoutTime"])));
                ns.Write(Convert.ToInt32(reader["playingTime"]));
                ns.Write(Convert.ToInt32(reader["fdLikeable"]));
                ns.Write(0x20); //unknown2
                ns.Write(0x1E4AC0); //unknown3
                ns.Write(0L); //unknown4
                ns.Write(Convert.ToInt32(reader["FarmTypeNum"]));
                ns.WriteBIG5Fixed_intSize(reader["Farmname"].ToString());
                ns.Write(0);
                ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["FarmExpireDateTime"])));
                ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["FarmCreateDateTime"])));
                ns.Write(1); //unk
                ns.Fill(0xF);
                ns.Write(Convert.ToInt64(reader["FarmExp"]));
                ns.Fill(0x6);//00 01 00 01 01 01
                ns.Write(Convert.ToInt32(reader["type"]));
                long expireTime = Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"]));
                ns.Write(expireTime);
                ns.Write(Convert.ToByte(reader["maxSavableCount"]));
                ns.Write(Convert.ToByte(reader["maxReceivableCount"]));
                ns.Write(Convert.ToInt32(reader["type"]));//0
                ns.Write(expireTime);
                ns.Write(Convert.ToInt32(reader["TopRank"])); //Top100排名
                ns.Write(0x6); //count?
                ns.Write(0); //unk
                ns.Write(0); //銅
                ns.Write(0); //銀
                ns.Write(0); //金
                ns.Write(0); //鑽石
                ns.Write(0); //冠軍

                ns.Write(0); //unk
                ns.Write(58018); //TalesBook基本背景 itemnum
                ns.Write(last);  //end
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }
}
