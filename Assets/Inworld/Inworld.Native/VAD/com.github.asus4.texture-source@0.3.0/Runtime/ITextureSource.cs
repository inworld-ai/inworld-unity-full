namespace TextureSource
{
    using UnityEngine;

    /// <summary>
    /// Interface for the source
    /// </summary>
    public interface ITextureSource
    {
        bool DidUpdateThisFrame { get; }
        Texture Texture { get; }

        void Start();
        void Stop();
        void Next();
    }
}
