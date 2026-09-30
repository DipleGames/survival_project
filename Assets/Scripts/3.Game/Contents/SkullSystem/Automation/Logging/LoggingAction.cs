using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingAction
{
    private readonly ToolType _toolType;


    public LoggingAction(ToolType toolType, float workDuration)
    {
        _toolType = toolType;
    }

    public bool CanExecute(SkullController skull, GatherableObj target)
    {
        if (skull == null || target == null)
            return false;

        return target.CanBeGatheredBy(skull);
    }

    public bool Execute(SkullController skull, GatherableObj target)
    {
        if (!CanExecute(skull, target))
            return false;

        target.ExecuteGatherBySkull(skull, _toolType);
        return true;
    }
}
