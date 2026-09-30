# Pirate Eight Direction Rig

1. Copy the `PirateEightDirection` folder into the target Unity project's `Assets` folder.
2. In Unity, run `Tools > Pirate Eight Direction > Prepare Textures and Prefab`.
3. Drag `Assets/PirateEightDirection/Generated/PirateEightDirectionRig.prefab` under the player.
4. When a `Rigidbody2D` exists on the player or a parent, direction and walking are read automatically.
5. For a custom controller, call `SetMotion(Vector2)` every frame. Call `UseRigidbodyMotion()` to return to automatic motion.

The rig contains six rigid image layers per direction: head, torso, front/back arms and front/back legs. Idle breathing and walking use bone transforms at runtime. The source turnaround is AI-assisted and should receive an art pass before final commercial use, especially at occluded shoulder and hip seams.
