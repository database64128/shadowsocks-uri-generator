using ShadowsocksUriGenerator.Data;
using ShadowsocksUriGenerator.OnlineConfig;

namespace ShadowsocksUriGenerator.Tests
{
    public class OnlineConfigTests
    {
        [Test]
        public async Task Generate_OnlineConfig_Properties()
        {
            var settings = new Settings();

            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");

            var users = new Users();
            users.AddUser("root");
            users.AddCredentialToUser("root", "MyGroup", "chacha20-ietf-poly1305", "ymghiR#75TNqpa");
            users.AddCredentialToUser("root", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var user = users.UserDict.First();

            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443, null, null, null, null, null, [], []);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv", user.Value.Uuid, ["test"], []);
            var myNode = nodes.Groups["MyGroup"].NodeDict["MyNode"];
            var myNodeWithPlugin = nodes.Groups["MyGroupWithPlugin"].NodeDict["MyNodeWithPlugin"];

            settings.OnlineConfigDeliverByGroup = false;
            var userSingleOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);
            settings.OnlineConfigDeliverByGroup = true;
            var userPerGroupOnlineConfigDict = SIP008StaticGen.GenerateForUser(user, users, nodes, settings);

            // userSingleOnlineConfigDict
            await Assert.That(userSingleOnlineConfigDict).HasSingleItem();
            var userSingleOnlineConfig = userPerGroupOnlineConfigDict[user.Value.Uuid];

            // userSingleOnlineConfig
            await Assert.That(userSingleOnlineConfig.Version).IsEqualTo(1);
            await Assert.That(userSingleOnlineConfig.Username).IsEqualTo("root");
            await Assert.That(userSingleOnlineConfig.Id).IsEqualTo(user.Value.Uuid);
            await Assert.That(userSingleOnlineConfig.BytesUsed).IsNull();
            await Assert.That(userSingleOnlineConfig.BytesRemaining).IsNull();
            await Assert.That(userSingleOnlineConfig.Servers.Count()).IsEqualTo(2);

            using var userSingleOnlineConfigEnumerator = userSingleOnlineConfig.Servers.GetEnumerator();

            userSingleOnlineConfigEnumerator.MoveNext();
            var singleServer = userSingleOnlineConfigEnumerator.Current;
            await Assert.That(singleServer.Id).IsEqualTo(myNode.Uuid);
            await Assert.That(singleServer.Name).IsEqualTo("MyNode");
            await Assert.That(singleServer.Host).IsEqualTo("github.com");
            await Assert.That(singleServer.Port).IsEqualTo(443);
            await Assert.That(singleServer.Method).IsEqualTo("chacha20-ietf-poly1305");
            await Assert.That(singleServer.Password).IsEqualTo("ymghiR#75TNqpa");
            await Assert.That(singleServer.PluginName).IsNull();
            await Assert.That(singleServer.PluginVersion).IsNull();
            await Assert.That(singleServer.PluginOptions).IsNull();
            await Assert.That(singleServer.PluginArguments).IsNull();
            await Assert.That(singleServer.Group).IsEqualTo("MyGroup");
            await Assert.That(singleServer.Owner).IsNull();
            await Assert.That(singleServer.Tags).IsNull();

            userSingleOnlineConfigEnumerator.MoveNext();
            var singleServerWithPlugin = userSingleOnlineConfigEnumerator.Current;
            await Assert.That(singleServerWithPlugin.Id).IsEqualTo(myNodeWithPlugin.Uuid);
            await Assert.That(singleServerWithPlugin.Name).IsEqualTo("MyNodeWithPlugin");
            await Assert.That(singleServerWithPlugin.Host).IsEqualTo("github.com");
            await Assert.That(singleServerWithPlugin.Port).IsEqualTo(443);
            await Assert.That(singleServerWithPlugin.Method).IsEqualTo("aes-256-gcm");
            await Assert.That(singleServerWithPlugin.Password).IsEqualTo("wLhN2STZ");
            await Assert.That(singleServerWithPlugin.PluginName).IsEqualTo("v2ray-plugin");
            await Assert.That(singleServerWithPlugin.PluginVersion).IsEqualTo("1.0");
            await Assert.That(singleServerWithPlugin.PluginOptions).IsEqualTo("server;tls;host=github.com");
            await Assert.That(singleServerWithPlugin.PluginArguments).IsEqualTo("-vvvvvv");
            await Assert.That(singleServerWithPlugin.Owner).IsEqualTo("root");
            await Assert.That(singleServerWithPlugin.Tags).IsNotNull();
            await Assert.That(singleServerWithPlugin.Tags).HasSingleItem();

