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
    NetworkManager m_NetworkManager;

    NetworkManager NetworkManager
    {
        get
        {
            if (!m_NetworkManager)
            {
                m_NetworkManager = NetworkManager.Singleton;
            }
            return m_NetworkManager;
        }
    }
    protected virtual void Awake()
    {
        address.text = m_Transport.ConnectionData.Address;
        port.text = m_Transport.ConnectionData.Port.ToString();
        DontDestroyOnLoad(gameObject);
    }

    public void StartServer()
    {
        if (NetworkManager.StartServer())
        {
            canvas.SetActive(false);
        }
            
    }
    public void StartClient()
    {
        m_Transport.ConnectionData.Address = address.text;
        if (ushort.TryParse(port.text, out ushort data))
            m_Transport.ConnectionData.Port = data;
        if (NetworkManager.StartClient())
            canvas.SetActive(false);
    }
    void Update()
    {
        CheckConnectionStatus();
    }
    void CheckConnectionStatus()
    {
        if (!NetworkManager)
            return;
        if (controller.gameObject.activeSelf)
            return;
        if (NetworkManager.IsServer && !NetworkManager.IsClient)
        {
            if (NetworkManager.ConnectedClientsIds.Count > 0)
                controller.gameObject.SetActive(true);
        }
    }
}
