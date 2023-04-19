using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.SSMv1;

/// <summary>
/// API client of Shadowsocks Server Management API v1 (SSMv1).
/// </summary>
public class SSMv1ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Uri _baseUri;
    private readonly Uri _usersUri;
    private readonly Uri _statsUri;
    private readonly Uri _statsClearUri;

    /// <summary>
    /// Constructs an API client.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance.</param>
    /// <param name="baseUri">The API endpoint.</param>
    public SSMv1ApiClient(HttpClient httpClient, Uri baseUri)
    {
        _httpClient = httpClient;

        if (!baseUri.AbsolutePath.EndsWith('/'))
            baseUri = new(baseUri.AbsoluteUri + "/");

        _baseUri = baseUri;
        _usersUri = new(baseUri, "users");
        _statsUri = new(baseUri, "stats");
        _statsClearUri = new(baseUri, "stats?clear=true");
    }

    /// <summary>
    /// Gets information about the server.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The server information.</returns>
    public Task<SSMv1ServerInfo?> GetServerInfoAsync(CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync(_baseUri, SSMv1JsonSerializerContext.Default.SSMv1ServerInfo, cancellationToken);

    /// <summary>
    /// Gets the user list.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The user list.</returns>
    public Task<SSMv1UserInfoList?> ListUsersAsync(CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync(_usersUri, SSMv1JsonSerializerContext.Default.SSMv1UserInfoList, cancellationToken);

    /// <summary>
    /// Adds the given user.
    /// </summary>
    /// <param name="user">The user to add.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The response message.</returns>
    public Task<HttpResponseMessage> AddUserAsync(SSMv1UserInfo user, CancellationToken cancellationToken = default) =>
        _httpClient.PostAsJsonAsync(_usersUri, user, SSMv1JsonSerializerContext.Default.SSMv1UserInfo, cancellationToken);

    /// <summary>
    /// Gets detailed information about the user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>Detailed user information.</returns>
    public Task<SSMv1UserDetails?> GetUserDetailsAsync(string username, CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync(new Uri(_usersUri, username), SSMv1JsonSerializerContext.Default.SSMv1UserDetails, cancellationToken);

    /// <summary>
    /// Updates the user's PSK.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="uPSK">The new uPSK.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The response message.</returns>
    public Task<HttpResponseMessage> UpdateUserAsync(string username, string uPSK, CancellationToken cancellationToken = default) =>
        _httpClient.PatchAsJsonAsync(new Uri(_usersUri, username), new SSMv1UserCred { UserPSK = uPSK, }, SSMv1JsonSerializerContext.Default.SSMv1UserCred, cancellationToken);

    /// <summary>
    /// Deletes the specified user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The response message.</returns>
    public Task<HttpResponseMessage> DeleteUserAsync(string username, CancellationToken cancellationToken = default) =>
        _httpClient.DeleteAsync(new Uri(_usersUri, username), cancellationToken);

    /// <summary>
    /// Gets traffic stats.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>Server's traffic stats.</returns>
    public Task<SSMv1Stats?> GetStatsAsync(CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync(_statsUri, SSMv1JsonSerializerContext.Default.SSMv1Stats, cancellationToken);

    /// <summary>
    /// Gets and clears traffic stats.
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>Server's traffic stats.</returns>
    public Task<SSMv1Stats?> GetAndClearStatsAsync(CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync(_statsClearUri, SSMv1JsonSerializerContext.Default.SSMv1Stats, cancellationToken);
}
