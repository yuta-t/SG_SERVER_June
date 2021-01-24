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
using AgentServer.Structuring.User;
using AgentServer.Structuring.Item;

namespace AgentServer.Packet.Send
{
    public sealed class ShowPage : NetPacket
    {
        public ShowPage(Account User, byte type, byte last)
        {
            string url;
            ns.Write((byte)115); //0x73 op code
            ns.Write(type);//0 lobby page, 1 charge page, 2 升級系統, 6 endgame page, 
            switch (type)
            {
                default:
                    Log.Warning("Unknown show page type {0}", type);
                    url = "https://www.renewcreation.com";
                    break;
                case 0:
                    url = ServerSettingHolder.ServerSettings.GateNoticeURL;
                    break;
                case 1:
                    url = ServerSettingHolder.ServerSettings.cashFillUpURL;
                    break;
                case 2:
                    url = ServerSettingHolder.ServerSettings.EveryDayEventURL;
                    break;
                case 6:
                    url = ServerSettingHolder.ServerSettings.QuitConfirmDialogURL;
                    break;
            }
            ns.WriteASCIIFixed_intSize(url);
            ns.Write(last);
        }
    }
    public sealed class PingTime_0X41 : NetPacket
    {
        public PingTime_0X41(long time, byte last)
        {
            ns.Write((byte)0x41);
            ns.Write(time);
            ns.Write(last);
        }
    }
    public sealed class SinglePlay_0X1D4 : NetPacket
    {
        public SinglePlay_0X1D4(Account User, int mapnum, byte last)
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
        public GetUserInfo(Account User, string nickname, byte last)
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
                bool fashionMode = Convert.ToBoolean(reader["fashionMode"]);
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
                if (!fashionMode) {
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
                }
                else
                {
                    ns.Write(Convert.ToInt16(reader["character"]));
                    ns.Fill(0x1C);
                }
                ns.Fill(0x88);
                ns.Write(fashionMode ? (short)0 : Convert.ToInt16(reader["fdCostumeMode"]));
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
                ns.Write(Convert.ToInt32(reader["fdGameOption"]));
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

    public sealed class GetUserPoint : NetPacket
    {
        public GetUserPoint(int type, int totlapoint, int currentpoint, byte last)
        {
            //FF 3A 05 60 09 00 00 5F 01 00 00 5F 01 00 00 10
            ns.Write((byte)0xFF);
            ns.Write((short)0x53A);
            ns.Write(type);
            ns.Write(totlapoint);
            ns.Write(currentpoint);
            ns.Write(last);
        }
    }
    public sealed class GetEventPickBoard : NetPacket
    {
        public GetEventPickBoard(int pickboardnum, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x51B);
            ns.Write(pickboardnum);
            ns.Write(2);
            ns.Write(last);
        }
    }

    public sealed class GetUserItemCollectionPointInfo : NetPacket
    {
        public GetUserItemCollectionPointInfo(string UserName, UserItemCollectionInfo info, byte last)
        {
            //20 00 00 00 00 04 00 00 00 B9 DC AB D3 01 64 0F 01 00 00 00 00 00 01 00 00 00 08
            ns.Write((byte)0x20);
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(UserName);
            ns.Write(info.noticedLevel);
            ns.Write(info.point);
            ns.Write(0);
            ns.Write(info.rank);
            ns.Write(last);
        }
    }
    public sealed class GetUserItemCollectionItemInfo : NetPacket
    {
        public GetUserItemCollectionItemInfo(string UserName, List<int> itemnums, byte last)
        {
            /*22 00 00 00 00 04 00 00 00 B9 DC AB D3 08 E1 E7 00 00
             18 D1 00 00 D3 AB 00 00 D2 AB 00 00 F6 E4 00 00 FA E4 00
             00 FE E4 00 00 D4 31 01 00 08*/
            ns.Write((byte)0x22);
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(UserName);
            ns.Write((byte)itemnums.Count);
            foreach (var itemnum in itemnums)
            {
                ns.Write(itemnum);
            }
            ns.Write(last);
        }
    }
    public sealed class SetItemCollectionShowItemOK : NetPacket
    {
        public SetItemCollectionShowItemOK(byte last)
        {
            //24 00 00 00 00 02
            ns.Write((byte)0x24);
            ns.Write(0);
            ns.Write(last);
        }
    }

    public sealed class SetGameOption : NetPacket
    {
        public SetGameOption(int option, byte last)
        {
            //93 00 00 00 00 31 00 00 00 20
            ns.Write((byte)0x93);
            ns.Write(0);
            ns.Write(option);
            ns.Write(last);
        }
    }

    public sealed class GetUserEXPInfo : NetPacket
    {
        public GetUserEXPInfo(short type, long value, byte last)
        {
            //1E 00 00 00 00 01 00 C3 01 00 00 00 00 00 00 04
            ns.Write((byte)0x1E);
            ns.Write(0);
            ns.Write(type);
            ns.Write(value);
            ns.Write(last);
        }
    }

    public sealed class ExpiredItemInfo : NetPacket
    {
        public ExpiredItemInfo(AvatarItemInfo item, byte last)
        {
            ns.Write((byte)0x6B);
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
            ns.Write((byte)0); //unk
            ns.Write(last);
        }
    }

    public sealed class RightClickItemInfo : NetPacket
    {
        public RightClickItemInfo(int ItemNum, byte last)
        {
            //FF 98 05 00 00 00 00 00 00 00 00 F8 64 00 00 01 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x598);
            ns.Write(0L);
            ns.Write(ItemNum);
            ns.Write((byte)1);
            ns.Write(last);
        }
    }

}
