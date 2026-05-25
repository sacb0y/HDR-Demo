using UnityEditor;
using UnityEngine;
using _Project.Scripts;

namespace _Project.Scripts.Editor
{
    [CustomEditor(typeof(DisplayInfo))]
    public class DisplayInfoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DisplayInfo script = (DisplayInfo)target;
            
            GUILayout.Space(10);
            if (GUILayout.Button("Populate Display Info", GUILayout.Height(30)))
            {
                script.PopulateDisplayInfo();
            }
        }
    }
}