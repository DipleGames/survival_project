using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    Texture2D cursorNormal;

    private void Start()
    {
        cursorNormal = GameManager.Instance.useCursorNormal;

        Vector2 cursorHotSpot = new Vector3(cursorNormal.width * 0.5f, cursorNormal.height * 0.5f);
        Cursor.SetCursor(cursorNormal, cursorHotSpot, CursorMode.ForceSoftware);

        //SoundManager.Instance.PlayBGM(4, false);
    }
}
