using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceCard : MonoBehaviour
{
    [SerializeField] Image statPanelImage;
    [SerializeField] Image descripPanelImage;
    [SerializeField] Image itemImage;
    [SerializeField] Text itemName;
    [SerializeField] Text maxCount;
    [SerializeField] GameObject[] descriptPrefabs;
    [SerializeField] Text descriptText;
    [SerializeField] Color[] gradeColors;
    [SerializeField] Transform itemShapeViewParent;
    [SerializeField] Color[] shapeColors;
    
    DiabolicItemInfo item;

    public static Color[] GradeColors;

    private void Awake()
    {
        GradeColors = gradeColors;
#if UNITY_EDITOR

#else
        gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        UpdateCardUI(item);
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void UpdateCardUI(DiabolicItemInfo item)
    {
        ItemManager itemManager = ItemManager.Instance;

        int itemQuantity = 1;

        if (itemManager.getItems.ContainsKey(item))
        {
            itemQuantity += itemManager.getItems[item];
        }

        statPanelImage.color = gradeColors[(item.MaxCount - 1) - (itemQuantity - 1)];
        descripPanelImage.color = statPanelImage.color;
        itemImage.sprite = item.ItemSprite;
        itemName.text = item.ItemName;
        maxCount.text = item.MaxCount.ToString();
        DescriptionInfo(item, itemQuantity);

        ItemShape(item);
    }

    void ItemShape(DiabolicItemInfo item)
    {
        itemShapeViewParent.GetComponent<GridLayoutGroup>().constraintCount = item.ItemShape.Width;

        for (int i = 0; i < 3; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                if (i < item.ItemShape.Height && j < item.ItemShape.Width)
                {
                    itemShapeViewParent.GetChild(i * 3 + j).GetComponent<Image>().color = item.ItemShape.Shape[i * item.ItemShape.Width + j] ? shapeColors[1] : shapeColors[0];
                    itemShapeViewParent.GetChild(i * 3 + j).gameObject.SetActive(true);
                }

                else
                {
                    itemShapeViewParent.GetChild(i * 3 + j).gameObject.SetActive(false);
                }
            }
        }
    }

    void DescriptionInfo(DiabolicItemInfo item, int itemQuantity)
    {
        int max = descriptPrefabs.Length;
        int count = 0;
        Dictionary<Status, int> itemStatus = item.Stat();

        for (int i = 0; i < itemStatus.Count; i++)
        {
            if (itemStatus[(Status)i] > 0)
            {
                descriptPrefabs[count].transform.GetChild(0).GetComponent<Text>().text = $"{GameManager.Instance.statNames[i]}   :   <color=#60B015>+{itemStatus[(Status)i] * itemQuantity}</color>";

                /*if (gameManager.status[(Status)i] < 0)
                {
                    descriptPrefabs[count].transform.GetChild(2).GetComponent<Text>().color = Color.red;
                    descriptPrefabs[count].transform.GetChild(2).GetComponent<Text>().text = stats[i].ToString();
                }*/

                descriptPrefabs[count].gameObject.SetActive(true);
                descriptPrefabs[count].transform.GetChild(0).gameObject.SetActive(true);
                descriptPrefabs[count].transform.GetChild(1).gameObject.SetActive(false);

                count++;
            }
        }

        for (int i = max - 1; i >= count; i--)
        {
            if (i == count)
            {
                descriptPrefabs[count].transform.GetChild(1).GetComponent<Text>().text = item.SpecialStatInfo;

                descriptPrefabs[count].gameObject.SetActive(true);
                descriptPrefabs[count].transform.GetChild(0).gameObject.SetActive(false);
                descriptPrefabs[count].transform.GetChild(1).gameObject.SetActive(true);
            }

            else
                descriptPrefabs[i].gameObject.SetActive(false);
        }

        descriptText.text = item.Description;
    }

    public void GetRandomItem(DiabolicItemInfo itemInfo)
    {
        item = itemInfo;
    }

    public void OnSelect()
    {
        ItemManager.Instance.AddItem(item);
        transform.parent.gameObject.SetActive(false);
    }
}