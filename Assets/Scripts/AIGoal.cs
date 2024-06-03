using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGoal : MonoBehaviour
{
    public float targetSpeed = 80;
    public int id;

    private void OnTriggerEnter(Collider other)
    {
        CarAI cai = other.GetComponentInParent<CarAI>();
        if (cai != null)
        {
            if (cai.GoalID <= id)
            {
                cai.GoalID = (cai.GoalID + 1) % transform.parent.childCount;
                cai.ReachedGoal(transform.parent.GetChild(cai.GoalID).GetComponent<AIGoal>());
            }
            
        }
    }
}
