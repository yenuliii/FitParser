/*
*/

namespace FitParser.Models;

public sealed class TimeSeriesTable
{
    public IReadOnlyList<TimeSeriesColumn> Columns { get; }

    public IReadOnlyList<TimeSeriesRow> Rows { get; }

    public TimeSeriesTable(
        IReadOnlyList<TimeSeriesColumn> columns,
        IReadOnlyList<TimeSeriesRow> rows)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);

        Columns = columns;
        Rows = rows;
    }
}