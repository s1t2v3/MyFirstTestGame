using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EntityEvents
{
    public UnityEvent OnGroundEnter;
    public UnityEvent OnGroundExit;
    public UnityEvent OnRailEnter;
    public UnityEvent OnRailExit;
}
