using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.OnlineConfig;
using System;
using System.Linq;
using Xunit;

namespace ShadowsocksUriGenerator.Tests
{
    public class NodesTests
    {
        [Theory]
        [InlineData(
            new string[] { "A", "B", "C", "D", "E", "F", "G", },
            new int[] { 0, 0, 0, 0, 0, 0, 0, },
            new string[] { "A", "C" },
            new bool[] { true, true, },
            new string[] { "B", "D", "E", "F", "G", })]
        [InlineData(
            new string[] { "A", "B", "C", "A", "B", "F", "G", },
            new int[] { 0, 0, 0, 1, 1, 0, 0, },
            new string[] { "A", "H" },
            new bool[] { true, false, },
            new string[] { "B", "C", "F", "G", })]
        public void Add_Remove_Groups(string[] groupsToAdd, int[] expectedAddResults, string[] groupsToRemove, bool[] expectedRemovalResults, string[] expectedRemainingGroups)
        {
            using var nodes = new Nodes();

            var addResults = new int[groupsToAdd.Length];
            for (var i = 0; i < groupsToAdd.Length; i++)
                addResults[i] = nodes.AddGroup(groupsToAdd[i]);
            var removalResults = new bool[groupsToRemove.Length];
            for (var i = 0; i < groupsToRemove.Length; i++)
                removalResults[i] = nodes.RemoveGroup(groupsToRemove[i]);
            var remainingGroups = nodes.Groups.Select(x => x.Key).ToArray();

            Assert.Equal(expectedAddResults, addResults);
            Assert.Equal(expectedRemovalResults, removalResults);
            Assert.Equal(expectedRemainingGroups, remainingGroups);
        }

        [Theory]
        [InlineData(new string[] { "A", }, "A", "B", 0)]
        [InlineData(new string[] { "B", }, "B", "C", 0)]
        [InlineData(new string[] { "A", }, "B", "C", -1)]
        [InlineData(new string[] { "C", }, "B", "D", -1)]
        [InlineData(new string[] { "A", }, "A", "A", -2)]
        [InlineData(new string[] { "A", "B", }, "B", "A", -2)]
        [InlineData(new string[] { "A", "B", }, "A", "B", -2)]
        public void Rename_Group_ReturnsResult(string[] groupsToAdd, string oldName, string newName, int expectedResult)
        {
            using var nodes = new Nodes();
            foreach (var group in groupsToAdd)
                nodes.AddGroup(group);
            var count = nodes.Groups.Count;
            var oldNameExists = nodes.Groups.TryGetValue(oldName, out var targetGroup);

            var result = nodes.RenameGroup(oldName, newName);

            Assert.Equal(expectedResult, result);
            Assert.Equal(count, nodes.Groups.Count);
            // Verify Group object
            if (oldNameExists)
            {
                var currentName = result == 0 ? newName : oldName;
                Assert.Equal(targetGroup, nodes.Groups[currentName]);
            }
        }

        [Fact]
        public void Add_Remove_Node_ReturnsResult()
        {
            using var nodes = new Nodes();
            nodes.AddGroup("A");
            nodes.AddGroup("B");
            nodes.AddGroup("C");

            // Add
            var successAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", 443, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>());
            var successAddWithPlugin = nodes.AddNodeToGroup("B", "MyNode1", "github.com", 443, "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv", null, Array.Empty<string>(), Array.Empty<string>());
            var successAddWithOwnerAndTags = nodes.AddNodeToGroup("C", "MyNode2", "github.com", 443, null, null, null, null, "a2865866-5dc8-4eae-9772-692d10c274df", new string[] { "direct", "US", }, Array.Empty<string>());
            var duplicateAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", 443, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>());
            var badGroupAdd = nodes.AddNodeToGroup("D", "MyNode0", "github.com", 443, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>());

            Assert.Equal(0, successAdd);
            Assert.Equal(0, successAddWithPlugin);
            Assert.Equal(0, successAddWithOwnerAndTags);
            Assert.Equal(-1, duplicateAdd);
            Assert.Equal(-2, badGroupAdd);

            Assert.True(nodes.Groups.ContainsKey("A"));
            Assert.True(nodes.Groups["A"].NodeDict.ContainsKey("MyNode0"));
            var addedNodeInA = nodes.Groups["A"].NodeDict["MyNode0"];
            Assert.Equal("github.com", addedNodeInA.Host);
            Assert.Equal(443, addedNodeInA.Port);

            Assert.True(nodes.Groups.ContainsKey("B"));
            Assert.True(nodes.Groups["B"].NodeDict.ContainsKey("MyNode1"));
            var addedNodeInB = nodes.Groups["B"].NodeDict["MyNode1"];
            Assert.Equal("github.com", addedNodeInB.Host);
            Assert.Equal(443, addedNodeInB.Port);
            Assert.Equal("v2ray-plugin", addedNodeInB.Plugin);
            Assert.Equal("1.0", addedNodeInB.PluginVersion);
            Assert.Equal("server;tls;host=github.com", addedNodeInB.PluginOpts);
            Assert.Equal("-vvvvvv", addedNodeInB.PluginArguments);

            Assert.True(nodes.Groups.ContainsKey("C"));
            Assert.True(nodes.Groups["C"].NodeDict.ContainsKey("MyNode2"));
            var addedNodeInC = nodes.Groups["C"].NodeDict["MyNode2"];
            Assert.Equal("github.com", addedNodeInC.Host);
            Assert.Equal(443, addedNodeInC.Port);
            Assert.Equal("a2865866-5dc8-4eae-9772-692d10c274df", addedNodeInC.OwnerUuid);
            Assert.Equal(new string[] { "direct", "US", }, addedNodeInC.Tags);

            // Remove
            var successRemoval = nodes.RemoveNodeFromGroup("A", "MyNode0");
            var nonExistingNodeRemoval = nodes.RemoveNodeFromGroup("A", "MyNode1");
            var nonExistingGroupRemoval = nodes.RemoveNodeFromGroup("D", "MyNode0");

            Assert.Equal(0, successRemoval);
            Assert.Equal(-1, nonExistingNodeRemoval);
            Assert.Equal(-2, nonExistingGroupRemoval);
            Assert.True(nodes.Groups.ContainsKey("A"));
            Assert.Empty(nodes.Groups["A"].NodeDict);
        }

