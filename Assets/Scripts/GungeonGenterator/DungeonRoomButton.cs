using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonRoomButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txt_RoomId;
    private int roomId;
    private Button button;

    public void SetId(int _id)
    {
        roomId = _id;

        // TMP 텍스트 갱신
        if (txt_RoomId != null)
        {
            txt_RoomId.text = roomId.ToString();
        }

        // 버튼 클릭 이벤트 등록
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClickButton);
    }

    private void OnClickButton(){ DungeonManager.Instance.MoveTo(roomId); }
}
