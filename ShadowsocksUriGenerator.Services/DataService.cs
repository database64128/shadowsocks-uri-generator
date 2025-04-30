using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Data;
using System.Threading.Channels;

namespace ShadowsocksUriGenerator.Services;

public sealed partial class DataService(ILogger<DataService> logger) : IDataService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _cts;
    private Task? _updateTask;

    public Users UsersData { get; private set; } = new();

    public Nodes NodesData { get; private set; } = new();

    public Settings SettingsData { get; private set; } = new();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Channel<FileSystemEventArgs> usersChannel = CreateChannel();
        Channel<FileSystemEventArgs> nodesChannel = CreateChannel();
        Channel<FileSystemEventArgs> settingsChannel = CreateChannel();

        _watcher = new(".", "*.json");

        _watcher.Changed += OnEvent;
        _watcher.Created += OnEvent;
        _watcher.Deleted += OnEvent;
        _watcher.Renamed += OnEvent;

        void OnEvent(object? sender, FileSystemEventArgs e)
        {
            Channel<FileSystemEventArgs> channel;

            switch (e.Name)
            {
                case "Users.json":
                    channel = usersChannel;
                    break;
                case "Nodes.json":
                    channel = nodesChannel;
                    break;
                case "Settings.json":
                    channel = settingsChannel;
                    break;
                default:
                    LogUnknownFileEvent(e.Name);
                    return;
            }

            if (!channel.Writer.TryWrite(e))
            {
                LogFailedChannelWrite(e.Name);
            }
        }

        _watcher.EnableRaisingEvents = true;

        LogStartedFileWatcher();

        await LoadUsersAsync(cancellationToken);
        await LoadNodesAsync(cancellationToken);
        await LoadSettingsAsync(cancellationToken);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _updateTask = Task.WhenAll(
            UpdateDataAsync(usersChannel.Reader, LoadUsersAsync, _cts.Token),
            UpdateDataAsync(nodesChannel.Reader, LoadNodesAsync, _cts.Token),
            UpdateDataAsync(settingsChannel.Reader, LoadSettingsAsync, _cts.Token));
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_watcher is null || _cts is null || _updateTask is null)
        {
            return;
        }

        _watcher.EnableRaisingEvents = false;

        LogStoppedFileWatcher();

        try
        {
            _cts.Cancel();
        }
        finally
        {
            await _updateTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    private static Channel<FileSystemEventArgs> CreateChannel() => Channel.CreateBounded<FileSystemEventArgs>(new BoundedChannelOptions(1)
    {
        SingleReader = true,
        AllowSynchronousContinuations = true,
        FullMode = BoundedChannelFullMode.DropOldest,
    });

    private static readonly TimeSpan s_debounceInterval = TimeSpan.FromSeconds(5);

    private async Task UpdateDataAsync(
        ChannelReader<FileSystemEventArgs> reader,
        Func<CancellationToken, Task> loadAsync,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            FileSystemEventArgs e = await reader.ReadAsync(cancellationToken);

            // Debounce for 5 seconds.
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            while (true)
            {
                cts.CancelAfter(s_debounceInterval);

                try
                {
                    e = await reader.ReadAsync(cts.Token);

                    if (!cts.TryReset())
                    {
                        cts.Dispose();
                        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            cts.Dispose();

            if (e.Name is null)
                continue;

            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                LogFileDeletedEvent(e.Name);
                continue;
            }

            await loadAsync(cancellationToken);

            LogReloadedFileOnEvent(e.Name, e.ChangeType);
        }
    }

    private async Task LoadUsersAsync(CancellationToken cancellationToken = default)
    {
        var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
        if (loadUsersErrMsg is not null)
        {
            LogFailedToLoadFile("Users.json", loadUsersErrMsg);
            return;
        }
        UsersData = users;
    }

    private async Task LoadNodesAsync(CancellationToken cancellationToken = default)
    {
        var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
        if (loadNodesErrMsg is not null)
        {
            LogFailedToLoadFile("Nodes.json", loadNodesErrMsg);
            return;
        }
        NodesData.Dispose();
        NodesData = loadedNodes;
    }

    private async Task LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
        if (loadSettingsErrMsg is not null)
        {
            LogFailedToLoadFile("Settings.json", loadSettingsErrMsg);
            return;
        }
        SettingsData = settings;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _cts?.Dispose();
        NodesData.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "Event raised for unknown file {Name}")]
    private partial void LogUnknownFileEvent(string? name);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to write event of file {Name} to channel")]
    private partial void LogFailedChannelWrite(string? name);

    [LoggerMessage(Level = LogLevel.Information, Message = "Started file watcher watching Users.json, Nodes.json, Settings.json")]
    private partial void LogStartedFileWatcher();

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopped file watcher")]
    private partial void LogStoppedFileWatcher();

    [LoggerMessage(Level = LogLevel.Warning, Message = "File {Name} was deleted!")]
    private partial void LogFileDeletedEvent(string? name);

    [LoggerMessage(Level = LogLevel.Information, Message = "Reloaded {Name} on {ChangeType} event")]
    private partial void LogReloadedFileOnEvent(string? name, WatcherChangeTypes changeType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to load {Name}: {Error}")]
    private partial void LogFailedToLoadFile(string name, string error);
}
