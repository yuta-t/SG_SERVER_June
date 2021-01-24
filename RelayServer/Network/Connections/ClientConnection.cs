using LocalCommons.Logging;
using LocalCommons.Network;
using RelayServer.Network.Packet.AgentServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using RelayServer.Network.Packet;

namespace RelayServer.Network.Connections
{
    /// <summary>
    /// Connection That Used For Login Server Connection.
    /// </summary>
    public sealed class ClientConnection : IConnectionUDP
    {


        public ClientConnection(Socket socket) : base(socket)
        {
            Log.Info("Client IP: {0} connected", this); ;
            this.DisconnectedEvent += this.LoginConnection_DisconnectedEvent;
            this.m_LittleEndian = true;
            //this.SendAsync(new Net_RegisterRelayServer());
        }

        private void LoginConnection_DisconnectedEvent(object sender, EventArgs e)
        {
            Log.Info("Client IP: {0} disconnected", this);
            this.Dispose();
        }

        public override void HandleReceived(byte[] data, IPEndPoint endPoint)
        {
            var reader = new PacketReader(data, 0);
            reader.ReadByte();
            reader.ReadByte();
            short opcode = reader.ReadLEInt16();
            //Console.WriteLine(Utility.ByteArrayToString(data));
            //Console.WriteLine(endPoint.Address.ToString());
            //Console.WriteLine(endPoint.Port);
            /*var handler = DelegateList.LHandlers[opcode];
            if (handler != null)
                handler.OnReceive(this, reader);
            else
                Log.Info("Received Undefined Packet 0x{0:x2}", opcode);*/
            switch (opcode)
            {
                case 0x3D4:
                    // AgentServerHandle.Handle_RelayRegisterResult(this, reader);
                    UDPHandle.Handle_Connect_04FFD403(this, reader, endPoint);
                    break;
                case 0x3D8:
                    UDPHandle.Handle_Connect_04FFD803(this, reader, endPoint);
                    break;
                case 0x3D6:
                    UDPHandle.Handle_ConnectUser_04FFD603(this, reader, endPoint);
                    break;
                default:
                    break;

            }
        }
    }
}
