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

    private void OnClickButton()
    {
        var manager = DungeonManager.Instance;

        if (manager == null 
            || manager.IsPlayingRoom()                  //게임 중에는 이동 불가
            || manager.CurrentRoomId == roomId          //현재 방에서는 이동 불가
            || manager.getDungeonRoom(roomId).IsClear)  //클리어한 방에서는 이동 불가
            return;
        
        manager.MoveTo(roomId);
    }
}
