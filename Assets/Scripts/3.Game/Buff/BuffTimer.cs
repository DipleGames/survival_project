using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffTimer : MonoBehaviour
{
    void Update()
    {
        BuffManager.Instance.ProceedTime();
    }

}