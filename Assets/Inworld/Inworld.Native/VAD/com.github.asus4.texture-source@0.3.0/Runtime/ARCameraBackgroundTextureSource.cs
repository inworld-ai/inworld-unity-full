// Only available with AR Foundation
#if MODULE_ARFOUNDATION_ENABLED
namespace TextureSource
{
    using System;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    /// <summary>
    /// Source from ARFoundation
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Texture Source/ARFoundation", fileName = "ARFoundationTextureSource")]
    public sealed class ARFoundationTextureSource : BaseTextureSource
    {
        private static readonly int _DisplayTransformID = Shader.PropertyToID("_UnityDisplayTransform");

        private ARCameraManager cameraManager;
        private RenderTexture texture;
        private Material material;
        private int lastUpdatedFrame = -1;

        private static readonly Lazy<Shader> ARCameraBackgroundShader = new(() =>
        {
            string shaderName = Application.platform switch
            {
                RuntimePlatform.Android => "Unlit/ARCoreBackground",
                RuntimePlatform.IPhonePlayer => "Unlit/ARKitBackground",
#if UNITY_ANDROID
                _ => "Unlit/ARCoreBackground",
#elif UNITY_IOS
                _ => "Unlit/ARKitBackground",
#else
                _ => throw new NotSupportedException($"ARFoundationTextureSource is not supported on {Application.platform}"),
#endif
            };
            return Shader.Find(shaderName);
        });

        public override bool DidUpdateThisFrame => lastUpdatedFrame == Time.frameCount;
        public override Texture Texture => texture;

        public override void Start()
        {
            cameraManager = FindAnyObjectByType<ARCameraManager>();
            if (cameraManager == null)
            {
                throw new InvalidOperationException("ARCameraManager is not found");
            }

            var shader = ARCameraBackgroundShader.Value;
            material = new Material(shader);

            cameraManager.frameReceived += OnFrameReceived;
        }

        public override void Stop()
        {
            if (cameraManager != null)
            {
                cameraManager.frameReceived -= OnFrameReceived;
            }

            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
                texture = null;
            }

            if (material != null)
            {
                Destroy(material);
                material = null;
            }
        }

        public override void Next()
        {
            if (cameraManager == null)
            {
                return;
            }
            // Switch the camera facing direction.
            cameraManager.requestedFacingDirection = cameraManager.currentFacingDirection switch
            {
                CameraFacingDirection.World => CameraFacingDirection.User,
                CameraFacingDirection.User => CameraFacingDirection.World,
                _ => CameraFacingDirection.World,
            };
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            // Find best texture size
            int bestWidth = 0;
            int bestHeight = 0;
            int count = args.textures.Count;
            for (int i = 0; i < count; i++)
            {
                var tex = args.textures[i];
                bestWidth = Math.Max(bestWidth, tex.width);
                bestHeight = Math.Max(bestHeight, tex.height);
                material.SetTexture(args.propertyNameIds[i], tex);
            }

            // Swap if screen is portrait
            float screenAspect = (float)Screen.width / Screen.height;
            if (bestWidth > bestHeight && screenAspect < 1f)
            {
                (bestWidth, bestHeight) = (bestHeight, bestWidth);
            }

            // Create render texture
            Utils.GetTargetSizeScale(
               new Vector2Int(bestWidth, bestHeight), screenAspect,
                out Vector2Int dstSize, out Vector2 scale);
            EnsureRenderTexture(dstSize.x, dstSize.y);

            // SetMaterialKeywords(material, args.enabledMaterialKeywords, args.disabledMaterialKeywords);

            if (args.displayMatrix.HasValue)
            {
                material.SetMatrix(_DisplayTransformID, args.displayMatrix.Value);
            }

            Graphics.Blit(null, texture, material);

            lastUpdatedFrame = Time.frameCount;
        }

        private void EnsureRenderTexture(int width, int height)
        {
            if (texture == null || texture.width != width || texture.height != height)
            {
                if (texture != null)
                {
                    texture.Release();
                    texture = null;
                }
                int depth = 32;
                texture = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32);
                texture.Create();
            }
        }
    }
}
#endif // MODULE_ARFOUNDATION_ENABLED
