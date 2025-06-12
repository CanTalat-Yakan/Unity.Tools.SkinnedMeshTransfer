#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEssentials
{
    public class SkinnedMeshTransferEditor : EditorWindow
    {
        [SerializeField] private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        [SerializeField] private Transform _newArmature;
        [SerializeField] private Transform _newParent;

        private Vector2 _viewScrollPosition;
        private ReorderableList _skinnedMeshList;

        [MenuItem("Tools/Skinned Mesh Transfer")]
        public static void OpenWindow()
        {
            var window = GetWindow<SkinnedMeshTransferEditor>("Skinned Mesh Transfer");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnEnable()
        {
            if (_skinnedMeshRenderers == null)
                _skinnedMeshRenderers = new SkinnedMeshRenderer[0];

            _skinnedMeshList = new ReorderableList(null, typeof(SkinnedMeshRenderer), true, false, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    _skinnedMeshRenderers[index] = (SkinnedMeshRenderer)EditorGUI.ObjectField(
                        rect,
                        _skinnedMeshRenderers[index],
                        typeof(SkinnedMeshRenderer),
                        true);
                },
                onAddCallback = list => { ArrayUtility.Add(ref _skinnedMeshRenderers, null); },
                onRemoveCallback = list => { ArrayUtility.RemoveAt(ref _skinnedMeshRenderers, list.index); },
                elementHeight = EditorGUIUtility.singleLineHeight + 4
            };
        }

        public void OnGUI()
        {
            EditorGUILayout.Space();

            _viewScrollPosition = EditorGUILayout.BeginScrollView(_viewScrollPosition, GUILayout.Height(position.height - 90));
            {
                if (_skinnedMeshList != null)
                {
                    _skinnedMeshList.list = _skinnedMeshRenderers;
                    _skinnedMeshList.DoLayoutList();
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.BeginVertical();
            {
                _newArmature = (Transform)EditorGUILayout.ObjectField(
                    "New Armature (Hips)",
                    _newArmature,
                    typeof(Transform),
                    true);

                _newParent = (Transform)EditorGUILayout.ObjectField(
                    "New Parent",
                    _newParent,
                    typeof(Transform),
                    true);

                EditorGUILayout.Space();

                GUI.enabled = ShouldEnableTransferButton();
                if (GUILayout.Button("Transfer Skinned Meshes", GUILayout.Height(24)))
                    SkinnedMeshUtilities.RetargetSkinnedMeshes(_skinnedMeshRenderers, _newArmature, _newParent);
                GUI.enabled = true;
            }
            GUILayout.EndVertical();
        }

        private bool ShouldEnableTransferButton() =>
            _skinnedMeshRenderers != null &&
            _skinnedMeshRenderers.Length > 0 &&
            _skinnedMeshRenderers.All(r => r != null) &&
            _newArmature != null &&
            _newParent != null;

    }
}
#endif