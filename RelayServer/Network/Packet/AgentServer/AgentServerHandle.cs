using LocalCommons.Logging;
using LocalCommons.Network;
using RelayServer.Network.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayServer.Network.Packet.AgentServer
{
    public class AgentServerHandle
    {

        private static AgentConnection _mCurrentAgentServer;
        public static AgentConnection CurrentAgentServer
        {
            get { return _mCurrentAgentServer; }
        }

        public static void Handle_RelayRegisterResult(AgentConnection con, PacketReader reader)
        {
            var result = reader.ReadBoolean();
            if (result)
            {
                Log.Info("LoginServer successfully installed");
            }
            else
            {
                Log.Info("Some problems are appear while installing LoginServer");
            }

            if (result)
            {
                _mCurrentAgentServer = con;
            }

        }

    }
}
