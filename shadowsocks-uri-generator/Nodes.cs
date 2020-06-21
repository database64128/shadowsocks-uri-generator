using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace shadowsocks_uri_generator
{
    /// <summary>
    /// The class for storing node information in Nodes.json
    /// </summary>
    public class Nodes
    {
        /// <summary>
        /// The group dictionary.
        /// key is group name.
        /// value is group info.
        /// </summary>
        public Dictionary<string, Group> Groups { get; set; }

        public Nodes()
        {
            Groups = new Dictionary<string, Group>();
        }

        /// <summary>
        /// Adds new node groups to the group dictionary.
        /// </summary>
        /// <param name="groups">The list of groups to be added.</param>
        /// <returns>A List of groups successfully added.</returns>
        public List<string> AddGroups(string[] groups)
        {
            List<string> addedGroups = new List<string>();

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
        /// <returns>0 for success. -1 for non-existing group, duplicated node, invalid port string.</returns>
        public int AddNodeToGroup(string group, string node, string host, string portString)
        {
            if (int.TryParse(portString, out int port))
                return AddNodeToGroup(group, node, host, port);
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
        /// <returns>0 for success. -1 for non-existing group, duplicated node, bad port range.</returns>
        public int AddNodeToGroup(string group, string node, string host, int port)
        {
            if (Groups.TryGetValue(group, out Group? targetGroup))
            {
                return targetGroup.AddNode(node, host, port);
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
    }

    /// <summary>
    /// Nodes in a group share the same credential for a user.
    /// They may provide different credentials for different users.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// The Node Dictionary.
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
            NodeDict = new Dictionary<string, Node>();
        }

        /// <summary>
        /// Adds a node to NodeDict
        /// </summary>
        /// <param name="name">Node name.</param>
        /// <param name="host">Node's host</param>
        /// <param name="port">Node's port number</param>
        /// <returns>0 for success. -1 for duplicated name.</returns>
        public int AddNode(string name, string host, int port)
        {
            if (!NodeDict.ContainsKey(name))
            {
                var node = new Node(host, port);
                NodeDict.Add(name, node);
                return 0;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Removes nodes from NodeDict.
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
        public string Host { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// Parameterless constructor for System.Text.Json
        /// </summary>
        public Node()
        {
            Host = "";
        }

        public Node(string _host, int _port)
        {
            Host = _host;
            Port = _port;
        }
    }

    public static class NodesJson
    {
        public static async Task<Nodes> LoadNodesAsync()
        {
            Nodes nodes;

            if (!File.Exists("Nodes.json"))
            {
                nodes = new Nodes();
                await SaveNodesAsync(nodes);
                return nodes;
            }

            using FileStream nodesJsonFile = new FileStream("Nodes.json", FileMode.Open);
            nodes = await JsonSerializer.DeserializeAsync<Nodes>(nodesJsonFile);
            return nodes;
        }

        public static async Task SaveNodesAsync(Nodes nodes)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            using FileStream nodesJsonFile = new FileStream("Nodes.json", FileMode.Create);
            await JsonSerializer.SerializeAsync(nodesJsonFile, nodes, jsonSerializerOptions);
        }
    }
}
