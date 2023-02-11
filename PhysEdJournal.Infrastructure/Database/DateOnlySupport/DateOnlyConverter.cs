using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PhysEdJournal.Infrastructure.DateOnlySupport;

public sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter() : base(
        dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
        dateTime => DateOnly.FromDateTime(dateTime))
    {
    }
}