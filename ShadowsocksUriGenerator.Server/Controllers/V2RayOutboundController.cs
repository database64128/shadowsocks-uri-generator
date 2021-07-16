using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("v2ray/outbound")]
    public class V2RayOutboundController : ControllerBase
    {
        private readonly ILogger<V2RayOutboundController> _logger;
        private readonly IDataService _dataService;

        public V2RayOutboundController(ILogger<V2RayOutboundController> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Shadowsocks.Interop.V2Ray.Config> GetByUserId(string id, [FromQuery] string[] tag, [FromQuery] string[] group, [FromQuery] string[] groupOwner, [FromQuery] string[] nodeOwner, [FromQuery] bool sortByName)
        {
            var filteredUserEntries = _dataService.UsersData.UserDict.Where(x => x.Value.Uuid == id);

            if (!filteredUserEntries.Any())
            {
                return NotFound($"User ID {id} doesn't exist.");
            }

            var validGroups = group.All(x => _dataService.NodesData.Groups.ContainsKey(x));
            if (!validGroups)
            {
                return BadRequest("Not all groups exist.");
            }

            var validGroupOwners = FilterHelper.TryGetUserIds(_dataService.UsersData, groupOwner, out var targetGroupOwnerIds);
            if (!validGroupOwners)
            {
                return BadRequest("Not all group owners exist.");
            }

            var validNodeOwners = FilterHelper.TryGetUserIds(_dataService.UsersData, nodeOwner, out var targetNodeOwnerIds);
            if (!validNodeOwners)
            {
                return BadRequest("Not all node owners exist.");
            }

            var userEntry = filteredUserEntries.First();

            var config = new Shadowsocks.Interop.V2Ray.Config()
            {
                Outbounds = new(),
            };

            foreach (var membership in userEntry.Value.Memberships)
            {
                if ((group.Length == 0 || group.Contains(membership.Key))
                    && membership.Value.HasCredential
                    && _dataService.NodesData.Groups.TryGetValue(membership.Key, out var targetGroup)
                    && (targetGroupOwnerIds.Length == 0 || targetGroupOwnerIds.Contains(targetGroup.OwnerUuid)))
                {
                    foreach (var nodeEntry in targetGroup.NodeDict)
                    {
                        if (!nodeEntry.Value.Deactivated
                            && (targetNodeOwnerIds.Length == 0 || targetNodeOwnerIds.Contains(nodeEntry.Value.OwnerUuid))
                            && (tag.Length == 0 || tag.All(x => nodeEntry.Value.Tags.Exists(y => string.Equals(x, y, StringComparison.OrdinalIgnoreCase)))))
                        {
                            var server = new Shadowsocks.Models.Server()
                            {
                                Id = nodeEntry.Value.Uuid,
                                Name = nodeEntry.Key,
                                Host = nodeEntry.Value.Host,
                                Port = nodeEntry.Value.Port,
                                Method = membership.Value.Method,
                                Password = membership.Value.Password,
                                PluginPath = nodeEntry.Value.Plugin,
                                PluginOpts = nodeEntry.Value.PluginOpts,
                            };

                            config.Outbounds.Add(Shadowsocks.Interop.V2Ray.OutboundObject.GetShadowsocks(server));
                        }
                    }
                }
            }

            if (sortByName)
            {
                config.Outbounds = config.Outbounds.OrderBy(x => x.Tag).ToList();
            }

            _logger.LogInformation($"{userEntry.Key} ({id}) retrieved {config.Outbounds.Count} servers from {HttpContext.Connection.RemoteIpAddress} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

            return config;
        }
    }
}
