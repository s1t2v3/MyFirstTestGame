using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;

public class Player : Entity<Player>
{
    public PlayerEvents playerEvents;

    protected Vector3 m_respawnPosition;
    protected Quaternion m_respawnRotation;

    public Transform pickSlot;
    public Transform skin;

    protected Vector3 m_skinInitialPosition;
    protected Quaternion m_skinInitialRotation;

    public PlayerInputManager inputs { get; protected set; }

    public PlayerStatsManager stats {  get; protected set; }

    public Health health { get; protected set; }

    public bool onWater { get; protected set; }

    public int airSpinCounter {  get; protected set; }

    public Collider water { get; protected set; }

    public Pickable pickable { get; protected set; }

    public int airDashCounter {  get; protected set; }

    public float lastDashTime {  get; protected set; }

    public Vector3 lastWallNormal { get; protected set; }

    public Pole pole { get; protected set; }

    public virtual bool isAlive => !health.isEmpty;

    protected const float k_waterExitOffset = 0.25f;

    public virtual bool canStandUp => !SphereCast(Vector3.up, originHeight);

    public virtual void ResetJumps() => jumpCounter = 0;

    public virtual void SetJumps(int amount) => jumpCounter = amount;

    protected virtual void InitializedInputs()
    {
        inputs = GetComponent<PlayerInputManager>();
    }

    protected virtual void InitializedStats()
    {
        stats = GetComponent<PlayerStatsManager>();
    }

    protected virtual void InitializeTag() => tag = GameTags.Player;

    protected virtual void InitializeHealth() => health = GetComponent<Health>();

    protected virtual void InitializeRespawn()
    {
        m_respawnPosition = transform.position;
        m_respawnRotation = transform.rotation;
    }

    protected virtual void InitializeSkin()
    {
        if (skin)
        {
            m_skinInitialPosition = skin.localPosition;
            m_skinInitialRotation = skin.localRotation;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        InitializedInputs();
        InitializedStats();
        InitializeHealth();
        InitializeTag();
        InitializeRespawn();

        entityEvents.OnGroundEnter.AddListener(() =>
        {
            ResetJumps();
            ResetAirSpins();
            ResetAirDash();
        });

        entityEvents.OnRailEnter.AddListener(() =>
        {
            ResetJumps();
            ResetAirSpins();
            ResetAirDash();
            StartGrind();
        });
    }

    public virtual void Respawn()
    {
        health.Reset();
        transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
        states.Change<IdlePlayerState>();
    }

    public virtual void Accelerate(Vector3 direction)
    {
        var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurnningDrag : stats.current.turningDrag;
    
        var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
    
        var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;
    
        var finalAcceleration = isGrounded ? acceleration :  stats.current.airAcceleration;

        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);
    }

    /*public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
    {
        if(direction.sqrMagnitude > 0)
        {
            var speed = Vector3.Dot(direction, lateralVelocity);
            var newVelocity = direction * speed;    
            var turningVelocity = lateralVelocity - newVelocity;
            var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            var targetTopSpeed = topSpeed * topSpeedMultiplier;

            if(lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += acceleration * accelerationMultiplier * Time.deltaTime;
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }

            newVelocity = direction * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            lateralVelocity = newVelocity + turningVelocity;
        }
    }

    public float turningDragMultiplier { get; set; } = 1f;
    public float topSpeedMultiplier { get; set; } = 1f;
    public float accelerationMultiplier { get; set; } = 1f;*/


    public int jumpCounter {  get; protected set; }
    public bool holding { get; protected set; }


    public virtual void FaceDirectionSmooth(Vector3 direction)
    {
        FaceDirection(direction, stats.current.rotationSpeed);
    }

    public virtual void Decelerate()
    {
        Decelerate(stats.current.deceleration);
    }

    public virtual void Friction()
    {
        Decelerate(stats.current.friction);
    }

