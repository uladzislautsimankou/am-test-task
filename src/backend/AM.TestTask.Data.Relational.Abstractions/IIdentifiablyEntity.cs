namespace AM.TestTask.Data.Relational.Abstractions;

public interface IIdentifiablyEntity<TId>
{
    TId Id { get; set; }
}
