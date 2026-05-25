using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// The editor-side graph asset for creating UI Flows.
    /// This graph is composed of <see cref="UIFlowNode"/>s and is used to generate a <see cref="_Project.UI.Flow.Runtime.UIFlowRuntimeGraph"/>.
    /// </summary>
    [Serializable]
    [Graph(AssetExtension)]
    public class UIFlowGraph : Graph
    {
        /// <summary>
        /// The file extension for UI Flow graph assets.
        /// </summary>
        public const string AssetExtension = "uiflow";

        private const string DefaultName = "New UI Flow";

        /// <summary>
        /// Creates a new UI Flow graph asset in the Project window.
        /// </summary>
        [MenuItem("Assets/Create/UI Flow/Graph")]
        static void CreateAsset()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<UIFlowGraph>(DefaultName);
        }
    }
}