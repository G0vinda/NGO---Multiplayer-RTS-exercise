using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera tankVirtualCamera;
    [field: SerializeField] public Health Health {get; private set;}
    [field: SerializeField] public CoinWallet Wallet {get; private set;}

    public NetworkVariable<FixedString32Bytes> PlayerName = new();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var playerData = HostSingleton.Instance.GameManager.NetworkServer.GetPlayerDataByClientId(OwnerClientId);
            PlayerName.Value = playerData.playerName;
            
            OnPlayerSpawned?.Invoke(this);
        }
        
        if (IsOwner)
        {
            tankVirtualCamera.enabled = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
