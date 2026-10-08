using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.OnlineConfig;

namespace ShadowsocksUriGenerator.Tests
{
    public class NodesTests
    {
        [Test]
        [Arguments(
            new string[] { "A", "B", "C", "D", "E", "F", "G", },
            new int[] { 0, 0, 0, 0, 0, 0, 0, },
            new string[] { "A", "C" },
            new bool[] { true, true, },
            new string[] { "B", "D", "E", "F", "G", })]
        [Arguments(
            new string[] { "A", "B", "C", "A", "B", "F", "G", },
            new int[] { 0, 0, 0, 1, 1, 0, 0, },
            new string[] { "A", "H" },
            new bool[] { true, false, },
            new string[] { "B", "C", "F", "G", })]
        public async Task Add_Remove_Groups(string[] groupsToAdd, int[] expectedAddResults, string[] groupsToRemove, bool[] expectedRemovalResults, string[] expectedRemainingGroups)
        {
            using var nodes = new Nodes();

            var addResults = new int[groupsToAdd.Length];
            for (var i = 0; i < groupsToAdd.Length; i++)
                addResults[i] = nodes.AddGroup(groupsToAdd[i]);
            var removalResults = new bool[groupsToRemove.Length];
            for (var i = 0; i < groupsToRemove.Length; i++)
                removalResults[i] = nodes.RemoveGroup(groupsToRemove[i]);
            var remainingGroups = nodes.Groups.Select(x => x.Key).ToArray();

            await Assert.That(addResults).IsEquivalentTo(expectedAddResults);
            await Assert.That(removalResults).IsEquivalentTo(expectedRemovalResults);
            await Assert.That(remainingGroups).IsEquivalentTo(expectedRemainingGroups);
        }

        [Test]
        [Arguments(new string[] { "A", }, "A", "B", 0)]
        [Arguments(new string[] { "B", }, "B", "C", 0)]
        [Arguments(new string[] { "A", }, "B", "C", -1)]
        [Arguments(new string[] { "C", }, "B", "D", -1)]
        [Arguments(new string[] { "A", }, "A", "A", -2)]
        [Arguments(new string[] { "A", "B", }, "B", "A", -2)]
        [Arguments(new string[] { "A", "B", }, "A", "B", -2)]
        public async Task Rename_Group_ReturnsResult(string[] groupsToAdd, string oldName, string newName, int expectedResult)
        {
            using var nodes = new Nodes();
            foreach (var group in groupsToAdd)
                nodes.AddGroup(group);
            var count = nodes.Groups.Count;
            var oldNameExists = nodes.Groups.TryGetValue(oldName, out var targetGroup);

            var result = nodes.RenameGroup(oldName, newName);

            await Assert.That(result).IsEqualTo(expectedResult);
            await Assert.That(nodes.Groups.Count).IsEqualTo(count);
            // Verify Group object
            if (oldNameExists)
            {
                var currentName = result == 0 ? newName : oldName;
                await Assert.That(nodes.Groups[currentName]).IsEqualTo(targetGroup);
            }
        }

        [Test]
        public async Task Add_Remove_Node_ReturnsResult()
        {
            using var nodes = new Nodes();
            nodes.AddGroup("A");
            nodes.AddGroup("B");
            nodes.AddGroup("C");

            // Add
            var successAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", 443, null, null, null, null, null, [], []);
            var successAddWithPlugin = nodes.AddNodeToGroup("B", "MyNode1", "github.com", 443, "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv", null, [], []);
            var successAddWithOwnerAndTags = nodes.AddNodeToGroup("C", "MyNode2", "github.com", 443, null, null, null, null, "a2865866-5dc8-4eae-9772-692d10c274df", ["direct", "US",], []);
            var duplicateAdd = nodes.AddNodeToGroup("A", "MyNode0", "github.com", 443, null, null, null, null, null, [], []);
            var badGroupAdd = nodes.AddNodeToGroup("D", "MyNode0", "github.com", 443, null, null, null, null, null, [], []);

            await Assert.That(successAdd).IsEqualTo(0);
            await Assert.That(successAddWithPlugin).IsEqualTo(0);
            await Assert.That(successAddWithOwnerAndTags).IsEqualTo(0);
            await Assert.That(duplicateAdd).IsEqualTo(-1);
            await Assert.That(badGroupAdd).IsEqualTo(-2);

            await Assert.That(nodes.Groups.ContainsKey("A")).IsTrue();
            await Assert.That(nodes.Groups["A"].NodeDict.ContainsKey("MyNode0")).IsTrue();
            var addedNodeInA = nodes.Groups["A"].NodeDict["MyNode0"];
            await Assert.That(addedNodeInA.Host).IsEqualTo("github.com");
            await Assert.That(addedNodeInA.Port).IsEqualTo(443);

            await Assert.That(nodes.Groups.ContainsKey("B")).IsTrue();
            await Assert.That(nodes.Groups["B"].NodeDict.ContainsKey("MyNode1")).IsTrue();
            var addedNodeInB = nodes.Groups["B"].NodeDict["MyNode1"];
            await Assert.That(addedNodeInB.Host).IsEqualTo("github.com");
            await Assert.That(addedNodeInB.Port).IsEqualTo(443);
            await Assert.That(addedNodeInB.Plugin).IsEqualTo("v2ray-plugin");
            await Assert.That(addedNodeInB.PluginVersion).IsEqualTo("1.0");
            await Assert.That(addedNodeInB.PluginOpts).IsEqualTo("server;tls;host=github.com");
            await Assert.That(addedNodeInB.PluginArguments).IsEqualTo("-vvvvvv");

            await Assert.That(nodes.Groups.ContainsKey("C")).IsTrue();
            await Assert.That(nodes.Groups["C"].NodeDict.ContainsKey("MyNode2")).IsTrue();
            var addedNodeInC = nodes.Groups["C"].NodeDict["MyNode2"];
            await Assert.That(addedNodeInC.Host).IsEqualTo("github.com");
            await Assert.That(addedNodeInC.Port).IsEqualTo(443);
            await Assert.That(addedNodeInC.OwnerUuid).IsEqualTo("a2865866-5dc8-4eae-9772-692d10c274df");
            await Assert.That(addedNodeInC.Tags).IsEquivalentTo(["direct", "US",]);

            // Remove
            var successRemoval = nodes.RemoveNodeFromGroup("A", "MyNode0");
            var nonExistingNodeRemoval = nodes.RemoveNodeFromGroup("A", "MyNode1");
            var nonExistingGroupRemoval = nodes.RemoveNodeFromGroup("D", "MyNode0");

            await Assert.That(successRemoval).IsEqualTo(0);
            await Assert.That(nonExistingNodeRemoval).IsEqualTo(-1);
            await Assert.That(nonExistingGroupRemoval).IsEqualTo(-2);
            await Assert.That(nodes.Groups.ContainsKey("A")).IsTrue();
            await Assert.That(nodes.Groups["A"].NodeDict).IsEmpty();
        }

