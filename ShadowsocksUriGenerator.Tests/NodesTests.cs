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
    }
}
