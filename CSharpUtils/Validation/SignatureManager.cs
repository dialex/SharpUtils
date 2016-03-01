using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SoftLife.CSharp.Validation
{
    /// <summary>
    /// Encapsulates operations with signatures. It is completely system agnostic.
    /// </summary>
    public class SignatureManager
    {
        /// <summary>
        /// Returns a modified base64 (uppercase code) string with the signature
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string signHMACSHA1(string baseString, string key)
        {
            return hexStringToBase64(signHexHMACSHA1(baseString, key));
        }

        /// <summary>
        /// HMACSHA1 signature is URL encoded so that '+' and '/' can be passed via url params. 
        /// </summary>
        /// <param name="baseString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string signHMACSHA1URLEncoded(string baseString, string key)
        {
            return UrlEncodeUpperCase(signHMACSHA1(baseString, key));
        }

        //Encodes URL with the standard HttpUtility method. However,
        //.NET encodes special characters in lower string. The standard accepts both lower and upper string, but the
        //delphi side processes it as upperstring. Thus we need to look for URL encoded characters and uppercase those.
        static string UrlEncodeUpperCase(string value)
        {
            char[] temp = HttpUtility.UrlEncode(value).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }

        //Returns an hex string e.g. "AE-EF-00-..." with the HMACSHA1 signature.
        //The hex output is the only one provided by ComputeHash
        static string signHexHMACSHA1(string baseString, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            HMACSHA1 hmac = new HMACSHA1(keyBytes);
            hmac.Initialize();

            byte[] buffer = Encoding.UTF8.GetBytes(baseString);
            string result = BitConverter.ToString(hmac.ComputeHash(buffer));
            //Console.WriteLine(result + "\n");
            return result;
        }

        //Linq reference
        static string hexStringToBase64(string hexstr)
        {
            return Convert.ToBase64String(hexstr.Split('-').Select(s => Convert.ToByte(s, 16)).ToArray());
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetTimeAsString()
        {
            string currentTime = DateTime.Now.ToString();
            return currentTime;
        }

        public static bool IsValidTime(string baseString)
        {
            DateTime now = DateTime.UtcNow;
            DateTime read;
            DateTime.TryParse(baseString, out read);

            double diffInSeconds = (now - read).TotalSeconds;
            if (diffInSeconds < 4200) //1h e 10min
                return true;
            return false;
        }

        public static bool IsValidSignature(string baseString, string key, string signature)
        {
            if (key != null)
            {
                if (SignatureManager.signHMACSHA1(baseString, key).Equals(signature))
                    return true;
            }
            return false;
        }

        public static bool IsValidSignatureURLEncoded(string baseString, string key, string signature)
        {
            if (key != null)
            {
                if (SignatureManager.signHMACSHA1URLEncoded(baseString, key).Equals(signature))
                    return true;
            }
            return false;
        }
    }
}