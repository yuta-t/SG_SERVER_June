using IniParser;
using IniParser.Model;
using LoadBalanceServer.Network.Connections;
using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalanceServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Boot();
            new AsyncListener2(Conf.ServerIP, Conf.LoadBalanceServerPort, typeof(ClientConnection)); //Waiting For Client Connections
            Console.ReadLine();
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
