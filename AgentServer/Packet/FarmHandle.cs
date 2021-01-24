using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Holders;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Packet
{
    public class FarmHandle
    {
        public static void Handle_EnterFarm(ClientConnection Client, PacketReader packetReader, byte last)
        {
            //FF D1 01 00 13 EA 25 00 00 00 00 00 04
            Account User = Client.CurrentAccount;
            byte subopcode = packetReader.ReadByte();
            int farmindex = packetReader.ReadLEInt32();//13 EA 25 00 id??

            bool existRoom = Rooms.RoomList.Values.Any(rm => rm.FarmIndex == farmindex);
            NormalRoom room;
            if (!existRoom)
            {
                string FarmName = string.Empty;
                string Password = string.Empty;
                int RoomServerNum, RoomHandle;

                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_Farm_GetRoomInfo";
                    cmd.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = farmindex;
                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    if (reader.HasRows)
                    {
                        reader.Read();
                        FarmName = reader["FarmName"].ToString();
                        Password = reader["Password"].ToString();
                        RoomServerNum = Convert.ToInt32(reader["RoomServerNum"]);
                        RoomHandle = Convert.ToInt32(reader["RoomHandle"]);
                    }
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }

                room = new NormalRoom();
                room.setID(Rooms.RoomID);
                room.setName(FarmName);
                room.setPassword(Password);
                room.setItemType(0);
                room.setIsStepOn(false);
                room.setRoomKindID(75);
                room.setIsTeamPlay(0);
                room.FarmIndex = farmindex;
                room.Players.Add(User);
                if (RoomHolder.RoomKindInfos.TryGetValue(75, out var roomkindinfo))
                    room.setGameMode(roomkindinfo);

                //取得第一個位置 0
                User.RoomPos = (byte)(User.Attribute == 3 ? 100 : room.PosList.First());
                room.PosList.Remove(User.RoomPos);
                room.setRoomMasterIndex(User.RoomPos);


                //User.IsRoomMaster = true;
                User.InGame = true;
                User.CurrentRoomId = Rooms.RoomID;
                Rooms.RoomID += 1;

                room.StartAutoChangeRoomMaster();
                Rooms.AddRoom(room.ID, room);
            }
            else
            {
                room = Rooms.RoomList.Values.Where(rm => rm.FarmIndex == farmindex).FirstOrDefault();
                room.EnterRoom(Client, string.Empty, last);
            }
            Client.SendAsync(new GameRoom_Hex("A3", last));
            if (!existRoom)
                Client.SendAsync(new SendFarmInfo(User, room, farmindex, last));
            Client.SendAsync(new GameRoom_SendPlayerInfo(User, last));
            Client.SendAsync(new EnterFarm_0x2DD(last));
            Client.SendAsync(new EnterFarm_0x646(last));

        }

    }
}
