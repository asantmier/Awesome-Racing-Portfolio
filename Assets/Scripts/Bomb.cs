using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float countdown = 3f;
    public float radius;
    public LayerMask mask;
    public float damage;
    public GameObject model;
    public GameObject fx;

    bool exploded;

    // Start is called before the first frame update
    void Start()
    {
        model.SetActive(true);
        fx.SetActive(false);
        exploded = false;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0 && !exploded)
        {
            exploded = true;
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, mask.value);
            foreach (Collider hit in hits )
            {
                CarDurability cd = hit.GetComponentInParent<CarDurability>();
                cd.Damage(damage);
            }
            model.SetActive(false);
            fx.SetActive(true);
            Destroy(gameObject, 2f);
        }
    }
}
