/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AOT;
using Inworld.Entities;
using System.Collections;
using UnityEngine;

#if !UNITY_ANDROID
namespace Inworld.Audio
{
    public class WebGLCaptureModule : InworldAudioModule, IMicrophoneHandler, ICollectAudioHandler
    {
        public delegate void NativeCommand(string json);
        [DllImport("__Internal")] public static extern int WebGLInit(NativeCommand handler);
        [DllImport("__Internal")] public static extern int WebGLInitSamplesMemoryData(float[] array, int length);
        [DllImport("__Internal")] public static extern int WebGLIsRecording();
        [DllImport("__Internal")] public static extern string WebGLGetDeviceData();
        [DllImport("__Internal")] public static extern string WebGLGetDeviceCaps();
        [DllImport("__Internal")] public static extern int WebGLGetPosition();
        [DllImport("__Internal")] public static extern void WebGLMicStart(string deviceId, int frequency, int lengthSec);
        [DllImport("__Internal")] public static extern void WebGLMicEnd();
        [DllImport("__Internal")] public static extern void WebGLDispose();
        [DllImport("__Internal")] public static extern int WebGLIsPermitted();
        
        protected static float[] s_WebGLBuffer;
        public static bool WebGLPermission { get; set; }
        protected List<AudioDevice> m_Devices = new List<AudioDevice>();
        protected int m_LastPosition;
        protected int m_CurrPosition;


        public bool IsMicRecording => WebGLIsRecording() != 0;

        protected void Awake()
        {
            s_WebGLBuffer = new float[k_InputBufferSecond * k_InputSampleRate * k_InputChannels];
            WebGLInit(OnWebGLInitialized);
        }

        protected void OnDestroy()
        {
            m_Devices.Clear();
            WebGLDispose();
            s_WebGLBuffer = null;
        }
        string GetWebGLMicDeviceID() => m_Devices.FirstOrDefault(d => d.label == Audio.DeviceName)?.deviceId;
        
        public List<string> ListMicDevices() => m_Devices.Select(device => device.label).ToList();
        
        public bool ChangeInputDevice(string deviceName)
        {
            InworldAI.LogWarning($"Changing Microphone to {deviceName}");
            if (deviceName == Audio.DeviceName)
                return true;

            if (IsMicRecording)
                StopMicrophone();

            Audio.DeviceName = deviceName;
            if (!StartMicrophone())
                return false;
            Audio.StartCalibrate();
            return true;
        }
        public bool StartMicrophone()
        {
            if (m_Devices.Count == 0)
            {
                m_Devices = JsonUtility.FromJson<WebGLAudioDevicesData>(WebGLGetDeviceData()).devices;
            }
            if (string.IsNullOrEmpty(Audio.DeviceName))
            {
                Audio.DeviceName = m_Devices.Count == 0 ? "" : m_Devices[0].label;
            }
            string micDeviceID = GetWebGLMicDeviceID();
            if (string.IsNullOrEmpty(micDeviceID))
                throw new ArgumentException($"Couldn't acquire device ID for device name: {Audio.DeviceName} ");
            if (IsMicRecording)
                return false;
            if (Audio.RecordingClip)
                Destroy(Audio.RecordingClip);
            Audio.RecordingClip = AudioClip.Create("Microphone", k_InputSampleRate * k_InputBufferSecond, 1, k_InputSampleRate, false);
            StartCoroutine(StartWebGLMicrophone(micDeviceID));
            return true;
        }

        IEnumerator StartWebGLMicrophone(string micDeviceID)
        {
            if (s_WebGLBuffer == null || s_WebGLBuffer.Length == 0)
                s_WebGLBuffer = new float[k_InputSampleRate];
            WebGLInitSamplesMemoryData(s_WebGLBuffer, s_WebGLBuffer.Length);
            WebGLMicStart(micDeviceID, k_InputSampleRate, k_InputBufferSecond);
            while (!IsMicRecording)
            {
                Debug.Log("Starting Microphone...");
                yield return new WaitForFixedUpdate();
            }
            Audio.ResetPointer();
            Audio.StartAudioThread();
        }


        public bool StopMicrophone()
        {
            WebGLMicEnd();
            Audio.InputBuffer.Clear();
            Audio.ResetPointer();
            Audio.StopAudioThread();
            return true;
        }

        public int OnCollectAudio()
        {
            if (!IsMicRecording)
                StartMicrophone();
            AudioClip recClip = Audio.RecordingClip;
            if (!recClip)
                return -1;
            m_CurrPosition = WebGLGetPosition();

            if (m_CurrPosition < m_LastPosition)
                m_CurrPosition = recClip.samples;
            if (m_CurrPosition <= m_LastPosition)
                return -1;
            int nSize = m_CurrPosition - m_LastPosition;
            if (!WebGLGetAudioData())
                return -1;
            m_LastPosition = m_CurrPosition % recClip.samples;
            return nSize;
        }

        public void ResetPointer() => m_LastPosition = m_CurrPosition = 0;

        protected bool WebGLGetAudioData()
        {
            if (s_WebGLBuffer == null || s_WebGLBuffer.Length == 0)
                return false;
            List<short> samples = new List<short>();
            
            for (int i = m_LastPosition; i < m_CurrPosition; i++)
            {
                float clampedSample = Mathf.Clamp(s_WebGLBuffer[i], -1, 1);
                samples.Add(Convert.ToInt16(clampedSample * short.MaxValue));
            }
            Audio.InputBuffer.Enqueue(samples);
            return true;
        }
        [MonoPInvokeCallback(typeof(NativeCommand))]
        static void OnWebGLInitialized(string json)
        {
            try
            {
                WebGLCommand<object> command = JsonUtility.FromJson<WebGLCommandData<object>>(json).command;
                switch (command.command)
                {
                    case "PermissionChanged":
                        WebGLCommand<bool> boolCmd = JsonUtility.FromJson<WebGLCommandData<bool>>(json).command;
                        if (boolCmd.data) // Permitted.
                        {
                            WebGLPermission = true;
                            InworldController.Audio.StartMicrophone();
                        }
                        break;
                    case "StreamChunkReceived":
                        WebGLCommand<string> strCmd = JsonUtility.FromJson<WebGLCommandData<string>>(json).command;
                        string[] split = strCmd.data.Split(':');

                        int index = int.Parse(split[0]);
                        int length = int.Parse(split[1]);
                        int bufferLength = int.Parse(split[2]);
                        if (bufferLength == 0)
                        {
                            // Somehow the buffer will be dropped in the middle.
                            InworldAI.Log("Buffer released, reinstall");
                            WebGLInitSamplesMemoryData(s_WebGLBuffer, s_WebGLBuffer.Length); 
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (InworldAI.IsDebugMode)
                {
                    Debug.LogException(ex);
                }
            }
        }  
    }
}
#endif