            // userPerGroupOnlineConfigDict
            await Assert.That(userPerGroupOnlineConfigDict.Count).IsEqualTo(3);
            var userOnlineConfig = userPerGroupOnlineConfigDict[user.Value.Uuid];
            var userMyGroupOnlineConfig = userPerGroupOnlineConfigDict[$"{user.Value.Uuid}/MyGroup"];
            var userMyGroupWithPluginOnlineConfig = userPerGroupOnlineConfigDict[$"{user.Value.Uuid}/MyGroupWithPlugin"];

            // userOnlineConfig
            await Assert.That(userOnlineConfig.Version).IsEqualTo(1);
            await Assert.That(userOnlineConfig.Username).IsEqualTo("root");
            await Assert.That(userOnlineConfig.Id).IsEqualTo(user.Value.Uuid);
            await Assert.That(userOnlineConfig.BytesUsed).IsNull();
            await Assert.That(userOnlineConfig.BytesRemaining).IsNull();
            await Assert.That(userOnlineConfig.Servers.Count()).IsEqualTo(2);

            using var userOnlineConfigEnumerator = userOnlineConfig.Servers.GetEnumerator();

            userOnlineConfigEnumerator.MoveNext();
            var server = userOnlineConfigEnumerator.Current;
            await Assert.That(server.Id).IsEqualTo(myNode.Uuid);
            await Assert.That(server.Name).IsEqualTo("MyNode");
            await Assert.That(server.Host).IsEqualTo("github.com");
            await Assert.That(server.Port).IsEqualTo(443);
            await Assert.That(server.Method).IsEqualTo("chacha20-ietf-poly1305");
            await Assert.That(server.Password).IsEqualTo("ymghiR#75TNqpa");
            await Assert.That(server.PluginName).IsNull();
            await Assert.That(server.PluginVersion).IsNull();
            await Assert.That(server.PluginOptions).IsNull();
            await Assert.That(server.PluginArguments).IsNull();
            await Assert.That(server.Group).IsEqualTo("MyGroup");
            await Assert.That(server.Owner).IsNull();
            await Assert.That(server.Tags).IsNull();

            userOnlineConfigEnumerator.MoveNext();
            var serverWithPlugin = userOnlineConfigEnumerator.Current;
            await Assert.That(serverWithPlugin.Id).IsEqualTo(myNodeWithPlugin.Uuid);
            await Assert.That(serverWithPlugin.Name).IsEqualTo("MyNodeWithPlugin");
            await Assert.That(serverWithPlugin.Host).IsEqualTo("github.com");
            await Assert.That(serverWithPlugin.Port).IsEqualTo(443);
            await Assert.That(serverWithPlugin.Method).IsEqualTo("aes-256-gcm");
            await Assert.That(serverWithPlugin.Password).IsEqualTo("wLhN2STZ");
            await Assert.That(serverWithPlugin.PluginName).IsEqualTo("v2ray-plugin");
            await Assert.That(serverWithPlugin.PluginVersion).IsEqualTo("1.0");
            await Assert.That(serverWithPlugin.PluginOptions).IsEqualTo("server;tls;host=github.com");
            await Assert.That(serverWithPlugin.PluginArguments).IsEqualTo("-vvvvvv");
            await Assert.That(serverWithPlugin.Owner).IsEqualTo("root");
            await Assert.That(serverWithPlugin.Tags).IsNotNull();
            await Assert.That(serverWithPlugin.Tags).HasSingleItem();

            // userMyGroupOnlineConfig
            await Assert.That(userMyGroupOnlineConfig.Version).IsEqualTo(1);
            await Assert.That(userMyGroupOnlineConfig.Username).IsEqualTo("root");
            await Assert.That(userMyGroupOnlineConfig.Id).IsEqualTo(user.Value.Uuid);
            await Assert.That(userMyGroupOnlineConfig.BytesUsed).IsNull();
            await Assert.That(userMyGroupOnlineConfig.BytesRemaining).IsNull();
            await Assert.That(userMyGroupOnlineConfig.Servers).HasSingleItem();

