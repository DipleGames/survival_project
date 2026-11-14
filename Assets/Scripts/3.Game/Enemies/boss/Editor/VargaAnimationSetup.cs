using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// 명시적으로 실행하는 에셋 설정 도구. 기존 Controller와 프레임 분할을 재사용한다.
public static class VargaAnimationSetup
{
    private const string Art = "Assets/UseImage/BossImage/Varga/";
    private const string Folder = "Assets/Animation/NYS_Boss/Varga";
    private const string ControllerPath = "Assets/Animation/NYS_Boss/Varga.controller";
    private const string PrefabPath = "Assets/Prefabs/Enemy/Boss/Boss_Prototype.prefab";
    private const string SpeedParameter = "VargaChargeSpeed";

    [MenuItem("Tools/Boss/Varga/Configure Idle and Charge Animations")]
    [MenuItem("Tools/Boss/Varga/Configure Idle Walk and Charge Animations")]
    public static void Configure()
    {
        Sprite[] idleFrames = LoadFrames(Art + "Idie.png");
        Sprite[] chargeFrames = LoadFrames(Art + "ChargeAttack.png");
        Sprite[] walkFrames = LoadFrames(Art + "Walk.png");
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null || controller.layers.Length == 0)
            throw new InvalidOperationException("기존 Varga Controller를 찾을 수 없습니다.");

        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/Animation/NYS_Boss", "Varga");

        var parameter = controller.parameters.FirstOrDefault(p => p.name == SpeedParameter);
        if (parameter != null && parameter.type != AnimatorControllerParameterType.Float)
            throw new InvalidOperationException(SpeedParameter + " 파라미터가 Float가 아닙니다.");

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = FindOrCreate(machine, "Idle", new Vector3(260, 100));
        AnimatorState charge = FindOrCreate(machine, "ChargeAttack", new Vector3(500, 100));
        AnimatorState walk = FindOrCreate(machine, "Walk", new Vector3(260, 240));
        // 다른 상태, 파라미터, 전환은 삭제하지 않는다.
        AnimationClip idleClip = ConfigureClip(idle, Folder + "/Varga_Idle.anim", idleFrames, true);
        AnimationClip chargeClip = ConfigureClip(charge, Folder + "/Varga_ChargeAttack.anim", chargeFrames, false);
        AnimationClip walkClip = ConfigureClip(walk, Folder + "/Varga_Walk.anim", walkFrames, true);
        walk.motion = walkClip;
        walk.speed = 1f;
        walk.speedParameterActive = false;
        idle.motion = idleClip;
        idle.speed = 1f;
        idle.speedParameterActive = false;
        charge.motion = chargeClip;
        charge.speed = 1f;
        charge.speedParameter = SpeedParameter;
        charge.speedParameterActive = true;
        machine.defaultState = idle;
        if (parameter == null)
            controller.AddParameter(new AnimatorControllerParameter {
                name = SpeedParameter,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            });

        // 전환은 생성하지 않는다. ChargePattern이 시작/종료 시 Animator.Play로 제어한다.
        EditorUtility.SetDirty(idle);
        EditorUtility.SetDirty(charge);
        EditorUtility.SetDirty(walk);
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            BossTelegraphMotion motion = root.GetComponent<BossTelegraphMotion>();
            var motionData = motion != null ? new SerializedObject(motion) : null;
            Transform visual = motionData != null
                ? motionData.FindProperty("visual").objectReferenceValue as Transform : null;
            SpriteRenderer renderer = visual != null ? visual.GetComponentInChildren<SpriteRenderer>(true) : null;
            if (renderer == null)
                throw new InvalidOperationException("전조 모션의 Visual에서 SpriteRenderer를 찾을 수 없습니다.");

            // SpriteRenderer와 동일 오브젝트에 Animator를 둬 클립 binding path를 빈 문자열로 유지한다.
            Animator animator = renderer.GetComponent<Animator>();
            if (animator == null)
                animator = renderer.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            renderer.sprite = idleFrames[0];

