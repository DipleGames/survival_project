using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineCaching
{
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

    private static Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();
    private static Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealTime = new Dictionary<float, WaitForSecondsRealtime>();
    private static Dictionary<Func<bool>, WaitWhile> waitWhile = new Dictionary<Func<bool>, WaitWhile>();
    private static Dictionary<Func<bool>, WaitUntil> waitUntil = new Dictionary<Func<bool>, WaitUntil>();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!waitForSeconds.TryGetValue(seconds, out var _waitForSeconds))
        {
            waitForSeconds.Add(seconds, _waitForSeconds = new WaitForSeconds(seconds));
        }

        return _waitForSeconds;
    }

    public static WaitForSecondsRealtime WaitForSecondsRealTime(float seconds)
    {
        if (!waitForSecondsRealTime.TryGetValue(seconds, out var _waitForSecondsRealTime))
        {
            waitForSecondsRealTime.Add(seconds, _waitForSecondsRealTime = new WaitForSecondsRealtime(seconds));
        }

        return _waitForSecondsRealTime;
    }

    public static WaitWhile WaitWhile(Func<bool> predicate)
    {
        if(!waitWhile.TryGetValue(predicate, out var _waitWhile))
        {
            waitWhile.Add(predicate, _waitWhile = new WaitWhile(predicate));
        }

        return _waitWhile;
    }

    public static WaitUntil WaitUntil(Func<bool> predicate)
    {
        if (!waitUntil.TryGetValue(predicate, out var _waitUntil))
        {
            waitUntil.Add(predicate, _waitUntil = new WaitUntil(predicate));
        }

        return _waitUntil;
    }
}
