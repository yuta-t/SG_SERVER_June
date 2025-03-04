using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using LocalCommons.Cookie;
using LocalCommons.Cryptography;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Net;
using AgentServer.Database;
using LocalCommons.Utilities;
using System.Text;
using System.Collections.Generic;
using LocalCommons.Logging;
using System.Threading;
using System.Threading.Tasks;
using AgentServer.Structuring.Map;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Game;
using AgentServer.Structuring.Battle;

namespace AgentServer.Packet
{
    public class GameRoomHandle
    {
        private static int decodedDynamicBytes(byte[] src)
        {
            int result = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (i == 0)
                {
                    if (src[i] > 0x7F)
                    {
                        result = ((src[i] - 0x80) << 7);
                    }
                    else
                    {
                        result = src[i];
                        break;
                    }
                }
                else if (i == 1)
                {
                    if(src[i] > 0x7F)
                    {
                        result = (((src[i] - 0x80) + result) << 7);
                    }
                    else
                    {
                        result = result + src[i];
                        break;
                    }
                }
                else if (i == 2)
                {
                    if (src[i] > 0x7F)
                    {
                        result = (((src[i] - 0x80) + result) << 7);
                    }
                    else
                    {
                        result = result + src[i];
                        break;
                    }
                }
                else if (i == 3)
                {
                    if (src[i] > 0x7F)
                    {
                        result = (((src[i] - 0x80) + result) << 7);
                    }
                    else
                    {
                        result = result + src[i];
                        break;
                    }
                }
                else if (i == 4)
                {
                    result = result + src[i];
                }
            }
            //Console.WriteLine("decodedMultiBytes: {0}", result);
            return result;
        }
        public static void Handle_DualRequest(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            Account battledLeader;
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);

            int unk1 = reader.ReadLEInt32();
            ushort unk2 = reader.ReadLEUInt16();
            byte unk3 = reader.ReadByte();

            int battleIDCount = 1;
            byte battledID1 = reader.ReadByte();
            byte battledID2 = 0x00;
            byte battledID3 = 0x00;
            if (battledID1 > 0x7F)
            {
                battledID2 = reader.ReadByte();
                battleIDCount++;
            }
            if (battledID2 > 0x7F)
            {
                battledID3 = reader.ReadByte();
                battleIDCount++;
            }
            byte[] battledID = new byte[battleIDCount];
            for (int i = 0; i < battleIDCount; i++)
            {
                if (i == 0)
                {
                    battledID[i] = battledID1;
                }
                else if (i == 1)
                {
                    battledID[i] = battledID2;
                }
                else if (i == 2)
                {
                    battledID[i] = battledID3;
                }
            }

            battledLeader = room.Players.Find(p => p.GlobalID == decodedDynamicBytes(battledID));
            BattleRecord battle = new BattleRecord
            {
                battleLeader = User,
                battledLeader = battledLeader,
            };

