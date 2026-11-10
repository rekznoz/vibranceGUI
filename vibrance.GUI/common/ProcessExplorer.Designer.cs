using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static vibrance.GUI.common.VibranceGUI;

namespace vibrance.GUI.common
{
    partial class ProcessExplorer
    {
        private IContainer components;
        private KryptonDataGridView processGrid;
        private KryptonButton reloadButton;
        private KryptonButton addButton;
        private Label heading;
        private Label description;
        private Label processStatus;
        private BackgroundWorker backgroundWorker;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearProcesses();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            var resources = new ComponentResourceManager(typeof(ProcessExplorer));
            backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
            components.Add(backgroundWorker);
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            AutoScaleDimensions = new SizeF(7F, 15F);
            Font = new Font("Segoe UI", 9F);
            BackColor = PageColor;
            ForeColor = TextColor;
            ClientSize = new Size(760, 448);
            MinimumSize = new Size(590, 320);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ProcessExplorer";

            var layout = CreateTable(1, 4);
            layout.Padding = new Padding(22, 16, 22, 14);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 47));

            heading = new Label
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            description = new Label
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                ForeColor = MutedColor,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft
            };
            layout.Controls.Add(heading, 0, 0);
            layout.Controls.Add(description, 0, 1);

            processGrid = new KryptonDataGridView
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 33,
                ScrollBars = ScrollBars.Vertical,
                ShowCellToolTips = true,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.None
            };
            processGrid.RowTemplate.Height = 35;
            processGrid.StateCommon.Background.Color1 = InsetColor;
            processGrid.StateCommon.DataCell.Back.Color1 = InsetColor;
            processGrid.StateCommon.DataCell.Back.Color2 = InsetColor;
            processGrid.StateCommon.DataCell.Border.Color1 = CardColor;
            processGrid.StateCommon.DataCell.Border.Color2 = CardColor;
            processGrid.StateCommon.DataCell.Content.Color1 = TextColor;
            processGrid.StateCommon.HeaderColumn.Back.Color1 = CardColor;
            processGrid.StateCommon.HeaderColumn.Back.Color2 = CardColor;
            processGrid.StateCommon.HeaderColumn.Border.Color1 = BorderColor;
            processGrid.StateCommon.HeaderColumn.Border.Color2 = BorderColor;
            processGrid.StateCommon.HeaderColumn.Content.Color1 = MutedColor;
            processGrid.StateSelected.DataCell.Back.Color1 = ButtonColor;
            processGrid.StateSelected.DataCell.Back.Color2 = ButtonColor;
            processGrid.StateSelected.DataCell.Content.Color1 = TextColor;

            var programs = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 198,
                MinimumWidth = 116,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            programs.DefaultCellStyle.Padding = new Padding(29, 0, 0, 0);
            var paths = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 180,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            processGrid.Columns.AddRange(programs, paths);
            processGrid.CellPainting += processGrid_CellPainting;
            processGrid.CellDoubleClick += processGrid_CellDoubleClick;
            processGrid.SelectionChanged += processGrid_SelectionChanged;

            var tableHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            tableHost.Controls.Add(processGrid);
            layout.Controls.Add(CreateCard(tableHost, Padding.Empty), 0, 2);

            var footer = CreateTable(3, 1);
            footer.Padding = new Padding(0, 10, 0, 0);
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 146));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156));
            processStatus = new Label
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                ForeColor = MutedColor,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            reloadButton = CreateDashboardButton();
            reloadButton.Margin = new Padding(0, 0, 8, 0);
            reloadButton.Click += reloadButton_Click;
            addButton = CreateDashboardButton(primary: true);
            addButton.OverrideDefault.Back.Color1 = addButton.StateCommon.Back.Color1;
            addButton.OverrideDefault.Back.Color2 = addButton.StateCommon.Back.Color2;
            addButton.OverrideDefault.Border.Color1 = addButton.StateCommon.Border.Color1;
            addButton.OverrideDefault.Border.Color2 = addButton.StateCommon.Border.Color2;
            addButton.OverrideDefault.Content.ShortText.Color1 = addButton.StateCommon.Content.ShortText.Color1;
            addButton.OverrideDefault.Content.ShortText.Color2 = addButton.StateCommon.Content.ShortText.Color2;
            addButton.Enabled = false;
            addButton.Click += addButton_Click;
            footer.Controls.Add(processStatus, 0, 0);
            footer.Controls.Add(reloadButton, 1, 0);
            footer.Controls.Add(addButton, 2, 0);
            layout.Controls.Add(footer, 0, 3);
            Controls.Add(layout);
            AcceptButton = addButton;
            ResumeLayout(true);
        }
    }
}
