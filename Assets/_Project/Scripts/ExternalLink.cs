using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ExternalLink : MonoBehaviour {
    #if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void openWindow(string url);
#endif
    public void OpenURL(string _url)
    {

#if UNITY_WEBGL
        openWindow(_url);
#else
        Application.OpenURL(_url);
#endif
    }

    public void BUTTON_OPEN_URL(string _url)
    {

#if UNITY_WEBGL
        openWindow(_url);
#else
        Application.OpenURL(_url);
#endif
    }
}
