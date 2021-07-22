using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig
{
    public class SIP008Config
    {
        /// <inheritdoc cref="Shadowsocks.OnlineConfig.SIP008.SIP008Config.Version"/>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public string? Id { get; set; }

        /// <inheritdoc cref="Shadowsocks.OnlineConfig.SIP008.SIP008Config.BytesUsed"/>
        public ulong? BytesUsed { get; set; }

        /// <inheritdoc cref="Shadowsocks.OnlineConfig.SIP008.SIP008Config.BytesRemaining"/>
        public ulong? BytesRemaining { get; set; }

        /// <inheritdoc cref="Shadowsocks.OnlineConfig.SIP008.SIP008Config.Servers"/>
        public List<SIP008Server> Servers { get; set; } = new();
    }
}
