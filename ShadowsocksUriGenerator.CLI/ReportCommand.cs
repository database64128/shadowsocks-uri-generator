using ShadowsocksUriGenerator.CLI.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.CLI
{
    public static class ReportCommand
    {
        public static async Task<int> Generate(SortBy? groupSortBy, SortBy? userSortBy, string? csvOutdir, CancellationToken cancellationToken = default)
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
            var recordsByUser = users.GetDataUsageByUser();

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
            var groupSortByInEffect = groupSortBy ?? settings.GroupDataUsageDefaultSortBy;
            recordsByGroup = groupSortByInEffect switch
            {
                SortBy.DefaultAscending => recordsByGroup,
                SortBy.DefaultDescending => recordsByGroup.Reverse(),
                SortBy.NameAscending => recordsByGroup.OrderBy(x => x.group),
                SortBy.NameDescending => recordsByGroup.OrderByDescending(x => x.group),
                SortBy.DataUsedAscending => recordsByGroup.OrderBy(x => x.bytesUsed),
                SortBy.DataUsedDescending => recordsByGroup.OrderByDescending(x => x.bytesUsed),
                SortBy.DataRemainingAscending => recordsByGroup.OrderBy(x => x.bytesRemaining),
                SortBy.DataRemainingDescending => recordsByGroup.OrderByDescending(x => x.bytesRemaining),
                _ => throw new NotImplementedException("This sort method is not implemented!"),
            };

            var userSortByInEffect = userSortBy ?? settings.UserDataUsageDefaultSortBy;
            recordsByUser = userSortByInEffect switch
            {
                SortBy.DefaultAscending => recordsByUser,
                SortBy.DefaultDescending => recordsByUser.Reverse(),
                SortBy.NameAscending => recordsByUser.OrderBy(x => x.username),
                SortBy.NameDescending => recordsByUser.OrderByDescending(x => x.username),
                SortBy.DataUsedAscending => recordsByUser.OrderBy(x => x.bytesUsed),
                SortBy.DataUsedDescending => recordsByUser.OrderByDescending(x => x.bytesUsed),
                SortBy.DataRemainingAscending => recordsByUser.OrderBy(x => x.bytesRemaining),
                SortBy.DataRemainingDescending => recordsByUser.OrderByDescending(x => x.bytesRemaining),
                _ => throw new NotImplementedException("This sort method is not implemented!"),
            };

            // total
            Console.WriteLine("In the last 30 days:");
            Console.WriteLine();

            if (totalBytesUsed != 0UL)
                Console.WriteLine($"{"Total data used",-24}{Utilities.HumanReadableDataString1024(totalBytesUsed)}");

            if (totalBytesRemaining != 0UL)
                Console.WriteLine($"{"Total data remaining",-24}{Utilities.HumanReadableDataString1024(totalBytesRemaining)}");

            Console.WriteLine();

            // by group
            Console.WriteLine("Data usage by group");

            if (recordsByGroup.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
            {
                ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11);
                Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|");
                ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11);

                foreach (var (group, bytesUsed, _) in recordsByGroup)
                {
                    Console.Write($"|{group.PadRight(groupNameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        Console.WriteLine($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        Console.WriteLine($"{string.Empty,11}|");
                }

                ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11);
            }
            else
            {
                ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11, 16);
                Console.WriteLine($"|{"Group".PadRight(groupNameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11, 16);

                foreach (var (group, bytesUsed, bytesRemaining) in recordsByGroup)
                {
                    Console.Write($"|{group.PadRight(groupNameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        Console.Write($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        Console.Write($"{string.Empty,11}|");

                    if (bytesRemaining != 0UL)
                        Console.WriteLine($"{Utilities.HumanReadableDataString1024(bytesRemaining),16}|");
                    else
                        Console.WriteLine($"{string.Empty,16}|");
                }

                ConsoleHelper.PrintTableBorder(groupNameFieldWidth, 11, 16);
            }

            Console.WriteLine();

            // by user
            Console.WriteLine("Data usage by user");

            if (recordsByUser.All(x => x.bytesRemaining == 0UL)) // Omit data remaining column if no data.
            {
                ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11);
                Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|");
                ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11);

                foreach (var (username, bytesUsed, _) in recordsByUser)
                {
                    Console.Write($"|{username.PadRight(usernameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        Console.WriteLine($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        Console.WriteLine($"{string.Empty,11}|");
                }

                ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11);
            }
            else
            {
                ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11, 16);
                Console.WriteLine($"|{"User".PadRight(usernameFieldWidth)}|{"Data Used",11}|{"Data Remaining",16}|");
                ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11, 16);

                foreach (var (username, bytesUsed, bytesRemaining) in recordsByUser)
                {
                    Console.Write($"|{username.PadRight(usernameFieldWidth)}|");

                    if (bytesUsed != 0UL)
                        Console.Write($"{Utilities.HumanReadableDataString1024(bytesUsed),11}|");
                    else
                        Console.Write($"{string.Empty,11}|");

                    if (bytesRemaining != 0UL)
                        Console.WriteLine($"{Utilities.HumanReadableDataString1024(bytesRemaining),16}|");
                    else
                        Console.WriteLine($"{string.Empty,16}|");
                }

                ConsoleHelper.PrintTableBorder(usernameFieldWidth, 11, 16);
            }

            // CSV
            if (!string.IsNullOrEmpty(csvOutdir))
            {
                var (dataUsageByGroup, dataUsageByUser) = ReportHelper.GenerateDataUsageCSV(recordsByGroup, recordsByUser);

                try
                {
                    _ = Directory.CreateDirectory(csvOutdir);

                    var writeDataUsageByGroupTask = File.WriteAllTextAsync($"{csvOutdir}/data-usage-by-group.csv", dataUsageByGroup, cancellationToken);
                    var writeDataUsageByUserTask = File.WriteAllTextAsync($"{csvOutdir}/data-usage-by-user.csv", dataUsageByUser, cancellationToken);

                    await Task.WhenAll(writeDataUsageByGroupTask, writeDataUsageByUserTask);

                    Console.WriteLine();
                    Console.WriteLine($"Written to {csvOutdir}/data-usage-by-group.csv");
                    Console.WriteLine($"Written to {csvOutdir}/data-usage-by-user.csv");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error when saving CSV: {ex.Message}");
                }
            }

            return 0;
        }
    }
}
