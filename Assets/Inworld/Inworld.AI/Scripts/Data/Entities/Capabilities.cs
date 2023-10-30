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
    public class Capabilities
    {
        public bool audio;
        public bool emotions;
        public bool interruptions;
        public bool narratedActions;
        public bool text;
        public bool triggers;
        public bool phonemeInfo;
        public bool relations;
        public bool debugInfo;

        public Capabilities() {}
        public Capabilities(Capabilities rhs)
        {
            audio = rhs.audio;
            emotions = rhs.emotions;
            interruptions = rhs.interruptions;
            narratedActions = rhs.narratedActions;
            text = rhs.text;
            triggers = rhs.triggers;
            phonemeInfo = rhs.phonemeInfo;
            relations = rhs.relations;
            debugInfo = rhs.debugInfo;
        }
        public void CopyFrom(Capabilities rhs)
        {
            audio = rhs.audio;
            emotions = rhs.emotions;
            interruptions = rhs.interruptions;
            narratedActions = rhs.narratedActions;
            text = rhs.text;
            triggers = rhs.triggers;
            phonemeInfo = rhs.phonemeInfo;
            relations = rhs.relations;
            debugInfo = rhs.debugInfo;
        }
    }
}
