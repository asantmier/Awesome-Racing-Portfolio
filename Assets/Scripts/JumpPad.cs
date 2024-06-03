using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float force = 30000;

    private void OnTriggerEnter(Collider other)
    {
        var pcc = other.GetComponentInParent<PlayerCarControl>();
        if (pcc != null)
        {
            pcc.GetComponent<Rigidbody>().AddForce(Vector3.up * force, ForceMode.Impulse);
        }
    }
}
