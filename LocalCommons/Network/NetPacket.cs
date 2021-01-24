using System;
using System.Linq;
using LocalCommons.Cryptography;
using LocalCommons.Utilities;
//using LocalCommons.Compression;

namespace LocalCommons.Network
{
	/// <summary>
	/// Abstract Class For Writing Packets
	/// Author: Raphail
	/// </summary>
	public abstract class NetPacket
	{
		protected PacketWriter ns;
		/// <summary>
		/// Опкод пакета
		/// </summary>
		private readonly int _mPacketId;
		/// <summary>
		/// Сначала младшие байты
		/// </summary>
		private readonly bool _mLittleEndian;
		/// <summary>
		/// Пакет от сервера
		/// </summary>
		private bool _mIsArcheAge;
        /// <summary>
        /// Уровень схатия/шифрования
        /// </summary>
        private readonly byte _level;
        //Fix by Yanlong-LI
        private readonly byte[] _EncryptKey;
        /// <summary>
        /// Глобальный подсчет пакетов DD05
        /// </summary>
        //Исправление входа второго пользователя, вторичный логин, счетчик повторного соединения с возвратом в лобби, вызванный ошибкой
        public static byte NumPckSc = 0;  //修复第二用户、二次登陆、大厅返回重连DD05计数器造成错误问题 BUG глобальный подсчет пакетов DD05
		public static sbyte NumPckCs = -1; //глобальный подсчет пакетов 0005

		/// <summary>
		/// Пакет от сервера/клиента
		/// </summary>
		public bool IsArcheAgePacket
		{
			get { return this._mIsArcheAge; }
			set { this._mIsArcheAge = true; }
		}

        public byte[] EncryptKey { get; set; }
        public byte[] XorKey { get; set; }
        public bool isEncrypt;

        protected NetPacket()
        {
            this._mPacketId = 0;
            this._level = 1;
            this._mLittleEndian = true;
            this._mIsArcheAge = true;
            ns = PacketWriter.CreateInstance(16, true);
        }

        /// <summary>
        /// Creates Instance Of Any Other Packet
        /// </summary>
        /// <param name="packetId">Packet Identifier(opcode)</param>
        /// <param name="isLittleEndian">Send Data In Little Endian Or Not.</param>
        protected NetPacket(int packetId, bool isLittleEndian)
		{
			this._mPacketId = packetId;
			this._mLittleEndian = isLittleEndian;
			ns = PacketWriter.CreateInstance(16, isLittleEndian);
		}

        /// <summary>
        /// Creates Instance Of ArcheAge Game Packet.
        /// </summary>
        /// <param name="level">Packet Level</param>
        /// <param name="packetId">OP Code 1Byte</param>
        protected NetPacket(byte level, int packetId)
		{
			this._mPacketId = packetId;
			this._level = level;
			this._mLittleEndian = true;
			this._mIsArcheAge = true;
			ns = PacketWriter.CreateInstance(16, true);
		}

        /// <summary>
        /// Creates Instance Of ArcheAge Game Packet.
        /// </summary>
        /// <param name="level">Packet Level</param>
        /// <param name="packetId">OP Code 1Byte</param>
        protected NetPacket(byte level, byte[] EncryptKey)
        {
            this._mPacketId = 0;
            this._level = level;
            this._mLittleEndian = true;
            this._mIsArcheAge = true;
            //this._EncryptKey = EncryptKey;
            ns = PacketWriter.CreateInstance(16, true);
        }

        /// <summary>
        /// Creates Instance Of ArcheAge Game Packet.
        /// </summary>
        /// <param name="level">Packet Level</param>
        protected NetPacket(byte level)
        {
            this._mPacketId = 0;
            this._level = level;
            this._mLittleEndian = true;
            this._mIsArcheAge = false;
            ns = PacketWriter.CreateInstance(16, true);
        }

        /// <summary>
		/// 加密packet 1byte opcode
		/// </summary>
		/// <param name="level">Packet Level</param>
		/// <param name="packetId">OP Code 1Byte</param>
        /// /// <param name="EncryptKey">加密Key</param>
		/*protected NetPacket(byte level, int packetId, byte[] EncryptKey)
        {
            this._mPacketId = packetId;
            this._level = level;
            this._mLittleEndian = true;
            this._mIsArcheAge = true;
            this._EncryptKey = EncryptKey;
            ns = PacketWriter.CreateInstance(16, true);
        }*/

        /// <summary>
        /// Stream Where We Writing Data.
        /// </summary>
        public PacketWriter UnderlyingStream
		{
			get { return ns; }
		}

        /// <summary>
        /// Compiles Data And Return Compiled byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] Compile()
        {
            var temporary = PacketWriter.CreateInstance(1024, this._mLittleEndian);
            byte[] redata;
            short len = isEncrypt ? (short)(ns.Length + 3) : (short)(ns.Length + 2);
            //temporary.Write((short)(ns.Length + (m_IsArcheAge ? 6 : 2)));

            if (this._mIsArcheAge)
            {
                //Серверные пакеты
                switch (this._level)
                {
                    case 0: //未加密
                        temporary.Write((short)(ns.Length + 3));
                        temporary.Write((byte)this._mPacketId); //op code

                        redata = ns.ToArray();
                        temporary.Write(redata, 0, redata.Length);
                        break;
                    case 1: //未加密&加密 no op code
                        //temporary.Write(len);
                        /*if (isEncrypt)
                            redata = Encrypt.newEncryptByte(EncryptKey, XorKey, ns.ToArray());
                        else
                            redata = ns.ToArray();
                        */
                        /***Strugarden Code Block***/
                        redata = Encrypt.newEncryptByte(ns.ToArray());
                        temporary.Write(redata, 0, redata.Length);
                        temporary.Write((byte)0xA); //Packet End
                        //if (isEncrypt)
                            //temporary.Write((byte)1); //Packet End
                        break;
                    case 2: //udp
                        redata = ns.ToArray();
                        Console.WriteLine(Utility.ByteArrayToString(redata));
                        temporary.Write(redata, 0, redata.Length);
                        break;
                    case 3: //CommunityAgentServer
                        temporary.Write((short)(ns.Length + 2));
                        redata = ns.ToArray();

                        temporary.Write(redata, 0, redata.Length);
                        break;
                    default:
                        temporary.Write((short)(ns.Length + 3));
                        temporary.Write((byte)this._level);
                        temporary.Write((short)this._mPacketId);

                        redata = ns.ToArray();
                        temporary.Write(redata, 0, redata.Length);
                        break;
                }
            }
            else
            {
                temporary.Write((short)(ns.Length + 3));
                redata = ns.ToArray();
                temporary.Write(redata, 0, redata.Length);
                temporary.Write((byte)1); //Packet End
            }
            //PacketWriter.ReleaseInstance(ns);
            //ns = null;
            byte[] compiled = temporary.ToArray();
            PacketWriter.ReleaseInstance(temporary);
            temporary = null;

            return compiled;
        }

    }
}
