using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentServer.Structuring.Item
{
    public class ItemShopInfo
    {
        public bool CanBuy;
        public ushort ItemPosition;
        public bool NotDeleteWhenExpired;
    }

    public class ExchangeItemInfo
    {
        public int type;
        public int id;
        public int count;
    }
}
