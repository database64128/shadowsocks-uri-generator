using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Route("v2ray/outbound")]
public class V2RayOutboundController : OnlineConfigControllerBase
{
    private readonly ILogger<V2RayOutboundController> _logger;
    private readonly IDataService _dataService;

    public V2RayOutboundController(ILogger<V2RayOutboundController> logger, IDataService dataService) : base(dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    /// <summary>
    /// Gets online config by user ID in V2Ray outbound format.
    /// </summary>
    /// <remarks>
    /// Returns the online config document.
    ///
    ///     GET /[secret]/v2ray/outbound/[user_id]
    ///     {
    ///         "outbounds": [
    ///             "tag": "ServerName",
    ///             "protocol": "shadowsocks",
    ///             "settings": {
    ///                 "servers": [
    ///                     {
    ///                         "address": "example.com",
    ///                         "port": 8388,
    ///                         "method": "2022-blake3-aes-256-gcm",
    ///                         "password": "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=",
    ///                     }
    ///                 ]
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
    /// <returns>The online config document in V2Ray outbound format.</returns>
    /// <response code="200">Returns the online config document.</response>
    /// <response code="400">One or more queries contain invalid values.</response>
    /// <response code="404">The provided user ID doesn't exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Shadowsocks.Interop.V2Ray.Config> GetByUserId(string id, [FromQuery] string[] tag, [FromQuery] string[] group, [FromQuery] string[] groupOwner, [FromQuery] string[] nodeOwner, [FromQuery] bool sortByName)
    {
        if (!TryGetUserEntry(id, group, groupOwner, nodeOwner, out var username, out var user, out var targetGroupOwnerIds, out var targetNodeOwnerIds, out var objectResult))
            return objectResult;

        var servers = user.GetShadowsocksServers(_dataService.UsersData, _dataService.NodesData, group, tag, targetGroupOwnerIds, targetNodeOwnerIds);

        if (sortByName)
            servers = servers.OrderBy(x => x.Name);

        _logger.LogInformation($"{username} ({id}) retrieved {servers.Count()} servers from {HeaderHelper.GetRealIP(HttpContext)} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

        var config = new Shadowsocks.Interop.V2Ray.Config()
        {
            Outbounds = new(),
        };

        foreach (var server in servers)
        {
            if (!string.IsNullOrEmpty(server.PluginName))
                continue;

            config.Outbounds.Add(new()
            {
                Tag = server.Name,
                Protocol = "shadowsocks",
                Settings = new Shadowsocks.Interop.V2Ray.Protocols.Shadowsocks.OutboundConfigurationObject(server.Host, server.Port, server.Method, server.GetPassword())
            });
        }

        return config;
    }
}
