using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceModel : MonoBehaviour
{
    [SerializeField] private int _fenceHP;
    public int FenceHP
    {
        get => _fenceHP;
        set
        {
            int nv = value;
            _fenceHP = nv;
            if(_fenceHP <= 0)
            {
                ObjectBuilder.Instance.DismantleObject(this.gameObject);
            }
        }
    }

    void OnEnable()
    {
        FenceHP = 30;
    }
}
