using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LogTree : MonoBehaviour, IMouseInteraction
{
    [SerializeField] GameObject obstacle;
    [SerializeField] Transform[] logPoses;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] GameObject arrow;
    [SerializeField] Sprite woodSprite;
    [SerializeField] AudioClip loggingSound;
    [SerializeField] AudioClip instanceObstacleSound;

    bool canLog = false;
    [HideInInspector] int posNum;

    Player player;
    GameManager gameManager;
    SoundManager soundManager;

    Vector3 obstacleAngle;

    Color outlineColor;

    EffectSound currentSfx;

    bool isLogging = false;

    private void Start()
    {
        canLog = false;
        gameManager = GameManager.Instance;
        player = gameManager.Player;
        soundManager = SoundManager.Instance;

        outlineColor = spriteRenderer.material.GetColor("_SolidOutline");
    }

    private void Update()
    {
        
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (currentSfx != null)
        {
            soundManager.StopLoopSFX(currentSfx);
        }

        isLogging = false;

    }

    Vector3 LogAngle()
    {
        Vector3 logDir = (player.transform.position - transform.position).normalized;

        float angle = Mathf.Atan2(logDir.z, logDir.x) * Mathf.Rad2Deg;

        if (angle >= 0 && angle < 90)
        {
            posNum = 3;
            angle = 180;
        }

        else if (angle >= 90 && angle < 180)
        {
            posNum = 0;
            angle = -90;
        }

        else if (angle >= -90 && angle < 0)
        {
            posNum = 1;
            angle = 90;
        }

        else if (angle >= -180 && angle < -90)
        {
            posNum = 2;
            angle = 0;
        }

        return new Vector3(90, 0, angle);
    }

    public void InteractionLeftButtonFuc(GameObject hitObject)
    {
        if (canLog)
        {
            arrow.SetActive(false);
            obstacleAngle = LogAngle();
            canLog = false;

            if(posNum == 0)
            {
                Color color = spriteRenderer.color;
                color.a = 0.4f;
                spriteRenderer.color = color;
            }
        }
    }


    public void CanInteraction(bool _canInteraction)
    {
        canLog = _canInteraction;
    }

    public IEnumerator EndInteraction(Animator anim, float waitTime)
    {
        yield return null;
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2);
    }*/

    public void InteractionRightButtonFuc(GameObject hitObject)
    {
        
    }

    public bool ReturnCanInteraction()
    {
        return canLog;
    }

    private void OnMouseOver()
    {
        if (canLog)
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
}
