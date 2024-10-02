using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int damage = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var otherRb = collision.attachedRigidbody;
        if(otherRb != null)
        {
            if (otherRb.TryGetComponent<Health>(out var otherHealth))
                otherHealth.TakeDamage(damage);
        }
    }
}
