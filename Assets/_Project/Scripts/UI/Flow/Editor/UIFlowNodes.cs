using System;
using Unity.GraphToolkit.Editor;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// Base class for all nodes in the UI Flow editor graph.
    /// Provides standard execution ports (In/Next).
    /// </summary>
    [Serializable]
    public abstract class UIFlowNode : Node
    {
        /// <summary>
        /// The name of the standard input execution port.
        /// </summary>
        public const string INPUT_PORT_NAME = "In";

        /// <summary>
        /// The name of the standard output execution port.
        /// </summary>
        public const string OUTPUT_PORT_NAME = "Next";

        /// <summary>
        /// Defines the standard execution ports for the node.
        /// </summary>
        /// <param name="context">The port definition context.</param>
        protected virtual void AddExecutionPorts(IPortDefinitionContext context)
        {
            context.AddInputPort(INPUT_PORT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            context.AddOutputPort(OUTPUT_PORT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}