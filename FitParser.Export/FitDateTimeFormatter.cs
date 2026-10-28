/* 
This file is responsible for converting Fit Epoch timestamps which is recorded as the number 
of seconds since december 31th, 1989 to Unix Epoch which is recorded as the number of seconds
since January 1st, 1970. 
Then we will use a normal date library to convert the value into a calendar date. 

TODO: Implement a functino for the formater to call when it runs into a date_time value 
takes in the value as a FIT Epoch and returns a calendar date

*/

using System.Globalization;

namespace FitParser.Export;

public static class FitDateTimeFormatter
{
    private const uint MinimumAbsoluteDateTime = 0x10000000;
    private const uint InvalidDateTime = uint.MaxValue;

    private static readonly DateTimeOffset FitEpochUtc =
        new(
            1989,
            12,
            31,
            0,
            0,
            0,
            TimeSpan.Zero);

    private static readonly DateTime FitLocalEpoch =
        new(
            1989,
            12,
            31,
            0,
            0,
            0,
            DateTimeKind.Unspecified);

    public static string Format(
        object? value,
        string? fitDataType)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (string.Equals(
                fitDataType,
                "DateTime",
                StringComparison.OrdinalIgnoreCase))
        {
            return FormatDateTime(value);
        }

        if (string.Equals(
                fitDataType,
                "LocalDateTime",
                StringComparison.OrdinalIgnoreCase))
        {
            return FormatLocalDateTime(value);
        }

        return Convert.ToString(
            value,
            CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatDateTime(object value)
    {
        uint seconds =
            Convert.ToUInt32(
                value,
                CultureInfo.InvariantCulture);

        if (seconds < MinimumAbsoluteDateTime ||
            seconds == InvalidDateTime)
        {
            return seconds.ToString(
                CultureInfo.InvariantCulture);
        }

        DateTimeOffset utcTime =
            FitEpochUtc.AddSeconds(seconds);

        return utcTime.ToString(
            "yyyy-MM-dd'T'HH:mm:ss'Z'",
            CultureInfo.InvariantCulture);
    }

    private static string FormatLocalDateTime(object value)
    {
        uint seconds =
            Convert.ToUInt32(
                value,
                CultureInfo.InvariantCulture);

        if (seconds < MinimumAbsoluteDateTime ||
            seconds == InvalidDateTime)
        {
            return seconds.ToString(
                CultureInfo.InvariantCulture);
        }

        DateTime localTime =
            FitLocalEpoch.AddSeconds(seconds);

        return localTime.ToString(
            "yyyy-MM-dd'T'HH:mm:ss",
            CultureInfo.InvariantCulture);
    }
}