using ShadowsocksUriGenerator.SSMv1;
using System.Runtime.CompilerServices;

namespace ShadowsocksUriGenerator.Data;

/// <summary>
/// Represents a server managed via the Shadowsocks Server Management API v1 (SSMv1).
/// </summary>
public sealed class SSMv1Server
{
    private const int StatsRetentionDays = 30; // 30 days

    private SSMv1ApiClient? _apiClient;

    /// <summary>
    /// Gets or sets the group's Shadowsocks Server Management API v1 (SSMv1) base URI.
    /// </summary>
    public required Uri BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the encryption method in use when the server is managed via SSMv1.
    /// </summary>
    public string ServerMethod { get; set; } = "2022-blake3-aes-128-gcm";

    /// <summary>
    /// Gets or sets the Shadowsocks Server Management API v1 (SSMv1) server information object.
    /// </summary>
    public SSMv1ServerInfo? ServerInfo { get; set; }

    /// <summary>
    /// Gets or sets historical SSMv1 traffic statistics objects.
    /// </summary>
    public Queue<SSMv1StatsWithDate> HistoricalStats { get; set; } = new(StatsRetentionDays);

    /// <summary>
    /// Gets or sets the current day's SSMv1 traffic statistics object.
    /// </summary>
    public SSMv1StatsWithDate CurrentStats { get; set; } = new();

    /// <summary>
    /// Represents SSMv1 traffic statistics for a specific date.
    /// </summary>
    public class SSMv1StatsWithDate()
    {
        /// <summary>
        /// Gets or sets the date of the statistics.
        /// </summary>
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        /// <summary>
        /// Gets or sets the group traffic statistics object.
        /// </summary>
        public SSMv1StatsBase GroupStats { get; set; } = new();

        /// <summary>
        /// Gets or sets the user traffic statistics dictionary.
        /// </summary>
        public Dictionary<string, SSMv1StatsBase> UserStats { get; set; } = [];

        /// <summary>
        /// Resets the statistics to zero.
        /// </summary>
        public void Clear()
        {
            GroupStats.Clear();
            foreach (KeyValuePair<string, SSMv1StatsBase> userStatsEntry in UserStats)
            {
                userStatsEntry.Value.Clear();
            }
        }
    }

    /// <summary>
    /// Pulls server information, user credentials, and statistics from the SSMv1 API.
    /// </summary>
    /// <param name="httpClient">An instance of <see cref="HttpClient"/>.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="group">The <see cref="Group"/> object.</param>
    /// <param name="users">The <see cref="Users"/> object.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> for iterating through completed tasks.</returns>
    public IAsyncEnumerable<Task> PullAsync(HttpClient httpClient, string groupName, Group group, Users users, CancellationToken cancellationToken = default)
    {
        _apiClient ??= new(httpClient, BaseUri);
        return Task.WhenEach(
            PullServerInfoAsync(_apiClient, groupName, cancellationToken),
            PullServerUserCredentialsAsync(_apiClient, groupName, users, cancellationToken),
            PullServerStatsAsync(_apiClient, groupName, group, users, cancellationToken));
    }

