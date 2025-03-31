namespace TextureSource
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Transforms 2D texture with any arbitrary matrix
    /// </summary>
    public class TextureTransformer : IDisposable
    {
        private static readonly int _InputTex = Shader.PropertyToID("_InputTex");
        private static readonly int _OutputTex = Shader.PropertyToID("_OutputTex");
        private static readonly int _OutputTexSize = Shader.PropertyToID("_OutputTexSize");
        private static readonly int _TransformMatrix = Shader.PropertyToID("_TransformMatrix");

        private static readonly Matrix4x4 PopMatrix = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0));
        private static readonly Matrix4x4 PushMatrix = Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0));

        public static readonly Lazy<ComputeShader> DefaultComputeShader = new(()
            => Resources.Load<ComputeShader>("com.github.asus4.texture-source/TextureTransform"));

        private readonly ComputeShader compute;
        private readonly int kernel;
        private RenderTexture texture;
        public readonly int width;
        public readonly int height;

        public RenderTexture Texture => texture;

        public TextureTransformer(int width, int height, ComputeShader shader = null)
        {
            compute = shader != null
                ? shader
                : DefaultComputeShader.Value;
            kernel = compute.FindKernel("TextureTransform");

            this.width = width;
            this.height = height;

            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32)
            {
                enableRandomWrite = true,
                useMipMap = false,
                depthBufferBits = 0,
                // sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear,
            };
            texture = new RenderTexture(desc);
            texture.Create();
        }

        public void Dispose()
        {
            if (texture != null)
            {
                texture.Release();
                UnityEngine.Object.Destroy(texture);
            }
            texture = null;
        }

        /// <summary>
        /// Transform with a matrix
        /// </summary>
        /// <param name="input">A input texture</param>
        /// <param name="t">A matrix</param>
        /// <returns>The transformed texture</returns>
        public RenderTexture Transform(Texture input, Matrix4x4 t)
        {
            compute.SetTexture(kernel, _InputTex, input, 0);
            compute.SetTexture(kernel, _OutputTex, texture, 0);
            compute.SetInts(_OutputTexSize, texture.width, texture.height);
            compute.SetMatrix(_TransformMatrix, t);
            compute.Dispatch(kernel, Mathf.CeilToInt(texture.width / 8f), Mathf.CeilToInt(texture.height / 8f), 1);
            return texture;
        }

        /// <summary>
        /// Transform with offset, rotation, and scale
        /// </summary>
        /// <param name="input">A input texture</param>
        /// <param name="offset">A 2D offset</param>
        /// <param name="eulerRotation">A rotation in euler angles</param>
        /// <param name="scale">A scale</param>
        /// <returns>The transformed texture</returns>
        public RenderTexture Transform(Texture input, Vector2 offset, float eulerRotation, Vector2 scale)
        {
            Matrix4x4 trs = Matrix4x4.TRS(
                new Vector3(-offset.x, -offset.y, 0),
                Quaternion.Euler(0, 0, -eulerRotation),
                new Vector3(1f / scale.x, 1f / scale.y, 1));
            return Transform(input, PopMatrix * trs * PushMatrix);
        }

        /// <summary>
        /// Transform with multiple textures
        /// </summary>
        /// <param name="propertyIds">An array of property name IDs associated with each texture</param>
        /// <param name="textures">An array of textures</param>
        /// <param name="t">A matrix</param>
        /// <returns>The transformed texture</returns>
        public RenderTexture Transform(ReadOnlySpan<int> propertyIds, ReadOnlySpan<Texture> textures, Matrix4x4 t)
        {
            for (int i = 0; i < propertyIds.Length; i++)
            {
                compute.SetTexture(kernel, propertyIds[i], textures[i], 0);
            }
            compute.SetTexture(kernel, _OutputTex, texture, 0);
            compute.SetInts(_OutputTexSize, texture.width, texture.height);
            compute.SetMatrix(_TransformMatrix, t);
            compute.Dispatch(kernel, Mathf.CeilToInt(texture.width / 8f), Mathf.CeilToInt(texture.height / 8f), 1);
            return texture;
        }
    }
}
