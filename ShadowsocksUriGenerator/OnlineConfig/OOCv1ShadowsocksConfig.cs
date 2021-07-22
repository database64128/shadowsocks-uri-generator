using OpenOnlineConfig.v1;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig
{
    public class OOCv1ShadowsocksConfig : OOCv1ConfigBase
    {
        /// <inheritdoc cref="Shadowsocks.OnlineConfig.OOCv1.OOCConfigShadowsocks.Shadowsocks"/>
        public List<OOCv1ShadowsocksServer> Shadowsocks { get; set; } = new();

        /// <inheritdoc cref="Shadowsocks.OnlineConfig.OOCv1.OOCConfigShadowsocks.OOCConfigShadowsocks"/>
        public OOCv1ShadowsocksConfig() => Protocols.Add("shadowsocks");
    }
}