            var serverMyGroup = userMyGroupOnlineConfig.Servers.Single();
            await Assert.That(serverMyGroup.Id).IsEqualTo(myNode.Uuid);
            await Assert.That(serverMyGroup.Name).IsEqualTo("MyNode");
            await Assert.That(serverMyGroup.Host).IsEqualTo("github.com");
            await Assert.That(serverMyGroup.Port).IsEqualTo(443);
            await Assert.That(serverMyGroup.Method).IsEqualTo("chacha20-ietf-poly1305");
            await Assert.That(serverMyGroup.Password).IsEqualTo("ymghiR#75TNqpa");
            await Assert.That(serverMyGroup.PluginName).IsNull();
            await Assert.That(serverMyGroup.PluginVersion).IsNull();
            await Assert.That(serverMyGroup.PluginOptions).IsNull();
            await Assert.That(serverMyGroup.PluginArguments).IsNull();
            await Assert.That(serverMyGroup.Group).IsEqualTo("MyGroup");
            await Assert.That(serverMyGroup.Owner).IsNull();
            await Assert.That(serverMyGroup.Tags).IsNull();

            // userMyGroupWithPluginOnlineConfig
            await Assert.That(userMyGroupWithPluginOnlineConfig.Version).IsEqualTo(1);
            await Assert.That(userMyGroupWithPluginOnlineConfig.Username).IsEqualTo("root");
            await Assert.That(Guid.TryParse(userMyGroupWithPluginOnlineConfig.Id, out _)).IsTrue();
            await Assert.That(userMyGroupWithPluginOnlineConfig.Servers).HasSingleItem();

            var serverMyGroupWithPlugin = userMyGroupWithPluginOnlineConfig.Servers.Single();
            await Assert.That(serverMyGroupWithPlugin.Id).IsEqualTo(myNodeWithPlugin.Uuid);
            await Assert.That(serverMyGroupWithPlugin.Name).IsEqualTo("MyNodeWithPlugin");
            await Assert.That(serverMyGroupWithPlugin.Host).IsEqualTo("github.com");
            await Assert.That(serverMyGroupWithPlugin.Port).IsEqualTo(443);
            await Assert.That(serverMyGroupWithPlugin.Method).IsEqualTo("aes-256-gcm");
            await Assert.That(serverMyGroupWithPlugin.Password).IsEqualTo("wLhN2STZ");
            await Assert.That(serverMyGroupWithPlugin.PluginName).IsEqualTo("v2ray-plugin");
            await Assert.That(serverMyGroupWithPlugin.PluginVersion).IsEqualTo("1.0");
            await Assert.That(serverMyGroupWithPlugin.PluginOptions).IsEqualTo("server;tls;host=github.com");
            await Assert.That(serverMyGroupWithPlugin.PluginArguments).IsEqualTo("-vvvvvv");
            await Assert.That(serverMyGroupWithPlugin.Owner).IsEqualTo("root");
            await Assert.That(serverMyGroupWithPlugin.Tags).IsNotNull();
            await Assert.That(serverMyGroupWithPlugin.Tags).HasSingleItem();
        }

