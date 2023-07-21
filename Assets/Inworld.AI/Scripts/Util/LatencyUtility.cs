using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inworld.Packet;

namespace Inworld
{
    public class LatencyUtility : MonoBehaviour
    {
        [SerializeField] bool debugLatency;
        float lastPlayerResponseTime = 0f;
        float lastCharacterResponseTime = 0f;
        float characterResponseDelay = 0f;
        
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
                        characterResponseDelay = lastCharacterResponseTime > lastPlayerResponseTime ? Time.time - lastCharacterResponseTime : Time.time - lastPlayerResponseTime;
                        InworldAI.Log("Character Response Delay: " + characterResponseDelay + " lastCharacterResponseTime: " + lastCharacterResponseTime + " lastPlayerResponseTime: " + lastPlayerResponseTime);
                    }
                    lastCharacterResponseTime = Time.time;
                    break;
                case "PLAYER":
                    if(!(packet is TextPacket))
                    {
                        return;
                    }
                    lastPlayerResponseTime = Time.time;
                    break;
            }
        }
        
        // Start is called before the first frame update
        void Start() {}

        // Update is called once per frame
        void Update() {}
    }
}
