using CommunityAgentServer.Network.Connections;
using IniParser;
using IniParser.Model;
using LocalCommons.Logging;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CommunityAgentServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TextWriter _writer = new ConsoleTextBoxWriter(richTextBox1);
            Console.SetOut(_writer);
            Boot();

            new AsyncListener2(Conf.ServerIP, Conf.CommunityAgentServerPort, typeof(ClientConnection)); //Waiting For Client Connections
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
                Conf.MaxUserCount = Convert.ToInt32(data["Server"]["MaxUserCount"]);
                Log.Info("Loading Settings.ini........Done");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to shut down the server?", "TRServer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Close();
            }
        }
    }
}
