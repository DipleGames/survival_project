using Unity.VisualScripting;
using UnityEngine;

public class TamingMouseUI : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform moveArea;
    [SerializeField] RectTransform pet;
    [SerializeField] GameObject UIView;
    [SerializeField] AudioClip overlapSound;

    SoundManager soundManager;

    RectTransform rectTransform;

    EffectSound currentSfx = null;

    void Start()
    {
        soundManager = SoundManager.Instance;

        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        MoveFollowMouse();
        UIView.GetComponent<TamingViewUI>().isGaugeUp = IsOverlapping(rectTransform, pet);
    }

    void MoveFollowMouse()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, Camera.main, out Vector2 mousePos);

        rectTransform.localPosition = mousePos;
    }

    private bool IsOverlapping(RectTransform rect1, RectTransform rect2)
    {
        // Rect: x(xMin),y(yMin) 위치와 넓이, 높이에 의해 정의되는 2차원 사각형 영역
        // https://docs.unity3d.com/ScriptReference/Rect.html

        Rect rectUI = new(rect1.localPosition.x - rect1.rect.width / 2,
                        rect1.localPosition.y - rect1.rect.height / 2,
                        rect1.rect.width, rect1.rect.height);

        Rect rectPet = new(rect2.localPosition.x - rect2.rect.width / 2,
                         rect2.localPosition.y - rect2.rect.height / 2,
                         rect2.rect.width, rect2.rect.height);

        bool isOverlap = rectUI.Overlaps(rectPet);

        rect2.GetComponent<TamingGamePetMove>().moveSpeed = isOverlap ? rect2.GetComponent<TamingGamePetMove>().DefalutSpeed + 100f : rect2.GetComponent<TamingGamePetMove>().DefalutSpeed;

        if(currentSfx == null)
        {
            currentSfx = soundManager.PlaySFXAndReturn(overlapSound, false);
        }

        else
        {
            if (!currentSfx.source.isPlaying)
            {
                currentSfx = null;
            }

            else
            {
                if (rect2.GetComponent<TamingGamePetMove>().isCatch)
                {
                    soundManager.StopLoopSFX(currentSfx);
                }
            }
        }

        return isOverlap;
    }
}
