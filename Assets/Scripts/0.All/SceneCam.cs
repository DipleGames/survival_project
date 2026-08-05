using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCam : MonoBehaviour
{
    GameManager gameManager;
    Character character;

    Vector3 camPos;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager.currentScene == "Game")
            character = Character.Instance;
    }

    private void LateUpdate()
    {
        if (character != null && !character.isDead)
        {
            camPos = character.transform.position;
            camPos.y = transform.position.y;

            transform.position = camPos;
            //transform.position = new Vector3(Mathf.Clamp(lilpa.transform.position.x, -24f, 27f), transform.position.y, Mathf.Clamp(lilpa.transform.position.z, -57f, -23f));
            //transform.position = new Vector3(Mathf.Clamp(lilpa.transform.position.x, -14f, 17f), transform.position.y, Mathf.Clamp(lilpa.transform.position.z, -49f, -31f));
            //transform.position = Vector3.Lerp(transform.position, new Vector3(character.transform.position.x, transform.position.y, character.transform.position.z), 5*Time.deltaTime);
        }
    }

    /*private void Update()
    {
        if (lilpa != null && !lilpa.isDead)
        {
            transform.position = new Vector3(Mathf.Clamp(lilpa.transform.position.x, -14f, 17f), transform.position.y, Mathf.Clamp(lilpa.transform.position.z, -49f, -31f));
            //transform.position = Vector3.Lerp(transform.position, new Vector3(character.transform.position.x, transform.position.y, character.transform.position.z), 5*Time.deltaTime);
        }

        else if(loggingLilpa != null)
            transform.position = new Vector3(Mathf.Clamp(loggingLilpa.transform.position.x, -14f, 17f), transform.position.y, Mathf.Clamp(loggingLilpa.transform.position.z, -9f, 9f));
    }*/
}