    /// <summary>
    /// Pulls server information via the SSMv1 API.
    /// </summary>
    /// <param name="apiClient">The SSMv1 API client.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The task that represents the operation.</returns>
    private async Task PullServerInfoAsync(SSMv1ApiClient apiClient, string groupName, CancellationToken cancellationToken = default)
    {
        try
        {
            ServerInfo = await apiClient.GetServerInfoAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, "Failed to pull server information", ex);
        }
    }

    /// <summary>
    /// Pulls user credentials via the SSMv1 API.
    /// Credentials with a matching local username overwrite existing local credentials.
    /// Unmatched credentials are ignored.
    /// </summary>
    /// <param name="apiClient">The SSMv1 API client.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="users">The <see cref="Users"/> object.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The task that represents the operation.</returns>
    private async Task PullServerUserCredentialsAsync(SSMv1ApiClient apiClient, string groupName, Users users, CancellationToken cancellationToken = default)
    {
        SSMv1UserInfoList? serverUsers;

        try
        {
            serverUsers = await apiClient.ListUsersAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, "Failed to pull user credentials", ex);
        }

        if (serverUsers is null)
        {
            return;
        }

        foreach (SSMv1UserInfo serverUser in serverUsers.Users)
        {
            if (users.UserDict.TryGetValue(serverUser.Username, out User? user))
            {
                if (user.Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
                {
                    memberInfo.Password = serverUser.UserPSK;
                }
                else
                {
                    user.Memberships.Add(groupName, new(ServerMethod, serverUser.UserPSK));
                }
            }
        }
    }

    /// <summary>
    /// Pulls server statistics via the SSMv1 API.
    /// </summary>
    /// <param name="apiClient">The SSMv1 API client.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="group">The <see cref="Group"/> object.</param>
    /// <param name="users">The <see cref="Users"/> object.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The task that represents the operation.</returns>
    private async Task PullServerStatsAsync(SSMv1ApiClient apiClient, string groupName, Group group, Users users, CancellationToken cancellationToken = default)
    {
        SSMv1Stats? serverStats;

        try
        {
            serverStats = await apiClient.GetAndClearStatsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, "Failed to pull server statistics", ex);
        }

        if (serverStats is null)
        {
            return;
        }

        // Do housekeeping of current and historical server stats.
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (today != CurrentStats.Date)
        {
            DateOnly keepAfterDate = today.AddDays(-StatsRetentionDays);

            while (HistoricalStats.TryPeek(out SSMv1StatsWithDate? oldestStats))
            {
                if (oldestStats.Date > keepAfterDate)
                {
                    break;
                }

                // Subtract from group bytes used.
                group.SubBytesUsed(oldestStats.GroupStats.DownlinkBytes + oldestStats.GroupStats.UplinkBytes);

                // Subtract from user bytes used.
                foreach (KeyValuePair<string, SSMv1StatsBase> userStatsEntry in oldestStats.UserStats)
                {
                    if (users.UserDict.TryGetValue(userStatsEntry.Key, out User? user))
                    {
                        ulong bytesUsed = userStatsEntry.Value.DownlinkBytes + userStatsEntry.Value.UplinkBytes;

                        user.SubBytesUsed(bytesUsed);

                        if (user.Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
                        {
                            memberInfo.SubBytesUsed(bytesUsed, group.PerUserDataLimitInBytes);
                        }
                    }
                }

                _ = HistoricalStats.Dequeue();
            }

            HistoricalStats.Enqueue(CurrentStats);
            CurrentStats = new();
        }

        // Update group data usage.
        AddStats(CurrentStats.GroupStats, serverStats);
        group.AddBytesUsed(serverStats.DownlinkBytes + serverStats.UplinkBytes);

        // Update user data usage.
        foreach (SSMv1UserStats serverUserStats in serverStats.Users)
        {
            if (CurrentStats.UserStats.TryGetValue(serverUserStats.Username, out SSMv1StatsBase? userStats))
            {
                AddStats(userStats, serverUserStats);
            }
            else
            {
                CurrentStats.UserStats.Add(serverUserStats.Username, serverUserStats);
            }

            if (users.UserDict.TryGetValue(serverUserStats.Username, out User? user))
            {
                ulong bytesUsed = serverUserStats.DownlinkBytes + serverUserStats.UplinkBytes;

                user.AddBytesUsed(bytesUsed);

                if (user.Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
                {
                    memberInfo.AddBytesUsed(bytesUsed, group.PerUserDataLimitInBytes);
                }
            }
        }

        static void AddStats(SSMv1StatsBase s1, SSMv1StatsBase s2)
        {
            s1.DownlinkPackets += s2.DownlinkPackets;
            s1.DownlinkBytes += s2.DownlinkBytes;
            s1.UplinkPackets += s2.UplinkPackets;
            s1.UplinkBytes += s2.UplinkBytes;
            s1.TcpSessions += s2.TcpSessions;
            s1.UdpSessions += s2.UdpSessions;
        }
    }

    /// <summary>
    /// Deploys local user configurations to the server.
    /// </summary>
    /// <param name="httpClient">An instance of <see cref="HttpClient"/>.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="users">The <see cref="Users"/> object.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> for iterating through completed tasks.</returns>
    /// <exception cref="GroupApiRequestException">The API request failed.</exception>
    public async IAsyncEnumerable<Task> DeployAsync(HttpClient httpClient, string groupName, Users users, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _apiClient ??= new(httpClient, BaseUri);

        // Query server users.
        SSMv1UserInfoList? serverUserList;
        try
        {
            serverUserList = await _apiClient.ListUsersAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, "Failed to pull user credentials", ex);
        }

        // Query local group members.
        // Generate passwords for those without one.
        Dictionary<string, MemberInfo> memberInfoByUsername = [];
        foreach (KeyValuePair<string, User> userEntry in users.UserDict)
        {
            if (userEntry.Value.Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
            {
                memberInfoByUsername.Add(userEntry.Key, memberInfo);

                if (string.IsNullOrEmpty(memberInfo.Method))
                {
                    memberInfo.Method = ServerMethod;
                }
                if (string.IsNullOrEmpty(memberInfo.Password))
                {
                    memberInfo.GeneratePassword();
                }
            }
        }

        List<Task> tasks = [];

        // Add tasks to delete server users that are not in the local group.
        // While here, create a dictionary of server users for later use.
        ReadOnlySpan<SSMv1UserInfo> serverUsers = serverUserList is not null ? serverUserList.Users : [];
        Dictionary<string, string> serverUserPSKByUsername = new(serverUsers.Length);
        foreach (SSMv1UserInfo serverUser in serverUsers)
        {
            if (memberInfoByUsername.ContainsKey(serverUser.Username))
            {
                serverUserPSKByUsername.Add(serverUser.Username, serverUser.UserPSK);
            }
            else
            {
                tasks.Add(DeleteServerUserAsync(_apiClient, groupName, serverUser.Username, cancellationToken));
            }
        }

        // Add tasks to add or update server users.
        foreach (KeyValuePair<string, MemberInfo> userEntry in memberInfoByUsername)
        {
            if (serverUserPSKByUsername.TryGetValue(userEntry.Key, out string? serverUserPSK))
            {
                if (userEntry.Value.Password != serverUserPSK)
                {
                    tasks.Add(UpdateServerUserAsync(_apiClient, groupName, userEntry.Key, userEntry.Value.Password, cancellationToken));
                }
            }
            else
            {
                tasks.Add(AddServerUserAsync(_apiClient, groupName, userEntry.Key, userEntry.Value.Password, cancellationToken));
            }
        }

        await foreach (Task task in Task.WhenEach(tasks))
        {
            yield return task;
        }
    }

    /// <summary>
    /// Deletes a server user.
    /// </summary>
    /// <param name="apiClient">The SSMv1 API client.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The task that represents the operation.</returns>
    /// <exception cref="GroupApiRequestException">The API request failed.</exception>
    private static async Task DeleteServerUserAsync(SSMv1ApiClient apiClient, string groupName, string username, CancellationToken cancellationToken = default)
    {
        try
        {
            await apiClient.DeleteUserAsync(username, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, $"Failed to delete user {username}", ex);
        }
    }

    /// <summary>
    /// Updates a server user.
    /// </summary>
    /// <param name="apiClient">The SSMv1 API client.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="username">The username.</param>
    /// <param name="password">The new password.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The task that represents the operation.</returns>
    /// <exception cref="GroupApiRequestException">The API request failed.</exception>
    private static async Task UpdateServerUserAsync(SSMv1ApiClient apiClient, string groupName, string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            await apiClient.UpdateUserAsync(username, password, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, $"Failed to update user {username}", ex);
        }
    }

    /// <summary>
    /// Adds a server user.
    /// </summary>
    /// <param name="apiClient">The SSMv1 API client.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>The task that represents the operation.</returns>
    /// <exception cref="GroupApiRequestException">The API request failed.</exception>
    private static async Task AddServerUserAsync(SSMv1ApiClient apiClient, string groupName, string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            SSMv1UserInfo user = new()
            {
                Username = username,
                UserPSK = password,
            };
            await apiClient.AddUserAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new GroupApiRequestException(groupName, $"Failed to add user {username}", ex);
        }
    }

    /// <summary>
    /// Clears the server statistics.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <param name="group">The <see cref="Group"/> object.</param>
    /// <param name="users">The <see cref="Users"/> object.</param>
    public void ClearServerStats(string groupName, Group group, Users users)
    {
        ClearServerStatsCore(groupName, group, users, CurrentStats);
        CurrentStats.Clear();

        foreach (SSMv1StatsWithDate stats in HistoricalStats)
        {
            ClearServerStatsCore(groupName, group, users, stats);
        }
        HistoricalStats.Clear();
    }

    private static void ClearServerStatsCore(string groupName, Group group, Users users, SSMv1StatsWithDate stats)
    {
        // Remove group stats
        group.SubBytesUsed(stats.GroupStats.DownlinkBytes + stats.GroupStats.UplinkBytes);

        // Remove user stats
        foreach (KeyValuePair<string, SSMv1StatsBase> userStatsEntry in stats.UserStats)
        {
            if (users.UserDict.TryGetValue(userStatsEntry.Key, out User? user))
            {
                ulong bytesUsed = userStatsEntry.Value.DownlinkBytes + userStatsEntry.Value.UplinkBytes;

                user.SubBytesUsed(bytesUsed);

                if (user.Memberships.TryGetValue(groupName, out MemberInfo? memberInfo))
                {
                    memberInfo.SubBytesUsed(bytesUsed, group.PerUserDataLimitInBytes);
                }
            }
        }
    }
}
