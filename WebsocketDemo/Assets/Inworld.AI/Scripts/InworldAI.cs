using UnityEngine;

[CreateAssetMenu(fileName = "InworldAI", menuName = "Inworld/AI", order = 0)]
public class InworldAI : ScriptableObject
{
    const string k_GlobalDataPath = "InworldAI";
    static InworldAI __inst;
    
    public static InworldAI Instance
    {
        get
        {
            if (__inst)
                return __inst;
            __inst = Resources.Load<InworldAI>(k_GlobalDataPath);
            return __inst;
        }
    }
}
