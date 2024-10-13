using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [Range(0, 1)]
    [SerializeField] private float coinKeepingPercentage;

    public override void OnNetworkSpawn()
    {
        if(!IsServer)
            return;
        
        var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsServer)
            return;
        
        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (_) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie += (_) => HandlePlayerDie(player);
    }
    
    private void HandlePlayerDie(TankPlayer player)
    {
        var keptCoinValue = player.Wallet.TotalCoins.Value * coinKeepingPercentage;
        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayerNextFrame(player.OwnerClientId, Mathf.RoundToInt(keptCoinValue)));
    }

    private IEnumerator RespawnPlayerNextFrame(ulong ownerClientId, int keptCoinCount)
    {
        yield return null;

        var playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        
        playerInstance.Wallet.TotalCoins.Value = keptCoinCount;
    }
}
