using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string initialSceneName = "TitleScreen";

    private void Start()
    {
        SceneManager.LoadScene(initialSceneName);
    }
}