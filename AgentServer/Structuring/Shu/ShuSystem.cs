using AgentServer.Holders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Structuring.Shu
{
    public class DBShuInfo
    {
        public List<long> characterItemID = new List<long>();
        public ShuEggInfo eggitem = new ShuEggInfo();
        public ConcurrentDictionary<long, List<ShuItemInfo>> shuitems = new ConcurrentDictionary<long, List<ShuItemInfo>>();
        public ConcurrentDictionary<long, ShuCharInfo> shuchars = new ConcurrentDictionary<long, ShuCharInfo>();
        public ConcurrentDictionary<long, List<ShuAvatarInfo>> shuavatars = new ConcurrentDictionary<long, List<ShuAvatarInfo>>();
        public ConcurrentDictionary<long, List<ShuStatusInfo>> shustatus = new ConcurrentDictionary<long, List<ShuStatusInfo>>();
    }

    public class DBShuActionInfo
    {
        public int remainMP;
        public List<ShuActionResultInfo> ActionResult = new List<ShuActionResultInfo>();
        public ConcurrentDictionary<long, List<ShuStatusInfo>> shustatus = new ConcurrentDictionary<long, List<ShuStatusInfo>>();
        public int beforeLevel;
        public int afterLevel;
    }

    public class DBShuChangeAVInfo
    {
        public List<ShuAvatarState> AvatarState = new List<ShuAvatarState>();
        public ConcurrentDictionary<long, List<ShuAvatarInfo>> shuavatars = new ConcurrentDictionary<long, List<ShuAvatarInfo>>();
    }
    public class DBShuUseItemInfo
    {
        public List<ShuItemInfo> ItemInfos = new List<ShuItemInfo>();
        public ConcurrentDictionary<long, List<ShuStatusInfo>> shustatus = new ConcurrentDictionary<long, List<ShuStatusInfo>>();
        public int remainMP;
        public ConcurrentDictionary<long, ShuCharInfo> shuchars = new ConcurrentDictionary<long, ShuCharInfo>();
        public List<long> characterItemID = new List<long>();
        public ConcurrentDictionary<long, List<ShuAvatarInfo>> shuavatars = new ConcurrentDictionary<long, List<ShuAvatarInfo>>();
        public int beforeLevel;
        public int afterLevel;
    }

    public class ShuItemInfo
    {
        public int itemdescnum;
        public long itemID;
        public long gotDateTime;
        public int count;
        public int state;
    }

    public class ShuCharInfo
    {
        public int avatarItemNum;
        public string Name;
        public int state;
        public long MotionList;
        public long PurchaseMotionList;
    }

    public class ShuAvatarInfo
    {
        public int Position;
        public long itemID;
        public int avatarItemNum;
    }

    public class ShuStatusInfo
    {
        public int statustype;
        public int value;
    }

    public class ShuActionResultInfo
    {
        public int statusType;
        public int giveValue;
    }

    public class ShuAvatarState
    {
        public long itemID;
        public int state;
    }

    public class ShuItemCPK
    {
        public short character;
        public int position;
        public short kind;
    }

    public class ShuEggInfo
    {
        public long gotDateTime;
        public int count;
        public int state;
    }

    public class ExploreInfo
    {
        public byte zoneNum;
        public long characterItemID;
        public long endDateTime;
    }

    public class ShuRewardInfo
    {
        public int rewardType;
        public int rewardItem;
        public int rewardCount;
    }

    public class UserShuInfo
    {
        public string ShuName;
        public int ShuItemNum;
        public List<short> ShuAvatarKind = new List<short>(6){ 0, 0, 0, 0, 0, 0 };

        /*public int Satiety;
        public int Friendship;
        public int Satisfaction;
        public int Exp;*/
       // public List<ShuStatusInfo> Statusinfo = new List<ShuStatusInfo>(4);
        public List<int> Statusinfo = new List<int>(4) { 0, 0, 0, 0 };

        public long MotionList;

        //public ShuAvatarInfo CurrentAvatarInfo = new ShuAvatarInfo();


        public void updateavatar(List<ShuAvatarInfo> avatarinfos)
        {
            //int x = 0;
            foreach (var i in avatarinfos.OrderBy(o => o.Position))
            {
                if (ItemHolder.ShuItemCPKInfos.TryGetValue(i.avatarItemNum, out var cpkinfo))
                {
                    ShuAvatarKind[i.Position] = i.Position == 0 ? cpkinfo.character : cpkinfo.kind;
                }
                else
                    ShuAvatarKind[i.Position] = 0;
            }
        }
        public void updatecharinfo(ShuCharInfo charinfo)
        {
            ShuName = charinfo.Name;
            ShuItemNum = charinfo.avatarItemNum;
            MotionList = charinfo.MotionList;
        }
        public void updatestatusinfo(List<ShuStatusInfo> statusinfo)
        {
            //Statusinfo = statusinfo;
            foreach (var i in statusinfo)
            {
                Statusinfo[i.statustype] = i.value;
            }
        }
        public void UpdateEachstatusinfo(List<ShuStatusInfo> statusinfo)
        {
            foreach (var i in statusinfo)
            {
                Statusinfo[i.statustype] = i.value;
            }
        }
    }

}
