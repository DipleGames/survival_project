using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class Play_StartTitle
{
    [SerializeField] static bool use = true;
    static string startScenePath = "Assets/Scenes/StartTitle.unity";

    static Play_StartTitle()
    {
        if (!use) return;
        EditorApplication.playModeStateChanged += ChangePlayMode;
    }

    private static void ChangePlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (EditorSceneManager.GetActiveScene().path != startScenePath)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(startScenePath);
            }
        }
    }
}
