namespace ShadowsocksUriGenerator.Outline
{
    /// <summary>
    /// The mutable record type that stores an Outline access key.
    /// It's mutable so it can be atomically updated.
    /// </summary>
    public record OutlineAccessKey
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
        public int Port { get; set; }
        public string Method { get; set; } = "";
        public OutlineDataLimit? DataLimit { get; set; }
        public string AccessUrl { get; set; } = "";
    }
}
