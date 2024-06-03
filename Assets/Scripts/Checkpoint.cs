using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CarDurability cd = other.GetComponentInParent<CarDurability>();
        if (cd != null)
        {
            cd.SetCheckpoint(this);
        }
    }
}
