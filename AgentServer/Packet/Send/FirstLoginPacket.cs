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

    public sealed class FirstLoginSetNewNickNameOK_0X4C : NetPacket
    {
        public FirstLoginSetNewNickNameOK_0X4C(Account User, byte last)
        {
            ns.Write((byte)76); //0x4C  op code
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(User.NickName);
            ns.Write(last); //end
        }
    }

    public sealed class FirstLoginSetNewNickNameFail_0X4C : NetPacket
    {
        public FirstLoginSetNewNickNameFail_0X4C(Account User, byte last)
        {
            ns.Write((byte)76); //0x4C  op code
            ns.Write(0x38); //0x38
            ns.Write(last); //end
        }
    }

    public sealed class FirstLoginMakeStartCharacter_67_0X64 : NetPacket
    {
        public FirstLoginMakeStartCharacter_67_0X64(Account User, int charid, byte last)
        {
            ns.Write((byte)103); //0x67  op code
            ns.Write(0);
            ns.Write((byte)0x64); //0x64 sub op code?
            ns.Fill(6);
            int countpos = (int)ns.Position;
            ns.Write((short)0); //count
            short count = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_selectStartCharacter";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("charKind", MySqlDbType.Int32).Value = charid;
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ns.Write(0x9CC1D0); //D0 C1 9C 00
                    ns.Write(Convert.ToInt16(reader["character"]));
                    ns.Write(Convert.ToUInt16(reader["position"]));
                    ns.Write(Convert.ToInt16(reader["kind"]));
                    ns.Write(Convert.ToInt32(reader["itemdescnum"])); //4 bytes
                    ns.Write(Convert.IsDBNull(reader["expireTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                    ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                    ns.Write(Convert.ToInt32(reader["count"])); //4 bytes
                    ns.Write(Convert.ToInt32(reader["exp"]));
                    ns.Write((byte)0);
                    ns.Write(Convert.ToBoolean(reader["using"]));
                    count++;
                }
                ns.Write((short)0);//  daily buff count?
                ns.Write(last);  //end
                ns.Seek(countpos, SeekOrigin.Begin);
                ns.Write(count);

                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
    }

    public sealed class FirstLoginMakeStartCharacterOK_0X71 : NetPacket
    {
        public FirstLoginMakeStartCharacterOK_0X71(Account User, byte last)
        {
            ns.Write((byte)113); //0x71  op code
            ns.Write(0);
            ns.Write(last);
        }
    }
}
