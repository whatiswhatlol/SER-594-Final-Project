#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(ParallaxController))]
public class ParalaxControllerEditor : Editor
{
    ReorderableList _list;
    SerializedProperty _layersProp;

    void OnEnable()
    {
        _layersProp = serializedObject.FindProperty("layers");

        _list = new ReorderableList(serializedObject, _layersProp, true, true, true, true);

        _list.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Layers (back to front or any order)");

        _list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = _layersProp.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.height = EditorGUI.GetPropertyHeight(element, includeChildren: true);
            EditorGUI.PropertyField(rect, element, GUIContent.none, includeChildren: true);
        };

        _list.elementHeightCallback = (int index) =>
        {
            var element = _layersProp.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, includeChildren: true) + 6f;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "layers");

        EditorGUILayout.Space(6);
        _list.DoLayoutList();

        var controller = (ParallaxController)target;
        EditorGUILayout.Space(6);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Auto-Fill From Children"))
            {
                Undo.RecordObject(controller, "Auto-Fill Parallax Layers");
                controller.AutoFillFromChildren();
                EditorUtility.SetDirty(controller);
            }
            if (GUILayout.Button("Capture Starts"))
            {
                Undo.RecordObject(controller, "Capture Parallax Starts");
                controller.CaptureStarts();
                EditorUtility.SetDirty(controller);
            }
            if (GUILayout.Button("Bake Multipliers From Z"))
            {
                if (EditorUtility.DisplayDialog("Bake Multipliers From Z",
                    "Set per-layer multipliers from depth falloff and enable Override?", "Bake", "Cancel"))
                {
                    Undo.RecordObject(controller, "Bake Multipliers From Z");
                    controller.BakeMultipliersFromZ(1f);
                    EditorUtility.SetDirty(controller);
                }
            }
        }

#if UNITY_EDITOR
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Preview Drag (Edit Mode)", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            bool enable = EditorGUILayout.Toggle("Enable Preview", controller.editorPreviewEnabled);
            Vector2 offset = controller.editorPreviewOffset;
            if (enable)
            {
                offset.x = EditorGUILayout.Slider("Preview X", offset.x, -50f, 50f);
                offset.y = EditorGUILayout.Slider("Preview Y", offset.y, -50f, 50f);
            }
            controller.EditorSetPreview(offset, enable);
        }
#endif

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
