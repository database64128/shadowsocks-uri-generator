using System.CommandLine;
using System.CommandLine.Parsing;

namespace ShadowsocksUriGenerator.CLI
{
    /// <summary>
    /// Class for common utility validators.
    /// Do not put command-specific validators here.
    /// </summary>
    public static class Validators
    {
        public static Action<CommandResult> EnforceZeroUsernamesArgumentWhenAll(Argument<string[]> usernamesArgumentZeroOrMore, Option<bool> allUsersOption) => commandResult =>
        {
            bool hasUsernames = commandResult.GetValue(usernamesArgumentZeroOrMore)?.Length > 0;
            bool hasAllUsers = commandResult.GetValue(allUsersOption);
            if (!hasUsernames && !hasAllUsers)
            {
                commandResult.AddError("Please either specify target users, or use `--all-users` to target all users.");
            }
            else if (hasUsernames && hasAllUsers)
            {
                commandResult.AddError("You can't specify target users when targeting all users with `--all-users`.");
            }
        };

        public static Action<CommandResult> EnforceZeroNodenamesArgumentWhenAll(Argument<string[]> nodenamesArgumentZeroOrMore, Option<bool> allNodesOption) => commandResult =>
        {
            bool hasNodenames = commandResult.GetValue(nodenamesArgumentZeroOrMore)?.Length > 0;
            bool hasAllNodes = commandResult.GetValue(allNodesOption);
            EnforceZeroNodenamesWhenAll(hasNodenames, hasAllNodes, commandResult);
        };

        public static Action<CommandResult> EnforceZeroNodenamesOptionWhenAll(Option<string[]> nodenamesOption, Option<bool> allNodesOption) => commandResult =>
        {
            bool hasNodenames = commandResult.GetResult(nodenamesOption) is not null;
            bool hasAllNodes = commandResult.GetValue(allNodesOption);
            EnforceZeroNodenamesWhenAll(hasNodenames, hasAllNodes, commandResult);
        };

        private static void EnforceZeroNodenamesWhenAll(bool hasNodenames, bool hasAllNodes, CommandResult commandResult)
        {
            if (!hasNodenames && !hasAllNodes)
            {
                commandResult.AddError("Please either specify target nodes, or use `--all-nodes` to target all nodes.");
            }
            else if (hasNodenames && hasAllNodes)
            {
                commandResult.AddError("You can't specify target nodes when targeting all nodes with `--all-nodes`.");
            }
        }

        public static Action<CommandResult> EnforceZeroGroupsArgumentWhenAll(Argument<string[]> groupsArgumentZeroOrMore, Option<bool> allGroupsOption) => commandResult =>
        {
            bool hasGroups = commandResult.GetValue(groupsArgumentZeroOrMore)?.Length > 0;
            bool hasAllGroups = commandResult.GetValue(allGroupsOption);
            EnforceZeroGroupsWhenAll(hasGroups, hasAllGroups, commandResult);
        };

        public static Action<CommandResult> EnforceZeroGroupsOptionWhenAll(Option<string[]> groupsOption, Option<bool> allGroupsOption) => commandResult =>
        {
            bool hasGroups = commandResult.GetResult(groupsOption) is not null;
            bool hasAllGroups = commandResult.GetValue(allGroupsOption);
            EnforceZeroGroupsWhenAll(hasGroups, hasAllGroups, commandResult);
        };

        private static void EnforceZeroGroupsWhenAll(bool hasGroups, bool hasAllGroups, CommandResult commandResult)
        {
            if (!hasGroups && !hasAllGroups)
            {
                commandResult.AddError("Please either specify target groups, or use `--all-groups` to target all groups.");
            }
            else if (hasGroups && hasAllGroups)
            {
                commandResult.AddError("You can't specify target groups when targeting all users with `--all-groups`.");
            }
        }

        public static Action<CommandResult> ValidateOwnerOptions(Option<string?> ownerOption, Option<bool> unsetOwnerOption) => commandResult =>
        {
            bool setOwner = commandResult.GetResult(ownerOption) is not null;
            bool unsetOwner = commandResult.GetValue(unsetOwnerOption);

            if (setOwner && unsetOwner)
            {
                commandResult.AddError("You can't set and unset owner at the same time.");
            }
        };
    }
}
