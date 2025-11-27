using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Services.Interfaces;
using AM.TestTask.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AM.TestTask.Infrastructure.Services;

internal sealed class MeteoriteHttpDataProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<MeteoriteHttpDataProvider> logger,
    IOptionsSnapshot<MeteoriteDataProviderOptions> options
    ) : IDataProvider<MeteoriteDto>
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async IAsyncEnumerable<MeteoriteDto> GetDataStreamAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("MeteoriteClient");
        var url = options.Value.Url;

        logger.LogInformation("Sending GET request to url: {Url}", url);

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to fetch data. Status: {StatusCode}", response.StatusCode);
            throw;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<MeteoriteDto>(stream, _jsonOptions, cancellationToken))
        {
            if (item is null) continue;

            if (!long.TryParse(item.Id, out var id))
            {
                logger.LogWarning("Skipping item with invalid Id: '{ItemId}'. Name: '{ItemName}'", item.Id, item.Name);
                continue;
            }

            yield return item;
        }
    }
}
