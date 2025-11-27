using AM.TestTask.Data.Relational.Sync.Abstractions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AM.TestTask.Data.Relational.Sync.PostgreSql;

public class PostgreSqlTableSynchronizer<TSource, TEntity>(
    DbContext dbContext,
    ILogger<PostgreSqlTableSynchronizer<TSource, TEntity>> logger
    ) : ITableSynchronizer<TSource, TEntity>
    where TEntity : class
    where TSource : class
{
    private readonly string _tableName = dbContext.Model
        .FindEntityType(typeof(TEntity))?
        .GetSchemaQualifiedTableName()
        ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} not found in EF model.");

    public async Task SynchronizeAsync(IAsyncEnumerable<TSource> sourceStream, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting synchronization for {EntityName}...", typeof(TEntity).Name);

        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // без кавычек, что б BulkExtensions не ругался
            var stagingTableName = $"Staging_{Guid.NewGuid():N}";

            logger.LogDebug("Creating staging table {StagingTable}...", stagingTableName);

            await CreateStagingTableAsync(stagingTableName, cancellationToken);

            var sw = Stopwatch.StartNew();

            await FillStagingTableAsync(sourceStream, stagingTableName, cancellationToken);

            sw.Stop();

            logger.LogInformation("Data loaded into staging in {ElapsedMilliseconds}ms.", sw.ElapsedMilliseconds);


            logger.LogDebug("Executing MERGE operation...");

            sw.Restart();

            await PerformMergeAsync(stagingTableName, cancellationToken);

            sw.Stop();
            logger.LogInformation("MERGE operation completed in {ElapsedMilliseconds}ms.", sw.ElapsedMilliseconds);

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Synchronization failed. Rolling back transaction.");
            throw;
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }

    protected virtual async Task CreateStagingTableAsync(string stagingTable, CancellationToken cancellationToken = default)
    {
        var sql = $"CREATE TEMP TABLE \"{stagingTable}\" AS SELECT * FROM \"{_tableName}\" LIMIT 0";

        await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    protected virtual async Task PerformMergeAsync(string stagingTable, CancellationToken cancellationToken = default)
    {
        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        var keyName = entityType!.FindPrimaryKey()!.Properties.First().Name;
        var properties = entityType.GetProperties().Select(p => p.Name).ToList();
        var nonKeyProps = properties.Where(p => p != keyName).ToList();

        var updateSet = string.Join(", ", nonKeyProps.Select(p => $"\"{p}\" = EXCLUDED.\"{p}\""));
        var insertCols = string.Join(", ", properties.Select(p => $"\"{p}\""));
        var insertVals = string.Join(", ", properties.Select(p => $"Source.\"{p}\""));

        var sql = $@"
            MERGE INTO ""{_tableName}"" AS Target
            USING ""{stagingTable}"" AS Source
                ON Target.""{keyName}"" = Source.""{keyName}""
                WHEN MATCHED THEN
                UPDATE SET {string.Join(", ", nonKeyProps.Select(p => $"\"{p}\" = Source.\"{p}\""))}
            WHEN NOT MATCHED THEN
                    INSERT({insertCols}) VALUES({ insertVals}); ";
    
    await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    protected virtual async Task FillStagingTableAsync(
        IAsyncEnumerable<TSource> sourceStream,
        string stagingTable,
        CancellationToken cancellationToken = default)
    {
        await BatchInsertAsync(
            sourceStream,
            stagingTable,
            item => item,
            cancellationToken: cancellationToken);
    }

    protected async Task BatchInsertAsync<TTarget>(
        IAsyncEnumerable<TSource> sourceStream,
        string stagingTable,
        Func<TSource, TTarget?> mapFunc,
        int batchSize = 1000,
        CancellationToken cancellationToken = default)
        where TTarget : class
    {
        var bulkConfig = new BulkConfig
        {
            CustomDestinationTableName = stagingTable,
            BatchSize = batchSize,
            PreserveInsertOrder = false,
            UseTempDB = false
        };

        logger.LogInformation("Starting bulk insert into staging table '{StagingTable}'...", stagingTable);

        var totalCount = 0;
        var batch = new List<TTarget>(batchSize);

        await foreach (var item in sourceStream.WithCancellation(cancellationToken))
        {
            var mappedItem = mapFunc(item);
            if (mappedItem is null) continue;

            batch.Add(mappedItem);

            if (batch.Count >= batchSize)
            {
                logger.LogDebug("Writing batch of {BatchSize} records...", batch.Count);

                await dbContext.BulkInsertAsync(batch, bulkConfig, cancellationToken: cancellationToken);

                totalCount += batch.Count;
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            logger.LogDebug("Writing final batch of {BatchSize} records...", batch.Count);

            await dbContext.BulkInsertAsync(batch, bulkConfig, cancellationToken: cancellationToken);

            totalCount += batch.Count;
        }

        logger.LogInformation("Bulk insert completed. Total records processed: {TotalCount}", totalCount);
    }
}
