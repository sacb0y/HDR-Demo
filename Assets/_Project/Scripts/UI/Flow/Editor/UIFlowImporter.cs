using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using _Project.UI.Flow.Runtime;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// Custom asset importer that converts <see cref="UIFlowGraph"/> assets into <see cref="UIFlowRuntimeGraph"/> assets.
    /// </summary>
    [ScriptedImporter(1, UIFlowGraph.AssetExtension)]
    internal class UIFlowImporter : ScriptedImporter
    {
        /// <summary>
        /// Called by Unity when a .uiflow file is imported or changed.
        /// </summary>
        /// <param name="ctx">The import context.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var editorGraph = GraphDatabase.LoadGraphForImporter<UIFlowGraph>(ctx.assetPath);
            if (editorGraph == null) return;

            var runtimeGraph = ScriptableObject.CreateInstance<UIFlowRuntimeGraph>();
            var editorNodes = editorGraph.GetNodes().ToList();

            for (int i = 0; i < editorNodes.Count; i++)
            {
                var nodeModel = editorNodes[i];
                UIFlowRuntimeNode runtimeNode = null;
                switch (nodeModel)
                {
                    case StartNode _:
                        runtimeNode = new StartRuntimeNode();
                        runtimeGraph.startNodeIndex = i;
                        break;
                    case ViewNode view:
                        var buttonCount = GetPortOrOptionValue<int>(view, ViewNode.BUTTON_COUNT_OPTION_NAME, 0);
                        var viewAsset =
                            GetPortOrOptionValue<VisualTreeAsset>(view, ViewNode.VIEW_ASSET_OPTION_NAME,
                                view.viewAsset);
                        var showDelay =
                            GetPortOrOptionValue<float>(view, ViewNode.SHOW_DELAY_OPTION_NAME, view.showDelay);
                        var autoNext = GetPortOrOptionValue<bool>(view, ViewNode.AUTO_NEXT_OPTION_NAME, view.autoNext);
                        var waitSeconds =
                            GetPortOrOptionValue<float>(view, ViewNode.WAIT_SECONDS_OPTION_NAME, view.waitSeconds);
                        var clearHistory = GetPortOrOptionValue<bool>(view, ViewNode.CLEAR_HISTORY_OPTION_NAME,
                            view.clearHistory);
                        var hideOnNext =
                            GetPortOrOptionValue<bool>(view, ViewNode.HIDE_ON_NEXT_OPTION_NAME, view.hideOnNext);

                        var viewButtonNames = new List<string>(buttonCount);
                        for (var buttonIndex = 0; buttonIndex < buttonCount; buttonIndex++)
                        {
                            var optionName = UIFlowUtilities.GetButtonNameOptionName(buttonIndex);
                            var buttonName = GetPortOrOptionValue<string>(view, optionName, null);
                            viewButtonNames.Add(buttonName?.Trim());
                        }

                        runtimeNode = new ViewRuntimeNode
                        {
                            viewAsset = viewAsset,
                            showDelay = showDelay,
                            autoNext = autoNext,
                            waitSeconds = Mathf.Max(0f, waitSeconds),
                            clearHistory = clearHistory,
                            hideOnNext = hideOnNext
                        };
                        ((ViewRuntimeNode)runtimeNode).buttonNames = viewButtonNames;
                        break;
                    case UISequence sequenceContext:
                        var sequenceRuntimeNode = new UISequenceRuntimeNode();
                        sequenceRuntimeNode.nextOnComplete = GetPortOrOptionValue<bool>(sequenceContext,
                            UISequence.NEXT_ON_COMPLETE_OPTION_NAME, sequenceContext.nextOnComplete);

                        var sequenceButtonCount =
                            GetPortOrOptionValue<int>(sequenceContext, UISequence.BUTTON_COUNT_OPTION_NAME, 0);
                        for (var buttonIndex = 0; buttonIndex < sequenceButtonCount; buttonIndex++)
                        {
                            var optionName = UIFlowUtilities.GetButtonNameOptionName(buttonIndex);
                            var buttonName = GetPortOrOptionValue<string>(sequenceContext, optionName, null);
                            sequenceRuntimeNode.buttonNames.Add(buttonName?.Trim());
                        }

                        foreach (var block in sequenceContext.blockNodes)
                        {
                            switch (block)
                            {
                                case ShowUI showUIBlock:
                                    var showViewAsset = GetPortOrOptionValue<VisualTreeAsset>(showUIBlock,
                                        ShowUI.VIEW_ASSET_OPTION_NAME, showUIBlock.viewAsset);

                                    sequenceRuntimeNode.items.Add(new UISequenceRuntimeItem
                                    {
                                        type = UISequenceItemType.ShowUI,
                                        viewAsset = showViewAsset
                                    });
                                    break;
                                case AddUI addUIBlock:
                                    var addViewAsset = GetPortOrOptionValue<VisualTreeAsset>(addUIBlock,
                                        AddUI.VIEW_ASSET_OPTION_NAME, addUIBlock.viewAsset);

                                    sequenceRuntimeNode.items.Add(new UISequenceRuntimeItem
                                    {
                                        type = UISequenceItemType.AddUI,
                                        viewAsset = addViewAsset
                                    });
                                    break;
                                case WaitTime waitTimeBlock:
                                    var waitDuration = GetPortOrOptionValue<float>(waitTimeBlock,
                                        WaitTime.WAIT_SECONDS_OPTION_NAME, waitTimeBlock.waitSeconds);

                                    sequenceRuntimeNode.items.Add(new UISequenceRuntimeItem
                                    {
                                        type = UISequenceItemType.Wait,
                                        waitSeconds = Mathf.Max(0f, waitDuration)
                                    });
                                    break;
                            }
                        }

                        runtimeNode = sequenceRuntimeNode;
                        break;
                    case LoadSceneNode scene:
                        var sceneName = GetPortOrOptionValue<string>(scene, LoadSceneNode.SCENE_NAME_OPTION_NAME,
                            scene.sceneName);
                        var preventLoadingSameScene = GetPortOrOptionValue<bool>(scene,
                            LoadSceneNode.PREVENT_LOADING_SAME_SCENE_OPTION_NAME, scene.preventLoadingSameScene);
                        var loadSceneMode = GetPortOrOptionValue<LoadSceneMode>(scene,
                            LoadSceneNode.LOAD_SCENE_MODE_OPTION_NAME, scene.loadSceneMode);
                        var allowSceneActivation = GetPortOrOptionValue<bool>(scene,
                            LoadSceneNode.ALLOW_SCENE_ACTIVATION_OPTION_NAME, scene.allowSceneActivation);
                        var sceneActivationDelay = GetPortOrOptionValue<float>(scene,
                            LoadSceneNode.SCENE_ACTIVATION_DELAY_OPTION_NAME, scene.sceneActivationDelay);
                        var waitForSceneToLoad = GetPortOrOptionValue<bool>(scene,
                            LoadSceneNode.WAIT_FOR_SCENE_TO_LOAD_OPTION_NAME, scene.waitForSceneToLoad);

                        runtimeNode = new LoadSceneRuntimeNode
                        {
                            sceneName = sceneName,
                            preventLoadingSameScene = preventLoadingSameScene,
                            loadSceneMode = loadSceneMode,
                            allowSceneActivation = allowSceneActivation,
                            sceneActivationDelay = sceneActivationDelay,
                            waitForSceneToLoad = waitForSceneToLoad
                        };
                        break;
                    case QuitNode _:
                        runtimeNode = new QuitRuntimeNode();
                        break;
                }

                if (runtimeNode != null)
                {
                    runtimeNode.nodeName = nodeModel.GetType().Name;
                    runtimeGraph.nodes.Add(runtimeNode);
                }
                else
                {
                    runtimeGraph.nodes.Add(null);
                }
            }

            for (int i = 0; i < editorNodes.Count; i++)
            {
                var nodeModel = editorNodes[i];
                var runtimeNode = runtimeGraph.GetNode(i);
                if (runtimeNode == null) continue;

                foreach (var outputPort in nodeModel.GetOutputPorts())
                {
                    var connectedPort = outputPort.firstConnectedPort;
                    if (connectedPort != null)
                    {
                        var targetNode = connectedPort.GetNode();
                        int targetIndex = editorNodes.IndexOf(targetNode);
                        if (targetIndex != -1)
                        {
                            runtimeNode.links.Add(new UIFlowLink
                            {
                                portName = outputPort.name,
                                targetNodeIndex = targetIndex
                            });
                        }
                    }
                }
            }

            ctx.AddObjectToAsset("RuntimeAsset", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
        }

        private static T GetPortOrOptionValue<T>(Node node, string name, T fallback)
        {
            var port = node.GetInputPorts().FirstOrDefault(p => p.name == name);
            if (port != null && port.isConnected)
            {
                var connectedPort = port.firstConnectedPort;
                if (connectedPort != null)
                {
                    var connectedNode = connectedPort.GetNode();
                    switch (connectedNode)
                    {
                        case IVariableNode variableNode:
                            if (variableNode.variable.TryGetDefaultValue<T>(out var varValue))
                            {
                                if (typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(varValue as string))
                                {
                                    return (T)(object)variableNode.variable.name;
                                }

                                return varValue;
                            }

                            if (typeof(T) == typeof(string))
                            {
                                return (T)(object)variableNode.variable.name;
                            }

                            break;
                        case IConstantNode constantNode:
                            if (constantNode.TryGetValue<T>(out var constValue))
                                return constValue;
                            break;
                    }
                }
            }

            if (TryGetOptionValue<T>(node, name, out var optionValue))
            {
                return optionValue;
            }

            return fallback;
        }

        private static bool TryGetOptionValue<T>(Node node, string optionName, out T value)
        {
            value = default;

            var option = node.GetNodeOptionByName(optionName);
            return option != null && option.TryGetValue(out value);
        }
    }
}