using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class ReportCommand
    {
        public static async Task<int> Generate(SortBy? groupSortBy, SortBy? userSortBy, CancellationToken cancellationToken = default)
        {
            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                Console.WriteLine(loadUsersErrMsg);
                return 1;
            }

            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                Console.WriteLine(loadNodesErrMsg);
                return 1;
            }
            using var nodes = loadedNodes;

            var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (loadSettingsErrMsg is not null)
            {
                Console.WriteLine(loadSettingsErrMsg);
                return 1;
            }

            // collect data
            var totalBytesUsed = nodes.Groups.Select(x => x.Value.BytesUsed).Aggregate(0UL, (x, y) => x + y);
            var totalBytesRemaining = nodes.Groups.All(x => x.Value.DataLimitInBytes > 0UL)
                ? nodes.Groups.Select(x => x.Value.BytesRemaining).Aggregate(0UL, (x, y) => x + y)
                : 0UL;

            var recordsByGroup = nodes.GetDataUsageByGroup();
            var recordsByUser = users.GetDataUsageByUser(nodes);

            // calculate column width
            var maxGroupNameLength = recordsByGroup.Select(x => x.group.Length)
                                                   .DefaultIfEmpty()
                                                   .Max();
            var groupNameFieldWidth = maxGroupNameLength > 5 ? maxGroupNameLength + 2 : 7;
            var maxUsernameLength = recordsByUser.Select(x => x.username.Length)
                                                 .DefaultIfEmpty()
                                                 .Max();
            var usernameFieldWidth = maxUsernameLength > 4 ? maxUsernameLength + 2 : 6;

            // sort
            var groupSortByInEffect = settings.GroupDataUsageDefaultSortBy;
            if (groupSortBy is SortBy currentRunGroupSortBy)
                groupSortByInEffect = currentRunGroupSortBy;
            switch (groupSortByInEffect)
            {
                case SortBy.DefaultAscending:
                    break;
                case SortBy.DefaultDescending:
                    recordsByGroup.Reverse();
                    break;
                case SortBy.NameAscending:
                    recordsByGroup = recordsByGroup.OrderBy(x => x.group).ToList();
                    break;
                case SortBy.NameDescending:
                    recordsByGroup = recordsByGroup.OrderByDescending(x => x.group).ToList();
                    break;
                case SortBy.DataUsedAscending:
                    recordsByGroup = recordsByGroup.OrderBy(x => x.bytesUsed).ToList();
                    break;
                case SortBy.DataUsedDescending:
                    recordsByGroup = recordsByGroup.OrderByDescending(x => x.bytesUsed).ToList();
                    break;
                case SortBy.DataRemainingAscending:
                    recordsByGroup = recordsByGroup.OrderBy(x => x.bytesRemaining).ToList();
                    break;
                case SortBy.DataRemainingDescending:
                    recordsByGroup = recordsByGroup.OrderByDescending(x => x.bytesRemaining).ToList();
                    break;
            }
            var userSortByInEffect = settings.UserDataUsageDefaultSortBy;
            if (userSortBy is SortBy currentRunUserSortBy)
                userSortByInEffect = currentRunUserSortBy;
            switch (userSortByInEffect)
            {
                case SortBy.DefaultAscending:
                    break;
                case SortBy.DefaultDescending:
                    recordsByUser.Reverse();
                    break;
                case SortBy.NameAscending:
                    recordsByUser = recordsByUser.OrderBy(x => x.username).ToList();
                    break;
                case SortBy.NameDescending:
                    recordsByUser = recordsByUser.OrderByDescending(x => x.username).ToList();
                    break;
                case SortBy.DataUsedAscending:
                    recordsByUser = recordsByUser.OrderBy(x => x.bytesUsed).ToList();
                    break;
                case SortBy.DataUsedDescending:
                    recordsByUser = recordsByUser.OrderByDescending(x => x.bytesUsed).ToList();
                    break;
                case SortBy.DataRemainingAscending:
                    recordsByUser = recordsByUser.OrderBy(x => x.bytesRemaining).ToList();
                    break;
                case SortBy.DataRemainingDescending:
                    recordsByUser = recordsByUser.OrderByDescending(x => x.bytesRemaining).ToList();
                    break;
            }

            // total
            Console.WriteLine("In the last 30 days");
            Console.WriteLine();
            if (totalBytesUsed != 0UL)
                Console.WriteLine($"{"Total data used",-24}{Utilities.HumanReadableDataString(totalBytesUsed)}");
            if (totalBytesRemaining != 0UL)
                Console.WriteLine($"{"Total data remaining",-24}{Utilities.HumanReadableDataString(totalBytesRemaining)}");
            Console.WriteLine();

            // by group
            Console.WriteLine("Data usage by group");
            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11, 16);
            Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11, 16);
            foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
            {
                Console.Write($"|{group.PadRight(groupNameFieldWidth)}|");
                if (bytesUsed != 0UL)
                    Console.Write($"{Utilities.HumanReadableDataString(bytesUsed),11}|");
                else
                    Console.Write($"{string.Empty,11}|");
                if (bytesRemaining != 0UL)
                    Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                else
                    Console.WriteLine($"{string.Empty,16}|");
            }
            ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11, 16);
            Console.WriteLine();

            // by user
            Console.WriteLine("Data usage by user");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11, 16);
            Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11, 16);
            foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
            {
                Console.Write($"|{username.PadRight(usernameFieldWidth)}|");
                if (bytesUsed != 0UL)
                    Console.Write($"{Utilities.HumanReadableDataString(bytesUsed),11}|");
                else
                    Console.Write($"{string.Empty,11}|");
                if (bytesRemaining != 0UL)
                    Console.WriteLine($"{Utilities.HumanReadableDataString(bytesRemaining),16}|");
                else
                    Console.WriteLine($"{string.Empty,16}|");
            }
            ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11, 16);

            return 0;
        }
    }
}
