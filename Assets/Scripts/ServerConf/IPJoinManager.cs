using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

/// <summary>
/// Simple UI helper to join a Netcode game by IP (or start host/server).
/// Supports both legacy InputField and TextMeshPro InputField. Hook buttons to the public methods.
/// </summary>
public class IPJoinManager : MonoBehaviour
{
    [Header("UI References (optional)")]
    public TMP_InputField ipInputTMP;
    public InputField ipInputLegacy;
    public Button connectButton;
    public Button hostButton;
   

    [Header("Connection")]
    public string defaultIp = "127.0.0.1";
    public int port = 7788;

    private void Awake()
    {
        // Wire button callbacks if assigned
        if (connectButton != null) connectButton.onClick.AddListener(OnConnectButton);
        if (hostButton != null) hostButton.onClick.AddListener(OnHostButton);
    }

    private void OnDestroy()
    {
        if (connectButton != null) connectButton.onClick.RemoveListener(OnConnectButton);
        if (hostButton != null) hostButton.onClick.RemoveListener(OnHostButton);
    }

    private void OnConnectButton()
    {
        string ip = GetIPFromInput();
        ConnectTo(ip);
    }

    private void OnHostButton()
    {
        StartHost();
    }

    public string GetIPFromInput()
    {
        if (ipInputTMP != null && !string.IsNullOrWhiteSpace(ipInputTMP.text))
            return ipInputTMP.text.Trim();

        if (ipInputLegacy != null && !string.IsNullOrWhiteSpace(ipInputLegacy.text))
            return ipInputLegacy.text.Trim();

        return defaultIp;
    }

    /// <summary>
    /// Configures UnityTransport address/port and starts the client.
    /// </summary>
    public void ConnectTo(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) ipAddress = defaultIp;

        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Make sure a NetworkManager exists in the scene.");
            return;
        }

        // Try to find UnityTransport on the NetworkManager. If not found, try to find any in scene.
        UnityTransport transport = nm.GetComponent<UnityTransport>();
        if (transport == null) transport = FindObjectOfType<UnityTransport>();
        if (transport == null)
        {
            Debug.LogError("UnityTransport not found. Install and add Unity Transport to the scene.");
            return;
        }

        transport.ConnectionData.Address = ipAddress;
        transport.ConnectionData.Port = (ushort)port;

        Debug.Log($"Connecting to {ipAddress}:{port}...");
        nm.StartClient();
    }

    public void StartHost()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Make sure a NetworkManager exists in the scene.");
            return;
        }
        nm.StartHost();
    }
    

    // Optional helper called by TMP InputField OnSubmit / legacy InputField OnEndEdit
    public void OnIpSubmitted(string ip)
    {
        if (!string.IsNullOrWhiteSpace(ip)) ConnectTo(ip.Trim());
    }

    // Allows programmatic setting of the IP input
    public void SetIp(string ip)
    {
        if (ipInputTMP != null) ipInputTMP.text = ip;
        if (ipInputLegacy != null) ipInputLegacy.text = ip;
        defaultIp = ip;
    }
}
