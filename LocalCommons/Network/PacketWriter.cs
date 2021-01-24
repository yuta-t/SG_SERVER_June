using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using LocalCommons.Cryptography;
//using LocalCommons.Utilities;

namespace LocalCommons.Network
{
    /// <summary>
    /// Provides functionality for writing primitive binary data.
    /// Author: Raphail
    /// </summary>
    public class PacketWriter : IDisposable
    {
        private static Stack<PacketWriter> m_Pool = new Stack<PacketWriter>();
        private bool m_LittleEndian;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <returns></returns>
        public static PacketWriter CreateInstance()
        {
            return CreateInstance(32, false);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">ðàçìåð â áàéòàõ</param>
        /// <param name="LittleEndian">ïîðÿäîê áàéòîâ îò ìëàäøèõ ê ñòàðøèì (ñëåâà íàïðàâî)</param>
        /// <returns></returns>
        public static PacketWriter CreateInstance(int capacity, bool LittleEndian)
        {
            PacketWriter pw = null;
            lock (m_Pool)
            {
                if (m_Pool.Count > 0)
                {
                    pw = m_Pool.Pop();

                    if (pw != null)
                    {
                        pw.m_Capacity = capacity;
                        pw.m_Stream.SetLength(0);
                    }
                }
            }
            if (pw == null)
                pw = new PacketWriter(capacity);
            pw.m_LittleEndian = LittleEndian;
            return pw;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        /// <param name="pw">óêàçàòåëü íà îáúåêò PacketWriter</param>
        public static void ReleaseInstance(PacketWriter pw)
        {
            lock (m_Pool)
            {
                if (!m_Pool.Contains(pw))
                {
                    m_Pool.Push(pw);
                }
                else
                {
                    try
                    {
                        using (StreamWriter op = new StreamWriter("neterr.log"))
                        {
                            op.WriteLine("{0}\tInstance pool contains writer", DateTime.Now);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("net error");
                    }
                }
            }
        }

        /// <summary>
        /// Internal stream which holds the entire packet.
        /// </summary>
        private MemoryStream m_Stream;
        private int m_Capacity;

        /// <summary>
        /// Internal format buffer.
        /// </summary>
        private static byte[] m_Buffer = new byte[4];

        /// <summary>
        /// Instantiates a new PacketWriter instance with the default capacity of 4 bytes.
        /// </summary>
        public PacketWriter() : this(32)
        {
        }

        /// <summary>
        /// Instantiates a new PacketWriter instance with a given capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity for the internal stream.</param>
        public PacketWriter(int capacity)
        {
            m_Stream = new MemoryStream(capacity);
            m_Capacity = capacity;
        }

        /// <summary>
        /// Writes a 1-byte boolean value to the underlying stream. False is represented by 0, true by 1.
        /// </summary>
        public void Write(bool value)
        {
            m_Stream.WriteByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        /// Writes a 1-byte unsigned integer value to the underlying stream.
        /// </summary>
        public void Write(byte value)
        {
            m_Stream.WriteByte(value);
        }

        /// <summary>
        /// Writes a 1-byte signed integer value to the underlying stream.
        /// </summary>
        public void Write(sbyte value)
        {
            m_Stream.WriteByte((byte)value);
        }

        /// <summary>
        /// Writes a 2-byte signed integer value to the underlying stream.
        /// </summary>
        public void Write(short value)
        {
            if (m_LittleEndian)
            {
                m_Buffer[1] = (byte)(value >> 8);
                m_Buffer[0] = (byte)value;
            }
            else
            {
                m_Buffer[0] = (byte)(value >> 8);
                m_Buffer[1] = (byte)value;
            }
            m_Stream.Write(m_Buffer, 0, 2);
        }

        /// <summary>
        /// Writes a 2-byte unsigned integer value to the underlying stream.
        /// </summary>
        public void Write(ushort value)
        {
            if (m_LittleEndian)
            {
                m_Buffer[1] = (byte)(value >> 8);
                m_Buffer[0] = (byte)value;
            }
            else
            {
                m_Buffer[0] = (byte)(value >> 8);
                m_Buffer[1] = (byte)value;
            }
            m_Stream.Write(m_Buffer, 0, 2);
        }

        /// <summary>
        /// Writes a 3-byte signed integer value to the underlying stream.
        /// </summary>
        /*public void Write(Uint24 value)
        {
            if (m_LittleEndian)
            {
                m_Buffer[2] = (byte)(value >> 16);
                m_Buffer[1] = (byte)(value >> 8);
                m_Buffer[0] = (byte)value;
            }
            else
            {
                m_Buffer[0] = (byte)(value >> 16);
                m_Buffer[1] = (byte)(value >> 8);
                m_Buffer[2] = (byte)value;
            }
            m_Stream.Write(m_Buffer, 0, 3);
        }*/

        /// <summary>
        /// Writes a 4-byte signed integer value to the underlying stream.
        /// </summary>
        public void Write(int value)
        {
            if (m_LittleEndian)
            {
                m_Buffer[3] = (byte)(value >> 24);
                m_Buffer[2] = (byte)(value >> 16);
                m_Buffer[1] = (byte)(value >> 8);
                m_Buffer[0] = (byte)value;
            }
            else
            {
                m_Buffer[0] = (byte)(value >> 24);
                m_Buffer[1] = (byte)(value >> 16);
                m_Buffer[2] = (byte)(value >> 8);
                m_Buffer[3] = (byte)value;
            }
            m_Stream.Write(m_Buffer, 0, 4);
        }

        /// <summary>
        /// Writes a 4-byte unsigned integer value to the underlying stream.
        /// </summary>
        public void Write(uint value)
        {

            if (m_LittleEndian)
            {
                m_Buffer[3] = (byte)(value >> 24);
                m_Buffer[2] = (byte)(value >> 16);
                m_Buffer[1] = (byte)(value >> 8);
                m_Buffer[0] = (byte)value;
            }
            else
            {
                m_Buffer[0] = (byte)(value >> 24);
                m_Buffer[1] = (byte)(value >> 16);
                m_Buffer[2] = (byte)(value >> 8);
                m_Buffer[3] = (byte)value;
            }

            m_Stream.Write(m_Buffer, 0, 4);

        }

        public void Writec(float value, Boolean c)
        {
            if (c) { };
            byte[] data = BitConverter.GetBytes(value);
            if (!m_LittleEndian)
                Array.Reverse(data);
            m_Stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Ðàçâîðà÷èâàåì áàéòû LE / BE
        /// </summary>
        /// <param name="value"> float </param>
        /// <param name="le">LE-true, BE-false</param>
        public void WriteLEBE(float value, Boolean le)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (!le)
                Array.Reverse(data);
            m_Stream.Write(data, 0, data.Length);
        }

        public void Write(float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (!m_LittleEndian)
                Array.Reverse(data);
            m_Stream.Write(data, 0, 4);
        }

        public void Write(long value)
        {
            byte[] data = BitConverter.GetBytes(value);
            if (!m_LittleEndian)
                Array.Reverse(data);
            m_Stream.Write(data, 0, 8);
        }

        /// <summary>
		/// Writes a sequence of bytes to the underlying stream
        /// Ïèøåì ìàññèâ áàéò buffer, íà÷èíàÿ ñî ñìåùåíèÿ offset â ýòîì ìàññèâà, äëèíîé size, äëèíó íå âñòàâëÿåì ïåðåä ìàññèâîì
        /// </summary>
        /// <param name="buffer">óêàçàòåëü íà áóôåð ïàêåòà</param>
        /// <param name="offset">ñìåùåíèå â áóôåðå ïàêåòà</param>
        /// <param name="size">ðàçìåð áóôåðà ïàêåòà</param>
        public void Write(byte[] buffer, int offset, int size)
        {
            m_Stream.Write(buffer, offset, size);
        }

        /// <summary>
		/// Writes a sequence of bytes to the underlying stream
        /// Ïèøåì ìàññèâ áàéò buffer, íà÷èíàÿ ñî ñìåùåíèÿ offset â ýòîì ìàññèâà, äëèíîé size ïîäñ÷èòàííîé èç ðàçìåðà áóôåðà,
        /// äëèíó size âñòàâëÿåì ïåðåä ìàññèâîì
        /// Author: NLObP
        /// </summary>
        /// <param name="buffer">óêàçàòåëü íà áóôåð ïàêåòà</param>
        /// <param name="offset">ñìåùåíèå â áóôåðå ïàêåòà</param>
        public void Write(byte[] buffer, int offset)
        {
            int size = buffer.Length;
            //Write((short)size);
            m_Stream.Write(buffer, offset, size);
        }

        /// <summary>
        /// Writes a fixed-length ASCII-encoded string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
        /// Çàïèñûâàåò ñòðîêîâîå çíà÷åíèå ñ ôèêñèðîâàííîé äëèíîé ASCII â áàçîâûé ïîòîê. ×òîáû ñîîòâåòñòâîâàòü (ðàçìåð), ñîäåðæèìîå ñòðîêè ëèáî óñåêàåòñÿ, ëèáî äîïîëíÿåòñÿ íóëåâûìè ñèìâîëàìè.
        /// Ðàçìåð íå ïèøåì ïåðåä ñòðîêîé
        /// </summary>
        public void WriteASCIIFixedNoSize(string value, int size)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.ASCII.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }
        /// <summary>
        /// Writes a fixed-length ASCII-encoded string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
        /// Çàïèñûâàåò ñòðîêîâîå çíà÷åíèå ñ ôèêñèðîâàííîé äëèíîé ASCII â áàçîâûé ïîòîê. ×òîáû ñîîòâåòñòâîâàòü (ðàçìåð), ñîäåðæèìîå ñòðîêè ëèáî óñåêàåòñÿ, ëèáî äîïîëíÿåòñÿ íóëåâûìè ñèìâîëàìè.
        /// Ðàçìåð ïèøåì ïåðåä ñòðîêîé
        /// </summary>
        public void WriteASCIIFixed(string value, int size)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            Write((short)size);
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.ASCII.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }

        public void WriteASCIIFixed_intSize(string value)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            int size = value.Length;
            Write(size);
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.ASCII.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }

        /// <summary>
        /// Writes a fixed-length UTF-encoded string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
        /// Çàïèñûâàåò ñòðîêîâîå çíà÷åíèå ñ ôèêñèðîâàííîé äëèíîé UTF-8 â áàçîâûé ïîòîê. ×òîáû ñîîòâåòñòâîâàòü (ðàçìåð), ñîäåðæèìîå ñòðîêè ëèáî óñåêàåòñÿ, ëèáî äîïîëíÿåòñÿ íóëåâûìè ñèìâîëàìè.
        /// Ðàçìåð ïèøåì ïåðåä ñòðîêîé
        /// </summary>
        /// <param name="value"></param>
        /// <param name="size"></param>
        public void WriteUTF8Fixed(string value, int size)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteUTF8Fixed() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            Write((short)size);
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.UTF8.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.UTF8.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size * 3; //çà÷åì óìíîæåíèå íà 3?!
            }
        }
        
