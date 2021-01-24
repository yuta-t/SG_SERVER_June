using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CommunityAgentServer.Structuring;
using CommunityAgentServer.Packet;
/*using AgentServer.Database;*/
//using MySql.Data.MySqlClient;
using System.Data;
//using LocalCommons.Cryptography;
using LocalCommons.Utilities;
using System.Collections.Concurrent;
//using CommunityAgentServer.Controller;

namespace CommunityAgentServer.Network.Connections
{

    /// <summary>
    /// Connection That Used For ArcheAge Client( Game Side )
    /// </summary>
    public class ClientConnection : IConnection2
    {
        //----- Static
	    private readonly byte _mRandom;


        public byte NumPck = 0; 
        public static ConcurrentDictionary<string, Account> CurrentAccounts { get; } = new ConcurrentDictionary<string, Account>();

	    public Account CurrentAccount { get; set; }

        public ClientConnection(Socket socket) : base(socket)
        {
            Log.Info("Client IP: {0} connected", this);
	        this.DisconnectedEvent += this.ClientConnection_DisconnectedEvent;
	        this.m_LittleEndian = true;
            //LoginHandle = new LoginHandle();
            //this.CurrentAccount = new Account { isLogin = false};
        }



        public override void SendAsync(NetPacket packet)
        {
            packet.IsArcheAgePacket = true;
            //NetPacket.NumPckSc = this.NumPck;
            base.SendAsync(packet);
	        //this.NumPck = NetPacket.NumPckSc;
        }

        public void SendAsyncd(NetPacket packet)
        {
            packet.IsArcheAgePacket = false;
            base.SendAsync(packet);
        }
        void ClientConnection_DisconnectedEvent(object sender, EventArgs e)
        {
			//remove user
			try
			{
                if(CurrentAccounts.ContainsKey(CurrentAccount.NickName))
                    CurrentAccounts.TryRemove(CurrentAccount.NickName, out _);
            }
			catch
			{
				Log.Warning("Client IP: {0} disconnected,But the remove fail", this);
			}
			
            Log.Info("Client IP: {0} disconnected", this);
	        this.Dispose();
        }

        public override void HandleReceived(byte[] data)
        {
            PacketReader reader = new PacketReader(data, 0);
            //byte header = 0;
            byte opcode = 0;
            //byte subopcode = 0;

            opcode = reader.ReadByte(); //Packet Opcode/Level           

            switch (opcode)
            {
                case 0x02: //checking
                    CommonHandle.Handle_0x02(this, reader);
                    break;
                case 0x07:
                    //Log.Info("opcode 09");
                    CommonHandle.Handle_0x07(this, reader);
                    break;
                case 0x09:
                    Log.Info("opcode 09 ping?");
                    //CommonHandle.Handle_0x09(this);
                    break;
                case 0x0E:
                    CommonHandle.Handle_0x0E(this, reader);
                    break;

                default:
                    Log.Info("Unhandle opcode: {0:X2}  |   {1}", opcode,Utility.ByteArrayToString(reader.Buffer));
                    break;

            }
        }
    }
}
