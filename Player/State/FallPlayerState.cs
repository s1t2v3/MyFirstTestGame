using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallPlayerState : PlayerState
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
        entity.SnapToGround();
        entity.FaceDirectionSmooth(entity.lateralVelocity);
        entity.AccelerateToInputDirection();
        entity.Jump();
        entity.Spin();
        entity.StompAttack();
        entity.PickAndThrow();
        entity.LedgeGrab();
        entity.AirDive();
        entity.Glide();
        entity.Dash();
        if (entity.isGrounded)
        {
            entity.states.Change<IdlePlayerState>();
        }
    }
    public override void OnContact(Player entity, Collider other)
    {
        entity.PushRigidbody(other);
        entity.WallDrag(other);
        entity.GrabPole(other);
    }
}
