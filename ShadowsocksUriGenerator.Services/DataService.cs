using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Services
{
    public class DataService : IDataService, IDisposable
    {
        private readonly ILogger<DataService> _logger;
        private readonly FileSystemWatcher _watcher;
        private bool disposedValue;
        private bool isStarted;

        public Users UsersData { get; private set; } = new();

        public Nodes NodesData { get; private set; } = new();

        public Settings SettingsData { get; private set; } = new();

        public DataService(ILogger<DataService> logger)
        {
            _logger = logger;
            _watcher = new(Utilities.configDirectory, "*.json");
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (isStarted)
                return;

            StartFileWatcher();

            var (users, loadUsersErrMsg) = await Users.LoadUsersAsync(cancellationToken);
            if (loadUsersErrMsg is not null)
            {
                _logger.LogError(loadUsersErrMsg);
            }
            else
            {
                UsersData = users;
                _logger.LogInformation("Loaded Users.json");
            }

            var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync(cancellationToken);
            if (loadNodesErrMsg is not null)
            {
                _logger.LogError(loadNodesErrMsg);
            }
            else
            {
                NodesData = loadedNodes;
                _logger.LogInformation("Loaded Nodes.json");
            }

            var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync(cancellationToken);
            if (loadSettingsErrMsg is not null)
            {
                _logger.LogError(loadSettingsErrMsg);
            }
            else
            {
                SettingsData = settings;
                _logger.LogInformation("Loaded Settings.json");
            }

            isStarted = true;

            _logger.LogInformation("Started data service asynchronously");
        }

        private void StartFileWatcher()
        {
            var changed = Observable.FromEventPattern<FileSystemEventArgs>(_watcher, "Changed");
            var created = Observable.FromEventPattern<FileSystemEventArgs>(_watcher, "Created");
            var deleted = Observable.FromEventPattern<FileSystemEventArgs>(_watcher, "Deleted");
            var renamed = Observable.FromEventPattern<FileSystemEventArgs>(_watcher, "Renamed");

            var observables = Observable.Merge(changed, created, deleted, renamed);

            observables.Where(x => x.EventArgs.Name is "Users.json")
                       .Throttle(TimeSpan.FromSeconds(5.0))
                       .Select(x => x.EventArgs)
                       .Subscribe(UpdateDataAsync);

            observables.Where(x => x.EventArgs.Name is "Nodes.json")
                       .Throttle(TimeSpan.FromSeconds(5.0))
                       .Select(x => x.EventArgs)
                       .Subscribe(UpdateDataAsync);

            observables.Where(x => x.EventArgs.Name is "Settings.json")
                       .Throttle(TimeSpan.FromSeconds(5.0))
                       .Select(x => x.EventArgs)
                       .Subscribe(UpdateDataAsync);

            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("Started file watcher watching Users.json, Nodes.json, Settings.json");
        }

        private async void UpdateDataAsync(FileSystemEventArgs e)
        {
            if (e.Name is null)
                return;

            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                _logger.LogWarning($"File {e.Name} was deleted!");
                return;
            }

            _logger.LogInformation($"Acting on {e.ChangeType} event on file {e.Name}");

            switch (e.Name)
            {
                case "Users.json":
                    {
                        var (users, loadUsersErrMsg) = await Users.LoadUsersAsync();
                        if (loadUsersErrMsg is not null)
                        {
                            _logger.LogError(loadUsersErrMsg);
                        }
                        else
                        {
                            UsersData = users;
                            _logger.LogInformation("Reloaded Users.json");
                        }

                        break;
                    }

                case "Nodes.json":
                    {
                        var (loadedNodes, loadNodesErrMsg) = await Nodes.LoadNodesAsync();
                        if (loadNodesErrMsg is not null)
                        {
                            _logger.LogError(loadNodesErrMsg);
                        }
                        else
                        {
                            NodesData = loadedNodes;
                            _logger.LogInformation("Reloaded Nodes.json");
                        }

                        break;
                    }

                case "Settings.json":
                    {
                        var (settings, loadSettingsErrMsg) = await Settings.LoadSettingsAsync();
                        if (loadSettingsErrMsg is not null)
                        {
                            _logger.LogError(loadSettingsErrMsg);
                        }
                        else
                        {
                            SettingsData = settings;
                            _logger.LogInformation("Reloaded Settings.json");
                        }

                        break;
                    }

                default:
                    throw new Exception($"Event on file {e.Name} is not properly handled!");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _watcher.EnableRaisingEvents = false;
            _logger.LogInformation("Stopped file watcher");
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _watcher.Dispose();
                    NodesData.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
