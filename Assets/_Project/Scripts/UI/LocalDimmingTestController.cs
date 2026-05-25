using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace _Project.Scripts.UI
{
    public class LocalDimmingTestController : MonoBehaviour
    {
        [Header("Materials")]
        [SerializeField] private Material hdrMaterial;
        [SerializeField] private Material sdrMaterial;
        [SerializeField] private Material starMaterial;

        [Header("Settings")]
        [SerializeField] private int starCount = 300;

        private UIDocument _uiDocument;
        private UIFlowRunner _flowRunner;
        
        private Toggle _hdrToggle;
        private Toggle _hideUIToggle;
        private Slider _speedSlider;
        private SliderInt _starCountSlider;
        private Slider _cubeSizeSlider;
        private Toggle _coloredStarsToggle;
        private Button _reshuffleButton;
        private Button _resetButton;
        private TabView _tabView;
        private VisualElement _tabContainer;

        private GameObject _travelPointRoot;
        private GameObject _starsRoot;
        private GameObject _travelPointCube;
        private List<Renderer> _starRenderers = new List<Renderer>();

        private float _travelProgress = 0f;
        private bool _isInitialized = false;

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
            
            if (_tabView != null)
                _tabView.activeTabChanged -= OnTabChanged;

            if (SettingsManager.Instance != null)
                SettingsManager.Instance.OnSettingsChanged -= UpdateMaterials;
        }

        private void HandleViewChanged(string viewName)
        {
            if (viewName == "LocaDimmingTest")
            {
                InitializeUI();
            }
        }

        private void InitializeUI()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

            var root = _uiDocument.rootVisualElement;
            _hideUIToggle = root.Q<Toggle>("HideUIToggle");
            _speedSlider = root.Q<Slider>("TravelSpeedSlider");
            _starCountSlider = root.Q<SliderInt>("StarCountSlider");
            _cubeSizeSlider = root.Q<Slider>("CubeSizeSlider");
            _coloredStarsToggle = root.Q<Toggle>("ColoredStarsToggle");
            _reshuffleButton = root.Q<Button>("ReshuffleButton");
            _resetButton = root.Q<Button>("ResetButton");
            _tabView = root.Q<TabView>("MainTabView");
            _tabContainer = root.Q<VisualElement>("TabContainer");

            if (_cubeSizeSlider != null)
            {
                _cubeSizeSlider.value = 0.5f;
                _cubeSizeSlider.RegisterValueChangedCallback(evt => {
                    if (_travelPointCube != null) _travelPointCube.transform.localScale = Vector3.one * evt.newValue;
                });
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= UpdateMaterials;
                SettingsManager.Instance.OnSettingsChanged += UpdateMaterials;
            }

            if (_hideUIToggle != null)
            {
                _hideUIToggle.RegisterValueChangedCallback(evt => {
                    if (_tabContainer != null) _tabContainer.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
                });
            }

            if (_speedSlider != null)
            {
                _speedSlider.value = 5f;
            }

            if (_starCountSlider != null)
            {
                _starCountSlider.value = starCount;
                _starCountSlider.RegisterValueChangedCallback(evt => {
                    starCount = evt.newValue;
                    RebuildStars();
                });
            }

            if (_reshuffleButton != null)
            {
                _reshuffleButton.clicked -= ReshuffleStars;
                _reshuffleButton.clicked += ReshuffleStars;
            }

            if (_resetButton != null)
            {
                _resetButton.clicked -= OnResetClicked;
                _resetButton.clicked += OnResetClicked;
            }

            if (_coloredStarsToggle != null)
            {
                _coloredStarsToggle.RegisterValueChangedCallback(_ => ReshuffleStars());
            }

            if (_tabView != null)
            {
                _tabView.activeTabChanged -= OnTabChanged;
                _tabView.activeTabChanged += OnTabChanged;
                
                // Force initial tab to "Travel Point" to ensure only one test runs
                var travelTab = _tabView.Q<Tab>("TravelPointTab");
                if (travelTab != null) _tabView.activeTab = travelTab;
                
                UpdateModeVisibility();
            }

            FindSceneObjects();
            _isInitialized = true;
        }

        private void OnResetClicked()
        {
            _travelProgress = 0f;
            if (_speedSlider != null) _speedSlider.value = 5f;
            if (_cubeSizeSlider != null) _cubeSizeSlider.value = 0.5f;
            if (_starCountSlider != null) _starCountSlider.value = 300;
            if (_coloredStarsToggle != null) _coloredStarsToggle.value = false;
            
            if (_travelPointCube != null) _travelPointCube.transform.localScale = Vector3.one * 0.5f;
            
            RebuildStars();
            UpdateModeVisibility();
        }

        private void FindSceneObjects()
        {
            if (_travelPointRoot == null)
            {
                _travelPointRoot = new GameObject("TravelPoint");
                _travelPointRoot.transform.position = Vector3.zero;
            }
            
            if (_starsRoot == null)
            {
                _starsRoot = new GameObject("Stars");
                _starsRoot.transform.position = Vector3.zero;
            }

            if (_travelPointRoot != null)
            {
                if (_travelPointCube == null)
                {
                    _travelPointCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _travelPointCube.name = "MovingCube";
                    _travelPointCube.transform.SetParent(_travelPointRoot.transform);
                    float initialSize = _cubeSizeSlider != null ? _cubeSizeSlider.value : 0.5f;
                    _travelPointCube.transform.localScale = Vector3.one * initialSize;
                    if (_travelPointCube.TryGetComponent<Collider>(out var c)) DestroyImmediate(c);
                }
                
                foreach (Transform child in _travelPointRoot.transform)
                {
                    if (child.gameObject != _travelPointCube) child.gameObject.SetActive(false);
                }
            }
/*
            if (_starsRoot != null)
            {
                RebuildStars();
            }*/
            
            UpdateMaterials();
        }

        private void RebuildStars()
        {
            if (_starsRoot == null) return;

            foreach (Transform child in _starsRoot.transform)
            {
                Destroy(child.gameObject);
            }
            _starRenderers.Clear();
            for (int i = 0; i < starCount; i++)
            {
                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Cube);
                star.transform.SetParent(_starsRoot.transform);
                if (star.TryGetComponent<Collider>(out var c)) DestroyImmediate(c);
                var r = star.GetComponent<Renderer>();
                if (r != null) _starRenderers.Add(r);
            }

            UpdateMaterials();
            ReshuffleStars();
        }

        private void OnTabChanged(Tab oldTab, Tab newTab)
        {
            UpdateModeVisibility();
        }

        private void UpdateModeVisibility()
        {
            if (_tabView == null) return;
            
            bool isTravel = _tabView.activeTab?.label == "Travel Point";
            bool isStars = _tabView.activeTab?.label == "Stars";

            if (_travelPointRoot != null) _travelPointRoot.SetActive(isTravel);
            if (_starsRoot != null) _starsRoot.SetActive(isStars);
            
            if (isStars && _starRenderers.Count == 0) RebuildStars();
        }

        private void UpdateMaterials()
        {
            Material targetMat = HDROutputSettings.main.active ? hdrMaterial : sdrMaterial;
            if (targetMat == null) targetMat = sdrMaterial;

            if (_travelPointCube != null)
            {
                var r = _travelPointCube.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = targetMat;
            }

            foreach (var r in _starRenderers)
            {
                if (r == null) continue;
                r.sharedMaterial = starMaterial != null ? starMaterial : targetMat;
                if (_coloredStarsToggle != null && !_coloredStarsToggle.value)
                {
                    ApplyStarShaderUserValue(r, Color.white, false);
                }
            }
            
            if (_starsRoot != null && _starsRoot.activeSelf) ReshuffleStars();
        }

        private void ReshuffleStars()
        {
            if (_starsRoot == null) return;
            
            Camera cam = Camera.main;
            if (cam == null) return;

            float orthoHeight = cam.orthographicSize;
            float orthoWidth = orthoHeight * Screen.width / Screen.height;
            bool colored = _coloredStarsToggle != null && _coloredStarsToggle.value;

            float pixelSize = (orthoHeight * 2f) / Screen.height;

            foreach (var r in _starRenderers)
            {
                if (r == null) continue;
                Transform star = r.transform;
                star.localPosition = new Vector3(
                    UnityEngine.Random.Range(-orthoWidth, orthoWidth),
                    UnityEngine.Random.Range(-orthoHeight, orthoHeight),
                    0
                );

                float s = UnityEngine.Random.Range(pixelSize, 0.1f);
                star.localScale = Vector3.one * s;

                if (colored)
                {
                    Color color = UnityEngine.Random.ColorHSV(0, 1, 0.7f, 1, 0.8f, 1);
                    ApplyStarShaderUserValue(r, color, HDROutputSettings.main.active);
                }
                else
                {
                    ApplyStarShaderUserValue(r, Color.white, false);
                }
            }
        }

        private static void ApplyStarShaderUserValue(Renderer renderer, Color color, bool useHdrScale)
        {
            if (renderer is not MeshRenderer meshRenderer) return;

            Color linear = color.linear;
            byte r = (byte)Mathf.RoundToInt(Mathf.Clamp01(linear.r) * 255f);
            byte g = (byte)Mathf.RoundToInt(Mathf.Clamp01(linear.g) * 255f);
            byte b = (byte)Mathf.RoundToInt(Mathf.Clamp01(linear.b) * 255f);
            byte a = useHdrScale ? (byte)255 : (byte)0;

            uint data = ((uint)a << 24) |
                        ((uint)r << 16) |
                        ((uint)g << 8) |
                        b;

            meshRenderer.SetShaderUserValue(data);
        }

        private void Update()
        {
            if (!_isInitialized || _travelPointCube == null || _travelPointRoot == null || !_travelPointRoot.activeSelf) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            float orthoHeight = cam.orthographicSize;
            float orthoWidth = orthoHeight * (float)Screen.width / Screen.height;
            
            float margin = _travelPointCube.transform.localScale.x * 0.5f;
            float w = orthoWidth - margin;
            float h = orthoHeight - margin;

            float speed = _speedSlider != null ? _speedSlider.value : 5f;
            
            float fullW = 2 * w;
            float fullH = 2 * h;
            float total = 2 * (fullW + fullH);

            _travelProgress += (Time.deltaTime * speed) / total;
            if (_travelProgress > 1f) _travelProgress -= 1f;
            
            float dist = _travelProgress * total;
            Vector3 pos = Vector3.zero;

            if (dist < fullW)
                pos = new Vector3(-w + dist, -h, 0);
            else if (dist < fullW + fullH)
                pos = new Vector3(w, -h + (dist - fullW), 0);
            else if (dist < 2 * fullW + fullH)
                pos = new Vector3(w - (dist - (fullW + fullH)), h, 0);
            else
                pos = new Vector3(-w, h - (dist - (2 * fullW + fullH)), 0);

            _travelPointCube.transform.localPosition = pos;
        }
    }
}
