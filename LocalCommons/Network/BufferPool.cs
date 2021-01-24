using System;
using System.Collections;
using System.Collections.Generic;

namespace LocalCommons.Network
{
    /// <summary>
    /// Buffer Pool For byte[]s
    /// Author: Raphail
    /// </summary>
	public class BufferPool
	{

        public static List<BufferPool> Pools { get; private set; } = new List<BufferPool>();

        static BufferPool()
        {
            Pools = new List<BufferPool>();
        }

        private readonly string m_Name;

		private readonly int m_InitialCapacity;
		private readonly int m_BufferSize;

		private int m_Misses;

		private readonly Queue<byte[]> m_FreeBuffers;

        public int Count
        {
            get
            {
                lock (this)
                    return this.m_FreeBuffers.Count;
            }
        }

        /// <summary>
        /// Writing Information About your Pool Into your Variables.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="freeCount">Free Buffer Count</param>
        /// <param name="initialCapacity">Initial Capacity</param>
        /// <param name="currentCapacity">Capacity In Use</param>
        /// <param name="bufferSize">Buffer Length</param>
        /// <param name="misses">Misses Count</param>
        public void GetInfo(
            out string name,
            out int freeCount,
            out int initialCapacity,
            out int currentCapacity,
            out int bufferSize,
            out int misses)
		{
			lock (this)
			{
				name = this.m_Name;
				freeCount = this.m_FreeBuffers.Count;
				initialCapacity = this.m_InitialCapacity;
				currentCapacity = this.m_InitialCapacity * (1 + this.m_Misses);
				bufferSize = this.m_BufferSize;
				misses = this.m_Misses;
			}
		}

        /// <summary>
        /// Initializes New Buffer Pool
        /// </summary>
        /// <param name="name">Buffer Pool's Name/</param>
        /// <param name="initialCapacity">Buffer Pool's Capacity</param>
        /// <param name="bufferSize">Length Of Any Buffer.</param>
		public BufferPool(
            string name,
            int initialCapacity,
            int bufferSize)
		{
			this.m_Name = name;

			this.m_InitialCapacity = initialCapacity;
			this.m_BufferSize = bufferSize;

			this.m_FreeBuffers = new Queue<byte[]>(initialCapacity);

			for (int i = 0; i < initialCapacity; ++i) this.m_FreeBuffers.Enqueue(new byte[bufferSize]);

			lock (Pools)
				Pools.Add(this);
		}

        /// <summary>
        /// Returns Free Buffer.
        /// </summary>
        /// <returns></returns>
		public byte[] AcquireBuffer()
		{
			lock (this)
			{
				if (this.m_FreeBuffers.Count > 0)
					return this.m_FreeBuffers.Dequeue();

				++this.m_Misses;

				for (int i = 0; i < this.m_InitialCapacity; ++i) this.m_FreeBuffers.Enqueue(new byte[this.m_BufferSize]);

				return this.m_FreeBuffers.Dequeue();
			}
		}

        /// <summary>
        /// Releases Buffer and Put it to Free Buffers.
        /// </summary>
        /// <param name="buffer"></param>
		public void ReleaseBuffer(byte[] buffer)
		{
			if (buffer == null)
				return;

			lock (this) this.m_FreeBuffers.Enqueue(buffer);
		}

        /// <summary>
        /// Fully Release Buffer
        /// </summary>
		public void Free()
		{
			lock ( Pools )
				Pools.Remove( this );
		}
	}
}