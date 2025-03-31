namespace TextureSource
{
    using UnityEngine;

    /// <summary>
    /// Abstract class for the source.
    /// </summary>
    public abstract class BaseTextureSource : ScriptableObject, ITextureSource
    {
        public abstract bool DidUpdateThisFrame { get; }
        public abstract Texture Texture { get; }
        public abstract void Start();
        public abstract void Stop();
        public abstract void Next();
    }
}
