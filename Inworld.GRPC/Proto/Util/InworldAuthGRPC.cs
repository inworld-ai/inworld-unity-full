using Ai.Inworld.Studio.V1Alpha;
using Inworld.Grpc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Inworld.Runtime
{
    public class InworldAuthGRPC
    {
        const string k_Method = "ai.inworld.engine.WorldEngine/GenerateToken";
        const string k_RequestHead = "IW1-HMAC-SHA256";
        const string k_RequestTail = "iw1_request";
        SessionAccessToken m_AccessToken;

        string _CurrentUtcTime => DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        public DateTime ExpireTime => m_AccessToken != null ? m_AccessToken.ExpirationTime.ToDateTime() : DateTime.MinValue;
        public AccessToken Token { get; set; }
        public string SessionID => m_AccessToken?.SessionId;
        public bool IsExpired => DateTime.UtcNow > ExpireTime;
        static string Nonce
        {
            get
            {
                string nonce = "";
                for (int index = 0; index < 11; ++index)
                    nonce += Random.Range(0, 10).ToString();
                return nonce;
            }
        }

        string _GenerateSignature(List<string> strings, string strSecret)
        {
            HMACSHA256 hmacshA256 = new HMACSHA256();
            hmacshA256.Key = Encoding.UTF8.GetBytes("IW1" + strSecret);
            foreach (string s in strings)
                hmacshA256.Key = hmacshA256.ComputeHash(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(hmacshA256.Key).ToLower().Replace("-", "");
        }
        public string GetHeader(string studioServer, string apiKey, string apiSecret)
        {
            List<string> strings = new List<string>();
            string currentUtcTime = _CurrentUtcTime;
            string nonce = Nonce;
            strings.Add(currentUtcTime);
            strings.Add(new Uri(studioServer).Scheme);
            strings.Add(k_Method);
            strings.Add(nonce);
            strings.Add(k_RequestTail);
            string signature = _GenerateSignature(strings, apiSecret);
            return $"{k_RequestHead} ApiKey={apiKey},DateTime={currentUtcTime},Nonce={nonce},Signature={signature}";
        }
    }
}
