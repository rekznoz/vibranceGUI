using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using vibrance.GUI.AMD.vendor;
using vibrance.GUI.common;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.AMD
{
    public class AmdDynamicVibranceProxy : IVibranceProxy
    {
        private const int NeutralSaturation = 100;
        private bool _vibranceEnabled = true;
        private readonly IAmdAdapter _amdAdapter;
        private List<ApplicationSetting> _applicationSettings;
        private readonly Dictionary<string, List<ResolutionModeWrapper>> _supportedResolutionModes;
        private VibranceInfo _vibranceInfo;
        private WinEventHook _hook;
        private string _gameDisplay;
        private bool _gameChangedAllDisplays;

        public AmdDynamicVibranceProxy(IAmdAdapter amdAdapter, List<ApplicationSetting> applicationSettings, Dictionary<string, List<ResolutionModeWrapper>> supportedResolutionModes)
        {
            _amdAdapter = amdAdapter;
            _applicationSettings = applicationSettings;
            _supportedResolutionModes = supportedResolutionModes;

            try
            {
                _vibranceInfo = new VibranceInfo();
                if (amdAdapter.IsAvailable())
                {
                    _vibranceInfo.isInitialized = true;
                    amdAdapter.Init();
                }

                if (_vibranceInfo.isInitialized)
                {
                    _hook = WinEventHook.GetInstance();
                    _hook.WinEventHookHandler += OnWinEventHook;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                DialogResult result = MessageBox.Show(UiLanguage.Translate(NvidiaDynamicVibranceProxy.NvapiErrorInitFailed),
                    UiLanguage.Translate("vibranceGUI Error"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    Process.Start(NvidiaDynamicVibranceProxy.GuideLink);
                }
            }
        }

        public void SetApplicationSettings(List<ApplicationSetting> refApplicationSettings)
        {
            _applicationSettings = refApplicationSettings;
        }

        public void SetShouldRun(bool shouldRun)
        {
            _vibranceInfo.shouldRun = shouldRun;
        }

        public void SetVibranceEnabled(bool enabled)
        {
            _vibranceEnabled = enabled;
            ApplyDesktopVibrance();
            if (enabled)
                _hook.RefreshForegroundWindow();
        }

        public void SetNeverSwitchResolution(bool neverChangeResolution)
        {
            _vibranceInfo.neverChangeResolution = neverChangeResolution;
        }

        public void SetVibranceWindowsLevel(int vibranceWindowsLevel)
        {
            _vibranceInfo.userVibranceSettingDefault = vibranceWindowsLevel;
            if (_vibranceInfo.shouldRun && !WinEventHook.HasForegroundProfile(_applicationSettings))
                ApplyDesktopVibrance();
        }

        public void SetVibranceIngameLevel(int vibranceIngameLevel)
        {
            _vibranceInfo.userVibranceSettingActive = vibranceIngameLevel;
        }

        public bool UnloadLibraryEx()
        {
            _hook.RemoveWinEventHook();
            return true;
        }

        public void HandleDvcExit()
        {
            if (_hook != null)
                _hook.WinEventHookHandler -= OnWinEventHook;
            ResolutionHelper.RestoreProfileResolutions();
            if (_vibranceInfo.shouldRun)
                SetVibranceEnabled(false);
        }

        public void SetAffectPrimaryMonitorOnly(bool affectPrimaryMonitorOnly)
        {
            _vibranceInfo.affectPrimaryMonitorOnly = affectPrimaryMonitorOnly;
            if (_vibranceInfo.shouldRun && !WinEventHook.HasForegroundProfile(_applicationSettings))
                ApplyDesktopVibrance();
        }

        public VibranceInfo GetVibranceInfo()
        {
            return _vibranceInfo;
        }

        public GraphicsAdapter GraphicsAdapter { get; } = GraphicsAdapter.Amd;

        private void OnWinEventHook(object sender, WinEventHookEventArgs e)
        {
            if (!_vibranceInfo.shouldRun || e.Handle != WinEventHook.GetForegroundWindow())
                return;

            var profile = _applicationSettings.FirstOrDefault(x => string.Equals(x.Name, e.ProcessName, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                ResolutionHelper.RestoreProfileResolutions();
                ApplyDesktopVibrance();
                return;
            }

            Screen screen = Screen.FromHandle(e.Handle);
            var resolution = !_vibranceInfo.neverChangeResolution && profile.IsResolutionChangeNeeded
                ? profile.ResolutionSettings : null;
            ResolutionHelper.ApplyProfileResolution(screen.DeviceName, resolution, _supportedResolutionModes);

            if (!_vibranceEnabled)
                return;

            if (_vibranceInfo.affectPrimaryMonitorOnly)
            {
                if (_gameChangedAllDisplays || (_gameDisplay != null && _gameDisplay != screen.DeviceName))
                    ApplyDesktopVibrance();
                _amdAdapter.SetSaturationOnDisplay(profile.IngameLevel, screen.DeviceName);
                _gameDisplay = screen.DeviceName;
            }
            else
            {
                _amdAdapter.SetSaturationOnAllDisplays(profile.IngameLevel);
                _gameChangedAllDisplays = true;
            }
        }

        private void ApplyDesktopVibrance()
        {
            int level = _vibranceEnabled ? _vibranceInfo.userVibranceSettingDefault : NeutralSaturation;
            if (!_vibranceInfo.affectPrimaryMonitorOnly || _gameChangedAllDisplays)
            {
                _amdAdapter.SetSaturationOnAllDisplays(level);
            }
            else
            {
                string primary = Screen.PrimaryScreen?.DeviceName;
                if (primary != null)
                    _amdAdapter.SetSaturationOnDisplay(level, primary);
                if (_gameDisplay != null && _gameDisplay != primary)
                    _amdAdapter.SetSaturationOnDisplay(level, _gameDisplay);
            }
            _gameDisplay = null;
            _gameChangedAllDisplays = false;
        }
    }
}
