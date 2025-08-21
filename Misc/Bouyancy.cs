using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouyancy : MonoBehaviour
{
    public float force = 10f;

    protected Rigidbody m_rigidbody;

    protected virtual void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.VolumeWater))
        {
            if (transform.position.y < other.bounds.max.y)
            {
                var multiplier = Mathf.Clamp01((other.bounds.max.y - transform.position.y));
                var buoyancy = Vector3.up * force * multiplier;
                m_rigidbody.AddForce(buoyancy);
            }
        }
    }
}
