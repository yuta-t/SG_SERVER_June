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

namespace AgentServer.Packet
{
    public class GameModeHandle
    {

        public static void GameMode_LapTimeCountdwon(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 15 03 06 00 00 01 01
            //FF C0 02 FF 15 03 06 00 01 01 20
            //FF C0 02 FF 15 03 06 00 02 01 20
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            //Console.WriteLine("LapTimeCountdwon: {0}", Utility.ByteArrayToString(reader.Buffer));
            short second = reader.ReadLEInt16();
            byte round = reader.ReadByte(); //round?
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_LapTimeCountdwon(RoomPlayer, second, round, last));
            }
            if (room.GameMode == 38) //小遊戲
            {
                Task.Run(() => Task.Delay(second * 1000))
                    .ContinueWith((t) =>
                    {
                        foreach (Account RoomPlayer in room.Players)
                        {
                            RoomPlayer.Connection.SendAsync(new GameRoom_LapTimeCountdwon2(RoomPlayer, second, round, last));
                        }
                        room.Round = round;
                        if (room.Round == 0) //init pointlist
                        {
                            foreach (Account Player in room.Players)
                            {
                                DropList dropList = new DropList
                                {
                                    UserNum = Player.UserNum,
                                    BounsTR = 0,
                                    BounsEXP = 0,
                                    TR = 0,
                                    EXP = 0,
                                    Rank = 0,
                                    CardID = new List<int> { 0 },
                                    MiniGamePoint = 0,
                                    MiniGameStarPoint = 0
                                };
                                room.DropItem.Add(dropList);
                            }
                        }
                    });
            }

        }

        public static void GameMode_MiniGame_RoundTime(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 5E 05 00 00 00 00 00 00 00 00 00 00 A0 41 04
            //FF C0 02 FF 5E 05 02 00 00 00 00 00 00 00 00 00 20 42 01
            Account User = Client.CurrentAccount;
            int round = reader.ReadLEInt32(); //current round
            int nextround = reader.ReadLEInt32(); //nextround?
            float RoundTime = reader.ReadLESingle();
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_RoundTime(RoomPlayer, round, nextround, RoundTime, last));
                RoomPlayer.GameOver = false;
            }
            room.Survival = (byte)room.Players.Count(p => p.Attribute != 3);
            long RoundEndTime = Utility.CurrentTimeMilliseconds() + (long)(RoundTime * 1000);
            Task.Run(() => GameMode_MiniGame_RoundThread(RoundEndTime, room));
            //Console.WriteLine("room.Round: {0}", room.Round);
            Client.SendAsync(new GameRoom_MiniGame_55F(User, round, last));
            room.RespwanList.Clear();
        }
        public static void GameMode_MiniGame_RoundThread(long RoundTime, NormalRoom room)
        {
            while(Utility.CurrentTimeMilliseconds() < RoundTime)
            {
                if (room.Survival == 0) //next round
                    break;
                //wait all gameover or time
            }

            int point = 100 + 50 * room.Round;
            if (room.PlayingMapNum != 40006)
            {
                room.Players.Where(w => w.Attribute != 3 && !w.GameOver)
                    .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { p, d })
                        .ToList().ForEach(f => f.d.MiniGamePoint += point);
            }

            int isnextround = 1;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_RoundTime(RoomPlayer, room.Round, isnextround, 1, 0x1));
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_UpdatePoint(RoomPlayer, room, 0x1));
            }

            int nextround = room.Round + 1;
            int maxround = room.PlayingMapNum == 40003 ? 1 : 3;
            if (nextround == maxround) //endgame
            {
                int laptime = 100;

                foreach (var p in room.Players.Where(p => p.Attribute != 3)
                           .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new {p, d })
                                .OrderByDescending(o => o.d.MiniGamePoint).ThenBy(o => o.p.RoomPos))
                {
                    if (p.d.MiniGamePoint > 0)
                    {
                        p.d.Rank = room.Rank++;
                        int mylabtime = laptime++;
                        p.p.ServerLapTime = mylabtime;
                        p.p.LapTime = mylabtime;
                        p.d.MiniGameStarPoint = (int)Math.Round(p.d.MiniGamePoint / 4.5, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        p.d.Rank = 98;
                        p.p.ServerLapTime = 100000;
                        p.p.LapTime = 100000;
                    }
                }

                room.Result = GenResult_ForMiniGameMode(room, 0x1);
                room.RegisterItem(-1, -1, 2, 0x1049F00C, true);

                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameUpdateEXP(RoomPlayer, 0x1));
                }
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.Connection.SendAsync(new GameRoom_GameResult2(RoomPlayer, room.Result));
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF0102", 0x1));
                    foreach (Account Player in room.Players)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_RoomPosReady(RoomPlayer, Player.RoomPos, false, 0x1));
                    }
                    RoomPlayer.Connection.SendAsync(new GameRoom_GoodsInfo(RoomPlayer, room, 0x1));
                }
                foreach (Account RoomPlayer in room.Players)
                {
                     //FF2D02A3220000C4C11400CB110000DB0800000E0C00001E0F0000
                    RoomPlayer.Connection.SendAsync(new GameRoom_UpdateIndividualGameRecord(RoomPlayer, 0x1));
                }
                Task.Delay(6000);
                foreach (Account RoomPlayer in room.Players)
                {
                    //MoveToGameRoom
                    RoomPlayer.Connection.SendAsync(new GameRoom_Hex(RoomPlayer, "FF9503", 0x1)); //9704
                }

                //reset
                room.Rank = 1;
                room.Result = null;
                room.DropItem.Clear();
                room.RespwanList.Clear();
                room.isPlaying = false;
                foreach (Account RoomPlayer in room.Players)
                {
                    RoomPlayer.IsReady = false;
                    RoomPlayer.EndLoading = false;
                    RoomPlayer.GameEndType = 0;
                    RoomPlayer.GameOver = false;
                }
            }

        }

        public static void GameMode_MiniGame_GetPoint(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 5B 05 00 14 00 00 00 80
            Account User = Client.CurrentAccount;
            byte pos = reader.ReadByte();
            int point = reader.ReadLEInt32();
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            int nowpoint = room.DropItem.Find(f => f.UserNum == User.UserNum).MiniGamePoint += point;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_UpdatePoint(RoomPlayer, room, last));
            }
            Client.SendAsync(new GameRoom_MiniGame_GetPoint(User, nowpoint, point, last));

        }
        public static void GameMode_MiniGame_GameOver(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 3D 03 15 91 E5 39 10 00 A0 42 00 00 00 00 00 40
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            //Console.WriteLine("33D: {0}", Utility.ByteArrayToString(reader.Buffer));
            User.GameOver = true;
            foreach (Account RoomPlayer in room.Players)
            {
                RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_GameOver(RoomPlayer, User.RoomPos, last));
            }
            room.Survival -= 1;
        }
        public static void GameMode_MiniGame_Respawn(ClientConnection Client, PacketReader reader, byte last)
        {
            //FF C0 02 FF 59 05 00 00 00 00 40
            Account User = Client.CurrentAccount;
            NormalRoom room = Rooms.NormalRoomList.Find(rm => rm.ID == User.CurrentRoomId);
            room.RespwanList.Add(User.RoomPos);
            if (room.Players.Count(p => p.Attribute != 3 && p.GameOver) == room.RespwanList.Count)
            {
                foreach (Account RoomPlayer in room.Players)
                {
                    foreach (int pos in room.RespwanList)
                    {
                        RoomPlayer.Connection.SendAsync(new GameRoom_MiniGame_Respawn(RoomPlayer, pos, last));
                    }
                }
            }
        }


        private static byte[] GenResult_ForMiniGameMode(NormalRoom room, byte last)
        {
            PacketWriter ns = new PacketWriter();
            ns = PacketWriter.CreateInstance(16, true);
            ns.Write((byte)0xFF);
            ns.Write((short)0x381);
            ns.Write((byte)room.RoomKindID);
            ns.Fill(0xB);
            int playercount = room.Players.Count(p => p.Attribute != 3);
            ns.Write((byte)playercount); //count?
            ns.Fill(0x6);
            foreach (var p in room.Players.Where(p => p.Attribute != 3)
                            .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { player = p, droplist = d })
                                 .OrderBy(o => o.droplist.Rank).ThenBy(o => o.player.ServerLapTime).ThenBy(o => o.player.RoomPos))
            {
                ns.Write(p.player.ServerLapTime);
                ns.Write(p.player.LapTime);
                ns.Write(p.droplist.Rank);
                ns.Write(p.player.RoomPos);
                ns.Write(p.droplist.EXP);
                ns.Write(p.droplist.MiniGameStarPoint);
                ns.Write(p.droplist.TR);
                ns.Write(0);
                ns.Fill(0xC);
                ns.Write(p.droplist.BounsEXP);
                ns.Write(p.droplist.BounsTR);
                int bounsweight = 0;
                if (p.droplist.BounsTR > 0)
                    bounsweight += 4;
                if (p.droplist.BounsEXP > 0)
                    bounsweight += 8;
                ns.Write(bounsweight); // 0C 00 00 00
                ns.Write(playercount); //02 00 00 00 player count?
                int count = p.droplist.CardID.Count;
                ns.Write(count); //card count

                foreach (var card in p.droplist.CardID)
                {
                    ns.Write(card);
                }
                ns.WriteHex("00BA8B19000000000000000000000000000000000000000000C5B01B000000000000");
            }

            //minigame part
            //00 00 03 00 00 00 03 00 00 00 16 00 00 00
            //00 00 00 00 00 02 C2 01 00 00 02 00 00 00 01 00
            ns.Write((short)0);
            ns.Write(3);
            ns.Write(3);
            ns.Write(12 + playercount * 5);
            foreach (var p in room.Players.Where(p => p.Attribute != 3)
                           .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { p, d })
                                .OrderBy(o => o.p.RoomPos))
            {
                ns.Write(p.p.RoomPos);
                ns.Write(p.d.MiniGamePoint);
            }
            ns.Write(playercount); //player count?
            ns.Write(1);
            ns.Write(4);
            //minigame part end

            bool hasgavegoods = GameRoomEvent.SendRegisterGoods(room, out byte[] getgoodsnickname, last);
            if (hasgavegoods)
            {
                Account roomMaster = room.Players.Find(p => p.RoomPos == room.RoomMasterIndex);
                roomMaster.Connection.SendAsync(new GameRoom_DeleteKeepItem(roomMaster, room, last));
            }

            ns.Write(0x3EA);
            ns.Write(!hasgavegoods ? 0x11 : 0x11 + getgoodsnickname.Length);
            ns.Write(room.ItemNum);

            if (!hasgavegoods)
            {
                ns.Write(-1L);
                ns.Write((byte)0);
                ns.Write(1);
            }
            else
            {
                ns.Write(0x3b1c32);//32 1C 3B 00
                ns.Write(2);
                ns.Write(getgoodsnickname, 0, getgoodsnickname.Length);//B5 A3 B8 DC B6 5D A4 48
                ns.Write((byte)0);
                ns.Write(getgoodsnickname.Length + 1);
            }
            ns.Write(0x3F6);
            ns.Write(4);
            ns.Write(0);
            ns.Write(0xC);
            ns.Write(last);
            byte[] ret = ns.ToArray();
            PacketWriter.ReleaseInstance(ns);
            ns = null;
            //Console.WriteLine("Result packet: " + Utility.ByteArrayToString(ret));
            return ret;
        }
    }
}
