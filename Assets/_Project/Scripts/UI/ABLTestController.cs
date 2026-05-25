/*
2026-05-23 AI-Tag
This was created with the help of Assistant, a Unity Artificial Intelligence product.
*/
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;
using UnityEngine.Rendering;

namespace _Project.Scripts.UI
{
    public class ABLTestController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private UIFlowRunner _flowRunner;
        private Slider _brightnessSlider;
        private Label _coverageLabel;
        private Toggle _hdrToggle;
        private Button _applyButton;
        private Toggle _hideUIToggle;
        private VisualElement _tabContainer;
        private VisualElement _mainContainer;
        
        private GameObject _gridRoot;
        private Camera _camera;

        // Grid Constants (8x5, size 1, gap 0.2)
        private const float GridWidth = 9.4f;  // 8 * 1.0 + 7 * 0.2
        private const float GridHeight = 5.8f; // 5 * 1.0 + 4 * 0.2

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

            if (_brightnessSlider != null)
                _brightnessSlider.UnregisterValueChangedCallback(OnSliderChanged);
            
            if (_hideUIToggle != null)
                _hideUIToggle.UnregisterValueChangedCallback(OnHideUIToggled);
        }

        private void HandleViewChanged(string viewName)
        {
            if (viewName == "ABLTest")
            {
                InitializeUI();
            }
        }

        private void InitializeUI()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
            
            if( !_camera)
                _camera = Camera.main;

            var root = _uiDocument.rootVisualElement;
            _brightnessSlider = root.Q<Slider>("BrightnessSlider");
            _coverageLabel = root.Q<Label>("CoverageLabel");
            _hideUIToggle = root.Q<Toggle>("HideUIToggle");
            _tabContainer = root.Q<VisualElement>("TabContainer");
            _mainContainer = root.Q<VisualElement>("MainContainer");

            if (_brightnessSlider != null)
            {
                _brightnessSlider.UnregisterValueChangedCallback(OnSliderChanged);
                _brightnessSlider.RegisterValueChangedCallback(OnSliderChanged);
                UpdateValue(_brightnessSlider.value);
            }

            if (_hideUIToggle != null)
            {
                _hideUIToggle.UnregisterValueChangedCallback(OnHideUIToggled);
                _hideUIToggle.RegisterValueChangedCallback(OnHideUIToggled);
                _hideUIToggle.value = false;
                UpdateUIVisibility(false);
            }
        }

        private void OnSliderChanged(ChangeEvent<float> evt)
        {
            UpdateValue(evt.newValue);
        }

        private void OnHideUIToggled(ChangeEvent<bool> evt)
        {
            UpdateUIVisibility(evt.newValue);
        }

        private void UpdateUIVisibility(bool hide)
        {
            if (_tabContainer != null) _tabContainer.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
            //if (_mainContainer != null) _mainContainer.style.paddingBottom = hide ? 100 : 300;
        }

        private void UpdateValue(float value)
        {
            if (_gridRoot == null) _gridRoot = GameObject.Find("CubeGrid");
            if (_gridRoot == null) return;

            float aspect = (float)Screen.width / Screen.height;
            float orthoHeight = _camera.orthographicSize * 2f;
            float orthoWidth = orthoHeight * aspect;
            
            // Cell dimensions for an 8x8 grid spanning the screen
            float cellWidth = orthoWidth / 8f;
            float cellHeight = orthoHeight / 8f;

            // Cube scale inside cell to achieve 'value' area coverage
            // CubeArea = CellArea * value
            // (CellWidth * s) * (CellHeight * s) = (CellWidth * CellHeight) * value
            // s^2 = value => s = sqrt(value)
            float s = Mathf.Sqrt(value);
            float cubeWidth = cellWidth * s;
            float cubeHeight = cellHeight * s;

            Vector3 newScale = new Vector3(cubeWidth, cubeHeight, 1f);

            foreach (Transform child in _gridRoot.transform)
            {
                if (child.name.StartsWith("HDRCube"))
                {
                    child.localScale = newScale;
                }
            }

            if (_coverageLabel != null)
            {
                _coverageLabel.text = $"Coverage: {value * 100:F1}%";
            }
        }
}
}