        [Test]
        [Arguments("MyGroup", new string[] { "A", }, "MyGroup", "A", "B", 0)]
        [Arguments("MyGroup", new string[] { "B", }, "MyGroup", "B", "C", 0)]
        [Arguments("MyGroup", new string[] { "A", }, "MyGroup", "B", "C", -1)]
        [Arguments("MyGroup", new string[] { "C", }, "MyGroup", "B", "D", -1)]
        [Arguments("MyGroup", new string[] { "A", }, "MyGroup", "A", "A", -2)]
        [Arguments("MyGroup", new string[] { "A", "B", }, "MyGroup", "B", "A", -2)]
        [Arguments("MyGroup", new string[] { "A", "B", }, "MyGroup", "A", "B", -2)]
        [Arguments("MyGroup", new string[] { "A", }, "MyGroupWithPlugin", "A", "B", -3)]
        [Arguments("MyGroup", new string[] { "A", }, "My", "A", "B", -3)]
        public async Task Rename_Node_ReturnsResult(string addToGroup, string[] nodesToAdd, string group, string oldName, string newName, int expectedResult)
        {
            using var nodes = new Nodes();
            nodes.AddGroup(addToGroup);
            foreach (var nodeName in nodesToAdd)
                nodes.AddNodeToGroup(addToGroup, nodeName, "github.com", 443, null, null, null, null, null, [], []);
            var nodeDict = nodes.Groups[addToGroup].NodeDict;
            var count = nodeDict.Count;
            var oldNameExists = nodeDict.TryGetValue(oldName, out var node);

            var result = nodes.RenameNodeInGroup(group, oldName, newName);

            await Assert.That(result).IsEqualTo(expectedResult);
            await Assert.That(nodeDict.Count).IsEqualTo(count);
            // Verify Node object
            if (oldNameExists)
            {
                var currentName = result == 0 ? newName : oldName;
                await Assert.That(nodeDict[currentName]).IsEqualTo(node);
            }
        }

        [Test]
        public async Task Activate_Deactivate_Nodes_OnlineConfig_SSLinks()
        {
            var settings = new Settings();
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443, null, null, null, null, null, [], []);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com", null, null, null, [], []);
            var users = new Users();
            users.AddUser("root");
            users.AddCredentialToUser("root", "MyGroup", "chacha20-ietf-poly1305", "ymghiR#75TNqpa");
            users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var user = users.UserDict.First();

            // Initial status: all activated
            var userOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            var userSsLinks = user.Value.GetSSUris(users, nodes);

            await Assert.That(userOnlineConfigDict.First().Value.Servers.Count()).IsEqualTo(2);
            await Assert.That(userSsLinks.Count()).IsEqualTo(2);

            // Deactivate first node
            nodes.Groups["MyGroup"].NodeDict["MyNode"].Deactivated = true;
            userOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            userSsLinks = user.Value.GetSSUris(users, nodes);

            await Assert.That(userOnlineConfigDict.First().Value.Servers).HasSingleItem();
            await Assert.That(userSsLinks).HasSingleItem();

            // Reactivate first node and deactivate second node
            nodes.Groups["MyGroup"].NodeDict["MyNode"].Deactivated = false;
            nodes.Groups["MyGroupWithPlugin"].NodeDict["MyNodeWithPlugin"].Deactivated = true;
            userOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            userSsLinks = user.Value.GetSSUris(users, nodes);

            await Assert.That(userOnlineConfigDict.First().Value.Servers).HasSingleItem();
            await Assert.That(userSsLinks).HasSingleItem();
        }
    }
}