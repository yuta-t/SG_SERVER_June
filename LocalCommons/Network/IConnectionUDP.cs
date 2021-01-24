using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LocalCommons.Logging;
//using LocalCommons.Utilities;

namespace LocalCommons.Network
{
	/// <summary>
	/// Abstract Connection Which You Must Inherit
	/// Author: Raphail
	/// </summary>
	public abstract class IConnectionUDP : IDisposable
	{
		protected Socket m_CurrentChannel;
		private SocketAsyncEventArgs m_AsyncReceive;
		private byte[] m_RecvBuffer;
		private readonly object m_SyncRoot = new object();
		private Queue<NetPacket> m_PacketQueue;
		private bool m_Disposing;
		private readonly DateTime m_NextCheckActivity;
		private readonly string m_Address;
        private EndPoint m_EndPoint;
        private static int m_CoalesceSleep = -1;
		private bool m_BlockAllPackets;
		private readonly DateTime m_ConnectedOn;
		private static BufferPool m_RecvBufferPool = new BufferPool("Receive", 4096, 4096);
		protected event EventHandler DisconnectedEvent;
		private bool m_Running;

		//For ArcheAge Connections.
		protected bool m_LittleEndian;

		/// <summary>
		/// Current TCP Client.
		/// </summary>
		public Socket CurrentChannel
		{
			get { return this.m_CurrentChannel; }
		}

		/// <summary>
		/// Sleeping On Send.
		/// </summary>
		public static int CoalesceSleep
		{
			get { return m_CoalesceSleep; }
			set { m_CoalesceSleep = value; }
		}

		/// <summary>
		/// Blocking Packets - Not Send Means.
		/// </summary>
		public bool BlockAllPackets
		{
			get { return this.m_BlockAllPackets; }
			set { this.m_BlockAllPackets = value; }
		}

		/// <summary>
		/// New Instance Of IConnection or Any Your Connection.
		/// </summary>
		/// <param name="socket">Accepted Socket.</param>
		public IConnectionUDP(Socket socket)
		{
			this.m_CurrentChannel = socket;
			this.m_ConnectedOn = DateTime.Now;
			this.m_RecvBuffer = m_RecvBufferPool.AcquireBuffer();
			//-------------Async Receive ----------------------
			this.m_AsyncReceive = new SocketAsyncEventArgs();
			this.m_AsyncReceive.Completed += this.M_AsyncReceive_Completed;
			this.m_AsyncReceive.SetBuffer(this.m_RecvBuffer, 0, this.m_RecvBuffer.Length);
			//-------------------------------------------------
			this.m_PacketQueue = new Queue<NetPacket>();
			//-----------------------------------------------
			//this.m_Address = ((IPEndPoint)this.m_AsyncReceive.RemoteEndPoint).Address.ToString();
            this.m_EndPoint = new IPEndPoint(IPAddress.Any, 0);

            if (this.m_CurrentChannel == null)
			{
				return;
			}

			this.RunReceive();
			this.m_Running = true;
		}

		/// <summary>
		/// Set TRUE If you want Break Running.
		/// </summary>
		private readonly bool BreakRunProcess;

		/// <summary>
		/// Start Running Receiving Process.
		/// </summary>
		public void RunReceive()
		{
			try
			{
				bool res = false;
				do
				{
                    /*if (this.m_AsyncReceive == null) //Disposed
					{
						break;
					}

                    lock (this.m_SyncRoot)
					{
						//res = !this.m_CurrentChannel.ReceiveAsync(this.m_AsyncReceive);
                        res = !this.m_CurrentChannel.ReceiveFromAsync(this.m_AsyncReceive);

                    }

                    if (res)
					{
						this.ProceedReceiving(this.m_AsyncReceive);
					}
                    //res = true;
                    this.ProceedReceiving(this.m_AsyncReceive);*/
                    this.m_CurrentChannel.BeginReceiveFrom(this.m_RecvBuffer, 0, this.m_RecvBuffer.Length, SocketFlags.None,
                        ref this.m_EndPoint, new AsyncCallback(ReceiveDataAsync), null);
                }
				while (res);
			}
			catch (Exception e)
			{
				Log.Info(e.ToString());
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
			}
		}

        private void ReceiveDataAsync(IAsyncResult ar)
        {
            //AsyncSocketUDPState so = ar.AsyncState as AsyncSocketUDPState;
            //EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            int len = -1;
            try
            {
                len = this.m_CurrentChannel.EndReceiveFrom(ar, ref this.m_EndPoint);

                byte[] data = new byte[len];
                Buffer.BlockCopy(this.m_RecvBuffer, 0, data, 0, len);
                //IPEndPoint endPoint = (IPEndPoint)this.m_CurrentChannel.RemoteEndPoint;

                this.HandleReceived(data, (IPEndPoint)this.m_EndPoint);


            }
            catch (Exception)
            {
                //TODO 处理异常
                //RaiseOtherException(so);
            }
            finally
            {
                if (this.m_CurrentChannel != null)
                    this.m_CurrentChannel.BeginReceiveFrom(this.m_RecvBuffer, 0, this.m_RecvBuffer.Length, SocketFlags.None,
                ref this.m_EndPoint, new AsyncCallback(ReceiveDataAsync), this.m_CurrentChannel);
            }
        }

