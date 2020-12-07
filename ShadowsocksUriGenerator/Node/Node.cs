using System;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// Stores node's host and port.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the UUID of the node.
        /// Used for online configuration delivery (SIP008).
        /// </summary>
        public string Uuid { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string? Plugin { get; set; }
        public string? PluginOpts { get; set; }

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Node()
        {
            Uuid = Guid.NewGuid().ToString();
            Host = "";
            Port = 0;
        }

        public Node(string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            Uuid = Guid.NewGuid().ToString();
            Host = host;
            Port = port;
            Plugin = plugin;
            PluginOpts = pluginOpts;
        }
    }
}
