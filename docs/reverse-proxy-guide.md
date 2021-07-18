# Reverse Proxy Guide

The Shadowsocks URI Generator API server is required to be used with a reverse proxy.

## Design Choices

The client real IP is obtained from these sources following the order of `X-Real-IP -> X-Forwarded-For -> Connecting IP`. The API server doesn't verify the authenticity of these headers. Instead, it's the admin's responsibility to configure the reverse proxy to make sure the header used by the API server contains information from a reliable source. This choice is made for the following reasons:

1. `Microsoft.AspNetCore.HttpOverrides.IPNetwork` doesn't have a parse method out of box. We'd have to write our own parser if we decided to read `ForwardedHeadersOptions.KnownNetworks` from `appsettings.json`.
2. NGINX has [`ngx_http_realip_module`](http://nginx.org/en/docs/http/ngx_http_realip_module.html) that collects the client real IP from configured trusted sources and sets the `$remote_addr` variable.
3. Cloudflare reverse proxy sets the `CF-Connecting-IP` header and appends/sets the `X-Forwarded-For` header.
4. If the admin wants to make sure the client real IP is obtained from a trusted source, they can configure NGINX to set the `X-Real-IP` header. If the admin doesn't care whether the client IP is authentic, it just works out of box. I believe this is the perfect balance of security and usability.

## NGINX Example

```nginx
http {
    server {
        location /ad304cf2-98f8-44d3-824d-b8931d5e724c/ {
            # The / at the end is significant.
            proxy_pass http://[::1]:20477/;

            proxy_pass_request_headers on;

            # Other possible values: $proxy_host, $http_host
            proxy_set_header Host $host;

            proxy_set_header X-Real-IP $remote_addr;

            # Use this if this is the terminating reverse proxy.
            # $proxy_add_x_forwarded_for is an alias for '$http_x_forwarded_for, $remote_addr'.
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

            # Use this if the incoming traffic is proxied.
            #proxy_set_header X-Forwarded-For '$http_x_forwarded_for, $realip_remote_addr';

            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_set_header X-Forwarded-Host $http_host;

            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection $http_connection;
        }
    }
}
```

If you use Cloudflare reverse proxy, include this snippet in your `http` block:

```nginx
# Trust Cloudflare IP addresses to get client IP.
# Source: https://www.cloudflare.com/ips/
# Last updated: 2021-07-18

set_real_ip_from  103.21.244.0/22;
set_real_ip_from  103.22.200.0/22;
set_real_ip_from  103.31.4.0/22;
set_real_ip_from  104.16.0.0/13;
set_real_ip_from  104.24.0.0/14;
set_real_ip_from  108.162.192.0/18;
set_real_ip_from  131.0.72.0/22;
set_real_ip_from  141.101.64.0/18;
set_real_ip_from  162.158.0.0/15;
set_real_ip_from  172.64.0.0/13;
set_real_ip_from  173.245.48.0/20;
set_real_ip_from  188.114.96.0/20;
set_real_ip_from  190.93.240.0/20;
set_real_ip_from  197.234.240.0/22;
set_real_ip_from  198.41.128.0/17;
set_real_ip_from  2400:cb00::/32;
set_real_ip_from  2606:4700::/32;
set_real_ip_from  2803:f800::/32;
set_real_ip_from  2405:b500::/32;
set_real_ip_from  2405:8100::/32;
set_real_ip_from  2a06:98c0::/29;
set_real_ip_from  2c0f:f248::/32;
real_ip_header    X-Forwarded-For;
real_ip_recursive on;
```