            BossController boss = root.GetComponent<BossController>();
            var bossData = new SerializedObject(boss);
            bossData.FindProperty("bossAnimator").objectReferenceValue = animator;
            bossData.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        // 모든 기존 Charge SO에 동일한 동기화용 클립을 연결한다. 공격 설정은 유지한다.
        foreach (string guid in AssetDatabase.FindAssets("t:ChargePattern",
            new[] { "Assets/Scripts/3.Game/Enemies/boss" }))
        {
            var pattern = AssetDatabase.LoadAssetAtPath<ChargePattern>(AssetDatabase.GUIDToAssetPath(guid));
            var data = new SerializedObject(pattern);
            data.FindProperty("chargeAnimationClip").objectReferenceValue = chargeClip;
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pattern);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Varga 설정 완료: Idle/Walk/Charge 각 6프레임, 12FPS. " +
            "Charge는 clip.length / chargeDuration 배율로 재생. BossTest에서 두 번 이상 돌진을 확인하세요.");
    }

    private static Sprite[] LoadFrames(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != SpriteImportMode.Multiple)
            throw new InvalidOperationException(path + ": 기존 6프레임 Sprite 시트 분할을 확인하세요.");

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderBy(s => FrameNumber(s.name)).ToArray();
        if (sprites.Length != 6 || !sprites.Select(s => FrameNumber(s.name)).SequenceEqual(Enumerable.Range(0, 6)))
            throw new InvalidOperationException(path + ": 0~5 프레임 순서가 명확하지 않아 중단했습니다.");
        // 특정 PPU/중앙 Pivot을 강제하지 않고 실제 임포트된 첫 프레임을 기준으로 검사한다.
        // 원본 PNG와 플랫폼별 임포트 크기가 다를 수 있으므로 Pivot은 정규화해서 비교한다.
        Sprite first = sprites[0];
        Vector2 referencePivot = NormalizedPivot(first);
        if (sprites.Any(s => !Mathf.Approximately(s.pixelsPerUnit, first.pixelsPerUnit) ||
            Vector2.Distance(NormalizedPivot(s), referencePivot) > 0.001f))
            throw new InvalidOperationException(path + ": 프레임 간 PPU 또는 Pivot이 서로 다릅니다.");
        return sprites;
    }

    private static Vector2 NormalizedPivot(Sprite sprite)
    {
        return new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height
        );
    }

    private static int FrameNumber(string name)
    {
        Match match = Regex.Match(name, @"_(\d+)$");
        if (!match.Success)
            throw new InvalidOperationException("프레임 번호가 없는 Sprite: " + name);
        return int.Parse(match.Groups[1].Value);
    }

    private static AnimatorState FindOrCreate(AnimatorStateMachine machine, string name, Vector3 position)
    {
        AnimatorState[] matches = machine.states.Select(s => s.state).Where(s => s.name == name).ToArray();
        if (matches.Length > 1)
            throw new InvalidOperationException("중복 상태를 확인하세요: " + name);
        return matches.Length == 1 ? matches[0] : machine.AddState(name, position);
    }

    private static AnimationClip ConfigureClip(AnimatorState state, string path, Sprite[] sprites, bool loop)
    {
        AnimationClip clip = state.motion as AnimationClip;
        if (state.motion != null && clip == null)
            throw new InvalidOperationException(state.name + "에 기존 BlendTree가 있어 자동 변경하지 않습니다.");
        if (clip == null)
            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        if (AnimationUtility.GetCurveBindings(clip).Length != 0 ||
            AnimationUtility.GetObjectReferenceCurveBindings(clip).Any(b =>
                b.type != typeof(SpriteRenderer) || b.propertyName != "m_Sprite" || b.path != ""))
            throw new InvalidOperationException(clip.name + ": Sprite 이외의 기존 곡선이 있어 변경을 중단했습니다.");

        clip.frameRate = 12f;
        var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / 12f, value = sprites[i] };
        // 마지막 프레임도 1/12초 유지해 총 길이를 정확히 6/12초로 만든다.
        keys[sprites.Length] = new ObjectReferenceKeyframe {
            time = sprites.Length / 12f, value = loop ? sprites[0] : sprites[sprites.Length - 1]
        };
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding {
            path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite"
        }, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }
}
