using UnityEngine;

public class ThrowObjecMovement : ParabolaLineRenderer
{
    [SerializeField] float moveSpeed;
    [SerializeField] float totalThrowLength;
    [SerializeField] bool flyingHit;
    [SerializeField] bool scaleChange;
    [SerializeField] bool isRemain;

    float currentThrowLength = 0;

    Vector3 initScale;

    bool isArrive;

    private void Start()
    {
        character = Character.Instance;
        initScale = transform.localScale;
        GetComponent<SphereCollider>().enabled = flyingHit;
        isArrive = false;
    }

    private void Update()
    {
        if (!isArrive)
        {
            Throw();
            ScaleChange();
        }
    }

    float beforeDelta = 3;

    void Throw()
    {
        float halfTotal = totalThrowLength / 2;

        if (currentThrowLength < halfTotal)
            beforeDelta = Mathf.Clamp(beforeDelta - Time.deltaTime * 5, 1.5f, 3);

        else
            beforeDelta = Mathf.Clamp(beforeDelta + Time.deltaTime * 1, 1.5f, 3);

        currentThrowLength += (Time.deltaTime * beforeDelta * moveSpeed);

        transform.position = ParabolicPos(startPos, endPos, currentThrowLength, totalThrowLength);

        if (currentThrowLength >= totalThrowLength)
        {
            isArrive = true;
            transform.position = endPos;

            if (!isRemain)
                return;

            GetComponent<ThrowObject>().Grounded(isArrive);
            GetComponent<SphereCollider>().enabled = true;
        }
    }

    public void SetStartEndPoint(Vector3 _startPos, Vector3 _endPos)
    {
        startPos = _startPos;
        endPos = _endPos;
        if (GetComponent<MonsterBullet>().arrivePointMark != null)
        {
            GetComponent<MonsterBullet>().arrivePointMark.transform.position = endPos;
            GetComponent<MonsterBullet>().arrivePointMark.SetActive(true);
        }
        currentThrowLength = 0;
        isArrive = false;
    }

    void ScaleChange()
    {
        if (!scaleChange)
            return;

        float change = Mathf.Sin(currentThrowLength / totalThrowLength * Mathf.PI);
        transform.localScale = initScale + new Vector3(change, change, 0);
    }

    public void Fire(Vector3 startPos)
    {
        GetClickPos(startPos);
        currentThrowLength = 0;
        isArrive = false;
        GetComponent<SphereCollider>().enabled = false;
    }

    public bool CheckIsArrive()
    {
        return isArrive;
    }
}