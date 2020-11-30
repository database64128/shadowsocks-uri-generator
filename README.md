# 🌐 `shadowsocks-uri-generator`

A light-weight command line automation tool for multi-user `ss://` URI generation and [SIP008](https://github.com/shadowsocks/shadowsocks-org/issues/89) online configuration management.

## Features

- Intuitive Command Line Interface.
- Shadowsocks nodes are grouped for easier management.
- Associate users with groups using different credentials.
- Add credentials in plaintext or `base64url`.
- Export user's associated nodes as `ss://` [SIP002](https://shadowsocks.org/en/spec/SIP002-URI-Scheme.html) URIs.
- Support [SIP003](https://shadowsocks.org/en/spec/Plugin.html) plugins.
- Generate SIP008-compliant online configuration files.
- Generate and print any user's SIP008 delivery URL.

## Usage

```bash
# See more usage information.
$ ./ss-uri-gen --help

# Add some users.
$ ./ss-uri-gen add-users MyUserA MyUserB

# Add some node groups.
$ ./ss-uri-gen add-node-groups MyGroupA MyGroupB

# Add a node.
$ ./ss-uri-gen add-node MyGroupA MyNodeA 1.1.1.1 853

# Add a node with v2ray-plugin.
$ ./ss-uri-gen add-node MyGroupB MyNodeB 1.1.1.1 853 --plugin v2ray-plugin --plugin-opts "tls;host=cloudflare-dns.com"

# Add a credential associating MyGroupA with MyUserA.
$ ./ss-uri-gen add-credential MyUserA MyGroupA --method aes-256-gcm --password MyPassword

# Add a credential with base64url.
$ ./ss-uri-gen add-credential MyUserB MyGroupA --userinfo-base64url eGNoYWNoYTIwLWlldGYtcG9seTEzMDU6TXlQYXNzd29yZA

# Settings: change the online configuration generation output directory to 'sip008'.
$ ./ss-uri-gen change-settings --online-config-output-directory sip008

# Generate SIP008-compliant online configuration files.
$ ./ss-uri-gen gen-online-config

# Print all users' SIP008 delivery URLs.
$ ./ss-uri-gen get-online-config-link

# Print a user's ss:// links.
$ ./ss-uri-gen get-ss-links MyUserA
```

## License

- Licensed under [GPLv3](LICENSE).

- [`JsonSnakeCaseNamingPolicy`](https://github.com/dotnet/corefx/pull/40003) is licensed under the MIT license.

© 2020 database64128
