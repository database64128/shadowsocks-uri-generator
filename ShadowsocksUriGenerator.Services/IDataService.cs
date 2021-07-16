using Microsoft.Extensions.Hosting;

namespace ShadowsocksUriGenerator.Services
{
    public interface IDataService : IHostedService
    {
        public Users UsersData { get; }
        public Nodes NodesData { get; }
        public Settings SettingsData { get; }
    }
}
