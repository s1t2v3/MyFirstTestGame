using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class EntityStateManager : MonoBehaviour
{
    public EntityStateManagerEvents events;
}

public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected abstract List<EntityState<T>> GetStateList();

    public EntityState<T> current
    {
        get;
        protected set;
    }

    protected List<EntityState<T>> m_list = new List<EntityState<T>>();
    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    protected virtual void InitializedStates()
    {
        m_list = GetStateList();
        foreach(var state in m_list)
        {
            var type = state.GetType();
            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type, state);
            }
        }
        if(m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    public T entity { get; protected set; }

    protected virtual void InitializedEntity()
    {
        this.entity = GetComponent<T>();
    }

    protected void Start()
    {
        InitializedEntity();
        InitializedStates();
    }

    public virtual bool ContainsStateOfType(Type type) => m_states.ContainsKey(type);

    public virtual void Step()
    {
        if(current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }


    public EntityState<T> last { get; protected set; }

    public int lastIndex => m_list.IndexOf(last);
    public int index => m_list.IndexOf(current);

    public virtual void Change(int to)
    {
        if (to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }

    public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);
        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }

    public virtual void Change(EntityState<T> toState)
    {
        if(toState != null && Time.timeScale > 0)
        {
            if(current != null)
            {
                current.Exit(entity);
                events.onExit.Invoke(current.GetType());
                last = current;
            }
            current = toState;
            current.Enter(entity);

            events.onEnter.Invoke(current.GetType());

            events.onChange?.Invoke();
        }
    }

    public virtual void OnContact(Collider other)
    {
        if (current != null && Time.timeScale > 0)
        {
            current.OnContact(entity, other);
        }
    }

    public bool IsCurrentOfType(Type type)
    {
        if(current == null)
        {
            return false;
        }
        return current.GetType() == type;
    }
}

