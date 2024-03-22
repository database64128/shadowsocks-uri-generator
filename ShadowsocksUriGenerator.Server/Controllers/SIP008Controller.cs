using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using ShadowsocksUriGenerator.Utils;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Route("sip008")]
public class SIP008Controller(ILogger<SIP008Controller> logger, IDataService dataService) : OnlineConfigControllerBase(dataService)
{

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
        if (!TryGetUserEntry(id, group, groupOwner, nodeOwner, out var username, out var user, out var targetGroupOwnerIds, out var targetNodeOwnerIds, out var objectResult))
            return objectResult;

        var servers = user.GetShadowsocksServers(DataService.UsersData, DataService.NodesData, group, tag, targetGroupOwnerIds, targetNodeOwnerIds);

        if (sortByName)
            servers = servers.OrderBy(x => x.Name);

        LoggerHelper.OnlineConfig(logger, username, id, HeaderHelper.GetRealIP(HttpContext), HttpContext.Request.Query);

        var resp = new SIP008Config()
        {
            Username = username,
            Id = id,
            BytesUsed = user.BytesUsed > 0UL ? user.BytesUsed : null,
            BytesRemaining = user.BytesRemaining > 0UL ? user.BytesRemaining : null,
            Servers = servers.Select(x => new SIP008Server(x)),
        };

        return new JsonResult(resp, FileHelper.APISnakeCaseJsonSerializerOptions);
    }
}
