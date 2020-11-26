using System;
using System.Linq;
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
            users.AddCredentialToUser("root", "MyGroup", "Y2hhY2hhMjAtaWV0Zi1wb2x5MTMwNTp5bWdoaVIjNzVUTnFwYQ", nodes);
            users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ", nodes);

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
    }
}
