/*************************************************************************************************
 * Copyright 2022-2025 Theai, Inc. dba Inworld AI
 *
 * Use of this source code is governed by the Inworld.ai Software Development Kit License Agreement
 * that can be found in the LICENSE.md file or at https://www.inworld.ai/sdk-license
 *************************************************************************************************/

using System.Linq;
using UnityEngine;

namespace Inworld
{
    public static class InworldAssetProcessor
    {
        const float k_LuminanceRed = 0.2126f;
        const float k_LuminanceGreen = 0.7152f;
        const float k_LuminanceBlue = 0.0722f;
        
        /// <summary>
        /// Get the picture's luminance. Used to display the opposite color of the text.
        /// </summary>
        /// <param name="color">the color of a pixel.</param>
        public static float GetLuminance(Color color) 
            => k_LuminanceRed * color.r + k_LuminanceGreen * color.g + k_LuminanceBlue * color.b;
        
        public static Color GetFontColor(Texture2D texture)
            => texture == InworldAI.DefaultThumbnail || texture.GetPixels().Average(GetLuminance) < 0.5 ? Color.white : Color.black;

    }
}