using Inworld;
using Inworld.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInput : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
            InworldMessenger.DebugSendError();
        if (Input.GetKeyUp(KeyCode.Alpha2))
            InworldMessenger.DebugSendCritical();
        if (Input.GetKeyUp(KeyCode.Alpha3))
            InworldMessenger.DebugSendGoAway();
        if (Input.GetKeyUp(KeyCode.Alpha4))
            InworldMessenger.DebugSendIncompleteInteraction();
    }

}
