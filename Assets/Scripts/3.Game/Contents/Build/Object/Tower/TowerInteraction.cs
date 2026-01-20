using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerInteraction : MonoBehaviour, IMouseInteraction
{
    [Header("타워")]
    public GameObject tower;
    public TowerModel towerModel;

    [Header("UIRoot")]
    public GameObject uiRoot;

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
 
    }

    /// <summary>
    /// 타워 ui 키기
    /// </summary>
    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        if(GetComponent<TowerBuildInfo>().isBuilt)
        {
            uiRoot.SetActive(true);
        }
    }

    public void CanInteraction(bool _canInteraction)
    {
        
    }
    public bool ReturnCanInteraction()
    {
        return false;
    }
    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        yield return null;
    }

    #region 버튼 상호작용
    public void OnClickedTowerInfoBtn()
    {
        GameSceneUI.Instance.OnTowerWindow(towerModel);
        uiRoot.SetActive(false);
    }

    public void OnClickedDismantleBtn()
    {
        ObjectBuilder.Instance.DismantleObject(tower);
    }
    #endregion
}
