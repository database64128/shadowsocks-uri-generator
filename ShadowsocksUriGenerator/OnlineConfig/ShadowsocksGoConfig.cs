using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class ShadowsocksGoConfig
{
    public IEnumerable<ShadowsocksGoClientConfig>? Clients { get; set; }
}
