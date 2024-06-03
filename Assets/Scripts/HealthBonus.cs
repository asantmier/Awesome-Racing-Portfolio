using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBonus : MonoBehaviour
{
    public float durabilityHeal = 30;

    private void OnTriggerEnter(Collider other)
    {
        CarDurability cd = other.GetComponentInParent<CarDurability>();
        if (cd != null)
        {
            cd.Durability += durabilityHeal;
            Destroy(gameObject);
        }
    }
}
