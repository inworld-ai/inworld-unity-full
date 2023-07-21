using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inworld.Packet;

namespace Inworld
{
    public class LatencyUtility : MonoBehaviour
    {
        [SerializeField] bool debugLatency;
        float m_LastCharacterResponseTime = 0f;
        float m_CharacterResponseDelay = 0f;
        
        void OnEnable()
        {
            InworldController.Client.OnPacketReceived += ReceivePacket;
        }
        void OnDisable()
        {
            if (InworldController.Instance)
                InworldController.Client.OnPacketReceived -= ReceivePacket;
        }
        
        void ReceivePacket(InworldPacket packet)
        {
            switch (packet.routing.source.type.ToUpper())
            {
                case "AGENT":
                    if(!(packet is AudioPacket))
                    {
                        return;
                    }
                    
                    if (debugLatency)
                    {                    
                        m_CharacterResponseDelay = m_LastCharacterResponseTime > InworldController.lastPlayerResponseTime ? Time.time - m_LastCharacterResponseTime : Time.time - InworldController.lastPlayerResponseTime;
                        InworldAI.Log("Character Response Delay: " + m_CharacterResponseDelay + " lastCharacterResponseTime: " + m_LastCharacterResponseTime + " lastPlayerResponseTime: " + InworldController.lastPlayerResponseTime);
                    }
                    m_LastCharacterResponseTime = Time.time;
                    break;
            }
        }
    }
}
