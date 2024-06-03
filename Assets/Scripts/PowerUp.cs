using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float downtime = 5f;
    float timer;
    bool disabled;
    Collider coll;
    public MeshRenderer mr;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        disabled = false;
        coll = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (disabled)
        {
            timer += Time.deltaTime;
            if (timer > downtime)
            {
                mr.enabled = true;
                coll.enabled = true;
                disabled = false;
                timer = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        string power = "";
        int choice = Random.Range(0, 3);
        switch (choice)
        {
            case 0:
                power = "Bomb";
                break;
            case 1:
                power = "Boost";
                break;
            case 2:
                power = "Oil";
                break;
        }
        PlayerCarControl playerCar = other.GetComponentInParent<PlayerCarControl>();
        if (playerCar != null )
        {
            playerCar.GrantPower(power);
        } 
        else
        {
            // Do check for AI
            CarAI carAI = other.GetComponentInParent<CarAI>();
            if (carAI != null)
            {
                carAI.GrantPower(power);
            }
            else
            {
                Debug.Log("Non racer somehow hit a power up!?");
            }
        }
        audioSource.Play();
        mr.enabled = false;
        coll.enabled = false;
        disabled = true;
        timer = 0;
    }
}
