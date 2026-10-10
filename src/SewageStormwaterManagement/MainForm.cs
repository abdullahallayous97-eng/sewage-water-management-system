using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace SewageStormwaterManagement;

public class MainForm : Form
{
    private readonly TabControl _tabControl = new();
    private readonly DataGridView _areasGrid = new();
    private readonly DataGridView _sewageGrid = new();
    private readonly DataGridView _rainwaterGrid = new();
    private readonly DataGridView _inspectionGrid = new();
    private readonly DataGridView _maintenanceGrid = new();
    private readonly Label _areaCountLabel = new();
    private readonly Label _sewageCountLabel = new();
    private readonly Label _rainwaterCountLabel = new();
    private readonly Label _inspectionCountLabel = new();
    private readonly Label _maintenancePendingLabel = new();

    public MainForm()
    {
        Text = "برنامج إدارة شبكات الصرف الصحي ومياه الأمطار";
        Width = 1200;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Information;

        BuildInterface();
        RefreshAll();
    }

    private void BuildInterface()
    {
        var rootPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };

        rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        rootPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var headerPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(28, 48, 74) };
        var titleLabel = new Label
        {
            Text = "لوحة التحكم الرئيسية",
            ForeColor = Color.White,
            Font = new Font("Tahoma", 16F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 30)
        };
        headerPanel.Controls.Add(titleLabel);

        var actionBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(5),
            BackColor = Color.Transparent
        };

        var refreshButton = CreateButton("تحديث", Color.FromArgb(0, 123, 255));
        var generateReportButton = CreateButton("إنشاء تقرير", Color.FromArgb(40, 167, 69));
        var addMaintenanceButton = CreateButton("إضافة صيانة", Color.FromArgb(255, 193, 7));
        var addInspectionButton = CreateButton("إضافة فحص", Color.FromArgb(23, 162, 184));
        var addRainButton = CreateButton("إضافة أمطار", Color.FromArgb(108, 117, 125));
        var addSewageButton = CreateButton("إضافة صرف", Color.FromArgb(220, 53, 69));
        var addAreaButton = CreateButton("إضافة منطقة", Color.FromArgb(102, 16, 242));

        refreshButton.Click += (sender, e) => RefreshAll();
        addAreaButton.Click += (sender, e) => AddArea();
        addSewageButton.Click += (sender, e) => AddSewageNetwork();
        addRainButton.Click += (sender, e) => AddRainwaterNetwork();
        addInspectionButton.Click += (sender, e) => AddInspection();
        addMaintenanceButton.Click += (sender, e) => AddMaintenance();
        generateReportButton.Click += (sender, e) => GenerateReport();

        actionBar.Controls.AddRange(new Control[]
        {
            refreshButton,
            generateReportButton,
            addMaintenanceButton,
            addInspectionButton,
            addRainButton,
            addSewageButton,
            addAreaButton
        });

        headerPanel.Controls.Add(actionBar);
        rootPanel.Controls.Add(headerPanel);

        _tabControl.Dock = DockStyle.Fill;

        var dashboardTab = new TabPage("لوحة التحكم");
        var dashboardPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(245, 247, 250)
        };

        dashboardPanel.Controls.Add(CreateSummaryCard("المناطق", _areaCountLabel, Color.FromArgb(102, 16, 242)));
        dashboardPanel.Controls.Add(CreateSummaryCard("شبكات الصرف", _sewageCountLabel, Color.FromArgb(220, 53, 69)));
        dashboardPanel.Controls.Add(CreateSummaryCard("شبكات الأمطار", _rainwaterCountLabel, Color.FromArgb(108, 117, 125)));
        dashboardPanel.Controls.Add(CreateSummaryCard("الفحوصات", _inspectionCountLabel, Color.FromArgb(23, 162, 184)));
        dashboardPanel.Controls.Add(CreateSummaryCard("الصيانة المعلقة", _maintenancePendingLabel, Color.FromArgb(255, 193, 7)));
        dashboardTab.Controls.Add(dashboardPanel);

        _tabControl.TabPages.Add(dashboardTab);
        AddDataTab("المناطق", _areasGrid);
        AddDataTab("شبكات الصرف", _sewageGrid);
        AddDataTab("شبكات الأمطار", _rainwaterGrid);
        AddDataTab("الفحوصات", _inspectionGrid);
        AddDataTab("الصيانة", _maintenanceGrid);

        rootPanel.Controls.Add(_tabControl);
        Controls.Add(rootPanel);
    }

    private void AddDataTab(string title, DataGridView grid)
    {
        var tabPage = new TabPage(title);
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.Fixed3D;
        grid.RowHeadersVisible = false;
        grid.Font = new Font("Tahoma", 10F);

        tabPage.Controls.Add(grid);
        _tabControl.TabPages.Add(tabPage);
    }

    private void RefreshAll()
    {
        var summary = DatabaseService.GetDashboardSummary();
        _areaCountLabel.Text = summary.AreaCount.ToString();
        _sewageCountLabel.Text = summary.SewageCount.ToString();
        _rainwaterCountLabel.Text = summary.RainwaterCount.ToString();
        _inspectionCountLabel.Text = summary.InspectionCount.ToString();
        _maintenancePendingLabel.Text = summary.MaintenancePending.ToString();

        _areasGrid.DataSource = DatabaseService.GetAreas();
        _sewageGrid.DataSource = DatabaseService.GetSewageNetwork();
        _rainwaterGrid.DataSource = DatabaseService.GetRainwaterNetwork();
        _inspectionGrid.DataSource = DatabaseService.GetInspections();
        _maintenanceGrid.DataSource = DatabaseService.GetMaintenanceRecords();
    }

    private void AddArea()
    {
        var name = Interaction.InputBox("أدخل اسم المنطقة:", "إضافة منطقة", "");
        if (string.IsNullOrWhiteSpace(name)) return;

        var description = Interaction.InputBox("أدخل وصف المنطقة:", "إضافة منطقة", "");
        DatabaseService.InsertArea(name.Trim(), description.Trim());
        RefreshAll();
        MessageBox.Show("تمت إضافة المنطقة بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AddSewageNetwork()
    {
        var name = Interaction.InputBox("أدخل اسم خط الصرف:", "إضافة خط صرف", "");
        if (string.IsNullOrWhiteSpace(name)) return;

        var zone = Interaction.InputBox("أدخل المنطقة:", "إضافة خط صرف", "");
        var status = Interaction.InputBox("أدخل الحالة:", "إضافة خط صرف", "مستقر");
        var description = Interaction.InputBox("أدخل الوصف:", "إضافة خط صرف", "");

        DatabaseService.InsertAsset("SewageNetwork", name.Trim(), zone.Trim(), status.Trim(), description.Trim());
        RefreshAll();
        MessageBox.Show("تمت إضافة شبكة الصرف بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AddRainwaterNetwork()
    {
        var name = Interaction.InputBox("أدخل اسم شبكة مياه الأمطار:", "إضافة شبكة أمطار", "");
        if (string.IsNullOrWhiteSpace(name)) return;

        var zone = Interaction.InputBox("أدخل المنطقة:", "إضافة شبكة أمطار", "");
        var status = Interaction.InputBox("أدخل الحالة:", "إضافة شبكة أمطار", "مستقر");
        var description = Interaction.InputBox("أدخل الوصف:", "إضافة شبكة أمطار", "");

        DatabaseService.InsertAsset("RainwaterNetwork", name.Trim(), zone.Trim(), status.Trim(), description.Trim());
        RefreshAll();
        MessageBox.Show("تمت إضافة شبكة مياه الأمطار بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AddInspection()
    {
        var assetType = Interaction.InputBox("أدخل نوع الأصل (صرف صحي / مياه أمطار):", "إضافة فحص", "الصرف الصحي");
        if (string.IsNullOrWhiteSpace(assetType)) return;

        var assetName = Interaction.InputBox("أدخل اسم الأصل:", "إضافة فحص", "");
        var inspector = Interaction.InputBox("أدخل اسم المفتش:", "إضافة فحص", "");
        var condition = Interaction.InputBox("أدخل حالة الفحص:", "إضافة فحص", "جيد");
        var notes = Interaction.InputBox("أدخل ملاحظات الفحص:", "إضافة فحص", "");

        DatabaseService.InsertInspection(assetType.Trim(), assetName.Trim(), inspector.Trim(), DateTime.Today.ToString("yyyy-MM-dd"), condition.Trim(), notes.Trim());
        RefreshAll();
        MessageBox.Show("تمت إضافة الفحص بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AddMaintenance()
    {
        var workType = Interaction.InputBox("أدخل نوع العمل:", "إضافة صيانة", "تنظيف");
        if (string.IsNullOrWhiteSpace(workType)) return;

        var location = Interaction.InputBox("أدخل الموقع:", "إضافة صيانة", "");
        var assignedTo = Interaction.InputBox("أدخل اسم الموظف:", "إضافة صيانة", "");
        var status = Interaction.InputBox("أدخل الحالة:", "إضافة صيانة", "مجدول");
        var notes = Interaction.InputBox("أدخل ملاحظات الصيانة:", "إضافة صيانة", "");

        DatabaseService.InsertMaintenance(workType.Trim(), location.Trim(), assignedTo.Trim(), status.Trim(), DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), notes.Trim());
        RefreshAll();
        MessageBox.Show("تمت إضافة سجل الصيانة بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void GenerateReport()
    {
        var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        var summary = DatabaseService.GetDashboardSummary();
        var sb = new StringBuilder();
        sb.AppendLine("تقرير إدارة شبكات الصرف الصحي ومياه الأمطار");
        sb.AppendLine($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine("- - - - - - - - - - - - - - - - - - - - - - -");
        sb.AppendLine($"عدد المناطق: {summary.AreaCount}");
        sb.AppendLine($"عدد شبكات الصرف: {summary.SewageCount}");
        sb.AppendLine($"عدد شبكات الأمطار: {summary.RainwaterCount}");
        sb.AppendLine($"عدد الفحوصات: {summary.InspectionCount}");
        sb.AppendLine($"عدد الصيانة المعلقة: {summary.MaintenancePending}");

        File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
        MessageBox.Show($"تم إنشاء التقرير بنجاح:\n{savePath}", "تقرير", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static Button CreateButton(string text, Color color)
    {
        return new Button
        {
            Text = text,
            BackColor = color,
            ForeColor = Color.White,
            Font = new Font("Tahoma", 9F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            AutoSize = true,
            Margin = new Padding(4)
        };
    }

    private static Panel CreateSummaryCard(string title, Label valueLabel, Color accent)
    {
        var card = new Panel
        {
            Width = 200,
            Height = 120,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Tahoma", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            AutoSize = true,
            Location = new Point(10, 15)
        };

        valueLabel.Font = new Font("Tahoma", 20F, FontStyle.Bold);
        valueLabel.ForeColor = accent;
        valueLabel.AutoSize = true;
        valueLabel.Location = new Point(10, 55);

        var accentLine = new Panel
        {
            Size = new Size(160, 6),
            BackColor = accent,
            Location = new Point(10, 92)
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(accentLine);
        return card;
    }
}
