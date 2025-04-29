using System;
using System.Collections.Generic;

namespace ShadowsocksUriGenerator.Outline;

public record OutlineApiKey(Uri ApiUrl, string? CertSha256);
public record OutlineServerName(string Name);
public record OutlineServerHostname(string Hostname);
public record OutlineDataLimit(ulong Bytes);
public record OutlineDataLimitRequest(OutlineDataLimit Limit);
public record OutlineMetrics(bool MetricsEnabled);
public record OutlineAccessKeysPort(int Port);
public record OutlineAccessKeysResponse(List<OutlineAccessKey> AccessKeys);
public record OutlineDataUsage(Dictionary<int, ulong> BytesTransferredByUserId);
