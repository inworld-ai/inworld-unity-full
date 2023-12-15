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
    public class ControlEvent
    {
        public string action;
        public string description;
    }
    [Serializable]
    public class ControlPacket : InworldPacket
    {
        public ControlEvent control;

        public ControlPacket()
        {
            type = "CONTROL";
            control = new ControlEvent();
        }
        public ControlPacket(InworldPacket rhs, ControlEvent evt) : base(rhs)
        {
            type = "CONTROL";
            control = evt;
        }
    }
}
