using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private Texture2D[] normalCursorTextures;

    private void Start()
    {
        SetNormalCursor();
    }

    /// <summary>
    /// 저장된 커서 크기를 불러와 일반 커서를 적용한다.
    /// </summary>
    private void SetNormalCursor()
    {
        int cursorSize = PlayerPrefs.GetInt("CursorSize", 0);

        if (normalCursorTextures == null || cursorSize < 0 || cursorSize >= normalCursorTextures.Length)
            return;

        Texture2D cursorNormal = normalCursorTextures[cursorSize];

        if (cursorNormal == null)
            return;

        Vector2 cursorHotSpot = new Vector2(cursorNormal.width * 0.5f, cursorNormal.height * 0.5f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Cursor.SetCursor(cursorNormal, cursorHotSpot, CursorMode.ForceSoftware);
    }
}