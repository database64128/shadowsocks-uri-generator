using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Route("ooc/v1")]
public class OOCv1Controller : OnlineConfigControllerBase
{
    private readonly ILogger<OOCv1Controller> _logger;
    private readonly IDataService _dataService;

    public OOCv1Controller(ILogger<OOCv1Controller> logger, IDataService dataService) : base(dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    /// <summary>
    /// Gets online config by user ID in Open Online Config 1 format.
    /// </summary>
    /// <remarks>
    /// Returns the online config document.
    ///
    ///     GET /[secret]/ooc/v1/[user_id]
    ///     {
    ///         "username": "database64128",
    ///         "bytesUsed": 52940262597,
    ///         "bytesRemaining": 52940262597,
    ///         "protocols": [ "shadowsocks" ],
    ///         "shadowsocks": [
    ///             {
    ///                 "id": "27b8a625-4f4b-4428-9f0f-8a2317db7c79",
    ///                 "name": "ServerName",
    ///                 "owner": "database64128",
    ///                 "group": "examples",
    ///                 "tags": [ "direct" ],
    ///                 "address": "example.com",
    ///                 "port": 8388,
    ///                 "method": "2022-blake3-aes-256-gcm",
    ///                 "password": "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=",
    ///                 "pluginName": "plugin-name",
    ///                 "pluginVersion": "1.0",
    ///                 "pluginOptions": "whatever",
    ///                 "pluginArguments": "-vvvvvv"
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
    /// <returns>The online config document in Open Online Config 1 format.</returns>
    /// <response code="200">Returns the online config document.</response>
    /// <response code="400">One or more queries contain invalid values.</response>
    /// <response code="404">The provided user ID doesn't exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OOCv1ShadowsocksConfig> GetByUserId(string id, [FromQuery] string[] tag, [FromQuery] string[] group, [FromQuery] string[] groupOwner, [FromQuery] string[] nodeOwner, [FromQuery] bool sortByName)
    {
        if (!TryGetUserEntry(id, group, groupOwner, nodeOwner, out var username, out var user, out var targetGroupOwnerIds, out var targetNodeOwnerIds, out var objectResult))
            return objectResult;

        var servers = user.GetShadowsocksServers(_dataService.UsersData, _dataService.NodesData, group, tag, targetGroupOwnerIds, targetNodeOwnerIds);

        if (sortByName)
            servers = servers.OrderBy(x => x.Name);

        _logger.LogInformation($"{username} ({id}) retrieved {servers.Count()} servers from {HeaderHelper.GetRealIP(HttpContext)} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

        return new OOCv1ShadowsocksConfig()
        {
            Username = username,
            BytesUsed = user.BytesUsed > 0UL ? user.BytesUsed : null,
            BytesRemaining = user.BytesRemaining > 0UL ? user.BytesRemaining : null,
            Shadowsocks = servers.Select(x => new OOCv1ShadowsocksServer(x)),
        };
    }
}
