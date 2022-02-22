using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Manager;

internal interface IManagerApiTransport
{
    (byte[] buf, int bytesReceived) SendReceive(ReadOnlySpan<byte> request, int initRecvBufSize = 4096);
    Task<(byte[] buf, int bytesReceived)> SendReceiveAsync(ReadOnlyMemory<byte> request, int initRecvBufSize = 4096, CancellationToken cancellationToken = default);
}
