using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;
//using CommunityAgentServer.Database;
using CommunityAgentServer.Structuring;
using CommunityAgentServer.Network.Connections;
//using MySql.Data.MySqlClient;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;

namespace CommunityAgentServer.Packet.Send
{
    public sealed class NP_Hex : NetPacket
    {
        public NP_Hex(string value) : base(3, 0)
        {
            ns.WriteHex(value);
        }
    }
    public sealed class NP_Byte : NetPacket
    {
        public NP_Byte(byte[] value) : base(3, 0)
        {
            ns.Write(value, 0, value.Length);
        }
    }

    public sealed class NP_0x07 : NetPacket
    {
        public NP_0x07(string nickname, byte[] remain) : base(3, 0)
        {
            ns.Write((byte)0x07); //op
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(remain, 0);
        }
    }

    public sealed class NP_0x08 : NetPacket
    {
        public NP_0x08(string nickname, byte[] remain) : base(3, 0)
        {
            ns.Write((byte)0x08); //op
            ns.WriteBIG5Fixed_intSize(nickname);
            ns.Write(remain, 0);
        }
    }

    public sealed class GetMyProfile : NetPacket
    {
        public GetMyProfile(Account User) : base(3, 0)
        {
            /*0F 03 00 0A 00 00 00 01 01 03 00 07 01 08 01 09 01 
             0A 01 0B 01 14 01 15 01 18 01 05 00 00 00 02 01 00
             00 00 31 04 06 00 00 00 36 35 34 33 32 31 05 06 00
             00 00 31 32 33 34 35 36 06 03 00 00 00 31 36 30 17
             06 00 00 00 3F 3F 3F 20 3F 3F*/
            byte sex = 0;
            byte localtion = 0;
            string age = string.Empty;
            string job = string.Empty;
            string hobby = string.Empty;
            bool hasRows = false;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_profileGet";
                cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = User.NickName;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    sex = Convert.ToByte(reader["Sex"]);
                    localtion = Convert.ToByte(reader["Location"]);
                    age = reader["Age"].ToString();
                    job = reader["Job"].ToString();
                    hobby = reader["Hobby"].ToString();
                    hasRows = true;
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write((byte)0x0F);
            ns.Write((short)0x03);
            if (hasRows)
            {
                ns.Write(0x0A); //count
                ns.Write((byte)0x1); //1,"sex"
                ns.Write(sex);
                ns.Write((byte)0x3); //3,"location"
                ns.Write(localtion);
                ns.Write((byte)0x7); //7,"bodyType"
                ns.Write((byte)0);
                ns.Write((byte)0x8); //8,"style"
                ns.Write((byte)0);
                ns.Write((byte)0x9); //9,"bloodGroup"
                ns.Write((byte)0);
                ns.Write((byte)0xA); //10,"constellation"
                ns.Write((byte)0);
                ns.Write((byte)0xB); //11,"religion"
                ns.Write((byte)0);
                ns.Write((byte)0x14); //20,"isFriendSearch"
                ns.Write((byte)0);
                ns.Write((byte)0x15); //21,"isDateSearch"
                ns.Write((byte)0);
                ns.Write((byte)0x18); //24,"purpose"
                ns.Write((byte)1);
                ns.Write(0x05); //count2
                ns.Write((byte)0x2); //2,"age"
                ns.WriteBIG5Fixed_intSize(age);
                ns.Write((byte)0x4); //4,"job"
                ns.WriteBIG5Fixed_intSize(job);
                ns.Write((byte)0x5); //5,"hobby"
                ns.WriteBIG5Fixed_intSize(hobby);
                ns.Write((byte)0x6); //6,"height"
                ns.WriteBIG5Fixed_intSize("160");
                ns.Write((byte)0x17); //23,"anotherName"
                ns.WriteBIG5Fixed_intSize("name");
            }
            else
            {
                ns.Write(0L);
            }

        }
    }
    public sealed class SetMyProfile : NetPacket
    {
        public SetMyProfile() : base(3, 0)
        {
            //0F 02 00
            ns.Write((byte)0x0F);
            ns.Write((short)0x02);
        }
    }

    public sealed class SetMyProfileFail : NetPacket
    {
        public SetMyProfileFail() : base(3, 0)
        {
            //0F 02 00
            ns.Write((byte)0x0F);
            ns.Write((byte)0x02);
            ns.Write((byte)0x01);
        }
    }

    public sealed class GetProfileByNickName : NetPacket
    {
        public GetProfileByNickName(string NickName, short unk1) : base(3, 0)
        {
            /*0F 0B 00 15 00 00 00 00 00 00 00 00 00 02 00 00 00 
             01 01 03 00 03 00 00 00 02 02 00 00 00 31 31 04 0A
             00 00 00 68 35 33 31 36 35 34 33 32 31 05 0B 00 00
             00 6A 35 77 33 36 31 32 33 34 35 36*/
            byte sex = 0;
            byte localtion = 0;
            string age = string.Empty;
            string job = string.Empty;
            string hobby = string.Empty;
            bool hasRows = false;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_profileGet";
                cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = NickName;
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    sex = Convert.ToByte(reader["Sex"]);
                    localtion = Convert.ToByte(reader["Location"]);
                    age = reader["Age"].ToString();
                    job = reader["Job"].ToString();
                    hobby = reader["Hobby"].ToString();
                    hasRows = true;
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            ns.Write((byte)0x0F);
            ns.Write((short)0x0B);
            ns.Write(unk1);
            ns.Write(0L);
            if (hasRows)
            {
                ns.Write(0x02); //count
                ns.Write((byte)0x1); //1,"sex"
                ns.Write(sex);
                ns.Write((byte)0x3); //3,"location"
                ns.Write(localtion);
                ns.Write(0x03); //count2
                ns.Write((byte)0x2); //2,"age"
                ns.WriteBIG5Fixed_intSize(age);
                ns.Write((byte)0x4); //4,"job"
                ns.WriteBIG5Fixed_intSize(job);
                ns.Write((byte)0x5); //5,"hobby"
                ns.WriteBIG5Fixed_intSize(hobby);
            }
            else
            {
                ns.Write(0L);
            }

        }
    }
}
