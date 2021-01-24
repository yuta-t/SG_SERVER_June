using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Structuring.Item
{
    public class ItemAttr : ICloneable
    {
        public ushort Attr;
        public float AttrValue;
        public int ItemCorrect;
        public int ItemGlobalID;
        public int ItemDecodedID;
        public byte[] ItemEncodedID;
        public int ItemPos;
        public int ItemCount;
        public int ItemTypeNum;
        public int ItemAppearance;
        public int ItemWear;
        public int ItemPhysicalDamage;
        public int ItemMagicDamage;
        public string ItemType;
        public string ItemName;
        public string ItemDesc;
        public string ItemValidDate;
        public string ItemUsageLimit;
        public string ItemCombinable;
        public int ItemWeight;
        public int ItemDurability;
        public int[] ItemFoodAttackEffect = new int[15];
        public int[] ItemFoodDefenceEffect = new int[15];
        public int[] ItemEquipmentAttackEffect = new int[10];
        public int[] ItemEquipmentDefenceEffect = new int[10];
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class ItemSetAttr
    {
        public ushort Attr;
        public float AttrValue;
        public byte ApplyTarget;
    }

    public class AvatarItemInfo
    {
        public short character;
        public ushort position;
        public ushort kind;
        public int itemdescnum;
        public long expireTime;
        public long gotDateTime;
        public int count;
        public int exp;
        public bool use;

        public void UpdateItemInfo(int newcount, long newexpireTime, long newgotDateTime)
        {
            this.count = newcount;
            this.expireTime = newexpireTime;
            this.gotDateTime = newgotDateTime;
        }

    }

}
