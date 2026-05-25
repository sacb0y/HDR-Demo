using System;
using Unity.GraphToolkit.Editor;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// This node marks the starting point of the UI Flow.
    /// It only has an output port.
    /// </summary>
    [Serializable]
    public class StartNode : UIFlowNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort(OUTPUT_PORT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}