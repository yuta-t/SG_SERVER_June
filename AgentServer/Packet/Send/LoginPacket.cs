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
using AgentServer.Structuring.Item;
using LocalCommons.Cryptography;

namespace AgentServer.Packet.Send
{
    public sealed class NP_Hex : NetPacket
    {
        public NP_Hex(Account User, string value)
        {
            ns.WriteHex(value);
            Console.WriteLine("NP_Hex:{0}",value);
        }
    }
    public sealed class NP_Byte : NetPacket
    {
        public NP_Byte(Account User, byte[] value)
        {
            ns.Write(value, 0, value.Length);
        }
    }

    public sealed class LoginServerTime_0X41 : NetPacket
    {
        public LoginServerTime_0X41(Account User)
        {
            ns.Write((byte)0x41);
            ns.Write(Utility.CurrentTimeMilliseconds());
        }
    }
    public sealed class LoginGlobalID_0x34F_00 : NetPacket
    {
        public LoginGlobalID_0x34F_00(Account User)
        {
            ns.Write((byte)0x00);
            ns.Write((short)847); //0x34F  op code
            ns.Write((short)0);
            ns.Write(User.GlobalID);
            Console.WriteLine(User.GlobalID);
            Console.WriteLine(" ");
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
        }
    }
    public sealed class AccountResult_0x371_00 : NetPacket
    {
        public AccountResult_0x371_00(bool LoginCheckOK)
        {
            if(LoginCheckOK)
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write(0);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
            }
            else
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write(0);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x03);
                ns.Write((byte)0x00);
            }

            //ns.Write((short)0);
            /*ns.Write((byte)0x10);
            if (LoginCheckOK)
                ns.Write(0); //00 00 00 00 
            else
                ns.Write(0x0D); //0D 00 00 00  wrong pw or not exists
            ns.Write(0x01); //01 00 00 00 servernum?
            //ns.Write((int)0x09); //strTangID length
            //ns.WriteASCIIFixedNoSize("strTangID", 9);
            ns.WriteBIG5Fixed_intSize("strTangID");
            //ns.Write(UserID.Length);
            //ns.WriteASCIIFixedNoSize(UserID, UserID.Length);
            ns.WriteBIG5Fixed_intSize(UserID);
            if (LoginCheckOK)
            {
                ns.Write(0); //00 00 00 00
                byte[] unknown = { 0xcc, 0x80, 0xc4, 0x94, 0xf6, 0xba, 0x8c, 0x12, 0xcc };
                ns.Write(unknown, 0);
            }
            else
                ns.Write(0x1); //01 00 00 00
            */
        }
    }
    public sealed class LoginGameID_0x371_01 : NetPacket
    {
        public LoginGameID_0x371_01(Account User)
        {
            Console.WriteLine("GameID: {0}", User.GameID);
            if(User.GameID == null)
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x01);
                ns.Write(0);
                ns.Write(0);
                ns.Write((byte)0x00);
            }
            else
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x01);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x02);
                ns.Write((byte)0x01);
                ns.Write((byte)0x01);
                //ns.WriteBIG5FixedWithSize(User.GameID);
                //ns.Write((byte)User.GameID.Length);
                //ns.WriteASCIIFixedNoSize(User.GameID, User.GameID.Length);
            }
        }
    }
    public sealed class FirstLoginResult_0x371_02 : NetPacket
    {
        public FirstLoginResult_0x371_02(Account User, bool AccountCheckOK)
        {
            if (AccountCheckOK)
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x02);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x01);
                ns.Write((byte)0x00);
            }
            else
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x02);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
            }
        }
    }
    public sealed class LoginDisplayCharacter_0x371_03 : NetPacket
    {
        /*private static byte[] encodeMultiBytes (int src)
        {
            int result = 0;
            int remainder = 0;

            if(src > 60000)
            {
                byte[] cal = new byte[3];
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        cal[2] = (byte)(src & 0x7F);
                        result = src - cal[2];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[1] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 2)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else if (src > 127)
            {
                byte[] cal = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        cal[1] = (byte)(src & 0x7F);
                        result = src - cal[1];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else
            {
                byte[] cal = new byte[1];
                cal[0] = (byte)src;
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
        }*/
        private static byte[] encodedHairClump(byte[] clump)
        {
            //Console.WriteLine(BitConverter.ToInt32(User.CharacterDecodedHairClump1, 0));
            //Console.WriteLine(" ");
            byte[] cal = new byte[5];
            int result = 0;
            int src = 0;
            int remainder = 0;

            if (clump[0] == 0xFF)
            {
                byte[] noCal = new byte[1];
                noCal[0] = 0x00;
                return noCal;
            }
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    src = BitConverter.ToInt32(clump, 0);
                    cal[4] = (byte)(src & 0x7F);
                    result = src - cal[4];
                }
                else if (i == 1)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[3] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 2)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[2] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 3)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[1] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 4)
                {
                    cal[0] = 0x80;
                    /*src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[0] = (byte)(remainder + 0x80);*/
                }
            }
            //Console.WriteLine("Encoded Hair Clump: {0}", Utility.ByteArrayToString(cal));
            //Console.WriteLine(" ");
            return cal;
        }
        public LoginDisplayCharacter_0x371_03(Account User, int CharacterCount)
        {
            if (CharacterCount == 0)
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x03);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x02);
                ns.Write((byte)0x00);
            }
            else if (CharacterCount == 1)
            {
                //Console.WriteLine("CharNicknameLength: {0}", User.CharacterNickname[0].Length);
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x03);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)(User.CharacterPos - 1));
                ns.Write((byte)0x00);
                ns.Write((byte)User.CharacterNation1);
                ns.Write((byte)User.CharacterJob1);
                ns.Write((byte)User.CharacterLevel1);
                //ns.Write((byte)User.CharacterNickname[0].Length);
                //ns.WriteASCIIFixedNoSize(User.CharacterNickname[0], User.CharacterNickname[0].Length);
                ns.WriteBIG5FixedWithSize(User.CharacterNickname1);
                //ns.Write((byte)0x00);
                if (User.Gender == 1)
                {
                    ns.Write(Encrypt.encodeMultiBytes(65536), 0);
                }
                else
                {
                    ns.Write((byte)0x00);
                }
                for (int i = 0; i < 8; i++)
                {
                    ns.Write(Encrypt.encodeMultiBytes(User.CharacterOneEquipment[i]), 0);
                }
                ns.Write(encodedHairClump(User.CharacterDecodedHairClump1), 0); 
                ns.Write(encodedHairClump(User.CharacterDecodedHairClump2), 0);
                ns.Write(encodedHairClump(User.CharacterDecodedHairClump3), 0);
                ns.Write(encodedHairClump(User.CharacterDecodedHairClump4), 0);
                //ns.Write(0);
                ns.Write((long)0);
                ns.Write((long)0);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write(User.CharacterEncodedCloth, 0);
                ns.Write(User.CharacterEncodedSkin, 0);
                ns.Write(User.CharacterEncodedHair, 0);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0x0002);
            }
            else if(CharacterCount == 2)
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x03);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
                ns.Write((byte)User.CharacterNation1);
                ns.Write((byte)User.CharacterJob1);
                ns.Write((byte)User.CharacterLevel1);
                ns.WriteBIG5FixedWithSize(User.CharacterNickname1);
                ns.Write((byte)0x00);
                for (int i = 0; i < 8; i++)
                {
                    ns.Write(Encrypt.encodeMultiBytes(User.CharacterOneEquipment[i]), 0);
                }
                ns.Write(0);
                ns.Write((long)0);
                ns.Write((long)0);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x40);
                ns.Write((byte)0x40);
                ns.Write((byte)0x40);
                ns.Write((short)0x7f81);
                ns.Write((short)0x7f81);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0x0001);
                
                ns.Write((byte)User.CharacterNation2);
                ns.Write((byte)User.CharacterJob2);
                ns.Write((byte)User.CharacterLevel2);
                //ns.Write((byte)User.CharacterNickname[1].Length);
                //ns.WriteASCIIFixedNoSize(User.CharacterNickname[1], User.CharacterNickname[1].Length);
                ns.WriteBIG5FixedWithSize(User.CharacterNickname2);
                ns.Write((byte)0x00);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment1), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment2), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment3), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment4), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment5), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment6), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment7), 0);
                ns.Write(Encrypt.encodeMultiBytes(User.CharacterTwoEquipment8), 0);
                ns.Write(0);
                ns.Write((long)0);
                ns.Write((long)0);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x40);
                ns.Write((byte)0x40);
                ns.Write((byte)0x40);
                ns.Write((short)0x7f81);
                ns.Write((short)0x7f81);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((short)0x7f81);
                ns.Write((short)0);
                ns.Write((byte)0x00);
            }
        }
    }
    public sealed class LoginNewCharacter_0x371_04 : NetPacket
    {
        public LoginNewCharacter_0x371_04(Account User, bool NewCharCheckOK)
        {
            if (NewCharCheckOK)
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x04);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x01);
                ns.Write((byte)0x00);
            }
            else
            {
                ns.Write((byte)0x00);
                ns.Write((short)881); //0x371  op code
                ns.Write((short)0);
                ns.Write((byte)0x04);
                ns.Write(0);
                ns.Write((short)0);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
                ns.Write((byte)0x00);
            }
        }
    }
    public sealed class LoginDeleteCharacter_0x371_05 : NetPacket
    {
        public LoginDeleteCharacter_0x371_05(Account User)
        {
            ns.Write((byte)0x00);
            ns.Write((short)881); //0x371  op code
            ns.Write((short)0);
            ns.Write((byte)0x05);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)0x01);
            ns.Write((byte)0x00);
        }
    }
    public sealed class LoginMapInfo_0x1EC : NetPacket
    {
        public LoginMapInfo_0x1EC(Account User)
        {
            //Client.SendAsync(new NP_Hex(User, "00EC01000000000002AF0000006B000000FFFFFFFFDC000000DC00000018000000260000003200000040000000D2E72408000000000000000000000000000000006B0000004153453A31305F365F383A335F31355F31392F4153453A31305F365F31323A335F31395F39352F4153453A31305F365F323A335F3235345F31303930322D31303935312F4153453A31305F365F373A335F38395F302D332F4153453A31305F365F3100"));
            ns.Write((byte)0x00);
            ns.Write((short)492); //0x1EC  op code
            ns.Write((short)0);
            //ns.Write((byte)0x00);
            //ns.Write((byte)0x00);
            //ns.Write((byte)0x00);
            //ns.Write((byte)0x02);
            ns.Write(User.UserMap.decodedMapID, 0);
            ns.Write((short)User.GamePosX);
            ns.Write((short)0);
            ns.Write((short)User.GamePosY);
            ns.Write((short)0);
            ns.WriteHex("FFFFFFFF");
            ns.Write((short)User.UserMap.MapWidth);
            ns.Write((short)0);
            ns.Write((short)User.UserMap.MapHeight);
            ns.Write((short)0);
            ns.WriteHex("18000000260000003200000040000000");
            ns.WriteHex(User.UserMap.MapCRC);
            ns.WriteHex("2408000000000000000000000000000000006B0000004153453A31305F365F383A335F31355F31392F4153453A31305F365F31323A335F31395F39352F4153453A31305F365F323A335F3235345F31303930322D31303935312F4153453A31305F365F373A335F38395F302D332F4153453A31305F365F3100");
        }
    }
    public sealed class LoginMapMusicInfo_0x31B : NetPacket
    {
        public LoginMapMusicInfo_0x31B(Account User)
        {
            //Client.SendAsync(new NP_Hex(User, "001B0300000000000280000000020708C1B0C0FEB5F2C5C01B0201030586A084801304020805098FFCB1DC6E06098FFFC3914807098FFBE2D55C08098FFC89B45A1C098FFDD1F0781D098FFFFF91481E098FFCD9E6731F098FFCD9E673090264200464210481020B098FFFFFE1700C098FFFFEE9340D098FFBE3917A0E048FFBE180400F030010030022050000000000"));
            ns.Write((byte)0x00);
            ns.Write((short)795); //0x31B  op code
            ns.Write((short)0);
            //ns.Write((byte)0x00);
            //ns.Write((byte)0x00);
            //ns.Write((byte)0x00);
            //ns.Write((byte)0x02);
            ns.Write(User.UserMap.decodedMapID, 0);
            ns.WriteHex("80000000");
            ns.Write((short)0x0702); //0207
            //ns.WriteBIG5FixedWithSize("溫茲聯邦");
            ns.WriteBIG5FixedWithSize(User.UserMap.MapName);
            ns.WriteHex("1B0201");
            ns.Write((short)0x0503); //0305
            //ns.WriteHex("86A0848002");
            ns.WriteHex(User.UserMap.MapMusic);
            ns.WriteHex("04020805098FFCB1DC6E06098FFFC3914807098FFBE2D55C08098FFC89B45A1C098FFDD1F0781D098FFFFF91481E098FFCD9E6731F098FFCD9E673090264200464210481020B098FFFFFE1700C098FFFFEE9340D098FFBE3917A0E048FFBE180400F030010030022050000000000");
        }
    }
    public sealed class LoginCharParam_0x3A0 : NetPacket
    {
        public LoginCharParam_0x3A0(Account User)
        {
            //Client.SendAsync(new NP_Hex(User, "00A0030000F700000002048442030490988003040710A5B6A5EBA5D6A5EAA5B9A4CEC7DBB2BC05048163060707616c616e6c65690704AE35080485300904030A04680B047F1C04030C0484B7D9CB330D071CB0ADCBE2A4CEA5D5A5EAA4B7A4C6C0DAA4EACEF6A4A4A4BFB2CEA4F20E04030F0710A5DFA5AFA5ECA5F3A5B7A5A2C4EBB9F110048B7611048B761204824F1304810614040015040C0C0C0A0A0A0A0A0A0A16000000"));
            ns.Write((byte)0x00);
            ns.Write((short)928); //0x3A0  op code
            ns.Write((short)0);
            ns.Write((byte)0xF7);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x02);
            ns.WriteHex("02048442030490988003040710A5B6A5EBA5D6A5EAA5B9A4CEC7DBB2BC050481630607");
            //ns.Write((byte)User.CharacterNickname[User.CharacterPos].Length);
            //ns.WriteASCIIFixedNoSize(User.CharacterNickname[User.CharacterPos], User.CharacterNickname[User.CharacterPos].Length);
            ns.WriteBIG5FixedWithSize(User.CharacterNickname1);
            ns.Write((short)0x0407); //0704
            ns.Write(Encrypt.encodeMultiBytes(User.CharacterZula1), 0); 
            ns.WriteHex("080485300904030A04680B047F1C04030C0484B7D9CB330D071CB0ADCBE2A4CEA5D5A5EAA4B7A4C6C0DAA4EACEF6A4A4A4BFB2CEA4F20E04030F0710A5DFA5AFA5ECA5F3A5B7A5A2C4EBB9F110048B7611048B761204824F1304810614040015040C0C0C0A0A0A0A0A0A0A16000000");
        }
    }
    public sealed class LoginCharDisappear_0x3E8_04 : NetPacket
    {
        public LoginCharDisappear_0x3E8_04(Account User)
        {
            ns.Write((byte)0x00);
            ns.Write((short)994); //0x3E2  op code
            ns.Write((short)0);
            ns.Write((byte)0x04);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write(User.GlobalID);
        }
    }
    public sealed class LoginCharAppear_0x3E8_00 : NetPacket
    {
        public LoginCharAppear_0x3E8_00(Account User)
        {
            //Client.SendAsync(new NP_Hex(User, "00E8030000000012001D00320000040000015FFFBFBA020100000000001F00000021000000040000000500000006000000070000000800000009000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF1E00000A1F00000A2000000A2100000A88A0C0FFF0E8B8FFC87C60FFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000720000000948656176656E536B790AB4F5CBBEA4CECDA6BCD40EA4D8A4D8A4D8A4D8A4D8A4D6A4F32DA1F9A1F9A1F95448452D445245414D2D414C4C2D5354415253A1F9A1F9A1F92020A5B5A5D6A5DEA5B9A5BFA1BC10A5D8A5D6A5F3A5BAA5B2A1BCA5C8A2F604BDBDB5E908396403008FFFFFFF7F00"));
            //ns.WriteHex("00E8030000000012001D00320000040000015FFFBFD8000000000000001F00000021000000040000000500000006000000070000000800000009000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF1E00000A1F00000A2000000A2100000A88A0C0FFF0E8B8FFC87C60FFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000720000000948656176656E536B790AB4F5CBBEA4CECDA6BCD40EA4D8A4D8A4D8A4D8A4D8A4D6A4F32DA1F9A1F9A1F95448452D445245414D2D414C4C2D5354415253A1F9A1F9A1F92020A5B5A5D6A5DEA5B9A5BFA1BC10A5D8A5D6A5F3A5BAA5B2A1BCA5C8A2F604BDBDB5E908396403008FFFFFFF7F00");
            ns.Write((byte)0x00);
            ns.Write((short)1000); //0x3E8  op code
            ns.Write((short)0);
            ns.Write((short)0);
            //ns.Write((byte)0x00);
            ns.Write((byte)0x12);
            ns.Write((byte)0x00);

            /*if(User.GamePosX == 0x00)
            {
                ns.Write((byte)0xAF); //fieldWE
                //ns.Write((byte)0x1D); //fieldWE
                ns.Write((byte)0x00);
                //User.GamePosX = 0x1D;
                //User.GamePosX = 0xAF;
            }
            else
            {
                ns.Write((short)User.GamePosX); //fieldWE
            }*/
            ns.Write((short)User.GamePosX); //fieldWE
            /*if (User.GamePosY == 0x00)
            {
                ns.Write((byte)0x6B); //fieldNS
                //ns.Write((byte)0x32); //fieldNS
                ns.Write((byte)0x00);
                //User.GamePosY = 0x32;
                //User.GamePosY = 0x6B;
            }
            else
            {
                ns.Write((short)User.GamePosY); //fieldNS
            }*/
            ns.Write((short)User.GamePosY); //fieldNS
            ns.Write((byte)0x00); //dmy1
            /*if (User.GameDirection == 0x00)
            {
                ns.Write((byte)0x04); //direction
                User.GameDirection = 0x4;
            }
            else
            {
                ns.Write((byte)(User.GameDirection%10)); //direction
            }*/
            ns.Write((byte)(User.GameDirection % 10)); //direction
            //ns.Write((byte)0x04);
            ns.Write((byte)User.GameAct); //act 
            ns.Write((byte)0x00);
            ns.Write((byte)0x01); //type 1=Chara, 2=NPC
            ns.Write((byte)0x5F);
            ns.Write((byte)0xFF);
            ns.Write((byte)0xBF);
            ns.Write(User.GlobalID);
            ns.Write((short)0);
            ns.Write((byte)User.Gender);
            ns.Write((byte)0x00);
            for (int i = 0; i < 8; i++)
            {
                ns.Write(User.CharacterOneEquipment[i]);
            }
            ns.WriteHex("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            //1E00000A1F00000A2000000A2100000A
            ns.Write(User.CharacterDecodedHairClump1, 0);
            ns.Write(User.CharacterDecodedHairClump2, 0);
            ns.Write(User.CharacterDecodedHairClump3, 0);
            ns.Write(User.CharacterDecodedHairClump4, 0);
            ns.Write(User.CharacterDecodedCloth, 0, 3);
            ns.Write((byte)0xFF);
            ns.Write(User.CharacterDecodedSkin, 0, 3);
            ns.Write((byte)0xFF);
            ns.Write(User.CharacterDecodedHair, 0, 3);
            ns.Write((byte)0xFF);
            //ns.WriteHex("FFFFFFFFFFFFFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000072000000");
            ns.WriteHex("FFFFFFFFFFFFFFFFFFFFFFFF0000000000000000000000000000000000000000");
            if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 2 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[0] = 3;
                User.CharacterOneSEffect[1] = 3;
                User.CharacterOneSEffect[2] = 3;
                User.CharacterOneSEffect[3] = 3;
                ns.Write(3);
                ns.Write(3);
                ns.Write(3);
                ns.Write(3);
            }
            else if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 3 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[0] = 3;
                ns.Write(3);
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                User.CharacterOneSEffect[3] = 3;
                ns.Write(3);
            }
            else if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 4 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[0] = 3;
                ns.Write(3);
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 5 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[3] = 3;
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[3] = 0;
                    ns.Write(0);
                }
            }
            else if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 5 && i.ItemCorrect == 3) > 0)
            {
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 4 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[0] = 3;
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[0] = 3;
                    ns.Write(0);
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                User.CharacterOneSEffect[3] = 3;
                ns.Write(3);
            }
            else
            {
                User.CharacterOneSEffect[0] = 0;
                ns.Write(0);
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                User.CharacterOneSEffect[3] = 0;
                ns.Write(0);
            }
            if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 7 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[4] = 3;
                User.CharacterOneSEffect[5] = 3;
                ns.Write(3);
                ns.Write(3);
            }
            else
            {
                User.CharacterOneSEffect[4] = 0;
                User.CharacterOneSEffect[5] = 0;
                ns.Write(0);
                ns.Write(0);
            }
            ns.WriteHex("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000072000000");
            ns.WriteBIG5FixedWithSize(User.CharacterNickname1);
            ns.WriteHex("0AB4F5CBBEA4CECDA6BCD40EA4D8A4D8A4D8A4D8A4D8A4D6A4F32DA1F9A1F9A1F95448452D445245414D2D414C4C2D5354415253A1F9A1F9A1F92020A5B5A5D6A5DEA5B9A5BFA1BC10A5D8A5D6A5F3A5BAA5B2A1BCA5C8A2F604BDBDB5E908396403008FFFFFFF7F00");
            //Console.WriteLine("Broadcast 3e8 packet, global ID: {0}, X: {1}, Y: {2}, Direction: {3}", User.GlobalID, User.GamePosX, User.GamePosY, User.GameDirection);
        }
    }
    public sealed class LoginTeleportCharAppear_0x3E8_00 : NetPacket
    {
        public LoginTeleportCharAppear_0x3E8_00(Account User)
        {
            //Client.SendAsync(new NP_Hex(User, "00E8030000000012001D00320000040000015FFFBFBA020100000000001F00000021000000040000000500000006000000070000000800000009000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF1E00000A1F00000A2000000A2100000A88A0C0FFF0E8B8FFC87C60FFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000720000000948656176656E536B790AB4F5CBBEA4CECDA6BCD40EA4D8A4D8A4D8A4D8A4D8A4D6A4F32DA1F9A1F9A1F95448452D445245414D2D414C4C2D5354415253A1F9A1F9A1F92020A5B5A5D6A5DEA5B9A5BFA1BC10A5D8A5D6A5F3A5BAA5B2A1BCA5C8A2F604BDBDB5E908396403008FFFFFFF7F00"));

            ns.Write((byte)0x00);
            ns.Write((short)1000); //0x3E8  op code
            ns.Write((short)0);
            ns.Write((short)0);
            ns.Write((byte)0x12);
            ns.Write((byte)0x00);

            ns.Write((byte)0x32); //fieldWE
            ns.Write((byte)0x00);
            User.GamePosX = 0x32;

            ns.Write((byte)0x16); //fieldNS
            ns.Write((byte)0x00);
            User.GamePosY = 0x16;

            ns.Write((byte)0x00); //dmy1

            ns.Write((byte)0x04); //direction
            User.GameDirection = 0x4;

            ns.Write((byte)0x00); //act 
            ns.Write((byte)0x00);
            ns.Write((byte)0x01); //type 1=Chara, 2=NPC
            ns.Write((byte)0x5F);
            ns.Write((byte)0xFF);
            ns.Write((byte)0xBF);
            ns.Write(User.GlobalID);
            //ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)User.Gender);
            ns.Write((byte)0x00);
            for(int i = 0; i < 8; i++)
            {
                ns.Write(User.CharacterOneEquipment[i]);
            }
            /*ns.Write(User.CharacterOneEquipment1);
            ns.Write(User.CharacterOneEquipment2);
            ns.Write(User.CharacterOneEquipment3);
            ns.Write(User.CharacterOneEquipment4);
            ns.Write(User.CharacterOneEquipment5);
            ns.Write(User.CharacterOneEquipment6);
            ns.Write(User.CharacterOneEquipment7);
            ns.Write(User.CharacterOneEquipment8);*/
            ns.WriteHex("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            //1E00000A1F00000A2000000A2100000A
            ns.Write(User.CharacterDecodedHairClump1, 0);
            ns.Write(User.CharacterDecodedHairClump2, 0);
            ns.Write(User.CharacterDecodedHairClump3, 0);
            ns.Write(User.CharacterDecodedHairClump4, 0);
            ns.Write(User.CharacterDecodedCloth, 0, 3);
            ns.Write((byte)0xFF);
            ns.Write(User.CharacterDecodedSkin, 0, 3);
            ns.Write((byte)0xFF);
            ns.Write(User.CharacterDecodedHair, 0, 3);
            ns.Write((byte)0xFF);
            //ns.WriteHex("FFFFFFFFFFFFFFFFFFFFFFFF0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000072000000");
            ns.WriteHex("FFFFFFFFFFFFFFFFFFFFFFFF0000000000000000000000000000000000000000");
            if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 2 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[0] = 3;
                User.CharacterOneSEffect[1] = 3;
                User.CharacterOneSEffect[2] = 3;
                User.CharacterOneSEffect[3] = 3;
                ns.Write(3);
                ns.Write(3);
                ns.Write(3);
                ns.Write(3);
            }
            else if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 3 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[0] = 3;
                ns.Write(3);
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                User.CharacterOneSEffect[3] = 3;
                ns.Write(3);
            }
            else if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 4 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[0] = 3;
                ns.Write(3);
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 5 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[3] = 3;
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[3] = 0;
                    ns.Write(0);
                }
            }
            else if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 5 && i.ItemCorrect == 3) > 0)
            {
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 4 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[0] = 3;
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[0] = 3;
                    ns.Write(0);
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                User.CharacterOneSEffect[3] = 3;
                ns.Write(3);
            }
            else
            {
                User.CharacterOneSEffect[0] = 0;
                ns.Write(0);
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6 && i.ItemCorrect == 3) > 0)
                {
                    User.CharacterOneSEffect[1] = 3;
                    User.CharacterOneSEffect[2] = 3;
                    ns.Write(3);
                    ns.Write(3);
                }
                else
                {
                    User.CharacterOneSEffect[1] = 0;
                    User.CharacterOneSEffect[2] = 0;
                    ns.Write(0);
                    ns.Write(0);
                }
                User.CharacterOneSEffect[3] = 0;
                ns.Write(0);
            }
            if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 7 && i.ItemCorrect == 3) > 0)
            {
                User.CharacterOneSEffect[4] = 3;
                User.CharacterOneSEffect[5] = 3;
                ns.Write(3);
                ns.Write(3);
            }
            else
            {
                User.CharacterOneSEffect[4] = 0;
                User.CharacterOneSEffect[5] = 0;
                ns.Write(0);
                ns.Write(0);
            }
            ns.WriteHex("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000072000000");
            //ns.Write((byte)User.CharacterNickname[User.CharacterPos].Length);
            //ns.WriteASCIIFixedNoSize(User.CharacterNickname[User.CharacterPos], User.CharacterNickname[User.CharacterPos].Length);
            ns.WriteBIG5FixedWithSize(User.CharacterNickname1);
            ns.WriteHex("0AB4F5CBBEA4CECDA6BCD40EA4D8A4D8A4D8A4D8A4D8A4D6A4F32DA1F9A1F9A1F95448452D445245414D2D414C4C2D5354415253A1F9A1F9A1F92020A5B5A5D6A5DEA5B9A5BFA1BC10A5D8A5D6A5F3A5BAA5B2A1BCA5C8A2F604BDBDB5E908396403008FFFFFFF7F00");
            //Console.WriteLine("Broadcast 3e8 packet, global ID: {0}, X: {1}, Y: {2}, Direction: {3}", User.GlobalID, User.GamePosX, User.GamePosY, User.GameDirection);
        }
    }
    public sealed class LoginMovedItem : NetPacket
    {
        public LoginMovedItem(Account User, int destItemPos)
        {
            ns.Write((byte)0x00);
            ns.Write((short)862); //0x35E  op code
            ns.Write((short)0);
            ns.Write(User.GlobalID);
            ns.Write((byte)(destItemPos));
            ns.WriteHex("019002008FFFFFFF7F8FFFFFFF7F00");
        }
    }
    public sealed class LoginRemoveMultiItem : NetPacket
    {
        public LoginRemoveMultiItem(Account User, List<ItemAttr> UserItem)
        {
            ns.Write((byte)0x00);
            ns.Write((short)862); //0x35E  op code
            ns.Write((short)0);
            ns.Write(User.GlobalID);
            for (int i = 0; i < UserItem.Count; i++)
            {
                ns.Write((byte)UserItem[i].ItemPos);
                ns.WriteHex("019002008FFFFFFF7F");
            }
            ns.WriteHex("8FFFFFFF7F00");
        }
    }
    public sealed class LoginAddMultiItem : NetPacket
    {
        public LoginAddMultiItem(Account User, List<ItemAttr> UserItem)
        {
            if (UserItem.Count > 0)
            {
                ns.Write((byte)0x00);
                ns.Write((short)862); //0x35E  op code
                ns.Write((short)0);
                ns.Write(User.GlobalID);
                for (int i = 0; i < UserItem.Count; i++)
                {
                    if (UserItem[i].ItemTypeNum == 1)
                    {
                        ns.Write((byte)(UserItem[i].ItemPos));
                        ns.WriteHex("01A70F876981F8008F5182B880803A8F52378F53008F548A788F55853C8F56018F5800900100");
                        //A70F876981F800 8F5182B880803A 8F5237 8F5300 8F548A78 8F55853C 8F5601 8F5800
                        //8F5237 = bottom attr
                        ns.Write((short)0x0290);
                        ns.Write(UserItem[i].ItemEncodedID, 0);
                        ns.WriteHex("902282B8808038902301902500902800902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F");
                        ns.Write((short)0x4290);
                        ns.Write((byte)0x00);
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x12);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemName);
                        ns.Write((short)0x219F);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemDesc);
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x15);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemType);
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x16);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemUsageLimit);
                        ns.WriteHex("902B00902D00A717042D2D2D2DA71087F033A71100A73600A73700900100A73800A73A00");
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x39);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemValidDate);
                        //ns.WriteHex("A73005A7310FA73505");
                        for (int j = 0; j < 15; j++)
                        {
                            if (UserItem[i].ItemFoodAttackEffect[j] == 0)
                            {
                                continue;
                            }
                            else
                            {
                                ns.Write((byte)0xA7);
                                ns.Write((byte)(24 + j));
                                ns.Write((byte)(UserItem[i].ItemFoodAttackEffect[j]));
                            }
                        }
                        for (int k = 0; k < 15; k++)
                        {
                            if (UserItem[i].ItemFoodDefenceEffect[k] == 0)
                            {
                                continue;
                            }
                            else
                            {
                                ns.Write((byte)0xA7);
                                ns.Write((byte)(39 + k));
                                ns.Write((byte)(UserItem[i].ItemFoodDefenceEffect[k]));
                            }
                        }
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x3B);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemCombinable);
                        ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
                    }
                    else
                    {
                        //005E030000E73600000001A70F876981FF008F5199D080038F52038F53068F549CF0708F55A5598F56018F58028F5A148F5C008F5E148F60008F62148F64008F66148F68008F6A148F6C008F6E148F70008F72148F74008F76148F78008F7A148F7C008F7E14900000900100900286B08C802B9017908C802B90228FFFFFFF7F902301902500902816902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F904200A71212A5ECA5A4A5AAA5D6A5ECA5C3A5C9A1DCADB69F215CBDD1BCD4A4CECBE2CECFA4CBB8C6B1FEA4B7A4C6A1A2BFF4BEF2A4CEB9C8A4A4B8F7A4F2CAFCA4C4BEF3A1A3BDD1BCD4A4CECBE2CECFA4ACB9E2A4A4A4DBA4C9A1A2CAFCA4BFA4ECA4EBB8F7A4CEC0FEA4CFC2BFA4AFA4CAA4EBA1A3A71502BEF3A7160B4C56353220CFA3BCA1BEA4902B41902D46A717053130302025A70900A70A00A70B00A70C00A70D81C328A70E00A7108F8620A71100A73686A8B48066A73700900100A73800A73A00A73904A4CAA4B7A73B04C9D4B2C490408FFFFFFF7F904200A73C008FFFFFFF7F8FFFFFFF7F00
                        //Equ Type: 2 = Robe, 3 = Shoes, 4 = Weapon
                        ns.Write((byte)(UserItem[i].ItemPos));
                        ns.WriteHex("01A70F876981FF008F5181B88480048F5217");
                        ns.Write((short)0x538F);
                        if (UserItem[i].ItemTypeNum == 2) //1. Advanced Wear Position
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (UserItem[i].ItemTypeNum == 3)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (UserItem[i].ItemTypeNum == 4)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (UserItem[i].ItemTypeNum == 5)
                        {
                            ns.Write((byte)0x03);
                        }
                        else if (UserItem[i].ItemTypeNum == 6)
                        {
                            ns.Write((byte)0x04);
                        }
                        else if (UserItem[i].ItemTypeNum == 7)
                        {
                            ns.Write((byte)0x05);
                        }
                        else if (UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)0x06);
                        }
                        ns.WriteHex("8F5485238F55308F56018F5800");

                        for (int j = 0; j < 10; j++) //2. Wear Attack & Defence
                        {
                            ns.Write((byte)0x8F);
                            ns.Write((byte)(90 + j * 4));
                            ns.Write((byte)(UserItem[i].ItemEquipmentAttackEffect[j]));

                            if (j < 9)
                            {
                                ns.Write((byte)0x8F);
                                ns.Write((byte)(90 + j * 4 + 2));
                                ns.Write((byte)(UserItem[i].ItemEquipmentDefenceEffect[j]));
                            }
                            else
                            {
                                ns.Write((short)0x0090);
                                ns.Write((byte)(UserItem[i].ItemEquipmentDefenceEffect[j]));
                            }
                        }

                        ns.Write((short)0x0190); //3. Wear Type
                        if (UserItem[i].ItemTypeNum == 2)
                        {
                            ns.Write((byte)0x0E);
                        }
                        else if (UserItem[i].ItemTypeNum == 3)
                        {
                            ns.Write((byte)0x06);
                        }
                        else if (UserItem[i].ItemTypeNum == 4)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (UserItem[i].ItemTypeNum == 5)
                        {
                            ns.Write((byte)0x04);
                        }
                        else if (UserItem[i].ItemTypeNum == 6)
                        {
                            ns.Write((byte)0x08);
                        }
                        else if (UserItem[i].ItemTypeNum == 7)
                        {
                            ns.Write((byte)0x10);
                        }
                        else if (UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)0x00);
                        }

                        ns.Write((short)0x0290); //4. ItemID
                        ns.Write(UserItem[i].ItemEncodedID, 0);

                        if (UserItem[i].ItemTypeNum == 2) //5. Advanced Wear Position 2
                        {
                            ns.WriteHex("9005AF269006AF279007AF289008AF29");
                        }
                        else if (UserItem[i].ItemTypeNum == 3)
                        {
                            ns.WriteHex("9008AF29");
                        }
                        else if (UserItem[i].ItemTypeNum == 7)
                        {
                            ns.WriteHex("9009AF2A900AAF2B");
                        }
                        else if (UserItem[i].ItemTypeNum == 9)
                        {
                            ns.WriteHex("9017908C802A");
                        }
                        ns.WriteHex("90228FFFFFFF7F902301902500");

                        ns.Write((short)0x2890); //6. Item Weight
                        ns.Write(Encrypt.encodeMultiBytes(UserItem[i].ItemWeight), 0);
                        ns.WriteHex("902900902E00902F0090320090338768903400903500903600903700903A03903B00903D00903E00903F0090408FFFFFFF7F904200");

                        ns.Write((byte)0xA7); //7. Item Name
                        ns.Write((byte)0x12);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemName);

                        ns.Write((short)0x219F); //8. Item Description
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemDesc);

                        ns.Write((byte)0xA7); //9. Item Type Name
                        ns.Write((byte)0x15);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemType);

                        ns.Write((byte)0xA7); //10. Item Usage Limit
                        ns.Write((byte)0x16);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemUsageLimit);

                        ns.Write((short)0x2B90); //11. Item Physical Damage
                        if (UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)UserItem[i].ItemPhysicalDamage);
                        }
                        else
                        {
                            ns.Write((byte)0x00);
                        }

                        ns.Write((short)0x2D90); //12. Item Magic Damage
                        if (UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)UserItem[i].ItemMagicDamage);
                        }
                        else
                        {
                            ns.Write((byte)0x00);
                        }
                        ns.WriteHex("A7170438312025");
                        ns.Write((short)0x09A7); //13. Item isWear
                        ns.Write((byte)UserItem[i].ItemWear);
                        ns.WriteHex("A70A00A70B00A70C00A70D00A70E00");

                        ns.Write((short)0x10A7); //14.Item Durability
                        ns.WriteHex("8DBE15");
                        ns.WriteHex("A71100");

                        ns.Write((short)0x36A7); //15. Equipment Type (A73686A8B48064 = S)
                        if (UserItem[i].ItemCorrect == 0)
                        {
                            ns.Write((byte)0x00);
                            //ns.WriteHex("86A8B4800D"); //F
                        }
                        else if (UserItem[i].ItemCorrect == 1)
                        {
                            ns.WriteHex("86A8B48065");
                        }
                        else if (UserItem[i].ItemCorrect == 2)
                        {
                            ns.WriteHex("86A8B48066");
                        }
                        else if (UserItem[i].ItemCorrect == 3)
                        {
                            ns.WriteHex("86A8B48064");
                        }
                        ns.WriteHex("A73700");

                        ns.Write((short)0x0190); //16. Wear Type
                        if (UserItem[i].ItemTypeNum == 2)
                        {
                            ns.Write((byte)0x0E);
                        }
                        else if (UserItem[i].ItemTypeNum == 3)
                        {
                            ns.Write((byte)0x06);
                        }
                        else if (UserItem[i].ItemTypeNum == 4)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (UserItem[i].ItemTypeNum == 5)
                        {
                            ns.Write((byte)0x04);
                        }
                        else if (UserItem[i].ItemTypeNum == 6)
                        {
                            ns.Write((byte)0x08);
                        }
                        else if (UserItem[i].ItemTypeNum == 7)
                        {
                            ns.Write((byte)0x10);
                        }
                        else if (UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)0x00);
                        }
                        ns.WriteHex("A73800A73A00");

                        ns.Write((byte)0xA7); //17. Item Valid Date
                        ns.Write((byte)0x39);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemValidDate);

                        ns.Write((byte)0xA7); //18. Item Combinable
                        ns.Write((byte)0x3B);
                        ns.WriteBIG5FixedWithSize(UserItem[i].ItemCombinable);
                        ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
                    }
                }
                ns.WriteHex("8FFFFFFF7F00");
            }
        }
    }
    public sealed class LoginLoadAllItem : NetPacket
    {
        //上半身下半身手=2, 上半身下半身=3, 上半身=4, 下半身=5, 手=6, 鞋=7, 武器=9
        public LoginLoadAllItem(Account User)
        {
            if (User.UserItem.Count > 0)
            {
                ns.Write((byte)0x00);
                ns.Write((short)862); //0x35E  op code
                ns.Write((short)0);
                ns.Write(User.GlobalID);
                for (int i = 0; i < User.UserItem.Count; i++)
                {
                    if (User.UserItem[i].ItemTypeNum == 1)
                    {
                        ns.Write((byte)(User.UserItem[i].ItemPos));
                        ns.WriteHex("01A70F876981F8008F5182B880803A8F52378F53008F548A788F55853C8F56018F5800900100");
                        //A70F876981F800 8F5182B880803A 8F5237 8F5300 8F548A78 8F55853C 8F5601 8F5800
                        //8F5237 = bottom attr
                        ns.Write((short)0x0290);
                        ns.Write(User.UserItem[i].ItemEncodedID, 0);
                        ns.WriteHex("902282B8808038902301902500902800902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F");
                        ns.Write((short)0x4290);
                        ns.Write((byte)0x00);
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x12);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemName);
                        ns.Write((short)0x219F);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemDesc);
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x15);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemType);
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x16);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemUsageLimit);
                        ns.WriteHex("902B00902D00A717042D2D2D2DA71087F033A71100A73600A73700900100A73800A73A00");
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x39);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemValidDate);
                        //ns.WriteHex("A73005A7310FA73505");
                        for (int j = 0; j < 15; j++)
                        {
                            if (User.UserItem[i].ItemFoodAttackEffect[j] == 0)
                            {
                                continue;
                            }
                            else
                            {
                                ns.Write((byte)0xA7);
                                ns.Write((byte)(24 + j));
                                ns.Write((byte)(User.UserItem[i].ItemFoodAttackEffect[j]));
                            }
                        }
                        for (int k = 0; k < 15; k++)
                        {
                            if (User.UserItem[i].ItemFoodDefenceEffect[k] == 0)
                            {
                                continue;
                            }
                            else
                            {
                                ns.Write((byte)0xA7);
                                ns.Write((byte)(39 + k));
                                ns.Write((byte)(User.UserItem[i].ItemFoodDefenceEffect[k]));
                            }
                        }
                        ns.Write((byte)0xA7);
                        ns.Write((byte)0x3B);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemCombinable);
                        ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
                    }
                    else
                    {
                        //005E030000E73600000001A70F876981FF008F5199D080038F52038F53068F549CF0708F55A5598F56018F58028F5A148F5C008F5E148F60008F62148F64008F66148F68008F6A148F6C008F6E148F70008F72148F74008F76148F78008F7A148F7C008F7E14900000900100900286B08C802B9017908C802B90228FFFFFFF7F902301902500902816902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F904200A71212A5ECA5A4A5AAA5D6A5ECA5C3A5C9A1DCADB69F215CBDD1BCD4A4CECBE2CECFA4CBB8C6B1FEA4B7A4C6A1A2BFF4BEF2A4CEB9C8A4A4B8F7A4F2CAFCA4C4BEF3A1A3BDD1BCD4A4CECBE2CECFA4ACB9E2A4A4A4DBA4C9A1A2CAFCA4BFA4ECA4EBB8F7A4CEC0FEA4CFC2BFA4AFA4CAA4EBA1A3A71502BEF3A7160B4C56353220CFA3BCA1BEA4902B41902D46A717053130302025A70900A70A00A70B00A70C00A70D81C328A70E00A7108F8620A71100A73686A8B48066A73700900100A73800A73A00A73904A4CAA4B7A73B04C9D4B2C490408FFFFFFF7F904200A73C008FFFFFFF7F8FFFFFFF7F00
                        //Equ Type: 2 = Robe, 3 = Shoes, 4 = Weapon
                        ns.Write((byte)(User.UserItem[i].ItemPos));
                        ns.WriteHex("01A70F876981FF008F5181B88480048F5217");
                        ns.Write((short)0x538F);

                        if (User.UserItem[i].ItemTypeNum == 2) //1. Advanced Wear Position
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 3)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 4)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 5)
                        {
                            ns.Write((byte)0x03);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 6)
                        {
                            ns.Write((byte)0x04);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 7)
                        {
                            ns.Write((byte)0x05);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)0x06);
                        }

                        ns.WriteHex("8F5485238F55308F56018F5800");

                        for (int j = 0; j < 10; j++) //2. Wear Attack & Defence
                        {
                            ns.Write((byte)0x8F);
                            ns.Write((byte)(90 + j * 4));
                            ns.Write((byte)(User.UserItem[i].ItemEquipmentAttackEffect[j]));

                            if (j < 9)
                            {
                                ns.Write((byte)0x8F);
                                ns.Write((byte)(90 + j * 4 + 2));
                                ns.Write((byte)(User.UserItem[i].ItemEquipmentDefenceEffect[j]));
                            }
                            else
                            {
                                ns.Write((short)0x0090);
                                ns.Write((byte)(User.UserItem[i].ItemEquipmentDefenceEffect[j]));
                            }
                        }

                        ns.Write((short)0x0190); //3. Wear Type
                        //ns.Write((byte)0x00); weapon
                        if (User.UserItem[i].ItemTypeNum == 2)
                        {
                            ns.Write((byte)0x0E);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 3)
                        {
                            ns.Write((byte)0x06);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 4)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 5)
                        {
                            ns.Write((byte)0x04);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 6)
                        {
                            ns.Write((byte)0x08);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 7)
                        {
                            ns.Write((byte)0x10);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)0x00);
                        }


                        ns.Write((short)0x0290); //4. ItemID
                        ns.Write(User.UserItem[i].ItemEncodedID, 0);

                        if (User.UserItem[i].ItemTypeNum == 2) //5. Advanced Wear Position 2
                        {
                            ns.WriteHex("9005AF269006AF279007AF289008AF29");
                        }
                        else if (User.UserItem[i].ItemTypeNum == 3)
                        {
                            ns.WriteHex("9008AF29");
                        }
                        else if (User.UserItem[i].ItemTypeNum == 7)
                        {
                            ns.WriteHex("9009AF2A900AAF2B");
                        }
                        else if (User.UserItem[i].ItemTypeNum == 9)
                        {
                            ns.WriteHex("9017908C802A");
                        }
                        
                        ns.WriteHex("90228FFFFFFF7F902301902500");

                        ns.Write((short)0x2890); //6. Item Weight
                        ns.Write(Encrypt.encodeMultiBytes(User.UserItem[i].ItemWeight), 0);
                        ns.WriteHex("902900902E00902F0090320090338768903400903500903600903700903A03903B00903D00903E00903F0090408FFFFFFF7F904200");

                        ns.Write((byte)0xA7); //7. Item Name
                        ns.Write((byte)0x12);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemName);

                        ns.Write((short)0x219F); //8. Item Description
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemDesc);

                        ns.Write((byte)0xA7); //9. Item Type Name
                        ns.Write((byte)0x15);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemType);

                        ns.Write((byte)0xA7); //10. Item Usage Limit
                        ns.Write((byte)0x16);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemUsageLimit);

                        ns.Write((short)0x2B90); //11. Item Physical Damage
                        if (User.UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)User.UserItem[i].ItemPhysicalDamage);
                        }
                        else
                        {
                            ns.Write((byte)0x00);
                        }

                        ns.Write((short)0x2D90); //12. Item Magic Damage
                        if (User.UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)User.UserItem[i].ItemMagicDamage);
                        }
                        else
                        {
                            ns.Write((byte)0x00);
                        }
                        ns.WriteHex("A7170438312025");
                        ns.Write((short)0x09A7); //13. Item isWear
                        ns.Write((byte)User.UserItem[i].ItemWear);
                        ns.WriteHex("A70A00A70B00A70C00A70D00A70E00");

                        ns.Write((short)0x10A7); //14.Item Durability
                        ns.WriteHex("8DBE15");
                        ns.WriteHex("A71100");

                        ns.Write((short)0x36A7); //15. Equipment Type (A73686A8B48064 = S)
                        if (User.UserItem[i].ItemCorrect == 0)
                        {
                            ns.Write((byte)0x00);
                            //ns.WriteHex("86A8B4800D"); //F
                        }
                        else if (User.UserItem[i].ItemCorrect == 1)
                        {
                            ns.WriteHex("86A8B48065");
                        }
                        else if (User.UserItem[i].ItemCorrect == 2)
                        {
                            ns.WriteHex("86A8B48066");
                        }
                        else if(User.UserItem[i].ItemCorrect == 3)
                        {
                            ns.WriteHex("86A8B48064");
                        }
                        ns.WriteHex("A73700");

                        ns.Write((short)0x0190); //16. Wear Type
                        if (User.UserItem[i].ItemTypeNum == 2)
                        {
                            ns.Write((byte)0x0E);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 3)
                        {
                            ns.Write((byte)0x06);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 4)
                        {
                            ns.Write((byte)0x02);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 5)
                        {
                            ns.Write((byte)0x04);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 6)
                        {
                            ns.Write((byte)0x08);
                        }
                        else if(User.UserItem[i].ItemTypeNum == 7)
                        {
                            ns.Write((byte)0x10);
                        }
                        else if (User.UserItem[i].ItemTypeNum == 9)
                        {
                            ns.Write((byte)0x00);
                        }
                        
                        ns.WriteHex("A73800A73A00");
                        
                        ns.Write((byte)0xA7); //17. Item Valid Date
                        ns.Write((byte)0x39);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemValidDate);

                        ns.Write((byte)0xA7); //18. Item Combinable
                        ns.Write((byte)0x3B);
                        ns.WriteBIG5FixedWithSize(User.UserItem[i].ItemCombinable);
                        ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");         
                    }
                }
                ns.WriteHex("8FFFFFFF7F00");
            }
        }
    }
    public sealed class LoginMoveItem : NetPacket
    {
        public LoginMoveItem(Account User, ItemAttr UserItem, int isWear)
        {
            ns.Write((byte)0x00);
            ns.Write((short)862); //0x35E  op code
            ns.Write((short)0);
            ns.Write(User.GlobalID);
            if (UserItem.ItemTypeNum == 1)
            {
                ns.Write((byte)(UserItem.ItemPos));
                ns.WriteHex("01A70F876981F8008F5182B880803A8F52378F53008F548A788F55853C8F56018F5800900100");
                //A70F876981F800 8F5182B880803A 8F5237 8F5300 8F548A78 8F55853C 8F5601 8F5800
                //8F5237 = bottom attr
                ns.Write((short)0x0290);
                ns.Write(UserItem.ItemEncodedID, 0);
                ns.WriteHex("902282B8808038902301902500902800902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F");
                ns.Write((short)0x4290);
                ns.Write((byte)0x00);
                ns.Write((byte)0xA7);
                ns.Write((byte)0x12);
                ns.WriteBIG5FixedWithSize(UserItem.ItemName);
                ns.Write((short)0x219F);
                ns.WriteBIG5FixedWithSize(UserItem.ItemDesc);
                ns.Write((byte)0xA7);
                ns.Write((byte)0x15);
                ns.WriteBIG5FixedWithSize(UserItem.ItemType);
                ns.Write((byte)0xA7);
                ns.Write((byte)0x16);
                ns.WriteBIG5FixedWithSize(UserItem.ItemUsageLimit);
                ns.WriteHex("902B00902D00A717042D2D2D2DA71087F033A71100A73600A73700900100A73800A73A00");
                ns.Write((byte)0xA7);
                ns.Write((byte)0x39);
                ns.WriteBIG5FixedWithSize(UserItem.ItemValidDate);
                //ns.WriteHex("A73005A7310FA73505");
                for (int j = 0; j < 15; j++)
                {
                    if (UserItem.ItemFoodAttackEffect[j] == 0)
                    {
                        continue;
                    }
                    else
                    {
                        ns.Write((byte)0xA7);
                        ns.Write((byte)(24 + j));
                        ns.Write((byte)(UserItem.ItemFoodAttackEffect[j]));
                    }
                }
                for (int k = 0; k < 15; k++)
                {
                    if (UserItem.ItemFoodDefenceEffect[k] == 0)
                    {
                        continue;
                    }
                    else
                    {
                        ns.Write((byte)0xA7);
                        ns.Write((byte)(39 + k));
                        ns.Write((byte)(UserItem.ItemFoodDefenceEffect[k]));
                    }
                }
                ns.Write((byte)0xA7);
                ns.Write((byte)0x3B);
                ns.WriteBIG5FixedWithSize(UserItem.ItemCombinable);
                ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
            }
            else
            {
                //904200A71210B8ABBDACA4A4A4CEA5EDA1BCA5D6A3D39F214CB8ABBDACA4A4CBE2CBA1BBC8A4A4C3A3A4CBB0A6CDD1A4B5A4ECA4C6A4A4A4EBB0C2B2C1A4CAA5EDA1BCA5D6A1A3B4A8A4B5A4E4BDEBA4B5A4CBA4CFA4A2A4DEA4EAB6AFA4AFA4CAA4A4A1A3A71506A5EDA1BCA5D6A716104C563120B9F5CFA3BCE9BCA1C0BABEA4902B00902D00A7170438312025A70900A70A00A70B00A70C00A70D00A70E00A7108DBE15A71100A73686A8B48064A7370090010EA73800A73A00A73904A4CAA4B7A73B04C9D4B2C490408FFFFFFF7F904200A73C008FFFFFFF7F8FFFFFFF7F00
                ns.Write((byte)(UserItem.ItemPos));
                ns.WriteHex("01A70F876981FF008F5181B88480048F5217");
                ns.Write((short)0x538F);
                if (UserItem.ItemTypeNum == 2) //1. Advanced Wear Position
                {
                    ns.Write((byte)0x02);
                }
                else if (UserItem.ItemTypeNum == 3)
                {
                    ns.Write((byte)0x02);
                }
                else if (UserItem.ItemTypeNum == 4)
                {
                    ns.Write((byte)0x02);
                }
                else if (UserItem.ItemTypeNum == 5)
                {
                    ns.Write((byte)0x03);
                }
                else if (UserItem.ItemTypeNum == 6)
                {
                    ns.Write((byte)0x04);
                }
                else if (UserItem.ItemTypeNum == 7)
                {
                    ns.Write((byte)0x05);
                }
                else if (UserItem.ItemTypeNum == 9)
                {
                    ns.Write((byte)0x06);
                }
                
                ns.WriteHex("8F5485238F55308F56018F5800");

                for (int j = 0; j < 10; j++) //2. Wear Attack & Defence
                {
                    ns.Write((byte)0x8F);
                    ns.Write((byte)(90 + j * 4));
                    ns.Write((byte)(UserItem.ItemEquipmentAttackEffect[j]));

                    if (j < 9)
                    {
                        ns.Write((byte)0x8F);
                        ns.Write((byte)(90 + j * 4 + 2));
                        ns.Write((byte)(UserItem.ItemEquipmentDefenceEffect[j]));
                    }
                    else
                    {
                        ns.Write((short)0x0090);
                        ns.Write((byte)(UserItem.ItemEquipmentDefenceEffect[j]));
                    }
                }

                ns.Write((short)0x0190); //3. Wear Type
                if (UserItem.ItemTypeNum == 2)
                {
                    ns.Write((byte)0x0E);
                }
                else if (UserItem.ItemTypeNum == 3)
                {
                    ns.Write((byte)0x06);
                }
                else if (UserItem.ItemTypeNum == 4)
                {
                    ns.Write((byte)0x02);
                }
                else if (UserItem.ItemTypeNum == 5)
                {
                    ns.Write((byte)0x04);
                }
                else if (UserItem.ItemTypeNum == 6)
                {
                    ns.Write((byte)0x08);
                }
                else if (UserItem.ItemTypeNum == 7)
                {
                    ns.Write((byte)0x10);
                }
                else if (UserItem.ItemTypeNum == 9)
                {
                    ns.Write((byte)0x00);
                }

                ns.Write((short)0x0290); //4. ItemID
                ns.Write(UserItem.ItemEncodedID, 0);

                if (UserItem.ItemTypeNum == 2) //5. Advanced Wear Position 2
                {
                    ns.WriteHex("9005AF269006AF279007AF289008AF29");
                }
                else if (UserItem.ItemTypeNum == 3)
                {
                    ns.WriteHex("9008AF29");
                }
                else if (UserItem.ItemTypeNum == 7)
                {
                    ns.WriteHex("9009AF2A900AAF2B");
                }
                else if (UserItem.ItemTypeNum == 9)
                {
                    ns.WriteHex("9017908C802A");
                }
                ns.WriteHex("90228FFFFFFF7F902301902500");

                ns.Write((short)0x2890); //6. Item Weight
                ns.Write(Encrypt.encodeMultiBytes(UserItem.ItemWeight), 0);
                ns.WriteHex("902900902E00902F0090320090338768903400903500903600903700903A03903B00903D00903E00903F0090408FFFFFFF7F904200");

                ns.Write((byte)0xA7); //7. Item Name
                ns.Write((byte)0x12);
                ns.WriteBIG5FixedWithSize(UserItem.ItemName);

                ns.Write((short)0x219F); //8. Item Description
                ns.WriteBIG5FixedWithSize(UserItem.ItemDesc);

                ns.Write((byte)0xA7); //9. Item Type Name
                ns.Write((byte)0x15);
                ns.WriteBIG5FixedWithSize(UserItem.ItemType);

                ns.Write((byte)0xA7); //10. Item Usage Limit
                ns.Write((byte)0x16);
                ns.WriteBIG5FixedWithSize(UserItem.ItemUsageLimit);

                ns.Write((short)0x2B90); //11. Item Physical Damage
                if (UserItem.ItemTypeNum == 9)
                {
                    ns.Write((byte)UserItem.ItemPhysicalDamage);
                }
                else
                {
                    ns.Write((byte)0x00);
                }

                ns.Write((short)0x2D90); //12. Item Magic Damage
                if (UserItem.ItemTypeNum == 9)
                {
                    ns.Write((byte)UserItem.ItemMagicDamage);
                }
                else
                {
                    ns.Write((byte)0x00);
                }
                ns.WriteHex("A7170438312025");
                ns.Write((short)0x09A7); //13. Item isWear
                ns.Write((byte)isWear);
                ns.WriteHex("A70A00A70B00A70C00A70D00A70E00");

                ns.Write((short)0x10A7); //14.Item Durability
                ns.WriteHex("8DBE15");
                ns.WriteHex("A71100");

                ns.Write((short)0x36A7); //15. Equipment Type (A73686A8B48064 = S)
                if (UserItem.ItemCorrect == 0)
                {
                    ns.Write((byte)0x00);
                    //ns.WriteHex("86A8B4800D"); //F
                }
                else if (UserItem.ItemCorrect == 1)
                {
                    ns.WriteHex("86A8B48065");
                }
                else if (UserItem.ItemCorrect == 2)
                {
                    ns.WriteHex("86A8B48066");
                }
                else if (UserItem.ItemCorrect == 3)
                {
                    ns.WriteHex("86A8B48064");
                }
                ns.WriteHex("A73700");

                ns.Write((short)0x0190); //16. Wear Type
                if (UserItem.ItemTypeNum == 2)
                {
                    ns.Write((byte)0x0E);
                }
                else if (UserItem.ItemTypeNum == 3)
                {
                    ns.Write((byte)0x06);
                }
                else if (UserItem.ItemTypeNum == 4)
                {
                    ns.Write((byte)0x02);
                }
                else if (UserItem.ItemTypeNum == 5)
                {
                    ns.Write((byte)0x04);
                }
                else if (UserItem.ItemTypeNum == 6)
                {
                    ns.Write((byte)0x08);
                }
                else if (UserItem.ItemTypeNum == 7)
                {
                    ns.Write((byte)0x10);
                }
                else if (UserItem.ItemTypeNum == 9)
                {
                    ns.Write((byte)0x00);
                }

                ns.WriteHex("A73800A73A00");

                ns.Write((byte)0xA7); //17. Item Valid Date
                ns.Write((byte)0x39);
                ns.WriteBIG5FixedWithSize(UserItem.ItemValidDate);

                ns.Write((byte)0xA7); //18. Item Combinable
                ns.Write((byte)0x3B);
                ns.WriteBIG5FixedWithSize(UserItem.ItemCombinable);
                ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
            }
            ns.WriteHex("8FFFFFFF7F00");

        }
    }
    public sealed class LoginGenKey_0X12 : NetPacket
    {
        public LoginGenKey_0X12(Account User, byte[] key)
        {
            ns.Write((byte)0x12);
            ns.Write(key,0,257);
        }
    }
    public sealed class LoginUserInfo_0X02 : NetPacket
    {
        public LoginUserInfo_0X02(Account User, byte last)
        {
            string nickName;
            long gamemoney, userexp, lastLoginTime;
            int playingTime, shuMP, gameoption;
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
                        User.GameOption = Convert.ToInt32(reader["gameOption"]);
                        shuMP = Convert.ToInt32(reader["shuMP"]);
                        User.TopRank = Convert.ToInt32(reader["TopRank"]);
                        User.CurrentShuID = Convert.ToInt64(reader["currentShuCharacterItemID"]);
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
            ns.Write(User.GameOption);
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
        public LoginUserIndividualRecordGameRecord_FF2D02(Account User, byte last)
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
        public LoginUserIndividualRecordMiscellaneous_FF2E02(Account User, byte last)
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
        public LoginFarm_GetMyFarmInfo_FF1D02(Account User, byte last)
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
        public LoginNickName_0X20(Account User, byte last)
        {
            ns.Write((byte)32); //0x20  op code
            ns.Write(0);
            ns.WriteBIG5Fixed_intSize(User.NickName);
            ns.Fill(13);
            ns.Write(last);
        }
    }

    public sealed class Login_NOTIFY_MY_UDP : NetPacket
    {
        public Login_NOTIFY_MY_UDP(byte last)
        {
            ns.Write((byte)9); //0x09  op code
            ns.Write(last);
        }
    }

    public sealed class LoginGetNickName_0X1C : NetPacket
    {
        public LoginGetNickName_0X1C(Account User, byte last)
        {
            ns.Write((byte)28); //0x1C  op code
            if (User.noNickName)
            {
                ns.Write(0x37); //0x37 eServerResult_ENTER_NEW_NICKNAME
                //Console.WriteLine("NEED_ENTER_NEW_NICKNAME");
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
        public LoginGetUserCash_0X179(Account User, byte last)
        {
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
        public GetCurrentAvatarInfo_0X6D(Account User, byte last)
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
        public GetCurrentAvatarInfo(Account User, byte last)
        {
            ns.Write((byte)0x6D); //0x6D  op code
            if (User.haveAvatar)
            {
                ns.Write(0); //3D 00 00 00 new ac?
                if (!User.isFashionModeOn) {
                    for (int i = 0; i < 15; i++)
                    {
                        ns.Write(User.CurrentAvatarInfo[i]);
                    }
                    ns.Fill(0x88); //0x88
                    for (int i = 15; i < 30; i++)
                    {
                        ns.Write(User.CurrentAvatarInfo[i]);
                        int a = 1 | 2;
                    }
                    ns.Fill(0x88);
                    ns.Write(User.costumeMode);
                }
                else
                {
                    for (int i = 30; i < 45; i++)
                    {
                        ns.Write(User.CurrentAvatarInfo[i]);
                    }
                    ns.Fill(0x88); //0x88
                    ns.Write(User.CurrentAvatarInfo[30]);
                    ns.Fill(0xA4);
                    ns.Write((short)0);
                }
                ns.Write((byte)1);
            }
            else
            {
                ns.Write(0x3D); //3D 00 00 00 new ac?
            }

            ns.Write(last);  //end
        }
    }

    /*
    public sealed class GetUserItemAttr : NetPacket
    {
        public GetUserItemAttr(Account User, byte last)
        {
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
    */

    public sealed class Login_FF4D01_0X14E : NetPacket
    {
        public Login_FF4D01_0X14E(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)334); //opcode 0x14E
            ns.Write((byte)0);
            ns.Write(last);
        }
    }

    public sealed class Login_6E_0X6F : NetPacket
    {
        public Login_6E_0X6F(Account User, byte last)
        {
            ns.Write((byte)111); //opcode 0x6F
            ns.Fill(6);
            ns.Write(last);
        }
    }

    public sealed class GetCommunityAgentServer_0X74 : NetPacket
    {
        public GetCommunityAgentServer_0X74(Account User, byte last)
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
        public LoginError(Account User, int ErrorCode, byte last)
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
        public LoginBlackList(Account User, long start, long end, byte last)
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
