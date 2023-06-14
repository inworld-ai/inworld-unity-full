using Inworld;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
    void OnEnable()
    {
        InworldController.Client.OnStatusChanged += status => Debug.Log(status);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
