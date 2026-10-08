using ShadowsocksUriGenerator.Protocols.Shadowsocks;

namespace ShadowsocksUriGenerator.Tests;

public class UriTests
{
    [Test]
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // domain name
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "1.1.1.1", 853, "", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@1.1.1.1:853/")] // IPv4
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "2001:db8:85a3::8a2e:370:7334", 8388, "", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@[2001:db8:85a3::8a2e:370:7334]:8388/")] // IPv6
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#GitHub")] // fragment
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "👩‍💻", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#%F0%9F%91%A9%E2%80%8D%F0%9F%92%BB")] // fragment
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin")] // pluginName
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, "1.0", null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // pluginVersion
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, "server;tls;host=github.com", null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // pluginOptions
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // pluginArguments
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0")] // pluginName + pluginVersion
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, "server;tls;host=github.com", null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com")] // pluginName + pluginOptions
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginArguments=-vvvvvv")] // pluginName + pluginArguments
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", "server;tls;host=github.com", null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0")] // pluginName + pluginVersion + pluginOptions
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0&pluginArguments=-vvvvvv")] // pluginName + pluginVersion + pluginArguments
    [Arguments("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0&pluginArguments=-vvvvvv#GitHub")] // fragment + pluginName + pluginVersion + pluginOptions + pluginArguments
    public async Task Server_ToUrl(string method, string password, string host, int port, string fragment, string? pluginName, string? pluginVersion, string? pluginOptions, string? pluginArguments, string expectedSSUri)
    {
        var serverConfig = new ShadowsocksServerConfig()
        {
            UserPSK = password,
            Method = method,
            Host = host,
            Port = port,
            Name = fragment,
            PluginName = pluginName,
            PluginVersion = pluginVersion,
            PluginOptions = pluginOptions,
            PluginArguments = pluginArguments,
        };

        var ssUriString = serverConfig.ToUri().AbsoluteUri;

        await Assert.That(ssUriString).IsEqualTo(expectedSSUri);
    }

    [Test]
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null)] // domain name
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@1.1.1.1:853/", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "1.1.1.1", 853, "", null, null, null, null)] // IPv4
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@[2001:db8:85a3::8a2e:370:7334]:8388/", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "2001:db8:85a3::8a2e:370:7334", 8388, "", null, null, null, null)] // IPv6
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#GitHub", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", null, null, null, null)] // fragment
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#%F0%9F%91%A9%E2%80%8D%F0%9F%92%BB", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "👩‍💻", null, null, null, null)] // fragment
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, null)] // pluginName
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?pluginVersion=1.0", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null)] // pluginVersion
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?pluginArguments=-vvvvvv", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null)] // pluginArguments
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, null)] // pluginName + pluginVersion
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, "server;tls;host=github.com", null)] // pluginName + pluginOptions
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginArguments=-vvvvvv", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, "-vvvvvv")] // pluginName + pluginArguments
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", "server;tls;host=github.com", null)] // pluginName + pluginVersion + pluginOptions
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0&pluginArguments=-vvvvvv", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, "-vvvvvv")] // pluginName + pluginVersion + pluginArguments
    [Arguments("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0&pluginArguments=-vvvvvv#GitHub", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv")] // fragment + pluginName + pluginVersion + pluginOptions + pluginArguments
    [Arguments("ss://chacha20-ietf-poly1305:6%25m8D9aMB5bA%25a4%25@github.com:443/", true, "chacha20-ietf-poly1305", "6%m8D9aMB5bA%a4%", "github.com", 443, "", null, null, null, null)] // userinfo parsing
    [Arguments("ss://aes-256-gcm:bpNgk%2AJ3kaAYyxHE@github.com:443/", true, "aes-256-gcm", "bpNgk*J3kaAYyxHE", "github.com", 443, "", null, null, null, null)] // userinfo parsing
    [Arguments("ss://aes-128-gcm:vAAn%268kR%3A%24iAE4@github.com:443/", true, "aes-128-gcm", "vAAn&8kR:$iAE4", "github.com", 443, "", null, null, null, null)] // userinfo parsing
    [Arguments("ss://YWVzLTI1Ni1nY206d0xoTjJTVFpAZ2l0aHViLmNvbTo0NDM", false, "", "", "", 0, "", null, null, null, null)] // unsupported legacy URL
    [Arguments("ss://YWVzLTI1Ni1nY206d0xoTjJTVFpAZ2l0aHViLmNvbTo0NDM#some-legacy-url", false, "", "", "", 0, "", null, null, null, null)] // unsupported legacy URL with fragment
    [Arguments("ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/", false, "", "", "", 0, "", null, null, null, null)] // unsupported legacy URL with base64url-encoded userinfo
    [Arguments("https://github.com/", false, "", "", "", 0, "", null, null, null, null)] // non-Shadowsocks URL
    public async Task Server_TryParse(string ssUrl, bool expectedResult, string expectedMethod, string expectedPassword, string expectedHost, int expectedPort, string expectedFragment, string? expectedPluginName, string? expectedPluginVersion, string? expectedPluginOptions, string? expectedPluginArguments)
    {
        var result = ShadowsocksServerConfig.TryParse(ssUrl, out var server);

        await Assert.That(result).IsEqualTo(expectedResult);
        if (result)
        {
            await Assert.That(server!.GetPassword()).IsEqualTo(expectedPassword);
            await Assert.That(server.Method).IsEqualTo(expectedMethod);
            await Assert.That(server.Host).IsEqualTo(expectedHost);
            await Assert.That(server.Port).IsEqualTo(expectedPort);
            await Assert.That(server.Name).IsEqualTo(expectedFragment);
            await Assert.That(server.PluginName).IsEqualTo(expectedPluginName);
            await Assert.That(server.PluginVersion).IsEqualTo(expectedPluginVersion);
            await Assert.That(server.PluginOptions).IsEqualTo(expectedPluginOptions);
            await Assert.That(server.PluginArguments).IsEqualTo(expectedPluginArguments);
        }
    }
}