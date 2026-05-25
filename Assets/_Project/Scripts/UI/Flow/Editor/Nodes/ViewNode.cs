using System;
using Unity.GraphToolkit.Editor;
using UnityEngine.UIElements;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// Displays a single UI view. Replaces the current root UI content.
    /// Can be used for screens like Title, Main Menu, or Settings.
    /// </summary>
    [Serializable]
    public class ViewNode : UIFlowNode
    {
        private const int DEFAULT_BUTTON_COUNT = 2;
        public const string VIEW_ASSET_OPTION_NAME = "ViewAsset";
        public const string SHOW_DELAY_OPTION_NAME = "ShowDelay";
        public const string CLEAR_HISTORY_OPTION_NAME = "ClearHistory";
        public const string BUTTON_COUNT_OPTION_NAME = "ButtonCount";
        public const string AUTO_NEXT_OPTION_NAME = "AutoNext";
        public const string WAIT_SECONDS_OPTION_NAME = "WaitSeconds";
        public const string HIDE_ON_NEXT_OPTION_NAME = "HideOnNext";

        /// <summary>
        /// The <see cref="VisualTreeAsset"/> to display.
        /// </summary>
        public VisualTreeAsset viewAsset;

        /// <summary>
        /// Optional delay (in seconds) before the view is displayed.
        /// </summary>
        public float showDelay;

        /// <summary>
        /// If enabled, the flow will automatically transition to the next node after <see cref="waitSeconds"/>.
        /// </summary>
        public bool autoNext;

        /// <summary>
        /// Duration to wait before auto-transitioning. Only used if <see cref="autoNext"/> is true.
        /// </summary>
        public float waitSeconds;

        /// <summary>
        /// If true, clears the navigation history, preventing the "Back" action from returning to previous views.
        /// </summary>
        public bool clearHistory;

        /// <summary>
        /// If true, the UI view is hidden when transitioning to the next node.
        /// </summary>
        public bool hideOnNext;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
{
            context.AddOption<VisualTreeAsset>(VIEW_ASSET_OPTION_NAME)
                .WithDisplayName("View Asset")
                .WithDefaultValue(viewAsset);

            context.AddOption<float>(SHOW_DELAY_OPTION_NAME)
                .WithDisplayName("Show Delay")
                .WithDefaultValue(showDelay);
            
            context.AddOption<bool>(AUTO_NEXT_OPTION_NAME)
                .WithDisplayName("Auto Next")
                .WithDefaultValue(autoNext);

            if (autoNext)
            {
                context.AddOption<float>(WAIT_SECONDS_OPTION_NAME)
                    .WithDisplayName("Wait Seconds")
                    .WithDefaultValue(waitSeconds)
                    .Delayed();;
            }

            context.AddOption<bool>(CLEAR_HISTORY_OPTION_NAME)
                .WithDisplayName("Clear History")
                .WithDefaultValue(clearHistory);

            context.AddOption<bool>(HIDE_ON_NEXT_OPTION_NAME)
                .WithDisplayName("Hide On Next")
                .WithDefaultValue(hideOnNext);

            context.AddOption<int>(BUTTON_COUNT_OPTION_NAME)
.WithDisplayName("Button Count")
                .WithDefaultValue(DEFAULT_BUTTON_COUNT)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(INPUT_PORT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            if (autoNext)
            {
                context.AddOutputPort(OUTPUT_PORT_NAME)
                    .WithDisplayName(string.Empty)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }

            var buttonCountOption = GetNodeOptionByName(BUTTON_COUNT_OPTION_NAME);
            buttonCountOption.TryGetValue<int>(out var buttonCount);
            for (var i = 0; i < buttonCount; i++)
            {
                var optionName = UIFlowUtilities.GetButtonNameOptionName(i);
                context.AddInputPort<string>(optionName).Build();
                context.AddOutputPort(optionName).WithConnectorUI(PortConnectorUI.Circle).Build();
            }
        }

        private int GetConfiguredButtonCount()
        {
            var buttonCount = DEFAULT_BUTTON_COUNT;
            var buttonCountOption = GetNodeOptionByName(BUTTON_COUNT_OPTION_NAME);
            if (buttonCountOption != null && buttonCountOption.TryGetValue<int>(out var optionValue))
            {
                buttonCount = optionValue;
            }

            return Math.Max(0, buttonCount);
        }
    }
}