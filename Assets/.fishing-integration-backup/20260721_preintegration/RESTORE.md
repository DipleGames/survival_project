# Tile fishing integration rollback

Backup captured before the Game-scene tile fishing integration on 2026-07-21.

To restore the exact pre-integration state:

1. Restore `FishingTestController.cs` to `Assets/Scripts/FishingTest/FishingTestController.cs`.
2. Restore `FishingTestPlayerController.cs` to `Assets/Scripts/FishingTest/FishingTestPlayerController.cs`.
3. Restore `Game.unity` to `Assets/Scenes/Game.unity`.
4. Restore `FishingTestSceneBuilder.cs` to `Assets/Editor/FishingTestSceneBuilder.cs`.
5. Delete `Assets/Editor/TileFishingGameSceneSetup.cs` and its generated `.meta` file, if present.

Close Unity before restoring the scene, or let Unity reload the restored file afterward.
