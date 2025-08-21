using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameLoader : Singleton<GameLoader>
{
    public UnityEvent OnLoadStart;

    public UnityEvent OnLoadFinish;

    public UIAnimator loadingScreen;

    [Header("Minimum Time")]
    public float startDelay = 1f;
    public float finishDelay = 1f;

    public bool isLoading { get; protected set; }

    public float loadingProgress { get; protected set; }

    public string currentScene => SceneManager.GetActiveScene().name;

    public virtual void Load(string scene)
    {
        StartCoroutine(LoadRoutine(scene));
    }

    protected virtual IEnumerator LoadRoutine(string scene)
    {
        OnLoadStart?.Invoke();
        isLoading = true;
        loadingScreen.SetActive(true);
        loadingScreen.Show();

        yield return new WaitForSeconds(startDelay);

        var operation = SceneManager.LoadSceneAsync(scene);
        loadingProgress = 0;

        while (!operation.isDone)
        {
            loadingProgress = operation.progress;
            yield return null;
        }

        loadingProgress = 1;

        yield return new WaitForSeconds(finishDelay);

        isLoading = false;
        loadingScreen.Hide();
        OnLoadFinish?.Invoke();
    }
    public virtual void Reload()
    {
        StartCoroutine(LoadRoutine(currentScene));
    }
}
