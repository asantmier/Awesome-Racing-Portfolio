using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeforeFinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RaceController.Instance.AboutToFinishLap(other.transform.parent.gameObject);
    }
}
