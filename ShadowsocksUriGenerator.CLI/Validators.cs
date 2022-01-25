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
            var hasUsernames = commandResult.Children.ContainsAlias("usernames") || commandResult.Children.ContainsAlias("--usernames");
            var hasAllUsers = commandResult.Children.ContainsAlias("--all-users");

            if (!hasUsernames && !hasAllUsers)
            {
                commandResult.ErrorMessage = "Please either specify target users, or use `--all-users` to target all users.";
            }
            else if (hasUsernames && hasAllUsers)
            {
                commandResult.ErrorMessage = "You can't specify target users when targeting all users with `--all-users`.";
            }
        }

        public static void EnforceZeroNodenamesWhenAll(CommandResult commandResult)
        {
            var hasNodenames = commandResult.Children.ContainsAlias("nodenames") || commandResult.Children.ContainsAlias("--nodenames");
            var hasAllNodes = commandResult.Children.ContainsAlias("--all-nodes");

            if (!hasNodenames && !hasAllNodes)
            {
                commandResult.ErrorMessage = "Please either specify target nodes, or use `--all-nodes` to target all nodes.";
            }
            else if (hasNodenames && hasAllNodes)
            {
                commandResult.ErrorMessage = "You can't specify target nodes when targeting all nodes with `--all-nodes`.";
            }
        }

        public static void EnforceZeroGroupsWhenAll(CommandResult commandResult)
        {
            var hasGroups = commandResult.Children.ContainsAlias("groups") || commandResult.Children.ContainsAlias("--groups");
            var hasAllGroups = commandResult.Children.ContainsAlias("--all-groups");

            if (!hasGroups && !hasAllGroups)
            {
                commandResult.ErrorMessage = "Please either specify target groups, or use `--all-groups` to target all groups.";
            }
            else if (hasGroups && hasAllGroups)
            {
                commandResult.ErrorMessage = "You can't specify target groups when targeting all users with `--all-groups`.";
            }
        }

        public static void ValidateOwnerOptions(CommandResult commandResult)
        {
            var setOwner = commandResult.Children.ContainsAlias("--owner");
            var unsetOwner = commandResult.Children.ContainsAlias("--unset-owner");

            if (setOwner && unsetOwner)
            {
                commandResult.ErrorMessage = "You can't set and unset owner at the same time.";
            }
        }

        public static void ValidateAddCredential(CommandResult commandResult)
        {
            var hasMethod = commandResult.Children.ContainsAlias("--method");
            var hasPassword = commandResult.Children.ContainsAlias("--password");
            var hasUserinfo = commandResult.Children.ContainsAlias("--userinfo-base64url");

            if (hasMethod && hasPassword && !hasUserinfo ||
                !hasMethod && !hasPassword && hasUserinfo)
            {
                return;
            }

            commandResult.ErrorMessage = "You must specify either `--method <method> --password <password>` or `--userinfo-base64url <base64url>`.";
        }
    }
}
