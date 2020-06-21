# 🌐 `shadowsocks-uri-generator`

A light-weight command line automation tool for multi-user `ss://` URI generation.

## Features

- Intuitive Command Line Interface.
- Shadowsocks nodes are grouped for easier management.
- Associate users with groups using different credentials.
- Add credentials in plaintext or `base64url`.
- Export user's associated nodes as `ss://` [SIP002](https://shadowsocks.org/en/spec/SIP002-URI-Scheme.html) URIs.

## Usage

```bash
# See more usage information.
$ ./ss-uri-generator --help

# Add some users.
$ ./ss-uri-generator add-users MyUserA MyUserB

# Add some node groups.
$ ./ss-uri-generator add-node-groups MyGroupA MyGroupB

# Add a node.
$ ./ss-uri-generator add-node MyGroupA MyNodeA 1.1.1.1 853

# Add a credential associating MyGroupA with MyUserA
$ ./ss-uri-generator add-credential MyUserA MyGroupA --method aes-256-gcm --password MyPassword

# Add a credential with base64url
$ ./ss-uri-generator add-credential MyUserB MyGroupA --userinfo-base64url eGNoYWNoYTIwLWlldGYtcG9seTEzMDU6TXlQYXNzd29yZA
```

## License

Licensed under [GPLv3](LICENSE).

© 2020 database64128
