#if UNITY_EDITOR
using System.IO;
using System.Linq;
using MineTest;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class MineTestSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MineTest.unity";
    private const string PrefabFolder = "Assets/Prefabs/Mine";

    [MenuItem("Tools/MineTest/Rebuild MineTest Scene")]
    public static void Build()
    {
        EnsureFolder("Assets/Prefabs", "Mine");
        EnsureTag("jewel");
        EnsureTag("rock");

        TileBase grass = AssetDatabase.LoadAssetAtPath<TileBase>(
            "Assets/UseImage/Tiles/TilePalette/GrassTile_1.asset");
        Sprite miningSprite = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Scripts/3.Game/Contents/Mining/images(png).png")
            .OfType<Sprite>().FirstOrDefault();
        GameObject piratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Character/Pirate.prefab");
        GameObject clickUiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/ClickUI.prefab");

        if (grass == null || miningSprite == null || piratePrefab == null || clickUiPrefab == null)
            throw new System.InvalidOperationException("MineTest에 필요한 에셋을 불러오지 못했습니다.");

        GameObject jewelPrefab = CreateMiningPrefab("Jewel", "jewel", miningSprite, 50, true);
        GameObject rockPrefab = CreateMiningPrefab("Rock", "rock", miningSprite, 25, false);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        Tilemap ground = CreateGround(grass);
        FrameCamera(camera, ground);
        MineTestPlayerController player = CreatePlayer(scene, piratePrefab, ground);
        Text label = CreateCursorLabel();

        var managerObject = new GameObject("MineTest Controller");
        MiningManager manager = managerObject.AddComponent<MiningManager>();
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("worldCamera").objectReferenceValue = camera;
        serialized.FindProperty("player").objectReferenceValue = player;
        serialized.FindProperty("ground").objectReferenceValue = ground;
        serialized.FindProperty("jewelPrefab").objectReferenceValue = jewelPrefab;
        serialized.FindProperty("rockPrefab").objectReferenceValue = rockPrefab;
        serialized.FindProperty("clickUiPrefab").objectReferenceValue = clickUiPrefab;
        serialized.FindProperty("cursorLabel").objectReferenceValue = label;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("MineTest scene rebuilt: " + ScenePath);
    }

    [MenuItem("Tools/MineTest/Validate MineTest Scene")]
    public static void Validate()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Tilemap ground = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None)
            .FirstOrDefault(map => map.name == "ground");
        MiningManager manager = Object.FindFirstObjectByType<MiningManager>();
        MineTestPlayerController player = Object.FindFirstObjectByType<MineTestPlayerController>();
        if (ground == null || manager == null || player == null)
            throw new System.InvalidOperationException("MineTest 필수 오브젝트가 누락되었습니다.");

        int tileCount = 0;
        foreach (Vector3Int cell in ground.cellBounds.allPositionsWithin)
            if (ground.HasTile(cell)) tileCount++;
        if (tileCount != 100)
            throw new System.InvalidOperationException($"ground 타일 수가 100개가 아닙니다: {tileCount}");

        var serialized = new SerializedObject(manager);
        string[] references = { "worldCamera", "player", "ground", "jewelPrefab", "rockPrefab", "clickUiPrefab", "cursorLabel" };
        foreach (string reference in references)
            if (serialized.FindProperty(reference).objectReferenceValue == null)
                throw new System.InvalidOperationException("MineTest 참조가 비어 있습니다: " + reference);

        Debug.Log("MineTest validation passed: 100 ground tiles and all required references are connected.");
    }

    private static GameObject CreateMiningPrefab(
        string name, string tag, Sprite sprite, int health, bool randomColor)
    {
        var instance = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider), typeof(MiningNode));
        instance.tag = tag;
        SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 5;

        BoxCollider collider = instance.GetComponent<BoxCollider>();
        Vector2 spriteSize = sprite.bounds.size;
        // Keep hover/click detection close to the visible mineral instead of its tile footprint.
        collider.size = new Vector3(spriteSize.x * 0.56f, spriteSize.y * 0.56f, 0.35f);

        var node = new SerializedObject(instance.GetComponent<MiningNode>());
        node.FindProperty("maxHealth").intValue = health;
        node.FindProperty("randomJewelColor").boolValue = randomColor;
        node.FindProperty("outlineOffset").floatValue = 0.07f;
        node.FindProperty("occludedAlpha").floatValue = 0.5f;
        node.FindProperty("shadowAlpha").floatValue = 0.25f;
        node.ApplyModifiedPropertiesWithoutUndo();

        string path = $"{PrefabFolder}/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
            throw new System.InvalidOperationException("생성된 Prefab을 다시 불러오지 못했습니다: " + path);
        return prefab;
    }

    private static Tilemap CreateGround(TileBase grass)
    {
        var gridObject = new GameObject("MineTest Grid", typeof(Grid));
        Grid grid = gridObject.GetComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Isometric;
        if (grass is Tile grassTile && grassTile.sprite != null)
        {
            Vector2 size = grassTile.sprite.bounds.size;
            grid.cellSize = new Vector3(size.x, size.y, 1f);
        }

        var groundObject = new GameObject("ground", typeof(Tilemap), typeof(TilemapRenderer));
        groundObject.transform.SetParent(gridObject.transform, false);
        groundObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Tilemap ground = groundObject.GetComponent<Tilemap>();
        groundObject.GetComponent<TilemapRenderer>().sortingOrder = 0;
        var tiles = Enumerable.Repeat(grass, 100).ToArray();
        ground.SetTilesBlock(new BoundsInt(0, 0, 0, 10, 10, 1), tiles);
        ground.CompressBounds();
        ground.RefreshAllTiles();
        EditorUtility.SetDirty(ground);
        return ground;
    }

    private static MineTestPlayerController CreatePlayer(
        UnityEngine.SceneManagement.Scene scene, GameObject piratePrefab, Tilemap ground)
    {
        GameObject pirate = (GameObject)PrefabUtility.InstantiatePrefab(piratePrefab, scene);
        pirate.name = "Pirate (MineTest)";
        pirate.transform.position = ground.GetCellCenterWorld(new Vector3Int(4, 4, 0)) + Vector3.up * 0.1f;

        foreach (MonoBehaviour behaviour in pirate.GetComponentsInChildren<MonoBehaviour>(true).ToArray())
            Object.DestroyImmediate(behaviour, true);
        foreach (Rigidbody body in pirate.GetComponentsInChildren<Rigidbody>(true))
            Object.DestroyImmediate(body, true);
        foreach (Collider collider in pirate.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(collider, true);
        // Keep the Pirate prefab's complete visual and animator hierarchy, as TestScene does.
        // The character's transform scale remains unchanged.

        BoxCollider playerCollider = pirate.AddComponent<BoxCollider>();
        playerCollider.size = new Vector3(0.6f, 1f, 0.6f);
        return pirate.AddComponent<MineTestPlayerController>();
    }

    private static Camera CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 15f, 0f);
        cameraObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        // Match the camera framing currently used by TestScene.
        camera.orthographicSize = 4.5f;
        camera.backgroundColor = new Color(0.11f, 0.16f, 0.22f, 1f);
        return camera;
    }

    private static void FrameCamera(Camera camera, Tilemap ground)
    {
        Vector3 center = ground.transform.TransformPoint(ground.localBounds.center);
        camera.transform.position = new Vector3(center.x, 15f, center.z);
    }

    private static Text CreateCursorLabel()
    {
        var canvasObject = new GameObject("MineTest Cursor UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var labelObject = new GameObject("Mining Action Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvasObject.transform, false);
        Text label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "채광하기";
        label.fontSize = 24;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.rectTransform.sizeDelta = new Vector2(220f, 40f);
        Outline outline = labelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        return label;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = Path.Combine(parent, child).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static void EnsureTag(string tag)
    {
        Object tagManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
        var serialized = new SerializedObject(tagManager);
        SerializedProperty tags = serialized.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        serialized.ApplyModifiedProperties();
    }
}
#endif
