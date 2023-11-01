/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;

namespace Inworld.Entities
{
    [Serializable]
    public class Token
    {
        public string token;
        public string type;
        public string expirationTime;
        public string sessionId;
        
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(token))
                    return false;
                if (string.IsNullOrEmpty(type))
                    return false;
                string[] timeFormat = 
                {
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyy-MM-ddTHH:mm:ssZ"
                };
                if (DateTime.TryParseExact(expirationTime, timeFormat, 
                                           System.Globalization.CultureInfo.InvariantCulture, 
                                           System.Globalization.DateTimeStyles.RoundtripKind, 
                                           out DateTime outTime))
                    return DateTime.UtcNow < outTime;
                return false;
            }
        }
    }
    [Serializable]
    public class AccessTokenRequest
    {
        public string api_key;
        public string resource_id;
    }
}
