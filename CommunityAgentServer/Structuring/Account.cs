using System.Collections.Generic;
using CommunityAgentServer.Network.Connections;

namespace CommunityAgentServer.Structuring
{
	/// <summary>
	/// Stucture That Contains Information About Account
	/// </summary>
	public class Account
	{
        public string NickName{ get; set; }
        //public string UserID { get; set; }
        //public int Session { get; set; } //cookie
        public ClientConnection Connection { get; set; }
    }
}
