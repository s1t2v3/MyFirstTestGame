using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Pickable : MonoBehaviour, IEntityContact
{
    [Header("General Settings")]
    public Vector3 offset;
    public float releaseOffset = 0.5f;

    [Header("Respawn Settings")]
    public bool autoRespawn;
    public bool respawnOnHitHazards;
    public float respawnHeightLimit = -100;

    [Header("Attack Settings")]
    public bool attackEnemies = true;
    public int damage = 1;
    public float minDamageSpeed = 5f;

    public UnityEvent onPicked;
    public UnityEvent onReleased;
    public UnityEvent onRespawn;

    protected Collider m_collider;
    protected Rigidbody m_rigidBody;

    protected Vector3 m_initialPosition;
    protected Quaternion m_initialRotation;
    protected Transform m_initialParent;

    protected RigidbodyInterpolation m_interpolation;

    public MeshRenderer mesh;
    public ParticleSystem respawnParticle;
    public float particlePlayDuration = 1f;

    public bool beingHold { get; protected set; }

    protected virtual IEnumerator HazardRespawnRoutine()
    {
        if(mesh != null)
        {
            mesh.enabled = false;
            if(respawnParticle != null)
            {
                respawnParticle.Play();
            }
            yield return new WaitForSeconds(particlePlayDuration);
        }
        Respawn();
        if(mesh != null)
        {
            mesh.enabled = true;
        }
    }

    public virtual void Respawn()
    {
        m_rigidBody.velocity = Vector3.zero;
        transform.parent = m_initialParent;
        transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
        m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;
        if(respawnParticle != null)
        {
            respawnParticle.Play();
        }
    }

    public void OnEntityContact(Entity entity)
    {
        if (attackEnemies && entity is Enemy &&
            m_rigidBody.velocity.magnitude > minDamageSpeed)
        {
            entity.ApplyDamage(damage, transform.position);
        }
    }

    protected virtual void EvaluateHazardRespawn(Collider other)
    {
        if (autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
        {
            StartCoroutine(HazardRespawnRoutine());
        }
    }

    public virtual void PickUp(Transform slot)
    {
        if (!beingHold)
        {
            beingHold = true;
            transform.parent = slot;
            transform.localPosition = Vector3.zero + offset;
            transform.localRotation = Quaternion.identity;
            m_rigidBody.isKinematic = true;
            m_collider.isTrigger = true;
            m_interpolation = m_rigidBody.interpolation;
            m_rigidBody.interpolation = RigidbodyInterpolation.None;
            onPicked?.Invoke();
        }
    }

    public virtual void Release(Vector3 direction, float force)
    {
        if (beingHold)
        {
            transform.parent = null;
            transform.position += direction * releaseOffset;
            m_collider.isTrigger = m_rigidBody.isKinematic = beingHold = false;
            m_rigidBody.interpolation = m_interpolation;
            m_rigidBody.velocity = direction * force;
            onReleased?.Invoke();
        }
    }

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_initialPosition = transform.localPosition;
        m_initialRotation = transform.localRotation;
        m_initialParent = transform.parent;
    }

    protected virtual void OnTriggerEnter(Collider other) =>
        EvaluateHazardRespawn(other);

    protected virtual void OnCollisionEnter(Collision collision) =>
        EvaluateHazardRespawn(collision.collider);
}
