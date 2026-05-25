using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Properties;

namespace _Project.Scripts
{
    [Serializable]
    public class DisplayInfoEntry
    {
        [CreateProperty] public string Spec { get; set; }
        [CreateProperty] public string Result { get; set; }
        [CreateProperty] public bool IsWarning { get; set; }
    }

    [CreateAssetMenu(fileName = "DisplayInfoSampleData", menuName = "UI/Display Info Sample Data")]
    public class DisplayInfoSampleData : ScriptableObject
    {
        [CreateProperty]
        public List<DisplayInfoEntry> Entries = new List<DisplayInfoEntry>();
    }
}
