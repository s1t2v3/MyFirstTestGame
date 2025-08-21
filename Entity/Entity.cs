using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public abstract class Entity : MonoBehaviour
{
    public EntityEvents entityEvents;

    protected readonly float m_slopingGroundAngle = 20f;

    public CharacterController controller { get; protected set; }

    public SplineContainer rails { get; protected set; }

    public float groundAngle { get; protected set; }

    public Vector3 groundNormal { get; protected set; }

    public Vector3 localSlopeDirection { get; protected set; }

    public Vector3 velocity{ get; set; }

    public Vector3 lateralVelocity
    {
        get
        {
            return new Vector3(velocity.x, 0, velocity.z);
        }
        set
        {
            velocity = new Vector3(value.x, velocity.y, value.z);
        }
    }

    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }

    public Vector3 position => transform.position + center;
    public Vector3 unsizedPosition => position - transform.up * height * 0.5f + transform.up * originHeight * 0.5f;
    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);

    public Vector3 center => controller.center;
    public float height => controller.height;
    public float radius => controller.radius;

    protected Rigidbody m_rigidbody;

    public Vector3 lastPosition { get; set; }

    public float positionDelta { get; protected set; }

    public float originHeight { get; protected set; }

    public bool isGrounded { get; protected set; } = true;

    public bool onRails { get; set; }

    public float gravityMultiplier { get; set; } = 1f;
    public float decelerationMultiplier { get; set; } = 1f;
    public float turningDragMultiplier { get; set; } = 1f;
    public float topSpeedMultiplier { get; set; } = 1f;
    public float accelerationMultiplier { get; set; } = 1f;

    public virtual bool IsPointUnderStep(Vector3 point)
    {
        return stepPosition.y > point.y;
    }

    public virtual void ApplyDamage(int damage, Vector3 origin) { }

    public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
    QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
    }

    public virtual bool CapsuleCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var origin = position - direction * radius + center;
        var offset = transform.up * (height * 0.5f - radius);
        var top = origin + offset;
        var bottom = origin - offset;
        return Physics.CapsuleCast(top, bottom, radius, direction,
            out hit, distance + radius, layer, queryTriggerInteraction);
    }

    public virtual bool SphereCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
    QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
    }

    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position, radius, direction,
            out hit, castDistance, layer, queryTriggerInteraction);
    }
}

public abstract class Entity<T> : Entity where T : Entity<T>
{
    public EntityStateManager<T> states
    {
        get;
        protected set;
    }

    protected virtual void InitializeCollider()
    {
        m_collider = gameObject.AddComponent<CapsuleCollider>();
        m_collider.height = controller.height;
        m_collider.radius = controller.radius;
        m_collider.center = controller.center;
        m_collider.isTrigger = true;
        m_collider.enabled = false;
    }

    protected virtual void InitializeRigidbody()
    {
        m_rigidbody = gameObject.AddComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;
    }

    protected virtual void InitializeStateManager()
    {
        this.states = GetComponent<EntityStateManager<T>>();
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeController();
    }

    protected virtual void HandleState()
    {
        states.Step();
    }

    protected virtual void HandlePosition()
    {
        positionDelta = (position - lastPosition).magnitude;
        lastPosition = position;
    }

    protected virtual void HandleSpline()
    {
        var distance = (height * 0.5f) + height * 0.5f;

        if (SphereCast(-transform.up, distance, out var hit) &&
            hit.collider.CompareTag(GameTags.InteractiveRail))
        {
            if (!onRails && verticalVelocity.y <= 0)
            {
                EnterRail(hit.collider.GetComponent<SplineContainer>());
            }
        }
        else
        {
            ExitRail();
        }
    }

    protected virtual void Update()
    {
        if (controller.enabled || m_collider != null)
        {
            HandleState();
            HandleController();
            HandleSpline();
            HandleGround();
            HandleContacts();
            OnUpdate();
        }
    }

