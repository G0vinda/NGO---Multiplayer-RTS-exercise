using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IDisposable
{
    public NetworkServer NetworkServer { get; private set; }
    
    private const int MaxConnections = 20;
    private const string GameSceneName = "Game";
    
    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyId;
    
    public async void Dispose()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(_lobbyId))
        {
            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            _lobbyId = string.Empty;
        }
        
        NetworkServer?.Dispose();   
    }

    public async Task StartHostAsync()
    {
        try
        {
            _allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }

        try
        {
            _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.Log($"JoinCode: {_joinCode}");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var relayServerData = new RelayServerData(_allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            var lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: _joinCode)
                }
            };
            
            var playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name");
            var lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);

            _lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15f));
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        var playerData = new PlayerData()
        {
            playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            playerAuthId = AuthenticationService.Instance.PlayerId
        };
        var payload = JsonUtility.ToJson(playerData);
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        var heartBeatWait = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId);
            yield return heartBeatWait;
        }
    }
}