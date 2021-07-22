using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShadowsocksUriGenerator.Tests
{
    public class UsersTests
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
        public void Add_Remove_Users(string[] usersToAdd, int[] expectedAddResults, string[] usersToRemove, bool[] expectedRemovalResults, string[] expectedRemainingUsers)
        {
            var users = new Users();

            var addResults = new int[usersToAdd.Length];
            for (var i = 0; i < usersToAdd.Length; i++)
                addResults[i] = users.AddUser(usersToAdd[i]);
            var removalResults = new bool[usersToRemove.Length];
            for (var i = 0; i < usersToRemove.Length; i++)
                removalResults[i] = users.RemoveUser(usersToRemove[i]);
            var remainingUsers = users.UserDict.Select(x => x.Key).ToArray();

            Assert.Equal(expectedAddResults, addResults);
            Assert.Equal(expectedRemovalResults, removalResults);
            Assert.Equal(expectedRemainingUsers, remainingUsers);
        }

        [Theory]
        [InlineData(new string[] { "A", }, "A", "B", null)]
        [InlineData(new string[] { "B", }, "B", "C", null)]
        [InlineData(new string[] { "A", }, "B", "C", "Error: user B doesn't exist.")]
        [InlineData(new string[] { "C", }, "B", "D", "Error: user B doesn't exist.")]
        [InlineData(new string[] { "A", }, "A", "A", "Error: the new username A is already used. Please choose another username.")]
        [InlineData(new string[] { "A", "B", }, "B", "A", "Error: the new username A is already used. Please choose another username.")]
        [InlineData(new string[] { "A", "B", }, "A", "B", "Error: the new username B is already used. Please choose another username.")]
        public async Task Rename_User_ReturnsResult(string[] usersToAdd, string oldName, string newName, string? expectedResult)
        {
            var users = new Users();
            foreach (var username in usersToAdd)
                users.AddUser(username);
            using var nodes = new Nodes();
            var count = users.UserDict.Count;
            var oldNameExists = users.UserDict.TryGetValue(oldName, out var user);

            var result = await users.RenameUser(oldName, newName, nodes);

            Assert.Equal(expectedResult, result);
            Assert.Equal(count, users.UserDict.Count);

            // Verify User object
            if (oldNameExists)
            {
                var currentName = result is null ? newName : oldName;
                Assert.Equal(user, users.UserDict[currentName]);
            }
        }

        [Theory]
        [InlineData("chacha20-ietf-poly1305", "kf!V!TFzgeNd93GE", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTprZiFWIVRGemdlTmQ5M0dF")]
        [InlineData("aes-256-gcm", "ymghiR#75TNqpa", "YWVzLTI1Ni1nY206eW1naGlSIzc1VE5xcGE")]
        [InlineData("aes-128-gcm", "tK*sk!9N8@86:UVm", "YWVzLTEyOC1nY206dEsqc2shOU44QDg2OlVWbQ")]
        public void New_Credential_MethodPassword(string method, string password, string expectedUserinfoBase64url)
        {
            var credential = new MemberInfo(method, password);

            Assert.Equal(expectedUserinfoBase64url, credential.UserinfoBase64url);
        }

        [Theory]
        [InlineData("dXNlcmluZm9CYXNlNjR1cmw", false, "", "")] // missing :
        [InlineData("Og", false, "", "")] // only has :
        [InlineData("Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTo", false, "chacha20-ietf-poly1305", "")] // missing password
        [InlineData("OjYlbThEOWFNQjViQSVhNCU", false, "", "6%m8D9aMB5bA%a4%")] // missing method
        [InlineData("Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTo2JW04RDlhTUI1YkElYTQl", true, "chacha20-ietf-poly1305", "6%m8D9aMB5bA%a4%")]
        [InlineData("YWVzLTI1Ni1nY206YnBOZ2sqSjNrYUFZeXhIRQ", true, "aes-256-gcm", "bpNgk*J3kaAYyxHE")]
        [InlineData("YWVzLTEyOC1nY206dkFBbiY4a1I6JGlBRTQ", true, "aes-128-gcm", "vAAn&8kR:$iAE4")]
        public void New_Credential_UserinfoBase64url(string userinfoBase64url, bool expectedParseResult, string expectedMethod, string expectedPassword)
        {
            var result = MemberInfo.TryParseFromUserinfoBase64url(userinfoBase64url, out var method, out var password);

            Assert.Equal(expectedParseResult, result);
            if (result)
            {
                Assert.Equal(expectedMethod, method);
                Assert.Equal(expectedPassword, password);
            }
        }

        [Fact]
        public void Add_Update_Remove_Group_ReturnsResult()
        {
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com");
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            Assert.True(users.UserDict.ContainsKey("root"));
            Assert.True(users.UserDict.ContainsKey("http"));

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

            Assert.Equal(0, successAdd);
            Assert.Equal(0, anotherSuccessAdd);
            Assert.Equal(0, yetAnotherSuccessAdd);
            Assert.Equal(1, duplicateAdd);
            Assert.Equal(-1, badUserAdd);

            Assert.True(rootUserMemberships.ContainsKey("MyGroup"));
            Assert.True(rootUserMemberships.ContainsKey("MyGroupWithPlugin"));
            Assert.True(httpUserMemberships.ContainsKey("MyGroup"));

            // Update
            users.UpdateCredentialGroupsForAllUsers("MyGroup", "MyGroupNew");

            Assert.False(rootUserMemberships.ContainsKey("MyGroup"));
            Assert.False(httpUserMemberships.ContainsKey("MyGroup"));
            Assert.True(rootUserMemberships.ContainsKey("MyGroupNew"));
            Assert.True(rootUserMemberships.ContainsKey("MyGroupWithPlugin"));
            Assert.True(httpUserMemberships.ContainsKey("MyGroupNew"));
            Assert.Equal(rootMyGroupMembership, rootUserMemberships["MyGroupNew"]);
            Assert.Equal(rootMyGroupWithPluginMembership, rootUserMemberships["MyGroupWithPlugin"]);
            Assert.Equal(httpMyGroupMembership, httpUserMemberships["MyGroupNew"]);

            // Remove
            var successRemoval = users.RemoveUserFromGroup("root", "MyGroupWithPlugin");
            var nonExistingUserRemoval = users.RemoveUserFromGroup("nobody", "MyGroup");
            var nonExistingGroupRemoval = users.RemoveUserFromGroup("root", "MyGroupWithoutPlugin");

            Assert.Equal(0, successRemoval);
            Assert.Equal(-2, nonExistingUserRemoval);
            Assert.Equal(1, nonExistingGroupRemoval);

            Assert.Single(rootUserMemberships);
        }

        [Fact]
        public void Add_Remove_Credential_ReturnsResult()
        {
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com");
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            Assert.True(users.UserDict.ContainsKey("root"));
            Assert.True(users.UserDict.ContainsKey("http"));

            // Add
            var successAdd = users.AddCredentialToUser("root", "MyGroup", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTp5bWdoaVIjNzVUTnFwYQ");
            var anotherSuccessAdd = users.AddCredentialToUser("http", "MyGroup", "YWVzLTEyOC1nY206dEsqc2shOU44QDg2OlVWbWM");
            var yetAnotherSuccessAdd = users.AddCredentialToUser("root", "MyGroupWithPlugin", "YWVzLTEyOC1nY206dkFBbiY4a1I6JGlBRTQ0JA");
            var duplicateAdd = users.AddCredentialToUser("root", "MyGroup", "aes-256-gcm", "wLhN2STZ");
            var badUserAdd = users.AddCredentialToUser("nobody", "MyGroup", "aes-256-gcm", "wLhN2STZ");
            var badUserinfoAdd = users.AddCredentialToUser("http", "MyGroupWithPlugin", "PR6Lf9UR22C5LNBhzEcpsWxd6WpsaeSs");

            var rootUserMemberships = users.UserDict["root"].Memberships;
            var httpUserMemberships = users.UserDict["http"].Memberships;

            Assert.Equal(0, successAdd);
            Assert.Equal(0, anotherSuccessAdd);
            Assert.Equal(0, yetAnotherSuccessAdd);
            Assert.Equal(2, duplicateAdd);
            Assert.Equal(-1, badUserAdd);
            Assert.Equal(-2, badUserinfoAdd);

            Assert.True(rootUserMemberships.ContainsKey("MyGroup"));
            Assert.True(rootUserMemberships.ContainsKey("MyGroupWithPlugin"));
            Assert.True(httpUserMemberships.ContainsKey("MyGroup"));

            Assert.True(rootUserMemberships["MyGroup"].HasCredential);
            Assert.True(rootUserMemberships["MyGroupWithPlugin"].HasCredential);
            Assert.True(httpUserMemberships["MyGroup"].HasCredential);

            // Remove
            var successRemoval = users.RemoveCredentialFromUser("root", "MyGroupWithPlugin");
            var nonExistingUserRemoval = users.RemoveCredentialFromUser("nobody", "MyGroupNew");
            var nonExistingGroupRemoval = users.RemoveCredentialFromUser("root", "MyGroupWithoutPlugin");

            Assert.Equal(0, successRemoval);
            Assert.Equal(-2, nonExistingUserRemoval);
            Assert.Equal(-1, nonExistingGroupRemoval);

            Assert.True(rootUserMemberships["MyGroup"].HasCredential);
            Assert.True(httpUserMemberships["MyGroup"].HasCredential);
            Assert.False(rootUserMemberships["MyGroupWithPlugin"].HasCredential);

            // Remove from all
            users.RemoveCredentialsFromAllUsers(new string[] { "MyGroup" });

            Assert.False(rootUserMemberships["MyGroup"].HasCredential);
            Assert.False(httpUserMemberships["MyGroup"].HasCredential);
        }
    }
}
