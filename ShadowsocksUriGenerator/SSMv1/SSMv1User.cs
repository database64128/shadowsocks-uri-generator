using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.SSMv1;

/// <summary>
/// Contains the uPSK only.
/// </summary>
public class SSMv1UserCred
{
    /// <summary>
    /// Gets or sets the user's PSK.
    /// </summary>
    [JsonPropertyName("uPSK")]
    public string UserPSK { get; set; } = "";
}

/// <summary>
/// Contains a user's username and uPSK.
/// </summary>
public class SSMv1UserInfo
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Gets or sets the user's PSK.
    /// </summary>
    [JsonPropertyName("uPSK")]
    public string UserPSK { get; set; } = "";
}

/// <summary>
/// Contains a list of <see cref="SSMv1UserInfo"/>.
/// </summary>
public class SSMv1UserInfoList
{
    /// <summary>
    /// Gets or sets the list of users.
    /// </summary>
    public SSMv1UserInfo[] Users { get; set; } = [];
}

/// <summary>
/// Contains the username and the user's traffic stats.
/// </summary>
public class SSMv1UserStats : SSMv1StatsBase
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = "";
}

/// <summary>
/// Contains the user's username, uPSK, and traffic stats.
/// </summary>
public class SSMv1UserDetails : SSMv1StatsBase
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    /// Gets or sets the user's PSK.
    /// </summary>
    [JsonPropertyName("uPSK")]
    public string UserPSK { get; set; } = "";
}
