﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer
{
    public static class Conf
    {
        public static string Connstr = "server=127.0.0.1;port=3306;user id=root;password=;database=tr_game_db;charset=big5;";
        public static string ServerIP = "";
        public static int AgentPort = 0;
        public static int AgentPort2 = 0;
        public static int RelayPort = 0;
        public static int CommunityAgentServerPort = 0;
        public static int LoadBalanceServerPort = 0;
        public static bool HashCheck = false;
        public static int MaxUserCount = 0;
    }
}
