using System;
namespace Inworld.Packet
{
    [Serializable]
    public class ActionEvent
    {
        public string content;
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
