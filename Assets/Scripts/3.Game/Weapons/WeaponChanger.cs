using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponChanger : Singleton<WeaponChanger>
{
    [Header("Image")]
    [SerializeField] protected Sprite[] weaponImages;
    [SerializeField] Transform[] weaponTransforms;
    [SerializeField] Image currentItemImage;
    [SerializeField] Image nextItemImage;

    [Header("Sound")]
    [SerializeField] AudioClip changeSound;

    [Header("Other")]
    [SerializeField] GameObject bulletText;

    bool canScroll = true;

    Character character;
    SoundManager soundManager;
    GameManager gameManager;
    WeaponManager weaponManager;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        character = Character.Instance;
        soundManager = SoundManager.Instance;
        gameManager = GameManager.Instance;
        weaponManager = WeaponManager.Instance;

        currentItemImage.sprite = weaponImages[0];
        nextItemImage.sprite = weaponImages[1];

        foreach (Transform weapon in weaponManager.weaponParent)
        {
            weapon.gameObject.SetActive(false);
        }

        weaponManager.weaponParent.GetChild(0).gameObject.SetActive(true);

        bulletText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (character == null)
            character = Character.Instance;

        weaponManager.CurrentIndex = 0;
        character.ChangeAnimationController(weaponManager.CurrentIndex + 2);

        canScroll = true;
        weaponManager.canWeaponChange = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        canScroll = true;
    }

    void Update()
    {
        if (!canScroll || !character.isCanControll || !weaponManager.canWeaponChange || gameManager.isPause)
        {
            return;
        }

        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");

        if (mouseWheel == 0)
            return;

        weaponManager.BeforeIndex = weaponManager.CurrentIndex;

        soundManager.PlaySFX(changeSound);

        if (mouseWheel > 0)
        {
            weaponManager.CurrentIndex = weaponManager.CurrentIndex + 1 >= weaponImages.Length ? 0 : weaponManager.CurrentIndex + 1;
            weaponManager.NextIndex = weaponManager.CurrentIndex + 1 >= weaponImages.Length ? 0 : weaponManager.CurrentIndex + 1;
        }

        else if (mouseWheel < 0)
        {
            weaponManager.CurrentIndex = weaponManager.CurrentIndex - 1 < 0 ? weaponImages.Length - 1 : weaponManager.CurrentIndex - 1;
            weaponManager.NextIndex = weaponManager.BeforeIndex;
        }

        character.ChangeAnimationController(weaponManager.CurrentIndex + 2);

        currentItemImage.sprite = weaponImages[weaponManager.CurrentIndex];
        nextItemImage.sprite = weaponImages[weaponManager.NextIndex];

        weaponManager.weaponParent.GetChild(weaponManager.BeforeIndex).gameObject.SetActive(false);
        weaponManager.weaponParent.GetChild(weaponManager.CurrentIndex).gameObject.SetActive(true);

        if(weaponManager.CurrentIndex == 1)
            bulletText.gameObject.SetActive(true);

        else
            bulletText.gameObject.SetActive(false);

        StartCoroutine(ScrollCoolDown());
    }

    IEnumerator ScrollCoolDown()
    {
        canScroll = false;

        yield return CoroutineCaching.WaitForSeconds(weaponManager.changeDelay);

        canScroll = true;
    }

}
