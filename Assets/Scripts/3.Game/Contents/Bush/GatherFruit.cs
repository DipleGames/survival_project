using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherFruit : MonoBehaviour, IMouseInteraction
{
    [SerializeField] Transform[] gatherPoint;
    [SerializeField] int defaultGaugeUpValue;
    [SerializeField] DiabolicItemInfo[] bushPieceList;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite fruitImage;
    [SerializeField] AudioClip gatheringSound;

    bool canGather;

    Character character;
    ItemManager itemManager;
    GameSceneUI gameSceneUI;
    GamesceneManager gamesceneManager;
    SoundManager soundManager;

    List<DiabolicItemInfo> itemList = new List<DiabolicItemInfo>();

    Color outlineColor;

    EffectSound currentSfx;

    bool isGathering = false;

    private void Start()
    {
        canGather = false;
        character = Character.Instance;
        itemManager = ItemManager.Instance;
        gameSceneUI = GameSceneUI.Instance;
        gamesceneManager = GamesceneManager.Instance;
        soundManager = SoundManager.Instance;
        outlineColor = spriteRenderer.material.GetColor("_SolidOutline");
    }

    private void Update()
    {
        if(isGathering && gamesceneManager.isNight)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        isGathering = false;

        character.isCanControll = true;
        character.canFlip = true;

        if (currentSfx != null)
        {
            soundManager.StopLoopSFX(currentSfx);
        }
    }

    void GetRandomPiece()
    {
        itemList.Clear();

        for (int i = 0; i < bushPieceList.Length; ++i)
        {
            if (!itemManager.getItems.ContainsKey(bushPieceList[i]))
            {
                itemList.Add(bushPieceList[i]);
            }

            else
            {
                if (itemManager.getItems[bushPieceList[i]] < bushPieceList[i].MaxCount)
                    itemList.Add(bushPieceList[i]);
            }
        }

        if (itemList.Count <= 0)
        {
            return;
        }

        int totalWeightValue = 0;

        for (int i = 0; i < itemList.Count; ++i)
        {
            totalWeightValue += itemList[i].WeightValue;
        }

        int rand = Random.Range(0, totalWeightValue);

        float total = 0;

        for (int i = 0; i < itemList.Count; i++)
        {
            total += itemList[i].WeightValue;

            if (rand < total)
            {
                rand = i;
                break;
            }
        }

        gameSceneUI.ShowPieceCard(itemList[rand]);
        ItemManager.Instance.AddItem(itemList[rand]);
    }

    public void CanInteraction(bool _canInteraction)
    {
        canGather = _canInteraction;
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (canGather)
        {
            canGather = false;

            int num = (character.transform.position - transform.position).x > 0 ? 0 : 1;

            character.MoveToInteractableObject(gatherPoint[num].position, gameObject, 3, 6, -1, num);
        }
    }

    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        isGathering = true;

        currentSfx = soundManager.PlaySFXAndReturn(gatheringSound, true);

        yield return CoroutineCaching.WaitForSeconds(waitTime);

        if (gamesceneManager.isNight)
            yield break;

        isGathering = false;

        character.isCanControll = true;
        character.canFlip = true;

        if (currentSfx != null)
        {
            soundManager.StopLoopSFX(currentSfx);
        }

#if UNITY_EDITOR
        if (Random.Range(0, 100) >= 0)
            GetRandomPiece();
#else

        if (Random.Range(0, 100) >= 100 - GameManager.Instance.pieceCardGetRate)
            GetRandomPiece();
#endif

        RecoveryGaugeUp();
        
        anim.SetBool("isLogging", false);

        character.ChangeAnimationController(0);

        Destroy(gameObject);
    }

    void RecoveryGaugeUp()
    {
        int getFruitQuantity = Random.Range(1, 5);

        character.getItemUI.GetComponent<GetItemUI>().SetGetItemImage(fruitImage, getFruitQuantity);
        character.getItemUI.gameObject.SetActive(true);

        character.currentRecoveryGauge = Mathf.Clamp(character.currentRecoveryGauge + defaultGaugeUpValue * getFruitQuantity, 0, character.maxRecoveryGauge);
    }


    public void InteractionRightButtonFuc(GameObject hitObject)
    {

    }

    public bool ReturnCanInteraction()
    {
        return canGather;
    }

    private void OnMouseOver()
    {
        if (canGather)
        {
            if (outlineColor.a == 1)
                return;

            outlineColor.a = 1;

            spriteRenderer.material.SetColor("_SolidOutline", outlineColor);
        }

        else
        {
            if (outlineColor.a == 0)
                return;

            outlineColor.a = 0;

            spriteRenderer.material.SetColor("_SolidOutline", outlineColor);
        }
    }

    private void OnMouseExit()
    {
        if (outlineColor.a == 0)
            return;

        outlineColor.a = 0;

        spriteRenderer.material.SetColor("_SolidOutline", outlineColor);
    }

#if UNITY_EDITOR
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2);
    }*/
#endif
}
