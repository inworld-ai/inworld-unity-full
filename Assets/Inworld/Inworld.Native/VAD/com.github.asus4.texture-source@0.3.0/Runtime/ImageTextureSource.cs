namespace TextureSource
{
    using UnityEngine;

    /// <summary>
    /// Source from image texture
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Texture Source/Image", fileName = "ImageTextureSource")]
    public class ImageTextureSource : BaseTextureSource
    {
        [SerializeField]
        private Texture[] textures = default;

        [SerializeField]
        private bool sendContinuousUpdate = false;

        public override bool DidUpdateThisFrame
        {
            get
            {
                if (sendContinuousUpdate)
                {
                    return true;
                }

                bool updated = isUpdated;
                isUpdated = false;
                return updated;
            }
        }

        public override Texture Texture => textures[currentIndex];

        private int currentIndex = 0;
        private bool isUpdated = false;

        public override void Start()
        {
            if (textures.Length == 0)
            {
                Debug.LogError("No texture is set");
                return;
            }
            isUpdated = true;
        }

        public override void Stop()
        {
            isUpdated = false;
        }

        public override void Next()
        {
            currentIndex = (currentIndex + 1) % textures.Length;
            isUpdated = true;
        }
    }
}
