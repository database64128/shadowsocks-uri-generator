using Microsoft.Extensions.Logging;
using ShadowsocksUriGenerator.Data;
using System.Threading.Channels;

namespace ShadowsocksUriGenerator.Services;

public sealed class DataService(ILogger<DataService> logger) : IDataService, IDisposable
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
                    logger.LogTrace("Event raised for unknown file {Name}", e.Name);
                    return;
            }

            if (!channel.Writer.TryWrite(e))
            {
                logger.LogError("Failed to write event of file {Name} to channel", e.Name);
            }
        }

        _watcher.EnableRaisingEvents = true;

        logger.LogInformation("Started file watcher watching Users.json, Nodes.json, Settings.json");

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

        logger.LogInformation("Stopped file watcher");

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
                logger.LogWarning("File {Name} was deleted!", e.Name);
                continue;
            }

            await loadAsync(cancellationToken);

            logger.LogInformation("Reloaded {Name} on {ChangeType} event", e.Name, e.ChangeType);
        }
    }

    private async Task LoadUsersAsync(CancellationToken cancellationToken = default)
    {
        var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
        if (loadUsersErrMsg is not null)
        {
            logger.LogError("Failed to load user data: {Error}", loadUsersErrMsg);
            return;
        }
        UsersData = users;
    }

    private async Task LoadNodesAsync(CancellationToken cancellationToken = default)
    {
        var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
        if (loadNodesErrMsg is not null)
        {
            logger.LogError("Failed to load node data: {Error}", loadNodesErrMsg);
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
            logger.LogError("Failed to load settings: {Error}", loadSettingsErrMsg);
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
}
