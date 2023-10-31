/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using UnityEngine;

namespace Inworld.Sample
{
    public class PackageLatencyTest : MonoBehaviour
    {
        [SerializeField] string m_PacketType = "TEXT";
        [SerializeField] bool m_IsEnabled;
        
        bool IsFromPlayer(InworldPacket packet) => packet.routing.source.type.ToUpper() == "PLAYER";
        bool m_LastPacketIsFromPlayer;
        float m_PlayerTime;
        float m_ServerTime;
        void OnEnable()
        {
            InworldController.Instance.OnCharacterInteraction += OnInteraction;
        }

        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Instance.OnCharacterInteraction -= OnInteraction;
        }

        void OnInteraction(InworldPacket incomingPacket)
        {
            if (incomingPacket.type.ToUpper() == m_PacketType && IsFromPlayer(incomingPacket))
            {
                m_LastPacketIsFromPlayer = true;
                m_PlayerTime = Time.time;
            }
            else if (incomingPacket.type.ToUpper() == m_PacketType)
            {
                if (m_LastPacketIsFromPlayer && m_IsEnabled)
                    InworldAI.Log($"Package Latency: {Time.time - m_PlayerTime}");
                m_LastPacketIsFromPlayer = false;
            }
        }
    }
}
