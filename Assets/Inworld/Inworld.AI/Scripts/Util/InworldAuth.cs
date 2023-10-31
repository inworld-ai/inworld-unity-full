/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Random = UnityEngine.Random;

namespace Inworld
{
    public class InworldAuth
    {
        const string k_Method = "ai.inworld.engine.v1.SessionTokens/GenerateSessionToken";
        const string k_RequestHead = "IW1-HMAC-SHA256";
        const string k_RequestTail = "iw1_request";
        static string _CurrentUtcTime => DateTime.UtcNow.ToString("yyyyMMddHHmmss");
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

        static string _GenerateSignature(List<string> strings, string strSecret)
        {
            HMACSHA256 hmacshA256 = new HMACSHA256();
            hmacshA256.Key = Encoding.UTF8.GetBytes("IW1" + strSecret);
            foreach (string s in strings)
                hmacshA256.Key = hmacshA256.ComputeHash(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(hmacshA256.Key).ToLower().Replace("-", "");
        }
        /// <summary>
        /// Generate the header to access token
        /// </summary>
        /// <param name="studioServer">the server to generate token.</param>
        /// <param name="apiKey">the input API key.</param>
        /// <param name="apiSecret">the input API secret.</param>
        public static string GetHeader(string studioServer, string apiKey, string apiSecret)
        {
            List<string> strings = new List<string>();
            string currentUtcTime = _CurrentUtcTime;
            string nonce = Nonce;
            strings.Add(currentUtcTime);
            strings.Add(studioServer);
            strings.Add(k_Method);
            strings.Add(nonce);
            strings.Add(k_RequestTail);
            string signature = _GenerateSignature(strings, apiSecret);
            return $"{k_RequestHead} ApiKey={apiKey},DateTime={currentUtcTime},Nonce={nonce},Signature={signature}";
        }
    }
}
