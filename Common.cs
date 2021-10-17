using System;
using System.Collections.Generic;

namespace Hybs
{
    /// <summary>
    /// service region of hayabusa cloud
    /// </summary>
    public enum Region {
        /// <summary>
        /// Northeast Asia, Japan, Tokyo
        /// </summary>
        JP,
    }

    /// <summary>
    /// environment of hayabusa cloud
    /// </summary>
    public enum Environment
    {
        /// <summary>
        /// production environment
        /// </summary>
        Production,
        /// <summary>
        /// playground environment for trialing
        /// </summary>
        Playground,
    }

    /// <summary>
    /// authentication information
    /// </summary>
    [Serializable]
    public class Credential
    {
        public static Credential FromBase64String(string token)
        {
            var cred = new Credential();
            cred.accessKeys = new List<string>();
            cred.token = System.Text.Encoding.UTF8.GetBytes(token);
            while (token != "")
            {
                bool ok = DecodeKeyPair(token, out string key, out string secret);
                token = secret;
                if (ok)
                {
                    cred.accessKeys.Add(key);
                }
            }
            return cred;
        }

        public byte[] Base64Token { get => token; }
        public string Provider
        {
            get => accessKeys.Count > 0 ? accessKeys[0] : "";
        }
        public string Developer
        {
            get => accessKeys.Count > 1 ? accessKeys[1] : "";
        }
        public string AppName
        {
            get => accessKeys.Count > 2 ? accessKeys[2] : "";
        }
        public string UserId
        {
            get => accessKeys.Count > 3 ? accessKeys[3] : "";
        }

        public string AppId
        {
            get
            {
                if (Provider == "" || Developer == "" || AppName == "")
                {
                    return "";
                }
                string str = string.Format("{0}:{1}:{2}", Provider, Developer, AppName);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(bytes);
            }
        }

        private static bool DecodeKeyPair(string s, out string key, out string secret)
        {
            byte[] decoded = Convert.FromBase64String(s);
            for (int i = 0; i < decoded.Length; i++)
            {
                if (!decoded[i].Equals(0x3a))
                {
                    continue;
                }
                byte[] kBytes = new byte[i];
                Array.Copy(decoded, kBytes, i);
                key = System.Text.Encoding.UTF8.GetString(kBytes);
                if (i<decoded.Length-1)
                {
                    byte[] sBytes = new byte[decoded.Length - i - 1];
                    Array.Copy(decoded, i + 1, sBytes, 0, sBytes.Length);
                    secret = System.Text.Encoding.UTF8.GetString(sBytes);
                    return true;
                } else
                {
                    secret = "";
                    return false;
                }
            }
            key = System.Text.Encoding.UTF8.GetString(decoded);
            secret = "";
            return false;
        }

        public List<string> accessKeys;
        public byte[] token;
    }
}
