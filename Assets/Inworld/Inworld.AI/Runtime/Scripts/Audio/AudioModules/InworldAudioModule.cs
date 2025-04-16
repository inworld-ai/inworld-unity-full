/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Inworld.Entities;
using UnityEngine;

namespace Inworld.Audio
{
    /// <summary>
    /// The basic module class. All the module related interfaces are also put in the same file.
    /// </summary>
    public abstract class InworldAudioModule : MonoBehaviour
    {
        public int Priority {get; set;}
        protected const int k_InputSampleRate = 16000;
        protected const int k_InputChannels = 1;
        protected const int k_InputBufferSecond = 1;
        protected const int k_SizeofInt16 = sizeof(short);
        InworldAudioManager m_Manager;
        IEnumerator m_ModuleCoroutine;

        public InworldAudioManager Audio
        {
            get
            {
                if (m_Manager != null)
                    return m_Manager;
                m_Manager = FindFirstObjectByType<InworldAudioManager>();
                return m_Manager;
            }
        }
        public virtual void StartModule(IEnumerator moduleCycle)
        {
            if (moduleCycle == null || m_ModuleCoroutine != null) 
                return;
            m_ModuleCoroutine = moduleCycle;
            StartCoroutine(m_ModuleCoroutine);
        }

        public virtual void StopModule()
        {
            if (m_ModuleCoroutine == null) 
                return;
            StopCoroutine(m_ModuleCoroutine);
            m_ModuleCoroutine = null;
        }
    }
    public class ModuleNotFoundException : InworldException
    {
        public ModuleNotFoundException(string moduleName) : base($"Module {moduleName} not found")
        {
        }
    }

    public interface IMicrophoneHandler
    {
        List<string> ListMicDevices();
        bool IsMicRecording {get;}
        bool StartMicrophone();
        bool ChangeInputDevice(string deviceName);
        bool StopMicrophone();
    }

    public interface ICollectAudioHandler
    {
        int OnCollectAudio();
        void ResetPointer();
    }

    public interface ICalibrateAudioHandler
    {
        void OnStartCalibration();
        void OnStopCalibration();
        void OnCalibrate();
    }

    public interface IPlayerAudioEventHandler
    {
        IEnumerator OnPlayerUpdate();
        void StartVoiceDetecting();
        void StopVoiceDetecting();
    }

    public interface IProcessAudioHandler
    {
        bool OnPreProcessAudio();
        bool OnPostProcessAudio();
        CircularBuffer<short> ProcessedBuffer { get; set; }
    }

    public interface ISendAudioHandler
    {
        MicrophoneMode SendingMode { get; set; }
        void OnStartSendAudio();
        void OnStopSendAudio();
        bool OnSendAudio(AudioChunk audioChunk);
    }
}