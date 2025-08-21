using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    protected Game m_game => Game.instance;
    protected GameLoader m_loader => GameLoader.instance;
    public virtual void LoadScene(string scene) => m_loader.Load(scene);

    public virtual void AddRetries(int amount) => m_game.retries += amount;
}
