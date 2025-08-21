using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchPlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.ResizeCollider(player.stats.current.crouchHeight);
    }

    protected override void OnExit(Player player)
    {
        player.ResizeCollider(player.originHeight);
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.Fall();
        player.Decelerate(player.stats.current.crouchFriction);

        var inputDirection = player.inputs.GetMovementDirection();

        if (player.inputs.GetCrouchAndCrawl() || !player.canStandUp)
        {
            if (inputDirection.sqrMagnitude > 0 && !player.holding)
            {
                var speedMagnitude = player.lateralVelocity.sqrMagnitude;

                if (player.lateralVelocity.sqrMagnitude == 0)
                {
                    player.states.Change<CrawlingPlayerState>();
                }
            }
            else if (player.inputs.GetJumpDown())
            {
                player.Backflip(player.stats.current.backflipBackwardForce);
            }
        }
        else
        {
            player.states.Change<IdlePlayerState>();
        }
    }

    public override void OnContact(Player player, Collider other) { }
}
