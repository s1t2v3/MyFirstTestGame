using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakePlayerState : PlayerState
{
    protected override void OnEnter(Player entity)
    {

    }

    protected override void OnExit(Player entity)
    {

    }

    protected override void OnStep(Player entity)
    {
        var inputDirection = entity.inputs.GetMovementCameraDirection();
        if (entity.stats.current.canBackflip &&
            Vector3.Dot(inputDirection, entity.transform.forward) < 0 &&
            entity.inputs.GetJumpDown())
        {
            entity.Backflip(entity.stats.current.backflipBackwardTurnForce);
        }
        else
        {
            entity.Fall();
            entity.Jump();
            entity.SnapToGround();
            entity.Decelerate();

            if (entity.lateralVelocity.sqrMagnitude == 0)
            {
                entity.states.Change<IdlePlayerState>();
            }
        }
    }

    public override void OnContact(Player entity, Collider other)
    {

    }
}
