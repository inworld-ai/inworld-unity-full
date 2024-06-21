using System.Runtime.InteropServices;
namespace Inworld.Inworld.Native.VAD
{
    public class VADInterop
    {
        const string DLL_NAME = "inworld-ndk-vad";
        
        /// <summary>
        /// Initialize the VAD plugin with the prebuilt onnx file.
        /// We cannot use Sentis for now, because Sentis does not support IF in model.
        /// 
        /// TODO(Yan): Replace with Sentis directly when it's supported.
        /// </summary>
        /// <param name="model">the onnx model to load.</param>
        [DllImport(DLL_NAME)]
        public static extern void VAD_Initialize(string model);

        /// <summary>
        /// Free the VAD Handle
        /// </summary>
        [DllImport(DLL_NAME)]
        public static extern void VAD_Terminate();

        /// <summary>
        /// Sample the far end data. Far end audio is the current ambisonic environment of the audio data.
        /// </summary>
        /// <param name="audioData">the float array of the audio data we're sending.</param>
        /// <param name="size">the size of the audio data.</param>
        /// <returns></returns>
        [DllImport(DLL_NAME)]
        public static extern float VAD_Process(float[] audioData, int size);
        /// <summary>
        /// Process the actual echo removal.
        /// </summary>
        /// <param name="handle">>the current pointer AECHandle to use.</param>
        /// <param name="nearend">the near end (microphone) data that needs to process.</param>
        /// <param name="output">the near end data will substract the far end data, to keep the microphone voice only.</param>
        /// <returns></returns>
        [DllImport(DLL_NAME)]
        public static extern void VAD_ResetState();
    }
}
