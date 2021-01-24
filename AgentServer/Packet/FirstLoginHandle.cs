using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using AgentServer.Database;

namespace AgentServer.Packet
{
    public class FirstLoginHandle
    {
        public static void Handle_SelectStartCharacter(ClientConnection Client, PacketReader reader, byte last)
        {
            try
            {
                Account User = Client.CurrentAccount;
                int charid = reader.ReadLEInt16();
                //MakeStartCharacter(charid, last);
                Client.SendAsync(new FirstLoginMakeStartCharacter_67_0X64(User, charid, last));
                //MakeStartCharacterOK(last);
                Client.SendAsync(new FirstLoginMakeStartCharacterOK_0X71(User, last));
                //GetCurrentAvatarInfo(last);
                //Client.SendAsync(new GetCurrentAvatarInfo_0X6D(User, last));
                User.CurrentAvatarInfo.AddRange(Enumerable.Repeat((ushort)0, 45));
                LoginHandle.updateCurrentAvatarInfo(User);
                //LoginHandle.getCurrentAvatarInfo(User);
                Client.SendAsync(new GetCurrentAvatarInfo(User, last));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public static void Handle_SetNewNickName(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            bool NickNameOK = false;
            //preader.ReadByte(); //op code 0x4B
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            reader.Clear();
            Console.WriteLine(nickname);

            try
            {
                NickNameOK = SetNewNickNameCheck(Client, nickname, last);
                if (NickNameOK)
                {
                    Client.SendAsync(new LoginGetNickName_0X1C(User, last));
                    CreateFarm(User.UserNum);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private static bool SetNewNickNameCheck(ClientConnection Client, string NickName, byte last)
        {
            Account User = Client.CurrentAccount;
            int result = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_setNickname";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = NickName;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                result = Convert.ToInt32(reader["result"]);
                if (result == 0)
                {
                    User.noNickName = false;
                    User.NickName = NickName;
                    Client.SendAsync(new FirstLoginSetNewNickNameOK_0X4C(User, last));
                }
                else if (result == 1)
                {
                    Client.SendAsync(new FirstLoginSetNewNickNameFail_0X4C(User, last));
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            if (result == 0)
                return true;
            else
                return false;
        }

        private static void CreateFarm(int usernum)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "	usp_Farm_CreateFarm";
                cmd.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = usernum;
                cmd.Parameters.Add("pFarmName", MySqlDbType.VarChar).Value = "";
                cmd.Parameters.Add("pCheckFarmPeriod", MySqlDbType.Int32).Value = 0;
                MySqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }

    }
}
