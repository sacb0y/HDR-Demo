using UnityEngine;

namespace _Project.Scripts
{
    public class DoNotDestroy : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
            if (Application.isPlaying)
            {
                UnityEditor.SceneVisibilityManager.instance.Show(gameObject, false);
            }
#endif
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
/*
        private void OnEnable()
        {
            ResetGame.ResetGameEvent += DestroySelf;
        }

        private void OnDestroy()
        {
            ResetGame.ResetGameEvent -= DestroySelf;
        }
*/
        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
