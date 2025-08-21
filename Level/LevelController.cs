using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class LevelController : MonoBehaviour
{
    protected LevelScore m_score => LevelScore.instance;

    protected LevelPauser m_pauser => LevelPauser.instance;

    protected LevelFinisher m_finisher => LevelFinisher.instance;

    protected LevelRespawner m_respawner => LevelRespawner.instance;

    public virtual void AddCoins(int amount) => m_score.coins += amount;

    public virtual void Pause(bool value) => m_pauser.Pause(value);

    public virtual void Exit() => m_finisher.Exit();

    public virtual void Finish() => m_finisher.Finish();

    public virtual void Respawn(bool consumeRetries) => m_respawner.Respawn(consumeRetries);
    public virtual void Restart() => m_respawner.Restart();
}
