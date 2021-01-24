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

namespace AgentServer.Packet.Send
{
    public sealed class NP_Hex : NetPacket
    {
        public NP_Hex(Account User, string value) : base(1, User.EncryptKey)
        {
            ns.WriteHex(value);
        }
    }
    public sealed class NP_Byte : NetPacket
    {
        public NP_Byte(Account User, byte[] value) : base(1, User.EncryptKey)
        {
            ns.Write(value, 0, value.Length);
        }
    }

    public sealed class LoginServerTime_0X41 : NetPacket
    {
        public LoginServerTime_0X41() : base(0, 0x41)
        {
            ns.Write(Utility.CurrentTimeMilliseconds());
        }
    }
    public sealed class LoginCheck_0X10 : NetPacket
    {
        public LoginCheck_0X10(string UserID, bool LoginCheckOK) : base(0, 0x10)
        {
            if(LoginCheckOK)
            {
                ns.Write((int)0x00); //00 00 00 00 
            }
            else
            {
                ns.Write((int)0x0D); //0D 00 00 00  wrong pw or not exists
            }
            ns.Write((int)0x01); //01 00 00 00 servernum?
            ns.Write((int)0x09); //strTangID length
            ns.WriteASCIIFixedNoSize("strTangID", 9);
            ns.Write(UserID.Length);
            ns.WriteASCIIFixedNoSize(UserID, UserID.Length);
            if (LoginCheckOK)
            {
                ns.Write((int)0x00); //00 00 00 00
                                         //writer.Write((short)0x09);
                byte[] unknown = { 0xcc, 0x80, 0xc4, 0x94, 0xf6, 0xba, 0x8c, 0x12, 0xcc };
                ns.Write(unknown, 0);
            }
            else
            {
                ns.Write((int)0x01); //01 00 00 00
            }

        }
    }
    public sealed class LoginGenKey_0X12 : NetPacket
    {
        public LoginGenKey_0X12(byte[] key) : base(0, 0x12)
        {
            ns.Write(key,0,257);
        }
    }
    public sealed class LoginUserInfo_0X02 : NetPacket
    {
        public LoginUserInfo_0X02(Account User, byte last) : base(1, User.EncryptKey)
        {
            string nickName;
            long gamemoney, userexp, lastLoginTime;
            int playingTime, shuMP;

            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_login";
                cmd.Parameters.Add("servernum", MySqlDbType.Int32).Value = 1;
                cmd.Parameters.Add("userid", MySqlDbType.VarChar).Value = User.UserID;
                cmd.Parameters.Add("authnum", MySqlDbType.Int64).Value = -1;
                cmd.Parameters.Add("loginkey", MySqlDbType.Int32).Value = 123456;
                cmd.Parameters.Add("regpcroom", MySqlDbType.Int32).Value = -1;
                cmd.Parameters.Add("ip", MySqlDbType.VarChar).Value = User.LastIp;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                reader.Read();
                User.UserNum = Convert.ToInt32(reader["userindex"]);
                nickName = reader["nickName"].ToString();
                User.TR = Convert.ToInt64(reader["gamemoney"]);
                User.Exp = Convert.ToInt64(reader["userexp"]);
                playingTime = Convert.ToInt32(reader["playingTime"]);
                //lastLoginTime = ConvertToTimestamp(Convert.ToDateTime(reader["lastLoginTime"]));
                User.Attribute = Convert.ToInt32(reader["attribute"]);
                shuMP = Convert.ToInt32(reader["shuMP"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_login";
                    cmd.Parameters.Add("servernum", MySqlDbType.Int32).Value = 1;
                    cmd.Parameters.Add("userid", MySqlDbType.VarChar).Value = User.UserID;
                    cmd.Parameters.Add("authnum", MySqlDbType.Int64).Value = -1;
                    cmd.Parameters.Add("loginkey", MySqlDbType.Int32).Value = 123456;
                    cmd.Parameters.Add("regpcroom", MySqlDbType.Int32).Value = -1;
                    cmd.Parameters.Add("ip", MySqlDbType.VarChar).Value = User.LastIp;
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        User.UserNum = Convert.ToInt32(reader["userindex"]);
                        nickName = reader["nickName"].ToString();
                        User.TR = Convert.ToInt64(reader["gamemoney"]);
                        User.Exp = Convert.ToInt64(reader["userexp"]);
                        playingTime = Convert.ToInt32(reader["playingTime"]);
                        //lastLoginTime = ConvertToTimestamp(Convert.ToDateTime(reader["lastLoginTime"]));
                        User.Attribute = Convert.ToInt32(reader["attribute"]);
                        shuMP = Convert.ToInt32(reader["shuMP"]);
                        User.TopRank = Convert.ToInt32(reader["TopRank"]);
                    }
                }
            }

