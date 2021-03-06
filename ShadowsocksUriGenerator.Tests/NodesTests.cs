using System.Linq;
using Xunit;

namespace ShadowsocksUriGenerator.Tests
{
    public class NodesTests
    {
        [Theory]
        [InlineData(
            new string[] { "A", "B", "C", "D", "E", "F", "G", },
            new string[] { "A", "B", "C", "D", "E", "F", "G", },
            new string[] { "A", "C" },
            new string[] { "B", "D", "E", "F", "G", })]
        [InlineData(
            new string[] { "A", "B", "C", "A", "B", "F", "G", },
            new string[] { "A", "B", "C", "F", "G", },
            new string[] { "A", "H" },
            new string[] { "B", "C", "F", "G", })]
        public void Add_Remove_Groups(string[] groupsToAdd, string[] expectedAddedGroups, string[] groupsToRemove, string[] expectedRemainingGroups)
        {
            var nodes = new Nodes();

            var addedGroups = nodes.AddGroups(groupsToAdd).ToArray();
            nodes.RemoveGroups(groupsToRemove);
            var remainingGroups = nodes.Groups.Select(x => x.Key).ToArray();

            Assert.Equal(expectedAddedGroups, addedGroups);
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
            var nodes = new Nodes();
            nodes.AddGroups(groupsToAdd);
            var count = nodes.Groups.Count;
            var oldNameExists = nodes.Groups.TryGetValue(oldName, out var group);

            var result = nodes.RenameGroup(oldName, newName);

            Assert.Equal(expectedResult, result);
            Assert.Equal(count, nodes.Groups.Count);
            // Verify Group object
            if (oldNameExists)
            {
                var currentName = result == 0 ? newName : oldName;
                Assert.Equal(group, nodes.Groups[currentName]);
            }
        }

        [Fact]
        public void Add_Remove_Node_ReturnsResult()
        {
            var nodes = new Nodes();
            var groupsToAdd = new string[] { "A", "B", "C", "D", "E", "F", "G", };
            nodes.AddGroups(groupsToAdd);

            // Add
            var successAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", "443");
            var successAddWithPlugin = nodes.AddNodeToGroup("B", "MyNode1", "github.com", "443", "v2ray-plugin", "server;tls;host=github.com");
            var duplicateAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", "443");
            var badGroupAdd = nodes.AddNodeToGroup("H", "MyNode0", "github.com", "443");
            var badPortAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", "https");

            Assert.Equal(0, successAdd);
            Assert.Equal(0, successAddWithPlugin);
            Assert.Equal(-1, duplicateAdd);
            Assert.Equal(-1, badGroupAdd);
            Assert.Equal(-1, badPortAdd);

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
            Assert.Equal("server;tls;host=github.com", addedNodeInB.PluginOpts);

            // Remove
            var successRemoval = nodes.RemoveNodesFromGroup("A", new string[] { "MyNode0" });
            var nonExistingGroupRemoval = nodes.RemoveNodesFromGroup("H", new string[] { "MyNode0" });

            Assert.Equal(0, successRemoval);
            Assert.Equal(-1, nonExistingGroupRemoval);
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
            var nodes = new Nodes();
            nodes.AddGroups(new string[] { addToGroup, });
            foreach (var nodeName in nodesToAdd)
                nodes.AddNodeToGroup(addToGroup, nodeName, "github.com", 443);
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
            var nodes = new Nodes();
            nodes.AddGroups(new string[] { "MyGroup", "MyGroupWithPlugin" });
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", "443");
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", "443", "v2ray-plugin", "server;tls;host=github.com");
            var users = new Users();
            users.AddUsers(new string[] { "root" });
            users.AddCredentialToUser("root", "MyGroup", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTp5bWdoaVIjNzVUTnFwYQ");
            users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var user = users.UserDict.First();

            // Initial status: all activated
            var userOnlineConfigDict = OnlineConfig.GenerateForUser(user, nodes, settings);
            var userSsLinks = user.Value.GetSSUris(nodes);

            Assert.Equal(2, userOnlineConfigDict.First().Value.Servers.Count);
            Assert.Equal(2, userSsLinks.Count);

            // Deactivate first node
            nodes.Groups["MyGroup"].NodeDict["MyNode"].Deactivated = true;
            userOnlineConfigDict = OnlineConfig.GenerateForUser(user, nodes, settings);
            userSsLinks = user.Value.GetSSUris(nodes);

            Assert.Single(userOnlineConfigDict.First().Value.Servers);
            Assert.Single(userSsLinks);

            // Reactivate first node and deactivate second node
            nodes.Groups["MyGroup"].NodeDict["MyNode"].Deactivated = false;
            nodes.Groups["MyGroupWithPlugin"].NodeDict["MyNodeWithPlugin"].Deactivated = true;
            userOnlineConfigDict = OnlineConfig.GenerateForUser(user, nodes, settings);
            userSsLinks = user.Value.GetSSUris(nodes);

            Assert.Single(userOnlineConfigDict.First().Value.Servers);
            Assert.Single(userSsLinks);
        }
    }
}
