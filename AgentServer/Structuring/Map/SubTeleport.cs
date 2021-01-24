using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentServer.Structuring.Item;

namespace AgentServer.Structuring.Map
{
    public class SubTeleport
    {
        public int MapGlobalID;
        public List<TeleportRecord> SubList { get; } = new List<TeleportRecord>();
    }
}