            ns.Write((byte)0x02); //op code
            ns.Write(User.Session);
            //ns.Write(0x3D3100A8); //A8 00 31 3D  usersession
            ns.Write((short)-1); //FF FF
            ns.Write(0x6457); //57 64 00 00
            ns.Write(User.TR);
            ns.Write(User.Exp);
            ns.Write(User.Attribute);
            ns.Write(playingTime);
            ns.Write(-1); //FF FF FF FF
            ns.Write((long)0);
            ns.Write(0x00068BB6671178CB);
            ns.Write(0x00068BB6671178CB);
            ns.Write(0x00068BB6671178CB);
            ns.Write((long)-1);//FF FF FF FF FF FF FF FF
            ns.Write(0x3628FFFF); //FF FF 28 36
            ns.Write(-1); //FF FF FF FF
            ns.Fill(19); //Fill 19 0x00
                         /* ns.Write((long)0);
                          ns.Write((long)0);
                          ns.Write((byte)0);
                          ns.Write((byte)0);
                          ns.Write((byte)0);*/
                         // long nowtime = ConvertToTimestamp(DateTime.Now);
            ns.Write(Utility.CurrentTimeMilliseconds());
            ns.Write((short)2); //port length
                                //ns.Write((short)9155); //port
            byte[] port = BitConverter.GetBytes((short)Conf.RelayPort).Reverse().ToArray();
            byte[] ipaddr = BitConverter.GetBytes(Utility.IPToInt(Conf.ServerIP)).Reverse().ToArray();
            IEnumerable<byte> ipport = port.Concat(ipaddr);
            ns.Write(ipport.ToArray(), 0, ipport.ToArray().Length);

