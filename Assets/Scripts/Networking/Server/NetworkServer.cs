using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager _networkManager;
    private Dictionary<ulong, string> _authIdsByClientIds = new();
    private Dictionary<string, PlayerData> _playersByAuthIds = new();
    
    public NetworkServer(NetworkManager networkManager)
    {
        _networkManager = networkManager;

        _networkManager.ConnectionApprovalCallback += ApprovalCheck;
        _networkManager.OnServerStarted += OnNetworkReady;
    }
    
    public void Dispose()
    {
        if (_networkManager != null)
        {
            _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            _networkManager.OnServerStarted -= OnNetworkReady;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            
            if(_networkManager.IsListening)
                _networkManager.Shutdown();
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        var playerData = JsonUtility.FromJson<PlayerData>(payload);
        
        _authIdsByClientIds[request.ClientNetworkId] = playerData.playerAuthId;
        _playersByAuthIds[playerData.playerAuthId] = playerData;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }
    
    private void OnNetworkReady()
    {
        _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (_authIdsByClientIds.TryGetValue(clientId, out var authId))
        {
            _authIdsByClientIds.Remove(clientId);
            _playersByAuthIds.Remove(authId);
        }
    }
}
