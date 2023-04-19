using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.SSMv1;

[JsonSerializable(typeof(SSMv1Error))]
[JsonSerializable(typeof(SSMv1ServerInfo))]
[JsonSerializable(typeof(SSMv1Stats))]
[JsonSerializable(typeof(SSMv1UserCred))]
[JsonSerializable(typeof(SSMv1UserInfo))]
[JsonSerializable(typeof(SSMv1UserInfoList))]
[JsonSerializable(typeof(SSMv1UserDetails))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class SSMv1JsonSerializerContext : JsonSerializerContext
{
}
