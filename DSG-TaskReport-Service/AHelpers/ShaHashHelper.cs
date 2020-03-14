using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// SHA1 散列算法
    /// </summary>
    public class ShaHashHelper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <returns></returns>
        public static string GetShaHash(string input)
        {
            byte[] inputBytes = Encoding.Default.GetBytes(input);

            //  SHA1 sha = new SHA1CryptoServiceProvider();
            var sha = new SHA256CryptoServiceProvider();

            byte[] result = sha.ComputeHash(inputBytes);

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {
                sBuilder.Append(result[i].ToString("x2"));
            }

            return sBuilder.ToString().ToUpper();
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="input"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool VerifyShaHash(string input, string hash)
        {
            string hashOfInput = GetShaHash(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return 0 == comparer.Compare(hashOfInput, hash);
        }

    }
}
