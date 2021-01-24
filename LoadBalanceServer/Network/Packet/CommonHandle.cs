//using CommunityAgentServer.Holders;
using LoadBalanceServer.Network.Connections;
using LoadBalanceServer.Packet.Send;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
//using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using LocalCommons.Utilities;
using LocalCommons.Logging;
using System.Collections.Generic;
using System.IO;

namespace LoadBalanceServer.Packet
{
    public class CommonHandle
    {
        public static void Handle_0x01(ClientConnection Client, PacketReader reader)
        {
            /*3D 00 01 01 00 00 00 03 00 00 00 2E 00 00 00 
             31 2E 31 36 2E 31 34 2E 31 5F 72 65 6C 5F 34 
             64 66 31 39 66 30 65 30 36 31 66 63 30 32 33 
             64 66 34 35 62 65 32 63 64 62 36 63 38 63 32 37*/
            reader.ReadInt32(); //01 00 00 00
            reader.ReadInt32(); //03 00 00 00
            int hashlen = reader.ReadLEInt32();
            string hash = reader.ReadBig5StringSafe(hashlen);
            Log.Info("Client Hash: {0}", hash);

            if (Conf.HashCheck)
            {
                if (!CheckHashIsValid(hash))
                {
                    Log.Warning("InCorrect Hash: {0}", hash);
                    Client.SendAsync(new HashCheckFail());
                    return;
                }
            }

            Client.SendAsync(new HashCheckOK());
        }


        private static bool CheckHashIsValid(string hash)
        {
            List<string> allhashes = File.ReadAllLines("hash.ini").ToList();
            return allhashes.Exists(serverhash => serverhash == hash);
        }


    }

 
}
