using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Momoirosoft/GalleryConfig")]
public class GalleryConfig : ScriptableObject
{
    public List<GalleryImage> gallery;
    [Serializable]
    public class GalleryImage
    {
        public Texture image;
        [TextArea]
        public string description;
    }
}
