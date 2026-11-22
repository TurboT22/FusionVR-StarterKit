# FusionVR Starter Kit (Jungle)

Production-style starter stations for [FusionVR](https://github.com/TurboT22/FusionVR), all generated procedurally in the editor: lathed
pots and drums, bevelled planks, bark-wrapped trunks, bamboo with nodes, twisting vines and leaf blades. Textures
(wood grain, bark, mossy stone, moss, leaf veins, bamboo) are generated as tileable noise with normal maps.
No stock cubes and no imported art.

## Stations

| Prefab | What it does |
|---|---|
| `FusionVR_RoomComputer` | Wooden desk terminal with a keyboard. Type a code and press GO to join a private room. A PUBLIC key joins a random public room. |
| `FusionVR_PublicRoomBoard` | Carved totem with PREV / NEXT queue buttons and a glowing JOIN stone. |
| `FusionVR_NameStation` | Lectern with a keyboard (SHIFT, SPACE, DEL, CLR, SAVE) to change your username. |
| `FusionVR_ColourStation` | Shelves of clay paint pots. Press one to change your colour. A glowing pot picks a random colour; a pod shows your current colour. |
| `FusionVR_Leaderboard` | Bamboo-mounted board ranking everyone in the room by score, in their player colour. |
| `FusionVR_Wardrobe` | Open wardrobe with a plinth to browse body parts and cosmetics, then equip or remove them. |
| `FusionVR_ControlPanel` | Stone slab with MUTE, DEAFEN and LEAVE buttons and status lamps. |

## Install

1. Install FusionVR core (version 1.1.0 or newer) and make sure the Setup Wizard shows green ticks.
2. Add this package (`Add package from disk...` and pick `package.json`, or drop the folder into `Assets`).
3. Run `Tools > FusionVR > Starter Kit > Generate All Prefabs`. The first run takes a little while because it
   generates textures and meshes into `Assets/FusionVR/StarterKit/Generated`. Prefabs go to
   `Assets/FusionVR/StarterKit/Prefabs`.
4. Use `Tools > FusionVR > Starter Kit > Add ... to Scene`, or drag prefabs in yourself.
5. Run `Tools > FusionVR > Starter Kit > Setup Hand Triggers` (the core Setup Wizard also does this). Buttons only
   react to hands that carry a `FusionVRHandTrigger`.

Stations face +Z. Rotate them so the front faces where players will stand.

## Scores

The leaderboard reads `FusionVRPlayer.Score`. Set it from your game code:

```csharp
using Fusion.VR;

FusionVRManager.SetScore(0);
FusionVRManager.AddScore(10);
```

## Customising

- Re-run `Rebuild All Prefabs (regenerate)` to rebuild geometry. Meshes update in place, so existing scene
  instances keep working.
- `Regenerate Textures and Materials` rebuilds the jungle materials.
- Change queue names or room size on `FusionVRPublicRoomBoard`, name length on `FusionVRNameStation`, and rows on
  `FusionVRLeaderboard`.
- Wardrobe items come from the `Cosmetics` containers of the player prefab. Add more with
  `Tools > FusionVR > Cosmetic Helper`.

## Notes

- Requires TextMeshPro Essentials (the text is TextMeshPro 3D).
- Materials use URP Lit when available, otherwise Standard.
- Everything is baked into prefabs and meshes, so there is no runtime generation cost on Quest.
