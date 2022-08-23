using System;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator;

/// <summary>
/// Stores a user's credential and data limit in a group.
/// </summary>
public class MemberInfo : IEquatable<MemberInfo>
{
    public string Method { get; set; }

    public string Password { get; set; }

    /// <summary>
    /// Gets whether the member info contains credential.
    /// </summary>
    [JsonIgnore]
    public bool HasCredential => !string.IsNullOrEmpty(Method) && !string.IsNullOrEmpty(Password);

    /// <summary>
    /// Gets or sets the data limit in bytes
    /// enforced on the user in the group.
    /// Do not set this if it's a global or general per-key limit.
    /// Set this if the limit is specifically targeting this user in the group.
    /// </summary>
    public ulong DataLimitInBytes { get; set; }

    /// <summary>
    /// Parameterless constructor for System.Text.Json
    /// </summary>
    public MemberInfo()
    {
        Method = "";
        Password = "";
    }

    public MemberInfo(string method, string password, ulong dataLimitInBytes = 0UL)
    {
        Method = method;
        Password = password;
        DataLimitInBytes = dataLimitInBytes;
    }

    public bool Equals(MemberInfo? other) => Method == other?.Method && Password == other?.Password;

    public override bool Equals(object? obj) => Equals(obj as MemberInfo);

    public override int GetHashCode() => Method.GetHashCode() ^ Password.GetHashCode();

    /// <summary>
    /// Clears the credential information.
    /// </summary>
    public void ClearCredential()
    {
        Method = "";
        Password = "";
    }
}
