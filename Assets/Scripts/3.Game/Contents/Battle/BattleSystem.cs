using UnityEngine;
using System.Collections;

public class BattleSystem : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            StartCoroutine(BattleRoutine());
        }
    }

    IEnumerator BattleRoutine()
    {
        yield return null;
    }
}
