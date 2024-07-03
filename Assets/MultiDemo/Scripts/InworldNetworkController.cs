using Inworld;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
public class InworldNetworkController : MonoBehaviour
{
    public UnityTransport m_Transport;
    public InworldController controller;
    public GameObject canvas;
    public TMP_InputField address;
    public TMP_InputField port;

    protected virtual void Awake()
    {
        m_Transport = GetComponent<UnityTransport>();
        address.text = m_Transport.ConnectionData.Address;
        port.text = m_Transport.ConnectionData.Port.ToString();
        DontDestroyOnLoad(gameObject);
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton.StartServer())
            canvas.SetActive(false);
    }
    public void StartClient()
    {
        m_Transport.ConnectionData.Address = address.text;
        if (ushort.TryParse(port.text, out ushort data))
            m_Transport.ConnectionData.Port = data;
        if (NetworkManager.Singleton.StartClient())
            canvas.SetActive(false);
    }
}
