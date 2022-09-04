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
[Route("shadowsocks-go/clients")]
public class ShadowsocksGoClientConfigController : OnlineConfigControllerBase
{
    private readonly ILogger<ShadowsocksGoClientConfigController> _logger;
    private readonly IDataService _dataService;

    public ShadowsocksGoClientConfigController(ILogger<ShadowsocksGoClientConfigController> logger, IDataService dataService) : base(dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    /// <summary>
    /// Gets online config by user ID in shadowsocks-go client config format.
    /// </summary>
    /// <remarks>
    /// Returns the online config document.
    ///
    ///     GET /[secret]/shadowsocks-go/clients/[user_id]
    ///     {
    ///         "clients": [
    ///             {
    ///                 "name": "ServerName",
    ///                 "endpoint": "[2001:db8:bd63:362c:2071:a0f6:827:ab6a]:20220",
    ///                 "protocol": "2022-blake3-aes-128-gcm",
    ///                 "dialerFwmark": 0,
    ///                 "enableTCP": true,
    ///                 "dialerTFO": true,
    ///                 "enableUDP": true,
    ///                 "mtu": 1500,
    ///                 "psk": "qQln3GlVCZi5iJUObJVNCw==",
    ///                 "iPSKs": [
    ///                     "oE/s2z9Q8EWORAB8B3UCxw=="
    ///                 ],
    ///                 "paddingPolicy": ""
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
    /// <param name="paddingPolicy">The padding policy to use for outgoing Shadowsocks traffic.</param>
    /// <param name="sortByName">Whether to sort nodes by name. Defaults to false, or no sorting.</param>
    /// <param name="noDirect">
    /// If set to true, the generated clients won't include a direct client.
    /// By default a direct client is added since this is most certainly what the user wants.
    /// </param>
    /// <param name="disableTCP">Whether to disable TCP for servers.</param>
    /// <param name="disableTFO">Whether to disable TCP Fast Open for servers.</param>
    /// <param name="disableUDP">Whether to disable UDP for servers.</param>
    /// <param name="dialerFwmark">Set a fwmark for sockets.</param>
    /// <param name="mtu">The path MTU between client and server. Defaults to 1492 for PPPoE.</param>
    /// <returns>The online config document in shadowsocks-go client config format.</returns>
    /// <response code="200">Returns the online config document.</response>
    /// <response code="400">One or more queries contain invalid values.</response>
    /// <response code="404">The provided user ID doesn't exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ShadowsocksGoConfig> GetByUserId(
        string id,
        [FromQuery] string[] tag,
        [FromQuery] string[] group,
        [FromQuery] string[] groupOwner,
        [FromQuery] string[] nodeOwner,
        [FromQuery] string? paddingPolicy,
        [FromQuery] bool sortByName,
        [FromQuery] bool noDirect,
        [FromQuery] bool disableTCP,
        [FromQuery] bool disableTFO,
        [FromQuery] bool disableUDP,
        [FromQuery] int dialerFwmark,
        [FromQuery] int mtu = 1492)
    {
        if (!TryGetUserEntry(id, group, groupOwner, nodeOwner, out var username, out var user, out var targetGroupOwnerIds, out var targetNodeOwnerIds, out var objectResult))
            return objectResult;

        var servers = user.GetShadowsocksServers(_dataService.UsersData, _dataService.NodesData, group, tag, targetGroupOwnerIds, targetNodeOwnerIds);

        if (sortByName)
            servers = servers.OrderBy(x => x.Name);

        LoggerHelper.OnlineConfig(_logger, username, id, HeaderHelper.GetRealIP(HttpContext), HttpContext.Request.Query);

        var clients = servers.Select(x => new ShadowsocksGoClientConfig(x, paddingPolicy, disableTCP, disableTFO, disableUDP, dialerFwmark, mtu));

        if (!noDirect)
            clients = clients.Append(new(disableTCP, disableTFO, disableUDP, dialerFwmark, mtu));

        return new ShadowsocksGoConfig()
        {
            Clients = clients,
        };
    }
}
