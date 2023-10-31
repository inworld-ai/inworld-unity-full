/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inworld.Entities
{
    [Serializable]
    public class InworldCharacterData
    {
        public string agentId;
        public string brainName;
        public string givenName;
        public CharacterAssets characterAssets;
        public Texture2D thumbnail;

        public InworldCharacterData(){}
        public InworldCharacterData(CharacterReference charRef)
        {
            brainName = charRef.character;
            givenName = charRef.characterOverloads[0].defaultCharacterDescription.givenName;
            characterAssets = new CharacterAssets(charRef.characterOverloads[0].defaultCharacterAssets);
        }
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
    public class CharacterAssets
    {
        public string rpmModelUri;
        public string rpmImageUriPortrait;
        public string rpmImageUriPosture;
        public string avatarImg;
        public string avatarImgOriginal;

        public float thumbnailProgress;
        public float avatarProgress;
        float _ThumbnailProgress => string.IsNullOrEmpty(ThumbnailURL) ? 0.2f : thumbnailProgress * 0.2f;
        float _AvatarProgress => string.IsNullOrEmpty(rpmModelUri) ? 0.8f : avatarProgress * 0.8f;
        public float Progress => _ThumbnailProgress + _AvatarProgress;
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

        public string ThumbnailURL // YAN: For AvatarURL, just use rpmModelUri.
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
        public CharacterAssets() {}

        public CharacterAssets(CharacterAssets rhs)
        {
            rpmModelUri = rhs.rpmModelUri;
            rpmImageUriPortrait = rhs.rpmImageUriPortrait;
            rpmImageUriPosture = rhs.rpmImageUriPosture;
            avatarImg = rhs.avatarImg;
            avatarImgOriginal = rhs.avatarImgOriginal;
        }
        public void CopyFrom(CharacterAssets rhs)
        {
            rpmModelUri = rhs.rpmModelUri;
            rpmImageUriPortrait = rhs.rpmImageUriPortrait;
            rpmImageUriPosture = rhs.rpmImageUriPosture;
            avatarImg = rhs.avatarImg;
            avatarImgOriginal = rhs.avatarImgOriginal;
        }
    }
    [Serializable]
    public class CharacterDescription
    {
        public string givenName;
        public string description;
    }
    [Serializable]
    public class CharacterOverLoad
    {
        public CharacterDescription defaultCharacterDescription;
        public CharacterAssets defaultCharacterAssets;
    }
    [Serializable]
    public class CharacterReference
    {
        public string character; // agentID
        public List<CharacterOverLoad> characterOverloads;
        public float Progress => characterOverloads.Count == 1 ? characterOverloads[0].defaultCharacterAssets.Progress : 0;
        public string CharacterFileName
        {
            get
            {
                string[] data = character.Split('/');
                return data.Length < 4 ? character : $"{data[3]}_{data[1]}";
            }
        }
    }
}
