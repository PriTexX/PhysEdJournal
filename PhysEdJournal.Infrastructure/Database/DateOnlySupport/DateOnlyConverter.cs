using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PhysEdJournal.Infrastructure.Database.DateOnlySupport;

public sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(dateTime)
        ) { }
}
