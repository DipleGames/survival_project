using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponChanger : MonoBehaviour
{
    [SerializeField] Sprite[] weaponImages;
    [SerializeField] Image currentItemImage;
    [SerializeField] Image nextItemImage;
    [SerializeField] GameObject bulletText;
    [SerializeField] AudioClip changeSound;

    int beforeIndex = 0;
    int currentIndex = 0;
    int nextIndex = 0;

    bool canScroll = true;

    Character character;
    SoundManager soundManager;
    GameManager gameManager;

    void Awake()
    {
        character = Character.Instance;
        soundManager = SoundManager.Instance;
        gameManager = GameManager.Instance;

        currentItemImage.sprite = weaponImages[0];
        nextItemImage.sprite = weaponImages[1];

        foreach (Transform weapon in character.weaponParent)
        {
            weapon.gameObject.SetActive(false);
        }

        character.weaponParent.GetChild(0).gameObject.SetActive(true);

        bulletText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (character == null)
            character = Character.Instance;

        character.ChangeAnimationController(currentIndex + 2);

        canScroll = true;
        character.canWeaponChange = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        canScroll = true;
    }

    void Update()
    {
        if (!canScroll || !character.isCanControll || !character.canWeaponChange || gameManager.isPause)
        {
            return;
        }

        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        if (mouseWheel == 0)
            return;

        beforeIndex = currentIndex;

        soundManager.PlaySFX(changeSound);

        if (mouseWheel > 0)
        {
            currentIndex = currentIndex + 1 >= weaponImages.Length ? 0 : currentIndex + 1;
            nextIndex = currentIndex + 1 >= weaponImages.Length ? 0 : currentIndex + 1;
        }

        else if (mouseWheel < 0)
        {
            currentIndex = currentIndex - 1 < 0 ? weaponImages.Length - 1 : currentIndex - 1;
            nextIndex = beforeIndex;
        }

        character.ChangeAnimationController(currentIndex + 2);

        currentItemImage.sprite = weaponImages[currentIndex];
        nextItemImage.sprite = weaponImages[nextIndex];

        character.weaponParent.GetChild(beforeIndex).gameObject.SetActive(false);
        character.weaponParent.GetChild(currentIndex).gameObject.SetActive(true);

        if(currentIndex == 1)
            bulletText.gameObject.SetActive(true);

        else
            bulletText.gameObject.SetActive(false);

        character.currentWeaponIndex = currentIndex;

        StartCoroutine(ScrollCoolDown());
    }



    IEnumerator ScrollCoolDown()
    {
        canScroll = false;

        yield return CoroutineCaching.WaitForSeconds(0.5f);

        canScroll = true;
    }
}
