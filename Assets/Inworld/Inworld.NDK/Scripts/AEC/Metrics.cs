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


