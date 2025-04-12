using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Data;

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
    /// Gets or sets the data usage in bytes.
    /// </summary>
    public ulong BytesUsed { get; set; }

    /// <summary>
    /// Gets or sets the data remaining to be used in bytes.
    /// </summary>
    public ulong BytesRemaining { get; set; }

    /// <summary>
    /// Parameterless constructor for System.Text.Json
    /// </summary>
    public MemberInfo()
    {
        Method = "";
        Password = "";
    }

    /// <summary>
    /// Constructs a member info object with the given method and a generated password.
    /// </summary>
    /// <param name="method">Method name.</param>
    public MemberInfo(string method)
    {
        Method = method;
        GeneratePassword();
    }

    public MemberInfo(string method, string password)
    {
        Method = method;
        Password = password;
    }

    public bool Equals(MemberInfo? other) => Method == other?.Method && Password == other?.Password;

    public override bool Equals(object? obj) => Equals(obj as MemberInfo);

    public override int GetHashCode() => Method.GetHashCode() ^ Password.GetHashCode();

    [MemberNotNull(nameof(Password))]
    public void GeneratePassword()
    {
        int keySize = Method switch
        {
            "2022-blake3-aes-128-gcm" => 16,
            _ => 32,
        };
        Span<byte> key = stackalloc byte[keySize];
        RandomNumberGenerator.Fill(key);
        Password = Convert.ToBase64String(key);
    }

    /// <summary>
    /// Clears the credential information.
    /// </summary>
    public void ClearCredential()
    {
        Method = "";
        Password = "";
    }

    public string PasswordForNode(List<string> iPSKs)
    {
        if (iPSKs.Count == 0)
            return Password;

        var length = iPSKs.Count + iPSKs.Sum(x => x.Length) + Password.Length;
        return string.Create(length, iPSKs, (chars, iPSKs) =>
        {
            foreach (var iPSK in iPSKs)
            {
                iPSK.CopyTo(chars);
                chars[iPSK.Length] = ':';
                chars = chars[(iPSK.Length + 1)..];
            }

            Password.CopyTo(chars);
        });
    }

    public void AddBytesUsed(ulong bytesUsed, ulong perUserDataLimitInBytes)
    {
        BytesUsed += bytesUsed;
        UpdateBytesRemaining(perUserDataLimitInBytes);
    }

    public void SubBytesUsed(ulong bytesUsed, ulong perUserDataLimitInBytes)
    {
        BytesUsed = BytesUsed >= bytesUsed ? BytesUsed - bytesUsed : 0UL;
        UpdateBytesRemaining(perUserDataLimitInBytes);
    }

    public void ClearBytesUsed(ulong perUserDataLimitInBytes)
    {
        BytesUsed = 0UL;
        UpdateBytesRemaining(perUserDataLimitInBytes);
    }

    private void UpdateBytesRemaining(ulong perUserDataLimitInBytes)
    {
        ulong dataLimitInBytes = DataLimitInBytes > 0UL ? DataLimitInBytes : perUserDataLimitInBytes;
        BytesRemaining = dataLimitInBytes > BytesUsed ? dataLimitInBytes - BytesUsed : 0UL;
    }
}
