using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace vibrance.GUI.common
{
    public partial class ProcessExplorer : Form
    {
        private readonly VibranceGUI vibranceGui;

        public ProcessExplorer(Form vibranceGui)
        {
            this.vibranceGui = (VibranceGUI)vibranceGui;
            InitializeComponent();
            RefreshLanguage();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            VibranceGUI.ApplyDarkCaption(Handle);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ReloadProcesses();
        }

        internal void RefreshLanguage()
        {
            heading.Text = UiLanguage.Translate("Process explorer");
            Text = "vibranceGUI · " + heading.Text;
            description.Text = UiLanguage.Translate("Choose a running application to add to your game profiles.");
            reloadButton.Text = UiLanguage.Translate("Reload processes");
            addButton.Text = UiLanguage.Translate("Add selected");
            processGrid.AccessibleName = heading.Text;
            processGrid.Columns[0].HeaderText = UiLanguage.Translate("Programs");
            processGrid.Columns[1].HeaderText = UiLanguage.Translate("Full path");
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            processStatus.Text = backgroundWorker.IsBusy
                ? UiLanguage.Translate("Looking for running applications…")
                : processGrid.Rows.Count == 0 ? UiLanguage.Translate("No running applications found.") : "";
        }

        private void ReloadProcesses()
        {
            if (backgroundWorker.IsBusy)
                return;

            ClearProcesses();
            reloadButton.Enabled = false;
            backgroundWorker.RunWorkerAsync();
            RefreshStatus();
        }

        private void ClearProcesses()
        {
            if (processGrid == null || processGrid.IsDisposed)
                return;

            foreach (DataGridViewRow row in processGrid.Rows)
            {
                if (row.Tag is ProcessExplorerEntry entry)
                    entry.Icon?.Dispose();
            }
            processGrid.Rows.Clear();
        }

        private void AddSelectedProcess()
        {
            if (processGrid.SelectedRows.Count != 1 || !(processGrid.SelectedRows[0].Tag is ProcessExplorerEntry entry))
                return;

            Hide();
            vibranceGui.AddProgramExtern(entry);
            Close();
        }

        private void processGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0)
                return;

            e.Paint(e.ClipBounds, e.PaintParts);
            if (processGrid.Rows[e.RowIndex].Tag is ProcessExplorerEntry entry && entry.Icon != null)
            {
                const int iconSize = 18;
                e.Graphics.DrawIcon(entry.Icon, new Rectangle(e.CellBounds.X + 8,
                    e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2, iconSize, iconSize));
            }
            e.Handled = true;
        }

        private void processGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                AddSelectedProcess();
        }

        private void processGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (addButton != null)
                addButton.Enabled = processGrid.SelectedRows.Count == 1;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddSelectedProcess();
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            ReloadProcesses();
        }

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr process, IntPtr module, StringBuilder path, uint size);

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int ownId;
            using (var ownProcess = Process.GetCurrentProcess())
                ownId = ownProcess.Id;

            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    try
                    {
                        if (process.Id == ownId || process.MainWindowHandle == IntPtr.Zero)
                            continue;

                        var path = new StringBuilder(1024);
                        if (GetModuleFileNameEx(process.Handle, IntPtr.Zero, path, (uint)path.Capacity) == 0)
                            continue;

                        string executable = path.ToString();
                        if (File.Exists(executable))
                        {
                            var icon = Icon.ExtractAssociatedIcon(executable);
                            backgroundWorker.ReportProgress(0, new ProcessExplorerEntry(executable, icon, process));
                        }
                    }
                    catch (Exception ex)
                    {
                        VibranceGUI.Log(ex);
                    }
                }
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!(e.UserState is ProcessExplorerEntry entry))
                return;

            if (IsDisposed || Disposing)
            {
                entry.Icon?.Dispose();
                return;
            }

            int index = processGrid.Rows.Add(entry.ProcessName, entry.Path);
            DataGridViewRow row = processGrid.Rows[index];
            row.Tag = entry;
            row.Cells[1].ToolTipText = entry.Path;
            if (index == 0)
                processGrid.ClearSelection();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            reloadButton.Enabled = true;
            RefreshStatus();
        }
    }
}
