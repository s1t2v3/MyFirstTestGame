using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;

public class WalkPlayerState : PlayerState
{
    protected override void OnEnter(Player entity)
    {

    }

    protected override void OnExit(Player entity)
    {

    }

    protected override void OnStep(Player entity)
    {
        entity.Gravity();
        entity.Fall();
        entity.Jump();
        entity.Spin();
        entity.SnapToGround();
        entity.PickAndThrow();
        entity.Dash();

        entity.RegularSlopeFactor();
        var inputDirection = entity.inputs.GetMovementCameraDirection();

        if (inputDirection.sqrMagnitude > 0)
        {
            var dot = Vector3.Dot(inputDirection, entity.lateralVelocity);
            if(dot >= entity.stats.current.brakeThreshold)
            {
                entity.Accelerate(inputDirection);

                entity.FaceDirectionSmooth(entity.lateralVelocity);
            }
            else
            {
                entity.states.Change<BrakePlayerState>();
            }
        }
        else
        {
            entity.Friction();
            if(entity.lateralVelocity.sqrMagnitude <= 0)
            {
                entity.states.Change<IdlePlayerState>();
            }
        }

        if (entity.inputs.GetCrouchAndCrawl())
        {
            entity.states.Change<CrouchPlayerState>();
        }
    }

    public override void OnContact(Player entity, Collider other)
    {
        entity.PushRigidbody(other);
    }
}
