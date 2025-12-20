using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager
{
    private static BuffManager _instance;
    public static BuffManager Instance
    {
        get
        {
            _instance ??= new BuffManager();
            return _instance;
        }
    }
    private BuffManager() { }
    private readonly List<Buff> runningBuffs = new();

    public event Action<Buff> BuffApplied;
    public event Action<Buff> BuffRemoved;

    public T Apply<T>(object args) where T : Buff, new()
    {
        var buff = new T();
        buff.Initialize(args);
        Register(buff);
        BuffApplied?.Invoke(buff);
        buff.OnApply();
        return buff;
    }
    public void ProceedTime()
    {
        float dt = Time.deltaTime;

        for (int i = runningBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = runningBuffs[i];
            buff.OnUpdate(dt);

            if (buff.IsExpired)
            {
                buff.OnRemove();
                BuffRemoved?.Invoke(buff);
                runningBuffs.RemoveAt(i);
            }
        }
    }
    public void Register(Buff buff)
    {
        runningBuffs.Add(buff);
    }
}