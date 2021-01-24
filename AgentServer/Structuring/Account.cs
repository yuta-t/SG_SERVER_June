using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Shu;
using AgentServer.Structuring.Map;

namespace AgentServer.Structuring
{
	/// <summary>
	/// Stucture That Contains Information About Account
	/// </summary>
	public class Account
	{
        /***Strugarden Code Block***/
        public byte GamePosX { get; set; }
        public byte GamePosY { get; set; }
        public byte GameDirection { get; set; }
        public byte GameDirection2 { get; set; }
        public int charChooseNum { get; set; }
        public string CharacterEncodedClothColor { get; set; }
        public string CharacterEncodedHairColor { get; set; }
        public string CharacterEncodedSkinColor { get; set; }
        public byte[] CharacterEncodedCloth { get; set; }
        public byte[] CharacterEncodedHair { get; set; }
        public byte[] CharacterEncodedSkin { get; set; }
        public byte[] CharacterDecodedCloth { get; set; }
        public byte[] CharacterDecodedHair { get; set; }
        public byte[] CharacterDecodedSkin { get; set; }
        public byte[] CharacterDecodedHairClump1 { get; set; }
        public byte[] CharacterDecodedHairClump2 { get; set; }
        public byte[] CharacterDecodedHairClump3 { get; set; }
        public byte[] CharacterDecodedHairClump4 { get; set; }
        public int Gender { get; set; }
        public byte GameAct { get; set; }
        //public List<byte[]> CharacterEncodedColor { get; } = new List<byte[]>();
        public int UserNum { get; set; }
        public string UserID { get; set; }
        public string GameID { get; set; }
        public int UserType { get; set; }
        public int GlobalID { get; set; }
        public int CharacterCount { get; set; }
        public int CharacterPos { get; set; }
        public string CharacterNickname1 { get; set; }
        public string CharacterNickname2 { get; set; }
        public int CharacterNation1 { get; set; }
        public int CharacterNation2 { get; set; }
        public int CharacterZula1 { get; set; }
        public int CharacterZula2 { get; set; }
        public int CharacterJob1 { get; set; }
        public int CharacterJob2 { get; set; }
        public int CharacterLevel1 { get; set; }
        public int CharacterLevel2 { get; set; }
        public int CharacterOneRawEquipment { get; set; }
        public int[] CharacterOneEquipment = new int[8];
        public int[] CharacterOneSEffect = new int[6];
        public int CharacterTwoEquipment1 { get; set; }
        public int CharacterTwoEquipment2 { get; set; }
        public int CharacterTwoEquipment3 { get; set; }
        public int CharacterTwoEquipment4 { get; set; }
        public int CharacterTwoEquipment5 { get; set; }
        public int CharacterTwoEquipment6 { get; set; }
        public int CharacterTwoEquipment7 { get; set; }
        public int CharacterTwoEquipment8 { get; set; }
        public int BattleGlobalID { get; set; }
        public int BattleReady { get; set; }
        public MapInfo UserMap = new MapInfo();
        public List<ItemAttr> UserItem { get; } = new List<ItemAttr>();
        //public byte CharactersCount { get; set; }
        public List<ushort> CurrentAvatarInfo { get; } = new List<ushort>();
        //public List<ItemAttr> WearAvatarItemAttr { get; } = new List<ItemAttr>();
        //public List<ItemAttr> WearCosAvatarItemAttr { get; } = new List<ItemAttr>();
        //public List<ItemSetAttr> WearItemSetAttr { get; } = new List<ItemSetAttr>();
        public ConcurrentDictionary<ushort, float> AllItemAttr { get; } = new ConcurrentDictionary<ushort, float>();
        public List<int> WearAvatarItem { get; } = new List<int>();
        public List<int> WearCosAvatarItem { get; } = new List<int>();
        public List<int> WearFashionItem { get; } = new List<int>();
        public bool isFashionModeOn { get; set; }