        /// <summary>
        /// Adds Packet To Queue And After Send It.
        /// </summary>
        /// <param name="packet"></param>
        public virtual void SendAsync(NetPacket packet, EndPoint endPoint)
		{
			if (CoalesceSleep != -1)
			{
				Thread.Sleep(CoalesceSleep);
			}

			this.m_PacketQueue.Enqueue(packet);
			this.M_AsyncSend_Do(endPoint);
		}

		/// <summary>
		/// Calls When We Need Send Data
		/// </summary>
		private void M_AsyncSend_Do(EndPoint endPoint)
		{
			try
			{
				if (this.m_PacketQueue.Count <= 0)
				{
					return;
				}

				var packet = this.m_PacketQueue.Dequeue();
				var compiled = packet.Compile();
				//this.m_CurrentChannel.Send(compiled, compiled.Length, SocketFlags.None);
                this.m_CurrentChannel.SendTo(compiled,0, compiled.Length, SocketFlags.None, endPoint);


            }
			catch (Exception e)
			{
				Log.Info(e.ToString());
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Adds Packet To Queue And After Send It.
		/// длина не вычисляется, надо самому дописать перед пакетом
		/// </summary>
		/// <param name="packet"></param>
		public virtual void SendAsyncHex(NetPacket packet)
		{
			if (CoalesceSleep != -1)
			{
				Thread.Sleep(CoalesceSleep);
			}


			Console.ForegroundColor = ConsoleColor.Gray;
			//Log.Info(builder.ToString());
			Console.ResetColor();
		}

		/// <summary>
		/// Reading Length And Handles Data By [HandleReceived(byte[])] Without Length.
		/// </summary>
		/// <param name="e"></param>
		private void ProceedReceiving(SocketAsyncEventArgs e)
		{

			var transfered = e.BytesTransferred;
			/*if (e.SocketError != SocketError.Success || transfered <= 0)
			{
				this.DisconnectedEvent?.Invoke(this, EventArgs.Empty);
				return;
			}*/
           // IPEndPoint endPoint = (IPEndPoint)e.RemoteEndPoint;
            //var reader = new PacketReader(this.m_RecvBuffer, 0);
            //var size = reader.Size;
            //var length = reader.ReadLEUInt16() - 3;
            //ushort offset = 2;
            //this.HandleReceived(this.m_RecvBuffer, endPoint);
            /*do
			{
				byte[] data = new byte[length]; //создадим один раз
				Buffer.BlockCopy(reader.Buffer, offset, data, 0, length);


				try
				{

                    this.HandleReceived(data, endPoint); //отправляем на обработку данные пакета
				}
				catch (Exception ex)
				{
					Log.Info("Errors when parsing glued packets : {0}", ex.Message);
					//throw;
				}
				offset += (ushort)(length + 1);
				if (offset >= size) //проверяем не вышди за пределы буфера
				{
					continue;
				}
				reader.Offset = offset;
				offset += 2;
				length = (reader.ReadLEUInt16() - 3); //проверяем, есть ли еще пакет
			} while (length > 0 && offset < size);*/

            //reader.Clear();
		}

		/// <summary>
		/// Calls When Data Received From Server.
		/// </summary>
		/// <param name="data"></param>
		public abstract void HandleReceived(byte[] data, IPEndPoint endPoint);

		/// <summary>
		/// Returns Address Of Current Connection.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.m_Address;
		}

		/// <summary>
		/// Calls When Receiving Done.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void M_AsyncReceive_Completed(object sender, SocketAsyncEventArgs e)
		{
            this.m_CurrentChannel.BeginReceiveFrom(this.m_RecvBuffer, 0, this.m_RecvBuffer.Length, SocketFlags.None,
                        ref this.m_EndPoint, new AsyncCallback(ReceiveDataAsync), null);
            //this.ProceedReceiving(e);
			if (!this.m_Disposing)
			{
				this.RunReceive();
			}
		}

		#region IDisposable Support
		/// <summary>
		/// Dispose Current Listener.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>
		/// Fully Dispose Current Connection.
		/// Can Be Overriden.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// dispose managed resources
				if (this.m_CurrentChannel == null || this.m_Disposing)
				{
					return;
				}

				this.m_Disposing = true;

				try
				{
					this.m_CurrentChannel.Shutdown(SocketShutdown.Both);
				}
				catch (SocketException ex) { Log.Info(ex.ToString()); }

				try
				{
					this.m_CurrentChannel.Close();
				}
				catch (SocketException ex) { Log.Info(ex.ToString()); }

				if (this.m_RecvBuffer != null)
				{
					m_RecvBufferPool.ReleaseBuffer(this.m_RecvBuffer);
				}

				this.m_CurrentChannel.Close();
				this.m_AsyncReceive.Dispose();
				this.m_CurrentChannel = null;
				this.m_RecvBuffer = null;
				this.m_AsyncReceive = null;
				if (this.m_PacketQueue.Count <= 0)
				{
					lock (this.m_PacketQueue)
					{
						this.m_PacketQueue.Clear();
					}
				}

				this.m_PacketQueue = null;
				this.m_Disposing = false;
				this.m_Running = false;
			}
			// free native resources
		}
		#endregion
	}
}
