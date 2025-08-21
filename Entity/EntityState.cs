using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EntityState<T> where T : Entity<T>
{
    public UnityEvent onEnter;
    public UnityEvent onExit;

    public float timeSinceEntered
    {
        get;
        protected set;
    }
    
    public void Enter(T entity)
    {
        timeSinceEntered = 0f;
        onEnter?.Invoke();
        OnEnter(entity);
    }

    public void Exit(T entity)
    {
        onExit?.Invoke();
        OnExit(entity);
    }

    public void Step(T entity)
    {
        OnStep(entity);
        timeSinceEntered += Time.deltaTime;
    }

    protected abstract void OnEnter(T entity);
    protected abstract void OnExit(T entity);
    protected abstract void OnStep(T entity);

    public static EntityState<T> CreateFromString(string typeName)
    {
        return System.Activator.CreateInstance(System.Type.GetType(typeName)) as EntityState<T>;
    }

    public static List<EntityState<T>> CreateListFromStringArray(string[] array)
    {
        var list = new List<EntityState<T>>();

        foreach (var typeName in array)
        {
            list.Add(CreateFromString(typeName));
        }

        return list;
    }

    public abstract void OnContact(T entity, Collider other);
}