        public void WriteUTF8NoSize(string value, int size)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteUTF8Fixed() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.UTF8.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.UTF8.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size * 3; //çà÷åì óìíîæåíèå íà 3?!
            }
        }
        public void WriteBIG5Fixed_intSize(string value)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteBIG5Fixed_intSize() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            int size = Encoding.GetEncoding("Big5").GetBytes(value).Length;
            Write(size);
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.GetEncoding("Big5").GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.GetEncoding("Big5").GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }
        public void WriteBIG5FixedWithSize(string value)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteBIG5Fixed_intSize() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            int size = Encoding.GetEncoding("Big5").GetBytes(value).Length;
            Write((byte)size);
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.GetEncoding("Big5").GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.GetEncoding("Big5").GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }
        public void WriteBIG5FixedNoSize(string value)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteBIG5Fixed_intSize() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            int size = Encoding.GetEncoding("Big5").GetBytes(value).Length;
            //Write((byte)size);
            m_Stream.SetLength(m_Stream.Length + size);
            if (length >= size)
                m_Stream.Position += Encoding.GetEncoding("Big5").GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.GetEncoding("Big5").GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }
        /// <summary>
        /// Çàïèñûâàåì ñòðîêó ïåðåâåäåííóþ èç øåñòíàäöàòåðè÷íîãî òåêñòà â áàéòû, äëèíó íå çàïèñûâàåì ñïåðåäè
        /// </summary>
        /// <param name="value"></param>
        public void WriteHex(string value)
        {
            if (value.Length % 2 != 0)
            {
                Console.Write("Network: Attempted to WriteHex() the binary key cannot have an odd number of digits");
                return;
            }
            int length = value.Length / 2;
            m_Stream.SetLength(m_Stream.Length + length);
            byte[] bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(value.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            bytes.CopyTo(m_Stream.GetBuffer(), (int)m_Stream.Position);
            m_Stream.Position += length;
        }
        /// <summary>
        /// Çàïèñûâàåì ñòðîêó ïåðåâåäåííóþ èç øåñòíàäöàòåðè÷íîãî òåêñòà â áàéòû, äëèíó çàïèñûâàåì ñïåðåäè
        /// äëèíà áåðåòñÿ èç äëèíû ñòðîêè
        /// </summary>
        /// <param name="value">ñòðîêà â âèäå HEX</param>
        /// <param name="size">ïàðàìåòð íå èñïîëüçóåòñÿ</param>
        public void WriteHex(string value, int size)
        {
            if (value.Length % 2 != 0)
            {
                Console.Write("Network: Attempted to WriteHex() the binary key cannot have an odd number of digits");
                return;
            }
            if (value.Length / 2 != size / 2)
            {
                Console.WriteLine("Network: Attempted to WriteHex(value, size) with not equ value.Length and size value");
            }
            int length = value.Length / 2;
            Write((short)length);
            m_Stream.SetLength(m_Stream.Length + length);
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = byte.Parse(value.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            bytes.CopyTo(m_Stream.GetBuffer(), (int)m_Stream.Position);
            m_Stream.Position += length;
        }

        /// <summary>
        /// Writes a dynamic-length ASCII-encoded string value to the underlying stream, followed by a 1-byte null character.
        /// </summary>
        public void WriteDynamicASCII(string value)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteAsciiNull() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            m_Stream.SetLength(m_Stream.Length + length + 1);
            Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
            m_Stream.Position += length;
        }

        /// <summary>
        /// Writes a dynamic-length little-endian unicode string value to the underlying stream, followed by a 2-byte null character.
        /// </summary>

        public void WriteDynamicLittleUni(string value)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteLittleUniNull() with null value");
                value = String.Empty;
            }

            int length = value.Length;
            m_Stream.SetLength(m_Stream.Length + ((length + 1) * 2));

            m_Stream.Position += Encoding.Unicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
            m_Stream.Position += 2;
        }

        /// <summary>
        /// Writes a fixed-length little-endian unicode string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
        /// </summary>

        public void WriteFixedLittleEndian(string value, int size)
        {

            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteLittleUniFixed() with null value");
                value = String.Empty;

            }
            size *= 2;

            int length = value.Length;
            Write((short)length);
            m_Stream.SetLength(m_Stream.Length + size);

            if ((length * 2) >= size)
                m_Stream.Position += Encoding.Unicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.Unicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }

        /// <summary>
        /// Writes a dynamic-length big-endian unicode string value to the underlying stream, followed by a 2-byte null character.
        /// </summary>
        public void WriteDynamicBigUnicode(string value)
        {

            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteBigUniNull() with null value");
                value = String.Empty;
            }
            int length = value.Length;
            Write((short)length);
            m_Stream.SetLength(m_Stream.Length + ((length + 1) * 2));

            m_Stream.Position += Encoding.BigEndianUnicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
            m_Stream.Position += 2;
        }

        /// <summary>
        /// Writes a fixed-length big-endian unicode string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
        /// </summary>
        public void WriteFixedBigUnicode(string value, int size)
        {
            if (value == null)
            {
                Console.WriteLine("Network: Attempted to WriteBigUniFixed() with null value");
                value = String.Empty;
            }
            size *= 2;
            int length = value.Length;
            Write((short)length);
            m_Stream.SetLength(m_Stream.Length + size);

            if ((length * 2) >= size)
                m_Stream.Position += Encoding.BigEndianUnicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
            else
            {
                Encoding.BigEndianUnicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
                m_Stream.Position += size;
            }
        }

        /// <summary>
        /// Fills the stream from the current position up to (capacity) with 0x00's
        /// </summary>

        public void Fill()
        {
            Fill((int)(m_Capacity - m_Stream.Length));
        }

        /// <summary>
        /// Writes a number of 0x00 byte values to the underlying stream.
        /// </summary>

        public void Fill(int length)
        {
            if (m_Stream.Position == m_Stream.Length)
            {
                m_Stream.SetLength(m_Stream.Length + length);
                m_Stream.Seek(0, SeekOrigin.End);
            }
            else
            {
                m_Stream.Write(new byte[length], 0, length);
            }
        }
        /// <summary>
        /// Gets the total stream length.
        /// </summary>
        public long Length
        {
            get
            {
                return m_Stream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the current stream position.
        /// </summary>
        public long Position
        {
            get
            {
                return m_Stream.Position;
            }
            set
            {
                m_Stream.Position = value;
            }
        }

        /// <summary>
        /// The internal stream used by this PacketWriter instance.
        /// </summary>
        public MemoryStream UnderlyingStream
        {
            get
            {
                return m_Stream;
            }
        }

        /// <summary>
        /// Offsets the current position from an origin.
        /// </summary>
        public long Seek(long offset, SeekOrigin origin)
        {
            return m_Stream.Seek(offset, origin);
        }

        /// <summary>
        /// Gets the entire stream content as a byte array.
        /// </summary>

        public byte[] ToArray()
        {
            return m_Stream.ToArray();
        }

        #region IDisposable Support
        private bool disposedValue = false; // Äëÿ îïðåäåëåíèÿ èçáûòî÷íûõ âûçîâîâ

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: îñâîáîäèòü óïðàâëÿåìîå ñîñòîÿíèå (óïðàâëÿåìûå îáúåêòû).
                    m_Stream.Dispose();
                }

                // TODO: îñâîáîäèòü íåóïðàâëÿåìûå ðåñóðñû (íåóïðàâëÿåìûå îáúåêòû) è ïåðåîïðåäåëèòü íèæå ìåòîä çàâåðøåíèÿ.
                // TODO: çàäàòü áîëüøèì ïîëÿì çíà÷åíèå NULL.

                disposedValue = true;
            }
        }

        // TODO: ïåðåîïðåäåëèòü ìåòîä çàâåðøåíèÿ, òîëüêî åñëè Dispose(bool disposing) âûøå âêëþ÷àåò êîä äëÿ îñâîáîæäåíèÿ íåóïðàâëÿåìûõ ðåñóðñîâ.
        // ~PacketWriter() {
        //   // Íå èçìåíÿéòå ýòîò êîä. Ðàçìåñòèòå êîä î÷èñòêè âûøå, â ìåòîäå Dispose(bool disposing).
        //   Dispose(false);
        // }

        // Ýòîò êîä äîáàâëåí äëÿ ïðàâèëüíîé ðåàëèçàöèè øàáëîíà âûñâîáîæäàåìîãî êëàññà.
        public void Dispose()
        {
            // Íå èçìåíÿéòå ýòîò êîä. Ðàçìåñòèòå êîä î÷èñòêè âûøå, â ìåòîäå Dispose(bool disposing).
            Dispose(true);
            // TODO: ðàñêîììåíòèðîâàòü ñëåäóþùóþ ñòðîêó, åñëè ìåòîä çàâåðøåíèÿ ïåðåîïðåäåëåí âûøå.
            GC.SuppressFinalize(this);
        }
        #endregion

    }

}