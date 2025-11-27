using AM.TestTask.Business.DTOs;
using AM.TestTask.Business.Enums;
using AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Models;
using System.Globalization;

namespace AM.TestTask.Data.Relational.AppDatabase.PostgreSql.Extensions;

internal static class MeteoriteMappingExtensions
{
    public static MeteoriteStaging? ToStagingModelOrNull(this MeteoriteDto dto)
    {
        // мы уже проверяли это, и такое не должно прилетать, но что бы самодостаточной было
        // вдруг потом кто-то где-то не проверит этот момент
        if (!long.TryParse(dto.Id, out var id)) return null;

        return new MeteoriteStaging
        {
            Id = id,
            Name = string.IsNullOrEmpty(dto.Name) ? "Unknown" : dto.Name,
            RecClassRaw = string.IsNullOrEmpty(dto.Recclass) ? "Unknown" : dto.Recclass,

            NameType = Enum.TryParse<NameType>(dto.Nametype, true, out var nt) ? nt : NameType.Unknown,
            FallType = Enum.TryParse<FallType>(dto.Fall, true, out var ft) ? ft : FallType.Unknown,

            Mass = decimal.TryParse(dto.Mass, NumberStyles.Any, CultureInfo.InvariantCulture, out var mass) ? mass : null,

            Year = ParseYear(dto.Year),

            Latitude = double.TryParse(dto.Reclat, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) ? lat : null,
            Longitude = double.TryParse(dto.Reclong, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon) ? lon : null
        };
    }

    // вдруг год прилетит как просто год (1985), а не целиком дата
    private static short? ParseYear(string yearString)
    {
        if (string.IsNullOrWhiteSpace(yearString)) return null;

        if (short.TryParse(yearString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var simpleYear))
        {
            return simpleYear;
        }

        if (DateTime.TryParse(yearString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return (short)date.Year;
        }

        return null;
    }
}
