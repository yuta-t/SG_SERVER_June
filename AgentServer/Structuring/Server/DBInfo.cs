using System.Collections.Concurrent;
using System.Collections.Generic;
using AgentServer.Network.Connections;
using AgentServer.Structuring.Item;

namespace AgentServer.Structuring
{
    public class SettingInfo
    {
        public string Key;
        public string Value;
        public bool OnlyServerSetting;
    }

    public class ServerSetting
    {
        public float MultiplyTR;
        public float MultiplyEXP;
        public byte SurvivalMaxUserNum;
        public byte SurvivalMinUserNum;
        public string GateNoticeURL;
        public string QuitConfirmDialogURL;
        public string cashFillUpURL;
        public string EveryDayEventURL;
        public long NewbieOnlyChannelLimitExp;
        public int LoadingTimeOutMilliSeconds;
        public float RABBIT_TURTLE_FATIGUE_DEC;
        public float RABBIT_TURTLE_FATIGUE_INC;
        public float RABBIT_TURTLE_ITEM_FATIGUE_DEC;
        public float RABBIT_TURTLE_ITEM_FATIGUE_INC;
        public byte corunModeMinPlayerNum;
        public int corunModeDecreaseEnergyRatio;
    }
 }
