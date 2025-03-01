using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using AgentServer.Structuring;
using AgentServer.Packet;
using AgentServer.Database;
using MySql.Data.MySqlClient;
using System.Data;
//using LocalCommons.Cryptography;
using LocalCommons.Utilities;
using AgentServer.Controller;
using System.Collections.Concurrent;
using System.Threading;
using AgentServer.Packet.Send;
using MySqlX.XDevAPI;

namespace AgentServer.Network.Connections
{

    /// <summary>
    /// Connection That Used For ArcheAge Client( Game Side )
    /// </summary>
    public class ClientConnection : IConnection
    {
        //----- Static
	    private readonly byte _mRandom;
        public byte NumPck = 0; 
        public static ConcurrentDictionary<int, Account> CurrentAccounts { get; } = new ConcurrentDictionary<int, Account>();

        public Account CurrentAccount { get; set; }
        public int session { get; set; }

        public ClientConnection(Socket socket) : base(socket)
        {
            Log.Info("Client IP: {0} connected", this);
	        this.DisconnectedEvent += this.ClientConnection_DisconnectedEvent;
	        this.m_LittleEndian = true;
            //init
            session = LocalCommons.Cookie.Cookie.Generate();
            //byte[] key = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff, 0x00 };
            Account nCurrent = new Account
            {
                //EncryptKey = key,
                Connection = this,
                Session = session,
                isLogin = false
            };
            CurrentAccount = nCurrent;
            CurrentAccounts.TryAdd(session, CurrentAccount);
            //this.SendAsync(new Net_OnConnection());
            //this.SendAsync(new Net_OnConnection2());
            nCurrent = null;
            //key = null;
            //NormalRoom room = new NormalRoom();
            //room.setID(1000);
            //Rooms.AddRoom(room.ID, room);
        }

        public class Net_OnConnection : NetPacket
        {
            public Net_OnConnection() : base(0, 0x0e)
            {

                ns.WriteHex("0100");
            }
        }

        public class Net_OnConnection2 : NetPacket
        {
            public Net_OnConnection2() : base(0, 0x42)
            {

                ns.WriteHex("01000000000000008E0BE474BF739C59890B2229E00ED4729C06A20CB1391F444379567D36261955F703DA4A684C5A560000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006400000001CF2C1220000000C0160A03200000001CFF8706B896D017969D827CE85CD017C0000F1158344418A4FB8706000000000100000004000000480009030400000048000903D8140A0388010903");
            }
        }


