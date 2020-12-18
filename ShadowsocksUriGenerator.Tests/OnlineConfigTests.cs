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

            var onlineConfig = OnlineConfig.Generate(users.UserDict.First(), nodes, settings);

            Assert.Equal(1, onlineConfig.Version);
            Assert.Equal("root", onlineConfig.Username);
            Assert.True(Guid.TryParse(onlineConfig.UserUuid, out _));
            Assert.Equal(2, onlineConfig.Servers.Count);
            var server = onlineConfig.Servers[0];
            Assert.Equal("MyNode", server.Name);
            Assert.Equal("github.com", server.Host);
            Assert.Equal(443, server.Port);
            Assert.Equal("chacha20-ietf-poly1305", server.Method);
            Assert.Equal("ymghiR#75TNqpa", server.Password);
            Assert.Null(server.Plugin);
            Assert.Null(server.PluginOpts);
            Assert.True(Guid.TryParse(server.Uuid, out _));
            var serverWithPlugin = onlineConfig.Servers[1];
            Assert.Equal("MyNodeWithPlugin", serverWithPlugin.Name);
            Assert.Equal("github.com", serverWithPlugin.Host);
            Assert.Equal(443, serverWithPlugin.Port);
            Assert.Equal("aes-256-gcm", serverWithPlugin.Method);
            Assert.Equal("wLhN2STZ", serverWithPlugin.Password);
            Assert.Equal("v2ray-plugin", serverWithPlugin.Plugin);
            Assert.Equal("server;tls;host=github.com", serverWithPlugin.PluginOpts);
            Assert.True(Guid.TryParse(serverWithPlugin.Uuid, out _));
        }

        [Fact]
        public async Task Save_Clean_OnlineConfig_ForAllUsers()
        {
            var settings = new Settings();
            var directory = settings.OnlineConfigOutputDirectory;
            var nodes = new Nodes();
            var users = new Users();
            users.AddUsers(new string[] { "root", "http", "nobody", });

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
            var directory = settings.OnlineConfigOutputDirectory;
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
