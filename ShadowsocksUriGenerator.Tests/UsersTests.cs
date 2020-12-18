using System.Linq;
using Xunit;

namespace ShadowsocksUriGenerator.Tests
{
    public class UsersTests
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
        public void Add_Remove_Users(string[] usersToAdd, string[] expectedAddedUsers, string[] usersToRemove, string[] expectedRemainingUsers)
        {
            var users = new Users();

            var addedUsers = users.AddUsers(usersToAdd).ToArray();
            users.RemoveUsers(usersToRemove);
            var remainingUsers = users.UserDict.Select(x => x.Key).ToArray();

            Assert.Equal(expectedAddedUsers, addedUsers);
            Assert.Equal(expectedRemainingUsers, remainingUsers);
        }

        [Theory]
        [InlineData(new string[] { "A", }, "A", "B", 0)]
        [InlineData(new string[] { "B", }, "B", "C", 0)]
        [InlineData(new string[] { "A", }, "B", "C", -1)]
        [InlineData(new string[] { "C", }, "B", "D", -1)]
        [InlineData(new string[] { "A", }, "A", "A", -2)]
        [InlineData(new string[] { "A", "B", }, "B", "A", -2)]
        [InlineData(new string[] { "A", "B", }, "A", "B", -2)]
        public void Rename_User_ReturnsResult(string[] usersToAdd, string oldName, string newName, int expectedResult)
        {
            var users = new Users();
            users.AddUsers(usersToAdd);
            var count = users.UserDict.Count;
            var oldNameExists = users.UserDict.TryGetValue(oldName, out var user);

            var result = users.RenameUser(oldName, newName);

            Assert.Equal(expectedResult, result);
            Assert.Equal(count, users.UserDict.Count);
            // Verify User object
            if (oldNameExists)
            {
                var currentName = result == 0 ? newName : oldName;
                Assert.Equal(user, users.UserDict[currentName]);
            }
        }

