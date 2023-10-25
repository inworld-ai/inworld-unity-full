/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;

namespace Inworld.Packet
{
    [Serializable]
    public class NarrativeAction
    {
        public string content;
    }
    [Serializable]
    public class ActionEvent
    {
        public NarrativeAction narratedAction;
        public string playback;
    }
    [Serializable]
    public class ActionPacket : InworldPacket
    {
        public ActionEvent action;

        public ActionPacket()
        {
            type = "ACTION";
            action = new ActionEvent();
        }
        public ActionPacket(InworldPacket rhs, ActionEvent evt) : base(rhs)
        {
            action = evt;
            type = "ACTION";
        }
    }
}
