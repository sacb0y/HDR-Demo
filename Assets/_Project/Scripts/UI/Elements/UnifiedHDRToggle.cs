using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI.Elements
{
    [UxmlElement]
    public partial class UnifiedHDRToggle : Toggle
    {
        public UnifiedHDRToggle()
        {
            label = "Display HDR";
            this.RegisterValueChangedCallback(OnToggleChanged);
            
            RegisterCallback<AttachToPanelEvent>(OnAttach);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (SettingsManager.Instance != null)
            {
                SetValueWithoutNotify(SettingsManager.Instance.CurrentSettings.hdrEnabled);
                SettingsManager.Instance.OnSettingsChanged += SyncValue;
            }
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= SyncValue;
            }
        }

        private void SyncValue()
        {
            if (SettingsManager.Instance != null)
            {
                SetValueWithoutNotify(SettingsManager.Instance.CurrentSettings.hdrEnabled);
            }
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            if (SettingsManager.Instance != null && SettingsManager.Instance.CurrentSettings.hdrEnabled != evt.newValue)
            {
                SettingsManager.Instance.UpdateHDR(evt.newValue);
            }
        }
    }
}