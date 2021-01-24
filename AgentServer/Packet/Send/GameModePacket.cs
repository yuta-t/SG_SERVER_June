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
using AgentServer.Holders;

namespace AgentServer.Packet.Send
{


    public sealed class GameRoom_LapTimeCountdwon : NetPacket
    {
        public GameRoom_LapTimeCountdwon(short second, byte round, byte last)
        {
            //FF 16 03 01 06 00 00 01
            //FF 16 03 01 06 00 01 20
            //FF 16 03 01 06 00 02 20
            ns.Write((byte)0xFF);
            ns.Write((short)0x316);
            ns.Write((byte)0x1);
            ns.Write(second);
            ns.Write(round);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_LapTimeCountdwon2 : NetPacket
    {
        public GameRoom_LapTimeCountdwon2(short second, byte round, byte last)
        {
            //FF 17 03 00 06 00 01 02 00 00 00 00 00 00 00 02 00 00 00 02
            //FF 17 03 01 06 00 01 02 00 00 00 00 00 00 00 01 00 00 00 80
            //FF 17 03 02 06 00 01 02 00 00 00 00 00 00 00 02 00 00 00 80
            //FF 17 03 02 06 00 01 02 00 00 00 00 00 00 00 01 00 00 00 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x317);
            ns.Write(round);
            ns.Write(second);
            ns.Write((byte)0x1);
            ns.Write(2);
            ns.Write(0);
            //int remainround = 2 - round;
            ns.Write(2);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_MiniGame_RoundTime : NetPacket
    {
        public GameRoom_MiniGame_RoundTime(Account User, int currentround, int isnextround, float RoundTime, byte last)
        {
            //FF 60 05 00 00 00 00 00 00 00 00 00 00 A0 41 04 //0

            //FF 60 05 02 00 00 00 00 00 00 00 00 00 20 42 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x560);
            ns.Write(currentround);
            ns.Write(isnextround);
            ns.Write(RoundTime);
            ns.Write(last);

            //FF 60 05 00 00 00 00 01 00 00 00 00 00 80 3F 08 //1
            //FF 60 05 01 00 00 00 01 00 00 00 00 00 80 3F 08 //2
            //FF 60 05 02 00 00 00 01 00 00 00 00 00 80 3F 10 //3
        }
    }
    public sealed class GameRoom_MiniGame_GetPoint : NetPacket
    {
        public GameRoom_MiniGame_GetPoint(Account User, int nowpoint, int getpoint, byte last)
        {
            //FF 5C 05 00 15 00 00 00 14 00 00 00 00 00 00 00 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x55C);
            ns.Write(User.RoomPos);
            ns.Write(nowpoint);
            ns.Write(getpoint);
            ns.Write(0);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_UpdatePoint : NetPacket
    {
        public GameRoom_MiniGame_UpdatePoint(NormalRoom room, byte last)
        {
            //FF 5D 05 02 00 00 00 00 00 00 00 00 01 00 00 00 00 08
            //FF 5D 05 02 00 00 00 00 00 00 00 00 02 00 00 00 00 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x55D);
            ns.Write(room.PlayerCount());
            foreach (var p in room.Players.Where(p => p.Attribute != 3)
                           .Join(room.DropItem, p => p.UserNum, d => d.UserNum, (p, d) => new { p, d })
                                .OrderBy(o => o.d.MiniGamePoint).ThenBy(o => o.p.RoomPos))
            {

                ns.Write(p.p.RoomPos);
                ns.Write(p.d.MiniGamePoint);
            }
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_55F : NetPacket
    {
        public GameRoom_MiniGame_55F(Account User, int round, byte last)
        {
            //FF 5F 05 00 00 00 00 00 00 00 00 00 00 00 00 04 場主only?

            //FF 5F 05 02 00 00 00 00 00 00 00 00 00 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x55F);
            ns.Write(round);
            ns.Write(0L);
            ns.Write(last);
        }
    }
    public sealed class GameRoom_MiniGame_Respawn : NetPacket
    {
        public GameRoom_MiniGame_Respawn(Account User, int pos, byte last)
        {
            //FF 5A 05 00 00 00 00 00 00 00 00 01 00 00 00 20
            //FF 5A 05 00 00 00 00 00 00 00 00 00 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x55A);
            ns.Write(0L);
            ns.Write(pos);
            ns.Write(last);
        }
    }

    public sealed class FootStep_GoalIn : NetPacket
    {
        public FootStep_GoalIn(byte pos, int unk, byte last)
        {
            //FF 4F 03 00 01 00 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x34F);
            ns.Write(pos);
            ns.Write(unk);
            ns.Write(last);
        }
    }
    public sealed class FootStep_GoalIn_CountDown : NetPacket
    {
        public FootStep_GoalIn_CountDown(int laptime, int counttime, byte last)
        {
            //FF 50 03 BA 52 02 00 10 27 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x350);
            ns.Write(laptime);
            ns.Write(counttime);
            ns.Write(last);
        }
    }

    public sealed class Amsan_LapTime : NetPacket
    {
        public Amsan_LapTime(int unk, byte round, byte pos, byte last)
        {
            /*FF C1 02 01 00 00 00 13 03 04
              FF C1 02 00 00 00 00 06 00 20
              FF C1 02 00 00 00 00 1B 03 04*/
            ns.Write((byte)0xFF);
            ns.Write((short)0x2C1);
            ns.Write(unk);
            ns.Write(pos);
            ns.Write(round);
            ns.Write(last);
        }
    }
    public sealed class Amsan_Step_Button : NetPacket
    {
        public Amsan_Step_Button(int btnid, byte pos, byte last)
        {
            //FF 3A 03 26 00 00 00 17 04
            ns.Write((byte)0xFF);
            ns.Write((short)0x33A);
            ns.Write(btnid);
            ns.Write(pos);
            ns.Write(last);
        }
    }
    public sealed class Amsan_Step_Button_Push : NetPacket
    {
        public Amsan_Step_Button_Push(byte last)
        {
            //FF 3C 03 10
            ns.Write((byte)0xFF);
            ns.Write((short)0x33C);
            ns.Write(last);
        }
    }
    public sealed class Amsan_Goal_Button : NetPacket
    {
        public Amsan_Goal_Button(byte last)
        {
            //FF 80 03 0A 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x380);
            ns.Write((short)0x0); //reward A=100TR
            ns.Write(last);
        }
    }
    public sealed class Amsan_LapTimeControl : NetPacket
    {
        public Amsan_LapTimeControl(int currenttime, int totaltime, int addtime, bool isCorrect, byte last)
        {
            //FF 4D 03 00 00 00 00 A0 86 01 00 A0 86 01 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x34D);
            ns.Write(currenttime);
            ns.Write(totaltime);
            ns.Write(addtime);
            ns.Write(isCorrect);
            ns.Write(last);
        }
    }

    public sealed class RandomGameOver : NetPacket
    {
        public RandomGameOver(byte area, byte pos, List<byte> ranklist, byte last)
        {
            //FF 44 03 01 05 02 00 00 00 02 09 08
            //FF 44 03 01 03 01 00 00 00 07 02
            ns.Write((byte)0xFF);
            ns.Write((short)0x344);
            ns.Write(area);
            ns.Write(pos);
            ns.Write(ranklist.Count);
            foreach (var i in ranklist)
            {
                ns.Write(i);
            }
            ns.Write(last);
        }
    }
    public sealed class RandomGameOver_Die1 : NetPacket
    {
        public RandomGameOver_Die1(byte area, byte last)
        {
            //FF 46 03 01 08
            ns.Write((byte)0xFF);
            ns.Write((short)0x346);
            ns.Write(area);
            ns.Write(last);
        }
    }
    public sealed class RandomGameOver_Die2 : NetPacket
    {
        public RandomGameOver_Die2(byte area, byte last)
        {
            //FF 48 03 01 01 08
            //FF 48 03 06 01 80
            ns.Write((byte)0xFF);
            ns.Write((short)0x348);
            ns.Write(area);
            ns.Write((byte)1);
            ns.Write(last);
        }
    }

    public sealed class GameRoom_CatchFish : NetPacket
    {
        public GameRoom_CatchFish(Account User, NormalRoom room, int time, int fish, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x31B);
            ns.Write(User.RoomPos);
            ns.Write(room.CatchTime);
            ns.Write(time + 100);
            ns.Write(fish);
            ns.Write(0);
            ns.Write((byte)1);
            ns.Write(last);
        }
    }

    public sealed class RequestQuizList_Ack : NetPacket
    {
        public RequestQuizList_Ack(byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x355);
            ns.Write(0);
            ns.Write(6);//question count?
            Dictionary<int, int> runquiz = new Dictionary<int, int>();
            for (int i = 1; i <= 6; i++)
            {
                int runquiznum = RunQuizHolder.RunQuizInfo.Keys.ToList()[new Random(Guid.NewGuid().GetHashCode()).Next(0, RunQuizHolder.RunQuizInfo.Count)];
                ns.Write(i);
                ns.Write(runquiznum);
                RunQuizHolder.RunQuizInfo.TryGetValue(runquiznum, out string name);
                ns.WriteBIG5Fixed_intSize(name);
                runquiz.Add(i, runquiznum);
            }
            ns.Write(6);//question count?
            for (int i = 1; i <= 6; i++)
            {
                ns.Write(i);
                ns.Write(3);//answer choose count
                int correct = new Random(Guid.NewGuid().GetHashCode()).Next(1, 4);
                runquiz.TryGetValue(i, out int runquiznum);
                var wrongans = RunQuizHolder.RunQuizInfo.Keys.Where(w => w != runquiznum).OrderBy(_ => Guid.NewGuid()).Take(3).ToList();
                ns.Write(correct == 1 ? runquiznum : wrongans[0]);
                ns.Write(correct == 2 ? runquiznum : wrongans[1]);
                ns.Write(correct == 3 ? runquiznum : wrongans[2]);
            }
            ns.Write(last);
        }
    }
    public sealed class SetClearLimitTime_Ack : NetPacket
    {
        public SetClearLimitTime_Ack(int sec, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x365);
            ns.Write(sec);
            ns.Write(last);
        }
    }
    public sealed class ClearTimeSection_Ack : NetPacket
    {
        public ClearTimeSection_Ack(byte area, int ClearTime, int sec, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x369);
            ns.Write(area);
            ns.Write(ClearTime);
            ns.Write(sec);
            ns.Write(last);
        }
    }
    public sealed class EnterTimeSection_Ack : NetPacket
    {
        public EnterTimeSection_Ack(byte area, int remaintime, int sec, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x367);
            ns.Write(area);
            ns.Write(remaintime);
            ns.Write(sec);
            ns.Write(last);
        }
    }
    public sealed class AllocatePartner : NetPacket
    {
        public AllocatePartner(NormalRoom room, byte last)
        {
            var HareRedTeam = room.Players.FindAll(p => p.Team == 1 && p.Animal == 0);
            var HareBlueTeam = room.Players.FindAll(p => p.Team == 2 && p.Animal == 0);

            ns.Write((byte)0xFF);
            ns.Write((short)0x5E5);

            ns.Write(4);

            ns.Write(2);
            ns.Write((int)HareRedTeam[0].RoomPos);
            ns.Write(HareRedTeam[0].Partner);
            ns.Write(2);
            ns.Write((int)HareRedTeam[1].RoomPos);
            ns.Write(HareRedTeam[1].Partner);

            ns.Write(2);
            ns.Write((int)HareBlueTeam[0].RoomPos);
            ns.Write(HareBlueTeam[0].Partner);
            ns.Write(2);
            ns.Write((int)HareBlueTeam[1].RoomPos);
            ns.Write(HareBlueTeam[1].Partner);

            ns.Write(8);
            for (int i = 0; i <= 7; i++)
            {
                ns.Write(i);
                ns.Write(room.Players.Find(p => p.RoomPos == i).Animal);
            }
            ns.Write(last);
        }
    }
    public sealed class TeamStatus : NetPacket
    {
        public TeamStatus(Account User, NormalRoom room, byte last)
        {
            //FF E6 05 02 00 00 00 05 00 00 00 00 00 00 00 06 00 00 00 01 00 00 00 02 00 00 00 05 00 00 00 00 00 C8 42 06 00 00 00 00 00 00 00 05 00 00 00 00 04
            //var Hare = User.Animal == 0 ? User : room.Players.Find(p => p.RoomPos == User.Partner);
            //var Tortoise = User.Animal == 1 ? User : room.Players.Find(p => p.RoomPos == User.Partner);
            var MyTeam = room.Players.Where(w => w.RoomPos == User.RoomPos || w.RoomPos == User.Partner).ToList();
            ns.Write((byte)0xFF);
            ns.Write((short)0x5E6);

            ns.Write(MyTeam.Count);
            foreach (var p in MyTeam.OrderBy(o => o.RoomPos))
            {
                ns.Write((int)p.RoomPos);
                ns.Write(p.Animal);
            }
            /*ns.Write(2);
            ns.Write((int)Hare.RoomPos);
            ns.Write(0);
            ns.Write((int)Tortoise.RoomPos);
            ns.Write(1);*/

            ns.Write(MyTeam.Count);
            foreach (var p in MyTeam.OrderBy(o => o.RoomPos))
            {
                ns.Write((int)p.RoomPos);
                ns.Write(p.Fatigue);
            }
            /*ns.Write(2);
            ns.Write((int)Hare.RoomPos);
            ns.Write(Hare.Fatigue);//自己疲勞度，用float類型
            ns.Write((int)Tortoise.RoomPos);
            ns.Write(Tortoise.Fatigue);//隊友疲勞度，用float類型*/

            ns.Write((int)(User.TeamLeader ? User.RoomPos : User.Partner));
            //ns.Write((int)(Hare.TeamLeader ? Hare.RoomPos : Tortoise.RoomPos));
            ns.Write((byte)0);
            ns.Write(last);
        }
    }
    public sealed class ReqChangeTeamLeader : NetPacket
    {
        public ReqChangeTeamLeader(int type, byte last)
        {
            //FF EA 05 02 00 00 00 05 00 00 00 01
            //FF EA 05 01 00 00 00 02 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x5EA);
            ns.Write(type);//0=即時轉，1=等三秒轉，4=叫佢轉
            ns.Write(type == 1 ? 2 : 0);
            ns.Write(last);
        }
    }
    public sealed class NewTeamLeader : NetPacket
    {
        public NewTeamLeader(int leaderpos, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x5EB);
            ns.Write(leaderpos);
            ns.Write(last);
        }
    }
    public sealed class CorunMode_SetBossEnergy_Ack : NetPacket
    {
        public CorunMode_SetBossEnergy_Ack(int boss, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x36B);
            ns.Write(boss);
            ns.Write(last);
        }
    }
    public sealed class CorunMode_DecreaseBossEnergy_Ack : NetPacket
    {
        public CorunMode_DecreaseBossEnergy_Ack(int boss, int hp, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x36E);
            ns.Write(hp);
            ns.Write(boss);
            ns.Write(last);
        }
    }
    public sealed class CorunMode_SetObjectBossEnergy_Ack : NetPacket
    {
        public CorunMode_SetObjectBossEnergy_Ack(int boss, long bossid, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x370);
            ns.Write(boss);
            ns.Write(bossid);
            ns.Write(last);
        }
    }
    public sealed class CorunMode_DecreaseObjectBossEnergy_Ack : NetPacket
    {
        public CorunMode_DecreaseObjectBossEnergy_Ack(int boss, long bossid, int hp, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x373);
            ns.Write(boss);
            ns.Write(bossid);
            ns.Write(hp);
            ns.Write(last);
        }
    }
    public sealed class CorunMode_IncreaseObjectBossEnergy_Ack : NetPacket
    {
        public CorunMode_IncreaseObjectBossEnergy_Ack(int boss, long bossid, int hp, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x375);
            ns.Write(boss);
            ns.Write(bossid);
            ns.Write(hp);
            ns.Write(last);
        }
    }
    public sealed class TriggerObjectEvent_Ack : NetPacket
    {
        public TriggerObjectEvent_Ack(int unk, byte isLeave, int unk3, byte last)
        {
            //FF 5F 03 08 00 00 00 02 DB 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x35F);
            ns.Write(unk);
            ns.Write(isLeave);
            ns.Write(unk3);
            ns.Write(last);
        }
    }
    public sealed class TriggerCheckInObjectEvent_Ack : NetPacket
    {
        public TriggerCheckInObjectEvent_Ack(byte pos, int needcount, int tookcount, int unk2, byte last)
        {
            //FF 61 03 00 08 00 00 00 01 00 00 00 00 DB 00 00 00 40
            ns.Write((byte)0xFF);
            ns.Write((short)0x361);
            ns.Write(pos);
            ns.Write(needcount);
            ns.Write(tookcount);
            ns.Write((byte)0);
            ns.Write(unk2);
            ns.Write(last);
        }
    }
    public sealed class SendPlayerHP : NetPacket
    {
        public SendPlayerHP(NormalRoom room, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x476);
            ns.Write(room.PlayerCount());
            foreach (Account User in room.PlayerList().OrderBy(p => p.RoomPos))
            {
                User.GetMyLevel();
                User.HP = 100 + User.HealthPoint + User.Level * 2;
                User.MaxHP = User.HP;
                ns.Write((int)User.RoomPos);
                ns.Write(User.HP);
            }
            ns.Write(last);
        }
    }
    public sealed class AnubisObjectBossInit : NetPacket
    {
        public AnubisObjectBossInit(NormalRoom room, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x473);
            ns.Write(room.AnubisObjectBoss.Count);
            foreach (var objectBoss in room.AnubisObjectBoss)
            {
                ns.Write(0x8D72C8);
                ns.Write((int)objectBoss.Key);
                ns.Write(objectBoss.Value.HP);
                ns.Write(1L);
            }
            ns.Write(last);
        }
    }
    /*
    public sealed class AnubisObjectBossInitFail : NetPacket
    {
        public AnubisObjectBossInitFail(NormalRoom room, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x474);
            ns.Write(room.AnubisObjectBoss.Count);
            foreach (var objectBoss in room.AnubisObjectBoss)
            {
                ns.Write((int)objectBoss.Key);
            }
            ns.Write(last);
        }
    }*/
    public sealed class AnubisDecreasObjectBossHP : NetPacket
    {
        public AnubisDecreasObjectBossHP(NormalRoom room, int unk, int bossid, int reducehp, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x47D);
            ns.Write(0);
            ns.Write(unk);
            ns.Write(bossid);
            ns.Write(room.AnubisObjectBoss[bossid].HP);
            ns.Write(reducehp);
            ns.Write(last);
        }
    }
    public sealed class AnubisDecreaseMyHP : NetPacket
    {
        public AnubisDecreaseMyHP(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x478);
            ns.Write((int)User.RoomPos);
            ns.Write(User.HP);
            ns.Write(last);
        }
    }
    public sealed class AnubisRemainRebirthStone : NetPacket
    {
        public AnubisRemainRebirthStone(Account User, byte last)
        {
            //FF 71 04 D9 6B 00 00 09 00 00 00 01
            ns.Write((byte)0xFF);
            ns.Write((short)0x471);
            ns.Write(27609);//itemid
            ns.Write(9);//remain count TODO: 獲取玩家剩餘的復活石數量
            ns.Write(last);
        }
    }
    public sealed class AnubisRebirth : NetPacket
    {
        public AnubisRebirth(Account User, byte last)
        {
            ns.Write((byte)0xFF);
            ns.Write((short)0x48E);
            ns.Write((int)User.RoomPos);
            ns.Write(User.HP);
            ns.Write(last);
        }
    }
}
