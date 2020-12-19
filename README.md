# 🌐 `shadowsocks-uri-generator`

A light-weight command line automation tool for multi-user `ss://` URL generation, [SIP008](https://github.com/shadowsocks/shadowsocks-org/issues/89) online configuration delivery, and [Outline server](https://github.com/Jigsaw-Code/outline-server) deployment and management.

## Features

- Manage users, nodes, and groups with intuitive commands.
- Deploy and manage [Outline servers](https://github.com/Jigsaw-Code/outline-server).
- Retrieve user credentials automatically from Outline servers, or add credentials manually in plaintext or `base64url`.
- Gather user data usage statistics from Outline servers.
- Export user's associated nodes as [SIP002](https://shadowsocks.org/en/spec/SIP002-URI-Scheme.html) `ss://` URLs.
- Support [SIP003](https://shadowsocks.org/en/spec/Plugin.html) plugins.
- Generate SIP008-compliant online configuration files.
- Generate and print SIP008 delivery URL.

## Usage

```bash
# See usage guide.
$ ./ss-uri-gen --help

# Add users.
$ ./ss-uri-gen user add MyUserA MyUserB

# Add groups.
$ ./ss-uri-gen group add MyGroupA MyGroupB

# Add a new node.
$ ./ss-uri-gen node add MyGroupA MyNodeA 1.1.1.1 853

# Add a new node with v2ray-plugin.
$ ./ss-uri-gen node add MyGroupB MyNodeB 1.1.1.1 853 --plugin v2ray-plugin --plugin-opts "tls;host=cloudflare-dns.com"

# Join a group.
$ ./ss-uri-gen user join MyUserB MyGroupB

# Add multiple users to a group.
$ ./ss-uri-gen group add-user MyGroupA MyUserA MyUserB

# Add a credential associating MyGroupA with MyUserA.
$ ./ss-uri-gen user add-credential MyUserA MyGroupA --method aes-256-gcm --password MyPassword

# Add a credential in base64url.
$ ./ss-uri-gen user add-credential MyUserB MyGroupA --userinfo-base64url eGNoYWNoYTIwLWlldGYtcG9seTEzMDU6TXlQYXNzd29yZA

# Print a user's ss:// links.
$ ./ss-uri-gen user get-ss-links MyUserA

# Generate SIP008-compliant online configuration files.
$ ./ss-uri-gen online-config generate

# Print all users' SIP008 delivery URLs.
$ ./ss-uri-gen online-config get-link

# Associate a group with an Outline server.
$ ./ss-uri-gen outline-server add MyGroupA '{"apiUrl":"https://localhost/example","certSha256":"EXAMPLE"}'

# Change Outline server settings.
$ ./ss-uri-gen outline-server set MyGroupA --name MyOutlineA --hostname github.com --metrics true

# Update Outline server information.
$ ./ss-uri-gen outline-server update MyGroupA

# Deploy local configuration to Outline server.
$ ./ss-uri-gen outline-server deploy MyGroupA

# Set default user for Outline server's access key id 0.
$ ./ss-uri-gen settings set --outline-server-global-default-user MyUserA

# Settings: change the online configuration generation output directory to 'sip008'.
$ ./ss-uri-gen settings set --online-config-output-directory sip008
```

## License

- This project is licensed under [GPLv3](LICENSE).

- [`JsonSnakeCaseNamingPolicy`](https://github.com/dotnet/corefx/pull/40003) is licensed under the MIT license.

© 2020 database64128
