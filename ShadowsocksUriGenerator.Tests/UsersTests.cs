using ShadowsocksUriGenerator.Data;

namespace ShadowsocksUriGenerator.Tests
{
    public class UsersTests
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
        public async Task Add_Remove_Users(string[] usersToAdd, int[] expectedAddResults, string[] usersToRemove, bool[] expectedRemovalResults, string[] expectedRemainingUsers)
        {
            var users = new Users();

            var addResults = new int[usersToAdd.Length];
            for (var i = 0; i < usersToAdd.Length; i++)
                addResults[i] = users.AddUser(usersToAdd[i]);
            var removalResults = new bool[usersToRemove.Length];
            for (var i = 0; i < usersToRemove.Length; i++)
                removalResults[i] = users.RemoveUser(usersToRemove[i]);
            var remainingUsers = users.UserDict.Select(x => x.Key).ToArray();

            await Assert.That(addResults).IsEquivalentTo(expectedAddResults);
            await Assert.That(removalResults).IsEquivalentTo(expectedRemovalResults);
            await Assert.That(remainingUsers).IsEquivalentTo(expectedRemainingUsers);
        }

        [Test]
        [Arguments(new string[] { "A", }, "A", "B", null)]
        [Arguments(new string[] { "B", }, "B", "C", null)]
        [Arguments(new string[] { "A", }, "B", "C", "Error: user B doesn't exist.")]
        [Arguments(new string[] { "C", }, "B", "D", "Error: user B doesn't exist.")]
        [Arguments(new string[] { "A", }, "A", "A", "Error: the new username A is already used. Please choose another username.")]
        [Arguments(new string[] { "A", "B", }, "B", "A", "Error: the new username A is already used. Please choose another username.")]
        [Arguments(new string[] { "A", "B", }, "A", "B", "Error: the new username B is already used. Please choose another username.")]
        public async Task Rename_User_ReturnsResult(string[] usersToAdd, string oldName, string newName, string? expectedResult)
        {
            var users = new Users();
            foreach (var username in usersToAdd)
                users.AddUser(username);
            using var nodes = new Nodes();
            var count = users.UserDict.Count;
            var oldNameExists = users.UserDict.TryGetValue(oldName, out var user);

            var result = await users.RenameUser(oldName, newName, nodes);

            await Assert.That(result).IsEqualTo(expectedResult);
            await Assert.That(users.UserDict.Count).IsEqualTo(count);

            // Verify User object
            if (oldNameExists)
            {
                var currentName = result is null ? newName : oldName;
                await Assert.That(users.UserDict[currentName]).IsEqualTo(user);
            }
        }

        [Test]
        public async Task Add_Update_Remove_Group_ReturnsResult()
        {
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443, null, null, null, null, null, [], []);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com", null, null, null, [], []);
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            await Assert.That(users.UserDict.ContainsKey("root")).IsTrue();
            await Assert.That(users.UserDict.ContainsKey("http")).IsTrue();

            // Add
            var successAdd = users.AddUserToGroup("root", "MyGroup");
            var anotherSuccessAdd = users.AddUserToGroup("http", "MyGroup");
            var yetAnotherSuccessAdd = users.AddUserToGroup("root", "MyGroupWithPlugin");
            var duplicateAdd = users.AddUserToGroup("root", "MyGroup");
            var badUserAdd = users.AddUserToGroup("nobody", "MyGroup");

            var rootUserMemberships = users.UserDict["root"].Memberships;
            var httpUserMemberships = users.UserDict["http"].Memberships;
            var rootMyGroupMembership = rootUserMemberships["MyGroup"];
            var rootMyGroupWithPluginMembership = rootUserMemberships["MyGroupWithPlugin"];
            var httpMyGroupMembership = httpUserMemberships["MyGroup"];

            await Assert.That(successAdd).IsEqualTo(0);
            await Assert.That(anotherSuccessAdd).IsEqualTo(0);
            await Assert.That(yetAnotherSuccessAdd).IsEqualTo(0);
            await Assert.That(duplicateAdd).IsEqualTo(1);
            await Assert.That(badUserAdd).IsEqualTo(-1);

            await Assert.That(rootUserMemberships.ContainsKey("MyGroup")).IsTrue();
            await Assert.That(rootUserMemberships.ContainsKey("MyGroupWithPlugin")).IsTrue();
            await Assert.That(httpUserMemberships.ContainsKey("MyGroup")).IsTrue();

            // Update
            users.UpdateCredentialGroupsForAllUsers("MyGroup", "MyGroupNew");

