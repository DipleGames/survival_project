using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffDisplayer : MonoBehaviour
{
    [Header("Cache")]
    [SerializeField] Transform buffIconParent;
    [SerializeField] GameObject buffIconPrefab;

    private readonly Dictionary<Buff, BuffIcon> displayingBuffs = new();

    private void Awake()
    {
        if(buffIconParent == null) buffIconParent = transform;
    }
    void OnEnable()
    {
        BuffManager.Instance.BuffApplied += HandleApplied;
        BuffManager.Instance.BuffRemoved += HandleRemoved;
    }
    void OnDisable()
    {
        BuffManager.Instance.BuffApplied -= HandleApplied;
        BuffManager.Instance.BuffRemoved -= HandleRemoved;
    }

    void HandleApplied(Buff buff)
    {
        var view = Instantiate(buffIconPrefab, transform);
        if(view.TryGetComponent<BuffIcon>(out var icon))
        {
            icon.Bind(buff);
            displayingBuffs[buff] = icon;
        }
    }
    void HandleRemoved(Buff buff)
    {
        if (displayingBuffs.TryGetValue(buff, out var b))
        {
            Destroy(b.gameObject);
            displayingBuffs.Remove(buff);
        }
    }
}
