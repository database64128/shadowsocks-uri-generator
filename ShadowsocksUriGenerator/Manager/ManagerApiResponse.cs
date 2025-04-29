namespace ShadowsocksUriGenerator.Manager;

public record ManagerApiResponse(string Content)
{
    public bool IsOk => Content.Equals("ok", StringComparison.OrdinalIgnoreCase);
}
