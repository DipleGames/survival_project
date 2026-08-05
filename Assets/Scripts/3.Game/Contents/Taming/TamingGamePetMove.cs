using System.Collections;
using UnityEngine;

enum MoveType
{
    RANDOM,
    DIRECT,
    COUNT,
}

public class TamingGamePetMove : MonoBehaviour
{
    [SerializeField] RectTransform moveArea;
    [SerializeField] ParticleSystem dashParticle;
    [SerializeField] float defaultSpeed = 300f;
    [SerializeField] RectTransform catchUI;

    [HideInInspector] public float moveSpeed;
    float speedRatio = 1;

    RectTransform rectTransform;

    Rect areaRect;

    Vector3 nextPos;

    Vector3 dir;

    bool isTurn = false;
    bool isDash = false;
    public bool isCatch = false;

    MoveType moveType = MoveType.RANDOM;

    [SerializeField] float noiseScale = 0.5f;  // 노이즈 스케일
    [SerializeField] float offsetX, offsetY;  // 노이즈 오프셋

    public float DefalutSpeed => defaultSpeed;

    float scaleX;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dashParticle.GetComponentInChildren<Renderer>().enabled = false;
        areaRect = moveArea.rect;

        scaleX = transform.localScale.x;
    }

    private void OnEnable()
    {
        isCatch = false;
        moveSpeed = defaultSpeed;

        StartCoroutine(GetRandomSpeedRatio());
        StartCoroutine(SetMoveType());
        StartCoroutine(Dash());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if (!isCatch)
        {
            if (!isDash)
            {
                switch (moveType)
                {
                    case MoveType.DIRECT:
                        MoveDirectly();
                        break;

                    case MoveType.RANDOM:
                        MoveRandomDir();
                        break;
                }
            }

            Flip();
        }

        else
        {
            StopAllCoroutines();

            rectTransform.localPosition = catchUI.localPosition;
        }
    }

    void Flip()
    {
        if (dir.x == 0)
            return;

        transform.localScale = new Vector3(dir.x > 0 ? scaleX : -scaleX, transform.localScale.y, transform.localScale.z);
    }

    void MoveRandomDir()
    {
        float x = Mathf.PerlinNoise(Time.time * noiseScale + offsetX, 0f) * 2 - 1;
        float y = Mathf.PerlinNoise(0f, Time.time * noiseScale + offsetY) * 2 - 1;

        dir = new Vector3(x, y, 0f).normalized;

        if (isTurn)
        {
            dir = -dir;
        }

        nextPos = rectTransform.localPosition + dir * Time.deltaTime * moveSpeed * speedRatio;

        if (areaRect.Contains(nextPos))
        {
            rectTransform.localPosition = nextPos;
        }

        else
        {
            isTurn = !isTurn;
        }
    }

    void MoveDirectly()
    {
        nextPos = rectTransform.localPosition + dir * Time.deltaTime * moveSpeed * speedRatio * 1.5f;

        if (areaRect.Contains(nextPos))
            rectTransform.localPosition = nextPos;

        else
            dir = -dir;
    }

    IEnumerator Dash()
    {
        while (true)
        {
            yield return CoroutineCaching.WaitForSeconds(1.5f);

            isDash = true;

            if (scaleX < 0)
                dashParticle.transform.localScale = new Vector3(-1, 1, 1);

            else if (scaleX > 0)
                dashParticle.transform.localScale = new Vector3(1, 1, 1);

            float xPos = Random.Range(areaRect.xMin, areaRect.xMax);
            float yPos = Random.Range(areaRect.yMin, areaRect.yMax);

            dashParticle.GetComponentInChildren<Renderer>().enabled = true;
            
            rectTransform.localPosition = new Vector3(xPos, yPos, 0f);

            Invoke("ParticleOff", 0.2f);

            isDash = false;
        }
    }

    void ParticleOff()
    {
        dashParticle.GetComponentInChildren<Renderer>().enabled = false;
    }

    IEnumerator SetMoveType()
    {
        while (true)
        {
            moveType = (MoveType)Random.Range(0, 2);

            switch (moveType)
            {
                case MoveType.DIRECT:
                    GetRandomDir();
                    break;

                case MoveType.RANDOM:
                    GetRandomOffset();
                    break;
            }

            yield return CoroutineCaching.WaitForSeconds(2f);
        }
    }

    IEnumerator GetRandomSpeedRatio()
    {
        while (true) 
        {
            speedRatio = Random.Range(1f, 2f);

            yield return CoroutineCaching.WaitForSeconds(1.5f);
        }
    }

    void GetRandomDir()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        dir = new Vector3(x, y, 0f).normalized;
    }

    void GetRandomOffset()
    {
        isTurn = false;

        offsetX = Random.Range(100f, 1000f);
        offsetY = Random.Range(100f, 1000f);

        noiseScale = Random.Range(0.5f, 2f);
    }
}
