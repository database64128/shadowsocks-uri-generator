using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator
{
    /// <summary>
    /// The class for storing node information in Nodes.json
    /// </summary>
    public class Nodes
    {
        /// <summary>
        /// Gets the default configuration version
        /// used by this version of the app.
        /// </summary>
        public static int DefaultVersion => 2;
        
        /// <summary>
        /// Gets or sets the configuration version number.
        /// 0 for the legacy config version
        /// without a version number property.
        /// Newer config versions start from 1.
        /// Update if older config is present.
        /// Throw error if config is newer than supported.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the group dictionary.
        /// key is group name.
        /// value is group info.
        /// </summary>
        public Dictionary<string, Group> Groups { get; set; }

        public Nodes()
        {
            Version = DefaultVersion;
            Groups = new();
        }

        /// <summary>
        /// Adds new node groups to the group dictionary.
        /// </summary>
        /// <param name="groups">The list of groups to be added.</param>
        /// <returns>A List of groups successfully added.</returns>
        public List<string> AddGroups(string[] groups)
        {
            List<string> addedGroups = new();

            foreach (var group in groups)
                if (!Groups.ContainsKey(group))
                {
                    Groups.Add(group, new Group());
                    addedGroups.Add(group);
                }

            return addedGroups;
        }

        /// <summary>
        /// Removes groups from the group dictionary.
        /// </summary>
        /// <param name="groups">The list of groups to be removed.</param>
        public void RemoveGroups(string[] groups)
        {
            foreach (var group in groups)
                Groups.Remove(group);
        }

        /// <summary>
        /// Adds a node to a node group.
        /// </summary>
        /// <param name="group">Destination group name.</param>
        /// <param name="node">Node name</param>
        /// <param name="host">Node's host</param>
        /// <param name="portString">Node's port string to be parsed.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>0 for success. -1 for non-existing group, duplicated node, invalid port string.</returns>
        public int AddNodeToGroup(string group, string node, string host, string portString, string? plugin = null, string? pluginOpts = null)
        {
            if (int.TryParse(portString, out int port))
                return AddNodeToGroup(group, node, host, port, plugin, pluginOpts);
            else
                return -1;
        }


        /// <summary>
        /// Adds a node to a node group.
        /// </summary>
        /// <param name="group">Destination group name.</param>
        /// <param name="node">Node name</param>
        /// <param name="host">Node's host</param>
        /// <param name="port">Node's port number.</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>0 for success. -1 for non-existing group, duplicated node, bad port range.</returns>
        public int AddNodeToGroup(string group, string node, string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            if (Groups.TryGetValue(group, out Group? targetGroup))
            {
                return targetGroup.AddNode(node, host, port, plugin, pluginOpts);
            }
            else
                return -1;
        }

        /// <summary>
        /// Removes nodes from the node group.
        /// </summary>
        /// <param name="group">Group name to remove nodes from.</param>
        /// <param name="nodes">Node name to be removed.</param>
        /// <returns>0 for success or found target group. -1 for non-existing group.</returns>
        public int RemoveNodesFromGroup(string group, string[] nodes)
        {
            if (Groups.TryGetValue(group, out Group? targetGroup))
            {
                targetGroup.RemoveNodes(nodes);
                return 0;
            }
            else
                return -1;
        }

        /// <summary>
        /// Loads nodes from Nodes.json.
        /// </summary>
        /// <returns>A <see cref="Nodes"/> object.</returns>
        public static async Task<Nodes> LoadNodesAsync()
        {
            var nodes = await Utilities.LoadJsonAsync<Nodes>("Nodes.json", Utilities.commonJsonDeserializerOptions);
            if (nodes.Version != DefaultVersion)
            {
                UpdateNodes(ref nodes);
                await SaveNodesAsync(nodes);
            }
            return nodes;
        }

        /// <summary>
        /// Saves nodes to Nodes.json.
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object to save.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static async Task SaveNodesAsync(Nodes nodes)
            => await Utilities.SaveJsonAsync("Nodes.json", nodes, Utilities.commonJsonSerializerOptions);

        /// <summary>
        /// Updates the nodes version.
        /// </summary>
        /// <param name="nodes">The <see cref="Nodes"/> object to update.</param>
        public static void UpdateNodes(ref Nodes nodes)
        {
            switch (nodes.Version)
            {
                case 0: // generate UUID for each node
                    // already generated by the constructor
                    nodes.Version++;
                    goto case 1;
                case 1: // nullify empty Plugin and PluginOpts strings
                    foreach (var groupEntry in nodes.Groups)
                        foreach (var nodeEntry in groupEntry.Value.NodeDict)
                        {
                            var node = nodeEntry.Value;
                            if (string.IsNullOrEmpty(node.Plugin))
                                node.Plugin = null;
                            if (string.IsNullOrEmpty(node.PluginOpts))
                                node.PluginOpts = null;
                        }
                    nodes.Version++;
                    goto default; // go to the next update path
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Nodes in a group share the same credential for a user.
    /// They may provide different credentials for different users.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Gets or sets the Node Dictionary.
        /// key is node name.
        /// value is node info.
        /// 
        /// address = hostname + ":" + port.
        /// Examples: 
        /// foo.bar:80
        /// [2001::1]:443
        /// 1.1.1.1:853
        /// </summary>
        [JsonPropertyName("Nodes")]
        public Dictionary<string, Node> NodeDict { get; set; }

        public Group()
        {
            NodeDict = new();
        }

        /// <summary>
        /// Adds a node to <see cref="NodeDict"/>
        /// </summary>
        /// <param name="name">Node name.</param>
        /// <param name="host">Node's host</param>
        /// <param name="port">Node's port number</param>
        /// <param name="plugin">Optional. Plugin binary name.</param>
        /// <param name="pluginOpts">Optional. Plugin options.</param>
        /// <returns>0 for success. -1 for duplicated name.</returns>
        public int AddNode(string name, string host, int port, string? plugin = null, string? pluginOpts = null)
        {
            if (!NodeDict.ContainsKey(name))
            {
                var node = new Node(host, port, plugin, pluginOpts);
                NodeDict.Add(name, node);
                return 0;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Removes nodes from <see cref="NodeDict"/>.
        /// </summary>
        /// <param name="nodes">The list of nodes to be removed.</param>
        public void RemoveNodes(string[] nodes)
        {
            foreach (var node in nodes)
                NodeDict.Remove(node);
        }
    }

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
