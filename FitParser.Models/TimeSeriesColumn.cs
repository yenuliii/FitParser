/*
*/

namespace FitParser.Models;

public sealed record TimeSeriesColumn(
    string ColumnName,
    string MessageName,
    int MessageNumber,
    string FieldName,
    int FieldNumber,
    string? FitDataType,
    string? Units);