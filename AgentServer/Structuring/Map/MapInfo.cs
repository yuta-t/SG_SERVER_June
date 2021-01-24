using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Structuring.Map
{
    public class MapInfo
    {
        public int MapGlobalID { get; set; }
        public string MapID { get; set; }
        public byte[] decodedMapID = new byte[4];
        public string MapName { get; set; }
        public string MapMusic { get; set; }
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public string MapCRC { get; set; }
        public List<TeleportRecord> TeleportList { get; set; } = new List<TeleportRecord>();
        //public int MapNum;
        public bool CanTimeAttack;
        public int RuleType;
        //1 normal
        //2 生存
        //3 Hardcore
        public int GoalInLimitTime;
    }

    public class MapCapsuleItemInfo
    {
        public int PresentRuleType;
        public int Argument;
        public int GameItemNum;
        public int Rate;
    }
}
