using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Rescue
{
    /// <summary>
    /// Class for static methods that
    /// restore order from information collected by rescuers.
    /// </summary>
    public static class Restorers
    {
        /// <summary>
        /// Restores rescued <paramref name="users"/> and <paramref name="nodes"/>
        /// to JSON config files.
        /// </summary>
        /// <param name="configDir">Path to JSON config directory.</param>
        /// <param name="users">The rescued <see cref="Users"/> object.</param>
        /// <param name="nodes">The rescued <see cref="Nodes"/> object.</param>
        /// <param name="cancellationToken">A token that may be used to cancel the write operation.</param>
        /// <returns>An error message. Null if no errors occurred.</returns>
        public static async Task<string?> ToJsonFiles(string configDir, Users users, Nodes nodes, CancellationToken cancellationToken = default)
        {
            // Make sure configDir is either empty or a path that ends with a slash.
            if (!string.IsNullOrEmpty(configDir) && !(configDir.EndsWith('/') || configDir.EndsWith('\\')))
                configDir = $"{configDir}/";

            var usersErrMsg = await FileHelper.SaveJsonAsync($"{configDir}Users.json",
                                                            users,
                                                            FileHelper.dataJsonSerializerOptions,
                                                            false,
                                                            false,
                                                            cancellationToken);
            var nodesErrMsg = await FileHelper.SaveJsonAsync($"{configDir}Nodes.json",
                                                            nodes,
                                                            FileHelper.dataJsonSerializerOptions,
                                                            false,
                                                            false,
                                                            cancellationToken);

            if (usersErrMsg is not null && nodesErrMsg is not null)
                return $"{usersErrMsg}{Environment.NewLine}{nodesErrMsg}";
            else if (usersErrMsg is not null)
                return usersErrMsg;
            else if (nodesErrMsg is not null)
                return nodesErrMsg;
            else
                return null;
        }
    }
}
