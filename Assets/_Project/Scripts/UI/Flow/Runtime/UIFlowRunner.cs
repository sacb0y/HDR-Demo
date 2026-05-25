using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace _Project.UI.Flow.Runtime
{
    /// <summary>
    /// Executes a <see cref="UIFlowRuntimeGraph"/> at runtime, managing UI transitions and scene changes.
    /// This component is typically marked as DontDestroyOnLoad to handle flow across scenes.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class UIFlowRunner : MonoBehaviour
    {
        private const string BackButtonName = "BackButton";

        /// <summary>
        /// The runtime graph asset to execute.
        /// </summary>
        public UIFlowRuntimeGraph graph;

        private UIDocument _uiDocument;
        private UIFlowRuntimeNode _currentNode;
        private Stack<UIFlowRuntimeNode> _history = new Stack<UIFlowRuntimeNode>();
        private bool _sequenceWaitingForButton;
        private Coroutine _autoNextCoroutine;
        private readonly List<BoundButton> _boundButtons = new List<BoundButton>();

        /// <summary>
        /// Event triggered when the active UI view changes.
        /// Passes the name of the new <see cref="VisualTreeAsset"/>.
        /// </summary>
        public event Action<string> OnViewChanged;

        private static UIFlowRunner _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _uiDocument = GetComponent<UIDocument>();

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (_instance != this) return;

            if (graph != null && graph.startNodeIndex != -1)
            {
                var startNode = graph.GetNode(graph.startNodeIndex);
                ExecuteNode(startNode);
            }
        }

        /// <summary>
        /// Executes a specific node in the graph.
        /// </summary>
        /// <param name="node">The runtime node to execute.</param>
        private void ExecuteNode(UIFlowRuntimeNode node)
        {
            if (node == null) return;

            StopAutoNext();
            UnbindCurrentButtons();
            _currentNode = node;

            switch (node)
            {
                case StartRuntimeNode:
                    TriggerNext("Next", node);
                    break;
                case ViewRuntimeNode viewNode:
                {
                    if (viewNode.clearHistory) _history.Clear();
                    StartCoroutine(ShowViewCoroutine(viewNode));
                    break;
                }
                case UISequenceRuntimeNode sequenceNode:
                    StartCoroutine(ShowSequenceCoroutine(sequenceNode));
                    break;
                case LoadSceneRuntimeNode sceneNode:
                    StartCoroutine(LoadSceneCoroutine(sceneNode));
                    break;
                case QuitRuntimeNode:
                    Debug.Log("[UIFlowRunner] Quit node reached. Terminating application.");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }

        private IEnumerator LoadSceneCoroutine(LoadSceneRuntimeNode sceneNode)
        {
            if (string.IsNullOrWhiteSpace(sceneNode.sceneName))
            {
                Debug.LogWarning(
                    $"[UIFlowRunner] LoadSceneNode in '{sceneNode.nodeName}' has no scene name specified. Skipping load.");
                yield return null;
                TriggerNext("Next", sceneNode);
                yield break;
            }

            if (sceneNode.preventLoadingSameScene && SceneManager.GetActiveScene().name == sceneNode.sceneName)
            {
                TriggerNext("Next", sceneNode);
                yield break;
            }

            if (sceneNode.waitForSceneToLoad)
            {
                var op = SceneManager.LoadSceneAsync(sceneNode.sceneName, sceneNode.loadSceneMode);
                if (op == null)
                {
                    Debug.LogError(
                        $"[UIFlowRunner] Failed to start loading scene '{sceneNode.sceneName}'. Ensure it is added to Build Settings.");
                    TriggerNext("Next", sceneNode);
                    yield break;
                }

                op.allowSceneActivation = sceneNode.allowSceneActivation;

                if (!sceneNode.allowSceneActivation)
                {
                    while (op.progress < 0.9f)
                    {
                        yield return null;
                    }

                    if (sceneNode.sceneActivationDelay > 0)
                        yield return new WaitForSeconds(sceneNode.sceneActivationDelay);

                    op.allowSceneActivation = true;
                }

                while (!op.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                SceneManager.LoadScene(sceneNode.sceneName, sceneNode.loadSceneMode);
            }

            yield return null;
            TriggerNext("Next", sceneNode);
        }

        private IEnumerator ShowViewCoroutine(ViewRuntimeNode viewNode)
        {
            if (viewNode.showDelay > 0)
            {
                _uiDocument.visualTreeAsset = null;
                yield return new WaitForSeconds(viewNode.showDelay);
            }

            if (viewNode.viewAsset != null)
            {
                _uiDocument.visualTreeAsset = viewNode.viewAsset;
                var root = _uiDocument.rootVisualElement;
                BindDeclaredViewButtons(viewNode, root);

                if (viewNode.autoNext && HasLink(viewNode, "Next"))
                {
                    _autoNextCoroutine = StartCoroutine(AutoNextCoroutine(viewNode, viewNode.waitSeconds));
                }

                OnViewChanged?.Invoke(viewNode.viewAsset.name);
            }
        }

        private IEnumerator ShowSequenceCoroutine(UISequenceRuntimeNode sequenceNode)
        {
            _sequenceWaitingForButton = false;

            if (sequenceNode == null || sequenceNode.items == null || sequenceNode.items.Count == 0)
            {
                TriggerNext("Next", sequenceNode);
                yield break;
            }

            foreach (var item in sequenceNode.items)
            {
                if (item == null)
                    continue;

                if (item.type == UISequenceItemType.Wait)
                {
                    if (item.waitSeconds > 0)
                        yield return new WaitForSeconds(item.waitSeconds);
                    continue;
                }

                if (item.viewAsset == null)
                    continue;

                if (item.type == UISequenceItemType.ShowUI)
                {
                    _uiDocument.visualTreeAsset = item.viewAsset;
                    OnViewChanged?.Invoke(item.viewAsset.name);
                    continue;
                }

                if (item.type == UISequenceItemType.AddUI)
                {
                    var root = _uiDocument.rootVisualElement;
                    item.viewAsset.CloneTree(root);
                    OnViewChanged?.Invoke(item.viewAsset.name);
                }
            }

            if (sequenceNode.buttonNames != null && sequenceNode.buttonNames.Count > 0)
            {
                var root = _uiDocument.rootVisualElement;
                var hasBoundButton = false;
                _sequenceWaitingForButton = true;
                var boundNames = new HashSet<string>(StringComparer.Ordinal);

                for (var buttonIndex = 0; buttonIndex < sequenceNode.buttonNames.Count; buttonIndex++)
                {
                    var buttonName = sequenceNode.buttonNames[buttonIndex];
                    if (string.IsNullOrWhiteSpace(buttonName))
                        continue;

                    if (!boundNames.Add(buttonName))
                    {
                        Debug.LogWarning(
                            $"[UIFlowRunner] Duplicate sequence button binding '{buttonName}' on node '{sequenceNode.nodeName}'. Keeping the first mapping.");
                        continue;
                    }

                    var proceedButton = FindButton(root, buttonName);
                    if (proceedButton == null)
                    {
                        Debug.LogWarning(
                            $"[UIFlowRunner] Sequence button '{buttonName}' was configured but not found in '{_uiDocument.visualTreeAsset?.name}' (checked Name and Binding Path).");
                        continue;
                    }

                    var targetPort = $"Button{buttonIndex}";
                    if (!HasLink(sequenceNode, targetPort))
                    {
                        Debug.LogWarning(
                            $"[UIFlowRunner] Sequence button '{buttonName}' maps to missing port '{targetPort}' on node '{sequenceNode.nodeName}'.");
                    }

                    hasBoundButton = true;
                    var capturedPort = targetPort;
                    Action onClick = () =>
                    {
                        if (!_sequenceWaitingForButton)
                            return;

                        _sequenceWaitingForButton = false;

                        if (string.Equals(buttonName, BackButtonName, StringComparison.Ordinal) &&
                            !HasLink(sequenceNode, capturedPort))
                        {
                            GoBack();
                        }
                        else
                        {
                            TriggerNext(capturedPort, sequenceNode);
                        }
                    };
                    proceedButton.clicked += onClick;
                    _boundButtons.Add(new BoundButton(proceedButton, onClick));
                }

                if (hasBoundButton)
                {
                    yield break;
                }

                _sequenceWaitingForButton = false;
            }

            if (sequenceNode.nextOnComplete)
            {
                TriggerNext("Next", sequenceNode);
            }
            else
            {
                Debug.LogWarning(
                    $"[UIFlowRunner] Sequence node '{sequenceNode.nodeName}' completed without bound buttons and NextOnComplete is disabled.");
            }
        }

        private void BindDeclaredViewButtons(ViewRuntimeNode viewNode, VisualElement root)
        {
            var buttonNames = viewNode.buttonNames ?? new List<string>();
            var boundNames = new HashSet<string>(StringComparer.Ordinal);

            for (var buttonIndex = 0; buttonIndex < buttonNames.Count; buttonIndex++)
            {
                var buttonName = buttonNames[buttonIndex];
                if (string.IsNullOrWhiteSpace(buttonName))
                    continue;

                if (!boundNames.Add(buttonName))
                {
                    Debug.LogWarning(
                        $"[UIFlowRunner] Duplicate view button binding '{buttonName}' on node '{viewNode.nodeName}'. Keeping the first mapping.");
                    continue;
                }

                var button = FindButton(root, buttonName);
                if (button == null)
                {
                    Debug.LogWarning(
                        $"[UIFlowRunner] View button '{buttonName}' was configured but not found in '{viewNode.viewAsset?.name}' (checked Name and Binding Path).");
                    continue;
                }

                var targetPort = $"Button{buttonIndex}";
                if (!HasLink(viewNode, targetPort) &&
                    !string.Equals(buttonName, BackButtonName, StringComparison.Ordinal))
                {
                    Debug.LogWarning(
                        $"[UIFlowRunner] View button '{buttonName}' maps to missing port '{targetPort}' on node '{viewNode.nodeName}'.");
                }

                Action onClick = () =>
                {
                    if (string.Equals(buttonName, BackButtonName, StringComparison.Ordinal))
                    {
                        if (HasLink(viewNode, targetPort))
                        {
                            TriggerNext(targetPort, viewNode);
                        }
                        else
                        {
                            GoBack();
                        }

                        return;
                    }

                    _history.Push(viewNode);
                    TriggerNext(targetPort, viewNode);
                };

                button.clicked += onClick;
                _boundButtons.Add(new BoundButton(button, onClick));
            }

            if (!boundNames.Contains(BackButtonName))
            {
                BindFallbackBackButton(root);
            }
        }

        private Button FindButton(VisualElement root, string id)
        {
            if (root == null || string.IsNullOrWhiteSpace(id))
                return null;

            // 1. Try finding by Name
            var buttonById = root.Q<Button>(id);
            if (buttonById != null) return buttonById;

            // 2. Try finding by Binding Path
            return root.Query<Button>().Where(b => string.Equals(b.bindingPath, id, StringComparison.Ordinal)).First();
        }

        private void BindFallbackBackButton(VisualElement root)
        {
            var backButton = FindButton(root, BackButtonName);
            if (backButton == null)
                return;

            // Check if this button is already bound by an explicit link in BindDeclaredViewButtons
            if (_boundButtons.Any(b => b.Button == backButton))
                return;

            Action onClick = GoBack;
            backButton.clicked += onClick;
            _boundButtons.Add(new BoundButton(backButton, onClick));
        }

        private static bool HasLink(UIFlowRuntimeNode node, string portName)
        {
            return node.links != null &&
                   node.links.Any(link => string.Equals(link.portName, portName, StringComparison.Ordinal));
        }

        private void UnbindCurrentButtons()
        {
            foreach (var bound in _boundButtons)
            {
                if (bound.Button != null)
                {
                    bound.Button.clicked -= bound.Callback;
                }
            }

            _boundButtons.Clear();
            _sequenceWaitingForButton = false;
        }

        private IEnumerator AutoNextCoroutine(ViewRuntimeNode viewNode, float waitSeconds)
        {
            if (waitSeconds > 0f)
            {
                yield return new WaitForSeconds(waitSeconds);
            }

            TriggerNext("Next", viewNode);
        }

        private void StopAutoNext()
        {
            if (_autoNextCoroutine == null)
                return;

            StopCoroutine(_autoNextCoroutine);
            _autoNextCoroutine = null;
        }

        /// <summary>
        /// Navigates back to the previous <see cref="ViewRuntimeNode"/> in the history.
        /// </summary>
        public void GoBack()
        {
            if (_currentNode is ViewRuntimeNode viewNode && viewNode.hideOnNext)
            {
                _uiDocument.visualTreeAsset = null;
            }

            if (_history.Count > 0)
            {
                var prev = _history.Pop();
                ExecuteNode(prev);
            }
        }

        /// <summary>
        /// Triggers a transition from a specific port on a node.
        /// </summary>
        /// <param name="portName">The name of the port to follow.</param>
        /// <param name="fromNode">The node to transition from. If null, uses the current node.</param>
        public void TriggerNext(string portName, UIFlowRuntimeNode fromNode = null)
        {
            var targetNode = fromNode ?? _currentNode;
            if (targetNode == null) return;

            if (targetNode is ViewRuntimeNode viewNode && viewNode.hideOnNext)
            {
                _uiDocument.visualTreeAsset = null;
            }

            var link = targetNode.links.Find(l => l.portName == portName);
            if (link != null)
            {
                var next = graph.GetNode(link.targetNodeIndex);
                ExecuteNode(next);
            }
            else
            {
                Debug.LogWarning(
                    $"[UIFlowRunner] Missing link from node '{targetNode.nodeName}' for port '{portName}'.");
            }
        }

        private readonly struct BoundButton
        {
            public BoundButton(Button button, Action callback)
            {
                Button = button;
                Callback = callback;
            }

            public Button Button { get; }
            public Action Callback { get; }
        }
    }
}