//using CommunityAgentServer.Holders;
using CommunityAgentServer.Network.Connections;
using CommunityAgentServer.Packet.Send;
using CommunityAgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
//using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace CommunityAgentServer.Packet
{
    public class CommonHandle
    {
        public static void Handle_0x02(ClientConnection Client, PacketReader reader)
        {
            //Console.WriteLine(Utility.ByteArrayToString(reader.Buffer));
            /*02 2E 00 00 00 31 2E 31 36 2E 31 34 2E 31 5F 72 65 6C 5F 34 
             35 61 38 36 36 37 33 36 35 65 31 66 32 62 35 33 30 64 64 36 
             63 66 36 35 31 64 66 35 61 64 61 04 FF 4D 06 08 00 00 00 CE 
             78 A7 41 A6 D1 A5 C0 FF FF FF FF 00 00 00 00*/
            int hashlen = reader.ReadLEInt32();
            string hash = reader.ReadBig5StringSafe(hashlen);

            reader.ReadByte(); //04
            reader.ReadByte(); //FF
            reader.ReadLEInt16(); //opcode2
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            Account nCurrent = new Account
            {
                NickName = nickname,
                Connection = Client
            };
            Client.CurrentAccount = nCurrent;
            ClientConnection.CurrentAccounts.TryAdd(nickname, Client.CurrentAccount);

            Client.SendAsync(new NP_Hex("03"));
        }

        public static void Handle_0x07(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            //int mark = reader.Offset;
            //int remainsize = reader.Size - reader.Offset;
            //byte[] remain = reader.ReadByteArray(remainsize);
            //reader.Offset = mark;
            short sendsize = reader.ReadLEInt16(); //11 00
            //short unk2 = reader.ReadLEInt16(); //11 00
            //byte unk3 = reader.ReadByte(); //0B
            //int nicknamelen2 = reader.ReadLEInt32();
            //string nickname2 = reader.ReadBig5StringSafe(nicknamelen2);
            byte[] sentbyte = reader.ReadByteArray(sendsize);

            if (ClientConnection.CurrentAccounts.TryGetValue(nickname, out Account TargetAC))
            {
                TargetAC.Connection.SendAsync(new NP_0x07(User.NickName, sentbyte));
            }
            else
            {
                 Client.SendAsync(new NP_0x08(nickname, sentbyte));
            }

        }

        public static void Handle_0x0E(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            byte subop = reader.ReadByte();
            if (subop == 0x00)
            {
                Client.SendAsync(new NP_Hex("0F0000004B8ADA66B68B06007299310000000000"));
                                            //0f0000004b8ada66b68b06004b30490000000000
            }
            else if (subop == 0x02)
            {
                /*0E 02 02 00 00 00 01 01 03 00 03 00 00 00 02 02
                 00 00 00 34 38 04 05 00 00 00 34 34 31 32 33 05 
                 05 00 00 00 34 34 36 35 34 00*/
                reader.ReadLEInt32(); //02 00 00 00 count
                reader.ReadByte(); //sex code
                byte sex = reader.ReadByte();
                reader.ReadByte(); //location code
                byte location = reader.ReadByte();

                reader.ReadLEInt32(); //03 00 00 00 count
                reader.ReadByte(); //age code
                int agelen = reader.ReadLEInt32();
                if (agelen > 2)
                {
                    Client.SendAsync(new SetMyProfileFail());
                    return;
                }
                string age = reader.ReadBig5StringSafe(agelen);
                reader.ReadByte(); //job code
                int joblen = reader.ReadLEInt32();
                string job = reader.ReadBig5StringSafe(joblen);
                reader.ReadByte(); //hobby code
                int hobbylen = reader.ReadLEInt32();
                string hobby = reader.ReadBig5StringSafe(joblen);
                //Console.WriteLine("sex: {0}, location: {1}, age: {2}, job: {3}, hobby: {4}", sex, location, age, job, hobby);

                ProfileSet(User, sex, age, location, job, hobby);

                Client.SendAsync(new SetMyProfile());
            }
            else if (subop == 0x03)
            {
                //Client.SendAsync(new NP_Hex("0F03000000000000000000")); //0F0300020000000101030003000000020100000031040100000032050100000033"));
                Client.SendAsync(new GetMyProfile(User));
            }
            else if (subop == 0x0B)
            {
                /*3a000f0b0015b700000000000000000200000001010309030000000
                 2020000003230040700000052654e65777777050700000052654e65777777*/
                byte unk1 = reader.ReadByte(); //15   supopcode2?
                int niknamelen = reader.ReadLEInt32();
                string nikname = reader.ReadBig5StringSafe(niknamelen);

                //Console.WriteLine("getprofile");
                Client.SendAsync(new GetProfileByNickName(nikname, unk1));
            }
        }

        private static bool ProfileSet(Account User, int Sex, string Age, int Location, string Job, string Hobby)
        {
            bool ret = false;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_profileSet";
                cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = User.NickName;
                cmd.Parameters.Add("Sex", MySqlDbType.Int32).Value = Sex;
                cmd.Parameters.Add("Age", MySqlDbType.VarString).Value = Age;
                cmd.Parameters.Add("Location", MySqlDbType.Int32).Value = Location;
                cmd.Parameters.Add("Job", MySqlDbType.VarString).Value = Job;
                cmd.Parameters.Add("Hobby", MySqlDbType.VarString).Value = Hobby;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                reader.Read();
                ret = Convert.ToByte(reader["retval"]) == 0 ? true : false;
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

            return ret;
        }

    }

}
