using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelFinisher : Singleton<LevelFinisher>
{
    public UnityEvent OnFinish;
    public UnityEvent OnExit;

    public bool unlockNextLevel;
    public string nextScene;
    public string exitScene;
    public float loadingDelay = 1f;

    protected Game m_game => Game.instance;
    protected Level m_level => Level.instance;
    protected LevelScore m_score => LevelScore.instance;
    protected LevelPauser m_pauser => LevelPauser.instance;
    protected GameLoader m_loader => GameLoader.instance;

    protected virtual IEnumerator FinishRoutine()
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_score.stopTime = true;
        m_level.player.inputs.enabled = false;

        yield return new WaitForSeconds(loadingDelay);

        if (unlockNextLevel)
        {
            m_game.UnlockNextLevel();
        }

        Game.LockCursor(false);
        m_score.Consolidate();
        m_loader.Load(nextScene);
        OnFinish?.Invoke();
    }

    protected virtual IEnumerator ExitRoutine()
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.inputs.enabled = false;
        yield return new WaitForSeconds(loadingDelay);
        Game.LockCursor(false);
        m_loader.Load(exitScene);
        OnExit?.Invoke();
    }

    public virtual void Finish()
    {
        StopAllCoroutines();
        StartCoroutine(FinishRoutine());
    }

    public virtual void Exit()
    {
        StopAllCoroutines();
        StartCoroutine(ExitRoutine());
    }
}
