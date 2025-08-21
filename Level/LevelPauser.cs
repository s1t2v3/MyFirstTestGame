using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelPauser : Singleton<LevelPauser>
{
    public UnityEvent OnPause;

    public UnityEvent OnUnpause;

    public UIAnimator pauseScreen;

    public bool canPause { get; set; }

    public bool paused { get; protected set; }

    public virtual void Pause(bool value)
    {
        if (paused != value)
        {
            if (!paused)
            {
                if (canPause)
                {
                    Game.LockCursor(false);
                    paused = true;
                    Time.timeScale = 0;
                    pauseScreen.SetActive(true);
                    pauseScreen?.Show();
                    OnPause?.Invoke();
                }
            }
            else
            {
                Game.LockCursor();
                paused = false;
                Time.timeScale = 1;
                pauseScreen?.Hide();
                OnUnpause?.Invoke();
            }
        }
    }
}
