using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        var aimInputPosition = inputReader.AimPosition;
        var aimWorldPosition = Camera.main.ScreenToWorldPoint(aimInputPosition);

        var aimDistance = aimWorldPosition - turretTransform.position;
        var aimDirection = new Vector3(aimDistance.x, aimDistance.y, 0).normalized;
        turretTransform.up = aimDirection;
    }
}
