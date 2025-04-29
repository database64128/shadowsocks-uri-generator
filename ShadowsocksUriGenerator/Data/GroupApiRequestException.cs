namespace ShadowsocksUriGenerator.Data;

public class GroupApiRequestException : Exception
{
    public GroupApiRequestException()
    { }

    public GroupApiRequestException(string? message) : base(message)
    { }

    public GroupApiRequestException(string? message, Exception? innerException) : base(message, innerException)
    { }

    public GroupApiRequestException(string? groupName, string? message, Exception? innerException) : base(message, innerException)
    {
        GroupName = groupName;
    }

    public override string Message
    {
        get
        {
            string s = base.Message;
            if (GroupName is not null)
                s += Environment.NewLine + "Group: " + GroupName;
            return s;
        }
    }

    /// <summary>
    /// Gets the name of the group for which the API request failed.
    /// </summary>
    public string? GroupName { get; }
}
