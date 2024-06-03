using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PrometeoCarController))]
public class PlayerCarControl : MonoBehaviour
{
    // Boost
    public float boostDuration;
    public int speedBoost;
    public int accelerationBoost;
    // Air Control
    public float gyroTorque = 100;

    // Boost Trail
    public TrailRenderer leftTrail;
    public TrailRenderer rightTrail;

    // HUD
    public Speedometer speedometer;

    // Flip
    public float flipForce = 20;

    // Powers
    public Transform rearFirePoint;
    public Transform rearDropPoint;

    public float launchPower;
    public GameObject bomb;
    public GameObject oil;

    public string power1;
    public string power2;
    public TextMeshProUGUI power1Text;
    public TextMeshProUGUI power2Text;
    public GameObject power1Object;
    public GameObject power2Object;

    // Internal
    PrometeoCarController pcc;
    Rigidbody rb;
    WheelCollider wheelCollider;

    private bool boosting = false;
    private bool decelerating = false;

    private bool isFlipped;
    private float groundCheckDistance;
    private Vector3 groundCheckPosition;

    // Start is called before the first frame update
    void Start()
    {
        pcc = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();
        wheelCollider = GetComponentInChildren<WheelCollider>();
        leftTrail.emitting = false;
        rightTrail.emitting = false;

        groundCheckPosition = GetComponentInChildren<BoxCollider>().center;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHUD();

        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            Debug.Log("No keyboard detected!");
            return;
        }

        groundCheckDistance = GetComponentInChildren<BoxCollider>().bounds.extents.y + 0.1f;

        Debug.DrawRay(transform.TransformPoint(groundCheckPosition), Vector3.down * groundCheckDistance, Color.red, 0.1f) ;
        isFlipped = Physics.Raycast(transform.TransformPoint(groundCheckPosition), Vector3.down, groundCheckDistance, ~LayerMask.GetMask("Player"));


        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (power1.Length > 0)
            {
                switch (power1)
                {
                    case "Bomb":
                        GameObject bm = Instantiate(bomb, rearFirePoint.position, rearFirePoint.rotation);
                        bm.GetComponent<Rigidbody>().AddForce(launchPower * rearFirePoint.forward, ForceMode.Impulse);
                        power1 = power2;
                        power2 = "";
                        UpdateHUD();
                        break;
                    case "Boost":
                        if (wheelCollider.isGrounded && !boosting)
                        {
                            StartCoroutine("Boost");
                            power1 = power2;
                            power2 = "";
                            UpdateHUD();
                        }
                        break;
                    case "Oil":
                        RaycastHit hit;
                        if(Physics.Raycast(rearDropPoint.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
                        {
                            GameObject ol = Instantiate(oil, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                            power1 = power2;
                            power2 = "";
                            UpdateHUD();
                        }
                        break;
                }
            }
        }

        // Stop the boost deceleration if we've decelerated enough
        if (decelerating && pcc.carSpeed <= pcc.maxSpeed)
        {
            decelerating = false;
            CancelInvoke("DecelerateOverride");
        }

        // If the player isn't grounded, give them control over their movement, and pull them towards the center
        if (!wheelCollider.isGrounded)
        {
            var torque = gyroTorque;
            if (isFlipped /*&& Vector3.Dot(transform.up, Vector3.down) > 0*/)
            {
                torque *= flipForce;
            }
            Vector3 dir = Vector3.zero;
            if (keyboard.sKey.isPressed)
            {
                dir -= transform.right;
            }
            if (keyboard.wKey.isPressed)
            {
                dir += transform.right;
            }
            if (keyboard.aKey.isPressed)
            {
                dir += transform.forward;
            }
            if (keyboard.dKey.isPressed)
            {
                dir -= transform.forward;
            }
            rb.AddTorque(dir * torque * Time.deltaTime);
        }

        speedometer.SetSpeed(Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z * 3.6f / 1.609f));
    }

    void UpdateHUD()
    {
        if (power1.Length > 0)
        {
            power1Object.SetActive(true);
            power1Text.text = power1;
            if (power2.Length > 0)
            {
                power2Object.SetActive(true);
                power2Text.text = power2;
            } else
            {
                power2Object.SetActive(false);
            }
        } else if (power2.Length > 0)
        {
            // Move power 2 into power 1
            power1 = power2;
            power2 = "";
            power1Object.SetActive(true);
            power1Text.text = power1;
            power2Object.SetActive(false);
        } else
        {
            // Hide both
            power1Object.SetActive(false);
            power2Object.SetActive(false);
        }
    }

    // Returns speed in mph
    public float Speed()
    {
        return Mathf.Abs((transform.InverseTransformDirection(rb.velocity).z * 3.6f) / 1.609f);
    }

    // Manually call decelerate from here to avoid PCC canceling deceleration with its internals
    void DecelerateOverride()
    {
        pcc.DecelerateCar();
    }

    IEnumerator Boost()
    {
        // start
        boosting = true;

        // Enable the trail
        leftTrail.emitting = true;
        rightTrail.emitting = true;
        // Stop the boost deceleration if it's happening
        decelerating = false;
        CancelInvoke("DecelerateOverride");

        int tempSpeed = pcc.maxSpeed;
        int tempAcceleration = pcc.accelerationMultiplier;
        pcc.maxSpeed = pcc.maxSpeed + speedBoost;
        pcc.accelerationMultiplier = pcc.accelerationMultiplier + accelerationBoost;
        float timer = 0.0f;
        while (timer < boostDuration)
        {
            pcc.GoForward();
            yield return null;
            timer += Time.deltaTime;
        }
        pcc.maxSpeed = tempSpeed;
        pcc.accelerationMultiplier = tempAcceleration;

        // Decelerate from our speed that's higher than normal
        if (pcc.carSpeed > pcc.maxSpeed)
        {
            InvokeRepeating("DecelerateOverride", 0f, 0.1f);
            decelerating = true;
        }
        // Disable the trail
        leftTrail.emitting = false;
        rightTrail.emitting = false;

        // end
        boosting = false;
    }

    public bool IsFlipped()
    {
        return isFlipped;
    }

    public void GrantPower(string power)
    {
        if (power1.Length > 0)
        {
            if (power2.Length > 0)
            {
                return;
            } else
            {
                power2 = power;
            }
        } else
        {
            power1 = power;
        }
        UpdateHUD();
    }
}
