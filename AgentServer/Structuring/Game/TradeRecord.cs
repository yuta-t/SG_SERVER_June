using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentServer.Structuring.Item;

namespace AgentServer.Structuring.Game
{
    public class TradeRecord
    {
        public int tradeID;
        public Account tradePlayer;
        public Account tradedPlayer;
        public int tradeLock;
        public int tradedLock;
        public int tradeConfirm;
        public int tradedConfirm;
        public int tradeUpdate;
        public int tradedUpdate;
        public int tradeZula;
        public int tradedZula;
        public List<ItemAttr> TradeItem { get; set; } = new List<ItemAttr>();
        public List<ItemAttr> TradedItem { get; set; } = new List<ItemAttr>();
        public List<ItemAttr> TradeInsertedItem { get; set; } = new List<ItemAttr>();
        public List<ItemAttr> TradedInsertedItem { get; set; } = new List<ItemAttr>();
    }
}
