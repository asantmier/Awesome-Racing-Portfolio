using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public RectTransform needle;
    public RectTransform minimum;
    public RectTransform middle;
    public RectTransform maximum;
    public TextMeshProUGUI speedText;

    public float maxSpeed;

    private Quaternion minAngle;
    private Quaternion middleAngle;
    private Quaternion maxAngle;

    private float speed = 0;
    private float dSpeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        minAngle = minimum.rotation;
        middleAngle = middle.rotation;
        maxAngle = maximum.rotation;
        SetSpeed(0);
    }

    public void SetSpeed(float newSpeed)
    {
        float temp = Mathf.Clamp(newSpeed, 0, maxSpeed);
        // Smooth out jittery values from PCC
        speed = Mathf.SmoothDamp(speed, temp, ref dSpeed, 0.1f);
        
        float t = speed / maxSpeed;
        if (t < .5)
        {
            needle.rotation = Quaternion.Lerp(minAngle, middleAngle, t / .5f);
        }
        else
        {
            needle.rotation = Quaternion.Lerp(middleAngle, maxAngle, t - .5f);
        }

        speedText.text = Mathf.RoundToInt(speed).ToString();
    }
}
