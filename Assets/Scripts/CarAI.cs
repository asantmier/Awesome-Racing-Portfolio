using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PrometeoCarController))]
public class CarAI : MonoBehaviour
{
    public string racerName;
    public bool running = true;
    public float turnApproximation = 2f;
    public float speedApproximation = 10f;
    public AnimationCurve steerSpeedCurve = AnimationCurve.Constant(0, 1, 1f);
    public AIGoalHelper goalList;
    public AIGoal goal;
    public LineRenderer lineRenderer;
    public Transform forwardRaycaster;
    public Transform leftRaycaster;
    public Transform rightRaycaster;
    public float avoidDistance = 5f;
    public float lookAheadDistance = 3f;
    public float emergencyDistance = 1f;
    public ParticleSystem smoke;
    public TrailRenderer skid;

    // Powers
    public float powerTimer;
    private float _ptimer;
    // Between 0 and 1
    public float powerChance;

    public Transform rearFirePoint;
    public Transform rearDropPoint;

    public float launchPower;
    public GameObject bomb;
    public GameObject oil;

    public string power1;
    public string power2;

    // Boosting
    public float boostDuration;
    public int speedBoost;
    public int accelerationBoost;

    // Boost Trail
    public TrailRenderer leftTrail;
    public TrailRenderer rightTrail;

    Rigidbody rb;

    NavMeshAgent agent;
    PrometeoCarController pcc;
    NavMeshPath path;

    bool decelerating = false;
    bool boosting = false;

    private float speed = 0;
    private float dSpeed = 0;

    private int goalID;

    public int GoalID { get => goalID; set => goalID = value; }

