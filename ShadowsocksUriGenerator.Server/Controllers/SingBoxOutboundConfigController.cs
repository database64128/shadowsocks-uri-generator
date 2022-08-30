﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server.Filters;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[JsonSnakeCase]
[Route("sing-box/outbounds")]
public class SingBoxOutboundConfigController : OnlineConfigControllerBase
{
    private readonly ILogger<SingBoxOutboundConfigController> _logger;
    private readonly IDataService _dataService;

    public SingBoxOutboundConfigController(ILogger<SingBoxOutboundConfigController> logger, IDataService dataService) : base(dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    /// <summary>
    /// Gets online config by user ID in sing-box outbound config format.
    /// Visit https://sing-box.sagernet.org/ for documentation.
    /// </summary>
    /// <remarks>
    /// Returns the online config document.
    ///
    ///     GET /[secret]/sing-box/outbounds/[user_id]
    ///     {
    ///         "outbounds": [
    ///             {
    ///                 "type": "shadowsocks",
    ///                 "tag": "ServerName",
    ///                 "server": "example.com",
    ///                 "server_port": 8388,
    ///                 "method": "2022-blake3-aes-256-gcm",
    ///                 "password": "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw="
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
    /// <param name="network">Either "tcp" or "udp". If unspecified, both are enabled.</param>
    /// <param name="uot">Whether to enable UDP-over-TCP.</param>
    /// <param name="multiplex">Whether to enable multiplexing.</param>
    /// <param name="multiplexProtocol">Select a multiplexing protocol.</param>
    /// <param name="multiplexMaxConnections">Max number of connections.</param>
    /// <param name="multiplexMinStreams">Min number of streams.</param>
    /// <param name="multiplexMaxStreams">Max number of streams.</param>
    /// <param name="detour">Select an upstream outbound.</param>
    /// <param name="bindInterface">The network interface to bind to.</param>
    /// <param name="bindAddress">The address to bind to.</param>
    /// <param name="routingMark">Set a fwmark on sockets.</param>
    /// <param name="reuseAddr">Whether to set SO_REUSEADDR.</param>
    /// <param name="connectTimeout">The connect timeout in Go's duration string format.</param>
    /// <param name="tfo">Whether to enable TCP fast open.</param>
    /// <param name="domainStrategy">One of prefer_ipv4 prefer_ipv6 ipv4_only ipv6_only.</param>
    /// <param name="fallbackDelay">Happy Eyeballs fallback delay.</param>
    /// <returns>The online config document in sing-box config format.</returns>
    /// <response code="200">Returns the online config document.</response>
    /// <response code="400">One or more queries contain invalid values.</response>
    /// <response code="404">The provided user ID doesn't exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<SingBoxConfig> GetByUserId(
        string id,
        [FromQuery] string[] tag,
        [FromQuery] string[] group,
        [FromQuery] string[] groupOwner,
        [FromQuery] string[] nodeOwner,
        [FromQuery] bool sortByName,
        [FromQuery] string? network,
        [FromQuery] bool uot,
        [FromQuery] bool multiplex,
        [FromQuery] string? multiplexProtocol,
        [FromQuery] int multiplexMaxConnections,
        [FromQuery] int multiplexMinStreams,
        [FromQuery] int multiplexMaxStreams,
        [FromQuery] string? detour,
        [FromQuery] string? bindInterface,
        [FromQuery] string? bindAddress,
        [FromQuery] int routingMark,
        [FromQuery] bool reuseAddr,
        [FromQuery] string? connectTimeout,
        [FromQuery] bool tfo,
        [FromQuery] string? domainStrategy,
        [FromQuery] string? fallbackDelay)
    {
        if (!TryGetUserEntry(id, group, groupOwner, nodeOwner, out var username, out var user, out var targetGroupOwnerIds, out var targetNodeOwnerIds, out var objectResult))
            return objectResult;

        var servers = user.GetShadowsocksServers(_dataService.UsersData, _dataService.NodesData, group, tag, targetGroupOwnerIds, targetNodeOwnerIds);

        if (sortByName)
            servers = servers.OrderBy(x => x.Name);

        _logger.LogInformation($"{username} ({id}) retrieved {servers.Count()} servers from {HeaderHelper.GetRealIP(HttpContext)} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

        return new SingBoxConfig()
        {
            Outbounds = servers.Select(x => new SingBoxOutboundConfig(x, network, uot, multiplex, multiplexProtocol, multiplexMaxConnections, multiplexMinStreams, multiplexMaxStreams, detour, bindInterface, bindAddress, routingMark, reuseAddr, connectTimeout, tfo, domainStrategy, fallbackDelay)),
        };
    }
}