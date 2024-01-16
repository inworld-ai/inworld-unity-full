/*************************************************************************************************
 * Copyright 2022-2024 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System;
using System.Runtime.InteropServices;

namespace Inworld.AEC
{
	public class AECInterop
	{
		const string DLL_NAME = "webrtc_aec_plugin";

        /// <summary>
        /// Create a pointer of AECHandle in plugin dll.
        /// </summary>
        /// <param name="sample_rate_hz">the sample rate of the audio (has to be 16000 if use Inworld's wave data)</param>
		[DllImport(DLL_NAME)]
		public static extern IntPtr WebRtcAec3_Create(int sample_rate_hz);

        /// <summary>
        /// Free the AECHandle. Please call it when destroy.
        /// </summary>
        /// <param name="handle">the pointer of AECHandle to destroy.</param>
		[DllImport(DLL_NAME)]
		public static extern void WebRtcAec3_Free(IntPtr handle);

        /// <summary>
        /// Sample the far end data. Far end audio is the current ambisonic environment of the audio data.
        /// </summary>
        /// <param name="handle">the current pointer AECHandle to use.</param>
        /// <param name="farend">the far end data to remove.</param>
        /// <returns></returns>
		[DllImport(DLL_NAME)]
		public static extern int WebRtcAec3_BufferFarend(IntPtr handle, short[] farend);
        /// <summary>
        /// Process the actual echo removal.
        /// </summary>
        /// <param name="handle">>the current pointer AECHandle to use.</param>
        /// <param name="nearend">the near end (microphone) data that needs to process.</param>
        /// <param name="output">the near end data will substract the far end data, to keep the microphone voice only.</param>
        /// <returns></returns>
		[DllImport(DLL_NAME)]
		public static extern int WebRtcAec3_Process(IntPtr handle, short[] nearend, short[] output);
	}
}
