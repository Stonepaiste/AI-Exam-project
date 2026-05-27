# Bird AI — Flocking + Behavior Tree

A stalking-flock AI for birds that hunt the third-person player.

Birds **roam solo** until one spots the player; sightings escalate the
`HiveMind` through **Rallying → Diving → Investigating → Roaming**, and each
bird's Unity Behavior graph reacts to that shared state.

## Pipeline

```
BirdPerception  ─►  HiveMind                 ◄──── Behavior Graph (per bird)
  (raycast LOS)     (state: Roam/Rally/Dive/       reads hive state,
  reports sighting   Investigate, last known,     sets BirdMotor.Mode +
  each ~0.15s)       rally point)                 BirdMotor.Target
                                                         │
                                                         ▼
                                                   BirdMotor (moves bird)
```

The BT nodes never make motion decisions directly — they flip the bird's
`Motor.Mode` and feed it a `Motor.Target`. The motor translates that into
wander / seek / hold / dive with optional flocking.

## One-time scene setup

1. **Open Package Manager** (Window → Package Manager). Unity will auto-import
   `com.unity.behavior` from the manifest. If the version pinned in
   `Packages/manifest.json` doesn't resolve, select `com.unity.behavior` in
   Package Manager and click *Update to latest*.

2. **Create a `Cover` layer** (Edit → Project Settings → Tags and Layers).
   Assign it to any mesh that should block bird line-of-sight: walls, rocks,
   roofs, trees, doors. Leave terrain on its usual layer and add it to the mask too.

3. **Tag the player.** On the ThirdPersonController instance in the scene:
   - Add a **`PlayerTarget`** component.
   - Drag the character's **chest bone** (or an empty child at chest height)
     into `Center Mass`.
   - Drag the **head bone** (or an empty child at eye height) into `Head`.
   - Optionally tune `Max Health`.

4. **Drop a HiveMind into the scene.** Create an empty GameObject called
   `HiveMind` and add the `HiveMind` component. Tune `memoryDuration`,
   `minBirdsToDive`, `rallyHeight`, etc.

5. **Make the Bird prefab.**
   - Create a GameObject with your bird mesh.
   - Add `Bird`, `BirdMotor`, `BirdPerception`.
   - On `BirdPerception`, set `Cover Mask` to include the `Cover` layer
     (and anything else that should block LOS — walls, terrain).
   - Optionally drag an "eye" empty child into `Eye`.
   - Save as a prefab.

6. **Behavior graph (the BT).** Right-click in the Project window →
   *Create → Behavior → Behavior Graph*. Open it and build:

   ```
   Root → Selector
     ├── Sequence
     │     ├── Condition: Hive Is Diving
     │     └── Action:    Dive At Target
     ├── Sequence
     │     ├── Condition: Hive Is Rallying
     │     ├── Action:    Fly To Rally Point
     │     └── Action:    Hold At Rally Point   (Repeat forever modifier recommended)
     ├── Sequence
     │     ├── Condition: Hive Is Investigating
     │     └── Action:    Investigate Last Known
     └── Action: Roam
   ```

   The custom nodes appear in the node picker under **Action/Bird**,
   **Condition/Hive**, and **Condition/Bird**.

   In the graph's Blackboard add a `GameObject` variable called **`Self`**
   and wire it into every Action/Condition that has a `Self` slot.

   Save the graph asset.

7. **Wire up the bird prefab to the graph.**
   - Add a **`BehaviorGraphAgent`** to the Bird prefab.
   - Assign the graph asset to it.
   - In the agent's blackboard, bind `Self` to the bird's own GameObject
     (drag the root in, or leave it at runtime and fill it from a small
     bootstrap script if you prefer).

8. **Spawn birds.** Instantiate the prefab, or place 6–10 in the scene
   spread out at flying altitude. They'll register with the hive in
   `OnEnable` automatically.

## How the states fit together

| Hive state       | Triggered by                              | Bird does                            |
|------------------|-------------------------------------------|--------------------------------------|
| `Roaming`        | Start, or after `Investigating` times out | Solo wander (no flocking)            |
| `Rallying`       | Any bird reports a sighting               | Fly to rally point, form up with others |
| `Diving`         | `minBirdsToDive` reached rally point      | Full-speed dive at last known position |
| `Investigating`  | Memory decayed while Rallying/Diving      | Flock toward last known position     |

After a dive, if the player is still in memory the hive re-enters
`Rallying` for another pass; otherwise it drops to `Investigating`, then
`Roaming` once `memoryDuration + giveUpDuration` has passed without LOS.

## Hiding / cover

Any collider on a layer in `BirdPerception.coverMask` blocks LOS. Crouch
behind a wall and the bird rays hit the wall — no sighting, the hive's
memory times out, the flock goes home.

## Tuning knobs

| What | Where | Default |
|------|-------|---------|
| How many birds needed to commit a dive | `HiveMind.minBirdsToDive` | 4 |
| How long memory lasts | `HiveMind.memoryDuration` | 8 s |
| Extra investigate time | `HiveMind.giveUpDuration` | 5 s |
| Rally altitude above target | `HiveMind.rallyHeight` | 40 |
| Dive pass speed | `BirdMotor.diveSpeed` | 32 |
| LOS polling rate | `BirdPerception.checkInterval` | 0.15 s |
| Vision range / FOV | `BirdPerception.viewRange` / `viewAngleDeg` | 80 / 180° |
| Damage per dive hit | `Bird.diveDamage` | 8 |

## Files

- `HiveMind.cs` — scene-singleton shared awareness.
- `Bird.cs` — per-bird glue; registers with hive, exposes `ApplyDiveHit`.
- `BirdMotor.cs` — mode-switchable movement + conditional flocking.
- `BirdPerception.cs` — LOS raycast, reports sightings.
- `PlayerTarget.cs` — tag component on the player; takes damage.
- `BehaviorNodes/` — custom Unity Behavior Actions & Conditions.

The existing `UnityFlock.cs` and `UnityFlockController.cs` in `Scripts/` are
untouched so the original chapter example still works.
