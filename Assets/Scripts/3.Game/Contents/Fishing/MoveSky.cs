using UnityEngine;
using UnityEngine.UI;

public class MoveSky : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    RawImage skyImage;
    Rect imageRect;

    void Start()
    {
        skyImage = GetComponent<RawImage>();
    }

    void Update()
    {
        imageRect = skyImage.uvRect;
        imageRect.x += moveSpeed * Time.deltaTime;

        skyImage.uvRect = imageRect;
    }
}
