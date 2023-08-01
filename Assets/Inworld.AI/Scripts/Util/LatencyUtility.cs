using Inworld.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inworld.Packet;
using System;
using UnityEngine.Serialization;

namespace Inworld
{
    public class LatencyUtility : MonoBehaviour
    {
        [SerializeField] bool m_debugLatency;
        [SerializeField] bool m_showDelaySinceLastCharacterResponse;
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

                    if (m_debugLatency)
                    {
                        bool useCharacterTime = m_LastCharacterResponseTime > InworldController.Instance.LastPlayerResponseTime;
                        m_CharacterResponseDelay =  useCharacterTime? Time.time - m_LastCharacterResponseTime : Time.time - InworldController.Instance.LastPlayerResponseTime;
                        string since = useCharacterTime ? " since last Character response " : " since last Player response ";
                        if(!m_showDelaySinceLastCharacterResponse && useCharacterTime)
                            return;

                        InworldAI.Log("Character response delay" + since + m_CharacterResponseDelay); 
                    }
                    m_LastCharacterResponseTime = Time.time;
                    break;
                case "PLAYER":
                    if(!(packet is TextPacket))
                        return;
                    
                    InworldController.Instance.LastPlayerResponseTime = Time.time;
                    break;
            }
        }
    }
}
