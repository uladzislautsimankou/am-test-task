using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Enums;
using AM.TestTask.Business.Models;
using AM.TestTask.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AM.TestTask.Api.Controllers;

[ApiController, Route("[controller]")]
public class MeteoritesController(
    IMeteoriteReadService meteoriteReadService,
    IMeteoriteSyncService meteoriteSyncService
    ) : Controller
{
    [HttpGet("statistic")]
    public async Task<ActionResult<IEnumerable<MeteoriteStatisticDto>>> GetStatistic(
        [FromQuery] MeteoriteStatisticFilter filter,
        [FromQuery] StatisticSortBy sortBy = StatisticSortBy.Year,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        CancellationToken cancellationToken = default)
        => Ok(await meteoriteReadService.GetStatistic(filter, sortBy, sortDir, cancellationToken));

    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<RecClassDto>>> GetClasses(CancellationToken cancellationToken = default) 
        => Ok(await meteoriteReadService.GetClasses(cancellationToken));

    [HttpPost("sync")]
    public async Task<IActionResult> SyncMeteorites(CancellationToken cancellationToken = default)
    {
        await meteoriteSyncService.SynchronizeMeteoritesAsync(cancellationToken);
        return Ok();
    }
}