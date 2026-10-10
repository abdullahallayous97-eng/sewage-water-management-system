using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SewageStormwaterManagement;

public class Area
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class NetworkAsset
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class InspectionRecord
{
    public int Id { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Inspector { get; set; } = string.Empty;
    public string InspectionDate { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class MaintenanceRecord
{
    public int Id { get; set; }
    public string WorkType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ScheduledDate { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class DashboardSummary
{
    public int AreaCount { get; set; }
    public int SewageCount { get; set; }
    public int RainwaterCount { get; set; }
    public int InspectionCount { get; set; }
    public int MaintenancePending { get; set; }
}

public static class DatabaseService
{
    private const string DatabaseFileName = "sewage_management.db";

    public static string ConnectionString
    {
        get
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);
            return $"Data Source={filePath}";
        }
    }

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        ExecuteNonQuery(connection,
            @"CREATE TABLE IF NOT EXISTS Areas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL
            );");

        ExecuteNonQuery(connection,
            @"CREATE TABLE IF NOT EXISTS SewageNetwork (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Zone TEXT NOT NULL,
                Status TEXT NOT NULL,
                Description TEXT NOT NULL
            );");

        ExecuteNonQuery(connection,
            @"CREATE TABLE IF NOT EXISTS RainwaterNetwork (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Zone TEXT NOT NULL,
                Status TEXT NOT NULL,
                Description TEXT NOT NULL
            );");

        ExecuteNonQuery(connection,
            @"CREATE TABLE IF NOT EXISTS Inspections (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AssetType TEXT NOT NULL,
                AssetName TEXT NOT NULL,
                Inspector TEXT NOT NULL,
                InspectionDate TEXT NOT NULL,
                Condition TEXT NOT NULL,
                Notes TEXT NOT NULL
            );");

        ExecuteNonQuery(connection,
            @"CREATE TABLE IF NOT EXISTS MaintenanceRecords (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkType TEXT NOT NULL,
                Location TEXT NOT NULL,
                AssignedTo TEXT NOT NULL,
                Status TEXT NOT NULL,
                ScheduledDate TEXT NOT NULL,
                Notes TEXT NOT NULL
            );");

        if (GetCount(connection, "Areas") == 0)
        {
            InsertArea(connection, "المنطقة الشمالية", "منطقة الخدمة الشمالية");
            InsertArea(connection, "المنطقة الجنوبية", "منطقة الخدمة الجنوبية");
        }

        if (GetCount(connection, "SewageNetwork") == 0)
        {
            InsertAsset(connection, "SewageNetwork", "خط صرف رئيسي 01", "المنطقة الشمالية", "مستقر", "خط صرف رئيسي ضمن منطقة الخدمة");
            InsertAsset(connection, "SewageNetwork", "خط صرف رئيسي 02", "المنطقة الجنوبية", "قيد المراجعة", "خط صرف يحتاج إلى فحص فني");
        }

        if (GetCount(connection, "RainwaterNetwork") == 0)
        {
            InsertAsset(connection, "RainwaterNetwork", "مجمع مياه الأمطار 01", "المنطقة الشمالية", "مستقر", "سهل التشغيل");
            InsertAsset(connection, "RainwaterNetwork", "مجمع مياه الأمطار 02", "المنطقة الجنوبية", "قيد الصيانة", "بحاجة إلى تنظيف القناة");
        }

        if (GetCount(connection, "Inspections") == 0)
        {
            InsertInspection(connection, "الصرف الصحي", "خط صرف رئيسي 01", "أحمد علي", DateTime.Today.ToString("yyyy-MM-dd"), "جيد", "تمت مراجعة الخط ووجد أنه يعمل بشكل طبيعي");
            InsertInspection(connection, "مياه الأمطار", "مجمع مياه الأمطار 02", "سارة محمد", DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), "يحتاج إلى صيانة", "تمت ملاحظة انسداد جزئي");
        }

        if (GetCount(connection, "MaintenanceRecords") == 0)
        {
            InsertMaintenance(connection, "تنظيف", "خط صرف رئيسي 02", "فريق الصيانة", "قيد التنفيذ", DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), "تنظيف وفتح الفروع المعيقة");
            InsertMaintenance(connection, "إصلاح", "مجمع مياه الأمطار 02", "م. خالد", "مجدول", DateTime.Today.AddDays(3).ToString("yyyy-MM-dd"), "إصلاح الجدار الخارجي وممر التصريف");
        }
    }

    public static DashboardSummary GetDashboardSummary()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        return new DashboardSummary
        {
            AreaCount = GetCount(connection, "Areas"),
            SewageCount = GetCount(connection, "SewageNetwork"),
            RainwaterCount = GetCount(connection, "RainwaterNetwork"),
            InspectionCount = GetCount(connection, "Inspections"),
            MaintenancePending = GetPendingMaintenanceCount(connection)
        };
    }

    public static List<Area> GetAreas()
    {
        var result = new List<Area>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("SELECT Id, Name, Description FROM Areas ORDER BY Id", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new Area
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2)
            });
        }

        return result;
    }

    public static List<NetworkAsset> GetSewageNetwork()
    {
        return GetNetworkAssets("SewageNetwork");
    }

    public static List<NetworkAsset> GetRainwaterNetwork()
    {
        return GetNetworkAssets("RainwaterNetwork");
    }

    public static List<InspectionRecord> GetInspections()
    {
        var result = new List<InspectionRecord>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("SELECT Id, AssetType, AssetName, Inspector, InspectionDate, Condition, Notes FROM Inspections ORDER BY Id DESC", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new InspectionRecord
            {
                Id = reader.GetInt32(0),
                AssetType = reader.GetString(1),
                AssetName = reader.GetString(2),
                Inspector = reader.GetString(3),
                InspectionDate = reader.GetString(4),
                Condition = reader.GetString(5),
                Notes = reader.GetString(6)
            });
        }

        return result;
    }

    public static List<MaintenanceRecord> GetMaintenanceRecords()
    {
        var result = new List<MaintenanceRecord>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("SELECT Id, WorkType, Location, AssignedTo, Status, ScheduledDate, Notes FROM MaintenanceRecords ORDER BY Id DESC", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new MaintenanceRecord
            {
                Id = reader.GetInt32(0),
                WorkType = reader.GetString(1),
                Location = reader.GetString(2),
                AssignedTo = reader.GetString(3),
                Status = reader.GetString(4),
                ScheduledDate = reader.GetString(5),
                Notes = reader.GetString(6)
            });
        }

        return result;
    }

    public static void InsertArea(string name, string description)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        InsertArea(connection, name, description);
    }

    public static void InsertAsset(string tableName, string name, string zone, string status, string description)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        InsertAsset(connection, tableName, name, zone, status, description);
    }

    public static void InsertInspection(string assetType, string assetName, string inspector, string inspectionDate, string condition, string notes)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        InsertInspection(connection, assetType, assetName, inspector, inspectionDate, condition, notes);
    }

    public static void InsertMaintenance(string workType, string location, string assignedTo, string status, string scheduledDate, string notes)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        InsertMaintenance(connection, workType, location, assignedTo, status, scheduledDate, notes);
    }

    private static List<NetworkAsset> GetNetworkAssets(string tableName)
    {
        var result = new List<NetworkAsset>();
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand($"SELECT Id, Name, Zone, Status, Description FROM {tableName} ORDER BY Id", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new NetworkAsset
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Type = tableName == "SewageNetwork" ? "صرف صحي" : "مياه أمطار",
                Zone = reader.GetString(2),
                Status = reader.GetString(3),
                Description = reader.GetString(4)
            });
        }

        return result;
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private static int GetCount(SqliteConnection connection, string tableName)
    {
        using var command = new SqliteCommand($"SELECT COUNT(*) FROM {tableName}", connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static int GetPendingMaintenanceCount(SqliteConnection connection)
    {
        using var command = new SqliteCommand("SELECT COUNT(*) FROM MaintenanceRecords WHERE Status IN ('قيد التنفيذ', 'مجدول')", connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void InsertArea(SqliteConnection connection, string name, string description)
    {
        using var command = new SqliteCommand("INSERT INTO Areas (Name, Description) VALUES (@name, @description)", connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@description", description);
        command.ExecuteNonQuery();
    }

    private static void InsertAsset(SqliteConnection connection, string tableName, string name, string zone, string status, string description)
    {
        using var command = new SqliteCommand($"INSERT INTO {tableName} (Name, Zone, Status, Description) VALUES (@name, @zone, @status, @description)", connection);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@zone", zone);
        command.Parameters.AddWithValue("@status", status);
        command.Parameters.AddWithValue("@description", description);
        command.ExecuteNonQuery();
    }

    private static void InsertInspection(SqliteConnection connection, string assetType, string assetName, string inspector, string inspectionDate, string condition, string notes)
    {
        using var command = new SqliteCommand("INSERT INTO Inspections (AssetType, AssetName, Inspector, InspectionDate, Condition, Notes) VALUES (@assetType, @assetName, @inspector, @inspectionDate, @condition, @notes)", connection);
        command.Parameters.AddWithValue("@assetType", assetType);
        command.Parameters.AddWithValue("@assetName", assetName);
        command.Parameters.AddWithValue("@inspector", inspector);
        command.Parameters.AddWithValue("@inspectionDate", inspectionDate);
        command.Parameters.AddWithValue("@condition", condition);
        command.Parameters.AddWithValue("@notes", notes);
        command.ExecuteNonQuery();
    }

    private static void InsertMaintenance(SqliteConnection connection, string workType, string location, string assignedTo, string status, string scheduledDate, string notes)
    {
        using var command = new SqliteCommand("INSERT INTO MaintenanceRecords (WorkType, Location, AssignedTo, Status, ScheduledDate, Notes) VALUES (@workType, @location, @assignedTo, @status, @scheduledDate, @notes)", connection);
        command.Parameters.AddWithValue("@workType", workType);
        command.Parameters.AddWithValue("@location", location);
        command.Parameters.AddWithValue("@assignedTo", assignedTo);
        command.Parameters.AddWithValue("@status", status);
        command.Parameters.AddWithValue("@scheduledDate", scheduledDate);
        command.Parameters.AddWithValue("@notes", notes);
        command.ExecuteNonQuery();
    }
}
