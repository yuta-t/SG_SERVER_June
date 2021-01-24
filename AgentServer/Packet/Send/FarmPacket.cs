using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet.Send
{
    public sealed class EnterFarm_0x646 : NetPacket
    {
        public EnterFarm_0x646(byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x646);
            //ns.Write(3);
            //ns.Write(0xE);
            ns.Write(0L);
            ns.Write(last);
        }
    }
    public sealed class SendFarmInfo : NetPacket
    {
        public SendFarmInfo(Account User, NormalRoom room, int farmindex, byte last)
        {
            int FarmTypeNum = 0, TotalVisitedCount = 0, TodaysVisitorCount = 0, farmExp = 0;
            string FarmName = "", MasterName = "", Password = "";
            long ExpireDateTime = 0, CreateDateTime = 0;

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_Farm_GetFarmInfo";
                cmd.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = farmindex;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
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

            ns.Write((byte)0xA5);
            ns.Write(0);
            ns.Write((byte)room.RoomKindID);
            ns.WriteBIG5Fixed_intSize(room.Name);
            ns.Write((byte)0x16);
            ns.WriteBIG5Fixed_intSize(room.Password);//password
            ns.Write(2);
            ns.Write(room.ID);
            ns.Write(User.RoomPos);
            ns.Write(room.MapNum);
            ns.Write(0x1E);
            ns.Write(room.IsTeamPlay);
            ns.Write(room.IsStepOn);
            ns.Write(room.ItemType);
            ns.Write(0);
            ns.Write(Utility.CurrentTimeMilliseconds());
            ns.Write(room.PosWeight);
            ns.Write(farmindex);
            ns.Write(0);
            ns.Write(1);
            ns.Write(FarmTypeNum);
            ns.WriteBIG5Fixed_intSize(FarmName);
            ns.WriteBIG5Fixed_intSize(MasterName);//Farm Master??
            ns.Write(ExpireDateTime);//farm expire date time
            ns.Write(CreateDateTime);//farm create date time
            ns.Write((short)1);
            ns.Write(TotalVisitedCount);//total count
            ns.Write(TodaysVisitorCount);//today visitor count
            ns.Write((byte)0);
            ns.Write(0x68BB6671178CB);//datetime null
            ns.Fill(0x10);
            ns.Write(last);
        }
    }
    public sealed class EnterFarm_0x2DD : NetPacket
    {
        public EnterFarm_0x2DD(byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2DD);
            ns.Write((byte)0);
            ns.Write(last);
        }
    }
    public sealed class SendFarmItemPos : NetPacket
    {
        public SendFarmItemPos(NormalRoom room, byte last)
        {
            ns.Write((byte)0xA4);
            ns.Write(1);//item count
            //--item start
            ns.Write(1);
            ns.Write(0xF);
            ns.Write(504811880L);//farmitemid?
            ns.Write((byte)12);//CellPosX
            ns.Write((byte)10);//CellPosY
            ns.Write((byte)0);
            ns.Write((byte)3);//CellWidth
            ns.Write((byte)3);//CellHeight
            ns.Write((byte)12);//BlockPosX
            ns.Write((byte)10);//BlockPosY
            ns.Write((byte)3);//BlockWidth
            ns.Write((byte)3);//BlockHeight
            ns.Write((byte)0);
            ns.Write(0x7848);
            ns.Write((short)0);
            ns.Write(44971);//itemid
            ns.Fill(0x14);
            ns.Write(0x169AABF203B);//set datetime
            //--item end
            ns.Write(last);
        }
    }
}
