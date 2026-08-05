using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TamingViewUI : MonoBehaviour
{
    [SerializeField] Slider tamingGauge;
    [SerializeField] float gaugeSpeed = 1f;
    [SerializeField] Text catchText;
    [SerializeField] Text failText;
    [SerializeField] GameObject tamingPet;
    [SerializeField] GameObject tamingMouseUI;
    [SerializeField] AudioClip catchSound;

    [HideInInspector] public bool isGaugeUp = false;

    [HideInInspector] public GameObject catchPet;

    GamesceneManager gamesceneManager;
    Character character;
    GameManager gameManager;

    private void Awake()
    {
        gamesceneManager = GamesceneManager.Instance;
        gameManager = GameManager.Instance;
        character = Character.Instance;
    }

    private void OnEnable()
    {
        tamingGauge.value = 0.5f;

        tamingPet.SetActive(true);
        catchText.gameObject.SetActive(false);
        failText.gameObject.SetActive(false);

        Cursor.visible = false;
        gameManager.isCursorVisible = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if(gamesceneManager.isNight)
        {
            Cursor.visible = true;
            gameManager.isCursorVisible = true;
            gameObject.SetActive(false);
            character.isCanControll = true;
        }

        if (tamingPet.GetComponent<TamingGamePetMove>().isCatch)
            return;

        if (tamingGauge.value < 1)
        {
            if (tamingPet.activeSelf)
            {
#if UNITY_EDITOR
                tamingGauge.value += isGaugeUp ? Time.deltaTime * gaugeSpeed * 3 : -Time.deltaTime * gaugeSpeed * 0.1f;
#else
                tamingGauge.value += isGaugeUp ? Time.deltaTime * gaugeSpeed : -Time.deltaTime * gaugeSpeed * 0.5f;
#endif
            }

            if (tamingGauge.value <= 0)
            {
                tamingPet.SetActive(false);
                failText.gameObject.SetActive(true);

                if (catchPet != null)
                {
                    StartCoroutine(catchPet.GetComponent<IMouseInteraction>().EndInteraction(null, 2));
                }
            }
        }

        else
        {
            tamingPet.GetComponent<TamingGamePetMove>().isCatch = true;
            catchText.gameObject.SetActive(true);

            SoundManager.Instance.PlaySFX(catchSound);

            if (catchPet != null)
            {
                Character.Instance.TamingPet(GameManager.Instance.round);
                StartCoroutine(catchPet.GetComponent<IMouseInteraction>().EndInteraction(null, 3));
            }
        }
    }

    public void ExitGame()
    {
        Cursor.visible = true;
        gameManager.isCursorVisible = true;
        gameObject.SetActive(false);
        character.isCanControll = true;
    }

    public void OverExitButton(bool isEnter)
    {
        Cursor.visible = isEnter;
        gameManager.isCursorVisible = isEnter;
        tamingMouseUI.gameObject.SetActive(!isEnter);
    }
}
