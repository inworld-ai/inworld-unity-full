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
    public class UserRequest
    {
        public string name;
        public string id;
    }

    [Serializable]
    public class UserSetting
    {
        [HideInInspector] public bool viewTranscriptConsent;
        public PlayerProfile playerProfile;

        public UserSetting(List<PlayerProfileField> rhs)
        {
            viewTranscriptConsent = true;
            playerProfile = new PlayerProfile
            {
                fields = rhs
            };
        }
    }
    [Serializable]
    public class PlayerProfile
    {
        public List<PlayerProfileField> fields;
    }
    [Serializable]
    public class PlayerProfileField
    {
        public string fieldId;
        public string fieldValue;
    }
}
