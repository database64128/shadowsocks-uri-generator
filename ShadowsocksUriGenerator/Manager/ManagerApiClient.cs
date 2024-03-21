using ShadowsocksUriGenerator.OnlineConfig;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Manager;

public class ManagerApiClient : IManagerApiClient, IDisposable
{
    private readonly ManagerApiTransport _transport;
    private bool disposedValue;

    public ManagerApiClient(UnixDomainSocketEndPoint endPoint) => _transport = new ManagerApiTransport(endPoint);

    public ManagerApiClient(IPEndPoint endPoint) => _transport = new ManagerApiTransport(endPoint);

    public ManagerApiResponse Add(ManagerApiAddRequest request)
    {
        var (buf, bytesReceived) = _transport.SendReceive(request.ToBytes(), 2);
        var response = Encoding.UTF8.GetString(buf.AsSpan()[..bytesReceived]);
        ArrayPool<byte>.Shared.Return(buf);
        return new(response);
    }

    public ManagerApiResponse Remove(ManagerApiRemoveRequest request)
    {
        var (buf, bytesReceived) = _transport.SendReceive(request.ToBytes(), 2);
        var response = Encoding.UTF8.GetString(buf.AsSpan()[..bytesReceived]);
        ArrayPool<byte>.Shared.Return(buf);
        return new(response);
    }

    public SIP008Config List()
    {
        var requestBytes = Encoding.UTF8.GetBytes("list");
        var (buf, bytesReceived) = _transport.SendReceive(requestBytes);
        SIP008Config? response = null;
        try
        {
            if (bytesReceived > 6)
            {
                response = JsonSerializer.Deserialize(buf.AsSpan()[6..bytesReceived], ManagerAPIJsonSerializerContext.Default.SIP008Config);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        return response ?? new();
    }

    public Dictionary<int, ulong> Ping()
    {
        var requestBytes = Encoding.UTF8.GetBytes("ping");
        var (buf, bytesReceived) = _transport.SendReceive(requestBytes);
        Dictionary<int, ulong>? response = null;
        try
        {
            if (bytesReceived > 6)
            {
                response = JsonSerializer.Deserialize(buf.AsSpan()[6..bytesReceived], ManagerAPIJsonSerializerContext.Default.DictionaryInt32UInt64);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        return response ?? [];
    }

    public Dictionary<int, ulong> Stat(Dictionary<int, ulong> request)
    {
        var requestBytes = MakeStatRequest(request);
        var (buf, bytesReceived) = _transport.SendReceive(requestBytes);
        Dictionary<int, ulong>? response = null;
        try
        {
            if (bytesReceived > 6)
            {
                response = JsonSerializer.Deserialize(buf.AsSpan()[6..bytesReceived], ManagerAPIJsonSerializerContext.Default.DictionaryInt32UInt64);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        return response ?? [];
    }

    public async Task<ManagerApiResponse> AddAsync(ManagerApiAddRequest request, CancellationToken cancellationToken = default)
    {
        var (buf, bytesReceived) = await _transport.SendReceiveAsync(request.ToBytes(), 2, cancellationToken);
        var response = Encoding.UTF8.GetString(buf.AsSpan()[..bytesReceived]);
        ArrayPool<byte>.Shared.Return(buf);
        return new(response);
    }

    public async Task<ManagerApiResponse> RemoveAsync(ManagerApiRemoveRequest request, CancellationToken cancellationToken = default)
    {
        var (buf, bytesReceived) = await _transport.SendReceiveAsync(request.ToBytes(), 2, cancellationToken);
        var response = Encoding.UTF8.GetString(buf.AsSpan()[..bytesReceived]);
        ArrayPool<byte>.Shared.Return(buf);
        return new(response);
    }

    public async Task<SIP008Config> ListAsync(CancellationToken cancellationToken = default)
    {
        var requestBytes = Encoding.UTF8.GetBytes("list");
        var (buf, bytesReceived) = await _transport.SendReceiveAsync(requestBytes, cancellationToken: cancellationToken);
        SIP008Config? response = null;
        try
        {
            if (bytesReceived > 6)
            {
                response = JsonSerializer.Deserialize(buf.AsSpan()[6..bytesReceived], ManagerAPIJsonSerializerContext.Default.SIP008Config);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        return response ?? new();
    }

    public async Task<Dictionary<int, ulong>> PingAsync(CancellationToken cancellationToken = default)
    {
        var requestBytes = Encoding.UTF8.GetBytes("ping");
        var (buf, bytesReceived) = await _transport.SendReceiveAsync(requestBytes, cancellationToken: cancellationToken);
        Dictionary<int, ulong>? response = null;
        try
        {
            if (bytesReceived > 6)
            {
                response = JsonSerializer.Deserialize(buf.AsSpan()[6..bytesReceived], ManagerAPIJsonSerializerContext.Default.DictionaryInt32UInt64);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        return response ?? [];
    }

    public async Task<Dictionary<int, ulong>> StatAsync(Dictionary<int, ulong> request, CancellationToken cancellationToken = default)
    {
        var requestBytes = MakeStatRequest(request);
        var (buf, bytesReceived) = await _transport.SendReceiveAsync(requestBytes, cancellationToken: cancellationToken);
        Dictionary<int, ulong>? response = null;
        try
        {
            if (bytesReceived > 6)
            {
                response = JsonSerializer.Deserialize(buf.AsSpan()[6..bytesReceived], ManagerAPIJsonSerializerContext.Default.DictionaryInt32UInt64);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
        return response ?? [];
    }

    private static byte[] MakeStatRequest(Dictionary<int, ulong> request)
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(request, ManagerAPIJsonSerializerContext.Default.DictionaryInt32UInt64);
        var requestBytes = ArrayPool<byte>.Shared.Rent(6 + jsonBytes.Length);
        _ = Encoding.UTF8.GetBytes("stat: ", requestBytes);
        jsonBytes.CopyTo(requestBytes, jsonBytes.Length);
        return requestBytes;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (_transport is IDisposable disposableTransport)
                {
                    disposableTransport.Dispose();
                }
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
