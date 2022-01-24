using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Duality;
using Unity.Services.Core;
using static Duality.RelayManager;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{

    string joinCode = "";
    string hostJoinCode = "";

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        joinCode = GUILayout.TextField(joinCode);
        if (GUILayout.Button("Client")) HandleClientClick();
        if (GUILayout.Button("Host")) HandleHostClick();
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);

        if (hostJoinCode.Length > 0)
        {
            GUILayout.Label("Join Code: " + hostJoinCode);
        }

    }

    async void HandleHostClick()
    {
        RelayHostData hostData = await RelayManager.HostGame(4);
        hostJoinCode = hostData.JoinCode;
        //Retrieve the Unity transport used by the NetworkManager
        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            hostData.IPv4Address,
            hostData.Port,
            hostData.AllocationIDBytes,
            hostData.Key,
            hostData.ConnectionData
        );
        Debug.Log("Host Data:" + hostData);
        NetworkManager.Singleton.StartHost();
    }

    async void HandleClientClick()
    {
        RelayJoinData joinData = await RelayManager.JoinGame(joinCode);
        UnityTransport transport = NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            joinData.IPv4Address,
            joinData.Port,
            joinData.AllocationIDBytes,
            joinData.Key,
            joinData.ConnectionData,
            joinData.HostConnectionData
        );
        NetworkManager.Singleton.StartClient();
    }
}







