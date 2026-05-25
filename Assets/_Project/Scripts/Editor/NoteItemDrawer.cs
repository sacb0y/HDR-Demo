using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(NoteItem))]
    public class NoteItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("type");
            var titleProp = property.FindPropertyRelative("noteTitle");
            var textProp = property.FindPropertyRelative("noteText");
            var objectProp = property.FindPropertyRelative("relatedObject");

            Color color = Color.white;
            TypeEnum type = (TypeEnum)typeProp.enumValueIndex;
            switch (type)
            {
                case TypeEnum.Reminder:
                    color = Color.blue;
                    break;
                case TypeEnum.MinorNote:
                    color = Color.green;
                    break;
                case TypeEnum.Important:
                    color = Color.red;
                    break;
                case TypeEnum.Broken:
                    color = Color.yellow;
                    break;
            }

            Rect rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;

            // Type
            Color oldColor = GUI.color;
            GUI.color = color;
            EditorGUI.PropertyField(rect, typeProp, GUIContent.none);
            GUI.color = oldColor;

            // Title
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, titleProp, GUIContent.none);

            // Text
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            float textHeight = EditorGUI.GetPropertyHeight(textProp);
            rect.height = textHeight;
            EditorGUI.PropertyField(rect, textProp, GUIContent.none);

            // Object
            rect.y += textHeight + 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, objectProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var textProp = property.FindPropertyRelative("noteText");
            return (EditorGUIUtility.singleLineHeight + 2) * 3 + EditorGUI.GetPropertyHeight(textProp) + 5;
        }
    }
}