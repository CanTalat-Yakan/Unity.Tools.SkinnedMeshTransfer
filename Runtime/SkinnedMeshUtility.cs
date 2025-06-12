using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEssentials
{
    public static class SkinnedMeshUtilities
    {
        public class TransferResult
        {
            public int TransferredCount { get; set; }
            public int SkippedCount { get; set; }
            public List<(string meshName, string boneName)> MissingBones { get; } = new();
        }

        public static TransferResult RetargetSkinnedMeshes(
            SkinnedMeshRenderer[] renderers,
            Transform newArmature,
            Transform newParent,
            bool resetTransform = true)
        {
            if (renderers == null || renderers.Length == 0)
                throw new ArgumentException("No skinned mesh renderers provided.", nameof(renderers));
            if (newArmature == null)
                throw new ArgumentNullException(nameof(newArmature));
            if (newParent == null)
                throw new ArgumentNullException(nameof(newParent));

            var boneCache = BuildBoneCache(newArmature);
            var result = new TransferResult();

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    result.SkippedCount++;
                    continue;
                }

                var missingBones = new List<string>();
                var newBones = new Transform[renderer.bones.Length];

                for (int i = 0; i < renderer.bones.Length; i++)
                {
                    var boneName = renderer.bones[i]?.name;
                    if (string.IsNullOrEmpty(boneName) || !boneCache.TryGetValue(boneName, out var newBone) || newBone == null)
                    {
                        missingBones.Add(boneName ?? "<null>");
                        newBones[i] = null;
                    }
                    else
                    {
                        newBones[i] = newBone;
                    }
                }

                var rootBoneName = renderer.rootBone?.name;
                boneCache.TryGetValue(rootBoneName, out var newRootBone);

                renderer.bones = newBones;
                renderer.rootBone = newRootBone ?? newArmature;

                renderer.transform.SetParent(newParent, worldPositionStays: false);

                if (resetTransform)
                {
                    renderer.transform.localPosition = Vector3.zero;
                    renderer.transform.localRotation = Quaternion.identity;
                    renderer.transform.localScale = Vector3.one;
                }

                if (missingBones.Count > 0)
                {
                    Debug.LogWarning($"[{renderer.name}] Missing bones: {string.Join(", ", missingBones)}");
                    result.MissingBones.AddRange(missingBones.Select(b => (renderer.name, b)));
                }

                result.TransferredCount++;
            }

            Debug.Log($"[SkinnedMeshTransfer] Transferred {result.TransferredCount} skinned mesh(es), skipped {result.SkippedCount}. " +
                      (result.MissingBones.Count > 0 ? $"Missing bones: {result.MissingBones.Count}" : "All bones mapped."));

            return result;
        }

        private static Dictionary<string, Transform> BuildBoneCache(Transform armature) =>
            armature.GetComponentsInChildren<Transform>(true)
                .GroupBy(t => t.name)
                .ToDictionary(g => g.Key, g => g.First());
    }
}