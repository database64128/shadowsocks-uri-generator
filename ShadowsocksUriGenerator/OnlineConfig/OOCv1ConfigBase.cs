using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.OnlineConfig;

/// <summary>
/// OOCv1 config base.
/// Inherit from this class and add protocol-specific properties.
/// Serialize and deserialize in camelCase.
/// </summary>
public class OOCv1ConfigBase
{
    /// <summary>
    /// Gets or sets the username.
    /// Optional.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the amount of data used in bytes.
    /// Optional.
    /// </summary>
    public ulong? BytesUsed { get; set; }

    /// <summary>
    /// Gets or sets the amount of data remaining in bytes.
    /// Optional.
    /// </summary>
    public ulong? BytesRemaining { get; set; }

    /// <summary>
    /// Gets or sets the expiry date of the configuration.
    /// Optional.
    /// </summary>
    [JsonConverter(typeof(DateTimeOffsetUnixTimeSecondsConverter))]
    public DateTimeOffset? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the protocols used in the configuration.
    /// </summary>
    public List<string> Protocols { get; set; } = [];
}
