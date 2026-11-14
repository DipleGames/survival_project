# Boss system

- Save future boss system scripts under `Assets/Scripts/3.Game/Enemies/boss/`.
- Keep shared boss classes in `Core/` and pattern classes in `Patterns/`.
- Use `Assets/Scenes/Test_Scene/BossTest.unity` (the bosstest scene) for boss testing.
- Keep runtime state and cooldowns on individual boss instances, not ScriptableObject assets.
- BossPattern declares recoveryDuration. Pattern Execute coroutines end with the attack; only BossController waits for recoveryDuration while in Recovery state.