        [Theory]
        [InlineData("chacha20-ietf-poly1305", "kf!V!TFzgeNd93GE", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTprZiFWIVRGemdlTmQ5M0dF")]
        [InlineData("aes-256-gcm", "ymghiR#75TNqpa", "YWVzLTI1Ni1nY206eW1naGlSIzc1VE5xcGE")]
        [InlineData("aes-128-gcm", "tK*sk!9N8@86:UVm", "YWVzLTEyOC1nY206dEsqc2shOU44QDg2OlVWbQ")]
        public void New_Credential_MethodPassword(string method, string password, string expectedUserinfoBase64url)
        {
            var credential = new Credential(method, password);

            Assert.Equal(expectedUserinfoBase64url, credential.UserinfoBase64url);
        }

        [Theory]
        [InlineData("Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTo2JW04RDlhTUI1YkElYTQl", "chacha20-ietf-poly1305", "6%m8D9aMB5bA%a4%")]
        [InlineData("YWVzLTI1Ni1nY206YnBOZ2sqSjNrYUFZeXhIRQ", "aes-256-gcm", "bpNgk*J3kaAYyxHE")]
        [InlineData("YWVzLTEyOC1nY206dkFBbiY4a1I6JGlBRTQ", "aes-128-gcm", "vAAn&8kR:$iAE4")]
        public void New_Credential_UserinfoBase64url(string userinfoBase64url, string expectedMethod, string expectedPassword)
        {
            var credential = new Credential(userinfoBase64url);

            Assert.Equal(expectedMethod, credential.Method);
            Assert.Equal(expectedPassword, credential.Password);
        }

        [Fact]
        public void Add_Update_Remove_Group_ReturnsResult()
        {
            var nodes = new Nodes();
            nodes.AddGroups(new string[] { "MyGroup", "MyGroupWithPlugin" });
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", "443");
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", "443", "v2ray-plugin", "server;tls;host=github.com");
            var users = new Users();
            users.AddUsers(new string[] { "root", "http" });
            Assert.True(users.UserDict.ContainsKey("root"));
            Assert.True(users.UserDict.ContainsKey("http"));

            // Add
            var successAdd = users.AddUserToGroup("root", "MyGroup");
            var anotherSuccessAdd = users.AddUserToGroup("http", "MyGroup");
            var yetAnotherSuccessAdd = users.AddUserToGroup("root", "MyGroupWithPlugin");
            var duplicateAdd = users.AddUserToGroup("root", "MyGroup");
            var badUserAdd = users.AddUserToGroup("nobody", "MyGroup");

            var rootUserCredentials = users.UserDict["root"].Credentials;
            var httpUserCredentials = users.UserDict["http"].Credentials;
            var rootMyGroupCredential = rootUserCredentials["MyGroup"];
            var rootMyGroupWithPluginCredential = rootUserCredentials["MyGroupWithPlugin"];
            var httpMyGroupCredential = httpUserCredentials["MyGroup"];

            Assert.Equal(0, successAdd);
            Assert.Equal(0, anotherSuccessAdd);
            Assert.Equal(0, yetAnotherSuccessAdd);
            Assert.Equal(1, duplicateAdd);
            Assert.Equal(-1, badUserAdd);

            Assert.True(rootUserCredentials.ContainsKey("MyGroup"));
            Assert.True(rootUserCredentials.ContainsKey("MyGroupWithPlugin"));
            Assert.True(httpUserCredentials.ContainsKey("MyGroup"));

            // Update
            users.UpdateCredentialGroupsForAllUsers("MyGroup", "MyGroupNew");

            Assert.False(rootUserCredentials.ContainsKey("MyGroup"));
            Assert.False(httpUserCredentials.ContainsKey("MyGroup"));
            Assert.True(rootUserCredentials.ContainsKey("MyGroupNew"));
            Assert.True(rootUserCredentials.ContainsKey("MyGroupWithPlugin"));
            Assert.True(httpUserCredentials.ContainsKey("MyGroupNew"));
            Assert.Equal(rootMyGroupCredential, rootUserCredentials["MyGroupNew"]);
            Assert.Equal(rootMyGroupWithPluginCredential, rootUserCredentials["MyGroupWithPlugin"]);
            Assert.Equal(httpMyGroupCredential, httpUserCredentials["MyGroupNew"]);

            // Remove
            var successRemoval = users.RemoveUserFromGroup("root", "MyGroupWithPlugin");
            var nonExistingUserRemoval = users.RemoveUserFromGroup("nobody", "MyGroup");
            var nonExistingGroupRemoval = users.RemoveUserFromGroup("root", "MyGroupWithoutPlugin");

            Assert.Equal(0, successRemoval);
            Assert.Equal(-2, nonExistingUserRemoval);
            Assert.Equal(1, nonExistingGroupRemoval);

            Assert.Single(rootUserCredentials);
        }

        [Fact]
        public void Add_Remove_Credential_ReturnsResult()
        {
            var nodes = new Nodes();
            nodes.AddGroups(new string[] { "MyGroup", "MyGroupWithPlugin" });
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", "443");
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", "443", "v2ray-plugin", "server;tls;host=github.com");
            var users = new Users();
            users.AddUsers(new string[] { "root", "http" });
            Assert.True(users.UserDict.ContainsKey("root"));
            Assert.True(users.UserDict.ContainsKey("http"));

            // Add
            var successAdd = users.AddCredentialToUser("root", "MyGroup", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTp5bWdoaVIjNzVUTnFwYQ");
            var anotherSuccessAdd = users.AddCredentialToUser("http", "MyGroup", "YWVzLTEyOC1nY206dEsqc2shOU44QDg2OlVWbWM");
            var yetAnotherSuccessAdd = users.AddCredentialToUser("root", "MyGroupWithPlugin", "YWVzLTEyOC1nY206dkFBbiY4a1I6JGlBRTQ0JA");
            var duplicateAdd = users.AddCredentialToUser("root", "MyGroup", "aes-256-gcm", "wLhN2STZ");
            var badUserAdd = users.AddCredentialToUser("nobody", "MyGroup", "aes-256-gcm", "wLhN2STZ");
            var badUserinfoAdd = users.AddCredentialToUser("http", "MyGroupWithPlugin", "PR6Lf9UR22C5LNBhzEcpsWxd6WpsaeSs");

            var rootUserCredentials = users.UserDict["root"].Credentials;
            var httpUserCredentials = users.UserDict["http"].Credentials;

            Assert.Equal(0, successAdd);
            Assert.Equal(0, anotherSuccessAdd);
            Assert.Equal(0, yetAnotherSuccessAdd);
            Assert.Equal(2, duplicateAdd);
            Assert.Equal(-1, badUserAdd);
            Assert.Equal(-2, badUserinfoAdd);

            Assert.True(rootUserCredentials.ContainsKey("MyGroup"));
            Assert.True(rootUserCredentials.ContainsKey("MyGroupWithPlugin"));
            Assert.True(httpUserCredentials.ContainsKey("MyGroup"));

            Assert.NotNull(rootUserCredentials["MyGroup"]);
            Assert.NotNull(rootUserCredentials["MyGroupWithPlugin"]);
            Assert.NotNull(httpUserCredentials["MyGroup"]);

            // Remove
            var successRemoval = users.RemoveCredentialFromUser("root", "MyGroupWithPlugin");
            var nonExistingUserRemoval = users.RemoveCredentialFromUser("nobody", "MyGroupNew");
            var nonExistingGroupRemoval = users.RemoveCredentialFromUser("root", "MyGroupWithoutPlugin");

            Assert.Equal(0, successRemoval);
            Assert.Equal(-2, nonExistingUserRemoval);
            Assert.Equal(-1, nonExistingGroupRemoval);

            Assert.NotNull(rootUserCredentials["MyGroup"]);
            Assert.NotNull(httpUserCredentials["MyGroup"]);
            Assert.Null(rootUserCredentials["MyGroupWithPlugin"]);

            // Remove from all
            users.RemoveCredentialsFromAllUsers(new string[] { "MyGroup" });

            Assert.Null(rootUserCredentials["MyGroup"]);
            Assert.Null(httpUserCredentials["MyGroup"]);
        }

        [Theory]
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "", null, null, "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/")] // domain name
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "1.1.1.1", 853, "", null, null, "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@1.1.1.1:853/")] // IPv4
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "2001:db8:85a3::8a2e:370:7334", 8388, "", null, null, "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@[2001:db8:85a3::8a2e:370:7334]:8388/")] // IPv6
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "GitHub", null, null, "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/#GitHub")] // fragment
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "👩‍💻", null, null, "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/#%F0%9F%91%A9%E2%80%8D%F0%9F%92%BB")] // fragment
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "", "v2ray-plugin", null, "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/?plugin=v2ray-plugin")] // plugin
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "", null, "server;tls;host=github.com", "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/")] // pluginOpts
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "", "v2ray-plugin", "server;tls;host=github.com", "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com")] // plugin + pluginOpts
        [InlineData("YWVzLTI1Ni1nY206d0xoTjJTVFo", "github.com", 443, "GitHub", "v2ray-plugin", "server;tls;host=github.com", "ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com#GitHub")] // fragment + plugin + pluginOpts
        public void Get_SS_Uris(string userinfoBase64url, string host, int port, string fragment, string? plugin, string? pluginOpts, string expectedSSUri)
        {
            var ssUriString = User.SSUriBuilder(userinfoBase64url, host, port, fragment, plugin, pluginOpts).AbsoluteUri;

            Assert.Equal(expectedSSUri, ssUriString);
        }
    }
}
