/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;


namespace Inworld.Packet
{
    [Serializable]
    public class TextEvent
    {
        public string text;
        public string sourceType;
        public bool final;

        public TextEvent(string textToSend = "")
        {
            text = textToSend;
            sourceType = "TYPED_IN";
            final = true;
        }
    }
    [Serializable]
    public sealed class TextPacket : InworldPacket
    {
        public TextEvent text;
        public TextPacket()
        {
            text = new TextEvent();
        }
        public TextPacket(string textToSend)
        {
            text = new TextEvent(textToSend);
            PreProcess();
        }
        public TextPacket(InworldPacket rhs, TextEvent evt) : base(rhs)
        {
            text = evt;
        }
    }
}
