/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Inworld.Entities
{
    [Serializable]
    public class InworldCharacterData
    {
        public string agentId;
        public string brainName;
        public string givenName;
        public CharacterDescription description;
        public CharacterAssets characterAssets;
        public string language;
        
        [JsonIgnore]
        public Texture2D thumbnail;

        [JsonIgnore]
        public float Progress => characterAssets?.Progress ?? 0;
       
        public InworldCharacterData(){}

        public InworldCharacterData(CharacterOverLoad overLoad)
        {
            brainName = overLoad.name;
            givenName = overLoad.defaultCharacterDescription.givenName;
            language = overLoad.language;
            description = overLoad.defaultCharacterDescription;
            characterAssets = new CharacterAssets(overLoad.defaultCharacterAssets);
        }
        public InworldCharacterData(CharacterReference charRef)
        {
            brainName = charRef.character;
            givenName = charRef.characterOverloads[0].defaultCharacterDescription.givenName;
            description = charRef.characterOverloads[0].defaultCharacterDescription;
            characterAssets = new CharacterAssets(charRef.characterOverloads[0].defaultCharacterAssets);
        }
        public IEnumerator UpdateThumbnail(Texture2D inputThumbnail = null)
        {
            thumbnail = inputThumbnail;
            if (thumbnail)
                yield break;
            string url = characterAssets?.ThumbnailURL;
            if (string.IsNullOrEmpty(url))
                yield break;
            UnityWebRequest uwr = new UnityWebRequest(url);
            uwr.downloadHandler = new DownloadHandlerTexture();
            yield return uwr.SendWebRequest();
            if (uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
            {
                thumbnail = (uwr.downloadHandler as DownloadHandlerTexture)?.texture;
            }
        }
        public override string ToString() => $"{givenName}: {brainName} ID: {agentId}";

        [JsonIgnore]

        public string ShortBrainName
        {
            get
            {
                string[] data = brainName.Split('/');
                return data.Length > 0 ? data[^1] : brainName;
            }
        }
        [JsonIgnore]
        public string CharacterFileName
        {
            get
            {
                string[] data = brainName.Split('/');
                return data.Length < 4 ? brainName : $"{data[3]}_{data[1]}";
            }
        }
    }
    
    [Serializable] 
    public class CharacterName
    {
        public string name;
        public string languageCode;

        public CharacterName(string inputName, string language = "en-US")
        {
            name = inputName;
            languageCode = language;
        }
    }
    [Serializable]
    public class RPMInfo
    {
        public string rpmModelUri;
        public string rpmImageUri;
    }
    
    [Serializable]
    public class CharacterAssets
    {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string rpmModelUri;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string rpmImageUri;
        public string avatarImg;
        public string avatarImgOriginal;
        public RPMInfo rpmAvatar;

        [JsonIgnore]
        public float thumbnailProgress;
        
        [JsonIgnore]
        public float avatarProgress;
        
        [JsonIgnore]
        float _ThumbnailProgress => thumbnailProgress * 0.2f; //string.IsNullOrEmpty(ThumbnailURL) ? 0.2f : 
        
        [JsonIgnore]
        float _AvatarProgress => avatarProgress * 0.8f;
        
        [JsonIgnore]
        public float Progress => _ThumbnailProgress + _AvatarProgress;
        public bool IsAsset(string url)
        {
            if (rpmImageUri == url)
                return true;
            if (avatarImg == url)
                return true;
            if (avatarImgOriginal == url)
                return true;
            return false;
        }
        [JsonIgnore]
        public string ThumbnailURL // YAN: For AvatarURL, just use rpmModelUri.
        {
            get
            {
                if (!string.IsNullOrEmpty(avatarImg))
                    return avatarImg;
                if (!string.IsNullOrEmpty(avatarImgOriginal))
                    return avatarImgOriginal;
                if (!string.IsNullOrEmpty(rpmImageUri))
                    return rpmImageUri;
                return null;
            }
        }
        public CharacterAssets() {}

        public CharacterAssets(CharacterAssets rhs)
        {
            rpmModelUri = rhs.rpmModelUri;
            rpmImageUri = rhs.rpmImageUri;
            avatarImg = rhs.avatarImg;
            avatarImgOriginal = rhs.avatarImgOriginal;
            // Apply New structure to Legacy.
            if (rhs.rpmAvatar == null)
                return;
            if (!string.IsNullOrEmpty(rhs.rpmAvatar.rpmImageUri))
                rpmImageUri = rhs.rpmAvatar.rpmImageUri;
            if (!string.IsNullOrEmpty(rhs.rpmAvatar.rpmModelUri))
                rpmModelUri = rhs.rpmAvatar.rpmModelUri;
        }
        public void CopyFrom(CharacterAssets rhs)
        {
            rpmModelUri = rhs.rpmModelUri;
            rpmImageUri = rhs.rpmImageUri;
            avatarImg = rhs.avatarImg;
            avatarImgOriginal = rhs.avatarImgOriginal;
            // Apply New structure to Legacy.
            if (rhs.rpmAvatar == null)
                return;
            if (!string.IsNullOrEmpty(rhs.rpmAvatar.rpmImageUri))
                rpmImageUri = rhs.rpmAvatar.rpmImageUri;
            if (!string.IsNullOrEmpty(rhs.rpmAvatar.rpmModelUri))
                rpmModelUri = rhs.rpmAvatar.rpmModelUri;
        }
    }

    [Serializable]
    public class CharacterDescription
    {
        public string givenName;
        public string description;
        [JsonConverter(typeof(StringEnumConverter))]
        public Pronoun pronoun;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<string> nickNames;
        public string motivation;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<string> personalityAdjectives;
        [JsonConverter(typeof(StringEnumConverter))]
        public LifeStage lifeStage;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<string> hobbyOrInterests;
        public string characterRole;
        public string flaws;
    }
    [Serializable]
    public class CharacterOverLoad
    {
        public string name; // full Name.
        public string language;
        public CharacterDescription defaultCharacterDescription;
        public CharacterAssets defaultCharacterAssets;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public List<string> commonKnowledges;

    }
    [Serializable]
    public class CharacterReference
    {
        public string character; // agentID
        public List<CharacterOverLoad> characterOverloads;
    }
}
