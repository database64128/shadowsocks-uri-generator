using Shadowsocks.Models;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.OnlineConfig
{
    public class OOCv1ShadowsocksServer : Shadowsocks.OnlineConfig.OOCv1.OOCShadowsocksServer
    {
        /// <summary>
        /// Gets or sets the owner of the server.
        /// </summary>
        public string? Owner { get; set; } = "";

        /// <summary>
        /// Gets or sets the list of annotated tags.
        /// </summary>
        public List<string>? Tags { get; set; }

        public OOCv1ShadowsocksServer()
        {
        }

        public OOCv1ShadowsocksServer(IServer server) : base(server)
        {
        }
    }
}