        public override void SendAsync(NetPacket packet)
        {
            //packet.IsArcheAgePacket = true;
            try
            {
                //packet.EncryptKey = CurrentAccount.EncryptKey;
                //packet.XorKey = CurrentAccount.XorKey;
                //packet.isEncrypt = CurrentAccount.isLogin;
                base.SendAsync(packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //this.NumPck = NetPacket.NumPckSc;
           // Console.WriteLine("packet.isEncrypt {0}", packet.isEncrypt);
        }


        public void SendAsyncd(NetPacket packet)
        {
            //packet.IsArcheAgePacket = false;
            base.SendAsync(packet);
        }
        void ClientConnection_DisconnectedEvent(object sender, EventArgs e)
        {
			//remove user
			try
			{
                if(CurrentAccounts.TryRemove(session, out var account) && account.isLogin)
                {
                    GameRoomHandle.LeaveRoom(CurrentAccount);
                    //GameRoomEvent.LeaveRoom(this, true, 0x1);
                    //HandleLogout(this.CurrentAccount);
                }
            }
			catch
			{
				Log.Warning("Client IP: {0} disconnected,But the remove fail", this);
			}
			
            Log.Info("Client IP: {0} disconnected", this);
	        this.Dispose();
        }

        private void HandleLogout(Account User)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_logout";
                cmd.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
                cmd.Parameters.Add("nickName", MySqlDbType.VarString).Value = User.NickName;
                MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            //RelayServer relay = RelayController.CurrentRelayServer.FirstOrDefault().Value;
        }

        public override void HandleReceived(byte[] data)
        {
            //UnknownHandle.Handle_FF0405(this, reader, 0x00);
            PacketReader reader = new PacketReader(data, 0);
            byte header = 0;
            ushort opcode = 0;
            byte subopcode = 0;
            reader.Decrypt();

            header = reader.ReadByte();
            opcode = reader.ReadLEUInt16(); //Packet Opcode

            /*switch (opcode)
            {
                case 465:
                    ushort unk1 = reader.ReadLEUInt16();
                    subopcode = reader.ReadByte();
                    Console.WriteLine("header: {0}, opcode: {1}", header, opcode);
                    break;
                case 450:
                    ushort unk2 = reader.ReadLEUInt16();
                    subopcode = reader.ReadByte();
                    Console.WriteLine("header: {0}, opcode: {1}", header, opcode);
                    break;
                default:
                    Console.WriteLine("header: {0}, opcode: {1}, subopcode: {2}", header, opcode, subopcode);
                    break;
            }*/
           
            if(opcode == 465 || opcode == 450 || opcode == 973 || opcode == 989)
            {
                Console.WriteLine("header: {0}, opcode: {1}", header, opcode);
            }
            else
            {
                ushort unk1 = reader.ReadLEUInt16();
                subopcode = reader.ReadByte();
                Console.WriteLine("header: {0}, opcode: {1}, subopcode: {2}", header, opcode, subopcode);
            }
            
            switch (opcode)
            {
                case 453:
                    // 定期的に送られてくる
                    try
                    {
                        Account User = this.CurrentAccount;

                        // ログアウトパケット 無限ロードとかに対して有効。
                        LoginHandle.Handle_Logout(this, reader);

                        // LoginHandle.Handle_LoginUnknown1(this, reader);
                        // LoginHandle.Handle_LoginUnknown2(this, reader);
                        //LoginHandle.Handle_LoginUnknown3(this, reader);
                        //LoginHandle.Handle_LoginUnknown4(this, reader);
                        // LoginHandle.Handle_LoginUnknown5(this, reader); // 音が鳴った
                        // LoginHandle.Handle_LoadAllItem(this, reader);
                        // LoginHandle.Handle_LoginReadDataCompleted(this, reader);

                        // HEX送信テスト
                        // this.SendAsync(new NP_Hex(User, "A5A5A5A5A5A5A5A5")); // 送信テスト
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    break;
                case 450: //0x1C2
                    GameRoomHandle.Handle_ChatToAll(this, reader);
                    break;
                case 465: //0x1D1
                    GameRoomHandle.Handle_SendPlayerPosition(this, reader);
                    break;
                case 705: //0x2C1
                    if (subopcode == 0x00)
                    {
                        GameRoomHandle.Handle_DualRequest(this, reader);
                    }
                    else if (subopcode == 0x01)
                    {
                        GameRoomHandle.Handle_DualBattleCancel(this, reader);
                    }
                    else if (subopcode == 0x03)
                    {
                        GameRoomHandle.Handle_DualBattledCancel(this, reader);
                    }
                    else if (subopcode == 0x05) {
                        GameRoomHandle.Handle_DualConfirm(this, reader);
                    }
                    break;
                case 1633:
                    if (subopcode == 72)
                    {
                        LoginHandle.Handle_LoginCheck(this, reader);
                    }
                    break;
                case 865: //0x361
                    if (subopcode == 0x00)
                    {
                        LoginHandle.Handle_LoginCheck(this, reader);
                    }
                    else if (subopcode == 0x01)
                    {
                        LoginHandle.Handle_FirstLoginCheck(this, reader);
                    }
                    else if (subopcode == 0x02)
                    {
                        LoginHandle.Handle_LoginDisplayCharacter(this, reader);
                    }
                    else if (subopcode == 0x03)
                    {
                        LoginHandle.Handle_LoginNewCharacter(this, reader);
                    }
                    else if (subopcode == 0x04)
                    {
                        LoginHandle.Handle_LoginDeleteCharacter(this, reader);
                    }
                    break;
                case 825: //0x339
                    if (subopcode == 0x02)
                    {

                        LoginHandle.Handle_LoginCheckGlobalID(this, reader);
                        LoginHandle.Handle_LoginUnknown1(this, reader);
                        //LoginHandle.Handle_LoginUnknown2(this, reader); // LoginMapInfo_0x1EC
                        //LoginHandle.Handle_LoginUnknown3(this, reader); // LoginMapMusicInfo_0x31B
                        //LoginHandle.Handle_LoginUnknown4(this, reader); // 
                        LoginHandle.Handle_LoginUnknown5(this, reader); // LoginCharParam_0x3A0 -> LoginCharAppear_0x3E8_00 // 音が鳴った

                        // unknown5だけだと header: 0, opcode: 983, subopcode: 21だけ ロードは進む
                        // Unknown5までを一気に送るとheader: 0, opcode: 1032, subopcode: 0もくる ロードは進まない

                        // Unknown 2,3,4,5の順に送ると　header: 0, opcode: 983, subopcode: 21 と header: 0, opcode: 1032, subopcode: 0 ロードは進む

                        // Handle_LoginCheckGlobalID+Unknown5ではheader: 0, opcode: 983, subopcode: 21 だけ

                        // LoginHandle.Handle_LoadAllItem(this, reader);
                        // LoginHandle.Handle_LoginReadDataCompleted(this, reader); 
                    }
                    break;
                case 922: //0x39A
                    if (subopcode == 0x78)
                    {
                        LoginHandle.Handle_Logout(this, reader);
                    }
                    break;
                case 973: //0x3CD
                    GameRoomHandle.Handle_DualMove(this, reader);
                    break;
                case 983:
                    if(subopcode == 0x15)
                    {
                        LoginHandle.Handle_LoginUnknown2(this, reader); // LoginMapInfo_0x1EC
                        // LoginHandle.Handle_LoginUnknown3(this, reader); // LoginMapMusicInfo_0x31B
                        // LoginHandle.Handle_LoginUnknown4(this, reader); // 
                    }
                    break;
                case 989: //0x3DD
                    GameRoomHandle.Handle_ActToAll(this, reader);
                    break;
                case 1032:
                    if(subopcode == 0x00)
                    {
                        LoginHandle.Handle_LoginUnknown3(this, reader); // LoginMapMusicInfo_0x31B
                        LoginHandle.Handle_LoginUnknown4(this, reader);
                        LoginHandle.Handle_LoadAllItem(this, reader);
                        LoginHandle.Handle_LoginReadDataCompleted(this, reader);
                    }
                    break;
                case 1252: //0x4E4
                    LoginHandle.Handle_MoveItem(this, reader);
                    break;
                case 1309: //0x51D
                    if (subopcode == 0x00)
                    {
                        GameRoomHandle.Handle_TradeStart(this, reader);
                    }
                    else if(subopcode == 0x01)
                    {
                        GameRoomHandle.Handle_TradeLoadItem(this, reader);
                    }
                    else if(subopcode == 0x02)
                    {
                        GameRoomHandle.Handle_TradeConfirm(this, reader);
                    }
                    else if (subopcode == 0x04)
                    {
                        GameRoomHandle.Handle_TradeCancel(this, reader);
                    }
                    break;
                case 1329: //0x531
                    GameRoomHandle.Handle_WearItem(this, reader);
                    break;
            }

            /*
            PacketReader reader = new PacketReader(data, 0);
            byte header = 0;
            ushort opcode = 0;
            byte subopcode = 0;

            if (this.CurrentAccount != null && this.CurrentAccount.isLogin)
            {
                reader.Decrypt(this.CurrentAccount.EncryptKey, CurrentAccount.XorKey);
                header =  reader.ReadByte(); //Packet Opcode/Level
                if (header == 0xFF)
                {
                    opcode = reader.ReadLEUInt16(); //Packet Opcode
                    if (reader.Size > 2)
                    {
                        subopcode = reader.ReadByte();
                        reader.Offset -= 1;
                    }
                }
                else
                {
                    opcode = header; //Packet Opcode
                    if (opcode == 0x74 && reader.Size > 2)
                    {
                        subopcode = reader.ReadByte();
                        //reader.Offset -= 1;
                    }
                }
                //Console.WriteLine("header 0x{0:X2}", header);
            }
            else
            {
                opcode = reader.ReadByte(); //Packet Opcode/Level
            }

            byte last = reader.Buffer.Reverse().Take(1).ToArray()[0];
            //Log.LogFile = "log";
            switch (opcode)
            {
                case 15: //0x0F
                    //Console.WriteLine("LoginHandle_Request Login");
                    LoginHandle.Handle_LoginCheck(this, reader);
                    break;
                case 17: //0x11
                    //Console.WriteLine("ECC Public Key");
                    LoginHandle.Handle_GetClientKey(this, reader);
                    break;
                case 1: //0x01
                    //Console.WriteLine("isLogin true");
                    CurrentAccount.isLogin = true;
                    LoginHandle.Handle_LoginSuccess(this, reader, last);
                    break;
                case 8: //0x08
                    LoginHandle.Handle_NOTIFY_MY_UDP(this, reader, last);
                    break;
                case 0x0A:
                    //UDPOK
                    break;
                case 0x19: 
                    //Console.WriteLine("Bad User");
                    GMCommandHandle.Handle_ClientCheckAutoBan(this, reader, last);
                    break;
                case 27: //0x1B  nickname
                    //Console.WriteLine("GetNickName");
                    LoginHandle.Handle_GetNickName(this, reader, last);
                    break;
                case 0x1D:
                    LobbyHandle.Handle_GetUserEXPInfo(this, reader, last);
                    break;
                case 0x1F: 
                    LobbyHandle.Handle_GetUserItemCollectionPointInfo(this, reader, last);
                    break;
                case 0x21:
                    LobbyHandle.Handle_GetUserItemCollectionItemInfo(this, reader, last);
                    break;
                case 0x23:
                    LobbyHandle.Handle_SetItemCollectionShowItem(this, reader, last);
                    break;
                case 59: //0x3B
                    //Console.WriteLine("3B80");
                    UnknownHandle.Handle_3B80(this, last);
                    break;
                case 0x3D:
                    //Console.WriteLine("ItemOnOff");
                    MyRoomHandle.Handle_ItemOnOff(this, reader, last);
                    break;
                case 75: //0x4B
                    //Console.WriteLine("ENTER_NEW_NICKNAME");
                    FirstLoginHandle.Handle_SetNewNickName(this, reader, last);
                    break;
                case 91: //0x5B
                    //Console.WriteLine("itemMsgPop");
                    MyRoomHandle.Handle_ItemMsgPop(this, reader, last);
                    break;
                case 100: //0x64
                    //Console.WriteLine("MyRoomHandle_Handle_MyroomGetAllItem_6400");
                    MyRoomHandle.Handle_MyroomGetAllItem(this, reader, last);
                    break;
                case 101: //0x65
                    //Console.WriteLine("65a2");
                    UnknownHandle.Handle_65A2(this, last);
                    break;
                case 0x66: //102
                    MyRoomHandle.Handle_ActiveFuncItemOne(this, reader, last);
                    break;
                case 108: //0x6C
                    //Console.WriteLine("LoginHandle_GetCurrentAvatarInfo_6C0104");
                    LoginHandle.Handle_GetCurrentAvatarInfo(this, reader, last);
                    break;
                case 110: //0x6E
                    //Console.WriteLine("LoginHandle_6E10");
                    LoginHandle.Handle_6E(this, last);
                    break;
                case 112: //0x70
                    //Console.WriteLine("Handle_SelectStartCharacter");
                    FirstLoginHandle.Handle_SelectStartCharacter(this, reader, last);
                    break;
                case 114: //0x72
                    //Console.WriteLine("HandleEndGamePage");
                    LobbyHandle.Handle_ShowPage(this, reader, last);
                    break;
                case 116: //0x74
                    if (subopcode == 0x00)
                    {
                        //Log.Info("GetCommunityAgentServer");
                        LoginHandle.Handle_GetCommunityAgentServer(this, last);
                    }
                    else if (subopcode == 0x02)
                    {
                        //Log.Info("AddFriend");
                        CommunityHandle.Handle_AddFriend(this, reader, last);
                    }
                    else if(subopcode == 0x05)
                    {
                        //Log.Info("GetFriendListAccepted");
                        CommunityHandle.Handle_GetFriendListAccepted(this, last);
                    }
                    else if (subopcode == 0x08)
                    {
                        //Log.Info("AcceptFriend");
                        CommunityHandle.Handle_AcceptFriend(this, reader, last);
                    }
                    else if (subopcode == 0x0B)
                    {
                        Log.Info("DeclineFriend");
                        CommunityHandle.Handle_DeclineFriend(this, reader, last);
                    }
                    else if (subopcode == 0x0E)
                    {
                        //Log.Info("blockFriend");
                        CommunityHandle.Handle_BlockFriend(this, reader, last);
                    }
                    else if (subopcode == 0x11)
                    {
                        //Log.Info("UnblockFriend");
                        CommunityHandle.Handle_UnBlockFriend(this, reader, last);
                    }
                    else if (subopcode == 0x14)
                    {
                        //Log.Info("DeleteFriend");
                        CommunityHandle.Handle_DeleteFriend(this, reader, last);
                    }
                    else if (subopcode == 0x1A)
                    {
                        Log.Info("cancelAddFriend");
                        CommunityHandle.Handle_CancelAddFriend(this, reader, last);    
                    }
                    else if (subopcode == 0x1D)
                    {
                        //Log.Info("GetRequestedToMe");
                        CommunityHandle.Handle_GetRequestedToMe(this, last);
                    }
                    else if (subopcode == 0x1F)
                    {
                        //Log.Info("FriendGroupOP");
                        CommunityHandle.Handle_GetFriendGroup(this, reader, last);
                    }
                    else if (subopcode == 0x22)
                    {
                        //Log.Info("GroupMoveMember");
                        CommunityHandle.Handle_GroupMoveMember(this, reader, last);
                    }
                    else if (subopcode == 0x26)
                    {
                        Log.Info("CommunityAgentServer_74 26 10");
                        CommunityHandle.Handle_0x7426(this, last);
                    }
                    else if (subopcode == 0x28)
                    {
                        Log.Info("CommunityAgentServer_74 28 00 20");
                        CommunityHandle.Handle_0x7428(this, last);
                    }
                    else
                    {
                        Log.Info("Unhandle sopcode: 0x74 0x{0:X2}  |  {1}", subopcode, Utility.ByteArrayToString(reader.Buffer));
                        //LogHelp.Logging("Unhandle sopcode: 0x74 " + ByteArrayToString(hPacket, hPacket.Length));
                    }
                    break;
                case 64: //0x40
                case 117: //0x75
                    //Console.WriteLine("ping? 0x{0:X2} 0x{1:X2}", opcode, subopcode);
                    LobbyHandle.HandlePingTime(this, last);
                    break;
                case 0x7C:
                    CommandHandle.Handle_UseShoutItem(this, reader, last);
                    break;
                case 0x8A: //排名
                    RankHandle.Handle_GetRankInfo(this, reader, last);
                    break;
                case 0x8B: //排名
                    RankHandle.Handle_SearchRank(this, reader, last);
                    break;
                case 0x8E: //排名
                    RankHandle.Handle_GetMyRankInfo(this, reader, last);
                    break;
                case 0x90:
                    //Talesbook
                    LobbyHandle.Handle_GetUserInfo(this, reader, last);
                    break;
                case 0x92:
                    LobbyHandle.Handle_SetGameOption(this, reader, last);
                    break;
                case 263: //0x107
                    //usp_CM_getUserAlarmInfo
                    CommunityHandle.Handle_GetUserAlarmInfo(this, last);
                    break;
                case 308: //0x134 eServer_GET_ANIMAL_AVATAR_ACK?
                    //Console.WriteLine("FF3401");
                    UnknownHandle.Handle_FF3401(this, last);
                    break;
                case 321: //0x141
                    //Console.WriteLine("MyRoomHandle_GetCharacterAvatarItem_FF4101");
                    MyRoomHandle.Handle_MyroomGetCharacterAvatarItem(this, reader, last);
                    break;
                case 333: //0x14D
                    //Console.WriteLine("LoginHandle_FF4D01");
                    LoginHandle.Handle_FF4D01(this, last);
                    break;
                case 357: //0x165
                   //Console.WriteLine("FF6501");
                    UnknownHandle.Handle_FF6501(this, last);
                    break;
                case 376: //0x178
                    LoginHandle.Handle_GetUserCash(this, last);
                    break;
                case 381: //0x17D
                    Console.WriteLine("FF7D01"); //shop category?
                    UnknownHandle.Handle_FF7D01(this, last);
                    break;
                case 385: //0x181
                    Console.WriteLine("FF8101");
                    UnknownHandle.Handle_FF8101(this, last);
                    break;
                case 396: //0x18C
                    Console.WriteLine("FF8C01");
                    UnknownHandle.Handle_FF8C01(this, last);
                    break;
                case 403: //0x193
                    Console.WriteLine("FF9301");
                    UnknownHandle.Handle_FF9301(this, last);
                    break;
                case 409: //0x199
                    Console.WriteLine("FF9901");
                    UnknownHandle.Handle_FF9901(this, last);
                    break;
                case 449: //0x1C1
                    Console.WriteLine("FFC10140");
                    UnknownHandle.Handle_FFC10140(this, last);
                    break;
                case 0x1CF: //0x1CF MyRoomHandle
                    if (subopcode == 0x00)
                        MyRoomHandle.Handle_FFCF0100(this, last);
                    else if (subopcode == 0x02)
                        MyRoomHandle.Handle_MyRoomGetCharacterList(this, last);
                    else if (subopcode == 0x05)
                        MyRoomHandle.Handle_MyRoomGetMyCards(this, last);
                    else if (subopcode == 0x08)
                        MyRoomHandle.Handle_MyRoomSetDefaultCharacter(this, reader, last);
                    else if (subopcode == 0x0B)
                        MyRoomHandle.Handle_MyroomSetCharSetting(this, reader, last);
                    else if (subopcode == 0x14)
                        MyRoomHandle.Handle_ExitGetCurrentCharSetting(this, last);
                    else if (subopcode == 0x1A)
                        MyRoomHandle.Handle_UseLuckyBag(this, reader, last);
                    else if (subopcode == 0x20)
                        MyRoomHandle.Handle_FeedPet(this, reader, last);
                    else if (subopcode == 0x23)
                        MyRoomHandle.Handle_PetRebirth(this, reader, last);
                    else if (subopcode == 0x26)
                        MyRoomHandle.Handle_PetUpgrade(this, reader, last);
                    else if (subopcode == 0x29)
                        MyRoomHandle.Handle_RepairItem(this, reader, last);
                    else if (subopcode == 0x42)
                        MyRoomHandle.Handle_SetSlotItemSetting(this, reader, last);
                    else if (subopcode == 0x45)
                        MyRoomHandle.Handle_GetUserSlotInfo(this, last);
                    else if (subopcode == 0x4A)
                    {
                        //Console.WriteLine("MyRoomHandle_FFCF014A");
                        //Handle_FFCF014A(hPacket, hPacket.Length);
                        MyRoomHandle.Handle_FFCF014A(this, last);
                    }
                    else
                    {
                        Log.Info("Unhandle subopcode: 0x1CF 0x{0:X2}", subopcode);
                        //LogHelp.Logging("Unhandle FFCF01XX: " + ByteArrayToString(hPacket, hPacket.Length));
                    }
                    break;
                case 465: //0x1D1 Farm?? 
                    if (subopcode == 0x27)
                    {
                        //Console.WriteLine("FFD10127");
                        UnknownHandle.Handle_FFD10127(this, last);
                    }
                    else if (subopcode == 0x2E)
                    {
                        //Console.WriteLine("FFD1012E");
                        UnknownHandle.Handle_FFD1012E(this, last);
                    }
                    else if (subopcode == 0x0)
                    {
                        FarmHandle.Handle_EnterFarm(this, reader, last);
                    }
                    else
                    {
                        Log.Info("Unhandle farm subopcode: {0}", Utility.ByteArrayToString(reader.Buffer));
                        //LogHelp.Logging("Unhandle sopcode: " + ByteArrayToString(hPacket, hPacket.Length));
                    }
                    break;
                case 467: //0x1D3
                    //Console.WriteLine("Single play FFD301");
                    LobbyHandle.HandleSinglePlay(this, reader, last);
                    break;
                case 492: //0x1EC
                    Console.WriteLine("FFEC01"); //mission
                    UnknownHandle.Handle_FFEC01(this, last);
                    break;
                case 521: //0x209
                    Console.WriteLine("FF0902"); //usp_collectionMission_GetUserMission?
                    UnknownHandle.Handle_FF0902(this, last);
                    break;
                case 524: //0x20C
                    Console.WriteLine("FF0C02");
                    UnknownHandle.Handle_FF0C02(this, last);
                    break;
                case 528: //0x210
                    Console.WriteLine("FF1002");
                    UnknownHandle.Handle_FF1002(this, last);
                    break;
                case 531: //0x213
                    Console.WriteLine("FF1302"); //usp_optionalMission_GetUserInfo?
                    UnknownHandle.Handle_FF1302(this, last);
                    break;
                case 534: //0x216
                    Console.WriteLine("FF1602");
                    UnknownHandle.Handle_FF1602(this, last);
                    break;
                case 541: //0x21D
                    if (subopcode == 0x01)
                    {
                        Console.WriteLine("FF1D0201");
                        UnknownHandle.Handle_FF1D0201(this, last);
                    }
                    else if (subopcode == 0x03)
                    {
                        Console.WriteLine("FF1D0203");
                        UnknownHandle.Handle_FF1D0203(this, last);
                    }
                    else if (subopcode == 0x04)
                    {
                        Console.WriteLine("FF1D0204");
                        UnknownHandle.Handle_FF1D0204(this, last);
                    }
                    else if (subopcode == 0x05)
                    {
                        Console.WriteLine("FF1D0205");
                        UnknownHandle.Handle_FF1D0205(this, last);
                    }
                    else if (subopcode == 0x06)
                    {
                        Console.WriteLine("FF1D0206");
                        UnknownHandle.Handle_FF1D0206(this, last);
                    }
                    else if (subopcode == 0x07)
                    {
                        Console.WriteLine("FF1D0207");
                        UnknownHandle.Handle_FF1D0206(this, last);
                    }
                    else if (subopcode == 0x08)
                    {
                        Console.WriteLine("FF1D0208");
                        UnknownHandle.Handle_FF1D0208(this, last);
                    }
                    else if (subopcode == 0x09)
                    {
                        Console.WriteLine("FF1D0209");
                        UnknownHandle.Handle_FF1D0209(this, last);
                    }
                    else if (subopcode == 0x0A)
                    {
                        Console.WriteLine("FF1D020A");
                        UnknownHandle.Handle_FF1D020A(this, last);
                    }
                    else if (subopcode == 0x0B)
                    {
                        Console.WriteLine("FF1D020B");
                        UnknownHandle.Handle_FF1D020B(this, last);
                    }
                    else if (subopcode == 0x0C)
                    {
                        Console.WriteLine("FF1D020C");
                        UnknownHandle.Handle_FF1D020C(this, last);
                    }
                    else if (subopcode == 0x0D)
                    {
                        Console.WriteLine("FF1D020D");
                        UnknownHandle.Handle_FF1D020D(this, last);
                    }
                    else if (subopcode == 0x0E)
                    {
                        Console.WriteLine("FF1D020E");
                        UnknownHandle.Handle_FF1D020E(this, last);
                    }
                    else if (subopcode == 0x0F)
                    {
                        Console.WriteLine("FF1D020F");
                        UnknownHandle.Handle_FF1D020F(this, last);
                    }
                    else if (subopcode == 0x11)
                    {
                        Console.WriteLine("FF1D0211");
                        UnknownHandle.Handle_FF1D0211(this, last);
                    }
                    else
                    {
                        Log.Info("Unhandle sopcode: 0x{0:X2}", subopcode);
                        //LogHelp.Logging("Unhandle sopcode: " + ByteArrayToString(hPacket, hPacket.Length));
                    }
                    break;
                case 544: //0x220
                    Console.WriteLine("FF2002");
                    UnknownHandle.Handle_FF2002(this, last);
                    break;
                case 546: //0x222
                    Console.WriteLine("FF2202");
                    UnknownHandle.Handle_FF2202(this, last);
                    break;
                case 611: //0x263
                    //Console.WriteLine("User Logout?? FF6302"); //FF6302
                    //HandleLogout();
                    break;
                case 702: //0x2BE
                    Console.WriteLine("FFBE02");
                    UnknownHandle.Handle_FFBE02(this, last);
                    break;
                case 1281: //0x501
                    Console.WriteLine("FF0105");
                    UnknownHandle.Handle_FF0105(this, last);
                    break;
                case 1284: //0x504
                    UnknownHandle.Handle_FF0405(this, reader, last);
                    break;
                case 1286: //0x506
                    Console.WriteLine("FF0605");
                    UnknownHandle.Handle_FF0605(this, last);
                    break;
                case 0x52E: //0x52E
                    if (subopcode == 0x01)
                        ShuSystemHandle.Handle_Shu_Hatch(this, reader, last);
                    else if (subopcode == 0x03)
                        ShuSystemHandle.Handle_Shu_GetItemInfoByStr(this, reader, last);
                    else if(subopcode == 0x05)
                        ShuSystemHandle.Handle_Shu_GetUserItemInfo(this, reader, last);
                    else if (subopcode == 0x09)
                        ShuSystemHandle.Handle_Shu_ManagerAction(this, reader, last);
                    else if (subopcode == 0x0A)
                        ShuSystemHandle.Handle_Shu_ChangeName(this, reader, last);
                    else if (subopcode == 0x0B)
                        ShuSystemHandle.Handle_Shu_ChangeAvatarInfo(this, reader, last);
                    else if (subopcode == 0x0C)
                        ShuSystemHandle.Handle_Shu_ChangeCurrentShu(this, reader, last);
                    else if (subopcode == 0x0D)
                        ShuSystemHandle.Handle_Shu_UseItem(this, reader, last);
                    else if (subopcode == 0x0E)
                        ShuSystemHandle.Handle_Shu_GetGift(this, reader, last);
                    else if (subopcode == 0x0F)
                        ShuSystemHandle.Handle_Shu_ExploreCheck(this, reader, last);
                    else if (subopcode == 0x10)
                        ShuSystemHandle.Handle_Shu_ExploreStart(this, reader, last);
                    else if (subopcode == 0x11)
                        ShuSystemHandle.Handle_Shu_ExploreStop(this, reader, last);
                    else if (subopcode == 0x12)
                        ShuSystemHandle.Handle_Shu_ExploreReward(this, reader, last);
                    else
                        Log.Info("Unhandle Shu sopcode: {0:X2}, {1}", subopcode, Utility.ByteArrayToString(reader.Buffer));
                    break;
                case 1337: //0x539
                    LobbyHandle.Handle_GetUserPoint(this, reader, last);
                    break;
                case 1430: //0x596
                    Console.WriteLine("FF9605"); //阿努 魔王城 最少攻擊力
                    UnknownHandle.Handle_FF9605(this, last);
                    break;
                case 0x5A6: //0x5A6
                    MyRoomHandle.Handle_MyroomGetFashionMode(this, reader, last);
                    break;
                case 498: //0x1F2
                    Console.WriteLine("FFF201");
                    UnknownHandle.Handle_FFF201(this, last);
                    break;
                case 510: //0x1FE
                    Console.WriteLine("FFFE01");
                    UnknownHandle.Handle_FFFE01(this, last);
                    break;
                //case 116: //0x74
                    //Console.WriteLine("74");
                    //UnknownHandle.Handle_FFFE01(this, last);
                    //break;
                case 0x9B:
                    //Console.WriteLine("CreateGameRoomHandle");
                    GameRoomHandle.Handle_CreateGameRoom(this, reader, last);
                    break;
                case 0xA7:
                    //Console.WriteLine("LeaveRoomHandle");
                    GameRoomHandle.Handle_LeaveRoom(this, reader, last);
                    break;
                case 0x497:
                    GameRoomHandle.Handle_FF9704(this, reader, last);
                    break;
                case 0x97:
                    GameRoomHandle.Handle_GetRoomList(this, reader, last);
                    break;
                case 0x9F:
                    //Console.WriteLine("EnterRoomHandle");
                    GameRoomHandle.Handle_EnterRoom(this, reader, last);
                    break;
                case 0x99:
                    Log.Info("RandomEnterRoom");
                    GameRoomHandle.Handle_RandomEnterRoom(this, reader, last);
                    break;
                case 0xA0:
                    GameRoomHandle.Handle_PlayTogether(this, reader, last);
                    break;
                case 0x2C0:
                    //Log.Info("RoomControl");
                    GameRoomHandle.Handle_RoomControl(this, reader, last);
                    break;
                case 0x4b0:
                    //Console.WriteLine("KickPlayerHandle");
                    GameRoomHandle.Handle_KickPlayer(this, reader, last);
                    break;
                case 0x16C:
                    GameRoomHandle.Handle_PlayerList(this, reader, last);
                    break;
                case 0x1DA:
                    //Console.WriteLine("GameEndInfo");
                    GameRoomHandle.Handle_GameEndInfo(this, reader, last);
                    break;
                case 0x169:
                    //Console.WriteLine("BuyItem");
                    ShopHandle.Handle_BuyItem(this, reader, last);
                    break;
                case 0x170:
                    //Console.WriteLine("GiftItem");
                    ShopHandle.Handle_GiftItem(this, reader, last);
                    break;
                case 0x144:
                    //Console.WriteLine("GetMyroomItemInfo");
                    MyRoomHandle.Handle_GetGiftItemInfo(this, reader, last);
                    break;
                case 0x147:
                    //Console.WriteLine("GetItemInfo");
                    ShopHandle.Handle_GetItemInfo(this, reader, last);
                    break;
                case 0x17C:
                    //Console.WriteLine("GetCurrentMoney");
                    ShopHandle.Handle_GetCurrentGameMoney(this, reader, last);
                    break;
                case 0x172:
                    //Console.WriteLine("GetGiftList");
                    MyRoomHandle.Handle_GetGiftList(this, reader, last);
                    break;
                case 0x174:
                    //Console.WriteLine("AcceptGift");
                    MyRoomHandle.Handle_AcceptGift(this, reader, last);
                    break;
                case 0x68:
                    MyRoomHandle.Handle_MyroomGetUserItemAttr(this, reader, last);
                    break;
                case 0x52F://Monthly Gacha
                    GachaHandle.Handle_GetMonthlyGacha(this, last);
                    break;
                //鍊金/扭蛋&保管箱
                case 0x573:
                    ParkHandle.Handle_GetMachineInfo(this, reader, last);
                    break;
                case 0x197:
                    ParkHandle.Handle_Alchemist_MachineSelect(this, reader, last);
                    break;
                case 0x237:
                    ParkHandle.Handle_MachineReceiveORGiftItem(this, reader, last);
                    break;
                case 0x547:
                    ParkHandle.Handle_MachineKeepItem(this, reader, last);
                    break;
                case 0x549:
                    MyRoomHandle.Handle_GetStorageItemList(this, reader, last);
                    break;
                case 0x54B:
                    MyRoomHandle.Handle_StorageItemGift(this, reader, last);
                    break;
                case 0x54D:
                    MyRoomHandle.Handle_StorageItemReceive(this, reader, last);
                    break;
                //兌換系統
                case 0x4BD:
                    ExchangeHandle.Handle_GetExchangeSystemInfo(this, reader, last);
                    break;
                case 0x4BF:
                    ExchangeHandle.Handle_ExchangeItem(this, reader, last);
                    break;
                case 0x5A4:
                    MyRoomHandle.Handle_MyroomSetFashionMode(this, reader, last);
                    break;

                case 0x76:
                    GMCommandHandle.Handle_Notice(this, reader, last);
                    break;
                case 0x3DD:
                    GMCommandHandle.Handle_DisconnectUser(this, reader, last);
                    break;
                case 0x3DE:
                    GMCommandHandle.Handle_FindGo(this, reader, last);
                    break;
                case 0x51A:
                    LobbyHandle.Handle_GetEventPickBoard(this, reader, last);
                    break;

                case 0x1F8:
                    GameRoomHandle.Handle_FFF801(this, reader, last);
                    break;

                //EnchantSystem
                case 0x58A:
                    EnchantSystem.Handle_GetEnchantItemInfo(this, reader, last);
                    break;
                case 0x58C:
                    EnchantSystem.Handle_StoneMount(this, reader, last);
                    break;
                case 0x590:
                    EnchantSystem.Handle_StoneRemove(this, reader, last);
                    break;
                case 0x592:
                    EnchantSystem.Handle_SealErase(this, reader, last);
                    break;
                case 0x594:
                    EnchantSystem.Handle_Hardening(this, reader, last);
                    break;

                case 0x598:
                    LobbyHandle.Handle_RightClickItemInfo(this, reader, last);
                    break;
                default:
                    Log.Info("Unhandle opcode: {0}", Utility.ByteArrayToString(reader.Buffer));
                    //Log.WriteLine("Unhandle opcode: " + ByteArrayToString(hPacket, hPacket.Length));
                    break;
                    //FF 3A 05 B1 04 00 00 1A 01 00 00 1A 01 00 00 08
                    //FF 3A 05 F0 0A 00 00 06 00 00 00 06 00 00 00 08 mau
                    
            }*/
        }
    }
}
