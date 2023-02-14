/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/
using System;
using UnityEngine;
namespace Inworld.Studio
{
    [Serializable]
    public class FBTokenResponse
    {
        public string access_token;
        public string expires_in;
        public string token_type;
        public string refresh_token;
        public string id_token;
        public string user_id;
        public string project_id;
    }

    public class WorkspaceFetchingProgress
    {
        public float currentCharacters;
        public float currentKeys;
        public float currentScene;
        public float totalCharacters = -1;
        public float totalKeys = -1;
        public float totalScene = -1;

        public float Progress
        {
            get
            {
                float fResult = 0;
                fResult += totalScene == 0 ? 25f : currentScene / totalScene * 25f;
                fResult += totalCharacters == 0 ? 50f : currentCharacters / totalCharacters * 50f;
                fResult += totalKeys == 0 ? 25f : currentKeys / totalKeys * 25f;
                return Mathf.Max(fResult, 0);
            }
        }
    }
}
