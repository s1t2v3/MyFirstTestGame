using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    public float walkDustMinSpeed = 3.5f;
    public float landingParticleMinSpeed = 5f;

    public ParticleSystem walkDust;
    public ParticleSystem landDust;
    public ParticleSystem hurtDust;
    public ParticleSystem dashDust;
    public ParticleSystem speedTrails;
    public ParticleSystem grindTrails;

    protected Player m_player;

    public virtual void Play(ParticleSystem particle)
    {
        if (!particle.isPlaying)
        {
            particle.Play();
        }
    }

    public virtual void Stop(ParticleSystem particle, bool clear = false)
    {
        if (particle.isPlaying)
        {
            var mode = clear ? ParticleSystemStopBehavior.StopEmittingAndClear :
                ParticleSystemStopBehavior.StopEmitting;
            particle.Stop(true, mode);
        }
    }

    protected virtual void HandleWalkParticle()
    {
        if (m_player.isGrounded && !m_player.onRails && !m_player.onWater)
        {
            if (m_player.lateralVelocity.magnitude > walkDustMinSpeed)
            {
                Play(walkDust);
            }
            else
            {
                Stop(walkDust);
            }
        }
        else
        {
            Stop(walkDust);
        }
    }

    protected virtual void HandleRailParticle()
    {
        if (m_player.onRails)
            Play(grindTrails);
        else
            Stop(grindTrails, true);
    }

    protected virtual void HandleLandParticle()
    {
        if (!m_player.onWater &&
            Mathf.Abs(m_player.velocity.y) >= landingParticleMinSpeed)
        {
            Play(landDust);
        }
    }

    protected virtual void HandleHurtParticle() => Play(hurtDust);

    protected virtual void OnDashStarted()
    {
        Play(dashDust);
        Play(speedTrails);
    }

    protected virtual void Start()
    {
        m_player = GetComponent<Player>();
        m_player.entityEvents.OnGroundEnter.AddListener(HandleLandParticle);
        m_player.playerEvents.OnHurt.AddListener(HandleHurtParticle);
        m_player.playerEvents.OnDashStarted.AddListener(OnDashStarted);
        m_player.playerEvents.OnDashEnded.AddListener(() => Stop(speedTrails, true));
    }

    protected virtual void Update()
    {
        HandleWalkParticle();
        HandleRailParticle();
    }
}
