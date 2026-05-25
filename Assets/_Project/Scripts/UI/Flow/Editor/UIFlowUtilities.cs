namespace _Project.UI.Flow.Editor
{
    /// <summary>
    /// Utility methods for the UI Flow editor.
    /// </summary>
    public static class UIFlowUtilities
    {
        private const string BUTTON_OPTION_PREFIX = "Button";
        
        /// <summary>
        /// Gets the name used for a button's port or option based on its index.
        /// </summary>
        /// <param name="index">The button index.</param>
        /// <returns>The formatted name.</returns>
        public static string GetButtonNameOptionName(int index)
        {
            return $"{BUTTON_OPTION_PREFIX}{index}";
        }
    }
}