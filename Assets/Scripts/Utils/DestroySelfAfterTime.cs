using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfAfterTime : MonoBehaviour
{
    [SerializeField] float lifeTime;

    void Start()
    {
        StartCoroutine(DestroyTimer());
    }

    private IEnumerator DestroyTimer()
    {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }
}
