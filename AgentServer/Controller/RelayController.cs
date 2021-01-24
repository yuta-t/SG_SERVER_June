//using RelayServer.Structuring;
using LocalCommons.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using AgentServer.Network.Connections;

namespace AgentServer.Controller
{
    class RelayController
    {

        public static Dictionary<byte, RelayServer> CurrentRelayServer { get; } = new Dictionary<byte, RelayServer>();

        public static bool RegisterRelayServer(byte id, string password, RelayConnection con, short port, string ip)
        {
            if (!CurrentRelayServer.ContainsKey(id))
            {
                Log.Info("Game Server ID: {0} is not defined, please check", id);
                return false;
            }

            var template = CurrentRelayServer[id]; //Checking Containing By Packet

            if (con.CurrentInfo != null) //Fully Checking.
            {
                con.CurrentInfo = null;
            }

            if (template.password != password) //Checking Password
            {
                Log.Info("Game Server ID: {0} bad password", id);
                return false;
            }

            var server = CurrentRelayServer[id];
            server.CurrentConnection = con;
            server.IPAddress = ip;
            server.Port = port;
            con.CurrentInfo = server;
            //Update
            CurrentRelayServer.Remove(id);
            CurrentRelayServer.Add(id, server);
            Log.Info("RelayServer ID: {0} registered", id);
            return true;
        }
        public static bool DisconnecteRelayServer(byte id)
        {
            var server = CurrentRelayServer[id];
            server.CurrentConnection = null;
            CurrentRelayServer.Remove(id);
            CurrentRelayServer.Add(id, server);
            return true;
        }

        public static void LoadAvailableRelayServer()
        {
            var ser = new XmlSerializer(typeof(RelayTemplate));
            var template = (RelayTemplate)ser.Deserialize(new FileStream(@"system/data/Servers.xml", FileMode.Open));
            for (var i = 0; i < template.xmlservers.Count; i++)
            {
                var game = template.xmlservers[i];
                game.CurrentAuthorized = new List<long>();
                CurrentRelayServer.Add(game.Id, game);
            }

            Log.Info("Loading from Servers.xml {0} servers", CurrentRelayServer.Count);
        }
    }

    #region Classes For Server Info Deserialization.

    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot(ElementName = "servers", Namespace = "", IsNullable = false)]
    public class RelayTemplate
    {
        [XmlElement("server", Form = XmlSchemaForm.Unqualified)]
        public List<RelayServer> xmlservers;
    }

    [Serializable]
    [XmlType(Namespace = "", AnonymousType = true)]
    public class RelayServer
    {
        [XmlAttribute]
        public byte Id;

        [XmlIgnore]
        public string IPAddress;

        [XmlIgnore]
        public short Port;

        [XmlIgnore]
        public List<long> CurrentAuthorized;

        [XmlIgnore]
        public RelayConnection CurrentConnection;

        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public short MaxPlayers;

        [XmlAttribute]
        public string password;

        public bool IsOnline()
        {
            return this.CurrentConnection != null;
        }
        #endregion

    }
}
