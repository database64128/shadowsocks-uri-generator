# 🌐 Shadowsocks URI Generator

[![Build](https://github.com/database64128/shadowsocks-uri-generator/workflows/Build/badge.svg)](https://github.com/database64128/shadowsocks-uri-generator/actions?query=workflow%3ABuild)
[![Release](https://github.com/database64128/shadowsocks-uri-generator/workflows/Release/badge.svg)](https://github.com/database64128/shadowsocks-uri-generator/actions?query=workflow%3ARelease)

<a href="https://aur.archlinux.org/packages/ss-uri-gen-git/">
    <img alt="AUR badge for ss-uri-gen-git" src="https://img.shields.io/aur/version/ss-uri-gen-git?label=AUR%20ss-uri-gen-git" />
</a>
<a href="https://aur.archlinux.org/packages/ss-uri-gen-chatbot-telegram-git/">
    <img alt="AUR badge for ss-uri-gen-chatbot-telegram-git" src="https://img.shields.io/aur/version/ss-uri-gen-chatbot-telegram-git?label=AUR%20ss-uri-gen-chatbot-telegram-git" />
</a>
<a href="https://aur.archlinux.org/packages/ss-uri-gen-rescue-git/">
    <img alt="AUR badge for ss-uri-gen-rescue-git" src="https://img.shields.io/aur/version/ss-uri-gen-rescue-git?label=AUR%20ss-uri-gen-rescue-git" />
</a>

A light-weight command line automation tool for managing federated Shadowsocks servers. Automate deployments of [Outline servers](https://github.com/Jigsaw-Code/outline-server). Deliver configurations to users with [Open Online Config (OOC)](https://github.com/Shadowsocks-NET/OpenOnlineConfig).

## Features

- Manage users, nodes, and groups with intuitive commands.
- Deploy and manage [Outline servers](https://github.com/Jigsaw-Code/outline-server).
- Retrieve user credentials automatically from Outline servers, or add credentials manually in plaintext or `base64url`.
- Gather data usage statistics from Outline servers.
- Manage data usage limit on users and groups. Enforce data limit on Outline servers.
- Generate [SIP002](https://shadowsocks.org/en/spec/SIP002-URI-Scheme.html) `ss://` URLs for users.
- Support for [SIP003](https://shadowsocks.org/en/spec/Plugin.html) plugins.
- Generate Open Online Config (OOC) v1 files and delivery URLs.
- Run as a service to execute scheduled tasks.
- Easy user interactions via [Telegram bots](https://core.telegram.org/bots).

## Build

Prerequisites: .NET 5 SDK

Note for packagers: The application by default uses executable directory as config directory. To use user's config directory, define the constant `PACKAGED` when building.

```bash
# Build with Release configuration
$ dotnet build -c Release

# Publish as framework-dependent
$ dotnet publish ShadowsocksUriGenerator -c Release

# Publish as self-contained for Linux x64
$ dotnet publish ShadowsocksUriGenerator -c Release \
    -p:PublishReadyToRun=true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=link \
    -p:DebuggerSupport=false \
    -p:EnableUnsafeBinaryFormatterSerialization=false \
    -p:EnableUnsafeUTF7Encoding=false \
    -p:InvariantGlobalization=true \
    -r linux-x64 --self-contained

# Publish as self-contained for packaging on Linux x64
$ dotnet publish ShadowsocksUriGenerator -c Release \
    -p:DefineConstants=PACKAGED \
    -p:PublishReadyToRun=true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=link \
    -p:DebuggerSupport=false \
    -p:EnableUnsafeBinaryFormatterSerialization=false \
    -p:EnableUnsafeUTF7Encoding=false \
    -p:InvariantGlobalization=true \
    -r linux-x64 --self-contained
```

## Usage

```bash
# See usage guide.
$ ss-uri-gen --help

# Enter interactive mode (REPL).
$ ss-uri-gen interactive

# Run as a service to execute scheduled tasks.
$ ss-uri-gen service --interval 3600 --pull-outline-server --generate-online-config

# Add users.
$ ss-uri-gen user add MyUserA MyUserB

# Add groups.
$ ss-uri-gen group add MyGroupA MyGroupB

# Add a new node.
$ ss-uri-gen node add MyGroupA MyNodeA 1.1.1.1 853

# Add a new node with v2ray-plugin.
$ ss-uri-gen node add MyGroupB MyNodeB 1.1.1.1 853 --plugin v2ray-plugin --plugin-opts "tls;host=cloudflare-dns.com"

# Deactivate a node to exclude it from delivery.
$ ss-uri-gen node deactivate MyGroupB MyNodeB

# Join a group.
$ ss-uri-gen user join MyUserB MyGroupB

# Add multiple users to a group.
$ ss-uri-gen group add-user MyGroupA MyUserA MyUserB

# Add a credential associating MyGroupA with MyUserA.
$ ss-uri-gen user add-credential MyUserA MyGroupA --method aes-256-gcm --password MyPassword

# Add a credential in base64url.
$ ss-uri-gen user add-credential MyUserB MyGroupA --userinfo-base64url eGNoYWNoYTIwLWlldGYtcG9seTEzMDU6TXlQYXNzd29yZA

# Print a user's ss:// links.
$ ss-uri-gen user get-ss-links MyUserA

# Get a user's data usage metrics.
$ ss-uri-gen user get-data-usage MyUserA

# Get a group's data usage metrics.
$ ss-uri-gen group get-data-usage MyGroupA

# Generate Open Online Config (OOC) v1 files.
$ ss-uri-gen online-config generate

# Print all users' Open Online Config (OOC) v1 delivery URLs.
$ ss-uri-gen online-config get-links

# Associate a group with an Outline server.
$ ss-uri-gen outline-server add MyGroupA '{"apiUrl":"https://localhost/example","certSha256":"EXAMPLE"}'

# Change Outline server settings.
$ ss-uri-gen outline-server set MyGroupA --name MyOutlineA --hostname github.com --metrics true

# Pull updates from Outline server.
$ ss-uri-gen outline-server pull MyGroupA

# Deploy local configuration to Outline server.
$ ss-uri-gen outline-server deploy MyGroupA

# Set default user for Outline server's access key id 0.
$ ss-uri-gen settings set --outline-server-global-default-user MyUserA

# Settings: change the online configuration generation output directory to 'sip008'.
$ ss-uri-gen settings set --online-config-output-directory sip008

# Telegram bot: set bot token.
$ ss-uri-gen-chatbot-telegram config set --bot-token "1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy"

# Telegram bot: run as a service.
$ ss-uri-gen-chatbot-telegram

# Rescue tool: rebuild database from generated online config.
$ ss-uri-gen-rescue --online-config-dir /path/to/online/config
```

## License

- This project is licensed under [GPLv3](LICENSE).
- The icons are from [Material Design Icons](https://materialdesignicons.com/) and are licensed under the [Pictogrammers Free License](https://dev.materialdesignicons.com/license).
- [`System.CommandLine`](https://github.com/dotnet/command-line-api) is licensed under the MIT license.
- `System.Linq.Async` and `System.Interactive.Async` are from [dotnet/reactive](https://github.com/dotnet/reactive). They are licensed under the MIT license.
- `JsonSnakeCaseNamingPolicy` is adopted from [dotnet/corefx#40003](https://github.com/dotnet/corefx/pull/40003). It's licensed under the MIT license.
- [`Telegram.Bot`](https://github.com/TelegramBots/Telegram.Bot) and [`Telegram.Bot.Extensions.Polling`](https://github.com/TelegramBots/Telegram.Bot.Extensions.Polling) are licensed under the MIT license.

© 2021 database64128
