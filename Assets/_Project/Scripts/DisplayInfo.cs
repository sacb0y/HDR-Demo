using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using _Project.UI.Flow.Runtime;

namespace _Project.Scripts
{
    public class DisplayInfo : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private UIFlowRunner flowRunner;

        private MultiColumnListView _listView;
        private List<DisplayInfoRow> _items = new List<DisplayInfoRow>();

        private readonly Color _blueColor = new Color(0.161f, 0.514f, 0.718f); // #2983B7
        private readonly Color _redColor = new Color(0.718f, 0.165f, 0.165f);  // #b72a2a

        private struct DisplayInfoRow
        {
            public string Spec;
            public string Result;
            public bool IsWarning;
        }

        private void Awake()
        {
            if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
            if (flowRunner == null) flowRunner = GetComponent<UIFlowRunner>();
        }

        private void OnEnable()
        {
            if (flowRunner != null)
                flowRunner.OnViewChanged += HandleViewChanged;
            
            InitializeUI();
        }

        private void OnDisable()
        {
            if (flowRunner != null)
                flowRunner.OnViewChanged -= HandleViewChanged;
        }

        private void HandleViewChanged(string viewName)
        {
            // The viewName passed by UIFlowRunner is the name of the VisualTreeAsset
            if (viewName == "DisplayInfo")
            {
                InitializeUI();
            }
        }

        public void PopulateDisplayInfo()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null) return;

            _listView = uiDocument.rootVisualElement.Q<MultiColumnListView>("DisplayInfo");
            if (_listView == null) return;

            PopulateData();
            SetupListView();
        }

        private void SetupListView()
        {
            _listView.itemsSource = _items;

            // Columns "Spec" and "Result" are defined in DisplayInfo.uxml
            if (_listView.columns.Contains("Spec"))
            {
                var specColumn = _listView.columns["Spec"];
                specColumn.makeCell = () => new Label();
                specColumn.bindCell = (VisualElement e, int i) =>
                {
                    var label = (Label)e;
                    if (i < _items.Count)
                    {
                        label.text = _items[i].Spec;
                        label.style.color = _items[i].IsWarning ? _redColor : _blueColor;
                    }
                };
            }

            if (_listView.columns.Contains("Result"))
            {
                var resultColumn = _listView.columns["Result"];
                resultColumn.makeCell = () => new Label();
                resultColumn.bindCell = (VisualElement e, int i) =>
                {
                    var label = (Label)e;
                    if (i < _items.Count)
                    {
                        label.text = _items[i].Result;
                        label.style.color = Color.white;
                    }
                };
            }

            _listView.RefreshItems();
        }

        private void PopulateData()
        {
            _items.Clear();

            #if !PLATFORM_ANDROID
            var displayName = Screen.mainWindowDisplayInfo.name;
            var displayRefreshRate = Screen.mainWindowDisplayInfo.refreshRate;
            var nativeHeight = Screen.mainWindowDisplayInfo.height;
            var nativeWidth = Screen.mainWindowDisplayInfo.width;
            #else
            var displayName = "Android";
            var displayRefreshRate = Screen.currentResolution.refreshRateRatio;
            var nativeHeight = Screen.height;
            var nativeWidth = Screen.width;
            #endif

            var hdrActive = HDROutputSettings.main.active;
            var hdrAvailable = HDROutputSettings.main.available;
            
            AddRow("Display Name", displayName);
            AddRow("Refresh Rate", displayRefreshRate.ToString());
            AddRow("Native Resolution", $"{nativeWidth} x {nativeHeight}");
            AddRow("HDR Active", hdrActive.ToString());
            AddRow("HDR Available", hdrAvailable.ToString());

            if (!hdrAvailable)
            {
                _items.Add(new DisplayInfoRow { Spec = "The following values are Not Available if display does not support HDR", Result = "", IsWarning = true });
            }

            var maxFullFrameLuminance = hdrAvailable ? HDROutputSettings.main.maxFullFrameToneMapLuminance.ToString() : "NA";
            var maxLuminance = hdrAvailable ? HDROutputSettings.main.maxToneMapLuminance.ToString() : "NA";
            var minLuminance = hdrAvailable ? HDROutputSettings.main.minToneMapLuminance.ToString() : "NA";
            var displayPaperWhite = hdrAvailable ? HDROutputSettings.main.paperWhiteNits.ToString(CultureInfo.CurrentCulture) : "NA";
            var displayColorGamut = hdrAvailable ? HDROutputSettings.main.displayColorGamut.ToString() : "NA";
            var displayAutomaticToneMapping = hdrAvailable ? HDROutputSettings.main.automaticHDRTonemapping.ToString() : "NA";
            var automaticToneMapping = hdrAvailable && HDROutputSettings.main.automaticHDRTonemapping;
            var displaySupport = SystemInfo.hdrDisplaySupportFlags;
            const string displayBitDepth = "NA for now...";
            var displayGraphicsFormat = hdrAvailable ? HDROutputSettings.main?.graphicsFormat.ToString() : "NA";

            AddRow("Max Full Frame Luminance", maxFullFrameLuminance + " nits");
            AddRow("Max Luminance 10%", maxLuminance + " nits");
            AddRow("Min Luminance", minLuminance + " nits");
            AddRow("Windows Paper White", displayPaperWhite + " nits");
            AddRow("Automatic Tone Mapping", displayAutomaticToneMapping, automaticToneMapping);
            AddRow("Display Color Gamut", displayColorGamut);
            AddRow("Display Bit Depth", displayBitDepth);
            AddRow("Display Graphics Format", displayGraphicsFormat);
            AddRow("Display Support Flags", displaySupport.ToString());
        }

        private void AddRow(string spec, string result, bool isWarning = false)
        {
            _items.Add(new DisplayInfoRow { Spec = spec, Result = result, IsWarning = isWarning });
        }
    }
}

