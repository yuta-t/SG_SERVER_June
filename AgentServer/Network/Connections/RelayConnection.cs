using AgentServer.Packet.RelayServer;
using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using AgentServer.Packet.RelayServer.Send;
using AgentServer.Controller;

namespace AgentServer.Network.Connections
{
    /// <summary>
    /// Connection That Used Only For Interact With Game Servers.
    /// </summary>
    public class RelayConnection : IConnection
    {

        private RelayServer m_CurrentInfo;

        public RelayServer CurrentInfo
        {
            get { return this.m_CurrentInfo; }
            set { this.m_CurrentInfo = value; }
        }

        public RelayConnection(Socket socket) : base(socket)
        {
            Log.Info("RelayServer IP: {0} connected", this);
            this.DisconnectedEvent += this.ChannelConnection_DisconnectedEvent;
            this.m_LittleEndian = true;
        }

        void ChannelConnection_DisconnectedEvent(object sender, EventArgs e)
        {
            Log.Info("Relay IP: {0} disconnected", this.m_CurrentInfo != null ? this.m_CurrentInfo.Id.ToString() : this.ToString());
            this.Dispose();
            RelayController.DisconnecteRelayServer(this.m_CurrentInfo != null ? this.m_CurrentInfo.Id : this.CurrentInfo.Id);
            this.m_CurrentInfo = null;
            //Game server will be corresponding status offline
        }

        public override void HandleReceived(byte[] data)
        {
            PacketReader reader = new PacketReader(data, 0);
            byte opcode = reader.ReadByte();
            switch (opcode)
            {
                case 0:
                   RelayServerHandle.Handle_RegisterRelayServer(this, reader);
                    //RelayServerHandle.Handle_RelayRegisterResult(this, reader);
                    break;
                case 1:
                    RelayServerHandle.Handle_GetUDPInfo(this, reader);
                    break;
                default:
                    break;

            }
        }
    }
}
