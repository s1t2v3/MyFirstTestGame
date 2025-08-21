using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(AudioSource))]
public class Breakable : MonoBehaviour
{
    public GameObject display;
    public AudioClip clip;
    public bool hazardCanBreak;

    public UnityEvent OnBreak;

    protected Collider m_collider;
    protected AudioSource m_audio;
    protected Rigidbody m_rigidBody;

    public bool broken { get; protected set; }

    public virtual void Break()
    {
        if (!broken)
        {
            if (m_rigidBody)
            {
                m_rigidBody.isKinematic = true;
            }

            broken = true;
            display.SetActive(false);
            m_collider.enabled = false;
            m_audio.PlayOneShot(clip);
            OnBreak?.Invoke();
        }
    }

    protected void OnCollisionEnter(Collision other)
    {
        if(hazardCanBreak && other.collider.CompareTag(GameTags.Hazard))
        {
            Break();
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (hazardCanBreak && other.CompareTag(GameTags.Hazard))
        {
            Break();
        }
    }

    protected virtual void Start()
    {
        m_audio = GetComponent<AudioSource>();
        m_collider = GetComponent<Collider>();
        TryGetComponent(out m_rigidBody);
    }
}