            ns.Write(0x78485E2C); //2C 5E 48 78
            ns.Write(0);
            ns.Write(1);
            ns.Write((short)0x0100);
            ns.Fill(16);
            ns.Write(0x20);
            ns.Write(0x0259);//59 02    smartchannel 601
            ns.Write((long)0);
            ns.Write(shuMP);//shu mp
            int type = 0;
            long expireTime = 0;
            byte maxSavableCount = 0, maxReceivableCount = 0;
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
                type = Convert.ToInt32(reader["type"]);
                expireTime = Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"]));
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(type);
            ns.Write(expireTime);
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
                type = Convert.ToInt32(reader["type"]);
                expireTime = Convert.IsDBNull(reader["expireTime"]) ? 0L : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"]));
                maxSavableCount = Convert.ToByte(reader["maxSavableCount"]);
                maxReceivableCount = Convert.ToByte(reader["maxReceivableCount"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(maxSavableCount);
            ns.Write(maxReceivableCount);
            ns.Write(type);
            ns.Write(expireTime);

            /*ns.Write(0x04);
            ns.Write((long)0);
            ns.Write(0x1414);*/
            ns.Write(User.TopRank);
            ns.Write((byte)0);
            //ns.Fill(0x05);//Fill 15 0x00
            ns.Write(last); //end
        }
    }
    public sealed class LoginUserIndividualRecordGameRecord_FF2D02 : NetPacket
    {
        public LoginUserIndividualRecordGameRecord_FF2D02(Account User, byte last) : base(1, User.EncryptKey)
        {
            int fdPlayCount = 0, fdClearCount = 0, fdDistance = 0, fdFirst = 0, fdSecond = 0, fdThird = 0;

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_IndividualRecordGetGame";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    fdPlayCount = Convert.ToInt32(reader["fdPlayCount"]);
                    fdClearCount = Convert.ToInt32(reader["fdClearCount"]);
                    fdDistance = Convert.ToInt32(reader["fdDistance"]);
                    fdFirst = Convert.ToInt32(reader["fdFirst"]);
                    fdSecond = Convert.ToInt32(reader["fdSecond"]);
                    fdThird = Convert.ToInt32(reader["fdThird"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write((byte)0xFF);
            ns.Write((short)557); //0x22D  op code
            ns.Write(fdPlayCount);
            ns.Write(fdDistance);
            ns.Write(fdClearCount);
            ns.Write(fdFirst);
            ns.Write(fdSecond);
            ns.Write(fdThird);
            ns.Write(last);  //end

        }
    }
    public sealed class LoginUserIndividualRecordMiscellaneous_FF2E02 : NetPacket
    {
        public LoginUserIndividualRecordMiscellaneous_FF2E02(Account User, byte last) : base(1, User.EncryptKey)
        {
            int fdSClassCount = 0, fdAlchemistCount = 0, fdLicenseChallengeCount = 0, fdGrowthPotionCount = 0, fdNutritionPotionCount = 0,
                fdHarvestCount = 0, fdEnchantCount = 0, fdCleanCount = 0;

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_IndividualRecordGetMiscellaneous";
                cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    fdSClassCount = Convert.ToInt32(reader["fdSClassCount"]);
                    fdAlchemistCount = Convert.ToInt32(reader["ffdAlchemistCount"]);
                    fdLicenseChallengeCount = Convert.ToInt32(reader["fdLicenseChallengeCount"]);
                    fdGrowthPotionCount = Convert.ToInt32(reader["fdGrowthPotionCount"]);
                    fdNutritionPotionCount = Convert.ToInt32(reader["fdNutritionPotionCount"]);
                    fdHarvestCount = Convert.ToInt32(reader["fdHarvestCount"]);
                    fdEnchantCount = Convert.ToInt32(reader["fdEnchantCount"]);
                    fdCleanCount = Convert.ToInt32(reader["fdCleanCount"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write((byte)0xFF);
            ns.Write((short)558); //0x22E  op code
            ns.Write(fdSClassCount);
            ns.Write(fdAlchemistCount);
            ns.Write(fdLicenseChallengeCount);
            ns.Write(fdGrowthPotionCount);
            ns.Write(fdNutritionPotionCount);
            ns.Write(fdHarvestCount);
            ns.Write(fdEnchantCount);
            ns.Write(fdCleanCount);
            ns.Write(last);  //end
        }
    }
    public sealed class LoginFarm_GetMyFarmInfo_FF1D02 : NetPacket
    {
        public LoginFarm_GetMyFarmInfo_FF1D02(Account User, byte last) : base(1, User.EncryptKey)
        {
            int FarmIndex = 0, FarmTypeNum = 0, TotalVisitedCount = 0, TodaysVisitorCount = 0, farmExp = 0;
            string FarmName = "", MasterName = "", Password = "";
            long ExpireDateTime = 0, CreateDateTime = 0;

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_Farm_GetMyFarmInfo";
                cmd.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    FarmIndex = Convert.ToInt32(reader["FarmIndex"]);
                    FarmTypeNum = Convert.ToInt32(reader["FarmTypeNum"]);
                    FarmName = reader["FarmName"].ToString();
                    MasterName = reader["MasterName"].ToString();
                    ExpireDateTime = Convert.IsDBNull(reader["ExpireDateTime"]) ? 0x68BB6671178CB : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["ExpireDateTime"]));
                    CreateDateTime = Convert.IsDBNull(reader["CreateDateTime"]) ? 0x68BB6671178CB : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["CreateDateTime"]));
                    Password = reader["Password"].ToString();
                    TotalVisitedCount = Convert.ToInt32(reader["TotalVisitedCount"]);
                    TodaysVisitorCount = Convert.ToInt32(reader["TodaysVisitorCount"]);
                    farmExp = Convert.ToInt32(reader["farmExp"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write((byte)0xFF);
            ns.Write((short)466); //0x1D2  op code
            ns.Write((byte)0x18);
            ns.Write(0);
            ns.Write(FarmIndex); //D8 F7 11 00 FarmIndex?
            ns.Write(0); //???
            ns.Write(0); //???
            ns.Write(FarmTypeNum); // 01 00 00 00 FarmTypeNum?

            ns.WriteBIG5Fixed_intSize(FarmName);
            if (!User.noNickName)
            {
                ns.WriteBIG5Fixed_intSize(MasterName);
            }
            else
            {
                ns.Write(0);
            }
            ns.Write(ExpireDateTime);
            ns.Write(CreateDateTime);

            ns.Write((short)0x01);
            ns.Write(TotalVisitedCount); //TotalVisitedCount?
            ns.Write(TodaysVisitorCount); //TodaysVisitorCount?
            //ns.Write(50000); //farmExp?

            ns.Fill(17);
            ns.Write(last);  //end
        }
    }

    public sealed class LoginNickName_0X20 : NetPacket
    {
        public LoginNickName_0X20(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)32); //0x20  op code
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(User.NickName);
            ns.Fill(13);
            ns.Write(last);
        }
    }

