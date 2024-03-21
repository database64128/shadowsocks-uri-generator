using Microsoft.AspNetCore.Mvc;
using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.Server.Utils;
using ShadowsocksUriGenerator.Services;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ShadowsocksUriGenerator.Server.Controllers;

public abstract class OnlineConfigControllerBase(IDataService dataService) : ControllerBase
{
    protected IDataService DataService => dataService;

    protected bool TryGetUserEntry(
        string id,
        string[] group,
        string[] groupOwner,
        string[] nodeOwner,
        out string username,
        [NotNullWhen(true)] out User? user,
        [NotNullWhen(true)] out string[]? targetGroupOwnerIds,
        [NotNullWhen(true)] out string[]? targetNodeOwnerIds,
        [NotNullWhen(false)] out ObjectResult? objectResult)
    {
        username = "";
        user = null;
        targetGroupOwnerIds = null;
        targetNodeOwnerIds = null;
        objectResult = null;

        if (!dataService.UsersData.TryGetUserById(id, out var userEntry))
        {
            objectResult = NotFound($"User ID {id} doesn't exist.");
            return false;
        }

        var validGroups = group.All(x => dataService.NodesData.Groups.ContainsKey(x));
        if (!validGroups)
        {
            objectResult = BadRequest("Not all groups exist.");
            return false;
        }

        var validGroupOwners = FilterHelper.TryGetUserIds(dataService.UsersData, groupOwner, out targetGroupOwnerIds);
        if (!validGroupOwners)
        {
            objectResult = BadRequest("Not all group owners exist.");
            return false;
        }

        var validNodeOwners = FilterHelper.TryGetUserIds(dataService.UsersData, nodeOwner, out targetNodeOwnerIds);
        if (!validNodeOwners)
        {
            objectResult = BadRequest("Not all node owners exist.");
            return false;
        }

        username = userEntry.Key;
        user = userEntry.Value;
        return true;
    }
}
