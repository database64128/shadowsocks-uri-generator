using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowsocksUriGenerator.Manager;

internal class ManagerApiTransport : IManagerApiTransport, IDisposable
{
    private readonly Lock _locker = new();
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private readonly Socket _socket;
    private bool disposedValue;

    public ManagerApiTransport(UnixDomainSocketEndPoint endPoint)
    {
        _socket = new(AddressFamily.Unix, SocketType.Dgram, 0);
        _socket.Bind(endPoint);
        _socket.ReceiveTimeout = 1000;
    }

    public ManagerApiTransport(IPEndPoint endPoint, int receiveTimeoutMs = 5000)
    {
        _socket = new(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(endPoint);
        _socket.ReceiveTimeout = receiveTimeoutMs;
    }

    public (byte[] buf, int bytesReceived) SendReceive(ReadOnlySpan<byte> request, int initRecvBufSize = 4096)
    {
        lock (_locker)
        {
            _ = _socket.Send(request);

            var buf = ArrayPool<byte>.Shared.Rent(initRecvBufSize);
            var received = 0;

            while (true)
            {
                int n;
                try
                {
                    n = _socket.Receive(buf, received, buf.Length - received, SocketFlags.None);
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(buf);
                    throw;
                }
                received += n;

                if (received < buf.Length)
                    break;

                var newBuf = ArrayPool<byte>.Shared.Rent(buf.Length * 2);
                Buffer.BlockCopy(buf, 0, newBuf, 0, buf.Length);
                ArrayPool<byte>.Shared.Return(buf);
                buf = newBuf;
            }

            return (buf, received);
        }
    }

    public async Task<(byte[] buf, int bytesReceived)> SendReceiveAsync(ReadOnlyMemory<byte> request, int initRecvBufSize = 4096, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            _ = await _socket.SendAsync(request, SocketFlags.None, cancellationToken);

            var buf = ArrayPool<byte>.Shared.Rent(initRecvBufSize);
            var received = 0;
            var n = 0;

            while (true)
            {
                var receiveTask = _socket.ReceiveAsync(buf, SocketFlags.None, cancellationToken).AsTask();
                Task[] tasks = [receiveTask, Task.Delay(_socket.ReceiveTimeout, cancellationToken),];
                var finishedTask = await Task.WhenAny(tasks);
                if (finishedTask != receiveTask) // timeout
                {
                    Dispose();
                }

                try
                {
                    n = await receiveTask; // throws if disposed
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(buf);
                    throw;
                }
                received += n;

                if (received < buf.Length)
                    break;

                var newBuf = ArrayPool<byte>.Shared.Rent(buf.Length * 2);
                Buffer.BlockCopy(buf, 0, newBuf, 0, buf.Length);
                ArrayPool<byte>.Shared.Return(buf);
                buf = newBuf;
            }

            return (buf, received);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _socket.Dispose();
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
