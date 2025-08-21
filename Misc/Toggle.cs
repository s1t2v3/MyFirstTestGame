using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Toggle : MonoBehaviour
{
    public bool state = true;
    public float delay;
    public Toggle[] multiTrigger;

    public UnityEvent onActivate;

    public UnityEvent onDeactivate;

    public virtual void Set(bool value)
    {
        StopAllCoroutines();
        StartCoroutine(SetRoutine(value));
    }

    protected virtual IEnumerator SetRoutine(bool value)
    {
        yield return new WaitForSeconds(delay);

        if (value)
        {
            if (!state)
            {
                state = true;

                foreach (var toggle in multiTrigger)
                {
                    toggle.Set(state);
                }

                onActivate?.Invoke();
            }
        }
        else if (state)
        {
            state = false;

            foreach (var toggle in multiTrigger)
            {
                toggle.Set(state);
            }

            onDeactivate?.Invoke();
        }
    }
}
