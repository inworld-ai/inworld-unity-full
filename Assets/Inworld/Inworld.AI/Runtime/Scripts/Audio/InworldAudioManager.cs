/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Inworld.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class InworldAudioManager : MonoBehaviour
    {
        const int k_SampleRate = 16000;
        const string k_UniqueModuleChecker = "Find Multiple Modules with StartingAudio.\nPlease ensure there is only one in the feature list.";
        [SerializeField] List<InworldAudioModule> m_AudioModules;
        [SerializeField] AudioEvent m_AudioEvent;
        [SerializeField] string m_DeviceName;
        
        AudioSource m_RecordingSource;

        protected CircularBuffer<short> m_InputBuffer = new CircularBuffer<short>(k_SampleRate);
        protected IEnumerator m_AudioCoroutine;
        protected bool m_IsPlayerSpeaking;

        protected bool m_IsCalibrating = false;

        public CircularBuffer<short> InputBuffer
        {
            get => m_InputBuffer;
            set => m_InputBuffer = value;
        }
        public string DeviceName
        {
            get => m_DeviceName;
            set => m_DeviceName = value;
        }
        public AudioSource RecordingSource
        {
            get             
            {
                if (m_RecordingSource)
                    return m_RecordingSource;
                m_RecordingSource = GetComponent<AudioSource>();
                if (!m_RecordingSource)
                    m_RecordingSource = gameObject.AddComponent<AudioSource>();
                return m_RecordingSource;
            }
        }
        public AudioClip RecordingClip
        {
            get => RecordingSource?.clip;
            set
            {
                if (RecordingSource)
                    RecordingSource.clip = value;
            }
        }

        public AudioEvent Event => m_AudioEvent;

        //YAN: NOTE: All the flags will only trigger events invoking. They will not control other stuff.
        public bool IsPlayerSpeaking
        {
            get => m_IsPlayerSpeaking;
            set
            {
                _SetBoolWithEvent(ref m_IsPlayerSpeaking, value, Event.onPlayerStartSpeaking,
                    Event.onPlayerStopSpeaking);
                GetModules<ISendAudioHandler>().ForEach(h =>
                {
                    if (m_IsPlayerSpeaking)
                        h.OnStartSendAudio();
                    else
                        h.OnStopSendAudio();
                });
            }
        }

        public bool IsCalibrating
        {
            get => m_IsCalibrating;
            set => _SetBoolWithEvent(ref m_IsCalibrating, value, Event.onStartCalibrating, Event.onStopCalibrating);
        }


        public MicrophoneMode SendingMode
        {
            get => GetModule<ISendAudioHandler>()?.SendingMode ?? MicrophoneMode.UNSPECIFIED;
            set
            {
                ISendAudioHandler module = GetModule<ISendAudioHandler>();
                if (module != null)
                    module.SendingMode = value;
            }
        }

        public float Volume
        {
            get => RecordingSource?.volume ?? -1f;
            set
            {
                if (RecordingSource)
                    RecordingSource.volume = value;
            }
        }
        public bool IsMicRecording => GetUniqueModule<IMicrophoneHandler>()?.IsMicRecording ?? false;
        public bool StartMicrophone()
        {
            IMicrophoneHandler micHandler = GetModule<IMicrophoneHandler>();
            if (micHandler == null)
                return false;
            micHandler.StartMicrophone();
            Event.onRecordingStart.Invoke();
            return true;
        }

        public bool StopMicrophone()
        {
            IMicrophoneHandler micHandler = GetModule<IMicrophoneHandler>();
            if (micHandler == null)
                return false;
            micHandler.StopMicrophone();
            Event.onRecordingEnd.Invoke();
            return true;
        }

        public void StartAudioThread()
        {
            if (m_AudioCoroutine != null) 
                return;
            m_AudioCoroutine = AudioCoroutine();
            StartCoroutine(m_AudioCoroutine);
        }

        public void StopAudioThread()
        {
            if (m_AudioCoroutine == null) 
                return;
            StopCoroutine(m_AudioCoroutine);
            m_AudioCoroutine = null;
        }
        public void ResetPointer() => GetUniqueModule<ICollectAudioHandler>()?.ResetPointer();
        public void StartVoiceDetecting() => GetUniqueModule<IPlayerAudioEventHandler>()?.StartVoiceDetecting();
        public void StopVoiceDetecting() => GetUniqueModule<IPlayerAudioEventHandler>()?.StopVoiceDetecting();
        public void StartCalibrate() => GetModules<ICalibrateAudioHandler>().ForEach(module => module.OnStartCalibration());
        public void StopCalibrate() => GetModules<ICalibrateAudioHandler>().ForEach(module => module.OnStopCalibration());
        public void CollectAudio() => GetModules<ICollectAudioHandler>().ForEach(module => module.OnCollectAudio());
        public void PreProcess() => GetModules<IProcessAudioHandler>().ForEach(module => module.OnPreProcessAudio());
        public void PostProcess() => GetModules<IProcessAudioHandler>().ForEach(module => module.OnPostProcessAudio());

        /// <summary>
        /// Manually push the audio wave data to server.
        /// </summary>
        public IEnumerator PushAudio()
        {
            yield return new WaitForSeconds(1f);
        }

        public bool TryDeleteModule<T>()
        {
            bool hasDeleted = false; 
            for (int i = m_AudioModules.Count - 1; i >= 0; i--) 
            {
                T component = m_AudioModules[i].GetComponent<T>();
                if (component == null) 
                    continue;
                m_AudioModules.RemoveAt(i); 
                hasDeleted = true;
            }
            return hasDeleted;
        }
        public void AddModule(InworldAudioModule module) => m_AudioModules.Add(module);

        public T GetModule<T>() => m_AudioModules.Select(module => module.GetComponent<T>()).FirstOrDefault(result => result != null);

        public T GetUniqueModule<T>()
        {
            List<T> modules = GetModules<T>();
            if (modules == null || modules.Count == 0) 
                throw new ModuleNotFoundException(typeof(T).Name);
            if (modules.Count > 1)
                InworldAI.LogWarning(k_UniqueModuleChecker);
            return modules[0];
        }

        public List<T> GetModules<T>() => m_AudioModules.Select(module => module.GetComponent<T>()).Where(result => result != null).ToList();

        void OnDestroy()
        {
            StopMicrophone();
        }
        protected virtual IEnumerator AudioCoroutine()
        {
            while (IsMicRecording)
            {
                PreProcess();
                CollectAudio();
                PostProcess();
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        void _SetBoolWithEvent(ref bool flag, bool value, UnityEvent onEvent, UnityEvent offEvent)
        {
            if (flag == value)
                return;
            flag = value;
            if (flag)
                onEvent?.Invoke();
            else
                offEvent?.Invoke();
        }
    }
}