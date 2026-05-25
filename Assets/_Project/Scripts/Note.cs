using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[ExecuteInEditMode]

public class Note : MonoBehaviour
{
#if UNITY_EDITOR
    public List<NoteItem> Notes = new List<NoteItem>();
#endif
}
#if UNITY_EDITOR
[Serializable]
public struct NoteItem
{
    public TypeEnum type;
    public string noteTitle;
    public string noteText;
    public GameObject relatedObject;
}
public enum TypeEnum
{
    Reminder, MinorNote, Important, Broken
}
#endif