using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;

public class HDRColorsTestController : MonoBehaviour
{
    private UIDocument _uiDocument;
    private UIFlowRunner _flowRunner;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _flowRunner = GetComponent<UIFlowRunner>();
    }

    private void OnEnable()
    {
        if (_flowRunner != null)
            _flowRunner.OnViewChanged += HandleViewChanged;
    }

    private void OnDisable()
    {
        if (_flowRunner != null)
            _flowRunner.OnViewChanged -= HandleViewChanged;
    }

    private void HandleViewChanged(string viewName)
    {
        if (viewName == "HDRColorsTest")
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
    }
}
