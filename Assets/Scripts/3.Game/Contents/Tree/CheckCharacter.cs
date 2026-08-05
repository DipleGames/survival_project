using UnityEngine;

public class CheckCharacter : MonoBehaviour
{
    [SerializeField] GameObject arrow;
    [SerializeField] GameObject clickUI;
    [SerializeField] Collider coll;
    [SerializeField] TutoType tutoType;
    [SerializeField] TutoType nextTuto;
    [SerializeField] Sprite tutoImage;

    GamesceneManager gamesceneManager;

    public GameObject needItemImage = null;

    private void Awake()
    {
        gamesceneManager = GamesceneManager.Instance;
    }

    private void Start()
    {
        if (arrow != null)
        {
            arrow.gameObject.SetActive(false);
        }

        if (needItemImage != null)
        {
            needItemImage.SetActive(false);
        }

        clickUI.gameObject.SetActive(false);

    }

    private void Update()
    {
        if (gamesceneManager == null)
            gamesceneManager = GamesceneManager.Instance;

        if (!gamesceneManager.CanCharacterContact)
        {
            if (coll.enabled)
            {
                coll.enabled = false;
            }

            if (arrow != null)
            {
                arrow.gameObject.SetActive(false);
            }

            if (needItemImage != null)
            {
                needItemImage.SetActive(false);
            }

            clickUI.SetActive(false);
        }

        else
        {
            if (!coll.enabled)
                coll.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gamesceneManager.CanCharacterContact)
        {
            return;
        }

        if (other.CompareTag("Character"))
        {
            GameSceneUI.Instance.ActiveTutoPanel(tutoType, tutoImage, nextTuto);

            if (arrow != null)
            {
                arrow.SetActive(true);
                arrow.GetComponent<SpriteRenderer>().color = Color.blue;
            }

            if(needItemImage != null)
            {
                needItemImage.SetActive(true);
            }

            clickUI.SetActive(true);
            transform.parent.GetComponent<IMouseInteraction>().CanInteraction(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!gamesceneManager.CanCharacterContact)
        {
            return;
        }

        if (other.CompareTag("Character"))
        {
            if (arrow != null)
            {
                Vector3 logDir = -(Character.Instance.transform.position - transform.parent.position).normalized;

                //if (transform.parent.GetComponent<IMouseInteraction>().ReturnCanInteraction())
                ArrowRatateToDir(logDir);
            }

            clickUI.SetActive(transform.parent.GetComponent<IMouseInteraction>().ReturnCanInteraction());

            if (needItemImage != null)
            {
                needItemImage.SetActive(clickUI.activeSelf);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!gamesceneManager.CanCharacterContact)
            return;

        if (other.CompareTag("Character"))
        {
            if (arrow != null)
            {
                arrow.SetActive(false);
            }

            if (needItemImage != null)
            {
                needItemImage.SetActive(false);
            }

            clickUI.SetActive(false);
            transform.parent.GetComponent<IMouseInteraction>().CanInteraction(false);
        }
    }

    void ArrowRatateToDir(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

        if (angle >= 0 && angle < 90)
            angle = 0;

        else if (angle >= 90 && angle < 180)
            angle = 90;

        else if (angle >= -90 && angle < 0)
            angle = -90;

        else if (angle >= -180 && angle < -90)
            angle = -180;

        arrow.transform.rotation = Quaternion.Euler(90, -angle, 0);
    }
}
