using UnityEngine;
using UnityEngine.UIElements;
using _Project.Scripts.Data;
using _Project.UI.Flow.Runtime;
using System.Collections;
using System.Collections.Generic;

namespace _Project.Scripts.UI
{
    public class CreditsController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CreditsData creditsData;
        
        [Header("Settings")]
        [SerializeField] private float scrollSpeed = 50f;
        [SerializeField] private bool autoScroll = true;
        [SerializeField] private float finishDelay = 2f;
        [SerializeField] private bool autoReturn = true;

        private UIDocument _uiDocument;
        private UIFlowRunner _flowRunner;
        private ScrollView _scrollView;
        private VisualElement _creditsContent;
        private bool _isScrolling;
        private bool _hasFinished;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _flowRunner = GetComponent<UIFlowRunner>();
        }

        private void OnEnable()
        {
            if (_flowRunner != null)
                _flowRunner.OnViewChanged += HandleViewChanged;
            
            // Check if already active
            if (_uiDocument.visualTreeAsset != null && _uiDocument.visualTreeAsset.name == "Credits")
            {
                InitializeUI();
            }
        }

        private void OnDisable()
        {
            if (_flowRunner != null)
                _flowRunner.OnViewChanged -= HandleViewChanged;
            
            _isScrolling = false;
        }

        private void HandleViewChanged(string viewName)
        {
            if (viewName == "Credits")
            {
                InitializeUI();
            }
            else
            {
                _isScrolling = false;
            }
        }

        private void InitializeUI()
        {
            var root = _uiDocument.rootVisualElement;
            if (root == null) return;

            // Setup Title
            var titleLabel = root.Q<Label>("creditsTitle");
            if (titleLabel != null && creditsData != null)
                titleLabel.text = creditsData.title;

            // Setup ScrollView
            var container = root.Q<VisualElement>("creditsContainer");
            if (container != null)
            {
                container.Clear();
                _scrollView = new ScrollView();
                _scrollView.style.flexGrow = 1;
                _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                _scrollView.mouseWheelScrollSize = 0;
                container.Add(_scrollView);

                _creditsContent = new VisualElement();
                _creditsContent.name = "creditsContent";
                _scrollView.Add(_creditsContent);

                PopulateCredits();
                
                // Reset scroll
                _scrollView.scrollOffset = Vector2.zero;
                _isScrolling = autoScroll;
                _hasFinished = false;
            }
        }

        private void PopulateCredits()
        {
            if (creditsData == null || _creditsContent == null) return;

            // Add top padding
            var topSpacer = new VisualElement();
            topSpacer.style.height = 300;
            _creditsContent.Add(topSpacer);

            foreach (var section in creditsData.sections)
            {
                var sectionContainer = new VisualElement();
                sectionContainer.AddToClassList("section-container");

                if (!string.IsNullOrEmpty(section.sectionTitle))
                {
                    var sectionTitle = new Label(section.sectionTitle);
                    sectionTitle.AddToClassList("section-title");
                    sectionContainer.Add(sectionTitle);
                }

                foreach (var entry in section.entries)
                {
                    var entryLabel = new Label(entry);
                    entryLabel.AddToClassList("credit-entry");
                    sectionContainer.Add(entryLabel);
                }

                _creditsContent.Add(sectionContainer);
            }
            
            // Add bottom padding
            var bottomSpacer = new VisualElement();
            bottomSpacer.style.height = 1000;
            _creditsContent.Add(bottomSpacer);
        }

        private void Update()
        {
            if (_isScrolling && _scrollView != null)
            {
                Vector2 offset = _scrollView.scrollOffset;
                float oldY = offset.y;
                offset.y += scrollSpeed * Time.deltaTime;
                _scrollView.scrollOffset = offset;

                // Check if reached end (offset stops increasing)
                if (!_hasFinished && offset.y > 0 && Mathf.Approximately(offset.y, oldY))
                {
                    _hasFinished = true;
                    if (autoReturn)
                    {
                        StartCoroutine(FinishCreditsRoutine());
                    }
                }
            }
        }

        private IEnumerator FinishCreditsRoutine()
        {
            yield return new WaitForSeconds(finishDelay);
            if (_flowRunner != null)
            {
                _flowRunner.GoBack();
            }
        }
    }
}


