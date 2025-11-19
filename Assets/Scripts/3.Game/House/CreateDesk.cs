using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateDesk : MonoBehaviour, IMouseInteraction
{
    [SerializeField] CreatePanel createPanel;

    void Start()
    {
        if (createPanel == null)
            createPanel = FindObjectOfType<CreatePanel>(true); // 비활성 포함 찾기
    }

    public void CanInteraction(bool _canInteraction)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        throw new System.NotImplementedException();
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        createPanel.GetComponent<CreatePanel>().SetCreateAcquisition(Acquisition.CraftTable);
        createPanel.gameObject.SetActive(true);
    }

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        Debug.Log($"우클릭 함수 호출됨: {gameObject.name}");
    }

    public bool ReturnCanInteraction()
    {
        throw new System.NotImplementedException();
    }
}
