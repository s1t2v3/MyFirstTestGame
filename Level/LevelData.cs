using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LevelData
{
    public bool locked;
    public int coins;
    public float time;
    public bool[] stars = new bool[GameLevel.StarsPerLevel];
}
