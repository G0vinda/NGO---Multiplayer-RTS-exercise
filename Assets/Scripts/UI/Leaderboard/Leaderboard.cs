using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;
    
    private NetworkList<LeaderboardEntityState> _leaderboardEntities;
    private List<LeaderboardEntityDisplay> _leaderboardEntityDisplays = new();
    
    private void Awake()
    {
        _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _leaderboardEntities.OnListChanged += HandleLeaderBoardEntitiesChanged;
            foreach (var entity in _leaderboardEntities)
            {
                HandleLeaderBoardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }
        
        if(IsServer)
        {
            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                HandlePlayerSpawn(player);
            }

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawn;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawn;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            _leaderboardEntities.OnListChanged -= HandleLeaderBoardEntitiesChanged;
        }
        
        if(IsServer)
        {
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawn;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawn;   
        }
    }
    
    private void HandleLeaderBoardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (_leaderboardEntityDisplays.All(entity => entity.ClientId != changeEvent.Value.ClientId))
                {
                    var newEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    newEntity.Initialize(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);
                    _leaderboardEntityDisplays.Add(newEntity);
                }    
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Insert:
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                var entityToRemove = _leaderboardEntityDisplays.FirstOrDefault(entity => entity.ClientId == changeEvent.Value.ClientId);
                if (entityToRemove != null)
                {
                    entityToRemove.transform.SetParent(null);
                    Destroy(entityToRemove.gameObject);
                    _leaderboardEntityDisplays.Remove(entityToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.RemoveAt:
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                var entityToUpdate = _leaderboardEntityDisplays.FirstOrDefault(entity => entity.ClientId == changeEvent.Value.ClientId);
                if (entityToUpdate != null)
                {
                    entityToUpdate.UpdatePlayerCoins(changeEvent.Value.Coins);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Clear:
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Full:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _leaderboardEntityDisplays.Sort((e1, e2) => e2.PlayerCoins.CompareTo(e1.PlayerCoins));
        for (var i = 0; i < _leaderboardEntityDisplays.Count; i++)
        {
            _leaderboardEntityDisplays[i].transform.SetSiblingIndex(i);
            _leaderboardEntityDisplays[i].UpdateText();
            
            var shouldShow = i < entitiesToDisplay;
            _leaderboardEntityDisplays[i].gameObject.SetActive(shouldShow);
        }

        var playerDisplay = _leaderboardEntityDisplays.FirstOrDefault(entity => entity.ClientId == NetworkManager.Singleton.LocalClientId);
        if (playerDisplay != null)
        {
            if (playerDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                playerDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void HandlePlayerSpawn(TankPlayer player)
    {
        _leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });

        player.Wallet.TotalCoins.OnValueChanged += (_, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
    }
    
    private void HandlePlayerDespawn(TankPlayer player)
    {
        if(_leaderboardEntities == null)
            return;
        
        foreach (var entity in _leaderboardEntities)
        {
            if (entity.ClientId == player.OwnerClientId)
            {
                _leaderboardEntities.Remove(entity);
                break;
            }    
        }
        
        player.Wallet.TotalCoins.OnValueChanged -= (_, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    private void HandleCoinsChanged(ulong clientId, int newCoins)
    {
        for (var i = 0; i < _leaderboardEntities.Count; i++)
        {
            if (_leaderboardEntities[i].ClientId == clientId)
            {
                _leaderboardEntities[i] = new LeaderboardEntityState
                {
                    ClientId = _leaderboardEntities[i].ClientId,
                    PlayerName = _leaderboardEntities[i].PlayerName,
                    Coins = newCoins
                };
                
                return;
            }
        }
    }
}
