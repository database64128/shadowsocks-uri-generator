using Shadowsocks.Models;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig
{
    public class SIP008Server : Shadowsocks.OnlineConfig.SIP008.SIP008Server
    {
        /// <inheritdoc cref="OOCv1ShadowsocksServer.Owner"/>
        public string? Owner { get; set; } = "";

        /// <inheritdoc cref="OOCv1ShadowsocksServer.Tags"/>
        public List<string>? Tags { get; set; }

        public SIP008Server()
        {
        }

        public SIP008Server(IServer server) : base(server)
        {
        }
    }
}
