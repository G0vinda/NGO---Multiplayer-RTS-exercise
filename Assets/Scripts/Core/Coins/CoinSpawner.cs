using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins = 50;
    [SerializeField] private int coinValue = 10;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;

    private float _coinRadius;
    private Collider2D[] _coinBuffer = new Collider2D[1];

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        _coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        for(int i = 0; i < maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        var newCoin = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

        newCoin.SetValue(coinValue);
        newCoin.GetComponent<NetworkObject>().Spawn();

        newCoin.OnCollected += HandleCoinCollected;
    }

    private Vector2 GetSpawnPoint()
    {
        float x = 0;
        float y = 0;

        while(true)
        {
            x = UnityEngine.Random.Range(xSpawnRange.x, xSpawnRange.y);
            y = UnityEngine.Random.Range(ySpawnRange.x, ySpawnRange.y);
            var spawnPoint = new Vector2(x, y);
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, _coinRadius, _coinBuffer, layerMask);
            if(numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Respawn();
    }
}
