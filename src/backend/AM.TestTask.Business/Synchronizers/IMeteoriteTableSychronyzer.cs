using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Entities;
using AM.TestTask.Data.Relational.Sync.Abstractions;

namespace AM.TestTask.Business.Synchronizers;

public interface IMeteoriteTableSychronyzer : ITableSynchronizer<MeteoriteDto, Meteorite>
{
}
