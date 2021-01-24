using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Structuring.GameReward
{
    public class GameRewardGroupInfo
    {
        public short GroupType; //0 = common 1 = roomkind 2 = mapnum 3 = nuk
        public int Argument;
        public int ChildGroupNum;
        public float SpecialRewardRate;
    }

    public class GameRewardSubGroupInfo
    {
        public int SubGroup;
        public int SubGroupType;
        public float SubGroupRate;
        public byte StartRank;
        public byte EndRank;
        public int RaceRate;
        public float OnePlusOneRate;
    }

    public class GameRewardGroupRate
    {
        public int GroupNum;
        public int SubGroup;
        public float Rate;
        public short RewardType;
        public int RewardID;
        public int Amount;
    }

    public class GameRewardResult
    {
        public short RewardType;
        public int RewardID;
        public int RewardAmount;
    }

}
