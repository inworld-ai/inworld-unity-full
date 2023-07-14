using Inworld.Assets;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld.Sample
{
    [CreateAssetMenu(fileName = "FacialEmotion", menuName = "Inworld/Emotion", order = 0)]
    public class FacialEmotion : ScriptableObject
    {
        public List<FacialAnimation> emotions;
    }
}
