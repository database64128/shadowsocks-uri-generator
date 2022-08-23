using ShadowsocksUriGenerator.Protocols.Shadowsocks;
using Xunit;

namespace ShadowsocksUriGenerator.Tests;

public class UriTests
{
    [Theory]
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // domain name
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "1.1.1.1", 853, "", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@1.1.1.1:853/")] // IPv4
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "2001:db8:85a3::8a2e:370:7334", 8388, "", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@[2001:db8:85a3::8a2e:370:7334]:8388/")] // IPv6
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#GitHub")] // fragment
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "👩‍💻", null, null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#%F0%9F%91%A9%E2%80%8D%F0%9F%92%BB")] // fragment
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin")] // pluginName
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, "1.0", null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // pluginVersion
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, "server;tls;host=github.com", null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // pluginOptions
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/")] // pluginArguments
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0")] // pluginName + pluginVersion
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, "server;tls;host=github.com", null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com")] // pluginName + pluginOptions
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginArguments=-vvvvvv")] // pluginName + pluginArguments
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", "server;tls;host=github.com", null, "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0")] // pluginName + pluginVersion + pluginOptions
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0&pluginArguments=-vvvvvv")] // pluginName + pluginVersion + pluginArguments
    [InlineData("2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv", "ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0&pluginArguments=-vvvvvv#GitHub")] // fragment + pluginName + pluginVersion + pluginOptions + pluginArguments
    public void Server_ToUrl(string method, string password, string host, int port, string fragment, string? pluginName, string? pluginVersion, string? pluginOptions, string? pluginArguments, string expectedSSUri)
    {
        IShadowsocksServerConfig serverConfig = new ShadowsocksServerConfig()
        {
            Password = password,
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

        Assert.Equal(expectedSSUri, ssUriString);
    }

    [Theory]
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null)] // domain name
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@1.1.1.1:853/", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "1.1.1.1", 853, "", null, null, null, null)] // IPv4
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@[2001:db8:85a3::8a2e:370:7334]:8388/", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "2001:db8:85a3::8a2e:370:7334", 8388, "", null, null, null, null)] // IPv6
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#GitHub", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", null, null, null, null)] // fragment
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/#%F0%9F%91%A9%E2%80%8D%F0%9F%92%BB", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "👩‍💻", null, null, null, null)] // fragment
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, null)] // pluginName
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?pluginVersion=1.0", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null)] // pluginVersion
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?pluginArguments=-vvvvvv", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", null, null, null, null)] // pluginArguments
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, null)] // pluginName + pluginVersion
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, "server;tls;host=github.com", null)] // pluginName + pluginOptions
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginArguments=-vvvvvv", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", null, null, "-vvvvvv")] // pluginName + pluginArguments
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", "server;tls;host=github.com", null)] // pluginName + pluginVersion + pluginOptions
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin&pluginVersion=1.0&pluginArguments=-vvvvvv", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "", "v2ray-plugin", "1.0", null, "-vvvvvv")] // pluginName + pluginVersion + pluginArguments
    [InlineData("ss://2022-blake3-aes-256-gcm:z7by%2FoMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw%3D@github.com:443/?plugin=v2ray-plugin%3Bserver%3Btls%3Bhost%3Dgithub.com&pluginVersion=1.0&pluginArguments=-vvvvvv#GitHub", true, "2022-blake3-aes-256-gcm", "z7by/oMFjG7sunqq2q69hlGynqkrgk9bCKoWp29zhgw=", "github.com", 443, "GitHub", "v2ray-plugin", "1.0", "server;tls;host=github.com", "-vvvvvv")] // fragment + pluginName + pluginVersion + pluginOptions + pluginArguments
    [InlineData("ss://chacha20-ietf-poly1305:6%25m8D9aMB5bA%25a4%25@github.com:443/", true, "chacha20-ietf-poly1305", "6%m8D9aMB5bA%a4%", "github.com", 443, "", null, null, null, null)] // userinfo parsing
    [InlineData("ss://aes-256-gcm:bpNgk%2AJ3kaAYyxHE@github.com:443/", true, "aes-256-gcm", "bpNgk*J3kaAYyxHE", "github.com", 443, "", null, null, null, null)] // userinfo parsing
    [InlineData("ss://aes-128-gcm:vAAn%268kR%3A%24iAE4@github.com:443/", true, "aes-128-gcm", "vAAn&8kR:$iAE4", "github.com", 443, "", null, null, null, null)] // userinfo parsing
    [InlineData("ss://YWVzLTI1Ni1nY206d0xoTjJTVFpAZ2l0aHViLmNvbTo0NDM", false, "", "", "", 0, "", null, null, null, null)] // unsupported legacy URL
    [InlineData("ss://YWVzLTI1Ni1nY206d0xoTjJTVFpAZ2l0aHViLmNvbTo0NDM#some-legacy-url", false, "", "", "", 0, "", null, null, null, null)] // unsupported legacy URL with fragment
    [InlineData("ss://YWVzLTI1Ni1nY206d0xoTjJTVFo@github.com:443/", false, "", "", "", 0, "", null, null, null, null)] // unsupported legacy URL with base64url-encoded userinfo
    [InlineData("https://github.com/", false, "", "", "", 0, "", null, null, null, null)] // non-Shadowsocks URL
    public void Server_TryParse(string ssUrl, bool expectedResult, string expectedMethod, string expectedPassword, string expectedHost, int expectedPort, string expectedFragment, string? expectedPluginName, string? expectedPluginVersion, string? expectedPluginOptions, string? expectedPluginArguments)
    {
        var result = ShadowsocksServerConfig.TryParse(ssUrl, out var server);

        Assert.Equal(expectedResult, result);
        if (result)
        {
            Assert.Equal(expectedPassword, server!.Password);
            Assert.Equal(expectedMethod, server.Method);
            Assert.Equal(expectedHost, server.Host);
            Assert.Equal(expectedPort, server.Port);
            Assert.Equal(expectedFragment, server.Name);
            Assert.Equal(expectedPluginName, server.PluginName);
            Assert.Equal(expectedPluginVersion, server.PluginVersion);
            Assert.Equal(expectedPluginOptions, server.PluginOptions);
            Assert.Equal(expectedPluginArguments, server.PluginArguments);
        }
    }
}
