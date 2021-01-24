using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using LocalCommons.Core;
using MySql.Data.MySqlClient;
using LocalCommons.Network;
using System.IO;
using System.Globalization;
using System.Data;

namespace AgentServer.Database
{
    public class DBInit
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(8);

        private static byte[] _Levels;
        private static byte[] _HackTools;
        private static byte[] _GameServerSetting;
        private static byte[] _SmartChannelModeInfo;
        private static byte[] _SmartChannelScheduleInfo;
        private static byte[] _RoomKindPenaltyInfo;
        public static byte[] Levels
        {
            get
            {
                return _Levels;
            }
            set
            {
                _Levels = value;
            }
        }
        public static byte[] HackTools
        {
            get
            {
                return _HackTools;
            }
            set
            {
                _HackTools = value;
            }
        }
        public static byte[] GameServerSetting
        {
            get
            {
                return _GameServerSetting;
            }
            set
            {
                _GameServerSetting = value;
            }
        }
        public static byte[] SmartChannelModeInfo
        {
            get
            {
                return _SmartChannelModeInfo;
            }
            set
            {
                _SmartChannelModeInfo = value;
            }
        }
        public static byte[] SmartChannelScheduleInfo
        {
            get
            {
                return _SmartChannelScheduleInfo;
            }
            set
            {
                _SmartChannelScheduleInfo = value;
            }
        }
        public static byte[] RoomKindPenaltyInfo
        {
            get
            {
                return _RoomKindPenaltyInfo;
            }
            set
            {
                _RoomKindPenaltyInfo = value;
            }
        }


