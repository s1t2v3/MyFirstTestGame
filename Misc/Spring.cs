using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour,IEntityContact
{
    public float force = 25f;
    public AudioClip clip;

    protected AudioSource m_audio;
    protected Collider m_collider;

    public void ApplyForce(Player player)
    {
        if (player.verticalVelocity.y <= 0)
        {
            m_audio.PlayOneShot(clip);
            player.verticalVelocity = Vector3.up * force;
        }
    }

    public void OnEntityContact(Entity entity)
    {
        if (entity.IsPointUnderStep(m_collider.bounds.max) &&
            entity is Player player && player.isAlive)
        {
            ApplyForce(player);
            player.SetJumps(1);
            player.ResetAirSpins();
            player.ResetAirDash();
            player.states.Change<FallPlayerState>();
        }
    }

    protected virtual void Start()
    {
        tag = GameTags.Spring;
        m_collider = GetComponent<Collider>();

        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }
}
