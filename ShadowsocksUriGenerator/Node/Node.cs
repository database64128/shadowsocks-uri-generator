using System;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// Stores node's host and port.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the UUID of the node.
        /// </summary>
        public string Uuid { get; set; } = Guid.NewGuid().ToString();
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public string? Plugin { get; set; }
        public string? PluginOpts { get; set; }

        /// <summary>
        /// Gets or sets whether the node is deactivated.
        /// Defaults to false, or activated.
        /// When set to true, the node is excluded from delivery.
        /// </summary>
        public bool Deactivated { get; set; }

        /// <summary>
        /// Gets or sets the node's owner.
        /// </summary>
        public string? OwnerUuid { get; set; }

        /// <summary>
        /// Gets or sets the node's tags.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Node()
        {
        }

        public Node(string host, int port, string? plugin = null, string? pluginOpts = null, string? ownerUuid = null, params string[] tags)
        {
            Host = host;
            Port = port;
            Plugin = plugin;
            PluginOpts = pluginOpts;
            OwnerUuid = ownerUuid;
            Tags.AddRange(tags);
        }
    }
}
