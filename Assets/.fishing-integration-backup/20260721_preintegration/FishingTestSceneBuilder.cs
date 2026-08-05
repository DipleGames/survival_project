#if UNITY_EDITOR
using System.Linq;
using FishingTest;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class FishingTestSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/TestScene.unity";

    [MenuItem("Tools/Fishing Test/Rebuild Test Scene")]
    public static void Build()
    {
        EnsureTag("Lake");
        EnsureTag("Sea");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        TileBase grass = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/UseImage/Tiles/TilePalette/GrassTile_0.asset");
        TileBase water = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/UseImage/Tiles/TilePalette/SeaLinkTile_0.asset");
        GameObject piratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Character/Pirate.prefab");
        GameObject clickUiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ClickUI.prefab");
        AudioClip fishRodThrowClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/UseSound/SFX/Fishing/FishingStart/FishRodThrow.wav");
        AudioClip fishingCatchStartClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/UseSound/SFX/Fishing/CatchingStart/FishingCatchStart.wav");
        AudioClip fishingSuccessClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/UseSound/SFX/Fishing/Success/FishingSuccess.wav");
        AudioClip fishingFailClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/UseSound/SFX/Fishing/Fail/FishingFail.wav");

        if (grass == null || water == null || piratePrefab == null || clickUiPrefab == null ||
            fishRodThrowClip == null || fishingCatchStartClip == null ||
            fishingSuccessClip == null || fishingFailClip == null)
            throw new System.InvalidOperationException("Fishing test assets could not be loaded.");

        Camera camera = CreateCamera();
        var grid = new GameObject("Fishing Test Grid", typeof(Grid));
        Tilemap ground = CreateTilemap(grid.transform, "Grass Ground", "Untagged", 0);
        Tilemap lake = CreateTilemap(grid.transform, "Lake", "Lake", 1);
        Tilemap sea = CreateTilemap(grid.transform, "Sea", "Sea", 1);

        Fill(ground, grass, -2, 2, -4, 4);
        Fill(lake, water, -6, -3, -2, 1);
        Fill(sea, water, 3, 6, -2, 1);

        GameObject pirate = (GameObject)PrefabUtility.InstantiatePrefab(piratePrefab, scene);
        pirate.name = "Pirate (Fishing Test)";
        pirate.transform.position = new Vector3(0.5f, 0.1f, -0.5f);

        foreach (MonoBehaviour behaviour in pirate.GetComponentsInChildren<MonoBehaviour>(true).ToArray())
            Object.DestroyImmediate(behaviour, true);
        foreach (UnityEngine.AI.NavMeshAgent agent in pirate.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>(true))
            Object.DestroyImmediate(agent, true);
        foreach (Rigidbody body in pirate.GetComponentsInChildren<Rigidbody>(true))
            Object.DestroyImmediate(body, true);

        foreach (Animator animator in pirate.GetComponentsInChildren<Animator>(true))
            animator.enabled = false;
        foreach (SpriteRenderer renderer in pirate.GetComponentsInChildren<SpriteRenderer>(true))
            renderer.enabled = false;

        Sprite idleSprite = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/UseImage/Pirate/CharacterImage/PC_idle.PNG")
            .OfType<Sprite>()
            .First(sprite => sprite.name == "PC_idle_0");
        var visualObject = new GameObject("Fishing Test Opaque Visual", typeof(SpriteRenderer));
        visualObject.transform.SetParent(pirate.transform, false);
        visualObject.transform.localPosition = -idleSprite.bounds.center;
        visualObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        SpriteRenderer visualRenderer = visualObject.GetComponent<SpriteRenderer>();
        visualRenderer.sprite = idleSprite;
        visualRenderer.color = Color.white;
        visualRenderer.sortingOrder = 10;

        FishingTestPlayerController player = pirate.AddComponent<FishingTestPlayerController>();
        Text label = CreateCursorLabel();
        var managerObject = new GameObject("Fishing Test Controller");
        FishingTestController manager = managerObject.AddComponent<FishingTestController>();
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("worldCamera").objectReferenceValue = camera;
        serialized.FindProperty("player").objectReferenceValue = player;
        serialized.FindProperty("lakeTilemap").objectReferenceValue = lake;
        serialized.FindProperty("seaTilemap").objectReferenceValue = sea;
        serialized.FindProperty("cursorLabel").objectReferenceValue = label;
        serialized.FindProperty("clickUiPrefab").objectReferenceValue = clickUiPrefab;
        serialized.FindProperty("fishRodThrowClip").objectReferenceValue = fishRodThrowClip;
        serialized.FindProperty("fishingCatchStartClip").objectReferenceValue = fishingCatchStartClip;
        serialized.FindProperty("fishingSuccessClip").objectReferenceValue = fishingSuccessClip;
        serialized.FindProperty("fishingFailClip").objectReferenceValue = fishingFailClip;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Fishing test scene rebuilt: " + ScenePath);
    }

    private static Camera CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0.5f, 10f, -0.5f);
        cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.backgroundColor = new Color(0.11f, 0.16f, 0.22f, 1f);
        return camera;
    }

    private static Tilemap CreateTilemap(Transform parent, string name, string tag, int sortingOrder)
    {
        var child = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        child.transform.SetParent(parent, false);
        child.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        child.tag = tag;
        child.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;
        return child.GetComponent<Tilemap>();
    }

    private static void Fill(Tilemap map, TileBase tile, int minX, int maxX, int minY, int maxY)
    {
        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
            map.SetTile(new Vector3Int(x, y, 0), tile);
    }

    private static Text CreateCursorLabel()
    {
        var canvasObject = new GameObject("Fishing Cursor UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var labelObject = new GameObject("Action Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvasObject.transform, false);
        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.rectTransform.sizeDelta = new Vector2(220f, 40f);
        var outline = labelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        return label;
    }

    private static void EnsureTag(string tag)
    {
        Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        var serialized = new SerializedObject(tagManager);
        SerializedProperty tags = serialized.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        serialized.ApplyModifiedProperties();
    }
}
#endif
