using Inworld.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inworld.Packet;
using UnityEngine.Serialization;

namespace Inworld
{
    public class LatencyUtility : MonoBehaviour
    {
        [SerializeField] bool m_debugLatency;
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
                        return;
                    
                    if (m_debugLatency && m_LastCharacterResponseTime > InworldController.Instance.LastPlayerResponseTime)
                    {                    
                        m_CharacterResponseDelay =  Time.time - m_LastCharacterResponseTime;// : Time.time - InworldController.Instance.LastPlayerResponseTime;
                        InworldAI.Log("Character Response Delay: " + m_CharacterResponseDelay + " lastCharacterResponseTime: " + m_LastCharacterResponseTime + " lastPlayerResponseTime: " + InworldController.Instance.LastPlayerResponseTime);
                    }
                    m_LastCharacterResponseTime = Time.time;
                    break;
            }
        }
    }
}
