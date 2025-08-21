using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset actions;

    public InputAction m_movement;

    protected Camera m_camera;

    protected InputAction m_run;
    protected InputAction m_jump;
    protected InputAction m_look;
    protected InputAction m_pause;
    protected InputAction m_spin;
    protected InputAction m_stomp;
    protected InputAction m_pickAndDrop;
    protected InputAction m_releaseLedge;
    protected InputAction m_dive;
    protected InputAction m_crouch;
    protected InputAction m_airDive;
    protected InputAction m_glide;
    protected InputAction m_grindBrake;
    protected InputAction m_dash;

    protected float? m_lastJumpTime;

    protected const float k_jumpBuffer = 0.15f;
    protected const string k_mouseDeviceName = "Mouse";


    protected virtual void CacheActions()
    {
        m_movement = actions["Movement"];
        m_run = actions["Run"];
        m_jump = actions["Jump"];
        m_look = actions["Look"];
        m_pause = actions["Pause"];
        m_spin = actions["Spin"];
        m_stomp = actions["Stomp"];
        m_pickAndDrop = actions["PickAndDrop"];
        m_releaseLedge = actions["ReleaseLedge"];
        m_dive = actions["Dive"];
        m_crouch = actions["Crouch"];
        m_airDive = actions["AirDive"];
        m_glide = actions["Glide"];
        m_grindBrake = actions["GrindBrake"];
        m_dash = actions["Dash"];
    }


    protected virtual void Awake()
    {
        m_camera = Camera.main;
        CacheActions();//缓存输入动作的引用，提高后续访问效率
    }

    private void Start()
    {
        actions.Enable();//启用输入动作资产，使输入系统开始监听玩家输入
    }

    protected virtual void Update()
    {
        if (m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime = Time.time;
        }
    }

    protected void OnEnable()//对象启用时调用，确保输入系统在对象激活时启用。
    {
        actions?.Enable();
    }

    protected void OnDisable()//对象禁用时调用，禁用输入系统以避免空引用错误或不必要的输入监听。
    {
        actions?.Disable();
    }


    protected float m_movementDirectionUnlockTime;

    public virtual Vector3 GetMovementDirection()
    {
        if (Time.time < m_movementDirectionUnlockTime)
        {
            return Vector3.zero;
        }
        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadZone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadZone ? RemapToDeadzone(axis.x, deadZone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadZone ? RemapToDeadzone(axis.y, deadZone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }

    protected float RemapToDeadzone(float value, float deadzone)
    {
        return (value - (value > 0 ? -deadzone : deadzone)) / (1 - deadzone);
    }


    public virtual Vector3 GetMovementCameraDirection()
    {
        var direction = GetMovementDirection();

        if(direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
            direction = rotation * direction;
            direction = direction.normalized;
        }
        return direction;
    }


    public virtual bool GetRun()
    {
        return m_run.IsPressed();
    }

    public virtual bool GetRunUp()
    {
        return m_run.WasPerformedThisFrame();
    }

    public virtual bool GetJumpDown()
    {
        if (m_lastJumpTime != null &&
            Time.time - m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }

        return false;
    }

    public virtual bool GetJumpUp()
    {
        return m_jump.WasReleasedThisFrame();
    }


    public virtual bool IsLookingWithMouse()
    {
        if (m_look.activeControl == null)
        {
            return false;
        }

        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }

    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();

        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }

        return GetAxisWithCrossDeadZone(value);
    }

    public virtual bool GetPauseDown()
    {
        return m_pause.WasPressedThisFrame();
    }

    public virtual void LockMovementDirection(float duration = 0.25f)
    {
        m_movementDirectionUnlockTime = Time.time + duration;
    }

    public virtual bool GetSpinDown()
    {
        return m_spin.WasReleasedThisFrame();
    }

    public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();

    public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();

    public virtual bool GetReleaseLedgeDown() => m_releaseLedge.WasPressedThisFrame();

    public virtual bool GetDive() => m_dive.IsPressed();

    public virtual bool GetCrouchAndCrawl() => m_crouch.IsPressed();

    public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();

    public virtual bool GetGlide() => m_glide.IsPressed();

    public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();

    public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();
}
