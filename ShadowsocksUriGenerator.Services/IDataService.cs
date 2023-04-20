using Microsoft.Extensions.Hosting;
using ShadowsocksUriGenerator.Data;

namespace ShadowsocksUriGenerator.Services
{
    public interface IDataService : IHostedService
    {
        public Users UsersData { get; }
        public Nodes NodesData { get; }
        public Settings SettingsData { get; }
    }
}
