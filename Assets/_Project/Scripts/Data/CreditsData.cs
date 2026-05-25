using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "CreditsData", menuName = "UI/Credits Data")]
    public class CreditsData : ScriptableObject
    {
        public string title = "Credits";
        public List<CreditSection> sections = new List<CreditSection>();
    }

    [Serializable]
    public class CreditSection
    {
        public string sectionTitle;
        public List<string> entries = new List<string>();
    }
}
