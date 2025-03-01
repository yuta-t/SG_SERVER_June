using LocalCommons.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace LocalCommons.Cryptography
{
	public class Encrypt
	{
        [DllImport("trdecrypt.dll", EntryPoint = "encryptkey", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr encryptkey(byte[] client_pubkeych, byte[] inkey);

        public static byte[] LoginGenKey(byte[] client_publickey, byte[] packet_key)
        {
            byte[] returnarray = new byte[257];
            IntPtr encrypt = encryptkey(client_publickey, packet_key);
            Marshal.Copy(encrypt, returnarray, 0, 257);

            /*var mahByteArray = new List<byte>();
            mahByteArray.AddRange(returnarray);
            mahByteArray.Insert(0, 0x12);
            mahByteArray.Insert(0, 0x01);
            mahByteArray.Insert(0, 0x04);*/

            return returnarray;
        }

        /*public static byte[] EncryptByte(byte[] inkey, byte[] src)
        {
            Stream srcstream = new MemoryStream(src);
            MemoryStream fsEncrypted = new MemoryStream();

            int filesize = src.Length;
            int xorsize = filesize % 16;
            int encryptsize;
            byte[] returnarray;

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.Key = inkey;
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;

                if (xorsize > 0)
                {
                    encryptsize = filesize - xorsize;

                    using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[encryptsize];
                        srcstream.Read(bytearrayinput, 0, encryptsize);
                        cs.Write(bytearrayinput, 0, encryptsize); ;
                        //byte[] remainbyte = new byte[aessize];
                        byte[] xorData;
                        srcstream.Seek(encryptsize, SeekOrigin.Begin);
                        xorData = xorbyte(ReadFully(srcstream, xorsize), xorsize);
                        fsEncrypted.Seek(encryptsize, SeekOrigin.Begin);
                        fsEncrypted.Write(xorData, 0, xorData.Length);
                        returnarray = fsEncrypted.ToArray();
                        cs.Close();
                        srcstream.Close();
                        fsEncrypted.Close();
                    }
                }
                else //if(xorsize == 0 && filesize > 16)
                {
                    using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[srcstream.Length];
                        srcstream.Read(bytearrayinput, 0, bytearrayinput.Length);
                        cs.Write(bytearrayinput, 0, bytearrayinput.Length);
                        returnarray = fsEncrypted.ToArray();
                        cs.Close();
                        srcstream.Flush();
                        srcstream.Close();
                        //fsEncrypted.Close();
                    }
                }
                return returnarray;

            }

        }*/

        public static byte[] newEncryptByte(byte[] src)
        {
            /***Strugarden Code Block***/
            // Console.WriteLine("Send: {0}", Utility.ByteArrayToString(src));
            // Console.WriteLine(" ");
            //Console.WriteLine("Length of New Src: {0}", src.Length);
            int filledZero = 0;
            int count = 0;
            int encCount = 0;
            int i, j, k, l, m;
            var key = Encoding.ASCII.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-");

            if (src.Length % 3 == 0)
            {
                filledZero = 0;
            }
            else if (src.Length % 3 == 1)
            {
                filledZero = 2;
            }
            else if (src.Length % 3 == 2)
            {
                filledZero = 1;
            }
            byte[] newSrc = new byte[src.Length + filledZero];
            src.CopyTo(newSrc, 0);
            byte[] returnarray = new byte[(newSrc.Length/3) * 4];
            newSrc = XorKey(newSrc, newSrc.Length);

            for (j = 0; j < newSrc.Length / 3; j++)
            {
                byte[] inChar = new byte[3];
                byte[] outChar = new byte[4];
                byte[] tempChar = new byte[3];

                for (k = 0; k < 3; k++)
                {
                    inChar[k] = newSrc[count + k];
                }
                
                tempChar[0] = (byte)((Convert.ToInt32(inChar[1]) & 0xF) & 0xF);
                tempChar[1] = (byte)(tempChar[0] + tempChar[0]);
                tempChar[2] = (byte)(tempChar[1] + tempChar[1]);
                outChar[0] = (byte)(Convert.ToInt32(inChar[0]) >> 2);
                outChar[1] = (byte)(((Convert.ToInt32(inChar[0]) & 0x3) << 4) | (Convert.ToInt32(inChar[1]) >> 4));
                outChar[2] = (byte)(Convert.ToInt32(tempChar[2]) | (Convert.ToInt32(inChar[2]) >> 6));
                outChar[3] = (byte)(Convert.ToInt32(inChar[2]) & 0x3F);
                
                for (i = 0; i < 4; i++)
                {
                    outChar[i] = key[outChar[i]];
                }
                
                for (l = 0; l < 4; l++)
                {
                    returnarray[encCount + l] = outChar[l];
                }
                
                count = count + 3;
                encCount = encCount + 4;
            }
            // Console.WriteLine("CompliedSend:{0}", Utility.ByteArrayToString(returnarray));
            // Console.WriteLine(" ");
            return returnarray;
            /*Stream srcstream = new MemoryStream(src);
            MemoryStream fsEncrypted = new MemoryStream();

            int filesize = src.Length;
            int xorsize = filesize % 16;
            int encryptsize;
            byte[] returnarray;

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.Key = inkey;
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;

                if (xorsize > 0)
                {
                    encryptsize = filesize - xorsize;

                    using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[encryptsize];
                        srcstream.Read(bytearrayinput, 0, encryptsize);
                        cs.Write(bytearrayinput, 0, encryptsize); ;
                        //byte[] remainbyte = new byte[aessize];
                        byte[] xorData;
                        srcstream.Seek(encryptsize, SeekOrigin.Begin);
                        xorData = xorbyte(ReadFully(srcstream, xorsize), xorkey, xorsize);
                        fsEncrypted.Seek(encryptsize, SeekOrigin.Begin);
                        fsEncrypted.Write(xorData, 0, xorData.Length);
                        returnarray = fsEncrypted.ToArray();
                        cs.Close();
                        srcstream.Close();
                        fsEncrypted.Close();
                    }
                }
                else //if(xorsize == 0 && filesize > 16)
                {
                    using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[srcstream.Length];
                        srcstream.Read(bytearrayinput, 0, bytearrayinput.Length);
                        cs.Write(bytearrayinput, 0, bytearrayinput.Length);
                        returnarray = fsEncrypted.ToArray();
                        cs.Close();
                        srcstream.Flush();
                        srcstream.Close();
                        //fsEncrypted.Close();
                    }
                }
                return returnarray;

            }
            */
        }

        public static byte[] EncryptKey(byte[] inkey, byte[] src)
        {
            Stream srcstream = new MemoryStream(src);
            MemoryStream fsEncrypted = new MemoryStream();

            byte[] returnarray;

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.Key = inkey;
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;

                using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] bytearrayinput = new byte[srcstream.Length];
                    srcstream.Read(bytearrayinput, 0, bytearrayinput.Length);
                    cs.Write(bytearrayinput, 0, bytearrayinput.Length);
                    returnarray = fsEncrypted.ToArray();
                    cs.Close();
                    srcstream.Flush();
                    srcstream.Close();
                    //fsEncrypted.Close();
                }
                return returnarray;

            }

        }

        /*public static MemoryStream EncryptByte2(byte[] inkey, byte[] src)
        {
            Stream srcstream = new MemoryStream(src);
            MemoryStream fsEncrypted = new MemoryStream();

            int filesize = src.Length;
            int xorsize = filesize % 16;
            int encryptsize;
            byte[] returnarray;

            using (RijndaelManaged AES = new RijndaelManaged())
            {
                AES.Key = inkey;
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;

                if (xorsize > 0)
                {
                    encryptsize = filesize - xorsize;

                    using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[encryptsize];
                        srcstream.Read(bytearrayinput, 0, encryptsize);
                        cs.Write(bytearrayinput, 0, encryptsize); ;
                        //byte[] remainbyte = new byte[aessize];
                        byte[] xorData;
                        srcstream.Seek(encryptsize, SeekOrigin.Begin);
                        xorData = xorbyte(ReadFully(srcstream, xorsize), xorsize);
                        fsEncrypted.Seek(encryptsize, SeekOrigin.Begin);
                        fsEncrypted.Write(xorData, 0, xorData.Length);
                        returnarray = fsEncrypted.ToArray();
                        cs.Close();
                        srcstream.Close();
                        fsEncrypted.Close();
                    }
                }
                else //if(xorsize == 0 && filesize > 16)
                {
                    using (var cs = new CryptoStream(fsEncrypted, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[srcstream.Length];
                        srcstream.Read(bytearrayinput, 0, bytearrayinput.Length);
                        cs.Write(bytearrayinput, 0, bytearrayinput.Length);
                       // returnarray = fsEncrypted.ToArray();
                        cs.Close();
                        srcstream.Flush();
                        srcstream.Close();
                        //fsEncrypted.Close();
                    }
                }
                return fsEncrypted;

            }

        }*/
        public static byte[] XorKey(byte[] src, int length)
        {
            int i, j;
            var key = Encoding.ASCII.GetBytes("Ixg2HGf1-Feu+Vdt9Ucs8Tbr7Saq6RZp5QYo4PXn3OWm0NElzMDkyLCjwKAivJBh");

            j = 0;
            for(i = 0; i < length; i++)
            {
                if(j >= 64)
                {
                    j = 0;
                }
                else
                {
                    src[i] = (byte)(src[i] ^ key[j]);
                    j++;
                }
            }
            return src;
        }
        public static byte[] DecryptByte(byte[] src)
        {
            byte[] inChar = new byte[4];
            byte[] outChar = new byte[3];
            int filledZero = 0;
            int count = 0;
            int deCount = 0;
            int i, j, k, l, m;

            Console.WriteLine("Original Src: {0}", Utility.ByteArrayToString(src));
            if (src.Length % 4 == 0)
            {
                filledZero = 0;
            }
            else if (src.Length % 4 == 1)
            {
                filledZero = 3;
            }
            else if(src.Length % 4 == 2)
            {
                filledZero = 2;
            }
            else if(src.Length % 4 == 3)
            {
                filledZero = 1;
            }
            byte[] newSrc = new byte[src.Length + filledZero];
            byte[] returnarray = new byte[(src.Length + filledZero) / 4 * 3];
            src.CopyTo(newSrc, 0);
            for (j = 0; j < (newSrc.Length) / 4; j++)
            {
                //Console.WriteLine("Total Run: {0}", j);
                for (k = 0; k < 4; k++)
                {
                    inChar[k] = newSrc[count + k];
                }

                //Console.WriteLine("OrigChar: {0}", Utility.ByteArrayToString(inChar));
                //Console.WriteLine(" ");
                for (i = 0; i < 4; i++)
                {
                    if (inChar[i] >= 'A' && inChar[i] <= 'Z')
                    {
                        int ByteToInt = Convert.ToInt32(inChar[i]);
                        inChar[i] = (byte)(ByteToInt - 65);
                    }
                    else if (inChar[i] >= 'a' && inChar[i] <= 'z')
                    {
                        int ByteToInt = Convert.ToInt32(inChar[i]);
                        inChar[i] = (byte)(ByteToInt - 97 + 26);
                    }
                    else if (inChar[i] >= '0' && inChar[i] <= '9')
                    {
                        int ByteToInt = Convert.ToInt32(inChar[i]);
                        inChar[i] = (byte)(ByteToInt - 48 + 52);
                    }
                    else if (inChar[i] == '+')
                    {
                        inChar[i] = 0x3E;
                    }
                    else
                    {
                        inChar[i] = 0x3F;
                    }
                }

                outChar[0] = (byte)((Convert.ToInt32(inChar[0]) << 2) | ((Convert.ToInt32(inChar[1]) & 0x30) >> 4));
                outChar[1] = (byte)((Convert.ToInt32(inChar[1]) << 4) | ((Convert.ToInt32(inChar[2]) >> 2) & 0x0F));
                outChar[2] = (byte)((Convert.ToInt32(inChar[2]) << 6) | Convert.ToInt32(inChar[3]));

                for (l = 0; l < 3; l++)
                {
                    returnarray[deCount + l] = outChar[l];
                }
                count = count + 4;
                deCount = deCount + 3;
                //Console.WriteLine("InChar: {0}", Utility.ByteArrayToString(inChar));
                //Console.WriteLine(" ");
                //Console.WriteLine("OutChar: {0}", Utility.ByteArrayToString(outChar));
                //Console.WriteLine(" ");

            }
            //Console.WriteLine("Total Counter: {0}", j);
            //Console.WriteLine("Length of Src: {0}", src.Length);
            //Console.WriteLine("Length of New Src: {0}", newSrc.Length);
            Console.WriteLine(" ");
            Console.WriteLine("New Src: {0}", Utility.ByteArrayToString(newSrc));
            Console.WriteLine(" ");
            //Console.WriteLine("{0}", Utility.ByteArrayToString(returnarray));
            Console.WriteLine(" ");
            returnarray = XorKey(returnarray, returnarray.Length);
            //***Display Receive Packet***
            Console.WriteLine("ReturnAray:{0}", Utility.ByteArrayToString(returnarray));
            Console.WriteLine(" ");
            return returnarray;
            /*Stream srcstream = new MemoryStream(src);
            MemoryStream fsDecrypted = new MemoryStream();

            int filesize = src.Length;
            int xorsize = filesize % 16;
            int decryptsize = new int();
            byte[] returnarray;

            using (RijndaelManaged AES = new RijndaelManaged())
            {

                AES.Key = inkey;
                AES.Mode = CipherMode.ECB;
                AES.Padding = PaddingMode.None;

                if (xorsize > 0)
                {
                    decryptsize = filesize - xorsize;

                    using (var cs = new CryptoStream(fsDecrypted, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        //int decryptsize = filesize - aessize;
                        byte[] bytearrayinput = new byte[decryptsize];
                        srcstream.Read(bytearrayinput, 0, decryptsize);
                        cs.Write(bytearrayinput, 0, decryptsize);
                        byte[] xorData;
                        srcstream.Seek(decryptsize, SeekOrigin.Begin);
                        xorData = xorbyte(ReadFully(srcstream, xorsize), xorkey, xorsize);
                        fsDecrypted.Seek(decryptsize, SeekOrigin.Begin);
                        fsDecrypted.Write(xorData, 0, xorData.Length);
                        returnarray = fsDecrypted.ToArray();
                        cs.Close();
                        srcstream.Close();
                        fsDecrypted.Close();
                    }
                }
                else
                {
                    using (var cs = new CryptoStream(fsDecrypted, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        byte[] bytearrayinput = new byte[srcstream.Length];
                        srcstream.Read(bytearrayinput, 0, bytearrayinput.Length);
                        cs.Write(bytearrayinput, 0, bytearrayinput.Length);
                        returnarray = fsDecrypted.ToArray();
                        cs.Close();
                        srcstream.Close();
                        fsDecrypted.Close();
                    }
                }
                return returnarray;
            }
            */
        }

        private static byte[] xorbyte(byte[] input, byte[] xorkey, int size)
        {
            //byte[] xorkey = { 0x08, 0x3a, 0x8b, 0x04, 0x4c, 0x3a, 0xe0, 0x89, 0xeb, 0x82, 0xa9, 0xbc, 0xd4, 0x24, 0xe6, 0x4e };
            //byte[] xor = HexToByteArray(inkey);
            int i;
            for (i = 0; i < size; i++)
            {
                input[i] ^= xorkey[i];
                //return input;
            }
            return input;
        }

        private static byte[] HexToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static byte[] ReadFully(Stream input, int aessize)
        {
            byte[] buffer = new byte[aessize];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        public static byte[] encodedHairClump(byte[] clump)
        {
            //Console.WriteLine(BitConverter.ToInt32(User.CharacterDecodedHairClump1, 0));
            //Console.WriteLine(" ");
            byte[] cal = new byte[5];
            int result = 0;
            int src = 0;
            int remainder = 0;

            if (clump[0] == 0xFF)
            {
                byte[] noCal = new byte[1];
                noCal[0] = 0x00;
                return noCal;
            }
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    src = BitConverter.ToInt32(clump, 0);
                    cal[4] = (byte)(src & 0x7F);
                    result = src - cal[4];
                }
                else if (i == 1)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[3] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 2)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[2] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 3)
                {
                    src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[1] = (byte)(remainder + 0x80);
                    result = src - remainder;
                }
                else if (i == 4)
                {
                    cal[0] = 0x80;
                    /*src = result >> 7;
                    remainder = (byte)(src & 0x7F);
                    cal[0] = (byte)(remainder + 0x80);*/
                }
            }
            //Console.WriteLine("Encoded Hair Clump: {0}", Utility.ByteArrayToString(cal));
            //Console.WriteLine(" ");
            return cal;
        }
        public static int decodedDynamicBytes(byte[] src)
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
        public static byte[] encodeMultiBytes(int src)
        {
            int result = 0;
            int remainder = 0;

            if (src > 268435455)
            {
                byte[] cal = new byte[5];
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0)
                    {
                        cal[4] = (byte)(src & 0x7F);
                        result = src - cal[4];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[3] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 2)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[2] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 3)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[1] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 4)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else if (src > 2097151)
            {
                byte[] cal = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    if (i == 0)
                    {
                        cal[3] = (byte)(src & 0x7F);
                        result = src - cal[3];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[2] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 2)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[1] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 3)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else if (src > 16383)
            {
                byte[] cal = new byte[3];
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        cal[2] = (byte)(src & 0x7F);
                        result = src - cal[2];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[1] = (byte)(remainder + 0x80);
                        result = src - remainder;
                    }
                    else if (i == 2)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else if (src > 127)
            {
                byte[] cal = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        cal[1] = (byte)(src & 0x7F);
                        result = src - cal[1];
                    }
                    else if (i == 1)
                    {
                        src = result >> 7;
                        remainder = (byte)(src & 0x7F);
                        cal[0] = (byte)(remainder + 0x80);
                    }
                }
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
            else
            {
                byte[] cal = new byte[1];
                cal[0] = (byte)src;
                //Console.WriteLine("encodeMultiBytes: {0}", Utility.ByteArrayToString(cal));
                //Console.WriteLine(" ");
                return cal;
            }
        }
    }
}
