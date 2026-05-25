using System;
using Unity.GraphToolkit.Editor;
using UnityEngine.SceneManagement;

namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// Triggers a scene load operation. 
    /// Can handle single or additive loading, with optional wait times and activation control.
    /// </summary>
    [Serializable]
    public class LoadSceneNode : UIFlowNode
    {
        public const string SCENE_NAME_OPTION_NAME = "SceneName";
        public const string PREVENT_LOADING_SAME_SCENE_OPTION_NAME = "PreventLoadingSameScene";
        public const string LOAD_SCENE_MODE_OPTION_NAME = "LoadSceneMode";
        public const string ALLOW_SCENE_ACTIVATION_OPTION_NAME = "AllowSceneActivation";
        public const string SCENE_ACTIVATION_DELAY_OPTION_NAME = "SceneActivationDelay";
        public const string WAIT_FOR_SCENE_TO_LOAD_OPTION_NAME = "WaitForSceneToLoad";
        public const string CONNECT_PROGRESSOR_OPTION_NAME = "ConnectProgressor";

        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        public string sceneName;

        /// <summary>
        /// If true, avoids reloading if the target scene is already the active scene.
        /// </summary>
        public bool preventLoadingSameScene = true;

        /// <summary>
        /// The loading mode (Single or Additive).
        /// </summary>
        public LoadSceneMode loadSceneMode = LoadSceneMode.Single;

        /// <summary>
        /// If false, the scene will load in the background but won't activate until manually triggered.
        /// </summary>
        public bool allowSceneActivation = true;

        /// <summary>
        /// Delay before the scene becomes active.
        /// </summary>
        public float sceneActivationDelay = 0.2f;

        /// <summary>
        /// If true, execution pauses until the scene has finished loading.
        /// </summary>
        public bool waitForSceneToLoad = true;

        /// <summary>
        /// (Editor Only) Whether to show a progressor port.
        /// </summary>
        public bool connectProgressor;

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>(SCENE_NAME_OPTION_NAME)
                .WithDisplayName("Scene Name")
                .WithDefaultValue(sceneName);

            context.AddOption<bool>(PREVENT_LOADING_SAME_SCENE_OPTION_NAME)
                .WithDisplayName("Prevent Loading Same Scene")
                .WithDefaultValue(preventLoadingSameScene);

            context.AddOption<LoadSceneMode>(LOAD_SCENE_MODE_OPTION_NAME)
                .WithDisplayName("Load Scene Mode")
                .WithDefaultValue(loadSceneMode);

            context.AddOption<bool>(ALLOW_SCENE_ACTIVATION_OPTION_NAME)
                .WithDisplayName("Allow Scene Activation")
                .WithDefaultValue(allowSceneActivation);

            context.AddOption<float>(SCENE_ACTIVATION_DELAY_OPTION_NAME)
                .WithDisplayName("Scene Activation Delay")
                .WithDefaultValue(sceneActivationDelay);

            context.AddOption<bool>(WAIT_FOR_SCENE_TO_LOAD_OPTION_NAME)
                .WithDisplayName("Wait for Scene to Load")
                .WithDefaultValue(waitForSceneToLoad);

            context.AddOption<bool>(CONNECT_PROGRESSOR_OPTION_NAME)
                .WithDisplayName("Connect Progressor")
                .WithDefaultValue(connectProgressor);
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            AddExecutionPorts(context);
        }
    }
}
