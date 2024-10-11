using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Serialization;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyPlayersText;

    private LobbiesList _lobbiesList;
    private Lobby _lobby;

    public void Initialize(LobbiesList lobbiesList, Lobby lobby)
    {
        _lobbiesList = lobbiesList;
        _lobby = lobby;
        
        lobbyNameText.text = lobby.Name;
        lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void Join()
    {
        _lobbiesList.JoinLobbyAsync(_lobby);
    }
}
