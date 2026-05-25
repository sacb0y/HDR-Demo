using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIFlowRunner flowRunner;

    private DropdownField _displayDropdown;
    private Toggle _hdrToggle;
    private Button _applyButton;
    private Button _saveButton;

    private List<DisplayInfo> _displays = new List<DisplayInfo>();

    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (flowRunner == null) flowRunner = GetComponent<UIFlowRunner>();
    }

    private void OnEnable()
    {
        if (flowRunner != null)
            flowRunner.OnViewChanged += HandleViewChanged;
        
        if (uiDocument != null && uiDocument.visualTreeAsset != null && uiDocument.visualTreeAsset.name == "Settings")
        {
            InitializeUI();
        }
    }

    private void OnDisable()
    {
        if (flowRunner != null)
            flowRunner.OnViewChanged -= HandleViewChanged;
    }

    private void HandleViewChanged(string viewName)
    {
        if (viewName == "Settings")
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;
        _displayDropdown = root.Q<DropdownField>("displayDropdown");
        _applyButton = root.Q<Button>("applyButton");
        _saveButton = root.Q<Button>("saveButton");

        SetupDisplayDropdown();
        
        if (_applyButton != null)
        {
            _applyButton.clicked += OnApplyClicked;
        }

        if (_saveButton != null)
        {
            _saveButton.clicked += OnSaveClicked;
        }
    }

    private void SetupDisplayDropdown()
    {
        if (_displayDropdown == null) return;

        _displays.Clear();
        Screen.GetDisplayLayout(_displays);

        List<string> displayNames = new List<string>();
        for (int i = 0; i < _displays.Count; i++)
        {
            displayNames.Add($"{i}: {_displays[i].name} ({_displays[i].width}x{_displays[i].height})");
        }

        _displayDropdown.choices = displayNames;
        int currentIndex = SettingsManager.Instance.CurrentSettings.targetDisplayIndex;
        if (currentIndex >= 0 && currentIndex < displayNames.Count)
        {
            _displayDropdown.index = currentIndex;
        }
        else
        {
            _displayDropdown.index = 0;
        }
    }

    private void OnApplyClicked()
    {
        if (_displayDropdown != null)
        {
            SettingsManager.Instance.CurrentSettings.targetDisplayIndex = _displayDropdown.index;
        }

        SettingsManager.Instance.ApplySettings();
    }

    private void OnSaveClicked()
    {
        OnApplyClicked();
        SettingsManager.Instance.SaveSettings();
    }
}
