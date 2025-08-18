using UnityEngine;
using UnityEngine.UI;

public class BowCatchBar : MonoBehaviour
{
    [SerializeField] Slider catchBar;
    [SerializeField] AudioClip chargingSound;

    SoundManager soundManager;
    GameManager gameManager;
    Character character;
    WeaponManager weaponManager;

    float barSpeed;
    float maxBarSpeed = 600;

    bool isMin = true;

    bool isCatch = false;

    EffectSound currentSfx = null;

    private void Awake()
    {
        soundManager = SoundManager.Instance;
        gameManager = GameManager.Instance;
        character = Character.Instance;
        weaponManager = WeaponManager.Instance;
    }

    private void OnEnable()
    {
        catchBar.gameObject.SetActive(false);
        catchBar.value = 0;
    }

    private void Update()
    {
        if (gameManager.isPause || character.isDead)
            return;

        if (Input.GetMouseButton(0))
        {
            if (weaponManager.canAttack)
            {
                if (currentSfx == null)
                {
                    //    Debug.Log("charging sound");
                    currentSfx = soundManager.PlaySFXAndReturn(chargingSound, false);
                }

                catchBar.gameObject.SetActive(true);
                MoveBar();
            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            if (currentSfx != null)       // null 에러 발생
            {
                soundManager.StopLoopSFX(currentSfx);
            }

            currentSfx = null;


            Catch();
            catchBar.gameObject.SetActive(false);
            catchBar.value = 0;
        }
    }

    void MoveBar()
    {
        if (catchBar.value == catchBar.minValue)
            isMin = true;

        else if (catchBar.value == catchBar.maxValue)
            isMin = false;

        barSpeed = Mathf.Clamp(catchBar.value <= catchBar.maxValue * 0.5f ? (catchBar.value / (catchBar.maxValue * 0.5f)) * maxBarSpeed : ((catchBar.maxValue - catchBar.value) / (catchBar.maxValue * 0.5f)) * maxBarSpeed, 100, maxBarSpeed);

        catchBar.value += isMin ? Time.deltaTime * barSpeed : -Time.deltaTime * barSpeed;
    }

    void Catch()
    {
        isCatch = catchBar.value >= 77 && catchBar.value <= 123 ? true : false;
    }

    public bool IsCatch()
    {
        return isCatch;
    }
}
