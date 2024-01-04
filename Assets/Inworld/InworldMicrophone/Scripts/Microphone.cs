//#define FORCE_WEBGL_MIC

// C# 10 feature
//global using Microphone = FrostweepGames.MicrophonePro.Microphone;

#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
using System.Runtime.InteropServices;
#endif
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using AOT;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace FrostweepGames.MicrophonePro
{
    public sealed class Microphone
    {
        /// <summary>
        /// Uses for debugging native commands. Works only in WebGL
        /// </summary>
        public static bool Logging = false;

        /// <summary>
        /// Fire when permission for microphone was changed. In WebGL it does automatically. On Android/IOS fire only when requested permission via RequestPermission() function
        /// </summary>
        public static event Action<bool> PermissionChangedEvent;

        /// <summary>
        /// Fire when receiving stream chunk from native recorder. Works only in WebGL
        /// </summary>
        public static event Action<StreamData> RecordStreamDataEvent;

#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
        private delegate void NativeCommand(string json);

        #region __Internal

        [DllImport("__Internal")]
        private static extern int init(NativeCommand handler);

        [DllImport("__Internal")]
        private static extern int initSamplesMemoryData(float[] array, int length, int left);

        [DllImport("__Internal")]
        private static extern int isRecording();

        [DllImport("__Internal")]
        private static extern string devicesData();

        [DllImport("__Internal")]
        private static extern string getDeviceCaps();

        [DllImport("__Internal")]
        private static extern int getPosition();

        [DllImport("__Internal")]
        private static extern void start(string deviceId, int frequency, int loop, int lengthSec);

        [DllImport("__Internal")]
        private static extern void end();

        [DllImport("__Internal")]
        private static extern void dispose();

        [DllImport("__Internal")]
        private static extern int isPermissionGranted();

        [DllImport("__Internal")]
        private static extern void setLeapSync(int enabled);

        #endregion

        private static float[] _leftChannelBuffer;
        private static float[] _rightChannelBuffer;
        private static float[] _mainBuffer;
        private static List<Device> _devices;
        private static int _channels = 2;
#endif
        private static AudioClip _microphoneClip;

        /// <summary>
        ///   <para>A list of available microphone devices, identified by name.</para>
        /// </summary>
        public static string[] devices
        {
            get
            {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
                _devices = Deserialize<SimpleDevicesData>(devicesData()).devices;
                return _devices.Select(item => item.label).ToArray();
#else
                return UnityEngine.Microphone.devices;
#endif
            }
        }

        static Microphone()
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            _devices = new List<Device>();
            init(HandleNativeCommandExecution);
#endif
        }

        /// <summary>
        ///   <para>Start Recording with device.</para>
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        /// <param name="loop">Indicates whether the recording should continue recording if lengthSec is reached, and wrap around and record from the beginning of the AudioClip.</param>
        /// <param name="lengthSec">Is the length of the AudioClip produced by the recording.</param>
        /// <param name="frequency">The sample rate of the AudioClip produced by the recording.</param>
        /// <returns>
        ///   <para>The function returns null if the recording fails to start.</para>
        /// </returns>
        public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency, bool leapSync = false)
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            string microphoneDeviceIDFromName = GetMicrophoneDeviceIDFromName(deviceName);
            if (string.IsNullOrEmpty(microphoneDeviceIDFromName))
                throw new ArgumentException("Couldn't acquire device ID for device name " + deviceName);
            if (lengthSec <= 0)
                throw new ArgumentException("Length of recording must be greater than zero seconds (was: " + lengthSec + " seconds)");
            if (lengthSec > 600)
                throw new ArgumentException("Length of recording must be less than 10 minutes (was: " + lengthSec + " seconds)");
            if (frequency <= 0)
                throw new ArgumentException("Frequency of recording must be greater than zero (was: " + frequency + " Hz)");
            if (frequency > 48000)
                throw new ArgumentException("Frequency of recording must be less than 48000 (was: " + frequency + " Hz)");

            if (IsRecording(deviceName))
                return _microphoneClip;

            if (_microphoneClip != null)
                MonoBehaviour.Destroy(_microphoneClip);

            _microphoneClip = AudioClip.Create("Microphone", frequency * lengthSec, _channels, frequency, false);

            _leftChannelBuffer = new float[frequency * lengthSec];
            _rightChannelBuffer = new float[frequency * lengthSec];
            _mainBuffer = new float[frequency * lengthSec * _channels];

            initSamplesMemoryData(_leftChannelBuffer, _leftChannelBuffer.Length, 0);
            initSamplesMemoryData(_rightChannelBuffer, _rightChannelBuffer.Length, 1);

            setLeapSync(leapSync ? 1 : 0);

            start(microphoneDeviceIDFromName, frequency, loop ? 1 : 0, lengthSec);

            return _microphoneClip;
