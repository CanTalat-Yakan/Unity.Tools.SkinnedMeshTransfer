# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Skinned Mesh Transfer

> Quick overview: Rebind one or more SkinnedMeshRenderers to a new armature by bone name, re‑parent them under a new root, and optionally reset local transforms. Includes an Editor window for one‑click transfers and a runtime API with a detailed result.

Transfer skinned meshes from one rig to another in seconds. The tool maps bones by name from a target armature, assigns the renderer’s bones and root bone, reparents the renderers, and can zero out local transforms for clean alignment.

![screenshot](Documentation/Screenshot.png)

## Features
- Name‑based retargeting
  - Maps each renderer’s bones to a new armature by matching `Transform.name`
  - Root bone is matched by name when possible, otherwise falls back to the armature root
- Batch transfer
  - Process one or many `SkinnedMeshRenderer` components at once
  - Reparent all transferred renderers under a chosen parent
- Clean alignment
  - Optionally reset local position/rotation/scale to (0,0,0)/(identity)/(1,1,1) after reparenting
  - Editor window uses reset by default for predictable results
- Clear feedback
  - Console warnings list any missing bones per mesh
  - API returns a `TransferResult` with counts and missing bone pairs
- Editor and API
  - Editor window: Tools → Skinned Mesh Transfer (reorderable list + object pickers)
  - Runtime API: `SkinnedMeshUtilities.RetargetSkinnedMeshes(...)`

## Requirements
- Unity 6000.0+
- No external dependencies
- Skinned meshes must use consistent bone names across rigs

## Usage

### Editor window (one‑click transfer)
1) Open: Tools → Skinned Mesh Transfer
2) Add the `SkinnedMeshRenderer` components you want to move (drag from Hierarchy or Project)
3) Set:
   - New Armature (Hips): the root transform of the target skeleton (commonly “Hips”)
   - New Parent: the GameObject under which the renderers will be placed
4) Click "Transfer Skinned Meshes"
   - Bones and root bones are remapped by name
   - Renderers are reparented under New Parent
   - Local transforms are reset to default for a clean fit
5) Check the Console for a summary and any missing bone warnings

### Programmatic usage
```csharp
using UnityEssentials;
using UnityEngine;

public class TransferExample : MonoBehaviour
{
    public SkinnedMeshRenderer[] sources;
    public Transform newArmature; // e.g., target character Hips
    public Transform newParent;   // e.g., target character root

    void Start()
    {
        var result = SkinnedMeshUtilities.RetargetSkinnedMeshes(
            sources,
            newArmature,
            newParent,
            resetTransform: true // set false to keep local transforms
        );

        Debug.Log($"Transferred: {result.TransferredCount}, Skipped: {result.SkippedCount}, MissingBones: {result.MissingBones.Count}");
    }
}
```

## How It Works
- Build bone cache
  - Collect all `Transform` children of the armature and index them by `name`
  - If multiple bones share a name, the first found is used
- Remap each renderer
  - For every bone slot, look up a bone with the same name in the cache; assign or record as missing
  - Root bone is matched by name; if not found, use the armature root
  - Reparent the renderer under the chosen New Parent with `SetParent(worldPositionStays: false)`
  - Optionally reset local position/rotation/scale
- Report
  - Log a summary and one warning per renderer listing any missing bones
  - Return a `TransferResult` with counts and a `(meshName, boneName)` list for missing entries

## Notes and Limitations
- Name collisions and duplicates
  - Mapping is by bone name only; if an armature has duplicate names, the first match wins
- Transform behavior
  - Editor transfer resets local transforms to defaults; programmatic API lets you disable this via `resetTransform: false`
  - Parenting uses `worldPositionStays: false`; combined with reset=false, local transforms are preserved, which may change world position - verify alignment in your scene
- Scope
  - Only bones/rootBone and parenting are changed; meshes, weights, and blendshapes are untouched
  - This tool does not retarget animations or create Avatars; it only rebinds skinned meshes
- Input validation
  - Null renderers are skipped and counted in the result
  - Armature and parent must be assigned (throws otherwise)

## Files in This Package
- `Editor/SkinnedMeshTransferEditor.cs` – Tools window with list and object pickers
- `Runtime/SkinnedMeshUtility.cs` – Retargeting logic and `TransferResult` summary
- `Runtime/UnityEssentials.SkinnedMeshTransfer.asmdef` – Runtime assembly definition
- `Editor/UnityEssentials.SkinnedMeshTransfer.Editor.asmdef` – Editor assembly definition

## Tags
unity, skinned mesh, retargeting, bones, armature, rigging, character, mesh transfer, editor tool
