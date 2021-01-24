using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Linq;
using System.Net.Sockets;
using LocalCommons.Utilities;
using System.Collections.Concurrent;
using LoadBalanceServer.Packet;

namespace LoadBalanceServer.Network.Connections
{

    /// <summary>
    /// Connection That Used For ArcheAge Client( Game Side )
    /// </summary>
    public class ClientConnection : IConnection2
    {
        //----- Static
	    private readonly byte _mRandom;


        public byte NumPck = 0; 
        //public static ConcurrentDictionary<string, Account> CurrentAccounts { get; } = new ConcurrentDictionary<string, Account>();

	    //public Account CurrentAccount { get; set; }

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
			/*try
			{
            }
			catch
			{
				Log.Warning("Client IP: {0} disconnected,But the remove fail", this);
			}*/
			
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

            //byte last = reader.Buffer.Reverse().Take(1).ToArray()[0];

            switch (opcode)
            {
                case 0x01: //checking
                    CommonHandle.Handle_0x01(this, reader);
                    break;

                default:
                    Log.Info("Unhandle opcode: {0:X2}  |   {1}", opcode,Utility.ByteArrayToString(reader.Buffer));
                    break;

            }
        }
    }
}
