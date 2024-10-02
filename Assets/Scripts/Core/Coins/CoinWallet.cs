using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new();

    public void SpendCoins(int amount)
    {
        if (amount > TotalCoins.Value || !IsServer)
            return;

        TotalCoins.Value -= amount;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<Coin>(out var coin))
        {
            var coinValue = coin.Collect();

            if (IsServer)
                TotalCoins.Value += coinValue;
        }
    }
}
