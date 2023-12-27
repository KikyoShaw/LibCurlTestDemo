using System.Security.Cryptography;
using System.Text;

namespace LibCurlDemo
{
    public class Helper
    {
        public static string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2")); // 将每个字节转换为两位的十六进制字符串
            return sb.ToString();
        }
    }
}
