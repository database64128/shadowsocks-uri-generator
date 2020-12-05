using System;

namespace ShadowsocksUriGenerator.Outline
{
    public class AccessKey
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Method { get; set; }
        public string AccessUrl { get; set; }

        public AccessKey()
        {
            Id = new Guid().ToString();
            Name = "";
            Password = "";
            Port = 0;
            Method = "";
            AccessUrl = "";
        }
    }
}
