using System;
using UnityEngine.Serialization;

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
            action = new ActionEvent();
        }
        public ActionPacket(InworldPacket rhs, ActionEvent evt) : base(rhs)
        {
            action = evt;
        }
    }
}
