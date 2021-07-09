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
            var hasUsernames = commandResult.Children.Contains("usernames") || commandResult.Children.Contains("--usernames");
            var hasAllUsers = commandResult.Children.Contains("--all-users");

            if (!hasUsernames && !hasAllUsers)
                return "Please either specify target users, or use `--all-users` to target all users.";
            if (hasUsernames && hasAllUsers)
                return "You can't specify target users when targeting all users with `--all-users`.";

            return null;
        }

        public static string? EnforceZeroNodenamesWhenAll(CommandResult commandResult)
        {
            var hasNodenames = commandResult.Children.Contains("nodenames") || commandResult.Children.Contains("--nodenames");
            var hasAllNodes = commandResult.Children.Contains("--all-nodes");

            if (!hasNodenames && !hasAllNodes)
                return "Please either specify target nodes, or use `--all-nodes` to target all nodes.";
            if (hasNodenames && hasAllNodes)
                return "You can't specify target nodes when targeting all nodes with `--all-nodes`.";

            return null;
        }

        public static string? EnforceZeroGroupsWhenAll(CommandResult commandResult)
        {
            var hasGroups = commandResult.Children.Contains("groups") || commandResult.Children.Contains("--groups");
            var hasAllGroups = commandResult.Children.Contains("--all-groups");

            if (!hasGroups && !hasAllGroups)
                return "Please either specify target groups, or use `--all-groups` to target all groups.";
            if (hasGroups && hasAllGroups)
                return "You can't specify target groups when targeting all users with `--all-groups`.";

            return null;
        }

        public static string? ValidateOwnerOptions(CommandResult commandResult)
        {
            var setOwner = commandResult.Children.Contains("--owner");
            var unsetOwner = commandResult.Children.Contains("--unset-owner");

            if (setOwner && unsetOwner)
                return "You can't set and unset owner at the same time.";
            else
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
