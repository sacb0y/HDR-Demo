using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FAQData", menuName = "FAQ/FAQ Data")]
public class FAQData : ScriptableObject
{
    public List<FAQEntry> entries = new List<FAQEntry>();
}

[Serializable]
public class FAQEntry
{
    public string question;
    [TextArea(3, 10)]
    public string answer;
}
