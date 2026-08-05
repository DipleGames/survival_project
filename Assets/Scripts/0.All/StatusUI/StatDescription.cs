using UnityEngine;

public class StatDescription : MonoBehaviour
{
    [SerializeField] GameObject descripPanel;

    void Start()
    {
        descripPanel.SetActive(false);
    }

    public void ShowStatDescription(int num)
    {
        descripPanel.SetActive(true);
        descripPanel.GetComponent<DescriptionUI>().SetTextInfo((StatText)num);
    }

    public void CloseStatDescription()
    {
        descripPanel.SetActive(false);
    }
}
