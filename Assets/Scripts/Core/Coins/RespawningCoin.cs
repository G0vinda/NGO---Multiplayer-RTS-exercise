using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;

    public override int Collect()
    {
        if(!IsServer)
        {
            Show(false);
            return 0;
        }


        if (alreadyCollected)
            return 0;

        alreadyCollected = true;
        OnCollected?.Invoke(this);
        return coinValue;
    }

    public void Respawn()
    {
        alreadyCollected = false;
        Show(true);

        HandleRespawnClientRpc();
    }

    [ClientRpc]
    private void HandleRespawnClientRpc()
    {
        Show(true);
    }
}
