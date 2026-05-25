using UnityEngine;

namespace _Project.Scripts.PureCSharp
{
    public static class Utilities 
    {
        public static float RGBtoNits(Color color, float paperWhite)
        {
            var nits = Vector3.Dot(new Vector3(0.2126f, 0.7152f, 0.0722f), new Vector3(color.r, color.g, color.b)) * paperWhite;
            return nits;
        }
    }
}
