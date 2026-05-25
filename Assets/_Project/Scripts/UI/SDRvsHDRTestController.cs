using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SDRvsHDRTestController : MonoBehaviour
{
    private static readonly int HeatMap = Shader.PropertyToID("_HeatMap");
    private GlobalKeyword _heatmapKeyword;

    [Header("Test Roots")]
    public GameObject sdrVsHdrRoot;
    public GameObject fullGalleryRoot;
    public GameObject differencesRoot;
    public GameObject whiteVsLightRoot;
    public GameObject impossibleColorsRoot;

    private UIDocument _uiDocument;
    private UIFlowRunner _flowRunner;
    
    private VisualElement _comparisonLabels;
    private VisualElement _galleryControls;
    private List<Button> _modeButtons = new List<Button>();
    private List<Button> _formatButtons = new List<Button>();
    private Toggle _heatmapToggle;
    private Transform _fullGalleryOrigin;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) _uiDocument = FindFirstObjectByType<UIDocument>();
        
        _flowRunner = GetComponent<UIFlowRunner>();
        if (_flowRunner == null) _flowRunner = FindFirstObjectByType<UIFlowRunner>();
        
        _heatmapKeyword = new GlobalKeyword("_HeatMap");
    }

    private void OnEnable()
    {
        if (_flowRunner != null)
            _flowRunner.OnViewChanged += HandleViewChanged;
            
        // If already in the right view
        if (_uiDocument != null && _uiDocument.visualTreeAsset != null && _uiDocument.visualTreeAsset.name == "SDRvsHDRColor")
        {
            InitializeUI();
        }
    }

    private void OnDisable()
    {
        if (_flowRunner != null)
            _flowRunner.OnViewChanged -= HandleViewChanged;
        
        UnbindUI();
    }

    private void HandleViewChanged(string viewName)
    {
        if (viewName == "SDRvsHDRColor")
        {
            InitializeUI();
        }
        else
        {
            UnbindUI();
        }
    }

    private void UnbindUI()
    {
        if (_heatmapToggle != null)
        {
            _heatmapToggle.UnregisterValueChangedCallback(OnHeatmapToggleChanged);
        }
        _heatmapToggle = null;
        _modeButtons.Clear();
        _formatButtons.Clear();
    }

    private void InitializeUI()
    {
        if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

        UnbindUI();

        var root = _uiDocument.rootVisualElement;
        _comparisonLabels = root.Q<VisualElement>("ComparisonLabels");
        _galleryControls = root.Q<VisualElement>("GalleryControls");

        for (int i = 0; i < 5; i++)
        {
            var btn = root.Q<Button>($"Mode{i}");
            if (btn != null)
            {
                int index = i;
                btn.clicked += () => SetMode(index);
                _modeButtons.Add(btn);
            }
        }

        var directSelector = root.Q<VisualElement>("DirectSelector");
        if (directSelector != null)
        {
            for (int i = 0; i < 4; i++)
            {
                var btn = directSelector.Q<Button>($"Image{i}");
                if (btn != null)
                {
                    int index = i;
                    btn.clicked += () => SetFormat(index);
                    _formatButtons.Add(btn);
                }
            }
        }

        if (fullGalleryRoot != null)
        {
            _fullGalleryOrigin = fullGalleryRoot.transform.Find("Origin");
        }

        _heatmapToggle = root.Q<Toggle>("HeatmapToggle");
        if (_heatmapToggle != null)
        {
            _heatmapToggle.value = Shader.IsKeywordEnabled(_heatmapKeyword);
            _heatmapToggle.RegisterValueChangedCallback(OnHeatmapToggleChanged);
        }

        // Default to mode 0 (SDR vs HDR)
        SetMode(0);
    }

    private void OnHeatmapToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
            Shader.SetGlobalFloat(HeatMap, 1);
        else
            Shader.SetGlobalFloat(HeatMap, 0);
    }

    public void SetMode(int index)
    {
        if (sdrVsHdrRoot != null) sdrVsHdrRoot.SetActive(index == 0);
        if (fullGalleryRoot != null) fullGalleryRoot.SetActive(index == 1);
        if (differencesRoot != null) differencesRoot.SetActive(index == 2);
        if (whiteVsLightRoot != null) whiteVsLightRoot.SetActive(index == 3);
        if (impossibleColorsRoot != null) impossibleColorsRoot.SetActive(index == 4);

        if (_comparisonLabels != null)
            _comparisonLabels.style.display = (index == 0) ? DisplayStyle.Flex : DisplayStyle.None;

        if (_galleryControls != null)
            _galleryControls.style.display = (index == 1) ? DisplayStyle.Flex : DisplayStyle.None;

        // Update button styles
        for (int i = 0; i < _modeButtons.Count; i++)
        {
            if (i == index)
                _modeButtons[i].AddToClassList("mode-button--active");
            else
                _modeButtons[i].RemoveFromClassList("mode-button--active");
        }

        if (index == 1) SetFormat(3); // Default to Native HDR in gallery mode
    }

    private void SetFormat(int index)
    {
        if (_fullGalleryOrigin == null) return;

        for (int i = 0; i < _fullGalleryOrigin.childCount; i++)
        {
            _fullGalleryOrigin.GetChild(i).gameObject.SetActive(i == index);
        }

        for (int i = 0; i < _formatButtons.Count; i++)
        {
            if (i == index)
                _formatButtons[i].AddToClassList("image-toggle--active");
            else
                _formatButtons[i].RemoveFromClassList("image-toggle--active");
        }
    }
}
