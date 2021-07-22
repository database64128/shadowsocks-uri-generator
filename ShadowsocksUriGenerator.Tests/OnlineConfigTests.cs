using ShadowsocksUriGenerator.OnlineConfig;
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

            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");

            var users = new Users();
            users.AddUser("root");
            users.AddCredentialToUser("root", "MyGroup", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTp5bWdoaVIjNzVUTnFwYQ");
            users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var user = users.UserDict.First();

            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv", user.Value.Uuid, "test");
            var myNode = nodes.Groups["MyGroup"].NodeDict["MyNode"];
            var myNodeWithPlugin = nodes.Groups["MyGroupWithPlugin"].NodeDict["MyNodeWithPlugin"];

            settings.OnlineConfigDeliverByGroup = false;
            var userSingleOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            settings.OnlineConfigDeliverByGroup = true;
            var userPerGroupOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);

            // userSingleOnlineConfigDict
            Assert.Single(userSingleOnlineConfigDict);
            var userSingleOnlineConfig = userPerGroupOnlineConfigDict[user.Value.Uuid];

            // userSingleOnlineConfig
            Assert.Equal(1, userSingleOnlineConfig.Version);
            Assert.Equal("root", userSingleOnlineConfig.Username);
            Assert.Equal(user.Value.Uuid, userSingleOnlineConfig.Id);
            Assert.Null(userSingleOnlineConfig.BytesUsed);
            Assert.Null(userSingleOnlineConfig.BytesRemaining);
            Assert.Equal(2, userSingleOnlineConfig.Servers.Count);

            var singleServer = userSingleOnlineConfig.Servers[0];
            Assert.Equal(myNode.Uuid, singleServer.Id);
            Assert.Equal("MyNode", singleServer.Name);
            Assert.Equal("github.com", singleServer.Host);
            Assert.Equal(443, singleServer.Port);
            Assert.Equal("chacha20-ietf-poly1305", singleServer.Method);
            Assert.Equal("ymghiR#75TNqpa", singleServer.Password);
            Assert.Null(singleServer.PluginName);
            Assert.Null(singleServer.PluginVersion);
            Assert.Null(singleServer.PluginOptions);
            Assert.Null(singleServer.PluginArguments);
            Assert.Equal("MyGroup", singleServer.Group);
            Assert.Null(singleServer.Owner);
            Assert.Null(singleServer.Tags);

            var singleServerWithPlugin = userSingleOnlineConfig.Servers[1];
            Assert.Equal(myNodeWithPlugin.Uuid, singleServerWithPlugin.Id);
            Assert.Equal("MyNodeWithPlugin", singleServerWithPlugin.Name);
            Assert.Equal("github.com", singleServerWithPlugin.Host);
            Assert.Equal(443, singleServerWithPlugin.Port);
            Assert.Equal("aes-256-gcm", singleServerWithPlugin.Method);
            Assert.Equal("wLhN2STZ", singleServerWithPlugin.Password);
            Assert.Equal("v2ray-plugin", singleServerWithPlugin.PluginName);
            Assert.Equal("1.0", singleServerWithPlugin.PluginVersion);
            Assert.Equal("server;tls;host=github.com", singleServerWithPlugin.PluginOptions);
            Assert.Equal("-vvvvvv", singleServerWithPlugin.PluginArguments);
            Assert.Equal("root", singleServerWithPlugin.Owner);
            Assert.Single(singleServerWithPlugin.Tags, "test");

            // userPerGroupOnlineConfigDict
            Assert.Equal(3, userPerGroupOnlineConfigDict.Count);
            var userOnlineConfig = userPerGroupOnlineConfigDict[user.Value.Uuid];
            var userMyGroupOnlineConfig = userPerGroupOnlineConfigDict[$"{user.Value.Uuid}/MyGroup"];
            var userMyGroupWithPluginOnlineConfig = userPerGroupOnlineConfigDict[$"{user.Value.Uuid}/MyGroupWithPlugin"];

            // userOnlineConfig
            Assert.Equal(1, userOnlineConfig.Version);
            Assert.Equal("root", userOnlineConfig.Username);
            Assert.Equal(user.Value.Uuid, userOnlineConfig.Id);
            Assert.Null(userOnlineConfig.BytesUsed);
            Assert.Null(userOnlineConfig.BytesRemaining);
            Assert.Equal(2, userOnlineConfig.Servers.Count);

            var server = userOnlineConfig.Servers[0];
            Assert.Equal(myNode.Uuid, server.Id);
            Assert.Equal("MyNode", server.Name);
            Assert.Equal("github.com", server.Host);
            Assert.Equal(443, server.Port);
            Assert.Equal("chacha20-ietf-poly1305", server.Method);
            Assert.Equal("ymghiR#75TNqpa", server.Password);
            Assert.Null(server.PluginName);
            Assert.Null(server.PluginVersion);
            Assert.Null(server.PluginOptions);
            Assert.Null(server.PluginArguments);
            Assert.Equal("MyGroup", server.Group);
            Assert.Null(server.Owner);
            Assert.Null(server.Tags);

            var serverWithPlugin = userOnlineConfig.Servers[1];
            Assert.Equal(myNodeWithPlugin.Uuid, serverWithPlugin.Id);
            Assert.Equal("MyNodeWithPlugin", serverWithPlugin.Name);
            Assert.Equal("github.com", serverWithPlugin.Host);
            Assert.Equal(443, serverWithPlugin.Port);
            Assert.Equal("aes-256-gcm", serverWithPlugin.Method);
            Assert.Equal("wLhN2STZ", serverWithPlugin.Password);
            Assert.Equal("v2ray-plugin", serverWithPlugin.PluginName);
            Assert.Equal("1.0", serverWithPlugin.PluginVersion);
            Assert.Equal("server;tls;host=github.com", serverWithPlugin.PluginOptions);
            Assert.Equal("-vvvvvv", serverWithPlugin.PluginArguments);
            Assert.Equal("root", serverWithPlugin.Owner);
            Assert.Single(serverWithPlugin.Tags, "test");

            // userMyGroupOnlineConfig
            Assert.Equal(1, userMyGroupOnlineConfig.Version);
            Assert.Equal("root", userMyGroupOnlineConfig.Username);
            Assert.Equal(user.Value.Uuid, userMyGroupOnlineConfig.Id);
            Assert.Null(userMyGroupOnlineConfig.BytesUsed);
            Assert.Null(userMyGroupOnlineConfig.BytesRemaining);
            Assert.Single(userMyGroupOnlineConfig.Servers);

            var serverMyGroup = userMyGroupOnlineConfig.Servers[0];
            Assert.Equal(myNode.Uuid, serverMyGroup.Id);
            Assert.Equal("MyNode", serverMyGroup.Name);
            Assert.Equal("github.com", serverMyGroup.Host);
            Assert.Equal(443, serverMyGroup.Port);
            Assert.Equal("chacha20-ietf-poly1305", serverMyGroup.Method);
            Assert.Equal("ymghiR#75TNqpa", serverMyGroup.Password);
            Assert.Null(serverMyGroup.PluginName);
            Assert.Null(serverMyGroup.PluginVersion);
            Assert.Null(serverMyGroup.PluginOptions);
            Assert.Null(serverMyGroup.PluginArguments);
            Assert.Equal("MyGroup", serverMyGroup.Group);
            Assert.Null(serverMyGroup.Owner);
            Assert.Null(serverMyGroup.Tags);

            // userMyGroupWithPluginOnlineConfig
            Assert.Equal(1, userMyGroupWithPluginOnlineConfig.Version);
            Assert.Equal("root", userMyGroupWithPluginOnlineConfig.Username);
            Assert.True(Guid.TryParse(userMyGroupWithPluginOnlineConfig.Id, out _));
            Assert.Single(userMyGroupWithPluginOnlineConfig.Servers);

            var serverMyGroupWithPlugin = userMyGroupWithPluginOnlineConfig.Servers[0];
            Assert.Equal(myNodeWithPlugin.Uuid, serverMyGroupWithPlugin.Id);
            Assert.Equal("MyNodeWithPlugin", serverMyGroupWithPlugin.Name);
            Assert.Equal("github.com", serverMyGroupWithPlugin.Host);
            Assert.Equal(443, serverMyGroupWithPlugin.Port);
            Assert.Equal("aes-256-gcm", serverMyGroupWithPlugin.Method);
            Assert.Equal("wLhN2STZ", serverMyGroupWithPlugin.Password);
            Assert.Equal("v2ray-plugin", serverMyGroupWithPlugin.PluginName);
            Assert.Equal("1.0", serverMyGroupWithPlugin.PluginVersion);
            Assert.Equal("server;tls;host=github.com", serverMyGroupWithPlugin.PluginOptions);
            Assert.Equal("-vvvvvv", serverMyGroupWithPlugin.PluginArguments);
            Assert.Equal("root", serverMyGroupWithPlugin.Owner);
            Assert.Single(serverMyGroupWithPlugin.Tags, "test");
        }

        [Fact]
        public async Task Save_Clean_OnlineConfig_ForAllUsers()
        {
            var settings = new Settings();
            var directory = Utilities.GetAbsolutePath(settings.OnlineConfigOutputDirectory);
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com");
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            users.AddUser("nobody");
            users.AddCredentialToUser("root", "MyGroup", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTp5bWdoaVIjNzVUTnFwYQ");
            users.AddCredentialToUser("http", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var rootUser = users.UserDict["root"];
            var httpUser = users.UserDict["http"];
            var nobodyUser = users.UserDict["nobody"];

            // Disable delivery by group
            settings.OnlineConfigDeliverByGroup = false;
            // Save
            var genResult = await SIP008StaticGen.GenerateAndSave(users, nodes, settings);

            Assert.Null(genResult);
            Assert.True(Directory.Exists(directory));
            foreach (var user in users.UserDict.Values)
                Assert.True(File.Exists($"{directory}/{user.Uuid}.json"));

            // Clean
            SIP008StaticGen.Remove(users, settings);

            Assert.True(Directory.Exists(directory));
            foreach (var user in users.UserDict.Values)
                Assert.False(File.Exists($"{directory}/{user.Uuid}.json"));

            // Delete working directory.
            Directory.Delete(directory);

            // Enable delivery by group
            settings.OnlineConfigDeliverByGroup = true;
            // Save
            var genByGroupResult = await SIP008StaticGen.GenerateAndSave(users, nodes, settings);

            Assert.Null(genByGroupResult);
            Assert.True(Directory.Exists(directory));
            foreach (var user in users.UserDict.Values)
                Assert.True(File.Exists($"{directory}/{user.Uuid}.json"));
            Assert.True(Directory.Exists($"{directory}/{rootUser.Uuid}"));
            Assert.True(File.Exists($"{directory}/{rootUser.Uuid}/MyGroup.json"));
            Assert.False(File.Exists($"{directory}/{rootUser.Uuid}/MyGroupWithPlugin.json"));
            Assert.True(Directory.Exists($"{directory}/{httpUser.Uuid}"));
            Assert.False(File.Exists($"{directory}/{httpUser.Uuid}/MyGroup.json"));
            Assert.True(File.Exists($"{directory}/{httpUser.Uuid}/MyGroupWithPlugin.json"));
            Assert.False(Directory.Exists($"{directory}/{nobodyUser.Uuid}"));

            // Clean
            SIP008StaticGen.Remove(users, settings);

            Assert.True(Directory.Exists(directory));
            foreach (var user in users.UserDict.Values)
                Assert.False(File.Exists($"{directory}/{user.Uuid}.json"));
            Assert.False(Directory.Exists($"{directory}/{rootUser.Uuid}"));
            Assert.False(Directory.Exists($"{directory}/{httpUser.Uuid}"));
            Assert.False(Directory.Exists($"{directory}/{nobodyUser.Uuid}"));

            // Delete working directory.
            Directory.Delete(directory);
        }

        [Theory]
        [InlineData(null, "root")]
        [InlineData(null, "http", "nobody")]
        [InlineData(null, "root", "http", "nobody")]
        [InlineData("Error: user whoever doesn't exist.", "whoever")]
        [InlineData("Error: user whoever doesn't exist.", "whoever", "nobody")]
        [InlineData("Error: user whoever doesn't exist.", "nobody", "whoever", "http")]
        public async Task Save_Clean_OnlineConfig_ForSpecifiedUsers(string? expectedResult, params string[] selectedUsernames)
        {
            var settings = new Settings();
            var directory = Utilities.GetAbsolutePath(settings.OnlineConfigOutputDirectory);
            using var nodes = new Nodes();
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            users.AddUser("nobody");

            // Constant interpolated strings is a preview feature.
            if (expectedResult is not null)
                expectedResult = $"{expectedResult}{Environment.NewLine}";

            settings.OnlineConfigDeliverByGroup = false;
            // Save
            var genResult = await SIP008StaticGen.GenerateAndSave(users, nodes, settings, default, selectedUsernames);

            Assert.Equal(expectedResult, genResult);
            if (expectedResult is null)
            {
                Assert.True(Directory.Exists(directory));
                var expectedFileCount = selectedUsernames.Length;
                var fileCount = Directory.GetFiles(directory).Length;
                Assert.Equal(expectedFileCount, fileCount);
            }

            // Clean
            SIP008StaticGen.Remove(users, settings, selectedUsernames[0]);

            if (expectedResult is null)
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
