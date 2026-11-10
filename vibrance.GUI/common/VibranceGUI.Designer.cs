using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    partial class VibranceGUI
    {
        private static readonly Color PageColor = Color.FromArgb(15, 18, 24);
        private static readonly Color CardColor = Color.FromArgb(24, 29, 38);
        private static readonly Color InsetColor = Color.FromArgb(17, 21, 28);
        private static readonly Color TextColor = Color.FromArgb(240, 244, 249);
        private static readonly Color MutedColor = Color.FromArgb(164, 178, 196);
        private static readonly Color AccentColor = Color.FromArgb(174, 245, 123);
        private static readonly Color ButtonColor = Color.FromArgb(40, 48, 61);
        private static readonly Color BorderColor = Color.FromArgb(59, 72, 89);
        private static readonly Color WarningColor = Color.FromArgb(255, 156, 153);

        private IContainer components;
        private NotifyIcon notifyIcon;
        private BackgroundWorker backgroundWorker;
        private Timer settingsSaveTimer;
        private CheckBox checkBoxAutostart;
        private CheckBox checkBoxPrimaryMonitorOnly;
        private CheckBox checkBoxNeverChangeResolutions;
        private Label labelWindowsLevel;
        private TrackBar trackBarWindowsLevel;
        private Label statusLabel;
        private Label emptyProfilesLabel;
        private Button buttonProcessExplorer;
        private Button buttonAddProgram;
        private Button buttonEditProgram;
        private Button buttonRemoveProgram;
        private ListView listApplications;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            var resources = new ComponentResourceManager(typeof(VibranceGUI));
            var trayMenu = new ContextMenuStrip(components);
            var updatesItem = new ToolStripMenuItem("Updates");
            var exitItem = new ToolStripMenuItem("Exit");
            updatesItem.Click += twitterToolStripTextBox_Click;
            exitItem.Click += exitToolStripMenuItem_Click;
            trayMenu.Items.AddRange(new ToolStripItem[] { updatesItem, exitItem });

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
            ClientSize = new Size(576, 594);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "VibranceGUI";
            Text = "vibranceGUI";

            var mark = CreateLabel("VG", 24, 24, AccentColor, 13F, FontStyle.Bold);
            mark.AutoSize = false;
            mark.Size = new Size(44, 44);
            mark.BackColor = ButtonColor;
            mark.TextAlign = ContentAlignment.MiddleCenter;
            var title = CreateLabel("vibranceGUI", 80, 19, TextColor, 20F, FontStyle.Bold);
            var subtitle = CreateLabel("Desktop color and game profiles", 82, 54, MutedColor);
            statusLabel = CreateLabel("Starting", 422, 29, MutedColor, 9F, FontStyle.Bold);
            statusLabel.AutoSize = false;
            statusLabel.Size = new Size(130, 30);
            statusLabel.BackColor = CardColor;
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;

            var desktopCard = new Panel { Location = new Point(24, 88), Size = new Size(528, 112), BackColor = CardColor, TabIndex = 0 };
            var desktopHeading = CreateLabel("Desktop vibrance", 16, 12, TextColor, 13F, FontStyle.Bold);
            var desktopDescription = CreateLabel("Color level when no game profile is active.", 17, 41, MutedColor);
            labelWindowsLevel = CreateLabel("—", 413, 26, AccentColor, 26F, FontStyle.Bold);
            labelWindowsLevel.AutoSize = false;
            labelWindowsLevel.Size = new Size(98, 56);
            labelWindowsLevel.TextAlign = ContentAlignment.MiddleRight;
            trackBarWindowsLevel = new TrackBar
            {
                Name = "trackBarWindowsLevel",
                AccessibleName = "Desktop vibrance",
                Location = new Point(13, 65),
                Size = new Size(392, 30),
                AutoSize = false,
                Maximum = 63,
                TickStyle = TickStyle.None,
                BackColor = CardColor
            };
            trackBarWindowsLevel.Scroll += trackBarWindowsLevel_Scroll;
            desktopCard.Controls.AddRange(new Control[] { desktopHeading, desktopDescription, labelWindowsLevel, trackBarWindowsLevel });

            var profilesCard = new Panel { Location = new Point(24, 212), Size = new Size(528, 238), BackColor = CardColor, TabIndex = 1 };
            var profilesHeading = CreateLabel("Game profiles", 16, 12, TextColor, 13F, FontStyle.Bold);
            var profilesDescription = CreateLabel("Set a separate color level and display mode for each game.", 17, 41, MutedColor);
            buttonProcessExplorer = CreateButton("Add running game", 16, 67, 139, 32, 0, primary: true);
            buttonAddProgram = CreateButton("Browse…", 163, 67, 89, 32, 1);
            buttonEditProgram = CreateButton("Edit", 356, 67, 67, 32, 2);
            buttonRemoveProgram = CreateButton("Remove", 431, 67, 81, 32, 3);
            buttonProcessExplorer.Click += buttonProcessExplorer_Click;
            buttonAddProgram.Click += buttonAddProgram_Click;
            buttonEditProgram.Click += listApplications_DoubleClick;
            buttonRemoveProgram.Click += buttonRemoveProgram_Click;

            listApplications = new ListView
            {
                Name = "listApplications",
                AccessibleName = "Game profiles",
                AccessibleDescription = "Select a game profile and choose Edit to change it.",
                Location = new Point(16, 111),
                Size = new Size(496, 111),
                BackColor = InsetColor,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.None,
                HideSelection = false,
                TabIndex = 4,
                UseCompatibleStateImageBehavior = false
            };
            listApplications.DoubleClick += listApplications_DoubleClick;
            listApplications.SelectedIndexChanged += listApplications_SelectedIndexChanged;
            emptyProfilesLabel = CreateLabel("No game profiles yet\r\nAdd a running game or browse for an executable.",
                16, 111, MutedColor);
            emptyProfilesLabel.AutoSize = false;
            emptyProfilesLabel.Size = listApplications.Size;
            emptyProfilesLabel.TextAlign = ContentAlignment.MiddleCenter;
            emptyProfilesLabel.BackColor = InsetColor;
            profilesCard.Controls.AddRange(new Control[] { profilesHeading, profilesDescription, buttonProcessExplorer,
                buttonAddProgram, buttonEditProgram, buttonRemoveProgram, emptyProfilesLabel, listApplications });

            var preferencesCard = new Panel { Location = new Point(24, 462), Size = new Size(528, 78), BackColor = CardColor, TabIndex = 2 };
            var preferencesHeading = CreateLabel("Preferences", 16, 11, TextColor, 13F, FontStyle.Bold);
            checkBoxAutostart = CreateCheckBox("Start with Windows", 17, 43, 0);
            checkBoxPrimaryMonitorOnly = CreateCheckBox("Primary monitor only", 175, 43, 1);
            checkBoxNeverChangeResolutions = CreateCheckBox("Keep Windows resolution", 338, 43, 2);
            checkBoxAutostart.CheckedChanged += checkBoxAutostart_CheckedChanged;
            checkBoxPrimaryMonitorOnly.CheckedChanged += checkBoxPrimaryMonitorOnly_CheckedChanged;
            checkBoxNeverChangeResolutions.CheckedChanged += checkBoxNeverChangeResolutions_CheckedChanged;
            var toolTip = new ToolTip(components);
            toolTip.SetToolTip(checkBoxPrimaryMonitorOnly, "Limit vibrance changes to one monitor.");
            toolTip.SetToolTip(checkBoxNeverChangeResolutions, "Prevent game profiles from changing the display resolution.");
            preferencesCard.Controls.AddRange(new Control[] { preferencesHeading, checkBoxAutostart,
                checkBoxPrimaryMonitorOnly, checkBoxNeverChangeResolutions });

            var footer = CreateLabel("Changes are saved automatically", 24, 562, MutedColor);
            var updates = CreateButton("Updates", 386, 551, 80, 31, 3);
            var donate = CreateButton("Donate", 472, 551, 80, 31, 4);
            updates.Click += twitterToolStripTextBox_Click;
            donate.Click += buttonPaypal_Click;

            Controls.AddRange(new Control[] { mark, title, subtitle, statusLabel, desktopCard, profilesCard,
                preferencesCard, footer, updates, donate });
            UpdateProfileActions();
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Shown += Form1_Shown;
            Resize += Form1_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        private static Label CreateLabel(string text, int x, int y, Color color, float size = 9F, FontStyle style = FontStyle.Regular)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = color,
                Font = new Font("Segoe UI", size, style)
            };
        }

        private static Button CreateButton(string text, int x, int y, int width, int height, int tabIndex, bool primary = false)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                TabIndex = tabIndex,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                BackColor = primary ? AccentColor : ButtonColor,
                ForeColor = primary ? PageColor : TextColor,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = primary ? AccentColor : BorderColor;
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(197, 255, 161) : BorderColor;
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(149, 220, 101) : CardColor;
            return button;
        }

        private static CheckBox CreateCheckBox(string text, int x, int y, int tabIndex)
        {
            return new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                TabIndex = tabIndex,
                ForeColor = TextColor,
                UseVisualStyleBackColor = false
            };
        }
    }
}
