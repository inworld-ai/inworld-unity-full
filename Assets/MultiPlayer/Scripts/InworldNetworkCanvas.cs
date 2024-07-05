using Inworld;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class InworldNetworkCanvas : NetworkBehaviour
{
    public UnityTransport m_Transport;
    public InworldController controller;
    public GameObject canvas;
    public TMP_InputField address;
    public TMP_InputField port;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsServer)
        {
            controller.gameObject.SetActive(true);
        }
    }
}
