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
using AgentServer.Structuring.GameReward;
using AgentServer.Structuring.Item;
using LocalCommons.Cryptography;
using AgentServer.Structuring.Battle;

namespace AgentServer.Packet.Send
{

    public sealed class GameRoom_Hex : NetPacket
    {
        public GameRoom_Hex(string bytes, byte last)
        {
            //FF5805FFFFFFFFFFFFFFFF000000000000000D01
            ns.WriteHex(bytes);
            //ns.Write(bytes, 0, bytes.Length);
            ns.Write(last); //end
        }
    }
    public sealed class GameRoom_TradeStart : NetPacket
    {
        public GameRoom_TradeStart(Account User)
        {
            ns.Write((byte)0x00);
            ns.Write((short)1325); //0x52D  op code
            ns.Write(0);
            ns.Write(0);
            ns.Write((short)0);
            ns.WriteBIG5FixedWithSize(User.CharacterNickname1);
        }
    }
    public sealed class GameRoom_DualWaiting_0x2D1_00 : NetPacket
    {
        public GameRoom_DualWaiting_0x2D1_00()
        {
            ns.Write((byte)0x00);
            ns.Write((short)721); //0x2D1  op code
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_DualConfirm_0x2D1_01 : NetPacket
    {
        public GameRoom_DualConfirm_0x2D1_01()
        {
            ns.Write((byte)0x00);
            ns.Write((short)721); //0x2D1  op code
            ns.Write((short)0);
            ns.Write((byte)0x01);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_DualBattledCancel_0x2D1_02 : NetPacket
    {
        public GameRoom_DualBattledCancel_0x2D1_02()
        {
            ns.Write((byte)0x00);
            ns.Write((short)721); //0x2D1  op code
            ns.Write((short)0);
            ns.Write((byte)0x02);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_DualBattleCancel_0x2D1_03 : NetPacket
    {
        public GameRoom_DualBattleCancel_0x2D1_03()
        {
            ns.Write((byte)0x00);
            ns.Write((short)721); //0x2D1  op code
            ns.Write((short)0);
            ns.Write((byte)0x03);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_BattleStart_0x3FA : NetPacket
    {
        public GameRoom_BattleStart_0x3FA()
        {
            ns.WriteHex("00FA03000002000000250003C800000310270001641E001E001E00020000000000CB000000000EC5A8A4F2C1B4CCC7A4BBA4E8A1AA06020432050007D88884128FFFFFFF7F0014C5A8A4CEA5EAA1BCA5C0A1BCA4F2C5DDA4BBA1AA06010432050007D88884088FFFFFFF7F0010A4A2A4CEBEECBDEAA4F2C3A5A4A8A1AA0604020103020432050007D88884268FFFFFFF7F080ECCA3CAFDA4ACC1B4CCC7A4B9A4EB09320A018FFFFFFF7F0818CCA3CAFDA4CEA5EAA1BCA5C0A1BCA4ACC5DDA4B5A4ECA4EB09320A018FFFFFFF7F0818A4B3A4CEBEECBDEAA4CBA4BFA4C9A4EAA4C4A4ABA4ECA4EB09320A018FFFFFFF7F00");
        }
    }
    public sealed class GameRoom_LoadBattleMap_0x488 : NetPacket
    {
        public GameRoom_LoadBattleMap_0x488()
        {
            ns.WriteHex("00880400002D1A0000425030310001044B250003C80000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000120000000500000003000102FF7070702D00630302000002FF707070FFFFFFFF02000002FF707070FFFFFFFF02000002FF707070FFFFFFFF03000003FF707070FFFFFFFF04000103FF7078703E00130304000103FF7070703C00130304000103FF7070703C00130304000103FF7070703C00130304000103FF7070703C00130304000103FF7070703C00130303030003FF707070FFFFFFFF03031003FF707070FFFFFFFF04031004FF707070FFFFFFFF04031004FF707070FFFFFFFF05000005FF707070FFFFFFFF05000005FF707070FFFFFFFF0A000105FF7070700200590301000001FF707070FFFFFFFF01000001FF707070FFFFFFFF02000001FF707070FFFFFFFF02000002FF707070FFFFFFFF03000003FF707070FFFFFFFF03000002FF707470FFFFFFFF03000003FF7070701D00630303000003FF707070FFFFFFFF03000003FF707070FFFFFFFF03000003FF7070701D00630303000003FF707070FFFFFFFF03031003FF707470FFFFFFFF03031003FF707070FFFFFFFF04031004FF707070FFFFFFFF04031004FF707070FFFFFFFF05000005FF707070FFFFFFFF05000005FF707070FFFFFFFF05000005FF707070FFFFFFFF01000001FF707070FFFFFFFF01000001FF606B600000130301000001FF707B70FFFFFFFF02000002FF707070FFFFFFFF02000002FF707070FFFFFFFF03000002FF707070FFFFFFFF03000003FF707070FFFFFFFF03000003FF505E502300630303000003FF707070FFFFFFFF03000003FF707070FFFFFFFF03000003FF707070FFFFFFFF03031003FF707070FFFFFFFF03031003FF707070FFFFFFFF04031004FF707070FFFFFFFF04031004FF707070FFFFFFFF04000004FF707070FFFFFFFF05000005FF6064600100130305000005FF707070FFFFFFFF01000001FF707070FFFFFFFF01000001FF707070FFFFFFFF01000001FF707070FFFFFFFF02000002FF707070FFFFFFFF02000002FF707070FFFFFFFF03000003FF707470FFFFFFFF03000003FF203320FFFFFFFF03000003FF707070FFFFFFFF03000003FF7070701E00630303000003FF707070FFFFFFFF03000003FF707370FFFFFFFF03031003FF707470FFFFFFFF03031003FF707070FFFFFFFF03031002FF707070FFFFFFFF04031004FF707070FFFFFFFF04000004FF707070FFFFFFFF04000004FF707070FFFFFFFF04000004FF707070FFFFFFFF01000001FF707070FFFFFFFF02000101FF6060602D00630301000001FF707070FFFFFFFF02000002FF707070FFFFFFFF02000002FF7070702500630304000103FF7078703C00130304000103FF7078703C00130304000103FF7078703C00130304000103FF7078703C00130304000103FF7078703C00130304000103FF7070703E00130303030003FF707070FFFFFFFF08030103FF7070700300590303031003FF707070FFFFFFFF03031003FF707070FFFFFFFF03000003FF707070FFFFFFFF04000004FF707070FFFFFFFF04000004FF707070FFFFFFFF0101010100000000FFFFFFFF16000304160003041600030416000304160003040202020200000000FFFFFFFF16000304160003041600030416000304160003040101010100000100FFFFFFFF16000304160003041600030416000304160003040202020200000100FFFFFFFF16000304160003041600030416000304160003040101010100000200FFFFFFFF16000304160003041600030416000304160003040202020200000200FFFFFFFF16000304160003041600030416000304160003040101010100000300FFFFFFFF16000304160003041600030416000304160003040202020200000300FFFFFFFF16000304160003041600030416000304160003040101010103000400FFFFFFFF8E000D05160003040B0003040B000304160003040202020203000400FFFFFFFF8E000D05160003040B0003040B00030416000304030303030B000400FFFFFFFF8E000D058C000D050B0003040B000304160003040101010109000500FFFFFFFF90000D058C000D050B0003040B0003040B0003040202020209000500FFFFFFFF90000D058C000D050B0003040B0003040B0003040303030309000500FFFFFFFF90000D058C000D050B0003040B0003040B0003040101010100000600FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000600FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000600FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000700FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000700FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000700FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000800FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000800FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000800FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000900FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000900FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000900FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000A00FFFFFFFF90000D0592000D050B0003040B0003040B0003040202020200000A00FFFFFFFF90000D0592000D050B0003040B0003040B0003040303030300000A00FFFFFFFF90000D0592000D050B0003040B0003040B0003040101010100000B00FFFFFFFF16000304160003041600030416000304160003040202020200000B00FFFFFFFF16000304160003041600030416000304160003040303030302000B00FFFFFFFF8E000D05150003040B00030492000D050B0003040101010100000C00FFFFFFFF16000304160003041600030416000304160003040202020200000C00FFFFFFFF16000304160003041600030416000304160003040303030300000C00FFFFFFFF16000304150003041500030415000304150003040101010100000D00FFFFFFFF16000304160003041600030416000304160003040202020200000D00FFFFFFFF16000304160003041600030416000304160003040303030300000D00FFFFFFFF16000304160003041600030416000304160003040404040400000D00FFFFFFFF16000304150003041500030415000304150003040101010100000E00FFFFFFFF16000304160003041600030416000304160003040202020200000E00FFFFFFFF16000304160003041600030416000304160003040303030300000E00FFFFFFFF16000304160003041600030416000304160003040404040400000E00FFFFFFFF16000304150003041500030415000304150003040101010100000F00FFFFFFFF16000304160003041600030416000304160003040202020200000F00FFFFFFFF16000304160003041600030416000304160003040303030300000F00FFFFFFFF16000304160003041600030416000304160003040404040400000F00FFFFFFFF16000304160003041600030416000304160003040505050500000F00FFFFFFFF16000304150003041500030415000304150003040101010100001000FFFFFFFF16000304160003041600030416000304160003040202020200001000FFFFFFFF16000304160003041600030416000304160003040303030300001000FFFFFFFF16000304160003041600030416000304160003040404040400001000FFFFFFFF16000304160003041600030416000304160003040505050500001000FFFFFFFF16000304150003041500030415000304150003040101010100001100FFFFFFFF16000304160003041600030416000304160003040202020200001100FFFFFFFF16000304160003041600030416000304160003040303030300001100FFFFFFFF16000304160003041600030416000304160003040404040400001100FFFFFFFF16000304160003041600030416000304160003040505050500001100FFFFFFFF16000304150003041500030415000304150003040101010100000001FFFFFFFF16000304160003041600030416000304160003040101010100000101FFFFFFFF1600030416000304160003041600030416000304020202026C000201FFFFFFFF1600030492000D058C000D058C000D058C000D05020202020B010301FFFFFFFF8E000D058C000D050B0003040B00030492000D050303030309010401FFFFFFFF90000D058C000D050B0003040B00030492000D050303030300000501FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000601FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000701FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000801FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000901FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000A01FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030308000B01FFFFFFFF90000D058C000D050B00030492000D050B000304030303030A000C01FFFFFFFF8E000D058C000D050B00030492000D050B0003040404040400000D01FFFFFFFF16000304150003041500030415000304150003040404040400000E01FFFFFFFF16000304150003041500030415000304150003040505050500000F01FFFFFFFF16000304150003041500030415000304150003040505050500001001FFFFFFFF16000304150003041500030415000304150003040101010100001101FFFFFFFF16000304160003041600030416000304160003040202020200001101FFFFFFFF16000304160003041600030416000304160003040303030300001101FFFFFFFF16000304160003041600030416000304160003040404040400001101FFFFFFFF16000304160003041600030416000304160003040505050500001101FFFFFFFF16000304150003041500030415000304150003040101010100000002FFFFFFFF1600030416000304160003041600030416000304010101016C000102FFFFFFFF1600030492000D058C000D058C000D058C000D05010101010B000202FFFFFFFF94000D058C000D050B0003040B000304160003040202020209010302FFFFFFFF90000D058C000D050B0003040B00030492000D050202020200000402FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000502FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000602FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000702FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000802FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000902FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000A02FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000B02FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030308000C02FFFFFFFF92000D058C000D050B00030492000D050B0003040404040400000D02FFFFFFFF16000304150003041500030415000304150003040404040400000E02FFFFFFFF16000304150003041500030415000304150003040404040400000F02FFFFFFFF16000304150003041500030415000304150003040505050500001002FFFFFFFF16000304150003041500030415000304150003040101010100001102FFFFFFFF16000304160003041600030416000304160003040202020200001102FFFFFFFF16000304160003041600030416000304160003040303030300001102FFFFFFFF16000304160003041600030416000304160003040404040400001102FFFFFFFF16000304160003041600030416000304160003040505050500001102FFFFFFFF16000304150003041500030415000304150003040101010100000003FFFFFFFF1600030416000304160003041600030416000304010101010B010103FFFFFFFF94000D058C000D050B0003040B00030492000D050101010109010203FFFFFFFF93000D058C000D050B0003040B00030492000D050202020200000303FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000403FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000503FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000603FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000703FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000803FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000903FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000A03FFFFFFFF0C0003040B0003040B0003040B0003040B000304030303030B000B03FFFFFFFF93000D058C000D050B00030492000D050B0003040303030309000C03FFFFFFFF8E000D058C000D050B00030492000D050B0003040303030300000D03FFFFFFFF16000304150003041500030415000304150003040404040400000E03FFFFFFFF16000304150003041500030415000304150003040404040400000F03FFFFFFFF16000304150003041500030415000304150003040404040400001003FFFFFFFF16000304150003041500030415000304150003040101010100001103FFFFFFFF16000304160003041600030416000304160003040202020200001103FFFFFFFF16000304160003041600030416000304160003040303030300001103FFFFFFFF16000304160003041600030416000304160003040404040400001103FFFFFFFF16000304150003041500030415000304150003040101010100000004FFFFFFFF1600030416000304160003041600030416000304010101018A010104FFFFFFFF8C000D058C000D050B0003048C000D0592000D050101010100000204FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000304FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000304FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000404FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000404FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000504FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000504FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000504FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000604FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000604FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000604FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010102000704FFFFFFFF0D0003040B0003040B0003040B0003040B0003040202020200000704FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000704FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000804FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000804FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000804FFFFFFFF0C0003040B0003040B0003040B0003040B0003040101010100000904FFFFFFFF0C0003040B0003040B0003040B0003040B0003040202020200000904FFFFFFFF0C0003040B0003040B0003040B0003040B0003040303030300000904FFFFFFFF0C0003040B0003040B0003040B0003040B000304010101010B000A04FFFFFFFF90000D058C000D050B00030492000D050B000304020202020B000A04FFFFFFFF90000D058C000D050B00030492000D050B000304030303030B000A04FFFFFFFF90000D058C000D050B00030492000D050B0003040101010109000B04FFFFFFFF8E000D058C000D050B000304150003040B0003040202020209000B04FFFFFFFF8E000D058C000D050B000304150003040B0003040303030309000B04FFFFFFFF8E000D058C000D050B000304150003040B0003040101010100000C04FFFFFFFF16000304160003041600030416000304160003040202020200000C04FFFFFFFF1600030416000304160003041600030416000304030303032C000C04FFFFFFFF1600030492000D058C000D05150003048C000D050101010100000D04FFFFFFFF16000304160003041600030416000304160003040202020200000D04FFFFFFFF1600030416000304160003041600030416000304030303032C000D04FFFFFFFF1600030492000D058C000D05150003048C000D050101010100000E04FFFFFFFF16000304160003041600030416000304160003040202020200000E04FFFFFFFF1600030416000304160003041600030416000304030303032C000E04FFFFFFFF1600030492000D058C000D05150003048C000D050101010100000F04FFFFFFFF16000304160003041600030416000304160003040202020200000F04FFFFFFFF1600030416000304160003041600030416000304030303032C000F04FFFFFFFF1600030492000D058C000D05150003048C000D050101010100001004FFFFFFFF16000304160003041600030416000304160003040202020200001004FFFFFFFF16000304160003041600030416000304160003040303030300001004FFFFFFFF1600030416000304160003041600030416000304040404042C001004FFFFFFFF1600030492000D058C000D05150003048C000D050101010100001104FFFFFFFF16000304160003041600030416000304160003040202020200001104FFFFFFFF16000304160003041600030416000304160003040303030300001104FFFFFFFF16000304160003041600030416000304160003040404040400001104FFFFFFFF16000304150003041500030415000304150003040400");
        }
    }
    public sealed class GameRoom_BattleInfo_0x41C : NetPacket
    {
        public GameRoom_BattleInfo_0x41C()
        {
            ns.WriteHex("001C0400000E000000312632263026302630263026300000");
        }
    }
    public sealed class GameRoom_BattleUnitSet_0x3D3 : NetPacket
    {
        public GameRoom_BattleUnitSet_0x3D3(Account battleTeam, Account battledTeam, int isOther)
        {
            ns.Write((byte)0x00);
            ns.Write((short)979); //0x3D3  op code
            ns.Write((short)0);
            ns.WriteHex("DF0D0000");
            //ns.WriteDynamicASCII("1&10000&10015:1000D:1179C:1179D:1179E:1179F:117A0:117A1&A010014:A010016:A010017&2680231:26A0028:2752739:20B002D:2030019:27B000A:27E000A&F0#FC#A8#FF:F8#FC#F8#FF:F0#E4#B8#FF:FF#FF#FF#FF:FF#FF#FF#FF:FF#FF#FF#FF&100&100&0&4&4&2&1&0&192&192&10&200&33&102&3&70&29&0&255&0&1&0&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&&2#3:3#3:4#3:5#3:6#3:7#3:&0#3:3#3:4#3:&aaa&&$15&0&1F:2B:432:433:434:435:436:437&A00001E:A000020:A000021&&68#CC#68#FF:F8#E4#D0#FF:70#CC#B0#FF:FF#FF#FF#FF:FF#FF#FF#FF:FF#FF#FF#FF&100&100&0&13&3&3&3&1&227&227&10&200&32&105&100&55&28&0&255&0&1&0&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:12:0:0:0:0:0:0:0:0:&0:1:0:0:0:0:0:0:0:0:&&2#3:3#3:4#3:5#3:6#3:7#3:&0#3:2#3:&jjj&&");
            ns.WriteDynamicASCII("1&");
            if (battledTeam.Gender == 0)
            {
                ns.WriteDynamicASCII("0");
            }
            else
            {
                ns.WriteDynamicASCII("10000");
            }
            ns.WriteDynamicASCII("&");
            for (int i = 0; i < 8; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(battledTeam.CharacterOneEquipment[i].ToString("X"));
            }
            ns.WriteDynamicASCII("&");
            if (battledTeam.CharacterDecodedHairClump1 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battledTeam.CharacterDecodedHairClump1)).ToString("X"));
            }
            if (battledTeam.CharacterDecodedHairClump2 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                if (battledTeam.CharacterDecodedHairClump1 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battledTeam.CharacterDecodedHairClump2)).ToString("X"));
            }
            if (battledTeam.CharacterDecodedHairClump3 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                if (battledTeam.CharacterDecodedHairClump2 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battledTeam.CharacterDecodedHairClump3)).ToString("X"));
            }
            if (battledTeam.CharacterDecodedHairClump2 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                if (battledTeam.CharacterDecodedHairClump2 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battledTeam.CharacterDecodedHairClump4)).ToString("X"));
            }
            ns.WriteDynamicASCII("&&");

            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII("#");
                }
                if (battledTeam.CharacterDecodedCloth[i] != 0x00)
                {
                    ns.WriteDynamicASCII(battledTeam.CharacterDecodedCloth[i].ToString("X2"));
                }
                else
                {
                    ns.WriteDynamicASCII("FF");
                }
            }
            ns.WriteDynamicASCII(":");
            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII("#");
                }
                if (battledTeam.CharacterDecodedSkin[i] != 0x00)
                {
                    ns.WriteDynamicASCII(battledTeam.CharacterDecodedSkin[i].ToString("X2"));
                }
                else
                {
                    ns.WriteDynamicASCII("FF");
                }
            }
            ns.WriteDynamicASCII(":");
            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII("#");
                }
                if (battledTeam.CharacterDecodedHair[i] != 0x00)
                {
                    ns.WriteDynamicASCII(battledTeam.CharacterDecodedHair[i].ToString("X2"));
                }
                else
                {
                    ns.WriteDynamicASCII("FF");
                }
            }
            ns.WriteDynamicASCII(":FF#FF#FF#FF:FF#FF#FF#FF:FF#FF#FF#FF&100&100&0&");
            if (isOther == 0)
            {
                ns.WriteDynamicASCII("4");
            }
            else
            {
                ns.WriteDynamicASCII("13");
            }
            ns.WriteDynamicASCII("&2"); //char pos
            ns.WriteDynamicASCII("&2");
            if (isOther == 0)
            {
                ns.WriteDynamicASCII("&1"); //char dir
            }
            else
            {
                ns.WriteDynamicASCII("&3"); //char dir
            }
            ns.WriteDynamicASCII("&0"); //char yellow name
            ns.WriteDynamicASCII("&100"); //hp current
            ns.WriteDynamicASCII("&100"); //hp max
            ns.WriteDynamicASCII("&10"); //ap current
            ns.WriteDynamicASCII("&200");
            ns.WriteDynamicASCII("&33"); //enermy leader
            ns.WriteDynamicASCII("&102");
            ns.WriteDynamicASCII("&3"); //usage of weapon
            ns.WriteDynamicASCII("&70"); //char lv
            ns.WriteDynamicASCII("&29&0&255&0&1&0&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&&");
            ns.WriteDynamicASCII("2#");
            ns.WriteDynamicASCII(battledTeam.CharacterOneSEffect[0].ToString());
            ns.WriteDynamicASCII(":3#");
            ns.WriteDynamicASCII(battledTeam.CharacterOneSEffect[1].ToString());
            ns.WriteDynamicASCII(":4#");
            ns.WriteDynamicASCII(battledTeam.CharacterOneSEffect[2].ToString());
            ns.WriteDynamicASCII(":5#");
            ns.WriteDynamicASCII(battledTeam.CharacterOneSEffect[3].ToString());
            ns.WriteDynamicASCII(":6#");
            ns.WriteDynamicASCII(battledTeam.CharacterOneSEffect[4].ToString());
            ns.WriteDynamicASCII(":7#");
            ns.WriteDynamicASCII(battledTeam.CharacterOneSEffect[5].ToString());
            ns.WriteDynamicASCII(":&0#3:3#3:4#3:");

            ns.WriteDynamicASCII("&"); //char name
            ns.WriteBIG5FixedNoSize(battledTeam.CharacterNickname1);
            ns.WriteDynamicASCII("&&");

            ns.WriteDynamicASCII("$15&");
            if (battleTeam.Gender == 0)
            {
                ns.WriteDynamicASCII("0");
            }
            else
            {
                ns.WriteDynamicASCII("10000");
            }
            ns.WriteDynamicASCII("&");
            for (int i = 0; i < 8; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(battleTeam.CharacterOneEquipment[i].ToString("X"));
            }
            ns.WriteDynamicASCII("&");
            if (battleTeam.CharacterDecodedHairClump1 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battleTeam.CharacterDecodedHairClump1)).ToString("X"));
            }
            if (battleTeam.CharacterDecodedHairClump2 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                if (battleTeam.CharacterDecodedHairClump1 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battleTeam.CharacterDecodedHairClump2)).ToString("X"));
            }
            if (battleTeam.CharacterDecodedHairClump3 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                if (battleTeam.CharacterDecodedHairClump2 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battleTeam.CharacterDecodedHairClump3)).ToString("X"));
            }
            if (battleTeam.CharacterDecodedHairClump4 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
            {
                if (battleTeam.CharacterDecodedHairClump3 != Encrypt.StringToByteArrayFastest("0xFFFFFFFF"))
                {
                    ns.WriteDynamicASCII(":");
                }
                ns.WriteDynamicASCII(Encrypt.decodedDynamicBytes(Encrypt.encodedHairClump(battleTeam.CharacterDecodedHairClump4)).ToString("X"));
            }
            ns.WriteDynamicASCII("&&");

            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII("#");
                }
                if (battleTeam.CharacterDecodedCloth[i] != 0x00)
                {
                    ns.WriteDynamicASCII(battleTeam.CharacterDecodedCloth[i].ToString("X2"));
                }
                else
                {
                    ns.WriteDynamicASCII("FF");
                }
            }
            ns.WriteDynamicASCII(":");
            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII("#");
                }
                if (battleTeam.CharacterDecodedSkin[i] != 0x00)
                {
                    ns.WriteDynamicASCII(battleTeam.CharacterDecodedSkin[i].ToString("X2"));
                }
                else
                {
                    ns.WriteDynamicASCII("FF");
                }
            }
            ns.WriteDynamicASCII(":");
            for (int i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    ns.WriteDynamicASCII("#");
                }
                if (battleTeam.CharacterDecodedHair[i] != 0x00)
                {
                    ns.WriteDynamicASCII(battleTeam.CharacterDecodedHair[i].ToString("X2"));
                }
                else
                {
                    ns.WriteDynamicASCII("FF");
                }
            }

            ns.WriteDynamicASCII(":FF#FF#FF#FF:FF#FF#FF#FF:FF#FF#FF#FF&100&100&0&");
            if (isOther == 0)
            {
                ns.WriteDynamicASCII("13");
            }
            else
            {
                ns.WriteDynamicASCII("4");
            }
            ns.WriteDynamicASCII("&2"); //char pos
            ns.WriteDynamicASCII("&3");
            if (isOther == 0)
            {
                ns.WriteDynamicASCII("&3"); //char dir
            }
            else
            {
                ns.WriteDynamicASCII("&1"); //char dir
            }
            ns.WriteDynamicASCII("&1"); //char yellow name
            ns.WriteDynamicASCII("&100"); //hp current
            ns.WriteDynamicASCII("&100"); //hp max
            ns.WriteDynamicASCII("&10"); //ap current
            ns.WriteDynamicASCII("&200");
            ns.WriteDynamicASCII("&33");
            ns.WriteDynamicASCII("&102");
            ns.WriteDynamicASCII("&3"); //usage of weapon
            ns.WriteDynamicASCII("&60"); //char lv
            ns.WriteDynamicASCII("&28&0&255&0&1&0&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:12:0:0:0:0:0:0:0:0:&0:1:0:0:0:0:0:0:0:0:&&");
            ns.WriteDynamicASCII("2#");
            ns.WriteDynamicASCII(battleTeam.CharacterOneSEffect[0].ToString());
            ns.WriteDynamicASCII(":3#");
            ns.WriteDynamicASCII(battleTeam.CharacterOneSEffect[1].ToString());
            ns.WriteDynamicASCII(":4#");
            ns.WriteDynamicASCII(battleTeam.CharacterOneSEffect[2].ToString());
            ns.WriteDynamicASCII(":5#");
            ns.WriteDynamicASCII(battleTeam.CharacterOneSEffect[3].ToString());
            ns.WriteDynamicASCII(":6#");
            ns.WriteDynamicASCII(battleTeam.CharacterOneSEffect[4].ToString());
            ns.WriteDynamicASCII(":7#");
            ns.WriteDynamicASCII(battleTeam.CharacterOneSEffect[5].ToString());
            ns.WriteDynamicASCII(":&0#3:2#3:");

            //ns.WriteDynamicASCII("&29&0&255&0&1&0&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&&2#3:3#3:4#3:5#3:6#3:7#3:&0#3:3#3:4#3:");
            //ns.WriteDynamicASCII("&28&0&255&0&1&0&0:0:0:0:0:0:0:0:0:0:&0:0:0:0:0:0:0:0:0:0:&0:12:0:0:0:0:0:0:0:0:&0:1:0:0:0:0:0:0:0:0:&&2#3:3#3:4#3:5#3:6#3:7#3:&0#3:2#3:");
            ns.WriteDynamicASCII("&"); //char name
            ns.WriteBIG5FixedNoSize(battleTeam.CharacterNickname1);
            ns.WriteDynamicASCII("&&");

            ns.Write((byte)0x00);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_BattleS2CPrivate_0x3C2 : NetPacket
    {
        public GameRoom_BattleS2CPrivate_0x3C2()
        {
            ns.WriteHex("00C2030000130000003135263726322632263230343526302630263100");
        }
    }
    public sealed class GameRoom_BattleMove_0x41E : NetPacket
    {
        public GameRoom_BattleMove_0x41E(BattleRecord battle, int isOther)
        {
            ns.Write((byte)0x00);
            ns.Write((short)1054); //0x41E  op code
            ns.Write((short)0);
            ns.Write((byte)0x03);
            ns.Write((byte)0x05);
            ns.Write((short)0);
            ns.WriteDynamicASCII("&TUN#1#0:");
            if (isOther == 0)
            {
                ns.WriteDynamicASCII(battle.BattleMoveData[0]);
                ns.WriteDynamicASCII(battle.BattleMoveData[1]);
            }
            else
            {
                ns.WriteDynamicASCII(battle.BattledMoveData[0]);
                ns.WriteDynamicASCII(battle.BattledMoveData[1]);
            }
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_TradeCancel : NetPacket
    {
        public GameRoom_TradeCancel()
        {
            ns.Write((byte)0x00);
            ns.Write((short)1325); //0x52D  op code
            ns.Write((short)0);
            ns.Write((byte)0x05);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)0x02);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_TradeLock : NetPacket
    {
        public GameRoom_TradeLock(bool isLock)
        {
            int tradeLock = 1;
            if (isLock)
            {
                tradeLock = 0;
            }
            ns.Write((byte)0x00);
            ns.Write((short)1325); //0x52D  op code
            ns.Write((short)0);
            ns.Write((byte)0x04);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)tradeLock);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_TradeConfirm : NetPacket
    {
        public GameRoom_TradeConfirm(bool isConfirm)
        {
            int confirm = 0;
            if(isConfirm)
            {
                confirm = 1;
            }
            ns.Write((byte)0x00);
            ns.Write((short)1325); //0x52D  op code
            ns.Write((short)0);
            ns.Write((byte)0x02);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            ns.Write((byte)confirm);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_TradeSuccess : NetPacket
    {
        public GameRoom_TradeSuccess()
        {
            ns.Write((byte)0x00);
            ns.Write((short)1325); //0x52D  op code
            ns.Write((short)0);
            ns.Write((byte)0x05);
            ns.Write(0);
            ns.Write(0);
            ns.Write((byte)0x00);
        }
    }
    public sealed class GameRoom_TradeItem : NetPacket
    {
        public GameRoom_TradeItem(List<ItemAttr> TradeItem, byte[] tradeMoney)
        {
            ns.Write((byte)0x00);
            ns.Write((short)1325); //0x52D  op code
            ns.Write((short)0);
            ns.Write((byte)0x01);
            ns.Write(0);
            ns.Write((short)0);
            ns.Write((byte)0x00);
            //ns.Write((byte)0x00);
            ns.Write(tradeMoney, 0);
            for (int i = 0; i < TradeItem.Count; i++)
            {
                if (TradeItem[i].ItemTypeNum == 1)
                {
                    ns.Write((byte)i);
                    ns.WriteHex("01A70F876981F8008F5182B880803A8F52378F53008F548A788F55853C8F56018F5800900100");
                    //A70F876981F800 8F5182B880803A 8F5237 8F5300 8F548A78 8F55853C 8F5601 8F5800
                    //8F5237 = bottom attr
                    ns.Write((short)0x0290);
                    ns.Write(TradeItem[i].ItemEncodedID, 0);
                    ns.WriteHex("902282B8808038902301902500902800902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F");
                    ns.Write((short)0x4290);
                    ns.Write((byte)0x00);
                    ns.Write((byte)0xA7);
                    ns.Write((byte)0x12);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemName);
                    ns.Write((short)0x219F);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemDesc);
                    ns.Write((byte)0xA7);
                    ns.Write((byte)0x15);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemType);
                    ns.Write((byte)0xA7);
                    ns.Write((byte)0x16);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemUsageLimit);
                    ns.WriteHex("902B00902D00A717042D2D2D2DA71087F033A71100A73600A73700900100A73800A73A00");
                    ns.Write((byte)0xA7);
                    ns.Write((byte)0x39);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemValidDate);
                    //ns.WriteHex("A73005A7310FA73505");
                    for (int j = 0; j < 15; j++)
                    {
                        if (TradeItem[i].ItemFoodAttackEffect[j] == 0)
                        {
                            continue;
                        }
                        else
                        {
                            ns.Write((byte)0xA7);
                            ns.Write((byte)(24 + j));
                            ns.Write((byte)(TradeItem[i].ItemFoodAttackEffect[j]));
                        }
                    }
                    for (int k = 0; k < 15; k++)
                    {
                        if (TradeItem[i].ItemFoodDefenceEffect[k] == 0)
                        {
                            continue;
                        }
                        else
                        {
                            ns.Write((byte)0xA7);
                            ns.Write((byte)(39 + k));
                            ns.Write((byte)(TradeItem[i].ItemFoodDefenceEffect[k]));
                        }
                    }
                    ns.Write((byte)0xA7);
                    ns.Write((byte)0x3B);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemCombinable);
                    ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
                }
                else
                {
                    //005E030000E73600000001A70F876981FF008F5199D080038F52038F53068F549CF0708F55A5598F56018F58028F5A148F5C008F5E148F60008F62148F64008F66148F68008F6A148F6C008F6E148F70008F72148F74008F76148F78008F7A148F7C008F7E14900000900100900286B08C802B9017908C802B90228FFFFFFF7F902301902500902816902900902E00902F00903200903300903400903500903600903700903A00903B00903D00903E00903F0090408FFFFFFF7F904200A71212A5ECA5A4A5AAA5D6A5ECA5C3A5C9A1DCADB69F215CBDD1BCD4A4CECBE2CECFA4CBB8C6B1FEA4B7A4C6A1A2BFF4BEF2A4CEB9C8A4A4B8F7A4F2CAFCA4C4BEF3A1A3BDD1BCD4A4CECBE2CECFA4ACB9E2A4A4A4DBA4C9A1A2CAFCA4BFA4ECA4EBB8F7A4CEC0FEA4CFC2BFA4AFA4CAA4EBA1A3A71502BEF3A7160B4C56353220CFA3BCA1BEA4902B41902D46A717053130302025A70900A70A00A70B00A70C00A70D81C328A70E00A7108F8620A71100A73686A8B48066A73700900100A73800A73A00A73904A4CAA4B7A73B04C9D4B2C490408FFFFFFF7F904200A73C008FFFFFFF7F8FFFFFFF7F00
                    //Equ Type: 2 = Robe, 3 = Shoes, 4 = Weapon
                    ns.Write((byte)i);
                    ns.WriteHex("01A70F876981FF008F5181B88480048F5217");
                    ns.Write((short)0x538F);
                    if (TradeItem[i].ItemTypeNum == 2) //1. Advanced Wear Position
                    {
                        ns.Write((byte)0x02);
                    }
                    else if (TradeItem[i].ItemTypeNum == 3)
                    {
                        ns.Write((byte)0x02);
                    }
                    else if (TradeItem[i].ItemTypeNum == 4)
                    {
                        ns.Write((byte)0x02);
                    }
                    else if (TradeItem[i].ItemTypeNum == 5)
                    {
                        ns.Write((byte)0x03);
                    }
                    else if (TradeItem[i].ItemTypeNum == 6)
                    {
                        ns.Write((byte)0x04);
                    }
                    else if (TradeItem[i].ItemTypeNum == 7)
                    {
                        ns.Write((byte)0x05);
                    }
                    else if (TradeItem[i].ItemTypeNum == 9)
                    {
                        ns.Write((byte)0x06);
                    }
                    ns.WriteHex("8F5485238F55308F56018F5800");

                    for (int j = 0; j < 10; j++) //2. Wear Attack & Defence
                    {
                        ns.Write((byte)0x8F);
                        ns.Write((byte)(90 + j * 4));
                        ns.Write((byte)(TradeItem[i].ItemEquipmentAttackEffect[j]));

                        if (j < 9)
                        {
                            ns.Write((byte)0x8F);
                            ns.Write((byte)(90 + j * 4 + 2));
                            ns.Write((byte)(TradeItem[i].ItemEquipmentDefenceEffect[j]));
                        }
                        else
                        {
                            ns.Write((short)0x0090);
                            ns.Write((byte)(TradeItem[i].ItemEquipmentDefenceEffect[j]));
                        }
                    }

                    ns.Write((short)0x0190); //3. Wear Type
                    if (TradeItem[i].ItemTypeNum == 2)
                    {
                        ns.Write((byte)0x0E);
                    }
                    else if (TradeItem[i].ItemTypeNum == 3)
                    {
                        ns.Write((byte)0x06);
                    }
                    else if (TradeItem[i].ItemTypeNum == 4)
                    {
                        ns.Write((byte)0x02);
                    }
                    else if (TradeItem[i].ItemTypeNum == 5)
                    {
                        ns.Write((byte)0x04);
                    }
                    else if (TradeItem[i].ItemTypeNum == 6)
                    {
                        ns.Write((byte)0x08);
                    }
                    else if (TradeItem[i].ItemTypeNum == 7)
                    {
                        ns.Write((byte)0x10);
                    }
                    else if (TradeItem[i].ItemTypeNum == 9)
                    {
                        ns.Write((byte)0x00);
                    }

                    ns.Write((short)0x0290); //4. ItemID
                    ns.Write(TradeItem[i].ItemEncodedID, 0);

                    if (TradeItem[i].ItemTypeNum == 2) //5. Advanced Wear Position 2
                    {
                        ns.WriteHex("9005AF269006AF279007AF289008AF29");
                    }
                    else if (TradeItem[i].ItemTypeNum == 3)
                    {
                        ns.WriteHex("9008AF29");
                    }
                    else if (TradeItem[i].ItemTypeNum == 7)
                    {
                        ns.WriteHex("9009AF2A900AAF2B");
                    }
                    else if (TradeItem[i].ItemTypeNum == 9)
                    {
                        ns.WriteHex("9017908C802A");
                    }
                    ns.WriteHex("90228FFFFFFF7F902301902500");

                    ns.Write((short)0x2890); //6. Item Weight
                    ns.Write(Encrypt.encodeMultiBytes(TradeItem[i].ItemWeight), 0);
                    ns.WriteHex("902900902E00902F0090320090338768903400903500903600903700903A03903B00903D00903E00903F0090408FFFFFFF7F904200");

                    ns.Write((byte)0xA7); //7. Item Name
                    ns.Write((byte)0x12);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemName);

                    ns.Write((short)0x219F); //8. Item Description
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemDesc);

                    ns.Write((byte)0xA7); //9. Item Type Name
                    ns.Write((byte)0x15);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemType);

                    ns.Write((byte)0xA7); //10. Item Usage Limit
                    ns.Write((byte)0x16);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemUsageLimit);

                    ns.Write((short)0x2B90); //11. Item Physical Damage
                    if (TradeItem[i].ItemTypeNum == 9)
                    {
                        ns.Write((byte)TradeItem[i].ItemPhysicalDamage);
                    }
                    else
                    {
                        ns.Write((byte)0x00);
                    }

                    ns.Write((short)0x2D90); //12. Item Magic Damage
                    if (TradeItem[i].ItemTypeNum == 9)
                    {
                        ns.Write((byte)TradeItem[i].ItemMagicDamage);
                    }
                    else
                    {
                        ns.Write((byte)0x00);
                    }
                    ns.WriteHex("A7170438312025");
                    ns.Write((short)0x09A7); //13. Item isWear
                    ns.Write((byte)TradeItem[i].ItemWear);
                    ns.WriteHex("A70A00A70B00A70C00A70D00A70E00");

                    ns.Write((short)0x10A7); //14.Item Durability
                    ns.WriteHex("8DBE15");
                    ns.WriteHex("A71100");

                    ns.Write((short)0x36A7); //15. Equipment Type (A73686A8B48064 = S)
                    if (TradeItem[i].ItemCorrect == 0)
                    {
                        ns.Write((byte)0x00);
                        //ns.WriteHex("86A8B4800D"); //F
                    }
                    else if (TradeItem[i].ItemCorrect == 1)
                    {
                        ns.WriteHex("86A8B48065");
                    }
                    else if (TradeItem[i].ItemCorrect == 2)
                    {
                        ns.WriteHex("86A8B48066");
                    }
                    else if (TradeItem[i].ItemCorrect == 3)
                    {
                        ns.WriteHex("86A8B48064");
                    }
                    ns.WriteHex("A73700");

                    ns.Write((short)0x0190); //16. Wear Type
                    if (TradeItem[i].ItemTypeNum == 2)
                    {
                        ns.Write((byte)0x0E);
                    }
                    else if (TradeItem[i].ItemTypeNum == 3)
                    {
                        ns.Write((byte)0x06);
                    }
                    else if (TradeItem[i].ItemTypeNum == 4)
                    {
                        ns.Write((byte)0x02);
                    }
                    else if (TradeItem[i].ItemTypeNum == 5)
                    {
                        ns.Write((byte)0x04);
                    }
                    else if (TradeItem[i].ItemTypeNum == 6)
                    {
                        ns.Write((byte)0x08);
                    }
                    else if (TradeItem[i].ItemTypeNum == 7)
                    {
                        ns.Write((byte)0x10);
                    }
                    else if (TradeItem[i].ItemTypeNum == 9)
                    {
                        ns.Write((byte)0x00);
                    }
                    ns.WriteHex("A73800A73A00");

                    ns.Write((byte)0xA7); //17. Item Valid Date
                    ns.Write((byte)0x39);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemValidDate);

                    ns.Write((byte)0xA7); //18. Item Combinable
                    ns.Write((byte)0x3B);
                    ns.WriteBIG5FixedWithSize(TradeItem[i].ItemCombinable);
                    ns.WriteHex("90408FFFFFFF7F904200A73C008FFFFFFF7F");
                }
            }
            ns.WriteHex("8FFFFFFF7F00");
        }
    }
    public sealed class GameRoom_ChatToAll : NetPacket
    {
        public GameRoom_ChatToAll(Account User, byte fontColor, byte fontSize, byte wordlen, string word)
        {
            ns.Write((byte)0x00);
            ns.Write((short)466); //0x1D2  op code
            ns.Write((short)0);
            ns.Write((byte)fontColor);
            ns.Write((byte)fontSize);
            ns.Write((short)0);
            ns.Write(User.GlobalID);
            ns.Write(0);
            byte[] bytes = Encoding.UTF8.GetBytes(User.CharacterNickname1);
            ns.Write(bytes, 0);
            ns.Write((short)0x203A);
            ns.WriteUTF8NoSize(word, wordlen);
        }
    }
    public sealed class GameRoom_PlayerPosition : NetPacket
    {
        public GameRoom_PlayerPosition(Account User)
        {
            switch (User.GameDirection)
            {
                case 0xA: //10
                    User.GamePosY -= 1;
                    break;
                case 0xB: //11
                    User.GamePosX += 1;
                    User.GamePosY -= 1;
                    break;
                case 0xC: //12
                    User.GamePosX += 1;
                    break;
                case 0xD: //13
                    User.GamePosX += 1;
                    User.GamePosY += 1;
                    break;
                case 0xE: //14
                    User.GamePosY += 1;
                    break;
                case 0xF: //15
                    User.GamePosX -= 1;
                    User.GamePosY += 1;
                    break;
                case 0x10: //16
                    User.GamePosX -= 1;
                    break;
                case 0x11: //17
                    User.GamePosX -= 1;
                    User.GamePosY -= 1;
                    break;

                case 0x14: //20
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x15)
                    {
                        User.GamePosX += 1;
                    }
                    else if (User.GameDirection2 == 0x16)
                    {
                        User.GamePosX += 1;
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x1A)
                    {
                        User.GamePosX -= 1;
                        User.GamePosY += 1;
                    }
                    User.GamePosY -= 2;
                    break;
                case 0x15: //21
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosX -= 1;
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x16)
                    {
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x14)
                    {
                        User.GamePosX -= 1;
                    }
                    User.GamePosX += 2;
                    User.GamePosY -= 2;
                    break;
                case 0x16:
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosX -= 1;
                    }
                    else if (User.GameDirection2 == 0x14)
                    {
                        User.GamePosX -= 1;
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GamePosX -= 1;
                        User.GamePosY += 1;
                    }
                    User.GamePosX += 2;
                    break;
                case 0x17:
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosX -= 1;
                        User.GamePosY -= 1;
                    }
                    else if(User.GameDirection2 == 0x16)
                    {
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GamePosX -= 1;
                    }
                    User.GamePosX += 2;
                    User.GamePosY += 2;
                    break;
                case 0x18:
                    if(User.GameDirection2 == 0xFF)
                    {
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x16)
                    {
                        User.GamePosX += 1;
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x17)
                    {
                        User.GamePosX += 1;
                    }
                    else if (User.GameDirection2 == 0x19) //
                    {
                        User.GamePosX -= 1;
                    }
                    else if (User.GameDirection2 == 0x1A)
                    {
                        User.GamePosX -= 1;
                        User.GamePosY -= 1;
                    }
                    User.GamePosY += 2;
                    break;
                case 0x19:
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosX += 1;
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x1A)
                    {
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GamePosX += 1;
                    }
                    User.GamePosX -= 2;
                    User.GamePosY += 2;
                    break;
                case 0x1A:
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosX += 1;
                    }
                    else if (User.GameDirection2 == 0x14)
                    {
                        User.GamePosX += 1;
                        User.GamePosY -= 1;
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GamePosX += 1;
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x19)
                    {
                        User.GamePosY += 1;
                    }
                    User.GamePosX -= 2;
                    break;
                case 0x1B:
                    if (User.GameDirection2 == 0xFF)
                    {
                        User.GamePosX += 1;
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x1A)
                    {
                        User.GamePosY += 1;
                    }
                    else if (User.GameDirection2 == 0x14)
                    {
                        User.GamePosX += 1;
                    }
                    User.GamePosX -= 2;
                    User.GamePosY -= 2;
                    break; 
            }

            switch (User.GameDirection)
            {
                case 20:
                    if (User.GameDirection2 == 0x1A)
                    {
                        User.GameDirection = 26;
                    }
                    else if (User.GameDirection2 == 0x15)
                    {
                        User.GameDirection = 1; //
                    }
                    else if (User.GameDirection2 == 0x16)
                    {
                        User.GameDirection = 22;
                    }
                    else if (User.GameDirection2 == 0x1B)
                    {
                        User.GameDirection = 7; //
                    }
                    break;
                case 21:
                    if (User.GameDirection2 == 0x14)
                    {
                        User.GameDirection = 20;
                    }
                    else if (User.GameDirection2 == 0x16)
                    {
                        User.GameDirection = 22;
                    }
                    break;
                case 22:
                    if (User.GameDirection2 == 0x14)
                    {
                        User.GameDirection = 20; 
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GameDirection = 24;
                    }
                    break;
                case 23:
                    if (User.GameDirection2 == 0x16)
                    {
                        User.GameDirection = 22;
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GameDirection = 24;
                    }
                    break;
                case 24:
                    if (User.GameDirection2 == 0x1A)
                    {
                        User.GameDirection = 26;
                    }
                    else if (User.GameDirection2 == 0x16)
                    {
                        User.GameDirection = 22;
                    }
                    else if (User.GameDirection2 == 0x17)
                    {
                        User.GameDirection = 3; //
                    }
                    else if (User.GameDirection2 == 0x19)
                    {
                        User.GameDirection = 25; //
                    }
                    break;
                case 25:
                    if (User.GameDirection2 == 0x18)
                    {
                        User.GameDirection = 24;
                    }
                    else if (User.GameDirection2 == 0x1A)
                    {
                        User.GameDirection = 26;
                    }
                    break;
                case 26:
                    if (User.GameDirection2 == 0x14)
                    {
                        User.GameDirection = 20; 
                    }
                    else if (User.GameDirection2 == 0x18)
                    {
                        User.GameDirection = 24;
                    }
                    else if (User.GameDirection2 == 0x19)
                    {
                        User.GameDirection = 25;
                    }
                    break;
                case 27:
                    if (User.GameDirection2 == 0x14)
                    {
                        User.GameDirection = 20;
                    }
                    else if (User.GameDirection2 == 0x1A)
                    {
                        User.GameDirection = 26;
                    }
                    break;
            }
            if (User.GameDirection > 0x8)
            {
                ns.Write((byte)0x00);
                ns.Write((short)1005); //0x3ED  op code
                ns.Write((short)0);
                ns.Write(User.GlobalID);
                ns.Write((short)User.GamePosX);
                ns.Write((short)User.GamePosY);
                ns.Write((byte)0x00);
                ns.Write((byte)0x02);
                ns.Write((byte)0x11);
                ns.Write((byte)0x27);
                ns.Write((byte)User.GameDirection);
                ns.Write((byte)0xFF);
                ns.Write((byte)0xFF);
                ns.Write((byte)0xFF);
                ns.Write((byte)0x00);
            }
            else
            {
                ns.Write((byte)0x00);
                ns.Write((short)1005); //0x3ED  op code
                ns.Write((short)0);
                ns.Write(User.GlobalID);
                ns.Write((short)User.GamePosX);
                ns.Write((short)User.GamePosY);
                ns.Write((byte)0x00);
                ns.Write((byte)0x02);
                ns.Write((byte)0x10);
                ns.Write((byte)0x27);
                ns.Write((byte)User.GameDirection);
                ns.Write((byte)0xFF);
                ns.Write((byte)0xFF);
                ns.Write((byte)0xFF);
                ns.Write((byte)0x00);
            }
        }
    }
    public sealed class GameRoom_SendRoomInfo : NetPacket
    {
        public GameRoom_SendRoomInfo(NormalRoom room, byte last, byte roompos = 0)
        {
            byte roomkindid = (byte)room.RoomKindID;
            string name = room.Name;
            string password = room.Password;
            int isTeamPlay = room.IsTeamPlay;
            bool isStepOn = room.IsStepOn;
            int itemtype = room.ItemType;

            //byte header = 0xA5;
            ns.Write((byte)0xA5); //opcode
            ns.Write(0);
            ns.Write(roomkindid);
            ns.WriteBIG5Fixed_intSize(name);
            ns.Write(room.MaxPlayersCount);
            ns.WriteBIG5Fixed_intSize(password);
            int unk1 = 9;
            ns.Write(unk1);
            int roomid = room.ID;
            ns.Write(roomid);
            //byte[] unk2 = { 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 };
            //ns.Write(unk2, 0, unk2.Length);
            ns.Write(roompos);
            ns.Write(room.MapNum); //mapnum?
            ns.Write(10);
            ns.Write(isTeamPlay);
            ns.Write(isStepOn);
            ns.Write(itemtype);
            ns.Write(0);
            ns.Write(Utility.CurrentTimeMilliseconds());
            ns.Write(room.PosWeight); //位
            ns.Write(last); //end
        }
    }

    public sealed class GameRoom_SendPlayerInfo : NetPacket
    {
        public GameRoom_SendPlayerInfo(Account User, byte last)
        {
            //Account User = Client.CurrentAccount;
            ns.Write((byte)0xA6); //op code
            ns.Write(User.Session); //User session
            ns.Write(User.RoomPos);
            ns.Write(User.UDPInfo, 0, 0x30);
            /*ns.Write((short)0x2); //port length?
            byte[] clientport = BitConverter.GetBytes(User.UDPPort).Reverse().ToArray();
            byte[] clientip = BitConverter.GetBytes(Utility.IPToInt(User.LastIp)).Reverse().ToArray();
            ns.Write(clientport, 0, 2);
            ns.Write(clientip, 0, 4);
            ns.Write(0L);
            ns.Write((short)0x2); //port length?
            ns.Write(clientport, 0, 2); //內網ip
            ns.Write(clientip, 0, 4);
            ns.Write(0L);
            ns.Write((short)0x2);
            byte[] relayserverport = BitConverter.GetBytes(Conf.RelayPort).Reverse().ToArray();
            ns.Write(relayserverport, 0, 2);
            ns.Write(BitConverter.GetBytes(Utility.IPToInt(Conf.ServerIP)).Reverse().ToArray(), 0, 4);
            int unk5 = 0x78485E2C;
            ns.Write(unk5);
            ns.Write(0);*/
            ns.WriteBIG5Fixed_intSize(User.NickName);
            ns.Fill(0x10);

            ns.Write((short)0x64); //avatarInfoHeader
            if (!User.isFashionModeOn)
            {
                for (int i = 0; i < 15; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88); //0x88
                for (int i = 15; i < 30; i++)
                {
                    ns.Write(User.CurrentAvatarInfo[i]);
                }
                ns.Fill(0x88); //0x88
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
                ns.Write((short)0); //costumeMode
            }
            ns.Write((short)0);//00 00

            ns.Write(User.Exp);
            ns.Write((short)0);
            ns.Write(User.RoomPos);
            ns.Fill(0x14);
            //login packet
            ns.Write(-1);
            ns.Write(0L);
            ns.Write(0x00068BB6671178CB);
            ns.Write(0x00068BB6671178CB);
            ns.Write(0x00068BB6671178CB);
            ns.Write(-1L);
            ns.Write(0xFFFF);
            ns.Write(-1);
            ns.Write((byte)0);
            ns.Fill(0x10);

            ns.Write(1);
            ns.Write((short)User.AvatarItems.Count(w => w.Value.expireTime == 0 || w.Value.expireTime > Utility.CurrentTimeMilliseconds())); //count
            foreach (var item in User.AvatarItems.Values.Where(w => w.expireTime == 0 || w.expireTime > Utility.CurrentTimeMilliseconds()))
            {
                ns.Write(0x8C7C98); //98 7C 8C 00
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
            }
            ns.Write((short)0); //  daily buff count?
            //ns.Write((short)0);//  daily buff count?
            ns.Write(User.UserItemAttr.Count);
            foreach (var item in User.UserItemAttr)
            {
                ns.Write(item.Key); //itemnum
                ns.Write(item.Value.Count);
                foreach (var attr in item.Value)
                {
                    ns.Write(attr.Attr);
                    ns.Write(attr.AttrValue);
                }
            }
            ns.Fill(0x4C); //守護靈 0x51
            //ns.Write(0); //event team id
            bool UseShu = User.CurrentShuID != -1;
            ns.Write(UseShu);
            if (UseShu)
            {
                ns.WriteBIG5Fixed_intSize(User.UserShuInfo.ShuName);
                ns.Write(User.UserShuInfo.ShuItemNum);
                ns.Write(User.UserShuInfo.Statusinfo[3]); //exp
                ns.Write(1);
                ns.Write((short)0xC);
                foreach (var i in User.UserShuInfo.ShuAvatarKind)
                {
                    ns.Write(i);
                }
                ns.Write((short)0x10);
                foreach (var i in User.UserShuInfo.Statusinfo)
                    ns.Write(i);
                ns.Write(User.UserShuInfo.MotionList);
            }
            ns.Write(User.TopRank);
            //時裝
            /*ns.Write(1); //count
            ns.Write(17); //17:光光*/
            if (!User.isFashionModeOn)
            {
                ns.Write(0);
                ns.Write(User.WearFashionItem.Count);
                foreach (var i in User.WearFashionItem)
                {
                    ns.Write(i);
                }
            }
            else
            {
                ns.Write(User.WearAvatarItem.Count);
                foreach (var i in User.WearAvatarItem)
                {
                    ns.Write(i);
                }
                ns.Write(User.WearCosAvatarItem.Count);
                foreach (var i in User.WearCosAvatarItem)
                {
                    ns.Write(i);
                }
            }
            ns.Write(User.isFashionModeOn ? (byte)User.costumeMode : (byte)0);
            ns.Write(User.isFashionModeOn);
            int unk8 = 0x3F13A127;//27 A1 13 3F
            ns.Write(unk8);
            ns.Write((byte)1);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_ControlRoomPos : NetPacket
    {
        public GameRoom_ControlRoomPos(byte roompos, bool isOff, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0X301);
            ns.Write(roompos);
            ns.Write(isOff);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_KickPlayer : NetPacket
    {
        public GameRoom_KickPlayer(byte roompos, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0X302);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GetRoomList : NetPacket
    {
        public GameRoom_GetRoomList(Account User, List<NormalRoom> rooms, byte roomkindid, int page, byte last)
        {
            List<NormalRoom> pagedRooms = rooms.Skip(page * 16).Take(16).ToList();
            int maxpage = Convert.ToInt32(Math.Ceiling(rooms.Count / Convert.ToDouble(16))) - 1;
            ns.Write((byte)0x98);
            ns.Write(roomkindid);
            ns.Write(maxpage);
            ns.Write(page);
            ns.Write((short)pagedRooms.Count);
            foreach (NormalRoom room in pagedRooms)
            {
                /*byte MaxPlayer;
                if (room.is8Player)
                    MaxPlayer = room.SlotCount;
                else
                    MaxPlayer = room.MaxPlayersCount;*/
                bool hasPiero = room.Players.Exists(player => player.Attribute == 1);
                bool hasAfreecaTV = room.Players.Exists(player => player.Attribute == 3);
                ns.Write(room.ID);
                ns.WriteBIG5Fixed_intSize(room.Name);
                ns.Write(!room.HasPassword);
                ns.Write((byte)room.PlayerCount());
                ns.Write(room.SlotCount);
                ns.Write(!room.isPlaying);
                ns.Write(room.IsStepOn);
                ns.Write(room.ItemType);
                ns.Write(room.MapNum);
                ns.Write(6);//default 17
                ns.Write((byte)(room.IsTeamPlay == 2 ? 1 : 0));
                ns.Write(hasAfreecaTV);//非洲電視
                ns.Write(hasPiero);//小丑房間
                ns.Write((byte)0);//助理圍巾
                ns.Write((byte)0);//男數量
                ns.Write((byte)0);//女數量
                ns.Write((byte)1);
                ns.Write(-1);
                ns.Write((byte)0);//buff
                ns.Write(room.ItemNum);//房間獎勵
                ns.Write(0);//BONUS STAGE Level
                ns.Write((byte)0);
            }
            ns.Seek(ns.Position - 1, SeekOrigin.Begin);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RemoveRoomUser : NetPacket
    {
        public GameRoom_RemoveRoomUser(byte roompos, byte last)
        {
            ns.Write((byte)0xA8);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_LeaveRoomUser_0XA9 : NetPacket
    {
        public GameRoom_LeaveRoomUser_0XA9(Account User, byte roompos, byte last)
        {
            ns.Write((byte)0xA9);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RoomPosReady : NetPacket
    {
        public GameRoom_RoomPosReady(byte roompos, bool isReady, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2E7);
            ns.Write(roompos);
            ns.Write(isReady);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GetRoomMaster : NetPacket
    {
        public GameRoom_GetRoomMaster(byte roompos, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2DD); //opcode
            ns.Write((byte)roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeMap_FF6605 : NetPacket
    {
        public GameRoom_ChangeMap_FF6605(Account User, int mapid, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x566);
            ns.Write(mapid);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeMap_FFEA02 : NetPacket
    {
        public GameRoom_ChangeMap_FFEA02(Account User, int mapid, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2EA);
            ns.Write(mapid);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeSetting : NetPacket
    {
        public GameRoom_ChangeSetting(NormalRoom room, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2D9);
            ns.WriteBIG5Fixed_intSize(room.Name);
            ns.WriteBIG5Fixed_intSize(room.Password);
            ns.Write(room.IsStepOn);
            ns.Write(room.ItemType);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_EndLoading : NetPacket
    {
        public GameRoom_EndLoading(byte roompos, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x311);
            ns.Write(roompos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StartLoading: NetPacket
    {
        public GameRoom_StartLoading(int mapid, int randseed, byte last)
        {
            //FF 0B 03 89 13 00 00 92 D1 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x30B);
            ns.Write(mapid);
            ns.Write(randseed);
            ns.Write((byte)0);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_PlayerPosList : NetPacket
    {
        public GameRoom_PlayerPosList(List<Account> player, byte last)
        {
            //FF C5 02 02 00 00 00 02 00 00 00 00 03 00 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x2C5);
            ns.Write(player.Count);
            foreach (var p in player)
            {
                ns.Write(p.RoomPos);
                ns.Write(0);
            }
            ns.Write(last);
        }
    }
    public sealed class GameRoom_AllSync : NetPacket
    {
        public GameRoom_AllSync(byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x312);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_START_GAME_RES : NetPacket
    {
        public GameRoom_START_GAME_RES(Account User, int iGameStartTick, int iNumberOfItem, byte last)
        {
            //FF 14 03 41 00 00 00 C8 00 00 00 99 AC 09 00
            ns.Write((byte)0xFF);
            ns.Write((short)0x314);
            ns.Write(iGameStartTick);
            ns.Write(iNumberOfItem);
            ns.Write(0x816A4); //A4 16 08 00  //99 AC 09 00
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GoalInData : NetPacket
    {
        public GameRoom_GoalInData(byte pos, int LapTime, int flag, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x353);
            ns.Write(pos);
            ns.Write(LapTime);
            ns.Write(flag);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GameEndData : NetPacket
    {
        public GameRoom_GameEndData(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x3E3);
            ns.Write(User.RoomPos);
            ns.Write(2);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_StartTimeOutCount : NetPacket
    {
        public GameRoom_StartTimeOutCount(int LapTime, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x37C);
            ns.Write(LapTime); //dwGametime=LapTime+2000
            ns.Write((byte)10); //nTimeOutSeconds=10
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RoomChat : NetPacket
    {
        public GameRoom_RoomChat(byte pos, byte[] text, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2DE);
            ns.Write(pos);
           /* short len = (short)text.Length;
            ns.Write(len);*/
            ns.Write(text, 0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_ChangeStatus : NetPacket
    {
        public GameRoom_ChangeStatus(byte pos, int code, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2C9);
            ns.Write(pos);
            ns.Write(code);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GameResult2 : NetPacket
    {
        public GameRoom_GameResult2(Account User, byte[] result)
        {
            /*FF 81 03 04 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00
             00 00 00 21 CA 00 00 36 CA 00 00 01 00 55 00 00 00 00 00
             00 00 23 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 3F 00 17 00 0C 00 00 00 02 00 00 00 01 00 00 00
             00 00 00 00 00 F7 98 1C 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 00 00 00 E4 D5 1D 00 00 00 00 00 00
             5C D5 00 00 48 D5 00 00 02 01 10 00 00 00 00 00 00 00 09
             00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 00 02 00 00 00 01 00 00 00 70 02 00
             00 00 F7 98 1C 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 00 E6 D5 1D 00 00 00 00 00 00 00 00 02
             00 00 00 EA 03 00 00 11 00 00 00 FF FF FF FF FF FF FF FF
             FF FF FF FF 00 01 00 00 00 F6 03 00 00 04 00 00 00 00 00
             00 00 10*/
            ns.Write(result, 0, result.Length);
        }
    }

    public sealed class GameRoom_GameOver : NetPacket
    {
        public GameRoom_GameOver(byte pos, byte last)
        {
            //FF 3E 03 1A 02 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x33E); //opcode
            ns.Write(pos);
            ns.Write(2); //02 00 00 00
            ns.Write(last);
        }
    }
    public sealed class GameRoom_Alive : NetPacket
    {
        public GameRoom_Alive(byte pos, byte last)
        {
            //FF 3F 03 01 00 00 00 16 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x33F); //opcode
            ns.Write(1); //01 00 00 00
            ns.Write(pos);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GameUpdateEXP : NetPacket
    {
        public GameRoom_GameUpdateEXP(Account User, byte last)
        {
            ns.Write((byte)0xAA);
            ns.Write(0x2710);//ladder point?
            ns.Write(User.Exp);
            ns.Write(0x259); //601
            ns.Write(0L);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_UpdateIndividualGameRecord : NetPacket
    {
        public GameRoom_UpdateIndividualGameRecord(Account User, byte last)
        {
            int clear = 0, first = 0, second = 0, third = 0;
            int playCount = 0, distance = 0, clearCount = 0, firstCount = 0, secondCount = 0, thirdCount = 0;
            if (User.Rank < 4) {
                first = User.Rank == 1 ? 1 : 0;
                second = User.Rank == 2 ? 1 : 0;
                third = User.Rank == 3 ? 1 : 0;
            }
            clear = User.GameEndType == 1 ? 1 : 0;

            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_IndividualRecordUpdateGame";
                    cmd.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
                    cmd.Parameters.Add("clear", MySqlDbType.Int32).Value = clear;
                    cmd.Parameters.Add("distance", MySqlDbType.Int32).Value = User.RaceDistance;
                    cmd.Parameters.Add("first", MySqlDbType.Int32).Value = first;
                    cmd.Parameters.Add("second", MySqlDbType.Int32).Value = second;
                    cmd.Parameters.Add("third", MySqlDbType.Int32).Value = third;
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        playCount = Convert.ToInt32(reader["playCount"]);
                        distance = Convert.ToInt32(reader["distance"]);
                        clearCount = Convert.ToInt32(reader["clearCount"]);
                        firstCount = Convert.ToInt32(reader["firstCount"]);
                        secondCount = Convert.ToInt32(reader["secondCount"]);
                        thirdCount = Convert.ToInt32(reader["thirdCount"]);
                    }
                }
            }
            ns.Write((byte)0xFF);
            ns.Write((short)0x22D);
            ns.Write(playCount);
            ns.Write(distance);
            ns.Write(clearCount);
            ns.Write(firstCount);
            ns.Write(secondCount);
            ns.Write(thirdCount);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_MapControl : NetPacket
    {
        public GameRoom_MapControl(short len, byte[] unk, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2CB);
            ns.Write(len);
            ns.Write(unk, 0, unk.Length);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_TriggerMapEvent : NetPacket
    {
        public GameRoom_TriggerMapEvent(byte eventnum, int eventlaptime, byte last)
        {
            //FF 5D 03 01 D0 78 02 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x35D);
            ns.Write(eventnum);
            ns.Write(eventlaptime);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_GiveUpItem : NetPacket
    {
        public GameRoom_GiveUpItem(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x321);
            ns.Write(User.RoomPos);
            ns.Write((byte)0x1);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_DrawItem : NetPacket
    {
        public GameRoom_DrawItem(byte pos, int time, int CapsuleID, int itemid, int CapsuleNum, byte flag, byte last)
        {
            //FF 1B 03 00 00 00 00 00 51 34 00 00 0B 00 00 00 CF 00 00 00 00 02
            //FF 1B 03 00 C9 00 00 00 85 1F 00 00 38 00 00 00 00 00 00 00 00 02
            //FF 1B 03 00 CE 00 00 00 13 A5 00 00 1F 00 00 00 00 00 00 00 01 10
            ns.Write((byte)0xFF);
            ns.Write((short)0x31B);
            ns.Write(pos);
            ns.Write(CapsuleID);
            ns.Write(time);
            ns.Write(itemid);//item id
            ns.Write(CapsuleNum); //next id??
            ns.Write(flag);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_UseItem : NetPacket
    {
        public GameRoom_UseItem(byte pos, int time, int itemid, byte[] bytes, byte last)
        {
            /*FF 1E 03 00 A0 F3 00 00 0B 00 00 00 45 00 D0 00
             00 00 0B 00 00 00 A0 F3 00 00 00 00 C6 5E F3 C3 
             B6 2C 38 C5 9C 59 2A 44 00 00 00 00 00 00 00 00
             00 00 00 00 F3 04 B5 B9 00 00 00 00 00 00 00 00
             00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00
             00 00 00 40*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x31E);
            ns.Write(pos);
            ns.Write(time);
            ns.Write(itemid);
            ns.Write(bytes, 0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RegItem : NetPacket
    {
        public GameRoom_RegItem(int time, int itemid, int CapsuleNum, byte[] bytes, byte last)
        {
            /*FF 28 03 CA 43 00 00 39 00 00 00 CB 00 00 00 25 00 09 00 00 00 
             78 C8 43 00 00 F3 03 00 00 00 00 00 00 5E 82 F7 45 37 F8 B8 43
             98 D9 18 44 00 00 00 00 2A E4 CE 3F 20 */
            ns.Write((byte)0xFF);
            ns.Write((short)0x328);
            ns.Write(time);
            ns.Write(itemid);
            ns.Write(CapsuleNum);
            ns.Write(bytes, 0);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RoomPosTeam : NetPacket
    {
        public GameRoom_RoomPosTeam(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2ED);
            ns.Write(User.RoomPos);
            ns.Write(User.Team);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StartError : NetPacket
    {
        public GameRoom_StartError(int error, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x308);
            ns.Write(error);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RegisterSuccess : NetPacket
    {
        public GameRoom_RegisterSuccess(int itemnum, long storage_id, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x557);
            ns.Write(0);
            ns.Write(itemnum);
            ns.Write(storage_id);
            ns.Write(0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_GoodsInfo : NetPacket
    {
        public GameRoom_GoodsInfo(NormalRoom room, byte last)
        {
            //FF5805FFFFFFFF00000000000000000000000000
            ns.Write((byte)0xFF);
            ns.Write((short)0x558);
            ns.Write(room.ItemNum);
            ns.Write(0);
            ns.Write(room.isOrderBy);
            ns.Write(room.SendRank);
            ns.Write(room.isPublic);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StepOnButton : NetPacket
    {
        public GameRoom_StepOnButton(byte[] unk, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2FF);
            ns.Write(unk, 0, 0xC);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_LockKeepItem : NetPacket
    {
        public GameRoom_LockKeepItem(NormalRoom room, bool isCancel, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x555);
            ns.Write(1);
            ns.Write(2L);
            ns.Write(room.Storage_Id);
            ns.Write(room.ItemNum);
            ns.Write(Utility.ConvertToTimestamp(DateTime.Now));//TODO: 讀取保管日期
            ns.Write(1);
            ns.Write(1);
            ns.Write(isCancel ? 0 : (float)1);
            ns.Write(0L); 
            /*ns.Write((short)0);
            ns.Write(isCancel ? 0 : 0x3F80);//80 3F 00 00 00 00 00 00
            ns.Write(0);
            ns.Write((short)0);*/
            ns.Write(last);
        }
    }
    public sealed class GameRoom_DeleteKeepItem : NetPacket
    {
        public GameRoom_DeleteKeepItem(Account User, NormalRoom room, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x555);
            ns.Write(1);
            ns.Write(1L);
            ns.Write(room.Storage_Id);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_SendRoomMaster : NetPacket
    {
        public GameRoom_SendRoomMaster(Account User, int mapnum, byte roommasterindex, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((byte)0xB9);
            ns.Write((byte)0x1);
            ns.Write(mapnum);
            ns.Write(roommasterindex);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_CreateRoomError : NetPacket
    {
        public GameRoom_CreateRoomError(Account User, int errorid, byte last)
        {
            ns.Write((byte)0x9E);
            ns.Write(errorid);//5: Level limited, 8: No room server, 9: map incorrect
            ns.Write(last);
        }
    }
    public sealed class GameRoom_EnterRoomError : NetPacket
    {
        public GameRoom_EnterRoomError(Account User, byte errorid, byte roomkindid, byte last)
        {
            ns.Write((byte)0xA5);
            ns.Write(0x41);
            ns.Write(errorid);
            ns.Write(roomkindid);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_jw_Loading : NetPacket
    {
        public GameRoom_jw_Loading(byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x485);
            ns.Write(last);
        }
    }

    public sealed class MoveToGameRoom : NetPacket
    {
        public MoveToGameRoom(byte last)
        {
            //FF9503
            ns.Write((byte)0xFF);
            ns.Write((short)0x395);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RoomPosRelayTeam : NetPacket
    {
        public GameRoom_RoomPosRelayTeam(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2F0);
            ns.Write(User.RoomPos);
            ns.Write(User.RelayTeamPos);
            ns.Write((byte)0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_RelayChangeSlotState : NetPacket
    {
        public GameRoom_RelayChangeSlotState(byte slotid, bool isOFF, byte last)
        {
            //FF F4 02 02 01 00 20
            ns.Write((byte)0xFF);
            ns.Write((short)0x2F4);
            ns.Write(slotid);
            ns.Write(isOFF);
            ns.Write((byte)0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_PassBaton : NetPacket
    {
        public GameRoom_PassBaton(Account User, byte teampos, int unk, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2FD);
            ns.Write(User.RoomPos);
            ns.Write(teampos);
            ns.Write(unk);
            ns.Write(last);
        }
    }
    /*public sealed class GameRoom_PreparePassBaton : NetPacket
    {
        public GameRoom_PreparePassBaton(byte pos, short len, byte[] bytes, byte last)
        {
            //FF CF 02 03 08 00 03 00 00 00 02 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x2CF);
            ns.Write(pos);
            ns.Write(len);
            ns.Write(bytes, 0, len);
            ns.Write(last);
        }
    }*/
    public sealed class GameRoom_WaitPassBaton : NetPacket
    {
        public GameRoom_WaitPassBaton(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2F7);
            ns.Write(User.RoomPos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_WaitPassBaton2 : NetPacket
    {
        public GameRoom_WaitPassBaton2(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2F9);
            ns.Write(User.RoomPos);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_StartPassBaton : NetPacket
    {
        public GameRoom_StartPassBaton(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x2FB);
            ns.Write(User.RoomPos);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_LevelUP : NetPacket
    {
        public GameRoom_LevelUP(short type, long exp, byte pos, byte last)
        {
            //FF 86 03 01 00 0B 01 00 00 00 00 00 00 01 20
            //FF 86 03 01 00 6B 01 00 00 00 00 00 00 07 02
            ns.Write((byte)0xFF);
            ns.Write((short)0x386);
            ns.Write(type);//levelkind?
            ns.Write(exp);
            ns.Write(pos);
            ns.Write(last);
        }
    }

    public sealed class GetUserEXPInfo2 : NetPacket
    {
        public GetUserEXPInfo2(short type, int level, long expvalue, byte last)
        {
            //FF 84 03 01 00 07 00 00 00 C3 01 00 00 00 00 00 00 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x384);
            ns.Write(type);
            ns.Write(level);
            ns.Write(expvalue);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_FFF801 : NetPacket
    {
        public GameRoom_FFF801(int unk1, int unk2, int unk4, int unk5, byte last)
        {
            //FF F9 01 09 00 00 00 47 0E 00 00 B7 2F 00 00 01 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x1F9);
            ns.Write(unk1);
            ns.Write(unk2);
            ns.Write(unk4);
            ns.Write(unk5);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_CannotStart : NetPacket
    {
        public GameRoom_CannotStart(byte errid, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x30C);
            ns.Write(errid);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_RewardResult : NetPacket
    {
        public GameRoom_RewardResult(List<GameRewardResult> rewardresult, byte last)
        {
            /*FF 82 03 01 00 00 00 03 00 00 00 1F 8A 00 00
             00 00 00 00 0A 97 00 00 00 00 00 00 62 D1 00
             00 00 00 00 00 01 00 00 00 EA 03 00 00 00 00
             00 00 04*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x382);
            ns.Write(1);
            ns.Write(rewardresult.Count);
            foreach (var i in rewardresult)
            {
                ns.Write(i.RewardID);
                ns.Write(0);
            }
            ns.Write(1);
            ns.Write(0x3EA);
            ns.Write(last);
        }
    }

}
