using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdlePlayerState : PlayerState
{
    protected override void OnEnter(Player entity)
    {

    }

    protected override void OnExit(Player entity)
    {
        
    }

    protected override void OnStep(Player entity)
    {
        entity.Friction();
        entity.Gravity();
        entity.Fall();
        entity.Jump();
        entity.Spin();
        entity.SnapToGround();
        entity.PickAndThrow();

        entity.RegularSlopeFactor();
        var inputDirection = entity.inputs.GetMovementDirection();

        if (inputDirection.sqrMagnitude > 0 || entity.lateralVelocity.sqrMagnitude > 0)
        {
            entity.states.Change<WalkPlayerState>();
        }
        else if (entity.inputs.GetCrouchAndCrawl())
        {
            entity.states.Change<CrouchPlayerState>();
        }
    }

    public override void OnContact(Player entity, Collider other)
    {

    }
}
