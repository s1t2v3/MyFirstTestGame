using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinPlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        if (!player.isGrounded)
        {
            player.verticalVelocity = Vector3.up * player.stats.current.airSpinUpwardForce;
        }
    }

    protected override void OnExit(Player player) { }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.AccelerateToInputDirection();
        player.StompAttack();
        player.AirDive();

        if (timeSinceEntered >= player.stats.current.spinDuration)
        {
            if (player.isGrounded)
            {
                player.states.Change<IdlePlayerState>();
            }
            else
            {
                player.states.Change<FallPlayerState>();
            }
        }
    }

    public override void OnContact(Player player, Collider other) { }
}
