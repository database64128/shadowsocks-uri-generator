using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server.Filters;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[JsonSnakeCase]
[Route("sip008")]
public class SIP008Controller : OnlineConfigControllerBase
{
    private readonly ILogger<SIP008Controller> _logger;
    private readonly IDataService _dataService;

    public SIP008Controller(ILogger<SIP008Controller> logger, IDataService dataService) : base(dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    /// <summary>
    /// Gets online config by user ID in SIP008 format.
    /// </summary>
    /// <remarks>
    /// Returns the online config document.
    ///
    ///     GET /[secret]/sip008/[user_id]
    ///     {
    ///         "version": 1,
    ///         "username": "database64128",
    ///         "id": "[user_id]",
    ///         "bytes_used": 52940262597,
    ///         "bytes_remaining": 52940262597,
    ///         "servers": [
    ///             {
    ///                 "id": "27b8a625-4f4b-4428-9f0f-8a2317db7c79",
    ///                 "remarks": "ServerName",
    ///                 "owner": "database64128",
    ///                 "group": "examples",
    ///                 "tags": [ "direct" ],
    ///                 "server": "example.com",
    ///                 "server_port": 8388,
    ///                 "method": "2022-blake3-aes-256-gcm",
    ///                 "password": "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=",
    ///                 "plugin": "plugin-name",
    ///                 "plugin_version": "1.0",
    ///                 "plugin_opts": "whatever",
    ///                 "plugin_args": "-vvvvvv"
    ///             }
    ///         ]
    ///     }
    ///
    /// </remarks>
    /// <param name="id">User ID.</param>
    /// <param name="tag">Select nodes that contain all tags in this array.</param>
    /// <param name="group">Select nodes from groups in this array.</param>
    /// <param name="groupOwner">Select nodes from groups that belong to users in this array.</param>
    /// <param name="nodeOwner">Select nodes that belong to users in this array.</param>
    /// <param name="sortByName">Whether to sort nodes by name. Defaults to false, or no sorting.</param>
    /// <returns>The online config document in SIP008 format.</returns>
    /// <response code="200">Returns the online config document.</response>
    /// <response code="400">One or more queries contain invalid values.</response>
    /// <response code="404">The provided user ID doesn't exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SIP008Config> GetByUserId(string id, [FromQuery] string[] tag, [FromQuery] string[] group, [FromQuery] string[] groupOwner, [FromQuery] string[] nodeOwner, [FromQuery] bool sortByName)
    {
        var ret = TryGetUserEntry(id, group, groupOwner, nodeOwner, out var username, out var user, out var targetGroupOwnerIds, out var targetNodeOwnerIds, out var objectResult);
        if (!ret)
            return objectResult!;

        var config = new SIP008Config()
        {
            Username = username,
            Id = id,
        };

        if (user!.BytesUsed > 0UL)
            config.BytesUsed = user.BytesUsed;

        if (user.BytesRemaining > 0UL)
            config.BytesRemaining = user.BytesRemaining;

        foreach (var membership in user.Memberships)
        {
            if ((group.Length == 0 || group.Contains(membership.Key))
                && membership.Value.HasCredential
                && _dataService.NodesData.Groups.TryGetValue(membership.Key, out var targetGroup)
                && (targetGroupOwnerIds!.Length == 0 || targetGroupOwnerIds.Contains(targetGroup.OwnerUuid)))
            {
                foreach (var nodeEntry in targetGroup.NodeDict)
                {
                    if (!nodeEntry.Value.Deactivated
                        && (targetNodeOwnerIds!.Length == 0 || targetNodeOwnerIds.Contains(nodeEntry.Value.OwnerUuid))
                        && (tag.Length == 0 || tag.All(x => nodeEntry.Value.Tags.Exists(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)))))
                    {
                        var owner = nodeEntry.Value.OwnerUuid is not null
                            ? _dataService.UsersData.UserDict.Where(x => x.Value.Uuid == nodeEntry.Value.OwnerUuid)
                                                             .Select(x => x.Key)
                                                             .FirstOrDefault()
                            : null;

                        var tags = nodeEntry.Value.Tags.Count > 0
                            ? nodeEntry.Value.Tags
                            : null;

                        config.Servers.Add(new()
                        {
                            Id = nodeEntry.Value.Uuid,
                            Name = nodeEntry.Key,
                            Host = nodeEntry.Value.Host,
                            Port = nodeEntry.Value.Port,
                            Method = membership.Value.Method,
                            Password = membership.Value.PasswordForNode(nodeEntry.Value.IdentityPSKs),
                            PluginName = nodeEntry.Value.Plugin,
                            PluginVersion = nodeEntry.Value.PluginVersion,
                            PluginOptions = nodeEntry.Value.PluginOpts,
                            PluginArguments = nodeEntry.Value.PluginArguments,
                            Group = membership.Key,
                            Owner = owner,
                            Tags = tags,
                        });
                    }
                }
            }
        }

        if (sortByName)
            config.Servers = config.Servers.OrderBy(x => x.Name).ToList();

        _logger.LogInformation($"{username} ({id}) retrieved {config.Servers.Count} servers from {HeaderHelper.GetRealIP(HttpContext)} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

        return config;
    }
}
