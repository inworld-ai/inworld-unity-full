/*************************************************************************************************
 * Copyright 2022 Theai, Inc. (DBA Inworld)
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/
using System.Runtime.InteropServices;

namespace Inworld.AEC
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Metrics
	{
		public double echo_return_loss;
		public double echo_return_loss_enhancement;
		public int delay_ms;
	}
}


