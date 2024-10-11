using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    private bool _isJoining;
    private bool _isRefreshing;

    private void OnEnable()
    {
        RefreshListAsync();
    }

    public async void RefreshListAsync()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;

        var options = new QueryLobbiesOptions();
        options.Count = 25;

        options.Filters = new List<QueryFilter>()
        {
            new(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op: QueryFilter.OpOptions.GT,
                value: "0"),
            new(
                field: QueryFilter.FieldOptions.IsLocked,
                op: QueryFilter.OpOptions.EQ,
                value: "0")
        };

        try
        {
            var lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var lobby in lobbies.Results)
            {
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Initialize(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Console.WriteLine(e);
            throw;
        }

        _isRefreshing = false;
    }

    public async void JoinLobbyAsync(Lobby lobby)
    {
        if (_isJoining)
            return;

        _isJoining = true;

        try
        {
            var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            var joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }

        _isJoining = false;
    }
}