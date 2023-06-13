/*************************************************************************************************
* Copyright 2022 Theai, Inc. (DBA Inworld)
*
* Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
* that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
*************************************************************************************************/

using System;
using PlayerField = Inworld.Grpc.UserSettings.Types.PlayerProfile.Types.PlayerField;

namespace Inworld.Util
{
    [Serializable]
    public class InworldPlayerProfile
    {
        public string name;
        public string value;

        public PlayerField ToGrpc => new PlayerField
        {
            FieldId = name,
            FieldValue = value
        };
    }
}
