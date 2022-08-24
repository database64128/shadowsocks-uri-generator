using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig;

public class SingBoxConfig
{
    public IEnumerable<SingBoxOutboundConfig>? Outbounds { get; set; }
}
