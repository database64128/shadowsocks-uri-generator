using System.CommandLine.Parsing;

namespace ShadowsocksUriGenerator.CLI
{
    /// <summary>
    /// Class for common utility validators.
    /// Do not put command-specific validators here.
    /// </summary>
    public static class Validators
    {
        public static string? EnforceZeroUsernamesWhenAll(CommandResult commandResult)
        {
            var hasUsernames = commandResult.Children.Contains("usernames");
            var hasAll = commandResult.Children.Contains("--all");

            if (!hasUsernames && !hasAll)
                return "Please either specify target users, or use `--all` to target all users.";
            if (hasUsernames && hasAll)
                return "You can't specify target users when targeting all users with `--all`.";

            return null;
        }

        public static string? EnforceZeroNodenamesWhenAll(CommandResult commandResult)
        {
            var hasNodenames = commandResult.Children.Contains("nodenames");
            var hasAll = commandResult.Children.Contains("--all");

            if (!hasNodenames && !hasAll)
                return "Please either specify target nodes, or use `--all` to target all nodes.";
            if (hasNodenames && hasAll)
                return "You can't specify target nodes when targeting all nodes with `--all`.";

            return null;
        }

        public static string? EnforceZeroGroupsWhenAll(CommandResult commandResult)
        {
            var hasGroups = commandResult.Children.Contains("groups");
            var hasAll = commandResult.Children.Contains("--all");

            if (!hasGroups && !hasAll)
                return "Please either specify target groups, or use `--all` to target all groups.";
            if (hasGroups && hasAll)
                return "Please don't specify target groups when targeting all users with `--all`.";

            return null;
        }

        public static string? ValidateAddCredential(CommandResult commandResult)
        {
            var hasMethod = commandResult.Children.Contains("--method");
            var hasPassword = commandResult.Children.Contains("--password");
            var hasUserinfo = commandResult.Children.Contains("--userinfo-base64url");

            if (hasMethod && hasPassword && !hasUserinfo ||
                !hasMethod && !hasPassword && hasUserinfo)
                return null;
            else
                return "You must specify either `--method <method> --password <password>` or `--userinfo-base64url <base64url>`.";
        }
    }
}
