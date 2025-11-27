using AM.TestTask.Data.Relational.Abstractions;

namespace AM.TestTask.Business.Entities;

public sealed class RecClass : IIdentifiablyEntity<int>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
