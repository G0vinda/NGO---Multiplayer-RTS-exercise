using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealth { get; private set; }

    public NetworkVariable<int> CurrentHealth = new();

    public event Action<Health> OnDie;

    private bool _isDead;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        CurrentHealth.Value = MaxHealth;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if(_isDead)
            return;

        CurrentHealth.Value = Math.Clamp(CurrentHealth.Value + value, 0, MaxHealth);
        if(CurrentHealth.Value == 0)
        {
            _isDead = true;
            OnDie?.Invoke(this);
        }
    }
}
