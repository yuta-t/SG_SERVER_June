﻿using System;

namespace LocalCommons.Network
{
    /// <summary>
    /// Delegate Which Class When Packet Received
    /// </summary>
    /// <typeparam name="T">Any Connection Where T : IConnection, new()</typeparam>
    /// <param name="net">Connection </param>
    /// <param name="reader"></param>
	public delegate void OnPacketReceive<T>(T net, PacketReader reader );

    /// <summary>
    /// PacketHandler That Uses For Holding Delegate(OnPacketReceive) For Handle Packets.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class PacketHandler0<T>
	{
		private readonly int m_PacketID;
		private readonly OnPacketReceive<T> m_OnReceive;

		public PacketHandler0(int packetID, OnPacketReceive<T> onReceive )
		{
			this.m_PacketID = packetID;
			this.m_OnReceive = onReceive;
		}

		public int PacketID
		{
			get { return this.m_PacketID; }
		}

		public OnPacketReceive<T> OnReceive
		{
			get	{ return this.m_OnReceive; }
		}
	}
}
