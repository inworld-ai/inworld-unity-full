/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using Inworld.Packet;
using System;
using UnityEngine;
namespace Inworld.Sample
{
    public class PerceivedLatencyTest : MonoBehaviour
    {
        [SerializeField] bool m_AutoAttached = true;
        bool m_IsFromPlayer;
        float m_RoundTripTimeSampler;
        float m_PerceivedTimeSampler;

        void OnEnable()
        {
            if (!m_AutoAttached)
                return;
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnPacketReceived += RoundTripPacketReceived;
            InworldController.Audio.Event.onPlayerStopSpeaking.AddListener(StartSampling);
            InworldController.CharacterHandler.Event.onCharacterListJoined.AddListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.AddListener(OnCharacterLeft);
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnPacketReceived -= RoundTripPacketReceived;
            InworldController.CharacterHandler.Event.onCharacterListJoined.RemoveListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.RemoveListener(OnCharacterLeft);
        }
        protected virtual void OnCharacterJoined(InworldCharacter character)
        {
            // YAN: Clear existing event listener to avoid adding multiple times.
            character.Event.onPacketReceived.RemoveListener(RoundTripPacketReceived); 
            character.Event.onPacketReceived.AddListener(RoundTripPacketReceived);
            character.Event.onBeginSpeaking.RemoveListener(StopSampling);
            character.Event.onBeginSpeaking.AddListener(StopSampling);
        }

        protected virtual void OnCharacterLeft(InworldCharacter character)
        {
            character.Event.onPacketReceived.RemoveListener(RoundTripPacketReceived); 
            character.Event.onBeginSpeaking.RemoveListener(StopSampling);
        }
        protected virtual void RoundTripPacketReceived(InworldPacket packet)
        {
            switch (packet.Source)
            {
                case SourceType.PLAYER:
                    m_IsFromPlayer = true;
                    m_RoundTripTimeSampler = Time.unscaledTime;
                    break;
                case SourceType.AGENT:
                {
                    if (m_IsFromPlayer)
                    {
                        InworldAI.Log($"RoundTrip Latency: {Time.unscaledTime - m_RoundTripTimeSampler}ms.");
                        m_IsFromPlayer = false;
                    }
                    break;
                }
            }
        }
        /// <summary>
        /// Start Sampling. if AutoAttached is not toggled, you can assign this function anywhere.
        /// </summary>
        public virtual void StartSampling()
        {
            m_PerceivedTimeSampler = Time.unscaledTime;
        }
        /// <summary>
        /// Stop Sampling. if AutoAttached is not toggled, you can assign this function anywhere.
        /// </summary>
        public virtual void StopSampling(string _) => InworldAI.Log($"Perceived Latency: {Time.unscaledTime - m_PerceivedTimeSampler}ms.");

    }
}
