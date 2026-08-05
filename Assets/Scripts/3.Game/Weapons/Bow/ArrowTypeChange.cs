using UnityEngine;

public class ArrowTypeChange : MonoBehaviour
{
    public void ChangeArrowType(bool isCatch)
    {
        transform.GetChild(1).gameObject.SetActive(isCatch);
        transform.GetChild(0).gameObject.SetActive(!isCatch);
    }
}
