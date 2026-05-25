using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;

namespace _Project.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class VersionLabelController : MonoBehaviour
    {
        [SerializeField] private string labelName = "Version";
        private UIFlowRunner _flowRunner;
        private UIDocument _uiDocument;

        private void Awake()
        {
            _flowRunner = GetComponent<UIFlowRunner>();
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_flowRunner != null)
            {
                _flowRunner.OnViewChanged += HandleViewChanged;
            }
            
            UpdateVersionLabel();
        }

        private void OnDisable()
        {
            if (_flowRunner != null)
            {
                _flowRunner.OnViewChanged -= HandleViewChanged;
            }
        }

        private void HandleViewChanged(string viewName)
        {
            UpdateVersionLabel();
        }

        private void UpdateVersionLabel()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
            
            var versionLabel = _uiDocument.rootVisualElement.Q<Label>(labelName);
            if (versionLabel != null)
            {
                versionLabel.text = $"v{Application.version}";
            }
        }
    }
}
