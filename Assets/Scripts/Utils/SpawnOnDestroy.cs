using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;

    private void OnDestroy()
    {
        Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
    }
}
