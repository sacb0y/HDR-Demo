using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;
using UnityEngine.Serialization;
using UnityEngine.Rendering.Universal;

namespace _Project.Scripts.UI
{
    public class ColorBandingTestController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private UIFlowRunner flowRunner;
        
        [SerializeField] private GameObject gradients;
        [SerializeField] private GameObject hdrScreenshot;
        [SerializeField] private GameObject sdrGradients;
        [SerializeField] private GameObject hdrGradients;
        [SerializeField] private GameObject sdrDarkness;
        [SerializeField] private GameObject hdrDarkness;
        private VisualElement _markersContainer;
        private Toggle _markersToggle;
        private Toggle _hdrContentToggle;
        private TabView _tabView;

        private void Start()
        {
            Debug.Log("[ColorBandingTestController] Start");
            if (uiDocument == null) uiDocument = Object.FindAnyObjectByType<UIDocument>();
            if (flowRunner == null) flowRunner = Object.FindAnyObjectByType<UIFlowRunner>();
            
            // Try to initialize if we have a document, regardless of runner
            if (uiDocument != null && uiDocument.visualTreeAsset != null)
            {
                Debug.Log($"[ColorBandingTestController] Current UI Asset: {uiDocument.visualTreeAsset.name}");
                if (uiDocument.visualTreeAsset.name == "ColorBandingTest")
                {
                    InitializeUI();
                }
            }
            else
            {
                Debug.LogWarning("[ColorBandingTestController] No UIDocument or VisualTreeAsset found in Start");
            }
        }

        private void OnEnable()
        {
            if (flowRunner == null) flowRunner = Object.FindAnyObjectByType<UIFlowRunner>();
            if (flowRunner != null)
                flowRunner.OnViewChanged += HandleViewChanged;
        }

        private void OnDisable()
        {
            if (flowRunner != null)
                flowRunner.OnViewChanged -= HandleViewChanged;
        }

        private void HandleViewChanged(string viewName)
        {
            if (viewName == "ColorBandingTest")
            {
                InitializeUI();
            }
        }

        private void InitializeUI()
        {
            if (uiDocument == null) uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument?.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[ColorBandingTestController] Root visual element is null in InitializeUI");
                return;
            }

            Debug.Log("[ColorBandingTestController] Initializing UI");
            
            // Debug click handling to see what is blocking clicks
            root.UnregisterCallback<PointerDownEvent>(OnRootClicked, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerDownEvent>(OnRootClicked, TrickleDown.TrickleDown);
            
            Debug.Log($"[ColorBandingTestController] Gradients: {gradients != null}, HDRScreenshot: {hdrScreenshot != null}");

            // Setup Content Toggle
            _hdrContentToggle = root.Q<Toggle>("HDRContentToggle");
            if (_hdrContentToggle != null)
            {
                _hdrContentToggle.RegisterValueChangedCallback(evt => {
                    Debug.Log($"[ColorBandingTestController] Content HDR Toggle: {evt.newValue}");
                    UpdateSceneVisibility(_tabView?.activeTab?.label);
                });
            }

            _markersContainer = root.Q<VisualElement>("MarkersContainer");
            _markersToggle = root.Q<Toggle>("MarkersToggle");
            if (_markersToggle != null)
            {
                _markersToggle.value = false;
                if (_markersContainer != null) _markersContainer.style.display = DisplayStyle.None;
                
                _markersToggle.RegisterValueChangedCallback(evt => {
                    Debug.Log($"[ColorBandingTestController] Markers Toggle: {evt.newValue}");
                    if (_markersContainer != null)
                        _markersContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                });
            }

            // Tabs Toggle
            var tabsToggle = root.Q<Toggle>("TabsToggle");
            var tabViewContainer = root.Q<VisualElement>("TabViewContainer");
            if (tabsToggle != null && tabViewContainer != null)
            {
                tabsToggle.RegisterValueChangedCallback(evt => {
                    Debug.Log($"[ColorBandingTestController] Tabs Toggle: {evt.newValue}");
                    tabViewContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                });
            }

            // Dithering Toggle
            var ditheringToggle = root.Q<Toggle>("DitheringToggle");
            if (ditheringToggle != null)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                    if (camData != null)
                    {
                        ditheringToggle.value = camData.dithering;
                        ditheringToggle.RegisterValueChangedCallback(evt => {
                            Debug.Log($"[ColorBandingTestController] Dithering Toggle: {evt.newValue}");
                            camData.dithering = evt.newValue;
                        });
                    }
                }
            }

            // Setup TabView
            _tabView = root.Q<TabView>("MainTabView");
            if (_tabView != null)
            {
                Debug.Log("[ColorBandingTestController] Found MainTabView, binding event");
                _tabView.activeTabChanged -= OnTabChanged;
                _tabView.activeTabChanged += OnTabChanged;
                
                // Ensure headers are clickable
                for (int i = 0; i < 4; i++)
                {
                    var header = _tabView.GetTabHeader(i);
                    if (header != null) header.pickingMode = PickingMode.Position;
                }

                // Initial state
                UpdateSceneVisibility(_tabView.activeTab?.label ?? "Gradient Demo");
            }
            else
            {
                Debug.LogWarning("[ColorBandingTestController] MainTabView NOT found in UXML!");
            }
        }

        private void OnRootClicked(PointerDownEvent evt)
        {
            var target = evt.target as VisualElement;
            Debug.Log($"[ColorBandingTestController] Pointer Down on: {target?.name} (Type: {target?.GetType().Name}, PickingMode: {target?.pickingMode})");
        }

        private void OnTabChanged(Tab oldTab, Tab newTab)
        {
            Debug.Log($"[ColorBandingTestController] Tab Changed: '{oldTab?.label}' -> '{newTab?.label}'");
            UpdateSceneVisibility(newTab?.label);
        }

        private void UpdateSceneVisibility(string activeTabLabel)
        {
            Debug.Log($"[ColorBandingTestController] UpdateSceneVisibility: {activeTabLabel}");
            bool isGradient = activeTabLabel == "Gradient Demo";
            bool isScreenshot = activeTabLabel == "Screenshot Demo";

            bool useHDRContent = _hdrContentToggle?.value ?? true;

            // Requirement: Opinion and Settings tabs shouldn't toggle the test objects, 
            // those can just keep whatever is currently visible.
            if (isGradient)
            {
                if (gradients != null) gradients.SetActive(true);
                if (hdrScreenshot != null) hdrScreenshot.SetActive(false);
                
                if (sdrGradients != null) sdrGradients.SetActive(!useHDRContent);
                if (hdrGradients != null) hdrGradients.SetActive(useHDRContent);
            }
            else if (isScreenshot)
            {
                if (gradients != null) gradients.SetActive(false);
                if (hdrScreenshot != null) hdrScreenshot.SetActive(true);
                
                if (sdrDarkness != null) sdrDarkness.SetActive(!useHDRContent);
                if (hdrDarkness != null) hdrDarkness.SetActive(useHDRContent);
            }

            // Requirement: "Markers" toggle should only be visible when screenshot demo is enabled.
            if (_markersToggle != null)
            {
                _markersToggle.style.display = isScreenshot ? DisplayStyle.Flex : DisplayStyle.None;
                
                // Also hide markers themselves if we are leaving screenshot demo
                if (!isScreenshot && _markersContainer != null)
                {
                    _markersContainer.style.display = DisplayStyle.None;
                    _markersToggle.value = false;
                }
            }
        }
    }
}

