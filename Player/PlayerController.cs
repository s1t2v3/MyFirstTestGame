using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    protected Player m_player;
    public Player player
    {
        get
        {
            if (!m_player)
            {
                m_player = FindAnyObjectByType<Player>();
            }
            return m_player;
        }
    }
    public void AddHealth(int amount = 1) => player.health.Increase(amount);
}
