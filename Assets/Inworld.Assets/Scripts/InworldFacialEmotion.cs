using Inworld.Assets;
using System.Collections.Generic;
using UnityEngine;
namespace Inworld.Sample
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class InworldFacialEmotion : ScriptableObject
    {
        public List<FacialAnimation> emotions;
    }
}