#else
            if (_microphoneClip != null)
                MonoBehaviour.Destroy(_microphoneClip);

            _microphoneClip = UnityEngine.Microphone.Start(deviceName, loop, lengthSec, frequency);
            return _microphoneClip;
#endif
        }

        /// <summary>
        ///   <para>Query if a device is currently recording.</para>
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        public static bool IsRecording(string deviceName)
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            return isRecording() == 1;
#else
            return UnityEngine.Microphone.IsRecording(deviceName);
#endif
        }

        public static void GetDeviceCaps(string deviceName, out int minFreq, out int maxfreq)
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            int[] array = Deserialize<SimpleDeviceCapsData>(getDeviceCaps()).caps;
            minFreq = array[0];
            maxfreq = array[1];
#else
            UnityEngine.Microphone.GetDeviceCaps(deviceName, out minFreq, out maxfreq);
#endif
        }

        /// <summary>
        ///   <para>Get the position in samples of the recording.</para>
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        public static int GetPosition(string deviceName)
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            return getPosition();
#else
            return UnityEngine.Microphone.GetPosition(deviceName);
#endif
        }

        /// <summary>
        ///   <para>Stops recording.</para>
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        public static void End(string deviceName)
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            end();

            Interleave(_mainBuffer, 0);
            _microphoneClip.SetData(_mainBuffer, 0);
#else
            UnityEngine.Microphone.End(deviceName);
#endif
        }

        /// <summary>
        ///   <para>Request that the user grant access to a Microphone.</para>
        /// </summary>
        public static void RequestPermission()
        {
#if UNITY_ANDROID
            PermissionCallbacks permissionCallbacks = new PermissionCallbacks();
            permissionCallbacks.PermissionGranted += (permission) => { PermissionChangedEvent?.Invoke(true); };
            permissionCallbacks.PermissionDenied += (permission) => { PermissionChangedEvent?.Invoke(false); };
            permissionCallbacks.PermissionDeniedAndDontAskAgain += (permission) => { PermissionChangedEvent?.Invoke(false); };

            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                Permission.RequestUserPermission(Permission.Microphone, permissionCallbacks);
            else
                PermissionChangedEvent?.Invoke(true);
#elif UNITY_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                var operation = Application.RequestUserAuthorization(UserAuthorization.Microphone);
                operation.completed += (operation) => { PermissionChangedEvent?.Invoke(Application.HasUserAuthorization(UserAuthorization.Microphone)); };
            }
            else
                PermissionChangedEvent?.Invoke(true);
#elif (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            if (isPermissionGranted() == 0)
            {
                // TODO: do nothing
            }
#endif
        }

        /// <summary>
        ///   <para>Fills an array with sample data from the clip.</para>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offsetSamples"></param>
        public static bool GetData(float[] data, int offsetSamples)
        {
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
            return Interleave(data, offsetSamples);
#else
            if (_microphoneClip == null || !_microphoneClip)
                return false;
            return _microphoneClip.GetData(data, offsetSamples);
#endif
        }

        /// <summary>
        ///   <para>Resamples of samples array from sourceFrequency into targetFrequency.</para>
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sourceFrequency"></param>
        /// <param name="targetFrequency"></param>
        /// <returns></returns>
        public static float[] ResampleData(float[] data, int sourceFrequency, int targetFrequency)
        {
            if (targetFrequency == sourceFrequency)
                return data;

            int sampleRateRatio = sourceFrequency / targetFrequency;
            int newLength = data.Length / sampleRateRatio;
            float[] result = new float[newLength];
            int offsetResult = 0;
            int offsetBuffer = 0;

            while (offsetResult < result.Length)
            {
                int nextOffsetBuffer = (offsetResult + 1) * sampleRateRatio;
                float accum = 0,
                count = 0;

                for (int i = offsetBuffer; i < nextOffsetBuffer && i < data.Length; i++)
                {
                    accum += data[i];
                    count++;
                }

                result[offsetResult] = accum / count;
                offsetResult++;
                offsetBuffer = nextOffsetBuffer;
            }

            return result;
        }

