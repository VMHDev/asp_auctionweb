using System.Security.Cryptography;

namespace ASP_Project_DHS.Helpers
{
    public class StringUtils
    {
        public static string GetMD5(string strInput)
        {
            string str_md5 = "";
            byte[] mang = System.Text.Encoding.UTF8.GetBytes(strInput);
            MD5CryptoServiceProvider my_md5 = new MD5CryptoServiceProvider();
            mang = my_md5.ComputeHash(mang);

            foreach (byte b in mang)
            {
                str_md5 += b.ToString("x2");
            }
            return str_md5;
        }
    }
}