    protected virtual void LateUpdate()
    {
        if (controller.enabled)
        {
            HandlePosition();
        }
    }

    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(velocity * Time.deltaTime);
            return;
        }
        transform.position += velocity * Time.deltaTime;
    }


    protected virtual void InitializeController()
    {
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
        controller.skinWidth = 0.005f;
        controller.minMoveDistance = 0;

        //camera
        originHeight = controller.height;
    }
    
    public virtual void FaceDirection(Vector3 direction, float degreesPerSpeed)
    {
        if(direction != Vector3.zero)
        {
            var rotation = transform.rotation;
            var rotationDelta = degreesPerSpeed * Time.deltaTime;
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
        }
    }

    
    public virtual void Decelerate(float deceleration)
    {
        var delta = deceleration * decelerationMultiplier * Time.deltaTime;
        lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
    }
    
    protected readonly float m_groundOffset = 0.1f;
    public float lastGroundTime {  get; protected set; }
    
    public RaycastHit groundHit;

    public virtual bool SphereCast(Vector3 direction, float distance, out RaycastHit hit, int layer = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
    }

    protected virtual void HandleGround()
    {
        var distance = (height * 0.5f) + m_groundOffset;
        if(SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            if (!isGrounded)
            {
                if (EvaluateLanding(hit))
                {
                    EnterGround(hit);
                }
                else
                {
                    HandleHighLedge(hit);
                }
            }
            else if (IsPointUnderStep(hit.point))
            {
                UpdateGround(hit);

                if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
                {
                    //HandleSlopeLimit(hit);
                }
            }
        }
        else
        {
            ExitGround();
        }
    }

    protected virtual bool EvaluateLanding(RaycastHit hit)
    {
        return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
    }

    protected virtual void HandleHighLedge(RaycastHit hit)
    {

    }

    protected virtual void EnterGround(RaycastHit hit)
    {
        if (!isGrounded)
        {
            groundHit = hit;
            isGrounded = true;
            entityEvents.OnGroundEnter?.Invoke();
        }
    }

    protected  virtual void ExitGround()
    {
        if (isGrounded)
        {
            isGrounded = false;
            transform.parent = null;
            lastGroundTime = Time.time;
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    protected virtual void UpdateGround(RaycastHit hit)
    {
        if (isGrounded)
        {
            groundHit = hit;
            groundNormal = groundHit.normal;
            groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
            localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
        }
    }

    public virtual void UseCustomCollision(bool value)
    {
        controller.enabled = !value;

        if (value)
        {
            InitializeCollider();
            InitializeRigidbody();
        }
        else
        {
            Destroy(m_collider);
            Destroy(m_rigidbody);
        }
    }

    protected virtual void EnterRail(SplineContainer rails)
    {
        if (!onRails)
        {
            onRails = true;
            this.rails = rails;
            entityEvents.OnRailEnter.Invoke();
        }
    }

    public virtual void ExitRail()
    {
        if (onRails)
        {
            onRails = false;
            entityEvents.OnRailExit.Invoke();
        }
    }

    //Camera
    protected Collider[] m_contactBuffer = new Collider[10];
    protected CapsuleCollider m_collider;

    public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
    {
        var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
        var overlapsRadius = radius + contactOffset;
        var offset = (height +contactOffset) * 0.5f - overlapsRadius;
        var top = position + Vector3.up * offset;
        var bottom = position + Vector3.down * offset;
        return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
    }

    protected virtual void HandleContacts()
    {
        var overlaps = OverlapEntity(m_contactBuffer);

        for (int i = 0; i < overlaps; i++)
        {
            if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)
            {
                OnContact(m_contactBuffer[i]);

                var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();

                foreach (var contact in listeners)
                {
                    contact.OnEntityContact((T)this);
                }

                if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
                {
                    verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
                }
            }
        }
    }

    protected virtual void OnUpdate() { }

    protected virtual void OnContact(Collider other)
    {
        if (other)
        {
            states.OnContact(other);
        }
    }

    public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
    {
        if (direction.sqrMagnitude > 0)
        {
            var speed = Vector3.Dot(direction, lateralVelocity);
            var newVelocity = direction * speed;
            var turningVelocity = lateralVelocity - newVelocity;
            var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            var targetTopSpeed = topSpeed * topSpeedMultiplier;

            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += acceleration * accelerationMultiplier * Time.deltaTime;
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }

            newVelocity = direction * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            lateralVelocity = newVelocity + turningVelocity;
        }
    }

    public virtual void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = rotation;
        }
    }

    public virtual void Gravity(float gravity)
    {
        if (!isGrounded)
        {
            verticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    public virtual void SnapToGround(float force)
    {
        if (isGrounded && (verticalVelocity.y <= 0))
        {
            verticalVelocity = Vector3.down * force;
        }
    }

    public virtual bool FitsIntoPosition(Vector3 position)
    {
        var bounds = controller.bounds;
        var radius = controller.radius - controller.skinWidth;
        var offset = height * 0.5f - radius;
        var top = position + Vector3.up * offset;
        var bottom = position - Vector3.up * offset;

        return !Physics.CheckCapsule(top, bottom, radius,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    public virtual void SlopeFactor(float upwardForce, float downwardForce)
    {
        if (!isGrounded || !OnSlopingGround()) return;

        var factor = Vector3.Dot(Vector3.up, groundNormal);
        var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
        var multiplier = downwards ? downwardForce : upwardForce;
        var delta = factor * multiplier * Time.deltaTime;
        lateralVelocity += localSlopeDirection * delta;
    }

    public virtual bool OnSlopingGround()
    {
        if (isGrounded && groundAngle > m_slopingGroundAngle)
        {
            if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
            else
                return true;
        }

        return false;
    }
}