            Battle.AddBattle(battle);
            Client.SendAsync(new GameRoom_DualWaiting_0x2D1_00());
            battledLeader.Connection.SendAsync(new GameRoom_DualConfirm_0x2D1_01());
        }
        public static void Handle_DualConfirm(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;

            int battledPlayerCount = Battle.BattleList.Count(p => p.battledLeader == User);
            BattleRecord battle = null;
            if (battledPlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battledLeader == User);
            }
            battle.battleLeader.Connection.SendAsync(new GameRoom_BattleStart_0x3FA());
            battle.battledLeader.Connection.SendAsync(new GameRoom_BattleStart_0x3FA());
            battle.battleLeader.Connection.SendAsync(new GameRoom_LoadBattleMap_0x488());
            battle.battledLeader.Connection.SendAsync(new GameRoom_LoadBattleMap_0x488());
            battle.battleLeader.Connection.SendAsync(new GameRoom_BattleInfo_0x41C());
            battle.battledLeader.Connection.SendAsync(new GameRoom_BattleInfo_0x41C());
            battle.battleLeader.Connection.SendAsync(new GameRoom_BattleUnitSet_0x3D3(battle.battleLeader, battle.battledLeader, 0));
            battle.battledLeader.Connection.SendAsync(new GameRoom_BattleUnitSet_0x3D3(battle.battledLeader, battle.battleLeader, 1));
            battle.battleLeader.Connection.SendAsync(new GameRoom_BattleS2CPrivate_0x3C2());
            battle.battledLeader.Connection.SendAsync(new GameRoom_BattleS2CPrivate_0x3C2());
        }
        public static void Handle_DualBattleCancel(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            byte unk1 = reader.ReadByte();

            if(unk1 != 0x00)
            {
                return;
            }
            int battlePlayerCount = Battle.BattleList.Count(p => p.battleLeader == User);
            BattleRecord battle = null;
            if (battlePlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battleLeader == User);
            }
            Battle.RemoveBattle(battle);
            battle.battledLeader.Connection.SendAsync(new GameRoom_DualBattledCancel_0x2D1_02());
        }
        public static void Handle_DualBattledCancel(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;

            int battledPlayerCount = Battle.BattleList.Count(p => p.battledLeader == User);
            BattleRecord battle = null;
            if (battledPlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battledLeader == User);
            }
            Battle.RemoveBattle(battle);
            battle.battleLeader.Connection.SendAsync(new GameRoom_DualBattleCancel_0x2D1_03());
        }
        public static void Handle_DualMove(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            int battlePlayerCount = Battle.BattleList.Count(p => p.battleLeader == User);
            int battledPlayerCount = Battle.BattleList.Count(p => p.battledLeader == User);
            BattleRecord battle = null;

            Console.WriteLine("Dual Move battlePlayerCount, battledPlayerCount: {0}, {1}", battlePlayerCount, battledPlayerCount);
            if (battlePlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battleLeader == User);
            }
            else if (battledPlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battledLeader == User);
            }

            ushort unk1 = reader.ReadLEUInt16();
            int moveStep = reader.ReadByte();
            ushort unk2 = reader.ReadLEUInt16();
            byte unk3 = reader.ReadByte();

            moveStep = moveStep - 3;
            if (battlePlayerCount > 0)
            {
                battle.BattleMoveData[1] += "&HED#64#0#0:SUI#" + "F" + ":"; //F,D0:
                battle.BattledMoveData[0] += "&HED#64#0#0:SUI#" + "1" + ":"; //F,D0:
            }
            else if (battledPlayerCount > 0)
            {
                battle.BattleMoveData[0] += "&HED#64#0#0:SUI#" + "1" + ":"; //F,D0:
                battle.BattledMoveData[1] += "&HED#64#0#0:SUI#" + "F" + ":"; //F,D0:
            }
            
            for (int i = 0; i < (moveStep / 2 + 1); i++)
            {
                if(i > 0)
                {
                    if (battlePlayerCount > 0)
                    {
                        battle.BattleMoveData[1] += "&HED#" + (64 + i).ToString() + "#" + (64 + i - 1).ToString() + "#0:SUI#" + "F" + ":"; 
                        battle.BattledMoveData[0] += "&HED#" + (64 + i).ToString() + "#" + (64 + i - 1).ToString() + "#0:SUI#" + "1" + ":";
                    }
                    else if (battledPlayerCount > 0)
                    {
                        battle.BattleMoveData[0] += "&HED#" + (64 + i).ToString() + "#" + (64 + i - 1).ToString() + "#0:SUI#" + "1" + ":";
                        battle.BattledMoveData[1] += "&HED#" + (64 + i).ToString() + "#" + (64 + i - 1).ToString() + "#0:SUI#" + "F" + ":";
                    }
                }
                byte position = reader.ReadByte();
                byte unk4 = reader.ReadByte();
                switch(position)
                {
                    case 0x46:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "F,D0:";
                            battle.BattledMoveData[0] += "F,D0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "F,D0:";
                            battle.BattledMoveData[1] += "F,D0:";
                        }
                        break;
                    case 0x42:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "B,0:";
                            battle.BattledMoveData[0] += "B,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "B,0:";
                            battle.BattledMoveData[1] += "B,0:";
                        }
                        break;
                    case 0x4C:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "L,0:";
                            battle.BattledMoveData[0] += "L,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "L,0:";
                            battle.BattledMoveData[1] += "L,0:";
                        }
                        break;
                    case 0x52:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "R,0:";
                            battle.BattledMoveData[0] += "R,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "R,0:";
                            battle.BattledMoveData[1] += "R,0:";
                        }
                        break;
                    case 0x4E:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "N,0:";
                            battle.BattledMoveData[0] += "N,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "N,0:";
                            battle.BattledMoveData[1] += "N,0:";
                        }
                        break;
                }
            }
            /*if (moveStep == 0)
            {
                byte position = reader.ReadByte();
                byte unk4 = reader.ReadByte();
                switch (position)
                {
                    case 0x42:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "B,0:";
                            battle.BattledMoveData[0] += "B,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "B,0:";
                            battle.BattledMoveData[1] += "B,0:";
                        }
                        break;
                    case 0x4C:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "L,0:";
                            battle.BattledMoveData[0] += "L,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "L,0:";
                            battle.BattledMoveData[1] += "L,0:";
                        }
                        break;
                    case 0x52:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "R,0:";
                            battle.BattledMoveData[0] += "R,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "R,0:";
                            battle.BattledMoveData[1] += "R,0:";
                        }
                        break;
                    case 0x4E:
                        if (battlePlayerCount > 0)
                        {
                            battle.BattleMoveData[1] += "N,0:";
                            battle.BattledMoveData[0] += "N,0:";
                        }
                        else if (battledPlayerCount > 0)
                        {
                            battle.BattleMoveData[0] += "N,0:";
                            battle.BattledMoveData[1] += "N,0:";
                        }
                        break;
                }

                /*byte direction = reader.ReadByte();
                byte unk5 = reader.ReadByte();
                if (battlePlayerCount > 0)
                {
                    battle.BattleMoveData[1] += "D0:";
                    battle.BattledMoveData[0] += "D0:";
                }
                else if (battledPlayerCount > 0)
                {
                    battle.BattleMoveData[0] += "D0:";
                    battle.BattledMoveData[1] += "D0:";
                }
            }*/

            if (battlePlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battleLeader == User);
                battle.battleLeader.BattleReady = 1;
                if (battle.battledLeader.BattleReady == 1)
                {
                    battle.battleLeader.Connection.SendAsync(new GameRoom_BattleMove_0x41E(battle, 0));
                    battle.battledLeader.Connection.SendAsync(new GameRoom_BattleMove_0x41E(battle, 1));
                    Console.WriteLine("Move Data: {0}", battle.BattleMoveData[0]);
                    Console.WriteLine("Move Data1: {0}", battle.BattleMoveData[1]);
                }
            }
            if (battledPlayerCount > 0)
            {
                battle = Battle.BattleList.Find(p => p.battledLeader == User);
                battle.battledLeader.BattleReady = 1;
                if (battle.battleLeader.BattleReady == 1)
                {
                    battle.battleLeader.Connection.SendAsync(new GameRoom_BattleMove_0x41E(battle, 0));
                    battle.battledLeader.Connection.SendAsync(new GameRoom_BattleMove_0x41E(battle, 1));
                    Console.WriteLine("Move Data: {0}", battle.BattleMoveData[0]);
                    Console.WriteLine("Move Data1: {0}", battle.BattleMoveData[1]);
                }
            }
        }
        public static void Handle_TradeStart(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            Account tradedPlayer;
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
            int unk1 = reader.ReadLEInt32();
            ushort unk2 = reader.ReadLEUInt16();
            byte unk3 = reader.ReadByte();

            int tradeIDCount = 1;
            byte tradedID1 = reader.ReadByte();
            byte tradedID2 = 0x00;
            byte tradedID3 = 0x00;
            if(tradedID1 > 0x7F)
            {
                tradedID2 = reader.ReadByte();
                tradeIDCount++;
            }
            if(tradedID2 > 0x7F)
            {
                tradedID3 = reader.ReadByte();
                tradeIDCount++;
            }
            byte[] tradedID = new byte[tradeIDCount];
            for (int i = 0; i < tradeIDCount; i++)
            {
                if (i == 0)
                {
                    tradedID[i] = tradedID1;
                }
                else if (i == 1)
                {
                    tradedID[i] = tradedID2;
                }
                else if (i == 2)
                {
                    tradedID[i] = tradedID3;
                }
            }
            //decodedDynamicBytes(tradedID);
            tradedPlayer = room.Players.Find(p => p.GlobalID == decodedDynamicBytes(tradedID));

            TradeRecord trade = new TradeRecord
            {
                tradePlayer = User,
                tradedPlayer = tradedPlayer,
            };
            if (Trade.AddTrade(trade))
            {
                Client.SendAsync(new GameRoom_TradeStart(tradedPlayer));
                tradedPlayer.Connection.SendAsync(new GameRoom_TradeStart(User));
            }
        }
        public static void Handle_TradeCancel(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            int tradePlayerCount = Trade.TradeList.Count(p => p.tradePlayer == User);
            int tradedPlayerCount = Trade.TradeList.Count(p => p.tradedPlayer == User);
            TradeRecord trade = null;
            if (tradePlayerCount > 0)
            {
                trade = Trade.TradeList.Find(p => p.tradePlayer == User);
            }
            else if (tradedPlayerCount > 0)
            {
                trade = Trade.TradeList.Find(p => p.tradedPlayer == User);
            }
            Client.SendAsync(new GameRoom_TradeCancel());
            if (tradePlayerCount > 0)
            {
                trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeCancel());
            }
            else if(tradedPlayerCount > 0)
            {
                trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeCancel());
            }
            Trade.RemoveTrade(trade);
        }
        public static void Handle_TradeConfirm(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            int tradePlayerCount = Trade.TradeList.Count(p => p.tradePlayer == User);
            int tradedPlayerCount = Trade.TradeList.Count(p => p.tradedPlayer == User);
            TradeRecord trade = null;
            if (tradePlayerCount > 0)
            {
                trade = Trade.TradeList.Find(p => p.tradePlayer == User);
            }
            else if (tradedPlayerCount > 0)
            {
                trade = Trade.TradeList.Find(p => p.tradedPlayer == User);
            }

            if (tradePlayerCount > 0)
            {
                if (trade.tradeConfirm == 2 && trade.tradedConfirm == 2)
                {
                    trade.tradeLock = 1;
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeLock(true));
                    if (Trade.UpdateTrade(trade, 0, 1))
                    {
                        trade.tradeUpdate = 1;
                    }
                }
                else if (trade.tradeConfirm == 0)
                {
                    trade.tradeConfirm = 1;
                    if (trade.tradedConfirm > 0)
                    {
                        trade.tradeConfirm = 2;
                        trade.tradedConfirm = 2;
                    }
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeConfirm(true));
                }
                else if (trade.tradeConfirm == 1)
                {
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeConfirm(false));
                    trade.tradeConfirm = 0;
                    if (Trade.UpdateLock(trade, 0, 0))
                    {
                        trade.tradeUpdate = 0;
                    }
                }
            }
            else
            {
                if (trade.tradedConfirm == 2 && trade.tradeConfirm == 2)
                {
                    trade.tradedLock = 1;
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeLock(true));
                    if (Trade.UpdateTrade(trade, 1, 1))
                    {
                        trade.tradedUpdate = 1;
                    }
                }
                else if(trade.tradedConfirm == 0)
                {
                    trade.tradedConfirm = 1;
                    if (trade.tradeConfirm > 0)
                    {
                        trade.tradeConfirm = 2;
                        trade.tradedConfirm = 2;
                    }
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeConfirm(true));
                }
                else if(trade.tradedConfirm == 1)
                {
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeConfirm(false));
                    trade.tradedConfirm = 0;
                    if (Trade.UpdateLock(trade, 1, 0))
                    {
                        trade.tradedUpdate = 0;
                    }
                }
            }
            if (trade.tradeLock == 1 && trade.tradedLock == 1 && trade.tradeUpdate == 1 && trade.tradedUpdate == 1)
            {
                if (Trade.CompleteTrade(trade) == 0)
                {
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeSuccess());
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeSuccess());

                    trade.tradePlayer.Connection.SendAsync(new LoginRemoveMultiItem(trade.tradePlayer, trade.TradeItem));
                    trade.tradedPlayer.Connection.SendAsync(new LoginRemoveMultiItem(trade.tradedPlayer, trade.TradedItem));
                    trade.tradePlayer.UserItem.RemoveAll(i => trade.TradeItem.Contains(i));
                    trade.tradedPlayer.UserItem.RemoveAll(i => trade.TradedItem.Contains(i));
                    trade.tradePlayer.UserItem.AddRange(trade.TradeInsertedItem);
                    trade.tradedPlayer.UserItem.AddRange(trade.TradedInsertedItem);
                    trade.tradePlayer.Connection.SendAsync(new LoginAddMultiItem(trade.tradePlayer, trade.TradeInsertedItem));
                    trade.tradedPlayer.Connection.SendAsync(new LoginAddMultiItem(trade.tradedPlayer, trade.TradedInsertedItem));

                    trade.tradePlayer.CharacterZula1 = trade.tradePlayer.CharacterZula1 - trade.tradeZula + trade.tradedZula;
                    trade.tradedPlayer.CharacterZula1 = trade.tradedPlayer.CharacterZula1 - trade.tradedZula + trade.tradeZula;
                    trade.tradePlayer.Connection.SendAsync(new LoginCharParam_0x3A0(trade.tradePlayer));
                    trade.tradedPlayer.Connection.SendAsync(new LoginCharParam_0x3A0(trade.tradedPlayer));
                    Trade.RemoveTrade(trade);
                }
            }
            Console.WriteLine("(Confirm)Trade Lock: {0}, Traded Lock: {1}", trade.tradeLock, trade.tradedLock);
            Console.WriteLine("(Confirm)Trade Confirm: {0}, Traded Confirm: {1}", trade.tradeConfirm, trade.tradedConfirm);
            /*if (tradePlayerCount > 0)
            {
                if (Trade.UpdateTrade(trade, 0, 1))
                {
                    trade.tradedLock = 1;
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeConfirm(true));
                }
            }
            else
            {
                if (Trade.UpdateTrade(trade, 1, 1))
                {
                    trade.tradeLock = 1;
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeConfirm(true));
                }
            }
            if(trade.tradeLock == 1 && trade.tradedLock == 1)
            {
                if (Trade.CompleteTrade(trade) == 0)
                {
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeSuccess());
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeSuccess());

                    trade.tradePlayer.Connection.SendAsync(new LoginRemoveMultiItem(trade.tradePlayer, trade.TradeItem));
                    trade.tradedPlayer.Connection.SendAsync(new LoginRemoveMultiItem(trade.tradedPlayer, trade.TradedItem));
                    trade.tradePlayer.UserItem.RemoveAll(i => trade.TradeItem.Contains(i));
                    trade.tradedPlayer.UserItem.RemoveAll(i => trade.TradedItem.Contains(i));
                    trade.tradePlayer.UserItem.AddRange(trade.TradeInsertedItem);
                    trade.tradedPlayer.UserItem.AddRange(trade.TradedInsertedItem);
                    trade.tradePlayer.Connection.SendAsync(new LoginAddMultiItem(trade.tradePlayer, trade.TradeInsertedItem));
                    trade.tradedPlayer.Connection.SendAsync(new LoginAddMultiItem(trade.tradedPlayer, trade.TradedInsertedItem));

                    /*for(int i = 0; i < trade.tradePlayer.UserItem.Count; i++)
                    {
                        Console.WriteLine("Trade Player Item Count: {0}, Item Pos: {1}", trade.tradePlayer.UserItem.Count, trade.tradePlayer.UserItem[i].ItemPos);
                    }
                    for (int i = 0; i < trade.tradedPlayer.UserItem.Count; i++)
                    {
                        Console.WriteLine("Traded Player Item Count: {0}, Item Pos: {1}", trade.tradedPlayer.UserItem.Count, trade.tradedPlayer.UserItem[i].ItemPos);
                    }
                    Trade.RemoveTrade(trade);
                }
            }*/
        }
        public static void Handle_TradeLoadItem(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            List<ItemAttr> CurrentItem = new List<ItemAttr>();
            ushort unk1 = reader.ReadLEUInt16();
            byte unk2 = reader.ReadByte();
            int tradeItemCount = reader.ReadByte();
            //int unk3 = reader.ReadLEInt32();
            ushort unk3 = reader.ReadLEUInt16();
            byte unk4 = reader.ReadByte();
            byte tradeMoney1 = reader.ReadByte();
            byte[] tradeMoney = null;
            int Money = 0;
            int ItemPos = 0;

            tradeItemCount = tradeItemCount - 6;
            int tradePlayerCount = Trade.TradeList.Count(p => p.tradePlayer == User);
            int tradedPlayerCount = Trade.TradeList.Count(p => p.tradedPlayer == User);
            TradeRecord trade = null;
            if (tradePlayerCount > 0)
            {
                trade = Trade.TradeList.Find(p => p.tradePlayer == User);
            }
            else if (tradedPlayerCount > 0)
            {
                trade = Trade.TradeList.Find(p => p.tradedPlayer == User);
            }

            if (tradeMoney1 == 0x00)
            {
                for (int i = 0; i < tradeItemCount; i++)
                {
                    int TradeItemPos = reader.ReadByte();
                    ItemAttr item = User.UserItem.Find(w => w.ItemPos == TradeItemPos);
                    CurrentItem.Add(item);
                }
            }
            else
            {
                int tradeMoneyCount = 1;
                byte tradeMoney2 = 0x00;
                byte tradeMoney3 = 0x00;
                byte tradeMoney4 = 0x00;
                byte tradeMoney5 = 0x00;
                if (tradeMoney1 > 0x7F)
                {
                    tradeMoney2 = reader.ReadByte();
                    tradeMoneyCount++;
                }
                if (tradeMoney2 > 0x7F)
                {
                    tradeMoney3 = reader.ReadByte();
                    tradeMoneyCount++;
                }
                if (tradeMoney3 > 0x7F)
                {
                    tradeMoney4 = reader.ReadByte();
                    tradeMoneyCount++;
                }
                if (tradeMoney4 > 0x7F)
                {
                    tradeMoney5 = reader.ReadByte();
                    tradeMoneyCount++;
                }
                tradeMoney = new byte[tradeMoneyCount];
                for (int i = 0; i < tradeMoneyCount; i++)
                {
                    if (i == 0)
                    {
                        tradeMoney[i] = tradeMoney1;
                    }
                    else if (i == 1)
                    {
                        tradeMoney[i] = tradeMoney2;
                    }
                    else if (i == 2)
                    {
                        tradeMoney[i] = tradeMoney3;
                    }
                    else if (i == 3)
                    {
                        tradeMoney[i] = tradeMoney4;
                    }
                    else if (i == 4)
                    {
                        tradeMoney[i] = tradeMoney5;
                    }
                }
                Money = decodedDynamicBytes(tradeMoney);
                if(Money > 268435455)
                {
                    tradeItemCount = tradeItemCount - 4;
                }
                else if (Money > 2097151)
                {
                    tradeItemCount = tradeItemCount - 3;
                }
                else if (Money > 16383)
                {
                    tradeItemCount = tradeItemCount - 2;
                }
                else if (Money > 127)
                {
                    tradeItemCount = tradeItemCount - 1;
                }
                
                ItemPos = reader.ReadByte();
                if (ItemPos != 0x8F)
                {
                    int TradeItemPos;
                    for (int i = 0; i < tradeItemCount; i++)
                    {
                        if (i == 0)
                        {
                            TradeItemPos = ItemPos;
                        }
                        else
                        {
                            TradeItemPos = reader.ReadByte();
                        }
                        ItemAttr item = User.UserItem.Find(w => w.ItemPos == TradeItemPos);
                        CurrentItem.Add(item);
                        //Console.WriteLine("Current Item Pos: {0}", item.ItemPos);
                    }
                }
            }
            
            if (tradePlayerCount > 0)
            {
                if (trade.tradeLock == 0) //self lock = 0
                {
                    trade.tradeConfirm = 0; //reset self status
                    if (tradeMoney1 == 0x00)
                    {
                        tradeMoney = new byte[1];
                        tradeMoney[0] = 0x00;
                    }
                    else
                    {
                        trade.tradeZula = Money;
                    }
                    trade.TradeItem = CurrentItem;
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeItem(trade.TradeItem, tradeMoney));
                    trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeConfirm(false)); //reset self status

                    if (trade.tradedLock == 1) // other lock = 1
                    {
                        if (Trade.UpdateLock(trade, 1, 0))
                        {
                            trade.tradedConfirm = 0; //reset other status
                            trade.tradeLock = 0;
                            trade.tradedLock = 0; //unlock all traders
                            trade.tradeUpdate = 0;
                            trade.tradedUpdate = 0;
                            trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeConfirm(false)); //reset other status
                            trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeLock(false));
                        }
                    }
                }
            }
            else
            {
                if (trade.tradedLock == 0)
                {
                    trade.tradedConfirm = 0;
                    if (tradeMoney1 == 0x00)
                    {
                        tradeMoney = new byte[1];
                        tradeMoney[0] = 0x00;
                    }
                    else
                    {
                        trade.tradedZula = Money;
                    }
                    trade.TradedItem = CurrentItem;
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeItem(trade.TradedItem, tradeMoney));
                    trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeConfirm(false));

                    if (trade.tradeLock == 1)
                    {
                        if (Trade.UpdateLock(trade, 0, 0))
                        {
                            trade.tradeConfirm = 0;
                            trade.tradeLock = 0;
                            trade.tradedLock = 0;
                            trade.tradeUpdate = 0;
                            trade.tradedUpdate = 0;
                            trade.tradedPlayer.Connection.SendAsync(new GameRoom_TradeConfirm(false));
                            trade.tradePlayer.Connection.SendAsync(new GameRoom_TradeLock(false));
                        }
                    }
                }
            }
            Console.WriteLine("(Load)Trade Lock: {0}, Traded Lock: {1}", trade.tradeLock, trade.tradedLock);
        }
        public static void Handle_WearItem(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
            ushort unk1 = reader.ReadLEUInt16();
            byte unk2 = reader.ReadByte();
            int itemPos = reader.ReadByte();
            int isWear = reader.ReadByte();

            ItemAttr WearDuplicateItem;
            bool WearItemOK = false;
            int WearFull = 0;
            ItemAttr WearItem = User.UserItem.Find(i => i.ItemPos == itemPos);

            if (WearItem.ItemTypeNum == 2)
            {
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 3) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 3);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                    }
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 4) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 4);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                    }
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 5) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 5);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                    }
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 6) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 6);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                    }
                }
            }
            else if (WearItem.ItemTypeNum == 3)
            {
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 2) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 2);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                        WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 1);
                        WearFull = 1;
                    }
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 4) > 0) 
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 4);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                        //WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 2);
                        //WearFull = 1;
                    }
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 5) > 0) 
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 5);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                        //WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 2);
                        //WearFull = 1;
                    }
                }
            }
            else if (WearItem.ItemTypeNum == 4 || WearItem.ItemTypeNum == 5)
            {
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 3) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 3);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                        WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 1);
                        WearFull = 1;
                    }
                }
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 2) > 0) 
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 2);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                        WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 2); 
                        WearFull = 1;
                    }
                }
            }
            else if (WearItem.ItemTypeNum == 6)
            {
                if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == 2) > 0)
                {
                    WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == 2);
                    bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                    if (WearDuplicateItemOK)
                    {
                        WearDuplicateItem.ItemWear = 0;
                        Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                        WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 1);
                        WearFull = 1;
                    }
                }
            }
            if (User.UserItem.Count(i => i.ItemWear == 1 && i.ItemTypeNum == WearItem.ItemTypeNum) > 0)
            {
                WearDuplicateItem = User.UserItem.Find(i => i.ItemWear == 1 && i.ItemTypeNum == WearItem.ItemTypeNum);
                bool WearDuplicateItemOK = wearEquipmentItem(User, WearDuplicateItem, 0, 1, 0);

                if (WearDuplicateItemOK)
                {
                    WearDuplicateItem.ItemWear = 0;
                    Client.SendAsync(new LoginMoveItem(User, WearDuplicateItem, 0));
                }
            }
            if (WearFull == 0)
            {
                WearItemOK = wearEquipmentItem(User, WearItem, isWear, 0, 0);
            }

            if (WearItemOK)
            {
                WearItem.ItemWear = isWear;
                Client.SendAsync(new LoginMoveItem(User, WearItem, isWear));
                User.GameAct = 0;
                room.BroadcastToAll(new LoginCharAppear_0x3E8_00(User));
            }
        }
        public static void Handle_ActToAll(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
            ushort unk1 = reader.ReadLEUInt16();
            byte action = reader.ReadByte();

            User.GameAct = action;
            Console.WriteLine("{0} act {1}", User.CharacterNickname1, User.GameAct);
            room.BroadcastToAllExclude(new LoginCharAppear_0x3E8_00(User), User);
        }
        public static void Handle_ChatToAll(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
            ushort unk1 = reader.ReadLEUInt16();
            byte fontColor= reader.ReadByte();
            byte fontSize = reader.ReadByte();
            ushort unk2 = reader.ReadLEUInt16();
            int unk3 = reader.ReadLEInt32();
            byte wordlen = reader.ReadByte();
            string word = reader.ReadUTF8StringSafe(wordlen);

            Console.WriteLine("{0} say {1}", User.CharacterNickname1, word);
            room.BroadcastToAll(new GameRoom_ChatToAll(User, fontColor, fontSize, wordlen, word));
        }
        public static void Handle_SendPlayerPosition(ClientConnection Client, PacketReader reader)
        {
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
            ushort unk1 = reader.ReadLEUInt16();
            //User.GamePosX = reader.ReadLEUInt16();
            User.GamePosX = reader.ReadByte();
            byte unk2 = reader.ReadByte();
            //User.GamePosY = reader.ReadLEUInt16();
            User.GamePosY = reader.ReadByte();
            byte unk3 = reader.ReadByte();
            byte unk4 = reader.ReadByte();
            User.GameDirection = reader.ReadByte();
            User.GameDirection2 = reader.ReadByte();

            User.GameAct = 0;
            room.BroadcastToAllExclude(new GameRoom_PlayerPosition(User), User);
            if(teleportMap(User))
            {
                LoginHandle.getMapInfo(User);
                LoginHandle.getDecodedMapID(User);
                Client.SendAsync(new LoginMapInfo_0x1EC(User));
                Client.SendAsync(new LoginMapMusicInfo_0x31B(User));

                room.Players.Remove(User);
                if (Rooms.ExistRoom(User.UserMap.MapGlobalID))
                {
                    room = Rooms.GetRoom(User.UserMap.MapGlobalID);
                    room.EnterRoom(Client, "", 0x00);
                }
                else
                {
                    room.setID(User.UserMap.MapGlobalID);
                    Rooms.AddRoom(room.ID, room);

                    room.EnterRoom(Client, "", 0x00);
                }
            }
            Console.WriteLine("X: {0}, Y: {1}, Direction: {2}", User.GamePosX, User.GamePosY, User.GameDirection);
        }
        public static void LeaveRoom(Account User)
        {
            NormalRoom room = Rooms.GetRoom(User.UserMap.MapGlobalID);
            room.Players.Remove(User);
            room.BroadcastToAll(new LoginCharDisappear_0x3E8_04(User));
        }
        private static bool teleportMap(Account User)
        {
            if(User.UserMap.TeleportList == null)
            {
                return false;
            }
            else if (User.UserMap.TeleportList.Count(p => p.MapX == User.GamePosX && p.MapY == User.GamePosY && p.MapGlobalID == User.UserMap.MapGlobalID) > 0)
            {
                TeleportRecord teleport = User.UserMap.TeleportList.Find(p => p.MapX == User.GamePosX && p.MapY == User.GamePosY && p.MapGlobalID == User.UserMap.MapGlobalID);
                if (User.UserType == 1 || (User.UserType == 0 && teleport.MapTeleportID < 2000))
                {
                    User.UserMap.MapGlobalID = teleport.MapTeleportID;
                    User.GamePosX = (byte)teleport.MapInitX;
                    User.GamePosY = (byte)teleport.MapInitY;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private static bool wearEquipmentItem(Account User, ItemAttr UserItem, int isWear, int isCloth, int isExistDress)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_wearEquipmentItem";
                    cmd.Parameters.Add("globalid", MySqlDbType.Int32).Value = User.GlobalID;
                    cmd.Parameters.Add("itemid", MySqlDbType.Int32).Value = UserItem.ItemDecodedID;
                    cmd.Parameters.Add("globalitemid", MySqlDbType.Int32).Value = UserItem.ItemGlobalID;
                    cmd.Parameters.Add("isWear", MySqlDbType.Int32).Value = isWear;
                    
                    MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                    reader.Read();
                    string result = reader["result"].ToString();
                    cmd.Dispose();
                    reader.Close();
                    con.Close();
                    if (result == "1" && isCloth == 0)
                    {
                        int RemainGender = 0;
                        if (User.Gender == 1)
                        {
                            RemainGender = 65536;
                        }
                        if (isWear == 0)
                        {
                            if (UserItem.ItemTypeNum == 2)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    User.CharacterOneEquipment[i + 2] = (User.CharacterOneRawEquipment + i);
                                }
                            }
                            else if (UserItem.ItemTypeNum == 3)
                            {
                                User.CharacterOneEquipment[2] = (User.CharacterOneRawEquipment);
                                User.CharacterOneEquipment[5] = (User.CharacterOneRawEquipment + 3);
                            }
                            else if (UserItem.ItemTypeNum == 4)
                            {
                                User.CharacterOneEquipment[2] = (User.CharacterOneRawEquipment);
                            }
                            else if (UserItem.ItemTypeNum == 5)
                            {
                                User.CharacterOneEquipment[5] = (User.CharacterOneRawEquipment + 3);
                            }
                            else if (UserItem.ItemTypeNum == 6)
                            {
                                User.CharacterOneEquipment[3] = (User.CharacterOneRawEquipment + 1);
                                User.CharacterOneEquipment[4] = (User.CharacterOneRawEquipment + 2);
                            }
                            else if (UserItem.ItemTypeNum == 7)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    User.CharacterOneEquipment[i + 6] = (User.CharacterOneRawEquipment + i + 4);
                                }
                            }
                        }
                        else
                        {
                            if (UserItem.ItemTypeNum == 2)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    User.CharacterOneEquipment[i + 2] = (UserItem.ItemAppearance + i + RemainGender);
                                }
                            }
                            else if (UserItem.ItemTypeNum == 3)
                            {
                                User.CharacterOneEquipment[2] = (UserItem.ItemAppearance + RemainGender);
                                if (isExistDress == 1)
                                {
                                    User.CharacterOneEquipment[3] = (User.CharacterOneRawEquipment + 1);
                                    User.CharacterOneEquipment[4] = (User.CharacterOneRawEquipment + 2);
                                }
                                User.CharacterOneEquipment[5] = (UserItem.ItemAppearance + 3 + RemainGender);
                            }

                            else if (UserItem.ItemTypeNum == 4)
                            {
                                User.CharacterOneEquipment[2] = (UserItem.ItemAppearance + RemainGender);
                                if (isExistDress == 1)
                                {
                                    User.CharacterOneEquipment[5] = (User.CharacterOneRawEquipment + 3);
                                }
                                else if (isExistDress == 2)
                                {
                                    User.CharacterOneEquipment[3] = (User.CharacterOneRawEquipment + 1);
                                    User.CharacterOneEquipment[4] = (User.CharacterOneRawEquipment + 2);
                                    User.CharacterOneEquipment[5] = (User.CharacterOneRawEquipment + 3);
                                }
                            }
                            else if (UserItem.ItemTypeNum == 5)
                            {
                                if (isExistDress == 1)
                                {
                                    User.CharacterOneEquipment[2] = (User.CharacterOneRawEquipment);
                                }
                                else if (isExistDress == 2)
                                {
                                    User.CharacterOneEquipment[2] = (User.CharacterOneRawEquipment);
                                    User.CharacterOneEquipment[3] = (User.CharacterOneRawEquipment + 1);
                                    User.CharacterOneEquipment[4] = (User.CharacterOneRawEquipment + 2);
                                }
                                User.CharacterOneEquipment[5] = (UserItem.ItemAppearance + RemainGender);
                            }
                            if (UserItem.ItemTypeNum == 6)
                            {
                                if (isExistDress == 1)
                                {
                                    User.CharacterOneEquipment[2] = (User.CharacterOneRawEquipment);
                                    User.CharacterOneEquipment[5] = (User.CharacterOneRawEquipment + 3);
                                }
                                User.CharacterOneEquipment[3] = (UserItem.ItemAppearance + RemainGender);
                                User.CharacterOneEquipment[4] = (UserItem.ItemAppearance + 1 + RemainGender);
                            }
                            if (UserItem.ItemTypeNum == 7)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    User.CharacterOneEquipment[i + 6] = (UserItem.ItemAppearance + i + RemainGender);
                                }
                            }
                        }
                        return true;
                    }
                    else if(result == "1" && isCloth == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public static void Handle_CreateGameRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            /*
            Account User = Client.CurrentAccount;
            if (!User.InGame)
            {
                if (User.Attribute == 3)
                {
                    Client.SendAsync(new GameRoom_CreateRoomError(User, 8, last));
                    return;
                }
                byte roomkindid = reader.ReadByte();
                byte maxcount = reader.ReadByte();
                int namelength = reader.ReadLEInt32();
                string name = reader.ReadBig5StringSafe(namelength);
                int passwordlength = reader.ReadLEInt32();
                string password = string.Empty;
                if (passwordlength > 0)
                    password = reader.ReadBig5StringSafe(passwordlength);
                int isTeamPlay = reader.ReadLEInt32();
                bool isStepOn = reader.ReadBoolean();
                int itemtype = reader.ReadLEInt32();
                int unk = reader.ReadLEInt32();
                int mapnum = reader.ReadLEInt32();
                //byte footer = binaryReader.ReadByte();

                NormalRoom room = new NormalRoom();
                room.setID(Rooms.RoomID);
                room.setName(name);
                room.setPassword(password);
                room.setItemType(itemtype);
                room.setIsStepOn(isStepOn);
                room.setRoomKindID(roomkindid);
                room.setIsTeamPlay(isTeamPlay);
                if (mapnum != 0)
                    room.setMapNum(mapnum);
                room.Players.Add(User);

                Console.WriteLine("fdRoomKindID={0}", roomkindid);
                if (RoomHolder.RoomKindInfos.TryGetValue(roomkindid, out var roomkindinfo))
                    room.setGameMode(roomkindinfo);
                else
                    return;

                if (roomkindinfo.Channel == 1)
                {
                    if (User.Exp > ServerSettingHolder.ServerSettings.NewbieOnlyChannelLimitExp && User.Attribute == 0)
                    {
                        Client.SendAsync(new GameRoom_CreateRoomError(User, 5, last));
                        return;
                    }
                }

                //取得第一個位置 0
                User.RoomPos = (byte)(User.Attribute == 3 ? 100 : room.PosList.First());
                room.PosList.Remove(User.RoomPos);
                room.setRoomMasterIndex(User.RoomPos);


                //User.IsRoomMaster = true;
                User.InGame = true;
                User.CurrentRoomId = Rooms.RoomID;
                Rooms.RoomID += 1;

                Log.Info("Room name: {0}, isTeamPlay={1}, isStepOn={2}, ItemType={3}, HasPassword={4}, RoomKindID={5}", name, isTeamPlay, isStepOn, itemtype, passwordlength > 0 ? true : false, roomkindid);

                //Client.SendAsync(new GameRoom_Hex(User, "FF5805FFFFFFFFFFFFFFFF000000000000000D01", last));
                Client.SendAsync(new GameRoom_GoodsInfo(room, last));
                Client.SendAsync(new GameRoom_Hex("A3", last));

                Client.SendAsync(new GameRoom_SendRoomInfo(room, last));
                Client.SendAsync(new GameRoom_SendPlayerInfo(User, last));
                Client.SendAsync(new GameRoom_GetRoomMaster(room.RoomMasterIndex, last)); //場主位置

                //Client.SendAsync(new GameRoom_Hex(User, "FFB9010100000000", last));
                Client.SendAsync(new GameRoom_SendRoomMaster(User, room.MapNum, room.RoomMasterIndex, last));

                Client.SendAsync(new GameRoom_Hex("FF6405F70300000000000000000000", last));
                Client.SendAsync(new GameRoom_Hex("FF660501000000", last));

                if (isTeamPlay == 2)
                {
                    User.Team = 1;
                    Client.SendAsync(new GameRoom_RoomPosTeam(User, last));
                }

                if (room.GameMode == 3)
                {
                    User.SelectRelayTeam(9);
                    Client.SendAsync(new GameRoom_RoomPosRelayTeam(User, last));
                }

                room.StartAutoChangeRoomMaster();

                //Rooms.NormalRoomList.Add(room);
                Rooms.AddRoom(room.ID, room);
            }
            */
        }

        public static void Handle_LeaveRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            //Account User = Client.CurrentAccount;
            GameRoomEvent.LeaveRoom(Client, false, last);
        }

        public static void Handle_RoomControl(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            reader.Offset += 1; //FF
            short subopcode = reader.ReadLEInt16();
            switch (subopcode)
            {
                case 0x300: //開關欄位
                    RoomServerHandle.Handle_SlotControl(Client, reader, last);
                    break;
                case 0x2D8: //更改設定
                    RoomServerHandle.Handle_ChangeSetting(Client, reader, last);
                    break;
                case 0x2E6: //準備/取消準備
                    RoomServerHandle.Handle_Ready(Client, reader, last);
                    break;
                case 0x2E9: //轉地圖
                    RoomServerHandle.Handle_ChangeMap(Client, reader, last);
                    break;
                case 0x306: //Start game
                    RoomServerHandle.Handle_StartGame(Client, reader, last);
                    break;
                case 0x2C9:
                    RoomServerHandle.Handle_ChangeStatus(Client, reader, last);
                    break;
                case 0x30A:
                    RoomServerHandle.Handle_StartLoading(Client, reader, last);
                    break;
                case 0x310:
                    RoomServerHandle.Handle_EndLoading(Client, reader, last);
                    break;
                case 0x313:
                    RoomServerHandle.Handle_GameStart(Client, reader, last);
                    break;
                case 0x352:
                    RoomServerHandle.Handle_GoalInData(Client, reader, last);
                    break;
                case 0x37D:
                    Console.WriteLine("time over?alive?");
                    break;
                case 0x2DE:
                    RoomServerHandle.Handle_RoomChat(Client, reader, last);
                    break;
                case 0x2CA:
                    RoomServerHandle.Handle_MapControl(Client, reader, last);
                    break;
                case 0x2EB:
                    //click change map gui btn
                    break;
                case 0x315: //start lap countdown
                    GameModeHandle.GameMode_LapTimeCountdwon(Client, reader, last);
                    break;
                case 0x35C://天使神殿·外傳開門
                    //Log.Debug("Map event: {0}", Utility.ByteArrayToString(reader.Buffer));
                    RoomServerHandle.Handle_TriggerMapEvent(Client, reader, last);
                    break;
                case 0x2FE://四心踩制
                    RoomServerHandle.Handle_StepOnButton(Client, reader, last);
                    break;
                case 0x556://登陸商品
                    RoomServerHandle.Handle_RegisterItem(Client, reader, last);
                    break;
                case 0x559:
                    GameModeHandle.GameMode_MiniGame_Respawn(Client, reader, last);
                    break;
                case 0x55B:
                    GameModeHandle.GameMode_MiniGame_GetPoint(Client, reader, last);
                    break;
                case 0x55E:
                    GameModeHandle.GameMode_MiniGame_RoundTime(Client, reader, last);
                    break;
                case 0x33D:
                    GameModeHandle.GameMode_GameOver(Client, reader, last);
                    break;
                case 0x34E:
                    GameModeHandle.GameMode_FootStep_GoalIn(Client, reader, last);
                    break;
                case 0x2C1:
                    GameModeHandle.GameMode_Amsan_LapTime(Client, reader, last);
                    break;
                case 0x339:
                    GameModeHandle.GameMode_Amsan_StepButton(Client, reader, last);
                    break;
                case 0x33B:
                    GameModeHandle.GameMode_Amsan_StepButton_Push(Client, reader, last);
                    break;
                case 0x37F:
                    GameModeHandle.GameMode_Amsan_FinalButton(Client, reader, last);
                    break;
                case 0x34C:
                    GameModeHandle.GameMode_Amsan_LapTimeControl(Client, reader, last);
                    break;
                case 0x343:
                    GameModeHandle.GameMode_RandomGameOver(Client, reader, last);
                    break;
                case 0x345:
                    GameModeHandle.GameMode_RandomGameOver_Die(Client, reader, last);
                    break;


                case 0x320://放棄之前抽到的道具（重抽道具用）
                    RoomServerHandle.Handle_GiveUpItem(Client, reader, last);
                    break;
                case 0x31A://抽道具
                    RoomServerHandle.Handle_DrawItem(Client, reader, last);
                    break;
                case 0x31D://使用道具
                    RoomServerHandle.Handle_UseItem(Client, reader, last);
                    break;
                case 0x326:
                    RoomServerHandle.Handle_RegItem2(Client, reader, last);
                    break;
                case 0x327://使用道具
                    RoomServerHandle.Handle_RegItem(Client, reader, last);
                    break;

                case 0x2EC://change team
                    RoomServerHandle.Handle_ChangeTeam(Client, reader, last);
                    break;

                case 0x37E:
                    //倒數
                    //FF C0 02 FF 7E 03 00 00 00 00 00 00 00 00 80
                    break;

                case 0x484:
                    RoomServerHandle.Handle_jw_Loading(Client, last);
                    break;

                //接力
                case 0x2EF:
                    RoomServerHandle.Handle_ChangeRelayTeam(Client, reader, last);
                    break;
                case 0x2F2:
                    RoomServerHandle.Handle_RandomChooseRelayTeam(Client, last);
                    break;
                case 0x2F3:
                    RoomServerHandle.Handle_ChangeSlotStateRelay(Client, reader, last);
                    break;
                /*case 0x2CE:
                    RoomServerHandle.Handle_PreparePassBaton(Client, reader, last);
                    break;*/
                case 0x2F6:
                    RoomServerHandle.Handle_WaitPassBaton(Client, last);
                    break;
                case 0x2F8:
                    RoomServerHandle.Handle_WaitPassBaton2(Client, last);
                    break;
                case 0x2FA:
                    RoomServerHandle.Handle_StartPassBaton(Client, reader, last);
                    break;
                case 0x2FC:
                    RoomServerHandle.Handle_PassBaton(Client, reader, last);
                    break;
                case 0x329:
                    GameModeHandle.GameMode_CatchFish(Client, reader, last);
                    break;
                case 0x354:
                    GameModeHandle.RunQuizMode_RequestQuizList(Client, last);
                    break;

                case 0x5E8:
                    GameModeHandle.GameMode_TurtleEatItem(Client, reader, last);
                    break;
                case 0x5E9:
                    GameModeHandle.GameMode_ReqChangeTeamLeader(Client, last);
                    break;
                /*
            case 0x5F3:
                GameModeHandle.GameMode_ChangeTeamLeader(Client, reader, last);
                break;*/

                case 0x349:
                    //unknown
                    break;
                case 0x35E:
                    GameModeHandle.CorunMode_TriggerObjectEvent(Client, reader, last);
                    break;
                case 0x360:
                    GameModeHandle.CorunMode_TriggerCheckInObjectEvent(Client, reader, last);
                    break;
                case 0x364:
                    GameModeHandle.CorunMode_SetClearLimitTime(Client, reader, last);
                    break;
                case 0x366:
                    GameModeHandle.CorunMode_EnterTimeSection(Client, reader, last);
                    break;
                case 0x368:
                    GameModeHandle.CorunMode_ClearTimeSection(Client, reader, last);
                    break;
                case 0x36A:
                    GameModeHandle.CorunMode_SetBossEnergy(Client, reader, last);
                    break;
                case 0x36D:
                    GameModeHandle.CorunMode_DecreaseBossEnergy(Client, reader, last);
                    break;
                case 0x36F:
                    GameModeHandle.CorunMode_SetObjectBossEnergy(Client, reader, last);
                    break;
                case 0x372:
                    GameModeHandle.CorunMode_DecreaseObjectBossEnergy(Client, reader, last);
                    break;
                case 0x374:
                    GameModeHandle.CorunMode_IncreaseObjectBossEnergy(Client, reader, last);
                    break;

                case 0x475:
                    GameModeHandle.Anubis_GetPlayerHP(Client, last);
                    break;
                case 0x472:
                    GameModeHandle.Anubis_SetObjectBoss(Client, reader, last);
                    break;
                case 0x47C:
                    GameModeHandle.Anubis_DecreaseObjectBossHP(Client, reader, last);
                    break;
                case 0x477:
                    GameModeHandle.Anubis_DecreaseMyHP(Client, reader, last);
                    break;
                case 0x48D:
                    GameModeHandle.Anubis_Rebirth(Client, reader, last);
                    break;
                default:
                    Log.Info("Unhandle room opcode: {0}", Utility.ByteArrayToString(reader.Buffer));
                    break;
            }
        }

        public static void Handle_GameEndInfo(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            if (User.GameEndType == 0)
            {
                NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
                User.RaceDistance = reader.ReadLESingle();
                room.MapMaxDistance = reader.ReadLESingle();
                User.GameEndType = reader.ReadLEInt16();
                long CurrentTime = Utility.CurrentTimeMilliseconds();
                //GameEndType 1 = goal
                //GameEndType 2 = alive
                //GameEndType 3 = timeover
                //GameEndType 4 = gameover
                //GameEndType 5 = footstep goalin?
                Log.Debug("Game End - Nickname: {0}, GameEndType: {1}, RaceDistance: {2}", User.NickName, User.GameEndType, User.RaceDistance);
                if (User.GameEndType == 1)
                {
                    //User.LapTime = room.GetCurrentTime();// (int)(CurrentTime - room.StartTime);
                    //User.ServerLapTime = room.GetCurrentTime();
                    if (room.GameMode == 5)
                    {
                        foreach (var p in room.Players.OrderBy(o => o.RoomPos))
                        {
                            p.GameEndType = 1;
                        }
                    }

                }
                else if (User.GameEndType == 2)
                {
                    User.LapTime = room.GetCurrentTime();
                    User.ServerLapTime = User.LapTime;
                    //User.Rank = room.Rank++;
                    //Task calctask = Task.Run(() => GameRoomEvent.Calc_DropItem(User, room, User.Rank, last));
                    //long EndTime = CurrentTime + 5000;
                    //Task.Run(() => GameRoomEvent.Execute_GameEnd(room, EndTime, last));
                }
                else if (User.GameEndType == 3)
                {
                    User.LapTime = room.GetCurrentTime() + 200000;
                    User.ServerLapTime = User.LapTime;
                    /*if (room.GameMode == 3)
                    {
                        int i = 1;
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum))
                        {
                            teamplayer.LapTime = User.LapTime + i;
                            teamplayer.ServerLapTime = User.ServerLapTime + i;
                            teamplayer.GameEndType = User.GameEndType;
                            i++;
                        }
                    }*/
                }
                else if (User.GameEndType == 4)
                {
                    User.LapTime = room.GetCurrentTime() + 90000000;
                    User.ServerLapTime = User.LapTime;
                    /*if (room.GameMode == 3)
                    {
                        int i = 1;
                        foreach (var teamplayer in room.Players.Where(w => w.RelayTeam == User.RelayTeam && w.UserNum != User.UserNum))
                        {
                            teamplayer.LapTime = User.LapTime + i;
                            teamplayer.ServerLapTime = User.ServerLapTime + i;
                            teamplayer.GameEndType = User.GameEndType;
                            i++;
                        }
                    }*/
                }
                /*else if (User.GameEndType == 5)
                {
                    User.LapTime = room.GetCurrentTime();
                    User.ServerLapTime = User.LapTime;
                }*/
            }
        }

        public static void Handle_PlayerList(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF 6C 01 00 00 00 00 04
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            //Console.WriteLine("start play 0x16c"); unk
            //Client.SendAsync(new GameRoom_Hex("FFC5020200000000000000000100000000", last));
            //Client.SendAsync(new GameRoom_Hex("FF270500000000", last));
            Client.SendAsync(new GameRoom_PlayerPosList(room.Players, last));
        }

        public static void Handle_GetRoomList(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            byte roomkindid = reader.ReadByte();
            int unk1 = reader.ReadLEInt32();
            int page = reader.ReadLEInt32();
            byte unk2 = reader.ReadByte();

            List<NormalRoom> rooms = Rooms.RoomList.Values.Where(room => room.RoomKindID == roomkindid).ToList();
            //Rooms.NormalRoomList.FindAll(room => room.RoomKindID == roomkindid);

            Client.SendAsync(new GameRoom_GetRoomList(User, rooms, roomkindid, page, last));


        }

        public static void Handle_EnterRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int roomid = reader.ReadLEInt32();
            int pwlen = reader.ReadLEInt32();
            string pw = string.Empty;
            if (pwlen != 0)
                pw = reader.ReadBig5StringSafe(pwlen);
            if (User.CurrentRoomId != 0)
            {
                //GameRoomEvent.DisconnectRoom(User);
                GameRoomEvent.LeaveRoom(Client, false, last);
            }
            bool isExist = Rooms.ExistRoom(roomid);
            if (isExist)
            {
                NormalRoom room = Rooms.GetRoom(roomid);
                room.EnterRoom(Client, pw, last);
                //GameRoomEvent.EnterRoom(Client, room, pw, last);
            }
            else
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 0x1, 0x9B, last));
            }
        }

        public static void Handle_KickPlayer(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            int nicknamelen = reader.ReadLEInt32();
            string nickname = reader.ReadBig5StringSafe(nicknamelen);
            NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
            bool isExist = room.Players.Exists(p => p.NickName == nickname);
            if (isExist && User.RoomPos == room.RoomMasterIndex)
            {
                Account KickedPlayer = room.Players.Find(p => p.NickName == nickname);
                byte KickedPlayerIndex = KickedPlayer.RoomPos;
                if (room.RoomMasterIndex == User.RoomPos && room.RoomKindID != 0x4A && KickedPlayer.Attribute == 0 && !room.isPlaying)//room.RoomKindID != 0x4A <-----防公園踢人外掛, KickedPlayer.Attribute == 0 <-----只能踢普通玩家
                {
                    GameRoomEvent.KickPlayer(KickedPlayer, room, last);
                }
                if (User.Attribute != 0)
                {
                    GameRoomEvent.KickPlayer(KickedPlayer, room, last);
                }
            }
        }

        public static void Handle_FF9704(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            Client.SendAsync(new GameRoom_Hex("FF98044E00", last));
        }

        public static void Handle_RandomEnterRoom(ClientConnection Client, PacketReader reader, byte last)
        {
            Account User = Client.CurrentAccount;
            byte roomkindid = reader.ReadByte();
            //int empty = reader.ReadLEInt32();
            bool isExist = Rooms.RoomList.Values.Any(rm => rm.RoomKindID == roomkindid && rm.Players.Count < rm.SlotCount
                                    && !rm.isPlaying && !rm.HasPassword && !rm.Players.Exists(p => p.Attribute == 3));
            if (!isExist)
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 0xB, roomkindid, last));
            }
            else
            {
                NormalRoom room = Rooms.RoomList.Values.Where(rm => rm.RoomKindID == roomkindid && rm.Players.Count < rm.SlotCount
                                    && !rm.isPlaying && !rm.HasPassword && !rm.Players.Exists(p => p.Attribute == 3))
                                    .OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                room.EnterRoom(Client, string.Empty, last);
                //GameRoomEvent.EnterRoom(Client, room, string.Empty, last);
            }
        }

        public static void Handle_PlayTogether(ClientConnection Client, PacketReader packet, byte last)
        {
            Account User = Client.CurrentAccount;
            int unk = packet.ReadLEInt32();
            int roomid = packet.ReadLEInt32();
            byte roomkindid = packet.ReadByte();
            int pwlen = packet.ReadLEInt32();
            string pw = string.Empty;
            if (pwlen > 0)
                pw = packet.ReadBig5StringSafe(pwlen);
            bool isExist = Rooms.ExistRoom(roomid);
            if (!isExist)
            {
                Client.SendAsync(new GameRoom_EnterRoomError(User, 0x1, roomkindid, last));
            }
            else
            {
                NormalRoom room = Rooms.GetRoom(roomid);
                room.EnterRoom(Client, pw, last);
                //GameRoomEvent.EnterRoom(Client, room, pw, last);
            }
        }

        public static void Handle_FFF801(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF F8 01 09 00 00 00 47 0E 00 00 01 00 00 00 B7 2F 00 00 01 00 00 00 10
            Account User = Client.CurrentAccount;
            int unk1 = reader.ReadLEInt32();
            int unk2 = reader.ReadLEInt32();
            int unk3 = reader.ReadLEInt32();
            int unk4 = reader.ReadLEInt32();
            int unk5 = reader.ReadLEInt32();

            Client.SendAsync(new GameRoom_FFF801(unk1, unk2, unk4, unk5, last));
        }
    }
}