        public static void initlevel_run()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte)0xFF);
            binaryWriter.Write((short)0x045E);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                try
                {
                    con.Open();
                    for (int i=1;i < 8;)
                    {
                        List<int> columnBuilder = new List<int>();
                        //Console.WriteLine(i);
                        binaryWriter.Write((short)i);
                        string cmd = "SELECT * FROM essenlevelinfo WHERE fdLevelKind = "+i;
                        var command = new MySqlCommand(cmd, con);
                        var reader = command.ExecuteReader();
                        int count = 0;
                        while (reader.Read())
                        {
                            columnBuilder.Add((int)reader["fdLevel"]);
                            columnBuilder.Add((int)reader["fdExp"]);
                            columnBuilder.Add((int)0);
                            count++;
                        }
                        command.Dispose();
                        reader.Close();
                        columnBuilder.Insert(0, (int)count);
                        binaryWriter.Write(columnBuilder.SelectMany(BitConverter.GetBytes).ToArray());
                        columnBuilder.Clear();
                        i++;
                        if (i == 4)
                            i += 3;
                    }
                    binaryWriter.Write((byte)0x02);
                    binaryWriter.Dispose();
                    binaryWriter.Close();
                    Levels = memoryStream.ToArray();
                    memoryStream.Dispose();
                    memoryStream.Close();
                    //Console.WriteLine(ByteArrayToString(Levels, Levels.Length));
                }
                catch (Exception e)
                {
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static void inithacktool_run()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte)0x05);
            binaryWriter.Write(0);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                try
                {
                    con.Open();
                    //List<int> columnBuilder = new List<int>();
                    //Console.WriteLine(i);
                    //binaryWriter.Write((short)i);
                    string cmd = "SELECT * FROM tblhackingtoolhash WHERE fdImmediately = 1";
                    var command = new MySqlCommand(cmd, con);
                    var reader = command.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        /*columnBuilder.Add(reader["fdHash"].ToString());
                        columnBuilder.Add((int)reader["fdExp"]);
                        columnBuilder.Add((int)0);*/
                        byte[] toolhash = Encoding.ASCII.GetBytes(reader["fdHash"].ToString().ToCharArray());
                        //Console.WriteLine(toolhash);
                        binaryWriter.Write(toolhash.Length);
                        binaryWriter.Write(toolhash);
                        binaryWriter.Write((byte)0x01);
                        count++;
                    }
                    command.Dispose();
                    reader.Close();
                    /*columnBuilder.Insert(0, (int)count);
                    binaryWriter.Write(columnBuilder.SelectMany(BitConverter.GetBytes).ToArray());
                    columnBuilder.Clear();*/
                    binaryWriter.Seek(1, SeekOrigin.Begin);
                    binaryWriter.Write(count);
                    binaryWriter.Seek(0, SeekOrigin.End);
                    binaryWriter.Write((byte)0x02);
                    binaryWriter.Dispose();
                    binaryWriter.Close();
                    HackTools = memoryStream.ToArray();
                    memoryStream.Dispose();
                    memoryStream.Close();
                    //Console.WriteLine(ByteArrayToString(HackTools));
                }
                catch (Exception e)
                {
                }
                finally
                {
                    con.Close();
                }
            }
        }

        //public static void initGameserverSetting_run()
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
        //    binaryWriter.Write((byte)0x15);
        //    binaryWriter.Write(0x00);
        //    using (var con = new MySqlConnection(Conf.Connstr))
        //    {
        //        try
        //        {
        //            con.Open();
        //            //List<int> columnBuilder = new List<int>();
        //            //Console.WriteLine(i);
        //            //binaryWriter.Write((short)i);
        //            string cmd = "SELECT * FROM tblserversettinginfo WHERE fdOnlyServerSetting = 0";
        //            var command = new MySqlCommand(cmd, con);
        //            var reader = command.ExecuteReader();
        //            int count = 0;
        //            while (reader.Read())
        //            {
        //                /*columnBuilder.Add(reader["fdHash"].ToString());
        //                columnBuilder.Add((int)reader["fdExp"]);
        //                columnBuilder.Add((int)0);*/
        //                byte[] fdKey = Encoding.ASCII.GetBytes(reader["fdKey"].ToString().ToCharArray());
        //                byte[] fdValue = Encoding.GetEncoding("Big5").GetBytes(reader["fdValue"].ToString().ToCharArray());
        //                //Console.WriteLine(toolhash);
        //                binaryWriter.Write(fdKey.Length);
        //                binaryWriter.Write(fdKey);
        //                binaryWriter.Write(fdValue.Length);
        //                binaryWriter.Write(fdValue);
        //                //binaryWriter.Write((byte)0x01);
        //                count++;
        //            }
        //            command.Dispose();
        //            reader.Close();
        //            /*columnBuilder.Insert(0, (int)count);
        //            binaryWriter.Write(columnBuilder.SelectMany(BitConverter.GetBytes).ToArray());
        //            columnBuilder.Clear();*/

        //            binaryWriter.Write((byte)0x02);
        //            binaryWriter.Seek(1, SeekOrigin.Begin);
        //            binaryWriter.Write(count);
        //            binaryWriter.Dispose();
        //            binaryWriter.Close();
        //            GameServerSetting = memoryStream.ToArray();
        //            memoryStream.Dispose();
        //            memoryStream.Close();
        //            //Console.WriteLine(ByteArrayToString(GameServerSetting));
        //        }
        //        catch (Exception e)
        //        {
        //        }
        //        finally
        //        {
        //            con.Close();
        //        }
        //    }
        //}

        public static void initSmartChannelModeInfo_run()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte)0xFF);
            binaryWriter.Write((short)1297);  //11 05
            binaryWriter.Write((int)0);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                try
                {
                    con.Open();
                    //List<int> columnBuilder = new List<int>();
                    //Console.WriteLine(i);
                    //binaryWriter.Write((short)i);
                    string cmd = "SELECT * FROM  essensmartchannelinfo WHERE fdUse = 1";
                    var command = new MySqlCommand(cmd, con);
                    var reader = command.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        /*columnBuilder.Add(reader["fdHash"].ToString());
                        columnBuilder.Add((int)reader["fdExp"]);
                        columnBuilder.Add((int)0);*/
                        /*yte[] fdKey = Encoding.ASCII.GetBytes(reader["fdModeNum"].ToString().ToCharArray());
                        byte[] fdValue = Encoding.GetEncoding("Big5").GetBytes(reader["fdValue"].ToString().ToCharArray());*/
                        //Console.WriteLine(toolhash);
                        binaryWriter.Write(Convert.ToByte(reader["fdModeNum"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdSlotNum"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdType"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdMaxUserNum"]));
                        binaryWriter.Write(Convert.ToInt32(reader["fdMapNum"]));
                        binaryWriter.Write(Convert.ToInt32(reader["fdMapGroupNum"]));
                        binaryWriter.Write(Convert.ToInt32(reader["fdItemMode"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdTeamPlayMode"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdSteppingMode"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdMinLevel"]));
                        binaryWriter.Write(Convert.ToByte(reader["fdMaxLevel"]));
                        binaryWriter.Write(Convert.ToSingle(reader["fdBonusExp"]));
                        binaryWriter.Write(Convert.ToSingle(reader["fdBonusGameMoney"]));
                        binaryWriter.Write(Convert.ToInt32(reader["fdMinUserNum"]));
                        //binaryWriter.Write((byte)0x01);
                        count++;
                    }
                    command.Dispose();
                    reader.Close();
                    /*columnBuilder.Insert(0, (int)count);
                    binaryWriter.Write(columnBuilder.SelectMany(BitConverter.GetBytes).ToArray());
                    columnBuilder.Clear();*/

                    binaryWriter.Write((byte)0x02);
                    binaryWriter.Seek(3, SeekOrigin.Begin);
                    binaryWriter.Write(count);
                    binaryWriter.Dispose();
                    binaryWriter.Close();
                    SmartChannelModeInfo = memoryStream.ToArray();
                    memoryStream.Dispose();
                    memoryStream.Close();
                    //Console.WriteLine(ByteArrayToString(SmartChannelModeInfo));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static void initSmartChannelScheduleInfo_run()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte)0xFF);
            binaryWriter.Write((short)1298);  //12 05
            binaryWriter.Write((int)0);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                try
                {
                    con.Open();
                    //List<int> columnBuilder = new List<int>();
                    //Console.WriteLine(i);
                    //binaryWriter.Write((short)i);
                    string cmd = "SELECT * FROM  essensmartchannelschedule";
                    var command = new MySqlCommand(cmd, con);
                    var reader = command.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        /*columnBuilder.Add(reader["fdHash"].ToString());
                        columnBuilder.Add((int)reader["fdExp"]);
                        columnBuilder.Add((int)0);*/
                        /*yte[] fdKey = Encoding.ASCII.GetBytes(reader["fdModeNum"].ToString().ToCharArray());
                        byte[] fdValue = Encoding.GetEncoding("Big5").GetBytes(reader["fdValue"].ToString().ToCharArray());*/
                        //Console.WriteLine(toolhash);
                        binaryWriter.Write(ConvertToTimestamp(Convert.ToDateTime(reader["fdStartTime"])));
                        binaryWriter.Write(ConvertToTimestamp(Convert.ToDateTime(reader["fdEndTime"])));
                        //binaryWriter.Write((byte)0x01);
                        count++;
                    }
                    //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    //binaryWriter.Write(timestamp);
                    command.Dispose();
                    reader.Close();
                    /*columnBuilder.Insert(0, (int)count);
                    binaryWriter.Write(columnBuilder.SelectMany(BitConverter.GetBytes).ToArray());
                    columnBuilder.Clear();*/

                    binaryWriter.Write((byte)0x02);
                    binaryWriter.Seek(3, SeekOrigin.Begin);
                    binaryWriter.Write(count);
                    binaryWriter.Dispose();
                    binaryWriter.Close();
                    SmartChannelScheduleInfo = memoryStream.ToArray();
                    memoryStream.Dispose();
                    memoryStream.Close();
                    //Console.WriteLine(ByteArrayToString(SmartChannelScheduleInfo));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static void initRoomKindPenaltyInfo_run()
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write((byte)0xFF);
            binaryWriter.Write((short)1301);  //15 05
            binaryWriter.Write((int)0);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                try
                {
                    con.Open();
                    //List<int> columnBuilder = new List<int>();
                    //Console.WriteLine(i);
                    //binaryWriter.Write((short)i);
                    string cmd = "SELECT * FROM essenpenaltyinfo";
                    var command = new MySqlCommand(cmd, con);
                    var reader = command.ExecuteReader();
                    int count = 0;
                    while (reader.Read())
                    {
                        /*columnBuilder.Add(reader["fdHash"].ToString());
                        columnBuilder.Add((int)reader["fdExp"]);
                        columnBuilder.Add((int)0);*/
                        /*yte[] fdKey = Encoding.ASCII.GetBytes(reader["fdModeNum"].ToString().ToCharArray());
                        byte[] fdValue = Encoding.GetEncoding("Big5").GetBytes(reader["fdValue"].ToString().ToCharArray());*/
                        //Console.WriteLine(toolhash);
                        binaryWriter.Write((int)reader["fdRoomKindID"]);
                        binaryWriter.Write((int)reader["fdRestrictTime"]*1000);
                        //binaryWriter.Write((byte)0x01);
                        count++;
                    }
                    //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    //binaryWriter.Write(timestamp);
                    command.Dispose();
                    reader.Close();
                    /*columnBuilder.Insert(0, (int)count);
                    binaryWriter.Write(columnBuilder.SelectMany(BitConverter.GetBytes).ToArray());
                    columnBuilder.Clear();*/

                    binaryWriter.Write((byte)0x02);
                    binaryWriter.Seek(3, SeekOrigin.Begin);
                    binaryWriter.Write(count);
                    binaryWriter.Dispose();
                    binaryWriter.Close();
                    RoomKindPenaltyInfo = memoryStream.ToArray();
                    memoryStream.Dispose();
                    memoryStream.Close();
                    //Console.WriteLine(ByteArrayToString(RoomKindPenaltyInfo));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        public static void test()
        {
            string fdNickname = "";
            //byte[] fdValue = { };
            byte[] compiled;

            PacketWriter writer = new PacketWriter();
            writer = PacketWriter.CreateInstance(341, true);
            writer.Write((byte)109); //0x6D  op code
            writer.Write(0);
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_getCurrentAvatarInfo";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = 1;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.HasRows)
                {
                    reader.Read();
                    for (int i = 0; i < reader.FieldCount / 2; i++)
                    {
                        writer.Write(BitConverter.GetBytes(Convert.ToInt16(reader.GetValue(i))), 0, sizeof(short));
                    }
                    writer.Fill(136);
                    for (int i = reader.FieldCount / 2; i < reader.FieldCount - 1; i++)
                    {
                        writer.Write(BitConverter.GetBytes(Convert.ToInt16(reader.GetValue(i))), 0, sizeof(short));
                    }
                    writer.Fill(136);
                    writer.Write(Convert.ToInt16(reader["costumeMode"]));
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }

            writer.Write((byte)1);

            compiled = writer.ToArray();
            PacketWriter.ReleaseInstance(writer);

            //Console.WriteLine(fdNickname);
            Console.WriteLine(ByteArrayToString(compiled));

        }

        public static void test2()
        {
            byte last = 2;
            PacketWriter writer = new PacketWriter();
            for (int i = 0; i < 22; i++) {
                short count = 0;
                byte[] compiled;
                using (var con = new MySqlConnection(Conf.Connstr))
                {
                    con.Open();
                    var cmd = new MySqlCommand(string.Empty, con);
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_getCharacterAvatarItem";
                    cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = 1;
                    cmd.Parameters.Add("pcharacter", MySqlDbType.Int32).Value = i;
                    cmd.Parameters.Add("position", MySqlDbType.Int32).Value = -1;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        writer = PacketWriter.CreateInstance(16, true);
                        writer.Write((byte)0xFF);
                        writer.Write((short)322); //0x142  op code
                        writer.Write((short)i); //character
                        writer.Write(-1); //FF FF FF FF
                        writer.Fill(3);
                        writer.Write((short)0); //count
                        while (reader.Read())
                        {
                            writer.Write(0x9CC1D0); //D0 C1 9C 00
                            writer.Write(Convert.ToInt16(reader["character"]));
                            writer.Write(Convert.ToUInt16(reader["position"]));
                            writer.Write(Convert.ToInt16(reader["kind"]));
                            writer.Write(Convert.ToInt32(reader["itemdescnum"])); //4 bytes
                            writer.Write(Convert.IsDBNull(reader["expireTime"]) ? 0 : ConvertToTimestamp(Convert.ToDateTime(reader["expireTime"])));
                            writer.Write(ConvertToTimestamp(Convert.ToDateTime(reader["gotDateTime"])));
                            writer.Write(Convert.ToInt32(reader["count"])); //4 bytes
                            writer.Write(Convert.ToInt32(reader["exp"]));
                            writer.Write((byte)0);
                            writer.Write(Convert.ToBoolean(reader["using"]));
                            count++;
                        }
                        writer.Write(last);  //end
                        writer.Seek(12, SeekOrigin.Begin);
                        writer.Write(count);

                        compiled = writer.ToArray();
                        PacketWriter.ReleaseInstance(writer);
                        Console.WriteLine(ByteArrayToString(compiled));
                    }
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                }
            }

        }


        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            int max = 0;
            foreach (byte b in ba)
            {
                //max += 1;
                if (max < ba.Length)
                {
                    hex.AppendFormat("{0:x2}", b.ToString("X2") + " ");
                }
                else
                {
                    break;
                }
                max += 1;
            }
            return hex.ToString();
        }

        private static long ConvertToTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalMilliseconds;
        }
    }
}
