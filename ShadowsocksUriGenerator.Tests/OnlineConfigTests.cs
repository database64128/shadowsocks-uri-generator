using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShadowsocksUriGenerator.Tests
{
    public class OnlineConfigTests
    {
        [Fact]
        public void Generate_OnlineConfig_Properties()
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

            settings.OnlineConfigDeliverByGroup = false;
            var userSingleOnlineConfigDict = OnlineConfig.GenerateForUser(user, nodes, settings);
            settings.OnlineConfigDeliverByGroup = true;
            var userPerGroupOnlineConfigDict = OnlineConfig.GenerateForUser(user, nodes, settings);

            // userSingleOnlineConfigDict
            Assert.Single(userSingleOnlineConfigDict);
            var userSingleOnlineConfig = userPerGroupOnlineConfigDict[user.Value.Uuid];

            // userSingleOnlineConfig
            Assert.Equal(1, userSingleOnlineConfig.Version);
            Assert.Equal("root", userSingleOnlineConfig.Username);
            Assert.True(Guid.TryParse(userSingleOnlineConfig.UserUuid, out _));
            Assert.Equal(2, userSingleOnlineConfig.Servers.Count);
            var singleServer = userSingleOnlineConfig.Servers[0];
            Assert.Equal("MyNode", singleServer.Name);
            Assert.Equal("github.com", singleServer.Host);
            Assert.Equal(443, singleServer.Port);
            Assert.Equal("chacha20-ietf-poly1305", singleServer.Method);
            Assert.Equal("ymghiR#75TNqpa", singleServer.Password);
            Assert.Null(singleServer.Plugin);
            Assert.Null(singleServer.PluginOpts);
            Assert.True(Guid.TryParse(singleServer.Uuid, out _));
            var singleServerWithPlugin = userSingleOnlineConfig.Servers[1];
            Assert.Equal("MyNodeWithPlugin", singleServerWithPlugin.Name);
            Assert.Equal("github.com", singleServerWithPlugin.Host);
            Assert.Equal(443, singleServerWithPlugin.Port);
            Assert.Equal("aes-256-gcm", singleServerWithPlugin.Method);
            Assert.Equal("wLhN2STZ", singleServerWithPlugin.Password);
            Assert.Equal("v2ray-plugin", singleServerWithPlugin.Plugin);
            Assert.Equal("server;tls;host=github.com", singleServerWithPlugin.PluginOpts);
            Assert.True(Guid.TryParse(singleServerWithPlugin.Uuid, out _));

            // userPerGroupOnlineConfigDict
            Assert.Equal(3, userPerGroupOnlineConfigDict.Count);
            var userOnlineConfig = userPerGroupOnlineConfigDict[user.Value.Uuid];
            var userMyGroupOnlineConfig = userPerGroupOnlineConfigDict[$"{user.Value.Uuid}/MyGroup"];
            var userMyGroupWithPluginOnlineConfig = userPerGroupOnlineConfigDict[$"{user.Value.Uuid}/MyGroupWithPlugin"];

            // userOnlineConfig
            Assert.Equal(1, userOnlineConfig.Version);
            Assert.Equal("root", userOnlineConfig.Username);
            Assert.True(Guid.TryParse(userOnlineConfig.UserUuid, out _));
            Assert.Equal(2, userOnlineConfig.Servers.Count);
            var server = userOnlineConfig.Servers[0];
            Assert.Equal("MyNode", server.Name);
            Assert.Equal("github.com", server.Host);
            Assert.Equal(443, server.Port);
            Assert.Equal("chacha20-ietf-poly1305", server.Method);
            Assert.Equal("ymghiR#75TNqpa", server.Password);
            Assert.Null(server.Plugin);
            Assert.Null(server.PluginOpts);
            Assert.True(Guid.TryParse(server.Uuid, out _));
            var serverWithPlugin = userOnlineConfig.Servers[1];
            Assert.Equal("MyNodeWithPlugin", serverWithPlugin.Name);
            Assert.Equal("github.com", serverWithPlugin.Host);
            Assert.Equal(443, serverWithPlugin.Port);
            Assert.Equal("aes-256-gcm", serverWithPlugin.Method);
            Assert.Equal("wLhN2STZ", serverWithPlugin.Password);
            Assert.Equal("v2ray-plugin", serverWithPlugin.Plugin);
            Assert.Equal("server;tls;host=github.com", serverWithPlugin.PluginOpts);
            Assert.True(Guid.TryParse(serverWithPlugin.Uuid, out _));

            // userMyGroupOnlineConfig
            Assert.Equal(1, userMyGroupOnlineConfig.Version);
            Assert.Equal("root", userMyGroupOnlineConfig.Username);
            Assert.True(Guid.TryParse(userMyGroupOnlineConfig.UserUuid, out _));
            Assert.Single(userMyGroupOnlineConfig.Servers);
            var serverMyGroup = userMyGroupOnlineConfig.Servers[0];
            Assert.Equal("MyNode", serverMyGroup.Name);
            Assert.Equal("github.com", serverMyGroup.Host);
            Assert.Equal(443, serverMyGroup.Port);
            Assert.Equal("chacha20-ietf-poly1305", serverMyGroup.Method);
            Assert.Equal("ymghiR#75TNqpa", serverMyGroup.Password);
            Assert.Null(serverMyGroup.Plugin);
            Assert.Null(serverMyGroup.PluginOpts);
            Assert.True(Guid.TryParse(serverMyGroup.Uuid, out _));

            // userMyGroupWithPluginOnlineConfig
            Assert.Equal(1, userMyGroupWithPluginOnlineConfig.Version);
            Assert.Equal("root", userMyGroupWithPluginOnlineConfig.Username);
            Assert.True(Guid.TryParse(userMyGroupWithPluginOnlineConfig.UserUuid, out _));
            Assert.Single(userMyGroupWithPluginOnlineConfig.Servers);
            var serverMyGroupWithPlugin = userMyGroupWithPluginOnlineConfig.Servers[0];
            Assert.Equal("MyNodeWithPlugin", serverMyGroupWithPlugin.Name);
            Assert.Equal("github.com", serverMyGroupWithPlugin.Host);
            Assert.Equal(443, serverMyGroupWithPlugin.Port);
            Assert.Equal("aes-256-gcm", serverMyGroupWithPlugin.Method);
            Assert.Equal("wLhN2STZ", serverMyGroupWithPlugin.Password);
            Assert.Equal("v2ray-plugin", serverMyGroupWithPlugin.Plugin);
            Assert.Equal("server;tls;host=github.com", serverMyGroupWithPlugin.PluginOpts);
            Assert.True(Guid.TryParse(serverMyGroupWithPlugin.Uuid, out _));
        }

        [Fact]
        public async Task Save_Clean_OnlineConfig_ForAllUsers()
        {
            var settings = new Settings();
            var directory = Utilities.GetAbsolutePath(settings.OnlineConfigOutputDirectory);
            var nodes = new Nodes();
            var users = new Users();
            users.AddUsers(new string[] { "root", "http", "nobody", });

            settings.OnlineConfigDeliverByGroup = false;
            // Save
            var genResult = await OnlineConfig.GenerateAndSave(users, nodes, settings);

            Assert.Equal(0, genResult);
            Assert.True(Directory.Exists(directory));
            foreach (var user in users.UserDict.Values)
                Assert.True(File.Exists($"{directory}/{user.Uuid}.json"));

            // Clean
            OnlineConfig.Remove(users, settings);

            Assert.True(Directory.Exists(directory));
            foreach (var user in users.UserDict.Values)
                Assert.False(File.Exists($"{directory}/{user.Uuid}.json"));

            // Delete working directory.
            Directory.Delete(directory);
        }

        [Theory]
        [InlineData(0, "root")]
        [InlineData(0, "http", "nobody")]
        [InlineData(0, "root", "http", "nobody")]
        [InlineData(404, "whoever")]
        [InlineData(404, "whoever", "nobody")]
        [InlineData(404, "nobody", "whoever", "http")]
        public async Task Save_Clean_OnlineConfig_ForSpecifiedUsers(int expectedResult, params string[] selectedUsernames)
        {
            var settings = new Settings();
            var directory = Utilities.GetAbsolutePath(settings.OnlineConfigOutputDirectory);
            var nodes = new Nodes();
            var users = new Users();
            users.AddUsers(new string[] { "root", "http", "nobody", });

            // Save
            var genResult = await OnlineConfig.GenerateAndSave(users, nodes, settings, selectedUsernames);

            Assert.Equal(expectedResult, genResult);
            if (expectedResult == 0)
            {
                Assert.True(Directory.Exists(directory));
                var expectedFileCount = selectedUsernames.Length;
                var fileCount = Directory.GetFiles(directory).Length;
                Assert.Equal(expectedFileCount, fileCount);
            }

            // Clean
            OnlineConfig.Remove(users, settings, selectedUsernames[0]);

            if (expectedResult == 0)
            {
                Assert.True(Directory.Exists(directory));
                var expectedFileCount = selectedUsernames.Length - 1;
                var fileCount = Directory.GetFiles(directory).Length;
                Assert.Equal(expectedFileCount, fileCount);
            }

            // Delete working directory.
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }
}
