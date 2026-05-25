using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// A container node that executes a linear sequence of UI operations defined by block nodes.
    /// Useful for splash screens, tutorials, or multi-step dialogs.
    /// </summary>
    [Serializable]
    public class UISequence : ContextNode
    {
        private const int DEFAULT_BUTTON_COUNT = 2;
        public const string BUTTON_COUNT_OPTION_NAME = "ButtonCount";
        public const string NEXT_ON_COMPLETE_OPTION_NAME = "NextOnComplete";
        
        /// <summary>
        /// If enabled, the flow will automatically transition to the "Next" port once all items in the sequence have finished.
        /// </summary>
        public bool nextOnComplete;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(BUTTON_COUNT_OPTION_NAME)
                .WithDisplayName("Button Count")
                .WithDefaultValue(DEFAULT_BUTTON_COUNT)
                .Delayed();

            DefineButtonNameOptions(context, GetConfiguredButtonCount());
            
            context.AddOption<bool>(NEXT_ON_COMPLETE_OPTION_NAME)
                .WithDisplayName("Next on Complete")
                .WithDefaultValue(false);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(UIFlowNode.INPUT_PORT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            var nextOnCompleteOption = GetNodeOptionByName(NEXT_ON_COMPLETE_OPTION_NAME);
            var nextOnCompleteValue = nextOnComplete;
            if (nextOnCompleteOption != null && nextOnCompleteOption.TryGetValue<bool>(out var optionNextOnComplete))
            {
                nextOnCompleteValue = optionNextOnComplete;
            }

            if (nextOnCompleteValue)
            {

                context.AddOutputPort(UIFlowNode.OUTPUT_PORT_NAME)
                    .WithDisplayName(string.Empty)
                    .WithConnectorUI(PortConnectorUI.Arrowhead)
                    .Build();
            }

            var buttonCountOption = GetNodeOptionByName(BUTTON_COUNT_OPTION_NAME);
            var buttonCount = DEFAULT_BUTTON_COUNT;
            if (buttonCountOption != null && buttonCountOption.TryGetValue<int>(out var optionButtonCount))
            {
                buttonCount = optionButtonCount;
            }

            for (var i = 0; i < buttonCount; i++)
            {
                context.AddInputPort<string>(UIFlowUtilities.GetButtonNameOptionName(i)).Build();
                context.AddOutputPort(UIFlowUtilities.GetButtonNameOptionName(i)).WithConnectorUI(PortConnectorUI.Circle).Build();
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

        private static void DefineButtonNameOptions(IOptionDefinitionContext context, int buttonCount)
        {
            for (var i = 0; i < buttonCount; i++)
            {
                context.AddOption<string>(UIFlowUtilities.GetButtonNameOptionName(i))
                    .WithDisplayName($"Button {i} Name")
                    .WithDefaultValue(string.Empty);
            }
        }
    }
}