            await Assert.That(rootUserMemberships.ContainsKey("MyGroup")).IsFalse();
            await Assert.That(httpUserMemberships.ContainsKey("MyGroup")).IsFalse();
            await Assert.That(rootUserMemberships.ContainsKey("MyGroupNew")).IsTrue();
            await Assert.That(rootUserMemberships.ContainsKey("MyGroupWithPlugin")).IsTrue();
            await Assert.That(httpUserMemberships.ContainsKey("MyGroupNew")).IsTrue();
            await Assert.That(rootUserMemberships["MyGroupNew"]).IsEqualTo(rootMyGroupMembership);
            await Assert.That(rootUserMemberships["MyGroupWithPlugin"]).IsEqualTo(rootMyGroupWithPluginMembership);
            await Assert.That(httpUserMemberships["MyGroupNew"]).IsEqualTo(httpMyGroupMembership);

            // Remove
            var successRemoval = users.RemoveUserFromGroup("root", "MyGroupWithPlugin");
            var nonExistingUserRemoval = users.RemoveUserFromGroup("nobody", "MyGroup");
            var nonExistingGroupRemoval = users.RemoveUserFromGroup("root", "MyGroupWithoutPlugin");

            await Assert.That(successRemoval).IsEqualTo(0);
            await Assert.That(nonExistingUserRemoval).IsEqualTo(-2);
            await Assert.That(nonExistingGroupRemoval).IsEqualTo(1);

            await Assert.That(rootUserMemberships).HasSingleItem();
        }

        [Test]
        public async Task Add_Remove_Credential_ReturnsResult()
        {
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443, null, null, null, null, null, [], []);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com", null, null, null, [], []);
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            await Assert.That(users.UserDict.ContainsKey("root")).IsTrue();
            await Assert.That(users.UserDict.ContainsKey("http")).IsTrue();

            // Add
            var successAdd = users.AddCredentialToUser("root", "MyGroup", "chacha20-ietf-poly1305", "ymghiR#75TNqpa");
            var anotherSuccessAdd = users.AddCredentialToUser("http", "MyGroup", "aes-128-gcm", "tK*sk!9N8@86:UVmc");
            var yetAnotherSuccessAdd = users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-128-gcm", "vAAn&8kR:$iAE44$");
            var duplicateAdd = users.AddCredentialToUser("root", "MyGroup", "aes-256-gcm", "wLhN2STZ");
            var badUserAdd = users.AddCredentialToUser("nobody", "MyGroup", "aes-256-gcm", "wLhN2STZ");

            var rootUserMemberships = users.UserDict["root"].Memberships;
            var httpUserMemberships = users.UserDict["http"].Memberships;

            await Assert.That(successAdd).IsEqualTo(0);
            await Assert.That(anotherSuccessAdd).IsEqualTo(0);
            await Assert.That(yetAnotherSuccessAdd).IsEqualTo(0);
            await Assert.That(duplicateAdd).IsEqualTo(2);
            await Assert.That(badUserAdd).IsEqualTo(-1);

            await Assert.That(rootUserMemberships.ContainsKey("MyGroup")).IsTrue();
            await Assert.That(rootUserMemberships.ContainsKey("MyGroupWithPlugin")).IsTrue();
            await Assert.That(httpUserMemberships.ContainsKey("MyGroup")).IsTrue();

            await Assert.That(rootUserMemberships["MyGroup"].HasCredential).IsTrue();
            await Assert.That(rootUserMemberships["MyGroupWithPlugin"].HasCredential).IsTrue();
            await Assert.That(httpUserMemberships["MyGroup"].HasCredential).IsTrue();

            // Remove
            var successRemoval = users.RemoveCredentialFromUser("root", "MyGroupWithPlugin");
            var nonExistingUserRemoval = users.RemoveCredentialFromUser("nobody", "MyGroupNew");
            var nonExistingGroupRemoval = users.RemoveCredentialFromUser("root", "MyGroupWithoutPlugin");

            await Assert.That(successRemoval).IsEqualTo(0);
            await Assert.That(nonExistingUserRemoval).IsEqualTo(-2);
            await Assert.That(nonExistingGroupRemoval).IsEqualTo(-1);

            await Assert.That(rootUserMemberships["MyGroup"].HasCredential).IsTrue();
            await Assert.That(httpUserMemberships["MyGroup"].HasCredential).IsTrue();
            await Assert.That(rootUserMemberships["MyGroupWithPlugin"].HasCredential).IsFalse();

            // Remove from all
            users.RemoveCredentialsFromAllUsers(["MyGroup"]);

            await Assert.That(rootUserMemberships["MyGroup"].HasCredential).IsFalse();
            await Assert.That(httpUserMemberships["MyGroup"].HasCredential).IsFalse();
        }
    }
}