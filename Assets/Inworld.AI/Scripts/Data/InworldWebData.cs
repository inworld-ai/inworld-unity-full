using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld
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
    }
    [Serializable]
    public class LoadSceneRequest
    {
        public Client client;
        public User user;
        public Capabilities capabilities;
        public UserSetting userSetting;
    }
    [Serializable]
    public class LoadSceneResponse
    {
        public List<InworldCharacterData> agents = new List<InworldCharacterData>();
        public string key;
        public object previousState; // TODO(Yan): Solve packets from saved data.
    }
    
    [Serializable]
    public class User
    {
        public string name;
    }
    [Serializable]
    public class UserSetting
    {
        public bool viewTranscriptConsent;
        public PlayerProfile playerProfile;
    }
    [Serializable]
    public class PlayerProfile
    {
        public IEnumerable<Fields> fields;
    }
    [SerializeField]
    public class Fields
    {
        public string fieldId;
        public string fieldValue;
    }
    [Serializable]
    public class Client
    {
        public string id;
        public string version;
    }
    [Serializable]
    public class Capabilities
    {
        public bool audio;
        public bool emotions;
        public bool interruptions;
        public bool narratedActions;
        public bool silence;
        public bool text;
        public bool triggers;
        public bool continuation;
        public bool turnBasedStt;
        public bool phonemeInfo;
    }
    [Serializable]
    public class InworldCharacterData
    {
        public string agentId;
        public string brainName;
        public string givenName;
        public CharacterAssets characterAssets;
        public Texture2D thumbnail;
    }


    [Serializable]
    public class CharacterAssets
    {
        public string rpmModelUri;
        public string rpmImageUriPortrait;
        public string rpmImageUriPosture;
        public string avatarImg;
        public string avatarImgOriginal;

        public bool IsAsset(string url)
        {
            if (rpmImageUriPortrait == url)
                return true;
            if (rpmImageUriPosture == url)
                return true;
            if (avatarImg == url)
                return true;
            if (avatarImgOriginal == url)
                return true;
            return false;
        }

        public string URL
        {
            get
            {
                if (!string.IsNullOrEmpty(avatarImg))
                    return avatarImg;
                if (!string.IsNullOrEmpty(avatarImgOriginal))
                    return avatarImgOriginal;
                if (!string.IsNullOrEmpty(rpmImageUriPortrait))
                    return rpmImageUriPortrait;
                if (!string.IsNullOrEmpty(rpmImageUriPosture))
                    return rpmImageUriPosture;
                return null;
            }
        }
    }
}
