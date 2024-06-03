using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RaceController.Instance.FinishLap(other.transform.parent.gameObject);
    }
}
