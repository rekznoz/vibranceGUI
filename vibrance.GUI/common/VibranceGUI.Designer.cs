using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace vibrance.GUI.common
{
    partial class VibranceGUI
    {
        internal static readonly Color PageColor = Color.FromArgb(17, 20, 29);
        internal static readonly Color CardColor = Color.FromArgb(26, 31, 43);
        internal static readonly Color InsetColor = Color.FromArgb(20, 24, 34);
        internal static readonly Color TextColor = Color.FromArgb(240, 244, 249);
        internal static readonly Color MutedColor = Color.FromArgb(164, 178, 196);
        private static readonly Color AccentColor = Color.FromArgb(175, 194, 255);
        private static readonly Color ActiveColor = Color.FromArgb(148, 225, 184);
        internal static readonly Color ButtonColor = Color.FromArgb(40, 48, 61);
        internal static readonly Color BorderColor = Color.FromArgb(54, 64, 85);
        private static readonly Color WarningColor = Color.FromArgb(255, 156, 153);

        private IContainer components;
        private KryptonCustomPaletteBase comboPalette;
        private Bitmap[] switchImages;
        private NotifyIcon notifyIcon;
        private BackgroundWorker backgroundWorker;
        private Timer settingsSaveTimer;
        private KryptonCheckBox checkBoxAutostart;
        private KryptonCheckBox checkBoxPrimaryMonitorOnly;
        private KryptonCheckBox checkBoxNeverChangeResolutions;
        private Label labelWindowsLevel;
        private KryptonTrackBar trackBarWindowsLevel;
        private Label statusLabel;
        private Label emptyProfilesLabel;
        private KryptonButton buttonProcessExplorer;
        private KryptonButton buttonAddProgram;
        private KryptonButton buttonRemoveProgram;
        private ListView listApplications;
        private Panel profileEditor;
        private Label profileEditorPlaceholder;
        private Label profileEditorTitle;
        private Label profileVibranceValue;
        private KryptonTrackBar profileVibranceSlider;
        private KryptonCheckBox profileChangeResolution;
        private KryptonComboBox profileResolution;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
            if (disposing && switchImages != null)
            {
                foreach (var image in switchImages)
                    image.Dispose();
                switchImages = null;
            }
        }

        private void InitializeComponent()
        {
            components = new Container();
            var theme = new KryptonManager(components) { GlobalPaletteMode = PaletteMode.Microsoft365BlackDarkMode };
            var resources = new ComponentResourceManager(typeof(VibranceGUI));
            var trayMenu = new ContextMenuStrip(components);
            var exitItem = new ToolStripMenuItem();
            BindTranslation("Exit", text => exitItem.Text = text);
            exitItem.Click += exitToolStripMenuItem_Click;
            trayMenu.Items.Add(exitItem);

            notifyIcon = new NotifyIcon(components);
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.ContextMenuStrip = trayMenu;
            notifyIcon.Text = "vibranceGUI";
            notifyIcon.BalloonTipTitle = "vibranceGUI";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.Visible = true;

            backgroundWorker = new BackgroundWorker();
            components.Add(backgroundWorker);
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;

            settingsSaveTimer = new Timer(components);
            settingsSaveTimer.Interval = 5000;
            settingsSaveTimer.Tick += settingsSaveTimer_Tick;

            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(7F, 15F);
            Font = new Font("Segoe UI", 9F);
            BackColor = PageColor;
            ForeColor = TextColor;
            ClientSize = new Size(928, 652);
            MinimumSize = new Size(868, 655);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "VibranceGUI";
            Text = "vibranceGUI";

            var dashboard = CreateTable(1, 5);
            dashboard.Padding = new Padding(24, 18, 24, 12);
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            dashboard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var header = CreateTable(3, 2);
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 56));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            var mark = CreateLabel("VG", AccentColor, 13F, FontStyle.Bold, translate: false);
            mark.BackColor = ButtonColor;
            mark.TextAlign = ContentAlignment.MiddleCenter;
            mark.Margin = new Padding(0, 4, 12, 8);
            header.Controls.Add(mark, 0, 0);
            header.SetRowSpan(mark, 2);
            header.Controls.Add(CreateLabel("vibranceGUI", TextColor, 22F, FontStyle.Bold, translate: false), 1, 0);
            header.Controls.Add(CreateLabel("Desktop color and game profiles", MutedColor), 1, 1);
            statusLabel = CreateLabel(UiLanguage.Translate(_statusText), MutedColor, 9F, FontStyle.Bold, translate: false);
            statusLabel.BackColor = ButtonColor;
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            statusLabel.Margin = new Padding(0, 4, 0, 0);
            header.Controls.Add(statusLabel, 2, 0);
            dashboard.Controls.Add(header, 0, 0);

            var desktop = CreateTable(2, 3);
            desktop.Padding = new Padding(16, 10, 16, 10);
            desktop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            desktop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            desktop.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            desktop.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
            desktop.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            desktop.Controls.Add(CreateLabel("Desktop vibrance", TextColor, 13F, FontStyle.Bold), 0, 0);
            desktop.Controls.Add(CreateLabel("Color level when no game profile is active.", MutedColor), 0, 1);
            trackBarWindowsLevel = CreateSlider("Desktop vibrance");
            desktop.Controls.Add(trackBarWindowsLevel, 0, 2);
            labelWindowsLevel = CreateVibranceValue("—");
            desktop.Controls.Add(labelWindowsLevel, 1, 2);
            trackBarWindowsLevel.ValueChanged += trackBarWindowsLevel_ValueChanged;
            dashboard.Controls.Add(CreateCard(desktop, new Padding(0, 0, 0, 12)), 0, 1);

            var workspace = CreateTable(2, 1);
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 348));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var profiles = CreateTable(1, 4);
            profiles.Padding = new Padding(16, 12, 16, 12);
            profiles.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            profiles.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            profiles.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            profiles.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            profiles.Controls.Add(CreateLabel("Game profiles", TextColor, 13F, FontStyle.Bold), 0, 0);
            profiles.Controls.Add(CreateLabel("Select a game to edit its profile.", MutedColor), 0, 1);
            var profileActions = CreateTable(3, 1);
            profileActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            profileActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            profileActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            buttonProcessExplorer = CreateButton("Add running game", primary: true);
            buttonAddProgram = CreateButton("Browse…");
            buttonRemoveProgram = CreateButton("Remove");
            buttonProcessExplorer.Margin = new Padding(0, 0, 5, 6);
            buttonAddProgram.Margin = new Padding(0, 0, 5, 6);
            buttonRemoveProgram.Margin = new Padding(0, 0, 0, 6);
            buttonProcessExplorer.Click += buttonProcessExplorer_Click;
            buttonAddProgram.Click += buttonAddProgram_Click;
            buttonRemoveProgram.Click += buttonRemoveProgram_Click;
            profileActions.Controls.Add(buttonProcessExplorer, 0, 0);
            profileActions.Controls.Add(buttonAddProgram, 1, 0);
            profileActions.Controls.Add(buttonRemoveProgram, 2, 0);
            profiles.Controls.Add(profileActions, 0, 2);

            var profileListArea = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 0), BackColor = InsetColor };
            listApplications = new ListView
            {
                Name = "listApplications",
                Dock = DockStyle.Fill,
                BackColor = InsetColor,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.None,
                View = View.Details,
                HeaderStyle = ColumnHeaderStyle.None,
                FullRowSelect = true,
                OwnerDraw = true,
                HideSelection = false,
                UseCompatibleStateImageBehavior = false
            };
            BindTranslation("Game profiles", text => listApplications.AccessibleName = text);
            BindTranslation("Select a game to edit its profile.", text => listApplications.AccessibleDescription = text);
            var gameColumn = listApplications.Columns.Add("");
            BindTranslation("Game profiles", text => gameColumn.Text = text);
            listApplications.Resize += listApplications_Resize;
            listApplications.DrawSubItem += listApplications_DrawSubItem;
            listApplications.SelectedIndexChanged += listApplications_SelectedIndexChanged;
            emptyProfilesLabel = CreateLabel("No game profiles yet\r\nAdd a running game or browse for an executable.", MutedColor);
            emptyProfilesLabel.TextAlign = ContentAlignment.MiddleCenter;
            profileListArea.Controls.AddRange(new Control[] { emptyProfilesLabel, listApplications });
            profiles.Controls.Add(profileListArea, 0, 3);
            workspace.Controls.Add(CreateCard(profiles, new Padding(0, 0, 12, 0)), 0, 0);

            var editor = CreateTable(2, 8);
            profileEditor = editor;
            editor.Padding = new Padding(16, 12, 16, 12);
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            foreach (int height in new[] { 33, 23, 33, 33, 33, 22 })
                editor.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            editor.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            profileEditorTitle = CreateLabel("", TextColor, 15F, FontStyle.Bold, translate: false);
            editor.Controls.Add(profileEditorTitle, 0, 0);
            editor.SetColumnSpan(profileEditorTitle, 2);
            editor.Controls.Add(CreateLabel("In-game vibrance", MutedColor), 0, 1);
            profileVibranceValue = CreateVibranceValue("");
            editor.Controls.Add(profileVibranceValue, 1, 2);
            profileVibranceSlider = CreateSlider("In-game vibrance");
            profileVibranceSlider.ValueChanged += profileVibranceSlider_ValueChanged;
            editor.Controls.Add(profileVibranceSlider, 0, 2);
            profileChangeResolution = CreateCheckBox("Change resolution in-game");
            profileChangeResolution.CheckedChanged += profileChangeResolution_CheckedChanged;
            editor.Controls.Add(profileChangeResolution, 0, 3);
            editor.SetColumnSpan(profileChangeResolution, 2);
            profileResolution = CreateCombo("In-game resolution");
            editor.Controls.Add(profileResolution, 0, 4);
            editor.SetColumnSpan(profileResolution, 2);
            var resolutionHint = CreateLabel("Windowed or borderless mode only", MutedColor, 8.5F);
            editor.Controls.Add(resolutionHint, 0, 5);
            editor.SetColumnSpan(resolutionHint, 2);

            var editorActions = CreateTable(3, 1);
            editorActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            editorActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
            editorActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 102));
            var cancelProfile = CreateButton("Cancel");
            var saveProfile = CreateButton("Save", primary: true);
            saveProfile.Margin = new Padding(8, 0, 0, 0);
            cancelProfile.Click += cancelProfile_Click;
            saveProfile.Click += saveProfile_Click;
            editorActions.Controls.Add(cancelProfile, 1, 0);
            editorActions.Controls.Add(saveProfile, 2, 0);
            editor.Controls.Add(editorActions, 0, 7);
            editor.SetColumnSpan(editorActions, 2);

            var editorArea = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            profileEditorPlaceholder = CreateLabel("Select a game to edit its profile.", MutedColor);
            profileEditorPlaceholder.TextAlign = ContentAlignment.MiddleCenter;
            editorArea.Controls.AddRange(new Control[] { profileEditorPlaceholder, profileEditor });
            workspace.Controls.Add(CreateCard(editorArea, Padding.Empty), 1, 0);
            dashboard.Controls.Add(workspace, 0, 2);

            var preferences = CreateTable(3, 1);
            preferences.Padding = new Padding(16, 8, 16, 8);
            for (int column = 0; column < 3; column++)
                preferences.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
            preferences.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            checkBoxAutostart = CreateCheckBox("Start with Windows");
            checkBoxPrimaryMonitorOnly = CreateCheckBox("Primary monitor only");
            checkBoxNeverChangeResolutions = CreateCheckBox("Keep Windows resolution");
            checkBoxAutostart.CheckedChanged += checkBoxAutostart_CheckedChanged;
            checkBoxPrimaryMonitorOnly.CheckedChanged += checkBoxPrimaryMonitorOnly_CheckedChanged;
            checkBoxNeverChangeResolutions.CheckedChanged += checkBoxNeverChangeResolutions_CheckedChanged;
            var toolTip = new ToolTip(components);
            BindTranslation("Limit vibrance changes to one monitor.", text => toolTip.SetToolTip(checkBoxPrimaryMonitorOnly, text));
            BindTranslation("Prevent game profiles from changing the display resolution.", text => toolTip.SetToolTip(checkBoxNeverChangeResolutions, text));
            preferences.Controls.Add(checkBoxAutostart, 0, 0);
            preferences.Controls.Add(checkBoxPrimaryMonitorOnly, 1, 0);
            preferences.Controls.Add(checkBoxNeverChangeResolutions, 2, 0);
            dashboard.Controls.Add(CreateCard(preferences, new Padding(0, 12, 0, 0)), 0, 3);

            var footer = CreateTable(3, 1);
            footer.Padding = new Padding(0, 9, 0, 0);
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            footer.Controls.Add(CreateLabel("Desktop settings autosave", MutedColor), 0, 0);
            footer.Controls.Add(CreateLabel("Language", MutedColor), 1, 0);
            var languageSelector = CreateCombo("Language");
            languageSelector.Name = "languageSelector";
            languageSelector.Items.AddRange(new object[] { "English", "Español", "Português" });
            languageSelector.SelectedIndex = UiLanguage.SelectedIndex;
            languageSelector.SelectedIndexChanged += languageSelector_SelectedIndexChanged;
            footer.Controls.Add(languageSelector, 2, 0);
            dashboard.Controls.Add(footer, 0, 4);

            Controls.Add(dashboard);
            UpdateProfileActions();
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            ResumeLayout(true);
        }

        internal static TableLayoutPanel CreateTable(int columns, int rows)
        {
            return new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = columns,
                RowCount = rows,
                Margin = Padding.Empty
            };
        }

        internal static KryptonGroup CreateCard(Control content, Padding margin)
        {
            var card = new KryptonGroup { Dock = DockStyle.Fill, Margin = margin };
            card.StateCommon.Back.Color1 = CardColor;
            card.StateCommon.Back.Color2 = CardColor;
            card.StateCommon.Border.Color1 = BorderColor;
            card.StateCommon.Border.Color2 = BorderColor;
            card.StateCommon.Border.DrawBorders = PaletteDrawBorders.All;
            card.StateCommon.Border.Width = 1;
            card.StateCommon.Border.Rounding = 12;
            content.BackColor = CardColor;
            card.Panel.Controls.Add(content);
            return card;
        }

        private Label CreateLabel(string text, Color color, float size = 9F, FontStyle style = FontStyle.Regular, bool translate = true)
        {
            var label = new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                AutoEllipsis = true,
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", size, style)
            };
            if (translate)
                BindTranslation(text, translation => label.Text = translation);
            return label;
        }

        private KryptonButton CreateButton(string text, bool primary = false)
        {
            var button = CreateDashboardButton(primary);
            BindTranslation(text, translation => button.Text = translation);
            return button;
        }

        internal static KryptonButton CreateDashboardButton(bool primary = false)
        {
            var button = new KryptonButton { Dock = DockStyle.Fill, Margin = Padding.Empty };
            var background = primary ? AccentColor : ButtonColor;
            var border = primary ? AccentColor : BorderColor;
            button.StateCommon.Back.Color1 = background;
            button.StateCommon.Back.Color2 = background;
            button.StateCommon.Border.Color1 = border;
            button.StateCommon.Border.Color2 = border;
            button.StateCommon.Border.DrawBorders = PaletteDrawBorders.All;
            button.StateCommon.Border.Rounding = 7;
            button.StateCommon.Border.Width = 1;
            button.StateCommon.Content.ShortText.Color1 = primary ? PageColor : TextColor;
            button.StateCommon.Content.ShortText.Color2 = primary ? PageColor : TextColor;
            button.StateTracking.Back.Color1 = primary ? Color.FromArgb(197, 211, 255) : BorderColor;
            button.StateTracking.Back.Color2 = button.StateTracking.Back.Color1;
            button.StatePressed.Back.Color1 = primary ? Color.FromArgb(149, 171, 239) : CardColor;
            button.StatePressed.Back.Color2 = button.StatePressed.Back.Color1;
            button.StateDisabled.Content.ShortText.Color1 = MutedColor;
            return button;
        }

        private KryptonCheckBox CreateCheckBox(string text)
        {
            var checkBox = new KryptonCheckBox { Dock = DockStyle.Fill, Margin = Padding.Empty, AutoSize = false };
            checkBox.StateCommon.ShortText.Color1 = TextColor;
            checkBox.StateCommon.ShortText.Color2 = TextColor;
            checkBox.StateCommon.AdjacentGap = 8;
            if (switchImages == null)
            {
                switchImages = new[]
                {
                    DrawSwitch(false, BorderColor, MutedColor),
                    DrawSwitch(false, Color.FromArgb(74, 87, 115), TextColor),
                    DrawSwitch(false, ButtonColor, Color.FromArgb(108, 121, 141)),
                    DrawSwitch(true, AccentColor, PageColor),
                    DrawSwitch(true, Color.FromArgb(197, 211, 255), PageColor),
                    DrawSwitch(true, Color.FromArgb(84, 98, 137), CardColor)
                };
            }
            checkBox.Images.UncheckedNormal = switchImages[0];
            checkBox.Images.UncheckedTracking = checkBox.Images.UncheckedPressed = switchImages[1];
            checkBox.Images.UncheckedDisabled = switchImages[2];
            checkBox.Images.CheckedNormal = switchImages[3];
            checkBox.Images.CheckedTracking = checkBox.Images.CheckedPressed = switchImages[4];
            checkBox.Images.CheckedDisabled = switchImages[5];
            BindTranslation(text, translation => checkBox.Text = translation);
            return checkBox;
        }

        private static Bitmap DrawSwitch(bool enabled, Color trackColor, Color thumbColor)
        {
            var image = new Bitmap(34, 20);
            using (var graphics = Graphics.FromImage(image))
            using (var track = new SolidBrush(trackColor))
            using (var thumb = new SolidBrush(thumbColor))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillEllipse(track, 1, 2, 16, 16);
                graphics.FillEllipse(track, 17, 2, 16, 16);
                graphics.FillRectangle(track, 9, 2, 16, 16);
                graphics.FillEllipse(thumb, enabled ? 19 : 3, 4, 12, 12);
            }
            return image;
        }

        private Label CreateVibranceValue(string text)
        {
            var label = CreateLabel(text, AccentColor, 18F, FontStyle.Bold, translate: false);
            label.TextAlign = ContentAlignment.MiddleRight;
            label.AutoEllipsis = false;
            return label;
        }

        private KryptonTrackBar CreateSlider(string name)
        {
            var slider = new KryptonTrackBar
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = Padding.Empty,
                AutoSize = false,
                Height = 22,
                Maximum = 63,
                TickStyle = TickStyle.None,
                TrackBarSize = PaletteTrackBarSize.Medium,
                DrawBackground = false
            };
            slider.StateCommon.Track.Color1 = BorderColor;
            slider.StateCommon.Track.Color2 = BorderColor;
            slider.StateCommon.Position.Color1 = AccentColor;
            slider.StateCommon.Position.Color2 = AccentColor;
            slider.StateTracking.Position.Color1 = TextColor;
            slider.StateTracking.Position.Color2 = TextColor;
            BindTranslation(name, text => slider.AccessibleName = text);
            return slider;
        }

        private KryptonComboBox CreateCombo(string name)
        {
            if (comboPalette == null)
            {
                comboPalette = new KryptonCustomPaletteBase(components)
                {
                    BasePalette = KryptonManager.CurrentGlobalPalette,
                    BaseRenderer = new ComboChevronRenderer()
                };
                var dropButton = comboPalette.ButtonStyles.ButtonLowProfile.StateCommon;
                dropButton.Back.Draw = InheritBool.False;
                dropButton.Border.Draw = InheritBool.False;
            }
            var combo = new KryptonComboBox
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DropButtonStyle = ButtonStyle.LowProfile,
                LocalCustomPalette = comboPalette
            };
            combo.StateCommon.ComboBox.Back.Color1 = InsetColor;
            combo.StateCommon.ComboBox.Content.Color1 = TextColor;
            combo.StateCommon.ComboBox.Border.DrawBorders = PaletteDrawBorders.All;
            combo.StateCommon.ComboBox.Border.Color1 = BorderColor;
            combo.StateCommon.ComboBox.Border.Rounding = 6;
            BindTranslation(name, text => combo.AccessibleName = text);
            return combo;
        }

        private sealed class ComboChevronRenderer : RenderStandard
        {
            public override void DrawInputControlDropDownGlyph(RenderContext context, Rectangle bounds,
                IPaletteContent palette, PaletteState state)
            {
                var graphics = context.Graphics;
                float scale = graphics.DpiX / 96F;
                float x = bounds.Left + bounds.Width / 2F;
                float y = bounds.Top + bounds.Height / 2F;
                Color color = state == PaletteState.Disabled ? MutedColor : AccentColor;
                var smoothing = graphics.SmoothingMode;
                try
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var pen = new Pen(color, 1.5F * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                    {
                        graphics.DrawLines(pen, new[]
                        {
                            new PointF(x - 3.5F * scale, y - 1.5F * scale),
                            new PointF(x, y + 2F * scale),
                            new PointF(x + 3.5F * scale, y - 1.5F * scale)
                        });
                    }
                }
                finally
                {
                    graphics.SmoothingMode = smoothing;
                }
            }
        }
    }
}