    Vector3[] previousPositions = new Vector3[5];
    int cycleCounter = 0;
    float errorTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pcc = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(transform.position, out hit, 3f, agent.areaMask);
            agent.transform.position = hit.position;
            agent.enabled = false;
            agent.enabled = true;
        }

        path = new NavMeshPath();
        goalID = 0;
        goal = goalList.transform.GetChild(goalID).GetComponent<AIGoal>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!running)
        {
            errorTimer = 0;
            cycleCounter = 0;
            return;
        }
        // Forcibly respawns the car if it hasn't moved in 5 seconds because some bugs are really hard to fix
        errorTimer += Time.deltaTime;
        if (errorTimer > 1f)
        {
            errorTimer = 0;
            previousPositions[cycleCounter++ % previousPositions.Length] = transform.position;
            if (cycleCounter >= previousPositions.Length)
            {
                float sum = 0;
                for (int i = cycleCounter; i < cycleCounter + previousPositions.Length - 1; i++)
                {
                    sum += Vector3.Distance(previousPositions[i % previousPositions.Length], previousPositions[(i + 1) % previousPositions.Length]);
                }
                if (sum < 2f)
                {
                    GetComponent<CarDurability>().Damage(10000);
                }
            }
        }
        


        // Smooth out our speed using the same technique as Speedometer.cs
        speed = Mathf.SmoothDamp(speed, pcc.carSpeed, ref dSpeed, 0.1f);

        if (goal != null) {
            // Look ahead and see if we're gonna hit a wall
            bool gonnaCrash = false;
            Debug.DrawRay(forwardRaycaster.position, forwardRaycaster.forward * lookAheadDistance, Color.blue, Time.fixedDeltaTime);
            if (Physics.Raycast(forwardRaycaster.position, forwardRaycaster.forward, lookAheadDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore)) {
                gonnaCrash = true;
            }
            bool leftCrash = false;
            bool rightCrash = false;
            RaycastHit hit;
            Debug.DrawRay(leftRaycaster.position, leftRaycaster.forward * avoidDistance, Color.red, Time.fixedDeltaTime);
            if (Physics.Raycast(leftRaycaster.position, leftRaycaster.forward, out hit, avoidDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                leftCrash = true;
                if (hit.distance <= emergencyDistance)
                {
                    gonnaCrash = true;
                }
            }
            Debug.DrawRay(rightRaycaster.position, rightRaycaster.forward * avoidDistance, Color.green, Time.fixedDeltaTime);
            if (Physics.Raycast(rightRaycaster.position, rightRaycaster.forward, out hit, avoidDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                rightCrash = true;
                if (hit.distance <= emergencyDistance)
                {
                    gonnaCrash = true;
                }
            }
            // If we're crashing on both sides, forget about trying to steer away andd just brake
            if (rightCrash && leftCrash)
            {
                gonnaCrash = true;
            }
            // Prioritize braking over avoidance steering
            if (gonnaCrash)
            {
                rightCrash = false;
                leftCrash = false;
            }

            agent.CalculatePath(goal.transform.position, path);
            lineRenderer.SetPositions(path.corners);

            // Steering

            // TODO We could probably scale steering speed over distance
            // Adjust steering angle
            float turnAngle = 0;
            if (path.corners.Length > 1)
            {
                Vector3 nextLocation = path.corners[1];
                turnAngle = Vector3.SignedAngle(transform.forward, nextLocation - transform.position, Vector3.up);
            }
            //Debug.Log("Turn angle: " + turnAngle);
            bool reversing = transform.InverseTransformDirection(rb.velocity).z < 1f;

            if (leftCrash || rightCrash) // steer to not crash
            {
                if (leftCrash)
                {
                    pcc.TurnRight();
                }
                else
                {
                    pcc.TurnLeft();
                }
            }
            else // steer normally
            {
                // Use the tire's steering angle for precise movement
                if (turnAngle / 2 > pcc.frontLeftCollider.steerAngle)
                {
                    // Reverse direction if going reverse
                    if (!reversing)
                    {
                        pcc.TurnRight();
                    }
                    else
                    {
                        pcc.TurnLeft();
                    }
                }
                else if (turnAngle / 2 < pcc.frontLeftCollider.steerAngle)
                {
                    if (!reversing)
                    {
                        pcc.TurnLeft();
                    }
                    else
                    {
                        pcc.TurnRight();
                    }
                }
                else
                {
                    pcc.ResetSteeringAngle();
                }
            }

            // Adjust car speed
            float localVelocityX = transform.InverseTransformDirection(rb.velocity).x;
            bool losingControl = Mathf.Abs(localVelocityX) > 2.5f || (Mathf.Abs(localVelocityX) > 5f && Mathf.Abs(pcc.carSpeed) > 12f);
            //Debug.Log("Car Speed: " + speed + " Target Speed: " + goal.targetSpeed);
            float desiredSpeedMultiplier = steerSpeedCurve.Evaluate(pcc.frontLeftCollider.steerAngle / pcc.maxSteeringAngle);
            if (speed < goal.targetSpeed * desiredSpeedMultiplier && !gonnaCrash && !losingControl)
            {
                CancelInvoke("DecelerateOverride");
                decelerating = false;
                pcc.GoForward();
            } else if (speed > goal.targetSpeed * desiredSpeedMultiplier + speedApproximation || gonnaCrash || (Mathf.Abs(localVelocityX) > 5f && Mathf.Abs(pcc.carSpeed) > 12f))
            {
                CancelInvoke("DecelerateOverride");
                decelerating = false;
                pcc.GoReverse();
            } else if (!decelerating)
            {
                InvokeRepeating("DecelerateOverride", 0f, 0.1f);
                decelerating = true;
            }
            
        } 
        else
        {
            // Return to resting position
            pcc.ResetSteeringAngle();
            if (!decelerating)
            {
                InvokeRepeating("DecelerateOverride", 0f, 0.1f);
                decelerating = true;
            }
        }

        bool activatePower = false;
        _ptimer += Time.deltaTime;
        if (_ptimer > powerTimer)
        {
            _ptimer = 0;
            activatePower = Random.value < powerChance;
        }
        if (activatePower)
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
                        break;
                    case "Boost":
                        if (!boosting)
                        {
                            StartCoroutine("Boost");
                            power1 = power2;
                            power2 = "";
                        }
                        break;
                    case "Oil":
                        RaycastHit hit;
                        if (Physics.Raycast(rearDropPoint.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Environment")))
                        {
                            GameObject ol = Instantiate(oil, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                            power1 = power2;
                            power2 = "";
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

    }

    // Grabbed from PlayerCarControl.cs
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
        //leftTrail.emitting = true;
        //rightTrail.emitting = true;
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
            //pcc.GoForward();
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
        //leftTrail.emitting = false;
        //rightTrail.emitting = false;

        // end
        boosting = false;
    }

    public void ReachedGoal(AIGoal next)
    {
        goal = next;
    }

    public void GrantPower(string power)
    {
        if (power1.Length > 0)
        {
            if (power2.Length > 0)
            {
                return;
            }
            else
            {
                power2 = power;
            }
        }
        else
        {
            power1 = power;
        }
    }
}
