using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeAdjustment : MonoBehaviour
{
    [Range(0f, 1f)]
    public float percent;

    // Start is called before the first frame update
    void Start()
    {
        AudioListener.volume = percent;
    }

    // Update is called once per frame
    void Update()
    {
        AudioListener.volume = percent;
    }
}