#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
        [MonoPInvokeCallback(typeof(NativeCommand))]
        private static void HandleNativeCommandExecution(string json)
        {
            try
            {
                Command<object> command = Deserialize<SimpleCommandData<object>>(json).command;

                switch ((CommandType)Enum.Parse(typeof(CommandType), command.command))
                {
                    case CommandType.PermissionChanged:
                        {
                            var internalCommand = Deserialize<SimpleCommandData<bool>>(json).command;
                            PermissionChangedEvent?.Invoke(internalCommand.data);
                        }
                        break;
                    case CommandType.StreamChunkReceived:
                        {
                            var internalCommand = Deserialize<SimpleCommandData<string>>(json).command;

                            string[] split = internalCommand.data.Split(':');
                            int index = int.Parse(split[0]);
                            int length = int.Parse(split[1]);

                            List<float[]> channelsData = new List<float[]>();
                            float[] array;
                            float[] chunk;
                            int pointer;
                            int positionPointer;
                            for (int i = 0; i < _channels; i++)
                            {
                                array = i == 0 ? _leftChannelBuffer : _rightChannelBuffer;
                                chunk = new float[length];
                                pointer = 0;
                                positionPointer = index;
                                for (int j = index; j < index + length; j++)
                                {
                                    chunk[pointer] = array[positionPointer];
                                    pointer++;
                                    positionPointer++;

                                    if (positionPointer >= array.Length)
                                        positionPointer = 0;
                                }
                                channelsData.Add(chunk);
                            }

                            RecordStreamDataEvent?.Invoke(new StreamData(channelsData));
                        }
                        break;
                }

                if (Logging)
                {
                    UnityEngine.Debug.Log($"Executed command: {command.command}");
                }
            }
            catch (Exception ex)
            {
                if (Logging)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static T Deserialize<T>(string json)
        {
            if (Logging)
            {
                UnityEngine.Debug.Log($"Deserialize: {json}");
            }

            return UnityEngine.JsonUtility.FromJson<T>(json);
        }

        private static bool Interleave(float[] result, int offset)
        {
            if (_leftChannelBuffer == null || _leftChannelBuffer.Length == 0)
            {
                result = null;
                return false;
            }

            int length = _leftChannelBuffer.Length + _rightChannelBuffer.Length;
            int index = 0, inputIndex = Mathf.Clamp(offset, 0, _leftChannelBuffer.Length - 1);

            while (index < length)
            {
                if (index >= result.Length || inputIndex >= _leftChannelBuffer.Length)
                {
                    result = null;
                    return false;// tried to move over array bounds
                }

                result[index++] = _leftChannelBuffer[inputIndex];
                result[index++] = _rightChannelBuffer[inputIndex];
                inputIndex++;
            }

            return true;
        }

        private static string GetMicrophoneDeviceIDFromName(string deviceName)
        {
            return _devices?.Find(device => device.label == deviceName)?.deviceId;
        }
#endif
        public class StreamData
        {
            /// <summary>
            /// Returns channels data, first left then right channel
            /// </summary>
            public readonly List<float[]> ChannelsData;

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public StreamData()
            {
                ChannelsData = new List<float[]>();
            }

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public StreamData(List<float[]> channelsData)
            {
                ChannelsData = channelsData;
            }
        }
#if (UNITY_WEBGL && !UNITY_EDITOR) || FORCE_WEBGL_MIC
        [Serializable]
        private class Command<T>
        {
            public string command;
            public T data;

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public Command()
            {
            }

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public Command(string command, T data)
            {
                this.command = command;
                this.data = data;
            }
        }

        [Serializable]
        private class Device
        {
            public string deviceId;
            public string kind;
            public string label;
            public string groupId;

#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public Device()
            {
            }
#if UNITY_2018_4_OR_NEWER
            [UnityEngine.Scripting.Preserve]
#endif
            public Device(string deviceId, string kind, string label, string groupId)
            {
                this.deviceId = deviceId;
                this.kind = kind;
                this.label = label;
                this.groupId = groupId;
            }
        }

        [Serializable]
        private class SimpleDevicesData
        {
            public List<Device> devices;
        }

        [Serializable]
        private class SimpleDeviceCapsData
        {
            public int[] caps;
        }

        [Serializable]
        private class SimpleCommandData<T>
        {
            public Command<T> command;
        }

        private enum CommandType
        {
            PermissionChanged,
            StreamChunkReceived
        }
#endif
    }
}