    public sealed class LoginUDP_0X09 : NetPacket
    {
        public LoginUDP_0X09(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)9); //0x09  op code
            ns.Write(last);
        }
    }

    public sealed class LoginGetNickName_0X1C : NetPacket
    {
        public LoginGetNickName_0X1C(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)28); //0x1C  op code
            if (User.noNickName)
            {
                ns.Write(0x37); //0x37 eServerResult_ENTER_NEW_NICKNAME
                Console.WriteLine("NEED_ENTER_NEW_NICKNAME");
            }
            else
            {
                ns.Write(0);
                ns.WriteBIG5Fixed_intSize(User.NickName);
            }
            ns.Write(last);
        }
    }
    public sealed class LoginGetUserCash_0X179 : NetPacket
    {
        public LoginGetUserCash_0X179(Account User, byte last) : base(1, User.EncryptKey)
        {
            /*using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getUserCash";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                User.Cash = Convert.ToInt32(reader["cash"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }*/

            ns.Write((byte)0xFF);
            ns.Write((short)377); //opcode 0x179
            ns.Write(0);
            ns.Write(User.Cash);
            ns.Write(last);
        }
    }
    /*
    public sealed class GetCurrentAvatarInfo_0X6D : NetPacket
    {
        public GetCurrentAvatarInfo_0X6D(Account User, byte last) : base(1, User.EncryptKey)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCurrentAvatarInfo";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                ns.Write((byte)109); //0x6D  op code
                if (reader.HasRows)
                {
                    ns.Write(0); //3D 00 00 00 new ac?
                    reader.Read();
                    for (int i = 0; i < reader.FieldCount / 2; i++)
                    {
                        ns.Write(Convert.ToInt16(reader.GetValue(i)));
                        //ns.Write(BitConverter.GetBytes(Convert.ToInt16(reader.GetValue(i))), 0, sizeof(short));
                    }
                    ns.Fill(136); //0x88
                    for (int i = reader.FieldCount / 2; i < reader.FieldCount - 1; i++)
                    {
                        ns.Write(Convert.ToInt16(reader.GetValue(i)));
                        //ns.Write(BitConverter.GetBytes(Convert.ToInt16(reader.GetValue(i))), 0, sizeof(short));
                    }
                    ns.Fill(136);
                    ns.Write(Convert.ToInt16(reader["costumeMode"]));
                    ns.Write((byte)1);
                    //costumeMode = Convert.ToInt16(reader["costumeMode"]);
                }
                else
                {
                    ns.Write(0x3D); //3D 00 00 00 new ac?
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write(last);  //end
        }
    }
    */
    public sealed class GetCurrentAvatarInfo : NetPacket
    {
        public GetCurrentAvatarInfo(Account User, byte last) : base(1, User.EncryptKey)
        {

            ns.Write((byte)109); //0x6D  op code
            if (User.haveAvatar)
            {
                ns.Write(0); //3D 00 00 00 new ac?
                for (int i = 0; i < 15; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88); //0x88
                for (int i = 15; i < 30; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88);
                ns.Write(User.costumeMode);
                ns.Write((byte)1);
            }
            else
            {
                ns.Write(0x3D); //3D 00 00 00 new ac?
            }

            ns.Write(last);  //end
        }
    }

    public sealed class GetUserItemAttr : NetPacket
    {
        public GetUserItemAttr(Account User, byte last) : base(1, User.EncryptKey)
        {
            /*69 00 00 00 00 02 00 00 00 35 02 00 00 04 00 00
             00 02 00 29 5C 8F 3D 13 00 00 00 80 3F 2C 00 00
             00 48 43 32 00 9A 99 19 3E DC 1C 00 00 04 00 00
             00 01 00 29 5C 0F 3E 07 00 EC 51 B8 3D 09 00 00
             00 80 3E 2C 00 00 00 C8 42 00 00 00 00 01*/
            //69 00 00 00 00 00 00 00 00 00 00 00 00 04
            ns.Write((byte)0x69);
            ns.Write(0);
            ns.Write(User.UserItemAttr.Count);
            foreach (var item in User.UserItemAttr)
            {
                ns.Write(item.Key);
                ns.Write(item.Value.Count);
                foreach (var attr in item.Value)
                {
                    ns.Write(attr.Attr);
                    ns.Write(attr.AttrValue);
                }
            }
            ns.Write(0);
            ns.Write(last);  //end
        }
    }

    public sealed class Login_FF2E0505 : NetPacket
    {
        public Login_FF2E0505(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)1326); //opcode 0x52e  05
            ns.Write((byte)5);
            ns.Fill(15);
            ns.Write(last);  //end
        }
    }

    public sealed class Login_FF4D01_0X14E : NetPacket
    {
        public Login_FF4D01_0X14E(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)334); //opcode 0x14E
            ns.Write((byte)0);
            ns.Write(last);
        }
    }

    public sealed class Login_6E_0X6F : NetPacket
    {
        public Login_6E_0X6F(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)111); //opcode 0x6F
            ns.Fill(6);
            ns.Write(last);
        }
    }

    public sealed class GetCommunityAgentServer_0X74 : NetPacket
    {
        public GetCommunityAgentServer_0X74(Account User, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)116); //opcode 0x74
            ns.Write((byte)1);
            ns.Write((byte)1);
            ns.WriteASCIIFixed_intSize(Conf.ServerIP); //ip
            ns.Write(Conf.CommunityAgentServerPort); //port
            ns.Write(last);
        }
    }
    public sealed class LoginError : NetPacket
    {
        public LoginError(Account User, int ErrorCode, byte last) : base(1, User.EncryptKey)
        {
            ns.Write((byte)3);
            ns.Write(ErrorCode);
            ns.Write(last);
        }
    }
    public sealed class ServerNotReady : NetPacket
    {
        public ServerNotReady() : base(0, 0x3)
        {
            ns.Write(4);
            ns.Write((byte)0x1);
        }
    }
    public sealed class LoginBlackList : NetPacket
    {
        public LoginBlackList(Account User, long start, long end, byte last) : base(1, User.EncryptKey)
        {
            //03 0C 00 00 00 07 01 00 00 00 40 6D 7D 43 68 01 00 00 C8 50 85 43 68 01 00 00 04
            ns.Write((byte)0x3);
            ns.Write(0xC);
            ns.Write((byte)0x7);
            ns.Write(0x1);
            ns.Write(start);
            ns.Write(end);
            ns.Write(last);
        }
    }
}
