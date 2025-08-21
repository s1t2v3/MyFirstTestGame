using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [System.Serializable]
    public class ForcedTransition
    {
        public int fromStateId;
        public int animatorLayer;
        public string toAnimationState;
    }

    [Header("Paramters Names")]
    public string stateName = "State";
    public string lastStateName = "Last State";
    public string lateralSpeedName = "Lateral Speed";
    public string verticalSpeedName = "Vertical Speed";
    public string lateralAnimationSpeedName = "Lateral Animation Speed";
    public string healthName = "Health";
    public string jumpCounterName = "Jump Counter";
    public string isGroundedName = "Is Grounded";
    public string isHoldingName = "Is Holding";
    public string onStateChangedName = "On State Changed";

    [Header("Settings")]
    public float minLateralAnimationSpeed = 0.5f;

    public List<ForcedTransition> forcedTransitions;

    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_healthHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_isHoldingHash;
    protected int m_onStateChangedHash;

    protected Dictionary<int, ForcedTransition> m_forcedTransisions;

    protected Player m_player;

    public Animator animator;

    protected void Start()
    {
        InitializePlayer();
        InitializedForcedTransitions();
        InitializedParametersHash();
        InitializedAnimatorTrigger();
    }
    protected virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();

        m_player.states.events.onChange.AddListener(HandleForcedTransitions);

    }

    protected virtual void InitializedForcedTransitions()
    {
        m_forcedTransisions = new Dictionary<int, ForcedTransition>();
        foreach(var transition in forcedTransitions)
        {
            if (!m_forcedTransisions.ContainsKey(transition.fromStateId))
            {
                m_forcedTransisions.Add(transition.fromStateId, transition);
            }
        }
    }

    protected virtual void InitializedParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_isHoldingHash = Animator.StringToHash(isHoldingName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }

    protected virtual void InitializedAnimatorTrigger()
    {
        m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));
    }

    protected virtual void HandleForcedTransitions()
    {
        var lastStateIndex = m_player.states.lastIndex;

        if (m_forcedTransisions.ContainsKey(lastStateIndex))
        {
            var layer = m_forcedTransisions[lastStateIndex].animatorLayer;
            animator.Play(m_forcedTransisions[lastStateIndex].toAnimationState, layer);
        }
    }

    protected virtual void HandleAnimatorParameters()
    {
        var lateralSpeed = m_player.lateralVelocity.magnitude;
        var verticalSpeed = m_player.verticalVelocity.y;
        var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);
        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
        animator.SetInteger(m_jumpCounterHash, m_player.jumpCounter);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
        animator.SetBool(m_isHoldingHash, m_player.holding);
    }

    protected void LateUpdate()
    {
        HandleAnimatorParameters();
    }
}
