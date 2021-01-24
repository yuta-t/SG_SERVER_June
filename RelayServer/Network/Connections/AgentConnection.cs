using LocalCommons.Logging;
using LocalCommons.Network;
using RelayServer.Network.Packet.AgentServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer.Network.Connections
{
    /// <summary>
    /// Connection That Used For Login Server Connection.
    /// </summary>
    public sealed class AgentConnection : IConnection
    {

        public AgentConnection(Socket socket) : base(socket)
        {
            Log.Info("Connected to AgentServer, installing data...");
            this.DisconnectedEvent += this.LoginConnection_DisconnectedEvent;
            this.SendAsync(new Net_RegisterRelayServer());
        }

        private void LoginConnection_DisconnectedEvent(object sender, EventArgs e)
        {
            Log.Info("AgentServer IP: {0} disconnected", this);
            this.Dispose();
        }

        public override void HandleReceived(byte[] data)
        {
            var reader = new PacketReader(data, 0);
            byte opcode = reader.ReadByte();

            switch (opcode)
            {
                case 0:
                    AgentServerHandle.Handle_RelayRegisterResult(this, reader);
                    //AgentServerHandle.Handle_RegisterAgentServer(this, reader);
                    break;
                default:
                    break;

            }
        }
    }
}
