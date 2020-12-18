using System;

namespace ShadowsocksUriGenerator.Outline
{
    public class AccessKey : IEquatable<AccessKey>
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

        public bool Equals(AccessKey? other)
            => Id == other?.Id
            && Name == other.Name
            && Password == other.Password
            && Port == other.Port
            && Method == other.Method
            && AccessUrl == other.AccessUrl;

        public override bool Equals(object? obj) => Equals(obj as AccessKey);

        public override int GetHashCode() => base.GetHashCode();
    }
}
