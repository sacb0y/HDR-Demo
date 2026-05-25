using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;

public class FAQController : MonoBehaviour
{
    [SerializeField] private FAQData faqData;
    [SerializeField] private VisualTreeAsset entryTemplate;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIFlowRunner flowRunner;

    private ScrollView _scrollView;

    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (flowRunner == null) flowRunner = GetComponent<UIFlowRunner>();
    }

    private void OnEnable()
    {
        if (flowRunner != null)
            flowRunner.OnViewChanged += HandleViewChanged;
        
        // Check if we are already on the FAQ view
        if (uiDocument != null && uiDocument.visualTreeAsset != null && uiDocument.visualTreeAsset.name == "FAQ")
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
        // The viewName passed by UIFlowRunner is the name of the VisualTreeAsset
        if (viewName == "FAQ")
        {
            InitializeUI();
        }
    }

    public void InitializeUI()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        _scrollView = uiDocument.rootVisualElement.Q<ScrollView>("faqScrollView");
        if (_scrollView == null) return;

        PopulateFAQ();
    }

    private void PopulateFAQ()
    {
        if (_scrollView == null || faqData == null || entryTemplate == null) return;

        _scrollView.Clear();

        foreach (var entry in faqData.entries)
        {
            var entryInstance = entryTemplate.Instantiate();
            var questionButton = entryInstance.Q<Button>("questionButton");
            var answerContainer = entryInstance.Q<VisualElement>("answerContainer");
            var answerLabel = entryInstance.Q<Label>("answerLabel");

            if (questionButton != null)
            {
                questionButton.text = entry.question;
                questionButton.clicked += () => ToggleAnswer(answerContainer);
            }

            if (answerLabel != null)
            {
                answerLabel.text = entry.answer;
            }

            _scrollView.Add(entryInstance);
        }
    }

    private void ToggleAnswer(VisualElement container)
    {
        if (container == null) return;

        bool isExpanded = container.ClassListContains("expanded");
        if (isExpanded)
        {
            container.RemoveFromClassList("expanded");
        }
        else
        {
            container.AddToClassList("expanded");
        }
    }
}