    public virtual void Gravity()
    {
        if(!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;
            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

    public virtual void Fall()
    {
        if (!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }

    public virtual void Jump()
    {
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);

        if ((isGrounded || onRails || canMultiJump || canCoyoteJump) && !holding)
        {
            if (inputs.GetJumpDown())
            {
                Jump(stats.current.maxJumpHeight);
            }
        }

        if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }

    public virtual void Jump(float height)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        states.Change<FallPlayerState>();
        playerEvents.OnJump?.Invoke();
    }

    protected override bool EvaluateLanding(RaycastHit hit)
    {
        return base.EvaluateLanding(hit) && !hit.collider.CompareTag(GameTags.Spring);
    }


    public virtual void PushRigidbody(Collider other)
    {
        if (!IsPointUnderStep(other.bounds.max) &&
            other.TryGetComponent(out Rigidbody rigidbody))
        {
            var force = lateralVelocity * stats.current.pushForce;
            rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
        }
    }

    public override void ApplyDamage(int amount, Vector3 origin)
    {
        if (!health.isEmpty && !health.recovering)
        {
            health.Damage(amount);
            var damageDir = origin - transform.position;
            damageDir.y = 0;
            damageDir = damageDir.normalized;
            FaceDirection(damageDir);
            lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

            if (!onWater)
            {
                verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                states.Change<HurtPlayerState>();
            }

            playerEvents.OnHurt?.Invoke();

            if (health.isEmpty)
            {
                Throw();
                playerEvents.OnDie?.Invoke();
            }
        }
    }

    public virtual void Spin()
    {
        var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;
        if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
        {
            if (!isGrounded)
            {
                airSpinCounter++;
            }

            states.Change<SpinPlayerState>();
            playerEvents.OnSpin?.Invoke();
        }
    }

    public virtual void AccelerateToInputDirection()
    {
        var inputDirection = inputs.GetMovementCameraDirection();
        Accelerate(inputDirection);
    }

    public virtual  void StompAttack()
    {
        if(!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
        {
            states.Change<StompPlayerState>();
        }
    }

    public virtual void PickAndThrow()
    {
        if (stats.current.canPickUp && inputs.GetPickAndDropDown())
        {
            if (!holding)
            {
                if (CapsuleCast(transform.forward,
                    stats.current.pickDistance, out var hit))
                {
                    if (hit.transform.TryGetComponent(out Pickable pickable))
                    {
                        PickUp(pickable);
                    }
                }
            }
            else
            {
                Throw();
            }
        }
    }

    public virtual void PickUp(Pickable pickable)
    {
        if (!holding && (isGrounded || stats.current.canPickUpOnAir))
        {
            holding = true;
            this.pickable = pickable;
            pickable.PickUp(pickSlot);
            pickable.onRespawn.AddListener(RemovePickable);
            playerEvents.OnPickUp?.Invoke();
        }
    }

    public virtual void Throw()
    {
        if (holding)
        {
            var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
            pickable.Release(transform.forward, force);
            pickable = null;
            holding = false;
            playerEvents.OnThrow?.Invoke();
        }
    }

    public virtual void RemovePickable()
    {
        if (holding)
        {
            pickable = null;
            holding = false;
        }
    }

    public virtual void SetRespawn(Vector3 position, Quaternion rotation)
    {
        m_respawnPosition = position;
        m_respawnRotation = rotation;
    }

    protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
    {
        var contactOffset = Physics.defaultContactOffset + positionDelta;
        var ledgeMaxDistance = radius + forwardDistance;
        var ledgeHeightOffset = height * 0.5f + contactOffset;
        var upwardOffset = transform.up * ledgeHeightOffset;
        var forwardOffset = transform.forward * ledgeMaxDistance;

        if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
            Physics.Raycast(position + forwardOffset * .01f, transform.up, ledgeHeightOffset,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            ledgeHit = new RaycastHit();
            return false;
        }

        var origin = position + upwardOffset + forwardOffset;
        var distance = downwardDistance + contactOffset;

        return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance,
            stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
    }

    public virtual void LedgeGrab()
    {
        if (stats.current.canLedgeHang && velocity.y < 0 && !holding &&
            states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
            DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
        {
            if (!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
            {
                var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                var lateralOffset = transform.forward * ledgeDistance;
                var verticalOffset = Vector3.down * height * 0.5f - center;
                velocity = Vector3.zero;
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                transform.position = hit.point - lateralOffset + verticalOffset;
                states.Change<LedgeHangingPlayerState>();
                playerEvents.OnLedgeGrabbed?.Invoke();
            }
        }
    }

    public virtual void SetSkinParent(Transform parent)
    {
        if (skin)
        {
            skin.parent = parent;
        }
    }

    public virtual void ResetSkinParent()
    {
        if (skin)
        {
            skin.parent = transform;
            skin.localPosition = m_skinInitialPosition;
            skin.localRotation = m_skinInitialRotation;
        }
    }

    public virtual void WaterAcceleration(Vector3 direction) =>
        Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed);

    public virtual void WaterFaceDirection(Vector3 direction) => FaceDirection(direction, stats.current.waterRotationSpeed);

    public virtual void EnterWater(Collider water)
    {
        if (!onWater && !health.isEmpty)
        {
            Throw();
            onWater = true;
            this.water = water;
            states.Change<SwimPlayerState>();
        }
    }

    public virtual void ExitWater()
    {
        if (onWater)
        {
            onWater = false;
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.VolumeWater))
        {
            if (!onWater && other.bounds.Contains(unsizedPosition))
            {
                EnterWater(other);
            }
            else if (onWater)
            {
                var exitPoint = position + Vector3.down * k_waterExitOffset;

                if (!other.bounds.Contains(exitPoint))
                {
                    ExitWater();
                }
            }
        }
    }

    public virtual void Backflip(float force)
    {
        if (stats.current.canBackflip && !holding)
        {
            verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
            lateralVelocity = -transform.forward * force;
            states.Change<BackflipPlayerState>();
            playerEvents.OnBackflip.Invoke();
        }
    }

    public virtual void BackflipAcceleration()
    {
        var direction = inputs.GetMovementCameraDirection();
        Accelerate(direction, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed);
    }

    public virtual void ResizeCollider(float height)
    {
        var delta = height - this.height;
        controller.height = height;
        controller.center += Vector3.up * delta * 0.5f;
    }

    public virtual void CrawlingAccelerate(Vector3 direction) =>
            Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration, stats.current.crawlingTopSpeed);

    public virtual void AirDive()
    {
        if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
        {
            states.Change<AirDivePlayerState>();
            playerEvents.OnAirDive?.Invoke();
        }
    }

    public virtual void RegularSlopeFactor()
    {
        if (stats.current.applySlopeFactor)
            SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
    }

    public virtual void Glide()
    {
        if (!isGrounded && inputs.GetGlide() &&
            verticalVelocity.y <= 0 && stats.current.canGlide)
            states.Change<GlidingPlayerState>();
    }

    public virtual void Dash()
    {
        var canAirDash = stats.current.canAirDash && !isGrounded &&
            airDashCounter < stats.current.allowedAirDashes;
        var canGroundDash = stats.current.canGroundDash && isGrounded &&
            Time.time - lastDashTime > stats.current.groundDashCoolDown;

        if (inputs.GetDashDown() && (canAirDash || canGroundDash))
        {
            if (!isGrounded) airDashCounter++;

            lastDashTime = Time.time;
            states.Change<DashPlayerState>();
        }
    }

    public virtual void WallDrag(Collider other)
    {
        if (stats.current.canWallDrag && velocity.y <= 0 &&
            !holding && !other.TryGetComponent<Rigidbody>(out _))
        {
            if (CapsuleCast(transform.forward, 0.25f, out var hit,
                stats.current.wallDragLayers) && !DetectingLedge(0.25f, height, out _))
            {
                if (hit.collider.CompareTag(GameTags.Platform))
                    transform.parent = hit.transform;

                lastWallNormal = hit.normal;
                states.Change<WallDragPlayerState>();
            }
        }
    }

    public virtual void DirectionalJump(Vector3 direction, float height, float distance)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        lateralVelocity = direction * distance;
        playerEvents.OnJump?.Invoke();
    }

    public virtual void GrabPole(Collider other)
    {
        if (stats.current.canPoleClimb && velocity.y <= 0
            && !holding && other.TryGetComponent(out Pole pole))
        {
            this.pole = pole;
            states.Change<PoleClimbingPlayerState>();
        }
    }

    public virtual void StartGrind() => states.Change<RailGrindPlayerState>();
    public virtual void ResetAirSpins() => airSpinCounter = 0;
    public virtual void ResetAirDash() => airDashCounter = 0;


    //public virtual void Dash()
    //{
    //    var canAirDash = stats.current.canAirDash && !isGrounded &&
    //        airDashCounter < stats.current.allowedAirDashes;
    //    var canGroundDash = stats.current.canGroundDash && isGrounded &&
    //        Time.time - lastDashTime > stats.current.groundDashCoolDown;

    //    if (inputs.GetDashDown() && (canAirDash || canGroundDash))
    //    {
    //        if (!isGrounded) airDashCounter++;

    //        lastDashTime = Time.time;
    //        states.Change<DashPlayerState>();
    //    }
    //}
}
