using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryAnim : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] GameObject DiscriptionContents;

    private void OnEnable()
    {
        animator.enabled = true;
        DiscriptionContents.SetActive(false);
    }

    public void StartOpeningAnim()
    {
        animator.SetTrigger("Start");
    }

    public void EndOpeningAnim()
    {
        animator.enabled = false;
        DiscriptionContents.SetActive(true);
    }
}
