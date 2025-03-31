namespace TextureSource
{
    using UnityEngine;

    /// <summary>
    /// Internal utility functions
    /// </summary>
    internal static class Utils
    {
        public static void GetTargetSizeScale(
            Vector2Int srcSize, float dstAspect,
            out Vector2Int dstSize, out Vector2 scale)
        {
            float srcAspect = (float)srcSize.x / srcSize.y;
            int width, height;
            if (srcAspect > dstAspect)
            {
                width = RoundToEven(srcSize.y * dstAspect);
                height = srcSize.y;
                scale = new Vector2((float)srcSize.x / width, 1);
            }
            else
            {
                width = srcSize.x;
                height = RoundToEven(srcSize.x / dstAspect);
                scale = new Vector2(1, (float)srcSize.y / height);
            }
            dstSize = new Vector2Int(width, height);
        }

        private static int RoundToEven(float n)
        {
            return Mathf.RoundToInt(n / 2) * 2;
        }
    }
}
