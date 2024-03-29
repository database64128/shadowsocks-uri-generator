﻿using ShadowsocksUriGenerator.CLI.Utils;
using System.CommandLine.Parsing;

namespace ShadowsocksUriGenerator.CLI
{
    /// <summary>
    /// Class for common utility validators.
    /// Do not put command-specific validators here.
    /// </summary>
    public static class Validators
    {
        public static void EnforceZeroUsernamesWhenAll(CommandResult commandResult)
        {
            var hasUsernames = commandResult.ContainsOptionWithName("--usernames");
            var hasAllUsers = commandResult.ContainsOptionWithName("--all-users");

            if (!hasUsernames && !hasAllUsers)
            {
                commandResult.AddError("Please either specify target users, or use `--all-users` to target all users.");
            }
            else if (hasUsernames && hasAllUsers)
            {
                commandResult.AddError("You can't specify target users when targeting all users with `--all-users`.");
            }
        }

        public static void EnforceZeroNodenamesWhenAll(CommandResult commandResult)
        {
            var hasNodenames = commandResult.ContainsOptionWithName("--nodenames");
            var hasAllNodes = commandResult.ContainsOptionWithName("--all-nodes");

            if (!hasNodenames && !hasAllNodes)
            {
                commandResult.AddError("Please either specify target nodes, or use `--all-nodes` to target all nodes.");
            }
            else if (hasNodenames && hasAllNodes)
            {
                commandResult.AddError("You can't specify target nodes when targeting all nodes with `--all-nodes`.");
            }
        }

        public static void EnforceZeroGroupsWhenAll(CommandResult commandResult)
        {
            var hasGroups = commandResult.ContainsOptionWithName("--groups");
            var hasAllGroups = commandResult.ContainsOptionWithName("--all-groups");

            if (!hasGroups && !hasAllGroups)
            {
                commandResult.AddError("Please either specify target groups, or use `--all-groups` to target all groups.");
            }
            else if (hasGroups && hasAllGroups)
            {
                commandResult.AddError("You can't specify target groups when targeting all users with `--all-groups`.");
            }
        }

        public static void ValidateOwnerOptions(CommandResult commandResult)
        {
            var setOwner = commandResult.ContainsOptionWithName("--owner");
            var unsetOwner = commandResult.ContainsOptionWithName("--unset-owner");

            if (setOwner && unsetOwner)
            {
                commandResult.AddError("You can't set and unset owner at the same time.");
            }
        }
    }
}
