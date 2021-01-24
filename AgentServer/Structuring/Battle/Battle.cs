using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System.Data;
using AgentServer.Structuring.Item;

namespace AgentServer.Structuring.Battle
{
    public static class Battle
    {
        public static List<BattleRecord> BattleList { get; } = new List<BattleRecord>();

        public static void AddBattle(BattleRecord battleRecord)
        {
            BattleList.Add(battleRecord);
        }
        public static void RemoveBattle(BattleRecord battleRecord)
        {
            BattleList.Remove(battleRecord);
        }
    }
}
