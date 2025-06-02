/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
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
            InworldController.Client.OnPacketSent += StartSampling;
            InworldController.Client.EnableAudioLatencyReport = true;
            InworldController.CharacterHandler.Event.onCharacterListJoined.AddListener(OnCharacterJoined);
            InworldController.CharacterHandler.Event.onCharacterListLeft.AddListener(OnCharacterLeft);
        }
        void OnDisable()
        {
            if (!InworldController.Instance)
                return;
            InworldController.Client.OnPacketReceived -= RoundTripPacketReceived;
            InworldController.Client.OnPacketSent -= StartSampling;
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
            if (packet is TextPacket textPacket 
                && textPacket.Source == SourceType.PLAYER 
                && !textPacket.text.sourceType.ToUpper().Contains("TYPE") 
                && textPacket.text.final)
                InworldAI.Log($"ASR Latency: {Time.unscaledTime - m_RoundTripTimeSampler}s.");
        }
        /// <summary>
        /// Start Sampling. if AutoAttached is not toggled, you can assign this function anywhere.
        /// </summary>
        public virtual void StartSampling(InworldPacket pkt)
        {
            if (pkt is AudioPacket)
                m_RoundTripTimeSampler = Time.unscaledTime;
            m_PerceivedTimeSampler = Time.unscaledTime;
        }
        /// <summary>
        /// Stop Sampling. if AutoAttached is not toggled, you can assign this function anywhere.
        /// </summary>
        public virtual void StopSampling(string _)
        {
            float latency = Time.unscaledTime - m_PerceivedTimeSampler;
            InworldAI.Log($"Perceived Latency: {latency}s.");
            InworldController.Client.SendPerceivedLatencyReport(latency);
        }
    }
}
