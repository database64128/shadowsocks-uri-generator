namespace ShadowsocksUriGenerator.Outline
{
    public record Name(string name);
    public record Hostname(string hostname);
    public record DataLimit(ulong bytes);
    public record Metrics(bool metricsEnabled);
    public record Port(int port);
}
