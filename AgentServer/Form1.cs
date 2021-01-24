using AgentServer.Network.Connections;
using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgentServer.Database;
using IniParser;
using IniParser.Model;
using LocalCommons.Logging;
using AgentServer.Controller;
using AgentServer.Holders;
using AgentServer.Structuring;
using AgentServer.Packet.Send;
using System.Diagnostics;
using AgentServer.Dialog;
using AgentServer.Structuring.Game;
using AgentServer.Structuring.Map;

namespace AgentServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private static void decodedMultiBytes(byte[] src)
        {
            //Console.WriteLine(Utility.ByteArrayToString(User.CharacterDecodedHairClump1));
            //Console.WriteLine(" ");
            int result = 0;
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    result = ((src[i] - 0x80) << 7);
                }
                else if (i == 1)
                {
                    result = (((src[i] - 0x80) + result) << 7);
                }
                else if (i == 2)
                {
                    result = (((src[i] - 0x80) + result) << 7);
                }
                else if (i == 3)
                {
                    result = (((src[i] - 0x80) + result) << 7);
                }
                else if (i == 4)
                {
                    result = result + src[i];
                }
                Console.WriteLine("decodedMultiBytes: {0}", result);
            }
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            TextWriter _writer = new ConsoleTextBoxWriter(richTextBox1);
            Console.SetOut(_writer);
            //InstallRelayServer();
            Boot();
            /*
            RoomHolder.LoadRoomKindInfo();
            MapHolder.LoadMapInfo();
            MapItemHolder.LoadMapCapsuleItemInfo();
            MapCardHolder.LoadMapCardRateInfo();
            ItemHolder.LoadItemInfo();

            ItemHolder.LoadItemAttrInfo();
            ItemHolder.LoadItemSetInfo();
            ItemHolder.LoadPetExpInfo();
            ItemHolder.LoadExchangeSystemInfo();
            CapsuleMachineHolder.LoadCapsuleMachineInfo();
            AccountHolder.LoadLevelInfo();
            RunQuizHolder.LoadRunQuizInfo();
            GameModeHolder.LoadCorunModeResultInfo();
            GameRewardHolder.LoadGameRewardInfo();
            */
            Trade.LoadTradeInfo();
            Map.LoadTeleportInfo();
            //decodedMultiBytes(StringToByteArrayFastest("86A0808437"));

            new AsyncListener(Conf.ServerIP, Conf.AgentPort, typeof(ClientConnection)); //Waiting For Client Connections
            //new AsyncListener(Conf.ServerIP, Conf.AgentPort2, typeof(RelayConnection)); //Waiting For Relay Connections
            label2.Text = Conf.ServerIP;
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
                Conf.Connstr = data["Server"]["MySQLConnection"];
                Conf.HashCheck = Convert.ToBoolean(data["Server"]["HashCheck"]);
                Conf.MaxUserCount = Convert.ToInt32(data["Server"]["MaxUserCount"]);
                Log.Info("Loading Settings.ini........Done");

                //RelayController.LoadAvailableRelayServer();
         
                /*
                DBInit.initlevel_run();
                DBInit.inithacktool_run();
                ServerSettingHolder.LoadServerSettingInfo();
                DBInit.initSmartChannelModeInfo_run();
                DBInit.initSmartChannelScheduleInfo_run();
                DBInit.initRoomKindPenaltyInfo_run();

                Log.Info("Initializing DB........Done");
                */
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

        private void chkServerReady_CheckedChanged(object sender, EventArgs e)
        {
            ServerStatus.isReady = chkServerReady.Checked;
            Log.Info("Set Server Ready : {0}", ServerStatus.isReady);
        }

        private void btnNotice_Click(object sender, EventArgs e)
        {
            string content = txtNotice.Text;
            if (content.Length > 0)
            {
                foreach (Account User in ClientConnection.CurrentAccounts.Values)
                {
                    ClientConnection Client = User.Connection;
                    Client.SendAsync(new NoticePacket(User, content, 0x10));
                }
                Log.Info("Send notice NoticeType : 0, noticeKind : 1,  {0}", content);
            }
        }

        private void btnOpenHash_Click(object sender, EventArgs e)
        {
            Process.Start("hash.ini");
        }

        private void btnOpenDir_Click(object sender, EventArgs e)
        {
            string dir = Application.StartupPath;
            Process.Start("explorer.exe", dir);
        }

        private void btnShowUserNum_Click(object sender, EventArgs e)
        {
            int usernum = ClientConnection.CurrentAccounts.Count;
            int room = Rooms.RoomList.Values.Count(rm => rm.RoomKindID != 0x4A);
            //Rooms.NormalRoomList.Count(rm => rm.RoomKindID != 0x4A);
            int parkroom = Rooms.RoomList.Values.Count(rm => rm.RoomKindID == 0x4A);
            Log.Info("user({0}), room({1}), parkroom({2})", usernum, room, parkroom);
        }

        private void btnReloadtblServerSettingInfo_Click(object sender, EventArgs e)
        {
            //DBInit.initGameserverSetting_run();
            ServerSettingHolder.LoadServerSettingInfo();
            foreach (Account User in ClientConnection.CurrentAccounts.Values)
            {
                ClientConnection Client = User.Connection;
                Client.SendAsync(new NP_Byte(User, DBInit.GameServerSetting));
            }
            Log.Info("Reloaded tblServerSettingInfo!");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            int maxLines = 100;
            if (richTextBox1.Lines.Length > maxLines)
            {
                int moreLines = richTextBox1.Lines.Length - maxLines;
                string[] lines = richTextBox1.Lines;
                Array.Copy(lines, moreLines, lines, 0, maxLines);
                Array.Resize(ref lines, maxLines);
                richTextBox1.Lines = lines;
            }
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void btnCapsuleMachineManager_Click(object sender, EventArgs e)
        {
            var cpm = new CapsuleMachineManager();
            cpm.Show();
        }

        private void btnReloadMap_Click(object sender, EventArgs e)
        {
            MapHolder.LoadMapInfo();
        }

        private void btnGMTool_Click(object sender, EventArgs e)
        {
            var gmtool = new GMTool();
            gmtool.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GameRewardHolder.LoadGameRewardInfo();
        }

        private void btnTeleportLeftBottomCorner_Click(object sender, EventArgs e)
        {
            int usernum = ClientConnection.CurrentAccounts.Count;
            if (usernum > 0)
            {
                //NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
                //room.TeleportPlayers();
            }
        }

        /*private static void InstallRelayServer()
        {
            while (true)
            {
                var point = new IPEndPoint(IPAddress.Parse("192.168.1.4"), 9499);
                var con = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    con.Connect(point);
                }
                catch (Exception)
                {
                    //throw exp;
                    Logger.Trace("Unable to connect to relay server, retry after 1 second");
                }
                if (con.Connected)
                {
                    new RelayConnection(con);
                }
                else
                {
                    continue;
                }

                break;
            }
        }*/
    }
}
