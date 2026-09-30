using System.Collections.Generic;
using UnityEngine;

public class LoggingManager : Singleton<LoggingManager>
{
    [SerializeField] private List<GatherableObj> _gathers = new();

    private Transform _treeParent;

    public IReadOnlyList<GatherableObj> Gathers => _gathers;

    public void Initialize(Transform treeParent)
    {
        _treeParent = treeParent;
        RegisterGathers();
    }

    // 이미 배치된 나무 등록
    public void RegisterGathers()
    {
        _gathers.Clear();

        if (_treeParent == null)
        {
            Debug.LogError("treeParent가 연결되지 않았습니다.", this);
            return;
        }

        _gathers.AddRange(_treeParent.GetComponentsInChildren<GatherableObj>(true));
    }

    // 새로 생성된 나무 등록
    public void RegisterGather(GatherableObj gatherableObj)
    {
        if (gatherableObj == null || _treeParent == null || !gatherableObj.transform.IsChildOf(_treeParent) || _gathers.Contains(gatherableObj))
            return;

        _gathers.Add(gatherableObj);
    }

    public void UnregisterGather(GatherableObj gatherableObj)
    {
        _gathers.Remove(gatherableObj);
    }

    public void ClearGathers()
    {
        _gathers.Clear();
    }
}