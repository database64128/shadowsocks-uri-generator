using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server.Filters;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [JsonSnakeCase]
    [Route("sip008")]
    public class SIP008Controller : ControllerBase
    {
        private readonly ILogger<SIP008Controller> _logger;
        private readonly IDataService _dataService;

        public SIP008Controller(ILogger<SIP008Controller> logger, IDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<SIP008Config> GetByUserId(string id, [FromQuery] string[] tag, [FromQuery] string[] group, [FromQuery] string[] groupOwner, [FromQuery] string[] nodeOwner, [FromQuery] bool sortByName)
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

            var config = new SIP008Config()
            {
                Username = userEntry.Key,
                Id = id,
            };

            if (userEntry.Value.BytesUsed > 0UL)
            {
                config.BytesUsed = userEntry.Value.BytesUsed;
            }

            if (userEntry.Value.BytesRemaining > 0UL)
            {
                config.BytesRemaining = userEntry.Value.BytesRemaining;
            }

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
                                Password = membership.Value.Password,
                                PluginPath = nodeEntry.Value.Plugin,
                                PluginOpts = nodeEntry.Value.PluginOpts,
                                Owner = owner,
                                Tags = tags,
                            });
                        }
                    }
                }
            }

            if (sortByName)
            {
                config.Servers = config.Servers.OrderBy(x => x.Name).ToList();
            }

            _logger.LogInformation($"{userEntry.Key} ({id}) retrieved {config.Servers.Count} servers from {HeaderHelper.GetRealIP(HttpContext)} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

            return config;
        }
    }
}
