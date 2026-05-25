using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using _Project.UI.Flow.Runtime;

public class GalleryManager : MonoBehaviour
{
    public GalleryConfig gallery;
    public Renderer galleryRenderer; 
    
    private UIFlowRunner _flowRunner;
    private UIDocument _uiDocument;
    
    private Label _descriptionLabel;
    private Label _indexLabel;
    private Button _nextButton;
    private Button _previousButton;
    private Toggle _hdrToggle;
    private Toggle _heatmapToggle;
    
    private int _currentIndex = 0;
    private static readonly int HeatMapProp = Shader.PropertyToID("_HeatMap");
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        _flowRunner = FindFirstObjectByType<UIFlowRunner>();
        if (_flowRunner != null)
        {
            _uiDocument = _flowRunner.GetComponent<UIDocument>();
        }
    }

    private void OnEnable()
    {
        if (_flowRunner != null)
        {
            _flowRunner.OnViewChanged += HandleViewChanged;
            // If the view is already set (e.g. scene just loaded and runner already switched)
            if (_uiDocument != null && _uiDocument.visualTreeAsset != null && _uiDocument.visualTreeAsset.name == "HDRGallery")
            {
                InitializeUI();
            }
        }
    }

    private void OnDisable()
    {
        if (_flowRunner != null)
        {
            _flowRunner.OnViewChanged -= HandleViewChanged;
        }
        UnbindUI();
    }

    private void HandleViewChanged(string viewName)
    {
        if (viewName == "HDRGallery")
        {
            InitializeUI();
        }
        else
        {
            UnbindUI();
        }
    }

    private void InitializeUI()
    {
        if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;

        var root = _uiDocument.rootVisualElement;
        
        _descriptionLabel = root.Q<Label>("Credits");
        _indexLabel = root.Q<Label>("ImageNumber");
        _previousButton = root.Q<Button>("Previous");
        _nextButton = root.Q<Button>("Next");
        _heatmapToggle = root.Q<Toggle>("HeatmapToggle");

        if (_nextButton != null)
        {
            _nextButton.clicked -= NextImage;
            _nextButton.clicked += NextImage;
        }
        if (_previousButton != null)
        {
            _previousButton.clicked -= PrevImage;
            _previousButton.clicked += PrevImage;
        }

        if (_heatmapToggle != null)
        {
            _heatmapToggle.value = Shader.IsKeywordEnabled("_HeatMap");
            _heatmapToggle.UnregisterValueChangedCallback(OnHeatmapToggleChanged);
            _heatmapToggle.RegisterValueChangedCallback(OnHeatmapToggleChanged);
        }

        UpdateGallery();
        }

    private void UnbindUI()
    {
        if (_nextButton != null) _nextButton.clicked -= NextImage;
        if (_previousButton != null) _previousButton.clicked -= PrevImage;
        if (_heatmapToggle != null) _heatmapToggle.UnregisterValueChangedCallback(OnHeatmapToggleChanged);
        
        _nextButton = null;
        _previousButton = null;
        _heatmapToggle = null;
        _descriptionLabel = null;
        _indexLabel = null;
    }

    private void OnHeatmapToggleChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
            Shader.EnableKeyword("_HeatMap");
        else
            Shader.DisableKeyword("_HeatMap");

        if (galleryRenderer != null)
        {
            galleryRenderer.material.SetFloat(HeatMapProp, evt.newValue ? 1f : 0f);
        }
    }

    private void NextImage()
    {
        if (gallery == null || gallery.gallery.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % gallery.gallery.Count;
        UpdateGallery();
    }

    private void PrevImage()
    {
        if (gallery == null || gallery.gallery.Count == 0) return;
        _currentIndex = (_currentIndex - 1 + gallery.gallery.Count) % gallery.gallery.Count;
        UpdateGallery();
    }

    private void UpdateGallery()
    {
        if (gallery == null || _currentIndex < 0 || _currentIndex >= gallery.gallery.Count) return;

        var item = gallery.gallery[_currentIndex];
        if (item.image != null && galleryRenderer != null)
            galleryRenderer.material.SetTexture(BaseMap, item.image);
            
        if (_descriptionLabel != null)
            _descriptionLabel.text = item.description;
            
        if (_indexLabel != null)
            _indexLabel.text = (_currentIndex + 1).ToString();
    }
}
