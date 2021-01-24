using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentServer.Structuring.Item;

namespace AgentServer.Structuring.Battle
{
    public class BattleRecord
    {
        public Account battleLeader;
        public Account battledLeader;

        public string[] BattleMoveData = new string[2];
        public string[] BattledMoveData = new string[2];
    }
}
