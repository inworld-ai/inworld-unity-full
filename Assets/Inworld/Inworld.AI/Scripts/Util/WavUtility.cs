/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Inworld
{
    public class WavUtility
    {
        /// <summary>
        /// Gets the wave file from Unity's Resource folder.
        /// </summary>
        /// <param name="filePath">the file path to load the wave data</param>
        public static AudioClip ToAudioClip(string filePath)
        {
            if (filePath.StartsWith(Application.persistentDataPath) || filePath.StartsWith(Application.dataPath))
                return ToAudioClip(File.ReadAllBytes(filePath));
            Debug.LogWarning("This only supports files that are stored using Unity's Application data path. \nTo load bundled resources use 'Resources.Load(\"filename\") typeof(AudioClip)' method. \nhttps://docs.unity3d.com/ScriptReference/Resources.Load.html");
            return null;
        }
        /// <summary>
        /// Generate the Audio clip by byte array.
        /// </summary>
        /// <param name="fileBytes">the date to convert.</param>
        /// <param name="offsetSamples">the offset of the audio.</param>
        /// <param name="name">the name of the wav file.</param>
        public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
        {
            int int32_1 = BitConverter.ToInt32(fileBytes, 16);
            ushort uint16_1 = BitConverter.ToUInt16(fileBytes, 20);
            string str = FormatCode(uint16_1);
            Debug.AssertFormat((uint16_1 == 1 ? 1 : uint16_1 == 65534 ? 1 : 0) != 0, "Detected format code '{0}' {1}, but only PCM and WaveFormatExtensable uncompressed formats are currently supported.", uint16_1, str);
            ushort uint16_2 = BitConverter.ToUInt16(fileBytes, 22);
            int int32_2 = BitConverter.ToInt32(fileBytes, 24);
            ushort uint16_3 = BitConverter.ToUInt16(fileBytes, 34);
            int num = 20 + int32_1 + 4;
            int int32_3 = BitConverter.ToInt32(fileBytes, num);
            float[] audioClipData;
            switch (uint16_3)
            {
                case 8:
                    audioClipData = Convert8BitByteArrayToAudioClipData(fileBytes, num, int32_3);
                    break;
                case 16:
                    audioClipData = Convert16BitByteArrayToAudioClipData(fileBytes, num, int32_3);
                    break;
                case 24:
                    audioClipData = Convert24BitByteArrayToAudioClipData(fileBytes, num, int32_3);
                    break;
                case 32:
                    audioClipData = Convert32BitByteArrayToAudioClipData(fileBytes, num, int32_3);
                    break;
                default:
                    throw new Exception(uint16_3 + " bit depth is not supported.");
            }
            AudioClip audioClip = AudioClip.Create(name, audioClipData.Length, uint16_2, int32_2, false);
            audioClip.SetData(audioClipData, offsetSamples);
            return audioClip;
        }

        /// <summary>
        /// Convert the audio clip float data to int16 array then convert to byte array.
        /// Short array is the data format we use in the Inworld server.
        /// </summary>
        /// <param name="input">the audio clip data.</param>
        /// <param name="size">the size of the wave data.</param>
        /// <param name="output">the short array.</param>
        public static void ConvertAudioClipDataToInt16ByteArray
        (
            IReadOnlyList<float> input,
            int size,
            byte[] output
        )
        {
            MemoryStream memoryStream = new MemoryStream(output);
            for (int index = 0; index < size; ++index)
                memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(input[index] * short.MaxValue)), 0, 2);
            memoryStream.Dispose();
        }
        /// <summary>
        /// Convert the audio clip float data to int array.
        /// Still keep the API but Inworld don't process int array.
        /// </summary>
        /// <param name="input">the audio clip data.</param>
        /// <param name="size">the size of the wave data.</param>
        /// <param name="output">the int32 array.</param>
        public static void ConvertAudioClipDataToInt32ByteArray
        (
            IReadOnlyList<float> input,
            int size,
            byte[] output
        )
        {
            MemoryStream memoryStream = new MemoryStream(output);
            for (int index = 0; index < size; ++index)
            {
                int intValue = Convert.ToInt32(input[index] * int.MaxValue);
                memoryStream.Write(BitConverter.GetBytes(intValue), 0, 4);
            }
            memoryStream.Dispose();
        }
        /// <summary>
        /// Convert the audio clip float data to short array.
        /// Short array is the data format we use in the Inworld server.
        /// </summary>
        /// <param name="input">the audio clip data.</param>
        /// <param name="size">the size of the wave data.</param>
        public static short[] ConvertAudioClipDataToInt16Array(IReadOnlyList<float> input, int size)
        {
            if (input == null || input.Count == 0)
                return null;
            short[] output = new short[size];

            for (int index = 0; index < size; ++index)
            {
                output[index] = (short)(input[index] * short.MaxValue);
            }
            return output;
        }
        /// <summary>
        /// Get the byte array of the wave data from AudioClip
        /// </summary>
        /// <param name="audioClip">the input audio clip</param>
        public static byte[] FromAudioClip(AudioClip audioClip)
        {
            return FromAudioClip(audioClip, out string _, false);
        }
        /// <summary>
        /// Get the byte array of the wave data from AudioClip
        /// </summary>
        /// <param name="audioClip">the input audio clip</param>
        /// <param name="filepath">the file path of the wave file.</param>
        /// <param name="saveAsFile">check if the data is saved as file</param>
        /// <param name="dirname">the directory of the wave file.</param>
        /// <returns></returns>
        public static byte[] FromAudioClip
        (
            AudioClip audioClip,
            out string filepath,
            bool saveAsFile = true,
            string dirname = "recordings"
        )
        {
            MemoryStream stream = new MemoryStream();
            ushort bitDepth = 16;
            int fileSize = audioClip.samples * 2 + 44;
            WriteFileHeader(ref stream, fileSize);
            WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
            WriteFileData(ref stream, audioClip, bitDepth);
            byte[] array = stream.ToArray();
            Debug.AssertFormat((array.Length == fileSize ? 1 : 0) != 0, "Unexpected AudioClip to wav format byte count: {0} == {1}", array.Length, fileSize);
            if (saveAsFile)
            {
                filepath = string.Format("{0}/{1}/{2}.{3}", Application.persistentDataPath, dirname, DateTime.UtcNow.ToString("yyMMdd-HHmmss-fff"), "wav");
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                File.WriteAllBytes(filepath, array);
            }
            else
                filepath = null;
            stream.Dispose();
            return array;
        }
        /// <summary>
        /// Convert the float array to int16 array then convert to byte array.
        /// Short array is the data format we use in the Inworld server.
        /// </summary>
        /// <param name="data">the float array of wave data.</param>
        public static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
        {
            MemoryStream memoryStream = new MemoryStream();
            int count = 2;
            short maxValue = short.MaxValue;
            for (int index = 0; index < data.Length; ++index)
                memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[index] * maxValue)), 0, count);
            byte[] array = memoryStream.ToArray();
            Debug.AssertFormat((data.Length * count == array.Length ? 1 : 0) != 0, "Unexpected float[] to Int16 to byte[] size: {0} == {1}", data.Length * count, array.Length);
            memoryStream.Dispose();
            return array;
        }
        /// <summary>
        /// Get the bit depth of the audio clip
        /// </summary>
        /// <param name="audioClip">the target clip to sample.</param>
        /// <returns></returns>
        public static ushort BitDepth(AudioClip audioClip)
        {
            ushort uint16 = Convert.ToUInt16(audioClip.samples * audioClip.channels * audioClip.length / audioClip.frequency);
            int num;
            switch (uint16)
            {
                case 8:
                case 16:
                    num = 1;
                    break;
                default:
                    num = uint16 == 32 ? 1 : 0;
                    break;
            }
            object[] objArray = new object[1] {uint16};
            Debug.AssertFormat(num != 0, "Unexpected AudioClip bit depth: {0}. Expected 8 or 16 or 32 bit.", objArray);
            return uint16;
        }
        static float[] Convert8BitByteArrayToAudioClipData
        (
            byte[] source,
            int headerOffset,
            int dataSize
        )
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 <= 0 ? 0 : int32 == dataSize ? 1 : 0) != 0, "Failed to get valid 8-bit wav size: {0} from data bytes: {1} at offset: {2}", int32, dataSize, headerOffset);
            float[] audioClipData = new float[int32];
            sbyte maxValue = sbyte.MaxValue;
            for (int index = 0; index < int32; ++index)
                audioClipData[index] = source[index] / (float)maxValue;
            return audioClipData;
        }

        static float[] Convert16BitByteArrayToAudioClipData
        (
            byte[] source,
            int headerOffset,
            int dataSize
        )
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 <= 0 ? 0 : int32 == dataSize ? 1 : 0) != 0, "Failed to get valid 16-bit wav size: {0} from data bytes: {1} at offset: {2}", int32, dataSize, headerOffset);
            int num = 2;
            int length = int32 / num;
            float[] audioClipData = new float[length];
            short maxValue = short.MaxValue;
            for (int index = 0; index < length; ++index)
            {
                int startIndex = index * num + headerOffset;
                audioClipData[index] = BitConverter.ToInt16(source, startIndex) / (float)maxValue;
            }
            Debug.AssertFormat((audioClipData.Length == length ? 1 : 0) != 0, "AudioClip .wav data is wrong size: {0} == {1}", audioClipData.Length, length);
            return audioClipData;
        }

        static float[] Convert24BitByteArrayToAudioClipData
        (
            byte[] source,
            int headerOffset,
            int dataSize
        )
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 <= 0 ? 0 : int32 == dataSize ? 1 : 0) != 0, "Failed to get valid 24-bit wav size: {0} from data bytes: {1} at offset: {2}", int32, dataSize, headerOffset);
            int count = 3;
            int length = int32 / count;
            int maxValue = int.MaxValue;
            float[] audioClipData = new float[length];
            byte[] dst = new byte[4];
            for (int index = 0; index < length; ++index)
            {
                int srcOffset = index * count + headerOffset;
                Buffer.BlockCopy(source, srcOffset, dst, 1, count);
                audioClipData[index] = BitConverter.ToInt32(dst, 0) / (float)maxValue;
            }
            Debug.AssertFormat((audioClipData.Length == length ? 1 : 0) != 0, "AudioClip .wav data is wrong size: {0} == {1}", audioClipData.Length, length);
            return audioClipData;
        }
        static float[] Convert32BitByteArrayToAudioClipData
        (
            byte[] source,
            int headerOffset,
            int dataSize
        )
        {
            int int32 = BitConverter.ToInt32(source, headerOffset);
            headerOffset += 4;
            Debug.AssertFormat((int32 <= 0 ? 0 : int32 == dataSize ? 1 : 0) != 0, "Failed to get valid 32-bit wav size: {0} from data bytes: {1} at offset: {2}", int32, dataSize, headerOffset);
            int num = 4;
            int length = int32 / num;
            int maxValue = int.MaxValue;
            float[] audioClipData = new float[length];
            for (int index = 0; index < length; ++index)
            {
                int startIndex = index * num + headerOffset;
                audioClipData[index] = BitConverter.ToInt32(source, startIndex) / (float)maxValue;
            }
            Debug.AssertFormat((audioClipData.Length == length ? 1 : 0) != 0, "AudioClip .wav data is wrong size: {0} == {1}", audioClipData.Length, length);
            return audioClipData;
        }
        static int WriteFileHeader(ref MemoryStream stream, int fileSize)
        {
            int num1 = 0;
            int num2 = 12;
            byte[] bytes1 = Encoding.ASCII.GetBytes("RIFF");
            int num3 = num1 + WriteBytesToMemoryStream(ref stream, bytes1, "ID");
            int num4 = fileSize - 8;
            int num5 = num3 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num4), "CHUNK_SIZE");
            byte[] bytes2 = Encoding.ASCII.GetBytes("WAVE");
            int num6 = num5 + WriteBytesToMemoryStream(ref stream, bytes2, "FORMAT");
            Debug.AssertFormat((num6 == num2 ? 1 : 0) != 0, "Unexpected wav descriptor byte count: {0} == {1}", num6, num2);
            return num6;
        }
        static int WriteFileFormat
        (
            ref MemoryStream stream,
            int channels,
            int sampleRate,
            ushort bitDepth
        )
        {
            int num1 = 0;
            int num2 = 24;
            byte[] bytes = Encoding.ASCII.GetBytes("fmt ");
            int num3 = num1 + WriteBytesToMemoryStream(ref stream, bytes, "FMT_ID");
            int num4 = 16;
            int num5 = num3 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num4), "SUBCHUNK_SIZE");
            ushort num6 = 1;
            int num7 = num5 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num6), "AUDIO_FORMAT");
            ushort uint16_1 = Convert.ToUInt16(channels);
            int num8 = num7 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(uint16_1), "CHANNELS") + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(sampleRate), "SAMPLE_RATE");
            int num9 = sampleRate * channels * BytesPerSample(bitDepth);
            int num10 = num8 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num9), "BYTE_RATE");
            ushort uint16_2 = Convert.ToUInt16(channels * BytesPerSample(bitDepth));
            int num11 = num10 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(uint16_2), "BLOCK_ALIGN") + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(bitDepth), "BITS_PER_SAMPLE");
            Debug.AssertFormat((num11 == num2 ? 1 : 0) != 0, "Unexpected wav fmt byte count: {0} == {1}", num11, num2);
            return num11;
        }
        static int WriteFileData(ref MemoryStream stream, AudioClip audioClip, ushort bitDepth)
        {
            int num1 = 0;
            int num2 = 8;
            float[] data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);
            byte[] int16ByteArray = ConvertAudioClipDataToInt16ByteArray(data);
            byte[] bytes = Encoding.ASCII.GetBytes("data");
            int num3 = num1 + WriteBytesToMemoryStream(ref stream, bytes, "DATA_ID");
            int int32 = Convert.ToInt32(audioClip.samples * 2);
            int num4 = num3 + WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(int32), "SAMPLES");
            Debug.AssertFormat((num4 == num2 ? 1 : 0) != 0, "Unexpected wav data id byte count: {0} == {1}", num4, num2);
            int num5 = num4 + WriteBytesToMemoryStream(ref stream, int16ByteArray, "DATA");
            Debug.AssertFormat((int16ByteArray.Length == int32 ? 1 : 0) != 0, "Unexpected AudioClip to wav subchunk2 size: {0} == {1}", int16ByteArray.Length, int32);
            return num5;
        }
        static int WriteBytesToMemoryStream(ref MemoryStream stream, byte[] bytes, string tag = "")
        {
            int length = bytes.Length;
            stream.Write(bytes, 0, length);
            return length;
        }
        static int BytesPerSample(ushort bitDepth)
        {
            return bitDepth / 8;
        }
        static int BlockSize(ushort bitDepth)
        {
            switch (bitDepth)
            {
                case 8:
                    return 1;
                case 16:
                    return 2;
                case 32:
                    return 4;
                default:
                    throw new Exception(bitDepth + " bit depth is not supported.");
            }
        }
        static string FormatCode(ushort code)
        {
            switch (code)
            {
                case 1:
                    return "PCM";
                case 2:
                    return "ADPCM";
                case 3:
                    return "IEEE";
                case 7:
                    return "μ-law";
                case 65534:
                    return "WaveFormatExtensable";
                default:
                    Debug.LogWarning("Unknown wav code format:" + code);
                    return "";
            }
        }
    }
}
