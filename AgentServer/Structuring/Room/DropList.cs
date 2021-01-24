using AgentServer.Structuring.GameReward;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Structuring
{
    public class DropList
    {
        public int UserNum;
        public long TotalEXP;
        public float RaceDistance;
        public int ServerLapTime;
        public int LapTime;
        public byte Pos;
        public byte Team;
        public byte RelayTeam;
        public byte RelayTeamPos;
        public short BounsTR;
        public short BounsEXP;
        public int TR;
        public int EXP;
        public byte Rank;
        public bool isLevelUP;
        public List<int> CardID = new List<int>();
        public List<GameRewardResult> RewardItemID = new List<GameRewardResult>();

        public int MiniGamePoint;
        public int MiniGameStarPoint;
    }
}
