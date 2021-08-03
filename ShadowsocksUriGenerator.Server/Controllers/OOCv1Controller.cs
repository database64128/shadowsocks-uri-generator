﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.OnlineConfig;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("ooc/v1")]
    public class OOCv1Controller : ControllerBase
    {
        private readonly ILogger<OOCv1Controller> _logger;
        private readonly IDataService _dataService;

        public OOCv1Controller(ILogger<OOCv1Controller> logger, IDataService dataService)
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
        ///                 "method": "chacha20-ietf-poly1305",
        ///                 "password": "example",
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

            var config = new OOCv1ShadowsocksConfig()
            {
                Username = userEntry.Key,
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

                            config.Shadowsocks.Add(new()
                            {
                                Id = nodeEntry.Value.Uuid,
                                Name = nodeEntry.Key,
                                Host = nodeEntry.Value.Host,
                                Port = nodeEntry.Value.Port,
                                Method = membership.Value.Method,
                                Password = membership.Value.Password,
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
            {
                config.Shadowsocks = config.Shadowsocks.OrderBy(x => x.Name).ToList();
            }

            _logger.LogInformation($"{userEntry.Key} ({id}) retrieved {config.Shadowsocks.Count} servers from {HeaderHelper.GetRealIP(HttpContext)} under constraints of {tag.Length} tags, {group.Length} groups, {groupOwner.Length} group owners, {nodeOwner.Length} node owners, sortByName: {sortByName}.");

            return config;
        }
    }
}
