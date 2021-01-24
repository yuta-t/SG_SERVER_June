using LocalCommons.Logging;
using LocalCommons.Network;
using RelayServer.Network.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RelayServer.Network.Packet;
using IniParser;
using IniParser.Model;

namespace RelayServer
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Boot();
            InstallAgentServer();
            new AsyncListenerUDP(Conf.ServerIP, Conf.RelayPort, typeof(ClientConnection)); //Waiting For Client Connections
            Console.ReadLine();
        }

        private static void InstallAgentServer()
        {
            while (true)
            {
                var point = new IPEndPoint(IPAddress.Parse(Conf.ServerIP), Conf.AgentPort2);
                var con = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    con.Connect(point);
                }
                catch (Exception)
                {
                    //throw exp;
                    Logger.Trace("Unable to connect to agent server, retry after 1 second");
                }
                if (con.Connected)
                {
                    new AgentConnection(con);
                }
                else
                {
                    continue;
                }

                break;
            }
        }

        public static bool Boot()
        {
            try
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("settings.ini");
                Conf.ServerIP = data["Server"]["AgentServerIP"];
                Conf.AgentPort = Convert.ToInt16(data["Server"]["AgentServerTCPPort"]);
                Conf.AgentPort2 = Convert.ToInt16(data["Server"]["AgentServerTCPPort2"]);
                Conf.RelayPort = Convert.ToInt16(data["Server"]["RelayServerPort"]);
                Conf.CommunityAgentServerPort = Convert.ToInt16(data["Server"]["CommunityServerPort"]);
                Conf.LoadBalanceServerPort = Convert.ToInt16(data["Server"]["LoadBalanceServerPort"]);
                Conf.Connstr = data["Server"]["MySQLConnection"];
                Conf.HashCheck = Convert.ToBoolean(data["Server"]["HashCheck"]);
                Log.Info("Loading Settings.ini........Done");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
