using System;
using Unity.GraphToolkit.Editor;
using UnityEngine.UIElements;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// A block node within a <see cref="UISequence"/> that displays a new UI view, replacing the current content.
    /// </summary>
    [UseWithContext(typeof(UISequence))]
    [Serializable]
    public class ShowUI : BlockNode
    {
        public const string VIEW_ASSET_OPTION_NAME = "ViewAsset";

        /// <summary>
        /// The <see cref="VisualTreeAsset"/> to display.
        /// </summary>
        public VisualTreeAsset viewAsset;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<VisualTreeAsset>(VIEW_ASSET_OPTION_NAME)
                .WithDisplayName("View Asset")
                .WithDefaultValue(viewAsset);
        }
    }

    /// <summary>
    /// A block node within a <see cref="UISequence"/> that adds a UI view to the current screen without replacing existing content.
    /// </summary>
    [UseWithContext(typeof(UISequence))]
    [Serializable]
    public class AddUI : BlockNode
    {
        public const string VIEW_ASSET_OPTION_NAME = "ViewAsset";

        /// <summary>
        /// The <see cref="VisualTreeAsset"/> to add.
        /// </summary>
        public VisualTreeAsset viewAsset;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<VisualTreeAsset>(VIEW_ASSET_OPTION_NAME)
                .WithDisplayName("View Asset")
                .WithDefaultValue(viewAsset);
        }
    }

    /// <summary>
    /// A block node within a <see cref="UISequence"/> that pauses execution for a specified duration.
    /// </summary>
    [UseWithContext(typeof(UISequence))]
    [Serializable]
    public class WaitTime : BlockNode
    {
        public const string WAIT_SECONDS_OPTION_NAME = "WaitSeconds";

        /// <summary>
        /// Duration to wait in seconds.
        /// </summary>
        public float waitSeconds;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<float>(WAIT_SECONDS_OPTION_NAME)
                .WithDisplayName("Wait Seconds")
                .WithDefaultValue(waitSeconds);
        }
    }
}