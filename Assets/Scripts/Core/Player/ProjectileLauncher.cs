using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private CoinWallet coinWallet;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool _shouldFire;
    private float _fireTimer;
    private float _muzzleFlashTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner)
            return;

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    void Update()
    {
        if (_muzzleFlashTimer > 0f)
        {
            _muzzleFlashTimer -= Time.deltaTime;
            if(_muzzleFlashTimer < 0f)
                muzzleFlash.SetActive(false);
        }

        if (!IsOwner)
            return;

        _fireTimer -= Time.deltaTime;

        if (_shouldFire && _fireTimer <= 0)
        {
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        if (coinWallet.TotalCoins.Value < costToFire)
            return;
           
        _fireTimer = 1 / fireRate;

        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 position, Vector3 direction)
    {
        if (coinWallet.TotalCoins.Value < costToFire)
            return;

        coinWallet.SpendCoins(costToFire);

        var projectile = Instantiate(serverProjectilePrefab, position, Quaternion.identity);
        projectile.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

        var projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = projectileRb.transform.up * projectileSpeed;

        PrimaryFireClientRpc(position, direction);
    }

    [ClientRpc]
    private void PrimaryFireClientRpc(Vector3 position, Vector3 direction)
    {
        if (IsOwner)
            return;

        SpawnDummyProjectile(position, direction);
    }

    private void SpawnDummyProjectile(Vector3 position, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        _muzzleFlashTimer = muzzleFlashDuration;

        var projectile = Instantiate(clientProjectilePrefab, position, Quaternion.identity);
        projectile.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectile.GetComponent<Collider2D>());

        var projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = projectileRb.transform.up * projectileSpeed;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        _shouldFire = shouldFire;
    }
}
