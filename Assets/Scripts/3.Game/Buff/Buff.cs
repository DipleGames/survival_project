using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Buff
{
    //public GameObject Giver { get; private set; }
    //public GameObject Target { get; private set; }
    //public float Duration { get; protected set; }
    //public bool IsExpired => elapsedTime >= Duration;
    //float elapsedTime;

    public float Duration { get; protected set; }
    public int MaxStacks { get; protected set; } = 0;
    public bool IsExpired => Duration > 0 && elapsedTime >= Duration;
    float elapsedTime;
    public float ElapsedTime => elapsedTime;

    // UI용 데이터
    public virtual string DisplayName => GetType().Name;

    protected string description;
    public virtual string Description => description;
    public virtual string IconPath => "UI/Buffs"; //Resources 폴더 내


    public abstract void Initialize(object args);
    public virtual void OnApply() { Debug.Log("Need OnApply"); }
    public virtual void OnRemove() { Debug.Log("Need OnRemove"); }

    public void OnUpdate(float deltaTime)
    {
        elapsedTime += deltaTime;
    }
}
public abstract class Buff<T> : Buff
{
    public sealed override void Initialize(object args)
    {
        Initialize((T)args);
    }

    protected abstract void Initialize(T args);
}