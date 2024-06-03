using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilSlick : MonoBehaviour
{
    public float duration = 30f;
    float timer;

    private void Start()
    {
        timer = 0;
    }


    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > duration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CarDurability cd = other.GetComponentInParent<CarDurability>();
        if (cd != null)
        {
            cd.HitSlick();
        }
    }
}
