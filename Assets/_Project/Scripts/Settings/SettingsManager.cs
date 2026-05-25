using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SettingsData
{
    public int targetDisplayIndex = 0;
    public bool hdrEnabled = true;
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private string fileName = "settings.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, fileName);

    public SettingsData CurrentSettings { get; private set; } = new SettingsData();

    public event Action OnSettingsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentSettings, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Settings saved to: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save settings: {e.Message}");
        }
    }

    public void LoadSettings()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
                ApplySettings();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load settings: {e.Message}");
                CurrentSettings = new SettingsData();
            }
        }
        else
        {
            CurrentSettings = new SettingsData();
        }
    }

    public void ApplySettings()
    {
        // Apply HDR
        // In Unity 6, HDR output is managed per-display context.
        // If the display supports HDR and it's enabled in settings, we request it.
        if (HDROutputSettings.main.available)
        {
            HDROutputSettings.main.RequestHDRModeChange(CurrentSettings.hdrEnabled);
            Debug.Log($"[SettingsManager] HDR Mode change requested: {CurrentSettings.hdrEnabled}");
        }

        // Apply Display
        var displays = new List<DisplayInfo>();
        Screen.GetDisplayLayout(displays);
        
        if (CurrentSettings.targetDisplayIndex >= 0 && CurrentSettings.targetDisplayIndex < displays.Count)
        {
            var targetDisplay = displays[CurrentSettings.targetDisplayIndex];
            // Screen.mainWindowDisplayInfo matches the current display
            if (Screen.mainWindowDisplayInfo.name != targetDisplay.name)
            {
                Screen.MoveMainWindowTo(targetDisplay, targetDisplay.workArea.position);
            }
        }

        OnSettingsChanged?.Invoke();
    }

    public void UpdateHDR(bool enabled)
    {
        CurrentSettings.hdrEnabled = enabled;
        // In many Unity 6 setups, HDR is toggled via HDROutputSettings.main.RequestHDRModeChange(enabled);
        // However, it might require the display to be HDR capable.
        ApplySettings();
    }

    public void UpdateDisplay(int index)
    {
        CurrentSettings.targetDisplayIndex = index;
        ApplySettings();
    }
}
