using _Project.Scripts.PureCSharp;
using UnityEngine;
using UnityEngine.Serialization;

public class PickColorShowLuminance : MonoBehaviour
{
    [FormerlySerializedAs("TheColorPicked")] [ColorUsage(true, true)]
    public Color theColorPicked;

    public float paperWhite = 200f;
    public float nits;

    public RenderTexture renderTexture;

    private void Update()
    {
        /*
        if (true) return;
        //camera.targetTexture = renderTexture;
        //camera.Render();
        RenderTexture.active = renderTexture;
            
        var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBAFloat, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        var colors = texture.GetPixels();
        
        theColorPicked = colors[(int)Input.mousePosition.y * Screen.width + (int)Input.mousePosition.x];

        nits = Utilities.RGBtoNits(theColorPicked, paperWhite);
        RenderTexture.active = null;*/
    }
}
