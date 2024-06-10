using UnityEngine;
using NAudio.Wave;

public class AudioPlayer : MonoBehaviour
{
    private IWavePlayer wavePlayer;
    private WaveStream waveStream;
    private string audioFilePath = "./DebugInput.wav";
    void Start()
    {
        // Create WaveStream obj for stream
        waveStream = new WaveFileReader(audioFilePath);

        // Create WaveOut obj for play stream
        wavePlayer = new WaveOut();
        wavePlayer.Init(waveStream);
        wavePlayer.Play();
    }

    void OnApplicationQuit()
    {
        //stop play
        if (wavePlayer != null)
        {
            wavePlayer.Stop();
            wavePlayer.Dispose();
        }
        if (waveStream != null)
        {
            waveStream.Dispose();
        }
    }
}