        [Theory]
        [InlineData("MyGroup", new string[] { "A", }, "MyGroup", "A", "B", 0)]
        [InlineData("MyGroup", new string[] { "B", }, "MyGroup", "B", "C", 0)]
        [InlineData("MyGroup", new string[] { "A", }, "MyGroup", "B", "C", -1)]
        [InlineData("MyGroup", new string[] { "C", }, "MyGroup", "B", "D", -1)]
        [InlineData("MyGroup", new string[] { "A", }, "MyGroup", "A", "A", -2)]
        [InlineData("MyGroup", new string[] { "A", "B", }, "MyGroup", "B", "A", -2)]
        [InlineData("MyGroup", new string[] { "A", "B", }, "MyGroup", "A", "B", -2)]
        [InlineData("MyGroup", new string[] { "A", }, "MyGroupWithPlugin", "A", "B", -3)]
        [InlineData("MyGroup", new string[] { "A", }, "My", "A", "B", -3)]
        public void Rename_Node_ReturnsResult(string addToGroup, string[] nodesToAdd, string group, string oldName, string newName, int expectedResult)
        {
            using var nodes = new Nodes();
            nodes.AddGroup(addToGroup);
            foreach (var nodeName in nodesToAdd)
                nodes.AddNodeToGroup(addToGroup, nodeName, "github.com", 443, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>());
            var nodeDict = nodes.Groups[addToGroup].NodeDict;
            var count = nodeDict.Count;
            var oldNameExists = nodeDict.TryGetValue(oldName, out var node);

            var result = nodes.RenameNodeInGroup(group, oldName, newName);

            Assert.Equal(expectedResult, result);
            Assert.Equal(count, nodeDict.Count);
            // Verify Node object
            if (oldNameExists)
            {
                var currentName = result == 0 ? newName : oldName;
                Assert.Equal(node, nodeDict[currentName]);
            }
        }

        [Fact]
        public void Activate_Deactivate_Nodes_OnlineConfig_SSLinks()
        {
            var settings = new Settings();
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443, null, null, null, null, null, Array.Empty<string>(), Array.Empty<string>());
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com", null, null, null, Array.Empty<string>(), Array.Empty<string>());
            var users = new Users();
            users.AddUser("root");
            users.AddCredentialToUser("root", "MyGroup", "chacha20-ietf-poly1305", "ymghiR#75TNqpa");
            users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var user = users.UserDict.First();

            // Initial status: all activated
            var userOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            var userSsLinks = user.Value.GetSSUris(users, nodes);

            Assert.Equal(2, userOnlineConfigDict.First().Value.Servers.Count());
            Assert.Equal(2, userSsLinks.Count());

            // Deactivate first node
            nodes.Groups["MyGroup"].NodeDict["MyNode"].Deactivated = true;
            userOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            userSsLinks = user.Value.GetSSUris(users, nodes);

            Assert.Single(userOnlineConfigDict.First().Value.Servers);
            Assert.Single(userSsLinks);

            // Reactivate first node and deactivate second node
            nodes.Groups["MyGroup"].NodeDict["MyNode"].Deactivated = false;
            nodes.Groups["MyGroupWithPlugin"].NodeDict["MyNodeWithPlugin"].Deactivated = true;
            userOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            userSsLinks = user.Value.GetSSUris(users, nodes);

            Assert.Single(userOnlineConfigDict.First().Value.Servers);
            Assert.Single(userSsLinks);
        }
    }
}