        //public List<int> CurrentAvatarItemNum { get; } = new List<int>();
        //public List<int> CurrentCosAvatarItemNum { get; } = new List<int>();
        public ConcurrentDictionary<int, List<ItemAttr>> UserItemAttr { get; } = new ConcurrentDictionary<int, List<ItemAttr>>();
        //public List<AvatarItemInfo> AvatarItems { get; set; } = new List<AvatarItemInfo>();
        public ConcurrentDictionary<int, AvatarItemInfo> AvatarItems { get; set; } = new ConcurrentDictionary<int, AvatarItemInfo>();
        public short costumeMode { get; set; }
        public bool haveAvatar { get; set; }
        public int Session { get; set; } //cookie
		public ClientConnection Connection { get; set; }
		public string NickName { get; set; }
        public long Exp { get; set; }
        public long TR { get; set; }
        public int Cash { get; set; }
        public int Level { get; set; }
        public decimal Luck { get; set; }
        public string LastIp { get; set; }
        public short Port { get; set; }
        public short UDPPort { get; set; }
        public byte[] UDPInfo { get; set; }

        public bool isLogin { get; set; }
        public bool noNickName { get; set; }
        public byte[] EncryptKey { get; set; }
        public byte[] XorKey { get; set; }
        public int Attribute { get; set; }
        public int TopRank { get; set; }
        public int GameOption { get; set; }
        public long CurrentShuID { get; set; }
        public UserShuInfo UserShuInfo { get; set; } = new UserShuInfo();
        public long LastCheckTime { get; set; }

        public int AttackPoint { get; set; }
        public int HealthPoint { get; set; }
        public int DefensePoint { get; set; }

        //---遊戲房間
        public bool IsReady { get; set; }
        public bool InGame { get; set; }
        public int CurrentRoomId { get; set; }
        public byte RoomPos { get; set; }
        public bool EndLoading { get; set; }
        //public bool isGoal { get; set; }
        public int LapTime { get; set; }
        public int ServerLapTime { get; set; }
        public short GameEndType { get; set; }
        public float RaceDistance { get; set; }
        public byte Rank { get; set; }
        public byte Team { get; set; }
        public bool GameOver { get; set; }

        public int CurrentLapTime { get; set; }
        public int LastLapTime { get; set; }

        public byte RelayTeamPos { get; set; } = 0;
        public byte RelayTeam { get; set; } = 0;

        //---龜兔賽跑

        public int Partner { get; set; } = 8;
        public int Animal { get; set; }
        public float Fatigue { get; set; } = 0f;
        public bool TeamLeader { get; set; }
        public bool RequestChange { get; set; }

        //---阿努比斯

        public int HP { get; set; }
        public int MaxHP { get; set; }

        public void SelectRelayTeam(byte relayteampos)
        {
            IsReady = relayteampos > 2 ? true : false;
            RelayTeamPos = relayteampos;
            //1,2=Ready area 3,4,5=A 6,7,8=B 9,10,11=C 12,13,14=D 15,16,17=E 18,19,20=F
            switch (RelayTeamPos)
            {
                case 1:
                case 2:
                    RelayTeam = 0;
                    break;
                case 3:
                case 4:
                case 5:
                    RelayTeam = 1;
                    break;
                case 6:
                case 7:
                case 8:
                    RelayTeam = 2;
                    break;
                case 9:
                case 10:
                case 11:
                    RelayTeam = 3;
                    break;
                case 12:
                case 13:
                case 14:
                    RelayTeam = 4;
                    break;
                case 15:
                case 16:
                case 17:
                    RelayTeam = 5;
                    break;
                case 18:
                case 19:
                case 20:
                    RelayTeam = 6;
                    break;
                default:
                    RelayTeam = 0;
                    break;
            }
        }

        public void LeaveRoomReset()
        {
            InGame = false;
            IsReady = false;
            RoomPos = 0;
            CurrentRoomId = 0;
            Team = 0;
            RelayTeam = 0;
            RelayTeamPos = 0;
            EndLoading = false;
            Partner = 8;
            Fatigue = 0f;
            TeamLeader = false;
        }

        public void GetMyLevel()
        {
            Level = AccountHolder.LevelInfo.Count(c => c <= Exp) + 1;
        }

    }
}
