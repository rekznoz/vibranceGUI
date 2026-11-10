using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using vibrance.GUI.NVIDIA;

namespace vibrance.GUI.common
{

    class SettingsController : ISettingsController
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(
           string lpAppName,
           string lpKeyName,
           string lpDefault,
           StringBuilder lpReturnedString,
           uint nSize,
           string lpFileName);


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool WritePrivateProfileString(string lpAppName,
          string lpKeyName, string lpString, string lpFileName);

        const string SzSectionName = "Settings";
        const string SzKeyNameInactive = "inactiveValue";
        const string SzKeyNameRefreshRate = "refreshRate";
        const string SzKeyNameAffectPrimaryMonitorOnly = "affectPrimaryMonitorOnly";
        const string SzKeyNameNeverSwitchResolution = "neverSwitchResolution";

        private string _fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\vibranceGUI.ini";
        private string _fileNameApplicationSettings = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\vibranceGUI\\applicationData.xml";


        public bool SetVibranceSettings(string windowsLevel, string affectPrimaryMonitorOnly, string neverSwitchResolution, List<ApplicationSetting> applicationSettings)
        {
            string temporaryFile = _fileNameApplicationSettings + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_fileNameApplicationSettings));
                XmlSerializer serializer = new XmlSerializer(typeof(List<ApplicationSetting>));
                using (var writer = XmlWriter.Create(temporaryFile))
                {
                    serializer.Serialize(writer, applicationSettings);
                }

                if (!WritePrivateProfileString(SzSectionName, SzKeyNameInactive, windowsLevel, _fileName) ||
                    !WritePrivateProfileString(SzSectionName, SzKeyNameAffectPrimaryMonitorOnly, affectPrimaryMonitorOnly, _fileName) ||
                    !WritePrivateProfileString(SzSectionName, SzKeyNameNeverSwitchResolution, neverSwitchResolution, _fileName))
                {
                    return false;
                }

                if (File.Exists(_fileNameApplicationSettings))
                    File.Replace(temporaryFile, _fileNameApplicationSettings, null);
                else
                    File.Move(temporaryFile, _fileNameApplicationSettings);

                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
            {
                Trace.TraceError("Could not save vibranceGUI settings: {0}", ex);
                return false;
            }
            finally
            {
                try
                {
                    File.Delete(temporaryFile);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    Trace.TraceWarning("Could not delete temporary settings file: {0}", ex);
                }
            }
        }

        public bool SetVibranceSetting(string szKeyName, string value)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_fileName));
                return WritePrivateProfileString(SzSectionName, szKeyName, value, _fileName);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Trace.TraceError("Could not save vibranceGUI setting: {0}", ex);
                return false;
            }
        }

        public void ReadVibranceSettings(GraphicsAdapter graphicsAdapter, out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out bool neverSwitchResolution, out List<ApplicationSetting> applicationSettings)
        {
            int defaultLevel = 0; 
            int maxLevel = 0;
            if (graphicsAdapter == GraphicsAdapter.Nvidia)
            {
                defaultLevel = NvidiaDynamicVibranceProxy.NvapiDefaultLevel;
                maxLevel = NvidiaDynamicVibranceProxy.NvapiMaxLevel;
            }
            if (graphicsAdapter == GraphicsAdapter.Amd)
            {
                // todo
                defaultLevel = 100;
                maxLevel = 300;
            }

            if (!IsFileExisting(_fileName) || !IsFileExisting(_fileNameApplicationSettings))
            {
                vibranceWindowsLevel = defaultLevel;
                affectPrimaryMonitorOnly = false;
                applicationSettings = new List<ApplicationSetting>();
                neverSwitchResolution = false;
                return;
            }

            string szDefault = "";

            StringBuilder szValueInactive = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameInactive,
                szDefault,
                szValueInactive,
                Convert.ToUInt32(szValueInactive.Capacity),
                _fileName);

            StringBuilder szValueRefreshRate = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameRefreshRate,
                szDefault,
                szValueRefreshRate,
                Convert.ToUInt32(szValueRefreshRate.Capacity),
                _fileName);

            StringBuilder szValueAffectPrimaryMonitorOnly = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameAffectPrimaryMonitorOnly,
                "false",
                szValueAffectPrimaryMonitorOnly,
                Convert.ToUInt32(szValueAffectPrimaryMonitorOnly.Capacity),
                _fileName);

            StringBuilder szValueNeverSwitchResolution = new StringBuilder(1024);
            GetPrivateProfileString(SzSectionName,
                SzKeyNameNeverSwitchResolution,
                "false",
                szValueNeverSwitchResolution,
                Convert.ToUInt32(szValueNeverSwitchResolution.Capacity),
                _fileName);

            try
            {
                vibranceWindowsLevel = int.Parse(szValueInactive.ToString());
                affectPrimaryMonitorOnly = bool.Parse(szValueAffectPrimaryMonitorOnly.ToString());
                neverSwitchResolution = bool.Parse(szValueNeverSwitchResolution.ToString());
            }
            catch (Exception)
            {
                vibranceWindowsLevel = defaultLevel;
                affectPrimaryMonitorOnly = false;
                applicationSettings = new List<ApplicationSetting>();
                neverSwitchResolution = false;
                return;
            }

            if (vibranceWindowsLevel < defaultLevel || vibranceWindowsLevel > maxLevel)
                vibranceWindowsLevel = defaultLevel;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ApplicationSetting>));
                using (var reader = XmlReader.Create(_fileNameApplicationSettings))
                {
                    applicationSettings = (List<ApplicationSetting>)serializer.Deserialize(reader);
                }
            }
            catch (Exception)
            {
                applicationSettings = new List<ApplicationSetting>();
            }
        }

        private bool IsFileExisting(string szFilename)
        {
            return File.Exists(szFilename);
        }
    }
}
