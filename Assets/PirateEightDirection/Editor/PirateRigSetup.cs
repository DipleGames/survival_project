using System.IO;
using PirateEightDirection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PirateEightDirection.Editor
{
    public static class PirateRigSetup
    {
        private const string Root = "Assets/PirateEightDirection";
        private const string TestScenePath = Root + "/Test/PirateEightDirectionTest.unity";

        [InitializeOnLoadMethod]
        private static void CreateTestSceneWhenMissing()
        {
            EditorApplication.delayCall += TryCreateMissingTestScene;
        }

        private static bool IsEditorBusy =>
            EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating;

        private static void TryCreateMissingTestScene()
        {
            if (File.Exists(TestScenePath)) return;
            if (IsEditorBusy)
            {
                // Play 모드에서는 NewScene을 쓸 수 없으므로 Edit 모드로 돌아온 뒤 다시 시도한다.
                EditorApplication.playModeStateChanged -= RetryAfterPlayMode;
                EditorApplication.playModeStateChanged += RetryAfterPlayMode;
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.delayCall += TryCreateMissingTestScene;
                return;
            }
            CreateTestScene(false);
        }

        private static void RetryAfterPlayMode(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= RetryAfterPlayMode;
            EditorApplication.delayCall += TryCreateMissingTestScene;
        }

        private static bool WarnIfPlaying()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return false;
            EditorUtility.DisplayDialog("Pirate Eight Direction", "Play 모드를 종료한 뒤 다시 실행하세요.", "OK");
            return true;
        }

        [MenuItem("Tools/Pirate Eight Direction/Prepare Textures and Prefab")]
        public static void Prepare()
        {
            ConfigureTextures();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            string generated = Root + "/Generated";
            if (!AssetDatabase.IsValidFolder(generated)) AssetDatabase.CreateFolder(Root, "Generated");
            var instance = new GameObject("Pirate Eight Direction Rig");
            instance.AddComponent<PirateEightDirectionRig>();
            PrefabUtility.SaveAsPrefabAsset(instance, generated + "/PirateEightDirectionRig.prefab");
            Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(generated + "/PirateEightDirectionRig.prefab");
            Debug.Log("Pirate rig prepared. Drag the selected prefab under a player that has a Rigidbody2D.");
        }

        [MenuItem("Tools/Pirate Eight Direction/Create or Rebuild Test Scene")]
        public static void RebuildTestScene()
        {
            if (WarnIfPlaying()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CreateTestScene(true);
        }

        [MenuItem("Tools/Pirate Eight Direction/Open Test Scene")]
        public static void OpenTestScene()
        {
            if (WarnIfPlaying()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(TestScenePath)) CreateTestScene(false);
            EditorSceneManager.OpenScene(TestScenePath, OpenSceneMode.Single);
        }

        private static void CreateTestScene(bool openAfterCreation)
        {
            string testFolder = Root + "/Test";
            if (!AssetDatabase.IsValidFolder(testFolder)) AssetDatabase.CreateFolder(Root, "Test");

            string prefabPath = Root + "/Generated/PirateEightDirectionRig.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Prepare();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = "PirateEightDirectionTest";

            var cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            camera.clearFlags = CameraClearFlags.SolidColor;

            var player = new GameObject("Pirate Rig Test Player");
            SceneManager.MoveGameObjectToScene(player, scene);
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            player.AddComponent<PirateRigTestMover>();

            var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            rig.name = "Pirate Eight Direction Rig";
            rig.transform.SetParent(player.transform, false);
            rig.transform.localPosition = Vector3.zero;
            rig.transform.localScale = Vector3.one;

            EditorSceneManager.SaveScene(scene, TestScenePath);
            if (openAfterCreation)
                EditorSceneManager.OpenScene(TestScenePath, OpenSceneMode.Single);
            else
                EditorSceneManager.CloseScene(scene, true);

            AssetDatabase.SaveAssets();
            Debug.Log("Pirate test scene created: " + TestScenePath);
        }

        private static void ConfigureTextures()
        {
            string absolute = Path.Combine(Application.dataPath, "PirateEightDirection/Resources/PirateRig");
            foreach (string file in Directory.GetFiles(absolute, "*.png"))
            {
                string assetPath = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
