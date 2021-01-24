using LocalCommons.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalCommons.Utilities;
using AgentServer.Database;
using AgentServer.Structuring;
using AgentServer.Network.Connections;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using AgentServer.Structuring.Item;

namespace AgentServer.Packet.Send
{
    public sealed class GetExchangeSystemOK : NetPacket
    {
        public GetExchangeSystemOK(Account User, int SystemNum, byte last)
        {
            //FF BE 04 2D 00 00 00 00 00 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x4BE);
            ns.Write(SystemNum);
            ns.Write(0); 
            ns.Write(last);
        }
    }

    public sealed class ExchangeItem : NetPacket
    {
        public ExchangeItem(List<ExchangeItemInfo> exinfo, byte last)
        {
            //FF C0 04 00 00 00 00 01 00 00 00 64 00 00 00 3E 5A 00 00 01 00 00 00 08
            bool isSuccess = exinfo.Count > 0;
            ns.Write((byte)0xFF);
            ns.Write((short)0x4C0);
            ns.Write(isSuccess ? 0 : 2);//0=success, 1=db error, 2=system error, 3=not enough item/point
            if (isSuccess)
            {
                ns.Write(exinfo.Count); //count? type?
                foreach (var i in exinfo)
                {
                    ns.Write(i.type);
                    ns.Write(i.id);
                    ns.Write(i.count);
                }
            }
            ns.Write(last);
        }
    }
}
