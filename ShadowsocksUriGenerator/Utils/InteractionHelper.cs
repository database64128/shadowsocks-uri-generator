using System;

namespace ShadowsocksUriGenerator;

public static class InteractionHelper
{

    /// <summary>
    /// Tries to parse a data limit string.
    /// </summary>
    /// <param name="dataLimit">The data limit string to parse.</param>
    /// <param name="dataLimitInBytes">The parsed data limit in bytes.</param>
    /// <returns>True on successful parsing. False on failure.</returns>
    public static bool TryParseDataLimitString(ReadOnlySpan<char> dataLimit, out ulong dataLimitInBytes)
    {
        dataLimitInBytes = 0UL;

        if (dataLimit.Length == 0)
            return false;

        var multiplier = dataLimit[^1] switch
        {
            'K' => 1024UL,
            'M' => 1024UL * 1024UL,
            'G' => 1024UL * 1024UL * 1024UL,
            'T' => 1024UL * 1024UL * 1024UL * 1024UL,
            'P' => 1024UL * 1024UL * 1024UL * 1024UL * 1024UL,
            'E' => 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL,
            _ => 1UL,
        };

        if (multiplier == 1UL)
        {
            return ulong.TryParse(dataLimit, out dataLimitInBytes);
        }
        else if (ulong.TryParse(dataLimit[0..^1], out var dataLimitBeforeMultiplication))
        {
            dataLimitInBytes = dataLimitBeforeMultiplication * multiplier;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a data representation in bytes
    /// to a human readable data string.
    /// </summary>
    /// <param name="dataInBytes">
    /// The amount of data in bytes.
    /// </param>
    /// <param name="middle_i">
    /// Whether to use 1024-based 'GiB', 'TiB' instead of 1000-based 'GB', 'TB'.
    /// Defaults to false, or 1000-based 'GB', 'TB'.
    /// </param>
    /// <param name="trailingB">
    /// Whether the returned string has a trailing 'B' representing bytes.
    /// Defaults to true, or 'GB', 'TB'.
    /// Set to false for 'G', 'T'.
    /// </param>
    /// <returns>
    /// A human readable string representation of the amount of data.
    /// </returns>
    public static string HumanReadableDataString(ulong dataInBytes, bool middle_i = false, bool trailingB = true)
    {
        if (middle_i)
            return HumanReadableDataString1024(dataInBytes, middle_i, trailingB);
        else
            return HumanReadableDataString1000(dataInBytes, trailingB);
    }

    /// <summary>
    /// Converts a data representation in bytes
    /// to a human readable data string
    /// using 1000 as the conversion rate.
    /// </summary>
    /// <param name="dataInBytes">
    /// The amount of data in bytes.
    /// </param>
    /// <param name="trailingB">
    /// Whether the returned string has a trailing 'B' representing bytes.
    /// Defaults to true, or 'GB', 'TB'.
    /// Set to false for 'G', 'T'.
    /// </param>
    /// <returns>
    /// A human readable string representation of the amount of data.
    /// </returns>
    public static string HumanReadableDataString1000(ulong dataInBytes, bool trailingB = true)
    {
        var stringTail = trailingB ? "B" : "";

        return dataInBytes switch
        {
            < 1000UL => $"{dataInBytes}{(trailingB ? " B" : "")}",
            < 1000UL * 1000UL => $"{dataInBytes / 1000.0:G4} K{stringTail}",
            < 1000UL * 1000UL * 1000UL => $"{dataInBytes / 1000.0 / 1000.0:G4} M{stringTail}",
            < 1000UL * 1000UL * 1000UL * 1000UL => $"{dataInBytes / 1000.0 / 1000.0 / 1000.0:G4} G{stringTail}",
            < 1000UL * 1000UL * 1000UL * 1000UL * 1000UL => $"{dataInBytes / 1000.0 / 1000.0 / 1000.0 / 1000.0:G4} T{stringTail}",
            < 1000UL * 1000UL * 1000UL * 1000UL * 1000UL * 1000UL => $"{dataInBytes / 1000.0 / 1000.0 / 1000.0 / 1000.0 / 1000.0:G4} P{stringTail}",
            _ => $"{dataInBytes / 1000.0 / 1000.0 / 1000.0 / 1000.0 / 1000.0 / 1000.0:G4} E{stringTail}",
        };
    }

    /// <summary>
    /// Converts a data representation in bytes
    /// to a human readable data string
    /// using 1024 as the conversion rate.
    /// </summary>
    /// <param name="dataInBytes">
    /// The amount of data in bytes.
    /// </param>
    /// <param name="middle_i">
    /// Whether to return 'GiB', 'TiB' instead of 'GB', 'TB'.
    /// Defaults to true, or 'GiB', 'TiB'.
    /// This doesn't affect the conversion rate.
    /// </param>
    /// <param name="trailingB">
    /// Whether the returned string has a trailing 'B' representing bytes.
    /// Defaults to true, or 'GiB', 'TiB'.
    /// Set to false for 'Gi', 'Ti'.
    /// </param>
    /// <returns>
    /// A human readable string representation of the amount of data.
    /// </returns>
    public static string HumanReadableDataString1024(ulong dataInBytes, bool middle_i = true, bool trailingB = true)
    {
        var stringTail = $"{(middle_i ? "i" : "")}{(trailingB ? "B" : "")}";

        return dataInBytes switch
        {
            < 1024UL => $"{dataInBytes}{(trailingB ? " B" : "")}",
            < 1024UL * 1024UL => $"{dataInBytes / 1024.0:G4} K{stringTail}",
            < 1024UL * 1024UL * 1024UL => $"{dataInBytes / 1024.0 / 1024.0:G4} M{stringTail}",
            < 1024UL * 1024UL * 1024UL * 1024UL => $"{dataInBytes / 1024.0 / 1024.0 / 1024.0:G4} G{stringTail}",
            < 1024UL * 1024UL * 1024UL * 1024UL * 1024UL => $"{dataInBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0:G4} T{stringTail}",
            < 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL => $"{dataInBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0 / 1024.0:G4} P{stringTail}",
            _ => $"{dataInBytes / 1024.0 / 1024.0 / 1024.0 / 1024.0 / 1024.0 / 1024.0:G4} E{stringTail}",
        };
    }
}
