#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Dsu.Extension
{
    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    public class CustomTransformComponent : UnityEditor.Editor
    {
        private Transform _transform;
        private GUILayoutOption layoutMaxWidth;
        private static bool quaternionFoldout = false;
        private bool snapAllChildren = true;

        public override void OnInspectorGUI()
        {
            layoutMaxWidth ??= GUILayout.MaxWidth(600);
            //We need this for all OnInspectorGUI sub methods
            _transform = (Transform)target;

            StandardTransformInspector();
            QuaternionInspector();

            if (DsuEditorSettings.EnableWorldTransform)
                WorldTransformInspector();
        }

        private void StandardTransformInspector()
        {
            bool didPositionChange = false;
            bool didRotationChange = false;
            bool didScaleChange = false;

            // Watch for changes. 
            //  1)  Float values are imprecise, so floating point error may cause changes 
            //      when you've not actually made a change.
            //  2)  This allows us to also record an undo point properly since we're only
            //      recording when something has changed.

            // Store current values for checking later
            Vector3 initialLocalPosition = _transform.localPosition;
            Vector3 initialLocalEuler = _transform.localEulerAngles;
            Vector3 initialLocalScale = _transform.localScale;

            EditorGUI.BeginChangeCheck();
            Vector3 localPosition = EditorGUILayout.Vector3Field("Position", _transform.localPosition);
            if (EditorGUI.EndChangeCheck()) didPositionChange = true;

            EditorGUI.BeginChangeCheck();
            Vector3 localEulerAngles = EditorGUILayout.Vector3Field("Euler Angle", _transform.localEulerAngles);
            if (EditorGUI.EndChangeCheck()) didRotationChange = true;

            EditorGUI.BeginChangeCheck();
            Vector3 localScale = EditorGUILayout.Vector3Field("Scale", _transform.localScale);
            if (EditorGUI.EndChangeCheck()) didScaleChange = true;

            // Apply changes with record undo
            if (didPositionChange || didRotationChange || didScaleChange) {
                Undo.RecordObject(_transform, _transform.name);

                if (didPositionChange) _transform.localPosition = localPosition;
                if (didRotationChange) _transform.localEulerAngles = localEulerAngles;
                if (didScaleChange) _transform.localScale = localScale;
            }

            // Since BeginChangeCheck only works on the selected object 
            // we need to manually apply transform changes to all selected objects.
            Transform[] selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length > 1) {
                foreach (var item in selectedTransforms) {
                    if (didPositionChange || didRotationChange || didScaleChange)
                        Undo.RecordObject(item, item.name);

                    if (didPositionChange)
                        item.localPosition = ApplyVector3ChangesOnly(item.localPosition, initialLocalPosition, _transform.localPosition);

                    if (didRotationChange)
                        item.localEulerAngles = ApplyVector3ChangesOnly(item.localEulerAngles, initialLocalEuler, _transform.localEulerAngles);

                    if (didScaleChange)
                        item.localScale = ApplyVector3ChangesOnly(item.localScale, initialLocalScale, _transform.localScale);
                }
            }
        }

        private Vector3 ApplyVector3ChangesOnly(Vector3 toApply, Vector3 initial, Vector3 changed)
        {
            if (!Mathf.Approximately(initial.x, changed.x)) toApply.x = changed.x;
            if (!Mathf.Approximately(initial.y, changed.y)) toApply.y = changed.y;
            if (!Mathf.Approximately(initial.z, changed.z)) toApply.z = changed.z;
            return toApply;
        }

        private Quaternion ApplyQuaternionChangesOnly(Quaternion toApply, Quaternion initial, Quaternion changed)
        {
            if (!Mathf.Approximately(initial.w, changed.w)) toApply.w = changed.w;
            if (!Mathf.Approximately(initial.x, changed.x)) toApply.x = changed.x;
            if (!Mathf.Approximately(initial.y, changed.y)) toApply.y = changed.y;
            if (!Mathf.Approximately(initial.z, changed.z)) toApply.z = changed.z;
            return toApply;
        }

        private void QuaternionInspector()
        {

            //Additional element to also view the Quaternion rotation values
            quaternionFoldout = EditorGUILayout.Foldout(quaternionFoldout, "Quaternion Rotation:    " + _transform.localRotation.ToString("F3"));
            if (quaternionFoldout) {
                Vector4 q = QuaternionToVector4(_transform.localRotation);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.HelpBox("Be careful! Editing quaternion values directly can produce unexpected behavior.", MessageType.Warning);

                q = EditorGUILayout.Vector4Field("Quaternion", q);

                if (EditorGUI.EndChangeCheck()) {
                    if (EditorUtility.DisplayDialog("Warning", "Modifying Quaternion values directly can cause instability. Proceed?", "Yes", "Cancel")) {
                        Undo.RecordObject(_transform, "modify quaternion rotation on " + _transform.name);
                        _transform.localRotation = NormalizeQuaternion(ConvertToQuaternion(q));
                    }
                }
            }
        }

        private Quaternion NormalizeQuaternion(Quaternion q)
        {
            float magnitude = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return magnitude > 0f ? new Quaternion(q.x / magnitude, q.y / magnitude, q.z / magnitude, q.w / magnitude) : q;
        }

        private Quaternion ConvertToQuaternion(Vector4 v4) => new Quaternion(v4.x, v4.y, v4.z, v4.w);
        private Vector4 QuaternionToVector4(Quaternion q) => new Vector4(q.x, q.y, q.z, q.w);

        public static Vector3 RoundToInt(Vector3 pos) => new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        private void ApplyRoundToIntHierarchy(Transform root)
        {
            Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Snap All Positions");
            root.position = RoundToInt(root.position);
            foreach (Transform child in root) {
                ApplyRoundToIntHierarchy(child);
            }
        }

        private void ApplyRoundToIntOnlySelf(Transform target)
        {
            Undo.RecordObject(target, "Snap Position");
            target.position = RoundToInt(target.position);
        }

        private void WorldTransformInspector()
        {
            bool isUpdatePosition = false;
            bool isUpdateRotation = false;

            Vector3 position = _transform.position;
            Vector3 worldEulerAngles = _transform.rotation.eulerAngles;

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            position = EditorGUILayout.Vector3Field("World Position", position);
            if (EditorGUI.EndChangeCheck()) {
                isUpdatePosition = true;
            }

            EditorGUI.BeginChangeCheck();
            worldEulerAngles = EditorGUILayout.Vector3Field("World Rotation", worldEulerAngles);
            if (EditorGUI.EndChangeCheck()) {
                isUpdateRotation = true;
            }

            if (isUpdatePosition) _transform.position = position;
            if (isUpdateRotation) _transform.rotation = Quaternion.Euler(worldEulerAngles);

            Transform[] selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length > 1) {
                foreach (var item in selectedTransforms) {
                    if (isUpdatePosition || isUpdateRotation)
                        Undo.RecordObject(item, item.name);

                    if (isUpdatePosition)
                        item.position = ApplyVector3ChangesOnly(item.position, _transform.position, position);

                    if (isUpdateRotation)
                        item.rotation = ApplyQuaternionChangesOnly(item.rotation, _transform.rotation, Quaternion.Euler(worldEulerAngles));
                }
            }

            EditorGUILayout.BeginHorizontal();
            snapAllChildren = EditorGUILayout.ToggleLeft("All", snapAllChildren, GUILayout.Width(40));
            if (GUILayout.Button("Snap Positions", GUILayout.Width(100))) {
                if (snapAllChildren)
                    ApplyRoundToIntHierarchy(_transform);
                else
                    ApplyRoundToIntOnlySelf(_transform);
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button("Rotate -90", GUILayout.Width(76))) {
                worldEulerAngles.y -= 90.0f;
                _transform.rotation = Quaternion.Euler(worldEulerAngles);
            }

            if (GUILayout.Button("Rotate +90", GUILayout.Width(76))) {
                worldEulerAngles.y += 90.0f;
                _transform.rotation = Quaternion.Euler(worldEulerAngles);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

    }
}
#endif
