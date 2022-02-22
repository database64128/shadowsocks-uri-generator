using ShadowsocksUriGenerator.OnlineConfig;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Manager;

public interface IManagerApiClient
{
    ManagerApiResponse Add(ManagerApiAddRequest request);
    ManagerApiResponse Remove(ManagerApiRemoveRequest request);
    SIP008Config List();
    Dictionary<int, ulong> Ping();
    Dictionary<int, ulong> Stat(Dictionary<int, ulong> request);

    Task<ManagerApiResponse> AddAsync(ManagerApiAddRequest request, CancellationToken cancellationToken = default);
    Task<ManagerApiResponse> RemoveAsync(ManagerApiRemoveRequest request, CancellationToken cancellationToken = default);
    Task<SIP008Config> ListAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<int, ulong>> PingAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<int, ulong>> StatAsync(Dictionary<int, ulong> request, CancellationToken cancellationToken = default);
}
