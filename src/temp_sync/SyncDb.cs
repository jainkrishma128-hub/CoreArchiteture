using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;

var basePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "CommonArchitecture.API");
var configuration = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json")
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

Console.WriteLine($"Connecting to: {connectionString}");

using var connection = new SqlConnection(connectionString);
connection.Open();

// 1. Create __EFMigrationsHistory if it doesn't exist
var createHistoryTable = @"
if not exists (select * from sysobjects where name='__EFMigrationsHistory' and xtype='U')
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );";
using (var cmd = new SqlCommand(createHistoryTable, connection)) { cmd.ExecuteNonQuery(); }

// 2. Mark previous migrations as applied if their tables exist
string[] migrations = {
    "20251206042038_InitialCreate",
    "20251216000150_AddRoleTable",
    "20251216004623_ConfigureEntities",
    "20260110130602_RebuildLogsTable",
    "20260110131317_AddCoveringIndexes"
};

foreach (var m in migrations)
{
    var checkRecord = $"SELECT COUNT(*) FROM [__EFMigrationsHistory] WHERE [MigrationId] = '{m}'";
    using var cmdCheck = new SqlCommand(checkRecord, connection);
    if ((int)cmdCheck.ExecuteScalar() == 0)
    {
        Console.WriteLine($"Marking {m} as applied...");
        var insertRecord = $"INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('{m}', '9.0.0')";
        using var cmdInsert = new SqlCommand(insertRecord, connection);
        cmdInsert.ExecuteNonQuery();
    }
}

Console.WriteLine("Done sync.");
