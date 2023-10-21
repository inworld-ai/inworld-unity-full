/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
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

		[DllImport(DLL_NAME)]
		public static extern IntPtr WebRtcAec3_Create(int sample_rate_hz);

		[DllImport(DLL_NAME)]
		public static extern void WebRtcAec3_Free(IntPtr handle);

		[DllImport(DLL_NAME)]
		public static extern int WebRtcAec3_BufferFarend(IntPtr handle, short[] farend);

		[DllImport(DLL_NAME)]
		public static extern int WebRtcAec3_Process(IntPtr handle, short[] nearend, short[] output);

		[DllImport(DLL_NAME)]
		public static extern void WebRtcAec3_GetMetrics(IntPtr handle, ref Metrics metrics);
	}
}
