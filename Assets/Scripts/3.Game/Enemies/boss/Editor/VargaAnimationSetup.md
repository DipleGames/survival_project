# Varga animation setup

## Apply in Unity 2021.3.8f1

1. Open the project in a licensed Unity editor and wait for scripts to compile.
2. Outside Play mode, run **Tools > Boss > Varga > Configure Idle and Charge Animations**.
3. Confirm the Console message beginning `Varga 설정 완료`.
4. Open `Assets/Scenes/Test_Scene/BossTest.unity` and enter Play mode.

The menu reuses the existing `Assets/Animation/NYS_Boss/Varga.controller`.
It creates or reuses `Varga_Idle.anim` and `Varga_ChargeAttack.anim` under
`Assets/Animation/NYS_Boss/Varga/`, with states `Idle` and `ChargeAttack`.
Idle is the default. No transitions or Any State transitions are added;
`ChargePattern` explicitly plays each state at the appropriate time.
Existing unrelated states and parameters are preserved.

The setup attaches an Animator to the existing boss Sprite child, disables
Apply Root Motion, assigns the existing controller, connects BossController's
bossAnimator, and assigns the charge clip on existing ChargePattern assets.
No manual Inspector wiring is needed after successful setup.
The Sprite child rotation and scale, Rigidbody, damage, cooldown, movement,
warning VFX and telegraph motion settings are preserved.

## Source frames

- Idle: `Assets/UseImage/BossImage/Varga/Idie.png`, existing sub-sprites
  `bff0c572-0a60-4ebd-94e6-f4ad7edb8fd9_0` through `_5`, in numeric order.
- Charge: `Assets/UseImage/BossImage/Varga/ChargeAttack.png`, existing
  sub-sprites `ChargeAttack_0` through `_5`, in numeric order.
- Both are six-frame horizontal sheets, sliced left to right, with existing
  362 x 724 pixel rects, PPU 100 and centered pivots.
- Existing Multiple import mode is required for these sheets. Changing to
  Single would animate the entire sheet rather than individual frames.
- Import settings, filter mode, compression and original PNGs are not changed.
- `Phase2.png`, `SlamAttack.png`, `SwapAttack.png`, `Walk.png` are reserved and
  are not connected or modified.

Both clips use only SpriteRenderer.m_Sprite with an empty binding path and
12 FPS. Six displayed frames each last 1/12 second; an endpoint key at 0.5s
makes clip length exactly 0.5 seconds. Idle loops; Charge does not.
Only ChargeAttack uses float parameter `VargaChargeSpeed`, set at attack start
to `chargeAnimationClip.length / chargeDuration` (0.833333 for duration 0.6s).
Animator.speed remains unchanged. Animator.Play with normalized time 0 and
Update(0) applies the first sprite immediately. The coroutine returns to Idle
after movement and before recovery. No Animation Events are added.

## Verification

Confirmed outside Unity: source sheet images inspected, numeric frame order
and metadata checked; runtime and Editor scripts compile together against
Unity 2021.3.8f1 UnityEditor/UnityEngine and installed DOTween APIs.
CS0649 warnings refer to serialized Inspector fields populated by prefab/setup.

Unity batch asset generation was attempted but stopped on license activation:
`BatchMode: Unity has not been activated with a valid License`.
The setup has therefore not yet generated clips, modified the controller or
assigned an Animator in the prefab. The menu must be run to apply those changes.

In BossTest, verify initial Idle, Idle during chase and warning, first-frame
ChargeAttack at departure, advancing frames throughout charge, Idle during
recovery, and a second charge restarting at frame zero. Also verify visual X
rotation stays 90 degrees, telegraph squash/release still works, warning fills
and damage logging work as before, and Console has no errors. These Play mode
checks have not been performed by the agent.
