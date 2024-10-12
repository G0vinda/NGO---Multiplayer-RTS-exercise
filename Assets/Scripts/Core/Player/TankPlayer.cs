using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [SerializeField] CinemachineVirtualCamera tankVirtualCamera;

    public NetworkVariable<FixedString32Bytes> PlayerName = new(); 
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var playerData = HostSingleton.Instance.GameManager.NetworkServer.GetPlayerDataByClientId(OwnerClientId);
            PlayerName.Value = playerData.playerName;
        }
        
        if (IsOwner)
        {
            tankVirtualCamera.enabled = true;
        }
    }
}
