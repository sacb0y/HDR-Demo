using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace _Project.Scripts.Editor
{
    [CustomEditor(typeof(GalleryConfig))]
    public class GalleryConfigEditor : UnityEditor.Editor
    {
        private ReorderableList list;

        private void OnEnable()
        {
            var prop = serializedObject.FindProperty("gallery");
            list = new ReorderableList(serializedObject, prop, true, true, true, true);

            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Gallery");
            };

            list.elementHeight = 120;

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                var imageProp = element.FindPropertyRelative("image");
                var descProp = element.FindPropertyRelative("description");

                Rect imageRect = new Rect(rect.x, rect.y + 5, 110, 110);
                EditorGUI.PropertyField(imageRect, imageProp, GUIContent.none);

                Rect descRect = new Rect(rect.x + 115, rect.y + 5, rect.width - 115, 110);
                descProp.stringValue = EditorGUI.TextArea(descRect, descProp.stringValue);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}