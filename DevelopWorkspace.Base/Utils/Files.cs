using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DevelopWorkspace.Base.Utils
{
    public class Files
    {
        public static string ReadAllText(string fileName, Encoding encode)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            string fileContents;
            using (StreamReader reader = new StreamReader(fileStream, encode))
            {
                fileContents = reader.ReadToEnd();
            }
            return fileContents;
        }
        public static string GetSha256Hash(string source) {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                string hash = GetHash(sha256Hash, source);
                return hash;
            }
        }
        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GetHash(hashAlgorithm, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
    /// <summary>
    /// 文字コードに関するクラス
    ///    byte[] bs = System.IO.File.ReadAllBytes(@"D:\Users\os-jiangjiang.xu\Downloads\sample.csv");
    ///    CharCode.Detect(bs).Dump();
    /// </summary>
    public static class CharCode
    {
        /// <summary>
        /// ASCII
        /// </summary>
        public static System.Text.Encoding ASCII
        {
            get
            {
                return System.Text.Encoding.ASCII;
            }
        }

        /// <summary>
        /// EUC-JP
        /// </summary>
        public static System.Text.Encoding EUCJP
        {
            get
            {
                return System.Text.Encoding.GetEncoding("EUC-JP");
            }
        }

        /// <summary>
        /// JIS
        /// </summary>
        public static System.Text.Encoding JIS
        {
            get
            {
                return System.Text.Encoding.GetEncoding("iso-2022-jp");
            }
        }

        /// <summary>
        /// Shift_JIS
        /// </summary>
        public static System.Text.Encoding SJIS
        {
            get
            {
                return System.Text.Encoding.GetEncoding("Shift_JIS");
            }
        }

        /// <summary>
        /// UTF-7
        /// </summary>
        public static System.Text.Encoding UTF7
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-7");
            }
        }

        /// <summary>
        /// UTF-8
        /// </summary>
        public static System.Text.Encoding UTF8
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-8");
            }
        }

        /// <summary>
        /// UTF-8 (without BOM)
        /// </summary>
        public static System.Text.Encoding UTF8N
        {
            get
            {
                return new System.Text.UTF8Encoding(false);
            }
        }

        /// <summary>
        /// UTF-16
        /// </summary>
        public static System.Text.Encoding UTF16
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-16");
            }
        }

        /// <summary>
        /// UTF-16 Big-Endian
        /// </summary>
        public static System.Text.Encoding UTF16B
        {
            get
            {
                return System.Text.Encoding.GetEncoding("unicodeFFFE");
            }
        }

        /// <summary>
        /// UTF-16 (without BOM)
        /// </summary>
        public static System.Text.Encoding UTF16LE
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-16LE");
            }
        }

        /// <summary>
        /// UTF-16 Big-Endian (without BOM)
        /// </summary>
        public static System.Text.Encoding UTF16BE
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-16BE");
            }
        }

        /// <summary>
        /// UTF-32
        /// </summary>
        public static System.Text.Encoding UTF32
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-32");
            }
        }

        /// <summary>
        /// UTF-32 Big-Endian
        /// </summary>
        public static System.Text.Encoding UTF32B
        {
            get
            {
                return System.Text.Encoding.GetEncoding("UTF-32BE");
            }
        }

        /// <summary>
        /// バイト配列から文字コードを判別します。
        /// </summary>
        /// <param name="srcBytes">文字コードを判別するバイト配列</param>
        /// <returns><paramref name="srcBytes"/> から予想される文字コード</returns>
        public static System.Text.Encoding[] Detect(byte[] srcBytes)
        {
            if (srcBytes == null || srcBytes.Length <= 0)
            {
                return null;
            }

            System.Collections.Generic.List<System.Text.Encoding> dstEncodings = new System.Collections.Generic.List<System.Text.Encoding>();

            if (DetectEncoding(srcBytes, ASCII))
            {
                dstEncodings.Add(ASCII);
            }

            if (DetectEncoding(srcBytes, EUCJP))
            {
                dstEncodings.Add(EUCJP);
            }

            if (DetectEncoding(srcBytes, JIS))
            {
                dstEncodings.Add(JIS);
            }

            if (DetectEncoding(srcBytes, SJIS))
            {
                dstEncodings.Add(SJIS);
            }

            if (DetectEncoding(srcBytes, UTF7))
            {
                dstEncodings.Add(UTF7);
            }

            if (DetectBOM(srcBytes, 0xef, 0xbb, 0xbf) && DetectEncoding(srcBytes, UTF8))
            {
                dstEncodings.Add(UTF8);
            }
            else if (DetectEncoding(srcBytes, UTF8N))
            {
                dstEncodings.Add(UTF8N);
            }

            if (DetectBOM(srcBytes, 0xff, 0xfe) && DetectEncoding(srcBytes, UTF16))
            {
                dstEncodings.Add(UTF16);
            }
            else if (DetectBOM(srcBytes, 0xfe, 0xff) && DetectEncoding(srcBytes, UTF16B))
            {
                dstEncodings.Add(UTF16B);
            }

            if (DetectBOM(srcBytes, 0xff, 0xfe, 0x00, 0x00) && DetectEncoding(srcBytes, UTF32))
            {
                dstEncodings.Add(UTF32);
            }
            else if (DetectBOM(srcBytes, 0x00, 0x00, 0xfe, 0xff) && DetectEncoding(srcBytes, UTF32B))
            {
                dstEncodings.Add(UTF32B);
            }

            if (srcBytes.Length >= 2 && srcBytes.Length % 2 == 0)
            {
                if (srcBytes[0] == 0x00)
                {
                    dstEncodings.Add(UTF16BE);

                    for (int i = 0; i < srcBytes.Length; i += 2)
                    {
                        if (srcBytes[i] != 0x00 || srcBytes[i + 1] < 0x06 || srcBytes[i + 1] >= 0x7f)
                        {
                            dstEncodings.Remove(UTF16BE);
                            break;
                        }
                    }
                }
                else if (srcBytes[1] == 0x00)
                {
                    dstEncodings.Add(UTF16LE);

                    for (int i = 0; i < srcBytes.Length; i += 2)
                    {
                        if (srcBytes[i] < 0x06 || srcBytes[i] >= 0x7f || srcBytes[i + 1] != 0x00)
                        {
                            dstEncodings.Remove(UTF16LE);
                            break;
                        }
                    }
                }
            }

            return dstEncodings.ToArray();
        }

        /// <summary>
        /// BOM から文字コードを判定します。
        /// </summary>
        /// <param name="srcBytes">文字コードを判別するバイト配列</param>
        /// <param name="bom">バイトオーダーマーク</param>
        /// <returns><paramref name="srcBytes"/> から文字コードが判定できた場合は true</returns>
        private static bool DetectBOM(byte[] srcBytes, params byte[] bom)
        {
            if (srcBytes.Length < bom.Length)
            {
                return false;
            }

            for (int i = 0; i < bom.Length; i++)
            {
                if (srcBytes[i] != bom[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// バイト配列から文字列、文字列からバイト配列に変換し、文字コードを判別します。
        /// </summary>
        /// <param name="srcBytes">文字コードを判別するバイト配列</param>
        /// <param name="encoding">変換する文字コード</param>
        /// <returns><paramref name="srcBytes"/> から文字コードが判定できた場合は true</returns>
        private static bool DetectEncoding(byte[] srcBytes, System.Text.Encoding encoding)
        {
            string encodedStr = encoding.GetString(srcBytes);
            byte[] encodedByte = encoding.GetBytes(encodedStr);

            if (srcBytes.Length != encodedByte.Length)
            {
                return false;
            }

            for (int i = 0; i < srcBytes.Length; i++)
            {
                if (!srcBytes[i].Equals(encodedByte[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

}