        [Test]
        public async Task Save_Clean_OnlineConfig_ForAllUsers()
        {
            var settings = new Settings();
            var directory = settings.OnlineConfigOutputDirectory;
            using var nodes = new Nodes();
            nodes.AddGroup("MyGroup");
            nodes.AddGroup("MyGroupWithPlugin");
            nodes.AddNodeToGroup("MyGroup", "MyNode", "github.com", 443, null, null, null, null, null, [], []);
            nodes.AddNodeToGroup("MyGroupWithPlugin", "MyNodeWithPlugin", "github.com", 443, "v2ray-plugin", "server;tls;host=github.com", null, null, null, [], []);
            var users = new Users();
            users.AddUser("root");
            users.AddUser("http");
            users.AddUser("nobody");
            users.AddCredentialToUser("root", "MyGroup", "chacha20-ietf-poly1305", "ymghiR#75TNqpa");
            users.AddCredentialToUser("http", "MyGroupWithPlugin", "aes-256-gcm", "wLhN2STZ");
            var rootUser = users.UserDict["root"];
            var httpUser = users.UserDict["http"];
            var nobodyUser = users.UserDict["nobody"];

            // Disable delivery by group
            settings.OnlineConfigDeliverByGroup = false;
            // Save
            var genResult = await SIP008StaticGen.GenerateAndSave(users, nodes, settings);

            await Assert.That(genResult).IsNull();
            await Assert.That(Directory.Exists(directory)).IsTrue();
            foreach (var user in users.UserDict.Values)
                await Assert.That(File.Exists($"{directory}/{user.Uuid}.json")).IsTrue();

            // Clean
            SIP008StaticGen.Remove(users, settings);

            await Assert.That(Directory.Exists(directory)).IsTrue();
            foreach (var user in users.UserDict.Values)
                await Assert.That(File.Exists($"{directory}/{user.Uuid}.json")).IsFalse();

            // Delete working directory.
            Directory.Delete(directory);

            // Enable delivery by group
            settings.OnlineConfigDeliverByGroup = true;
            // Save
            var genByGroupResult = await SIP008StaticGen.GenerateAndSave(users, nodes, settings);

            await Assert.That(genByGroupResult).IsNull();
            await Assert.That(Directory.Exists(directory)).IsTrue();
            foreach (var user in users.UserDict.Values)
                await Assert.That(File.Exists($"{directory}/{user.Uuid}.json")).IsTrue();
            await Assert.That(Directory.Exists($"{directory}/{rootUser.Uuid}")).IsTrue();
            await Assert.That(File.Exists($"{directory}/{rootUser.Uuid}/MyGroup.json")).IsTrue();
            await Assert.That(File.Exists($"{directory}/{rootUser.Uuid}/MyGroupWithPlugin.json")).IsFalse();
            await Assert.That(Directory.Exists($"{directory}/{httpUser.Uuid}")).IsTrue();
            await Assert.That(File.Exists($"{directory}/{httpUser.Uuid}/MyGroup.json")).IsFalse();
            await Assert.That(File.Exists($"{directory}/{httpUser.Uuid}/MyGroupWithPlugin.json")).IsTrue();
            await Assert.That(Directory.Exists($"{directory}/{nobodyUser.Uuid}")).IsFalse();

            // Clean
            SIP008StaticGen.Remove(users, settings);

            await Assert.That(Directory.Exists(directory)).IsTrue();
            foreach (var user in users.UserDict.Values)
                await Assert.That(File.Exists($"{directory}/{user.Uuid}.json")).IsFalse();
            await Assert.That(Directory.Exists($"{directory}/{rootUser.Uuid}")).IsFalse();
            await Assert.That(Directory.Exists($"{directory}/{httpUser.Uuid}")).IsFalse();
            await Assert.That(Directory.Exists($"{directory}/{nobodyUser.Uuid}")).IsFalse();

            // Delete working directory.
            Directory.Delete(directory);
        }

        [Test]
        [Arguments(null, "root")]
        [Arguments(null, "http", "nobody")]
        [Arguments(null, "root", "http", "nobody")]
        [Arguments("Error: user whoever doesn't exist.", "whoever")]
        [Arguments("Error: user whoever doesn't exist.", "whoever", "nobody")]
        [Arguments("Error: user whoever doesn't exist.", "nobody", "whoever", "http")]
        public async Task Save_Clean_OnlineConfig_ForSpecifiedUsers(string? expectedResult, params string[] selectedUsernames)
        {
            var settings = new Settings();
            var directory = settings.OnlineConfigOutputDirectory;
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

            await Assert.That(genResult).IsEqualTo(expectedResult);
            if (expectedResult is null)
            {
                await Assert.That(Directory.Exists(directory)).IsTrue();
                var expectedFileCount = selectedUsernames.Length;
                var fileCount = Directory.GetFiles(directory).Length;
                await Assert.That(fileCount).IsEqualTo(expectedFileCount);
            }

            // Clean
            SIP008StaticGen.Remove(users, settings, selectedUsernames[0]);

            if (expectedResult is null)
            {
                await Assert.That(Directory.Exists(directory)).IsTrue();
                var expectedFileCount = selectedUsernames.Length - 1;
                var fileCount = Directory.GetFiles(directory).Length;
                await Assert.That(fileCount).IsEqualTo(expectedFileCount);
            }

            // Delete working directory.
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }
}