namespace ShadowsocksUriGenerator.SSMv1;

public class SSMv1ApiException : Exception
{
    public SSMv1ApiException()
    { }

    public SSMv1ApiException(string? message) : base(message)
    { }

    public SSMv1ApiException(string? message, Exception? innerException) : base(message, innerException)
    { }

    public SSMv1ApiException(SSMv1Error? error) : base(error?.Error)
    { }
}
