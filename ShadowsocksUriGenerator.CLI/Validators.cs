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
            var hasUsernames = commandResult.Children.ContainsAlias("usernames") || commandResult.Children.ContainsAlias("--usernames");
            var hasAllUsers = commandResult.Children.ContainsAlias("--all-users");

            if (!hasUsernames && !hasAllUsers)
                return "Please either specify target users, or use `--all-users` to target all users.";
            if (hasUsernames && hasAllUsers)
                return "You can't specify target users when targeting all users with `--all-users`.";

            return null;
        }

        public static string? EnforceZeroNodenamesWhenAll(CommandResult commandResult)
        {
            var hasNodenames = commandResult.Children.ContainsAlias("nodenames") || commandResult.Children.ContainsAlias("--nodenames");
            var hasAllNodes = commandResult.Children.ContainsAlias("--all-nodes");

            if (!hasNodenames && !hasAllNodes)
                return "Please either specify target nodes, or use `--all-nodes` to target all nodes.";
            if (hasNodenames && hasAllNodes)
                return "You can't specify target nodes when targeting all nodes with `--all-nodes`.";

            return null;
        }

        public static string? EnforceZeroGroupsWhenAll(CommandResult commandResult)
        {
            var hasGroups = commandResult.Children.ContainsAlias("groups") || commandResult.Children.ContainsAlias("--groups");
            var hasAllGroups = commandResult.Children.ContainsAlias("--all-groups");

            if (!hasGroups && !hasAllGroups)
                return "Please either specify target groups, or use `--all-groups` to target all groups.";
            if (hasGroups && hasAllGroups)
                return "You can't specify target groups when targeting all users with `--all-groups`.";

            return null;
        }

        public static string? ValidateOwnerOptions(CommandResult commandResult)
        {
            var setOwner = commandResult.Children.ContainsAlias("--owner");
            var unsetOwner = commandResult.Children.ContainsAlias("--unset-owner");

            if (setOwner && unsetOwner)
                return "You can't set and unset owner at the same time.";
            else
                return null;
        }

        public static string? ValidateAddCredential(CommandResult commandResult)
        {
            var hasMethod = commandResult.Children.ContainsAlias("--method");
            var hasPassword = commandResult.Children.ContainsAlias("--password");
            var hasUserinfo = commandResult.Children.ContainsAlias("--userinfo-base64url");

            if (hasMethod && hasPassword && !hasUserinfo ||
                !hasMethod && !hasPassword && hasUserinfo)
                return null;
            else
                return "You must specify either `--method <method> --password <password>` or `--userinfo-base64url <base64url>`.";
        }
    }
}
