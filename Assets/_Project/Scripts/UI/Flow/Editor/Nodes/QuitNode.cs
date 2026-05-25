using System;
using Unity.GraphToolkit.Editor;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// Terminates the application (or stops play mode in the editor).
    /// </summary>
    [Serializable]
    public class QuitNode : UIFlowNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort(INPUT_PORT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
    }
}
