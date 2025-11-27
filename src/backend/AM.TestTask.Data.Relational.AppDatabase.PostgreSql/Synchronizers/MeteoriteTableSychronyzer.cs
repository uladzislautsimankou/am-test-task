using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Entities;
using AM.TestTask.Business.Synchronizers;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Context;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Extensions;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Models;
using AM.TestTask.Data.Relational.Sync.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Synchronizers;

internal sealed class MeteoriteTableSychronyzer(
    ApplicationDatabaseContext context,
    ILogger<MeteoriteTableSychronyzer> logger
    ) : PostgreSqlTableSynchronizer<MeteoriteDto, Meteorite>(context, logger), IMeteoriteTableSychronyzer
{
    protected override async Task CreateStagingTableAsync(string stagingTable, CancellationToken cancellationToken = default)
    {
        var dtoType = context.Model.FindEntityType(typeof(MeteoriteStaging))
            ?? throw new InvalidOperationException("DTO not found in model");

        var columnsSql = dtoType.GetProperties().Select(p =>
        {
            var colName = p.GetColumnName();
            var colType = p.GetColumnType();
            var nullability = p.IsNullable ? "" : "NOT NULL";

            if (p.Name == nameof(MeteoriteStaging.Id))
                return $"\"{colName}\" {colType} NOT NULL PRIMARY KEY";

            return $"\"{colName}\" {colType} {nullability}";
        });

        var sql = $"CREATE TEMP TABLE \"{stagingTable}\" ({string.Join(", ", columnsSql)});";

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    protected override async Task PerformMergeAsync(string stagingTable, CancellationToken cancellationToken = default)
    {
        // таблицы
        var meteoriteType = context.Model.FindEntityType(typeof(Meteorite))!;
        var recClassType = context.Model.FindEntityType(typeof(RecClass))!;
        var stagingType = context.Model.FindEntityType(typeof(MeteoriteStaging))!;

        // имена таблиц
        var meteoriteTable = $"\"{meteoriteType.GetSchemaQualifiedTableName()}\"";
        var recClassTable = $"\"{recClassType.GetSchemaQualifiedTableName()}\"";

        // RecClass
        var rcIdCol = recClassType.FindPrimaryKey()!.Properties.First().GetColumnName();
        var rcNameCol = recClassType.FindProperty(nameof(RecClass.Name))!.GetColumnName();

        // Staging
        var stRawCol = stagingType.FindProperty(nameof(MeteoriteStaging.RecClassRaw))!.GetColumnName();

        // Meteorite
        var mRecClassIdCol = meteoriteType.FindProperty(nameof(Meteorite.RecClassId))!.GetColumnName();
        var mIdCol = meteoriteType.FindPrimaryKey()!.Properties.First().GetColumnName();

        // заполняем RecClass
        var syncDictSql = $@"
            INSERT INTO {recClassTable} (""{rcNameCol}"")
            SELECT DISTINCT s.""{stRawCol}""
            FROM ""{stagingTable}"" s
            WHERE s.""{stRawCol}"" IS NOT NULL 
              AND s.""{stRawCol}"" <> ''
              AND NOT EXISTS (
                  SELECT 1 FROM {recClassTable} c WHERE c.""{rcNameCol}"" = s.""{stRawCol}""
              );";

        await context.Database.ExecuteSqlRawAsync(syncDictSql, cancellationToken);

        // все, что можно просто 1 в 1 скопировать
        var commonProperties = meteoriteType.GetProperties()
            .Where(p => !p.IsPrimaryKey() && p.GetColumnName() != mRecClassIdCol)
            .Select(p => p.GetColumnName())
            .ToList();

        // для SELECT s."Name", s."Mass" и т.д.
        var sourceColumns = string.Join(", ", commonProperties.Select(c => $"s.\"{c}\""));

        // для UPDATE SET Target."Name" = Source."Name" и т.д.
        var updateParts = commonProperties.Select(c => $"\"{c}\" = Source.\"{c}\"").ToList();
        updateParts.Add($"\"{mRecClassIdCol}\" = Source.\"NewRecClassId\"");
        var updateSetSql = string.Join(",\n ", updateParts);

        // для INSERT COLUMNS "Name", "Mass" и т.д.
        var insertColsList = new List<string>(commonProperties) { mIdCol, mRecClassIdCol };
        var insertColsSql = string.Join(", ", insertColsList.Select(c => $"\"{c}\""));

        // для INSERT VALUES Source."Name", Source."Mass" и т.д.
        var insertValsList = commonProperties.Select(c => $"Source.\"{c}\"").ToList();
        insertValsList.Add($"Source.\"{mIdCol}\"");
        insertValsList.Add($"Source.\"NewRecClassId\"");
        var insertValsSql = string.Join(", ", insertValsList);

        // мержим
        var mergeSql = $@"
            MERGE INTO {meteoriteTable} AS Target
            USING (
                SELECT 
                    s.""{mIdCol}"",
                    {sourceColumns},
                    c.""{rcIdCol}"" AS ""NewRecClassId""
                FROM ""{stagingTable}"" s
                LEFT JOIN {recClassTable} c
                    ON s.""{stRawCol}"" = c.""{rcNameCol}""
            ) AS Source
            ON Target.""{mIdCol}"" = Source.""{mIdCol}""

            WHEN MATCHED THEN
                UPDATE SET
                    {updateSetSql}

            WHEN NOT MATCHED THEN
                INSERT ({insertColsSql})
                VALUES ({insertValsSql})

            WHEN NOT MATCHED BY SOURCE THEN
                DELETE;";

        await context.Database.ExecuteSqlRawAsync(mergeSql, cancellationToken);
    }


    protected override async Task FillStagingTableAsync(
        IAsyncEnumerable<MeteoriteDto> sourceStream,
        string stagingTable,
        CancellationToken cancellationToken = default)
    {
        await BatchInsertAsync(
            sourceStream,
            stagingTable,
            item => item.ToStagingModelOrNull(),
            cancellationToken: cancellationToken);
    }
}
