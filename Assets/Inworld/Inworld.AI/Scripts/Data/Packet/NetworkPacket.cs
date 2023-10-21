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
    public class NetworkPacketResponse
    {
        public InworldNetworkPacket result;
    }
    [Serializable]
    public class InworldNetworkPacket : InworldPacket
    {
        public TextEvent text;
        public ControlEvent control;
        public DataChunk dataChunk;
        public GestureEvent gesture;
        public CustomEvent custom;
        public MutationEvent mutation;
        public EmotionEvent emotion;
        public ActionEvent action;
        public RelationEvent debugInfo;

        public InworldPacket Packet
        {
            get
            {
                if (text != null && !string.IsNullOrEmpty(text.text))
                    return new TextPacket(this, text);
                if (control != null && !string.IsNullOrEmpty(control.action))
                    return new ControlPacket(this, control);
                if (dataChunk != null && !string.IsNullOrEmpty(dataChunk.chunk) && dataChunk.type == "AUDIO")
                    return new AudioPacket(this, dataChunk);
                if (gesture != null && !string.IsNullOrEmpty(gesture.type))
                    return new GesturePacket(this, gesture);
                if (custom != null && !string.IsNullOrEmpty(custom.name))
                    return new CustomPacket(this, custom);
                if (mutation != null && !string.IsNullOrEmpty(mutation.cancelResponses?.interactionId))
                    return new MutationPacket(this, mutation);
                if (emotion != null && !string.IsNullOrEmpty(emotion.behavior))
                    return new EmotionPacket(this, emotion);
                if (action != null && action.narratedAction != null && !string.IsNullOrEmpty(action.narratedAction.content))
                    return new ActionPacket(this, action);
                if (debugInfo != null)
                    return new RelationPacket(this, debugInfo);
                return this;
            }
        }
        public PacketType Type
        {
            get
            {
                if (text != null && !string.IsNullOrEmpty(text.text))
                    return PacketType.TEXT;
                if (control != null && !string.IsNullOrEmpty(control.action))
                    return PacketType.CONTROL;
                if (dataChunk != null && !string.IsNullOrEmpty(dataChunk.chunk) && dataChunk.type == "AUDIO")
                    return PacketType.AUDIO;
                if (gesture != null && !string.IsNullOrEmpty(gesture.type))
                    return PacketType.GESTURE;
                if (custom != null && !string.IsNullOrEmpty(custom.name))
                    return PacketType.CUSTOM;
                if (mutation != null && !string.IsNullOrEmpty(mutation.cancelResponses?.interactionId))
                    return PacketType.CANCEL_RESPONSE;
                if (emotion != null && !string.IsNullOrEmpty(emotion.behavior))
                    return PacketType.EMOTION;
                if (action != null && action.narratedAction != null && !string.IsNullOrEmpty(action.narratedAction.content))
                    return PacketType.ACTION;
                if (debugInfo != null)
                    return PacketType.RELATION;
                return PacketType.UNKNOWN;
            }
        }
    }
}
