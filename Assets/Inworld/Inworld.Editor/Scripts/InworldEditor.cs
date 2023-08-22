using UnityEngine;
namespace Inworld.AI.Editor
{

    public class InworldEditor : ScriptableObject
    {
        [SerializeField] Texture2D m_Banner;
        
        [SerializeField] Readme m_Readme;
        [SerializeField] bool m_DisplayReameOnLoad;
        const string k_GlobalDataPath = "InworldEditor";
        static InworldEditor __inst;

        public static InworldEditor Instance
        {
            get
            {
                if (__inst)
                    return __inst;
                __inst = Resources.Load<InworldEditor>(k_GlobalDataPath);
                return __inst;
            }
        }
        public static Texture2D Banner => Instance.m_Banner;
        public static bool LoadedReadme
        {
            get => Instance.m_DisplayReameOnLoad;
            set => Instance.m_DisplayReameOnLoad = value;
        }
        public static Readme ReadMe => Instance.m_Readme;
    }
}
