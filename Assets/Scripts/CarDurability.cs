using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(PrometeoCarController))]
public class CarDurability : MonoBehaviour
{
    public float maxDurability;
    // This setting determines the most damage one collision can deal, preventing one shots
    [Range(0f, 1f)]
    public float maxPercentDamagePerCollision = 0.2f;
    [SerializeField]
    private float durability;

    public Slider durabilitySlider;
    public GameObject deathFX;

    public float oilFwdStiff;
    public float oilSideStiff;
    public float oilDuration;
    public WheelCollider[] wheelColliders;
    private float defaultStiffness;

    private PrometeoCarController pcc;
    private Rigidbody rb;
    private Checkpoint lastCheckpoint;
    private RaceController raceController;

    public float Durability { get => durability; 
        set
        {
            durability = Mathf.Clamp(value, 0, maxDurability);
            if (durabilitySlider != null)
                durabilitySlider.value = durability / maxDurability;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        durability = maxDurability;

        pcc = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();
        raceController = RaceController.Instance;
        
        defaultStiffness = wheelColliders[0].forwardFriction.stiffness;

        deathFX.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Damage(float amount)
    {
        Durability -= amount;

        if (Durability <= 0)
        {
            RaceController rc = FindObjectOfType<RaceController>();
            if (rc == null)
            {
                Debug.LogError("Couldn't find race controller from car durability script!");
            } else
            {
                //rc.Lose();
                StartCoroutine("Respawn");
                return;
            }
        }

    }

    private IEnumerator Respawn()
    {
        Debug.Log("Respawning " + gameObject.name);
        if (gameObject.CompareTag("Player"))
        {
            pcc.enableControl = false;
            deathFX.SetActive(true);
            raceController.fader.FadeOut(1f);
            yield return new WaitForSeconds(1f);
            Durability = maxDurability;
            transform.position = lastCheckpoint.transform.position;
            raceController.fader.FadeIn(1f);
            yield return new WaitForSeconds(1f);
            deathFX.SetActive(false);
            pcc.enableControl = true;
        } else
        {
            CarAI carAI = GetComponent<CarAI>();
            carAI.running = false;
            deathFX.SetActive(true);
            yield return new WaitForSeconds(1f);
            Durability = maxDurability;
            transform.position = lastCheckpoint.transform.position;
            yield return new WaitForSeconds(1f);
            deathFX.SetActive(false);
            carAI.running = true;
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        float damageScale = 1f;
        //Debug.Log(string.Format("Collision! Force: {0}, Vecloity {1}, Scale {2}.", (collision.impulse / Time.fixedDeltaTime).magnitude, collision.relativeVelocity.magnitude, damageScale));
        Damage(Mathf.Clamp(collision.relativeVelocity.magnitude * damageScale, 0, maxPercentDamagePerCollision * maxDurability));
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        lastCheckpoint = checkpoint;
    }

    public Checkpoint GetCheckpoint() 
    { 
        return lastCheckpoint; 
    }

    public void HitSlick()
    {
        // oil slick code
        StopCoroutine("OilDebuff");
        StartCoroutine("OilDebuff");
    }

    IEnumerator OilDebuff()
    {
        foreach (WheelCollider wheel in wheelColliders)
        {
            var tmp = wheel.forwardFriction;
            tmp.stiffness = oilFwdStiff;
            wheel.forwardFriction = tmp;
            tmp = wheel.sidewaysFriction;
            tmp.stiffness = oilSideStiff;
            wheel.sidewaysFriction = tmp;
        }
        yield return new WaitForSeconds(oilDuration);
        foreach (WheelCollider wheel in wheelColliders)
        {
            var tmp = wheel.forwardFriction;
            tmp.stiffness = defaultStiffness;
            wheel.forwardFriction = tmp; 
            tmp = wheel.sidewaysFriction;
            tmp.stiffness = defaultStiffness;
            wheel.sidewaysFriction = tmp;
        }
    }
}
