using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace vibrance.GUI.common
{
    public partial class VibranceGUI : Form
    {
        private readonly int _defaultWindowsLevel;
        private readonly int _minTrackBarValue;
        private readonly int _maxTrackBarValue;
        private readonly int _defaultIngameValue;
        private readonly Func<int, string> _resolveLabelLevel;
        private readonly IVibranceProxy _v;
        private IRegistryController _registryController;
        private const string AppName = "vibranceGUI";
        private const string SaveError = "Your settings could not be saved. Check that the settings folder is writable.";
        private const string TwitterLink = "https://twitter.com/juvlarN";
        private const string PaypalDonationLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=JDQFNKNNEW356";

        private bool _allowVisible;
        private readonly List<Action> _languageBindings = new List<Action>();
        private string _statusText = "Starting";
        private bool _settingsLoaded;
        private bool _settingsDirty;
        private List<ApplicationSetting> _applicationSettings;
        private readonly List<ResolutionModeWrapper> _supportedResolutionList;

        public VibranceGUI(
            Func<List<ApplicationSetting>, Dictionary<string, List<ResolutionModeWrapper>>, IVibranceProxy> getProxy,
            int defaultWindowsLevel, 
            int minTrackBarValue,
            int maxTrackBarValue,
            int defaultIngameValue,
            Func<int, string> resolveLabelLevel)
        {
            _defaultWindowsLevel = defaultWindowsLevel;
            _minTrackBarValue = minTrackBarValue;
            _maxTrackBarValue = maxTrackBarValue;
            _defaultIngameValue = defaultIngameValue;
            _resolveLabelLevel = resolveLabelLevel;
            _allowVisible = true;

            InitializeComponent();

            trackBarWindowsLevel.Minimum = minTrackBarValue;
            trackBarWindowsLevel.Maximum = maxTrackBarValue;

            var supportedResolutionModes = new Dictionary<string, List<ResolutionModeWrapper>>(StringComparer.OrdinalIgnoreCase);
            foreach (Screen screen in Screen.AllScreens)
            {
                List<ResolutionModeWrapper> availableResolutions = ResolutionHelper.EnumerateSupportedResolutionModes(screen.DeviceName);
                supportedResolutionModes.Add(screen.DeviceName, availableResolutions);
                if (screen.Primary)
                    _supportedResolutionList = availableResolutions;
            }
            _applicationSettings = new List<ApplicationSetting>();
            _v = getProxy(_applicationSettings, supportedResolutionModes);

            backgroundWorker.WorkerReportsProgress = true;

            backgroundWorker.RunWorkerAsync();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!_allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated)
                {
                    CreateHandle();
                }
            }
            base.SetVisibleCore(value);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!SystemInformation.HighContrast)
            {
                const int useImmersiveDarkMode = 20;
                int enabled = 1;
                DwmSetWindowAttribute(Handle, useImmersiveDarkMode, ref enabled, sizeof(int));
            }
        }

        public void SetAllowVisible(bool value)
        {
            _allowVisible = value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetGuiEnabledFlag(false);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.notifyIcon.Visible = true;
                //this.notifyIcon.BalloonTipText = "Running minimized... Like the program? Consider donating!";
                //this.notifyIcon.ShowBalloonTip(250);
                this.Hide();
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int vibranceWindowsLevel = _defaultWindowsLevel;
            bool affectPrimaryMonitorOnly = false;
            bool neverSwitchResolution = false;

            while (!this.IsHandleCreated)
            {
                Thread.Sleep(500);
            }

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    ReadVibranceSettings(out vibranceWindowsLevel, out affectPrimaryMonitorOnly, out neverSwitchResolution);
                });
            }
            else
            {
                ReadVibranceSettings(out vibranceWindowsLevel, out affectPrimaryMonitorOnly, out neverSwitchResolution);
            }

            if (_v.GetVibranceInfo().isInitialized)
            {
                backgroundWorker.ReportProgress(1);

                SetGuiEnabledFlag(true);

                _v.SetApplicationSettings(_applicationSettings);
                _v.SetShouldRun(true);
                _v.SetVibranceWindowsLevel(vibranceWindowsLevel);
                _v.SetAffectPrimaryMonitorOnly(affectPrimaryMonitorOnly);
                _v.SetNeverSwitchResolution(neverSwitchResolution);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (_v != null && _v.GetVibranceInfo().isInitialized)
            {
                SetGuiEnabledFlag(true);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_settingsDirty)
                ForceSaveVibranceSettings(e.CloseReason == CloseReason.UserClosing);

            CleanUp();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackBarWindowsLevel_Scroll(object sender, EventArgs e)
        {
            _v.SetVibranceWindowsLevel(trackBarWindowsLevel.Value);
            labelWindowsLevel.Text = _resolveLabelLevel(trackBarWindowsLevel.Value);
            QueueSettingsSave();
        }

        private void QueueSettingsSave()
        {
            if (!_settingsLoaded)
                return;

            _settingsDirty = true;
            settingsSaveTimer.Stop();
            settingsSaveTimer.Start();
        }

        private void settingsSaveTimer_Tick(object sender, EventArgs e)
        {
            if (_settingsDirty)
                ForceSaveVibranceSettings();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                SetStatusText("●  Active");
                this.statusLabel.ForeColor = AccentColor;
            }
            else if (e.ProgressPercentage == 2)
            {
                SetStatusText("NVAPI unloaded");
                this.statusLabel.ForeColor = WarningColor;
            }
        }

        private void SetStatusText(string english)
        {
            _statusText = english;
            statusLabel.Text = UiLanguage.Translate(english);
        }

        private void BindTranslation(string english, Action<string> apply)
        {
            Action update = () => apply(UiLanguage.Translate(english));
            _languageBindings.Add(update);
            update();
        }

        private void languageSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ((ComboBox)sender).SelectedIndex;
            if (index < 0)
                return;

            bool saved = UiLanguage.Select(index);
            foreach (Action update in _languageBindings)
                update();
            statusLabel.Text = UiLanguage.Translate(_statusText);

            foreach (var explorer in Application.OpenForms.OfType<ProcessExplorer>())
                explorer.RefreshLanguage();

            if (!saved)
                ShowSaveError();
        }

        private void languageSelector_DrawItem(object sender, DrawItemEventArgs e)
        {
            bool selected = (e.State & DrawItemState.Selected) != 0;
            using (var background = new SolidBrush(selected ? BorderColor : ButtonColor))
                e.Graphics.FillRectangle(background, e.Bounds);

            if (e.Index >= 0)
            {
                var selector = (ComboBox)sender;
                var bounds = Rectangle.Inflate(e.Bounds, -4, 0);
                TextRenderer.DrawText(e.Graphics, selector.Items[e.Index].ToString(), selector.Font, bounds, TextColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            e.DrawFocusRectangle();
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            _allowVisible = true;
            this.Show();

            this.WindowState = FormWindowState.Normal;
            this.Visible = true;

            this.Refresh();
            this.ShowInTaskbar = true;
        }

        private void checkBoxPrimaryMonitorOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (this._v == null)
            {
                return;
            }

            this._v.SetAffectPrimaryMonitorOnly(this.checkBoxPrimaryMonitorOnly.Checked);
            QueueSettingsSave();
        }
        
        private void checkBoxNeverChangeResolutions_CheckedChanged(object sender, EventArgs e)
        {
            if (this._v == null)
            {
                return;
            }

            this._v.SetNeverSwitchResolution(this.checkBoxNeverChangeResolutions.Checked);
            QueueSettingsSave();
        }

        private void checkBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryController autostartController = new RegistryController();
            if (this.checkBoxAutostart.Checked)
            {
                string pathToExe = "\"" + Application.ExecutablePath + "\" -minimized";
                if (!autostartController.IsProgramRegistered(AppName))
                {
                    this.notifyIcon.BalloonTipText = UiLanguage.Translate(autostartController.RegisterProgram(AppName, pathToExe)
                        ? "Registered to Autostart!"
                        : "Registering to Autostart failed!");
                }
                else if (!autostartController.IsStartupPathUnchanged(AppName, pathToExe))
                {
                    this.notifyIcon.BalloonTipText = UiLanguage.Translate(autostartController.RegisterProgram(AppName, pathToExe)
                        ? "Updated Autostart Path!"
                        : "Updating Autostart Path failed!");
                }
                else
                {
                    return;
                }
            }
            else
            {
                this.notifyIcon.BalloonTipText = UiLanguage.Translate(autostartController.UnregisterProgram(AppName)
                    ? "Unregistered from Autostart!"
                    : "Unregistering from Autostart failed!");
            }

            notifyIcon.ShowBalloonTip(250);
        }

        private void twitterToolStripTextBox_Click(object sender, EventArgs e)
        {
            Process.Start(TwitterLink);
        }

        private void SetGuiEnabledFlag(bool flag)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.trackBarWindowsLevel.Enabled = flag;
                this.checkBoxAutostart.Enabled = flag;
                this.checkBoxPrimaryMonitorOnly.Enabled = flag;
                this.checkBoxNeverChangeResolutions.Enabled = flag;
                this.buttonAddProgram.Enabled = flag;
                this.buttonProcessExplorer.Enabled = flag;
                this.listApplications.Enabled = flag;
                UpdateProfileActions();
            });
        }

        private void CleanUp()
        {
            try
            {
                SetStatusText("Closing...");
                this.statusLabel.ForeColor = WarningColor;
                this.Update();
                if (_v != null && _v.GetVibranceInfo().isInitialized)
                {
                    _v.HandleDvcExit();
                    _v.SetShouldRun(false);
                    _v.UnloadLibraryEx();
                }
            }
            catch (Exception ex)
            {
                Log(ex);
            }
        }

        public static void Log(Exception ex)
        {
            using (StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vibranceGUI.log")))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("Exception Found:\nType: {0}", ex.GetType().FullName);
                w.WriteLine("Message: {0}", ex.Message);
                w.WriteLine("Source: {0}", ex.Source);
                w.WriteLine("Stacktrace: {0}", ex.StackTrace);
                w.WriteLine("Exception String: {0}", ex.ToString());

                w.WriteLine("-------------------------------");
            }
        }

        public static void Log(string msg)
        {
            using (StreamWriter w = File.AppendText("vibranceGUI_log.txt"))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine(msg);
                w.WriteLine("-------------------------------");
            }
        }

        private void ReadVibranceSettings(out int vibranceWindowsLevel, out bool affectPrimaryMonitorOnly, out bool neverSwitchResolution)
        {
            _registryController = new RegistryController();
            this.checkBoxAutostart.Checked = _registryController.IsProgramRegistered(AppName);

            SettingsController settingsController = new SettingsController();
            settingsController.ReadVibranceSettings(_v.GraphicsAdapter, out vibranceWindowsLevel, out affectPrimaryMonitorOnly, out neverSwitchResolution, out _applicationSettings);

            if (this.IsHandleCreated)
            {
                //no null check needed, SettingsController will always return matching values.
                labelWindowsLevel.Text = _resolveLabelLevel(vibranceWindowsLevel);

                trackBarWindowsLevel.Value = vibranceWindowsLevel;
                checkBoxPrimaryMonitorOnly.Checked = affectPrimaryMonitorOnly;
                checkBoxNeverChangeResolutions.Checked = neverSwitchResolution;
                foreach (ApplicationSetting application in _applicationSettings.ToList())
                {
                    if (!File.Exists(application.FileName))
                    {
                        _applicationSettings.Remove(application);
                        continue;
                    }                        

                    InitializeApplicationList();

                    Icon icon = Icon.ExtractAssociatedIcon(application.FileName);
                    if (icon != null)
                    {
                        this.listApplications.LargeImageList.Images.Add(icon);
                        ListViewItem lvi = new ListViewItem(application.Name);
                        lvi.ImageIndex = this.listApplications.Items.Count;
                        lvi.Tag = application.FileName;
                        this.listApplications.Items.Add(lvi);
                    }
                }
                UpdateProfileActions();
                _settingsLoaded = true;
            }
        }

        private void ForceSaveVibranceSettings(bool showError = true)
        {
            settingsSaveTimer.Stop();
            SettingsController settingsController = new SettingsController();
            _settingsDirty = !settingsController.SetVibranceSettings(
                trackBarWindowsLevel.Value.ToString(),
                checkBoxPrimaryMonitorOnly.Checked.ToString(),
                checkBoxNeverChangeResolutions.Checked.ToString(),
                _applicationSettings
            );

            if (_settingsDirty && showError)
                ShowSaveError();
        }

        private void ShowSaveError()
        {
            MessageBox.Show(this, UiLanguage.Translate(SaveError), AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void buttonPaypal_Click(object sender, EventArgs e)
        {
            Process.Start(PaypalDonationLink);
        }

        private void buttonAddProgram_Click(object sender, EventArgs e)
        {
            InitializeApplicationList();

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = UiLanguage.Translate("Select a game executable");
            DialogResult result = fileDialog.ShowDialog();
            if (result == DialogResult.OK && fileDialog.CheckFileExists && fileDialog.SafeFileName != null 
                && _applicationSettings.FirstOrDefault(x => x.FileName.ToLower() == fileDialog.FileName.ToLower()) == null)
            {
                Icon icon = Icon.ExtractAssociatedIcon(fileDialog.FileName);
                if (icon != null)
                {
                    ProcessExplorerEntry processExplorerEntry = new ProcessExplorerEntry(fileDialog.FileName, icon, Path.GetFileNameWithoutExtension(fileDialog.FileName));
                    AddProgramIntern(processExplorerEntry);
                }
            }
        }

        public void AddProgramExtern(ProcessExplorerEntry processExplorerEntry)
        {
            if(this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    AddProgramIntern(processExplorerEntry);
                });
            }
            else
            {
                AddProgramIntern(processExplorerEntry);
            }
        }

        private void AddProgramIntern(ProcessExplorerEntry processExplorerEntry)
        {
            InitializeApplicationList();
            
            if(!File.Exists(processExplorerEntry.Path) || _applicationSettings.FirstOrDefault(x => x.FileName.ToLower() == processExplorerEntry.Path.ToLower()) != null)
            {
                this.listApplications.SelectedIndices.Clear();
                return; 
            }

            Icon icon = processExplorerEntry.Icon;
            string path = processExplorerEntry.Path;
            if (icon != null)
            {
                this.listApplications.LargeImageList.Images.Add(icon);
                ListViewItem lvi = new ListViewItem(Path.GetFileNameWithoutExtension(path));
                lvi.ImageIndex = this.listApplications.Items.Count;
                lvi.Tag = path;
                this.listApplications.Items.Add(lvi);
                UpdateProfileActions();
                this.listApplications.SelectedIndices.Clear();
                lvi.Selected = true;
                listApplications_DoubleClick(this, EventArgs.Empty);
            }
        }

        private void InitializeApplicationList()
        {
            if (this.listApplications.LargeImageList == null)
            {
                ImageList imageList = new ImageList(components);
                imageList.ImageSize = new Size(36, 36);
                imageList.ColorDepth = ColorDepth.Depth32Bit;
                this.listApplications.LargeImageList = imageList;
                this.listApplications.SmallImageList = imageList;
            }
        }

        private void listApplications_Resize(object sender, EventArgs e)
        {
            listApplications.Columns[0].Width = Math.Max(0, listApplications.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        }

        private void listApplications_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            bool selected = e.Item.Selected;
            Rectangle row = e.Bounds;
            using (var background = new SolidBrush(selected ? ButtonColor : InsetColor))
                e.Graphics.FillRectangle(background, row);

            if (selected)
            {
                using (var accent = new SolidBrush(AccentColor))
                    e.Graphics.FillRectangle(accent, row.Left, row.Top, 3, row.Height);
            }

            const int iconSize = 26;
            var imageList = listApplications.SmallImageList;
            if (imageList != null && e.Item.ImageIndex >= 0 && e.Item.ImageIndex < imageList.Images.Count)
                imageList.Draw(e.Graphics, row.Left + 11, row.Top + (row.Height - iconSize) / 2, iconSize, iconSize, e.Item.ImageIndex);

            var textBounds = new Rectangle(row.Left + 47, row.Top, Math.Max(0, row.Width - 55), row.Height);
            TextRenderer.DrawText(e.Graphics, e.Item.Text, listApplications.Font, textBounds, TextColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            if (listApplications.Focused && e.Item.Focused)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(row, -4, -1), TextColor, ButtonColor);
        }

        private void listApplications_DoubleClick(object sender, EventArgs e)
        {
            if (listApplications.SelectedItems.Count == 1)
            {
                ListViewItem selectedItem = listApplications.SelectedItems[0];
                ApplicationSetting actualSetting = _applicationSettings.FirstOrDefault(x => x.FileName == selectedItem.Tag.ToString());
                VibranceSettings settingsWindow = new VibranceSettings(_v, _minTrackBarValue, _maxTrackBarValue, _defaultIngameValue, selectedItem, actualSetting, _supportedResolutionList, _resolveLabelLevel);
                DialogResult result = settingsWindow.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ApplicationSetting newSetting = settingsWindow.GetApplicationSetting();
                    if (_applicationSettings.FirstOrDefault(x => x.FileName == newSetting.FileName) != null)
                    {
                        _applicationSettings.Remove(_applicationSettings.First(x => x.FileName == newSetting.FileName));
                    }
                    _applicationSettings.Add(newSetting);
                    ForceSaveVibranceSettings();
                }
                else if(actualSetting == null)
                {
                    removeApplicationListItem(selectedItem);
                }
            }
        }

        private void buttonRemoveProgram_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listApplications.SelectedItems.Cast<ListViewItem>().ToArray())
            {
                removeApplicationListItem(eachItem);
                _applicationSettings.Remove(_applicationSettings.FirstOrDefault(x => x.FileName.Equals(eachItem.Tag.ToString())));
            }

            ForceSaveVibranceSettings();
        }

        private void removeApplicationListItem(ListViewItem item)
        {
            for (int i = item.Index + 1; i < listApplications.Items.Count; i++)
                listApplications.Items[i].ImageIndex--;

            Image img = this.listApplications.LargeImageList.Images[item.ImageIndex];
            this.listApplications.LargeImageList.Images.RemoveAt(item.ImageIndex);
            img.Dispose();
            this.listApplications.Items.Remove(item);
            UpdateProfileActions();
        }

        private void listApplications_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProfileActions();
        }

        private void UpdateProfileActions()
        {
            int selected = listApplications.SelectedItems.Count;
            buttonEditProgram.Enabled = listApplications.Enabled && selected == 1;
            buttonRemoveProgram.Enabled = listApplications.Enabled && selected > 0;
            bool hasProfiles = listApplications.Items.Count > 0;
            listApplications.Visible = hasProfiles;
            emptyProfilesLabel.Visible = !hasProfiles;
        }

        private void buttonProcessExplorer_Click(object sender, EventArgs e)
        {
            ProcessExplorer ex = new ProcessExplorer(this);
            ex.Show();
        }
    }
}
