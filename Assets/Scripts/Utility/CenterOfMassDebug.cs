using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteInEditMode]
public class CenterOfMassDebug : MonoBehaviour
{
    public Vector3 positionCM;
    public bool modifyCM;

    Rigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        body = GetComponent<Rigidbody>();
        UpdateCM();
    }

    public void UpdateCM()
    {
        if (modifyCM)
        {
            body.ResetCenterOfMass();
            body.centerOfMass += positionCM;
        }
    }

    private void OnDrawGizmos()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(body.worldCenterOfMass, 0.1f);
    }
}
