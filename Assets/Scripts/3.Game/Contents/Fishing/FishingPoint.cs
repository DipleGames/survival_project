using UnityEngine;

public class FishingPoint : MonoBehaviour
{
    [SerializeField] GameObject fishingGame;
    [SerializeField] Sprite fishingTutoImage;

    int round = 0;
    int currentRound = 0;

    GameManager gameManager;
    SoundManager soundManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        currentRound = gameManager.round;

        if (round > currentRound)
            return;

        if (other.CompareTag("Character"))
        {
            GameSceneUI.Instance.ActiveTutoPanel(TutoType.FishingEnterTuto, fishingTutoImage);

            round = currentRound;

            soundManager.PlayBGM(2, true);

            Character.Instance.isCanControll = false;
            fishingGame.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (round != currentRound)
            return;

        if (other.CompareTag("Character"))
        {
            round++;
            soundManager.PlayBGM(1, true);
        }
    }
}
