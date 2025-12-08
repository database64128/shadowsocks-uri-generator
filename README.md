# 🌐 Shadowsocks URI Generator

[![Build](https://github.com/database64128/shadowsocks-uri-generator/actions/workflows/build.yml/badge.svg)](https://github.com/database64128/shadowsocks-uri-generator/actions/workflows/build.yml)
[![Release](https://github.com/database64128/shadowsocks-uri-generator/actions/workflows/release.yml/badge.svg)](https://github.com/database64128/shadowsocks-uri-generator/actions/workflows/release.yml)
[![Nuget](https://img.shields.io/nuget/v/ShadowsocksUriGenerator)](https://www.nuget.org/packages/ShadowsocksUriGenerator/)
[![AUR version](https://img.shields.io/aur/version/ss-uri-gen-git?label=ss-uri-gen-git)](https://aur.archlinux.org/packages/ss-uri-gen-git/)
[![AUR version](https://img.shields.io/aur/version/ss-uri-gen-server-git?label=ss-uri-gen-server-git)](https://aur.archlinux.org/packages/ss-uri-gen-server-git/)
[![AUR version](https://img.shields.io/aur/version/ss-uri-gen-chatbot-telegram-git?label=ss-uri-gen-chatbot-telegram-git)](https://aur.archlinux.org/packages/ss-uri-gen-chatbot-telegram-git/)
[![AUR version](https://img.shields.io/aur/version/ss-uri-gen-rescue-git?label=ss-uri-gen-rescue-git)](https://aur.archlinux.org/packages/ss-uri-gen-rescue-git/)

Shadowsocks URI Generator is a management and distribution platform for censorship circumvention services.

## Features

- Manage users, nodes, and groups with intuitive commands.
- API Server for management and online config.
- Deploy and manage [Outline servers](https://github.com/Jigsaw-Code/outline-server).
- Retrieve user credentials automatically from Outline servers, or add credentials manually in plaintext or `base64url`.
- Gather data usage statistics from Outline servers.
- Generate data usage report and export as CSV.
- Manage data usage limit on users and groups. Enforce data limit on Outline servers.
- Generate [SIP002](https://shadowsocks.org/en/spec/SIP002-URI-Scheme.html) `ss://` URLs for users.
- Support for [SIP003](https://shadowsocks.org/en/spec/Plugin.html) plugins.
- Online config delivery with support for [Open Online Config](https://github.com/Shadowsocks-NET/OpenOnlineConfig), [SIP008](https://shadowsocks.org/en/wiki/SIP008-Online-Configuration-Delivery.html), and V2Ray outbound files.
- Run as a service to execute scheduled tasks.
- Easy user interactions via [Telegram bots](https://core.telegram.org/bots).

## Build

Prerequisites: .NET 10 SDK

```console
$ # Build with Release configuration
$ dotnet build -c Release

$ # Publish as framework-dependent
$ dotnet publish ShadowsocksUriGenerator.CLI -c Release

$ # Publish as self-contained for Linux x64
$ dotnet publish ShadowsocksUriGenerator.CLI -c Release \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:DebuggerSupport=false \
    -p:EnableUnsafeBinaryFormatterSerialization=false \
    -p:EnableUnsafeUTF7Encoding=false \
    -p:InvariantGlobalization=true \
    -r linux-x64 --self-contained
```

## API Server

The API Server provides an API endpoint for basic management tasks and online config.

To start the server, simple run the binary or start the systemd service:

```console
$ ss-uri-gen-server
```

```console
$ systemctl --user enable --now ss-uri-gen-server.service
```

Some `appsettings.json` samples are included in the project directory. `appsettings.systemd.json` is configured to use the systemd logging format.

To generate API URLs and tokens, make sure to set `ApiServerBaseUrl` and `ApiServerSecretPath`:

```console
$ # Set ApiServerBaseUrl and ApiServerSecretPath.
$ ss-uri-gen settings set --api-server-base-url https://example.com --api-server-secret-path 8c1da4d8-8684-4a2c-9abb-57b9d5fa7e52
$ # Get API URLs and tokens from CLI.
$ ss-uri-gen online-config get-links
```

Linked Telegram users can get their API URLs and tokens from the Telegram bot by executing `/get_online_config_links`.

## CLI

The CLI is the primary management tool for Shadowsocks URI Generator.

```console
$ # See usage guide.
$ ss-uri-gen --help

$ # Enter interactive mode (REPL).
$ ss-uri-gen interactive

$ # Run as a service to execute scheduled tasks.
$ ss-uri-gen service --interval 3600 --pull-outline-server --generate-online-config

$ # Add users.
$ ss-uri-gen user add MyUserA MyUserB

$ # Add groups.
$ ss-uri-gen group add MyGroupA MyGroupB

$ # Add a new node.
$ ss-uri-gen node add MyGroupA MyNodeA 1.1.1.1 853

$ # Add a new node with v2ray-plugin.
$ ss-uri-gen node add MyGroupB MyNodeB 1.1.1.1 853 --plugin v2ray-plugin --plugin-opts "tls;host=cloudflare-dns.com"

$ # Deactivate a node to exclude it from delivery.
$ ss-uri-gen node deactivate MyGroupB MyNodeB

$ # Join a group.
$ ss-uri-gen user join MyUserB MyGroupB

$ # Add multiple users to a group.
$ ss-uri-gen group add-user MyGroupA MyUserA MyUserB

$ # Add a credential associating MyGroupA with MyUserA.
$ ss-uri-gen user add-credential MyUserA MyGroupA --method aes-256-gcm --password MyPassword

$ # Add a credential in base64url.
$ ss-uri-gen user add-credential MyUserB MyGroupA --userinfo-base64url eGNoYWNoYTIwLWlldGYtcG9seTEzMDU6TXlQYXNzd29yZA

$ # Print a user's ss:// links.
$ ss-uri-gen user get-ss-links MyUserA

$ # Get a user's data usage metrics.
$ ss-uri-gen user get-data-usage MyUserA

$ # Get a group's data usage metrics.
$ ss-uri-gen group get-data-usage MyGroupA

$ # Generate Open Online Config (OOC) v1 files.
$ ss-uri-gen online-config generate

$ # Print all users' Open Online Config (OOC) v1 delivery URLs.
$ ss-uri-gen online-config get-links

$ # Associate a group with an Outline server.
$ ss-uri-gen outline-server add MyGroupA '{"apiUrl":"https://localhost/example","certSha256":"EXAMPLE"}'

$ # Change Outline server settings.
$ ss-uri-gen outline-server set MyGroupA --name MyOutlineA --hostname github.com --metrics true

$ # Pull updates from Outline server.
$ ss-uri-gen outline-server pull MyGroupA

$ # Deploy local configuration to Outline server.
$ ss-uri-gen outline-server deploy MyGroupA

$ # Generate data usage report and export as CSV to current directory.
$ ss-uri-gen report --csv-outdir .

$ # Set default user for Outline server's access key id 0.
$ ss-uri-gen settings set --outline-server-global-default-user MyUserA

$ # Settings: change the online configuration generation output directory to 'sip008'.
$ ss-uri-gen settings set --online-config-output-directory sip008

$ # Rescue tool: rebuild database from generated online config.
$ ss-uri-gen-rescue --online-config-dir /path/to/online/config
```

## Telegram Bot

The Telegram bot can act as a client portal for your service. The user ID is used as the secret key for authentication.

Send your users their user ID, so they can use `/link <user_id>` to link their Telegram account to their registered user.

See the full [command list](ShadowsocksUriGenerator.Chatbot.Telegram/UpdateHandler.cs) for what the bot can do. Some commands can be disabled in the config.

To host your Telegram bot, register a bot at [Bot Father](https://t.me/BotFather) and get the bot token. Then choose one of the following methods:

```console
$ # Method 1: Set the bot token in the config.
$ ss-uri-gen-chatbot-telegram config set --bot-token "1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy"
$ # Start the bot.
$ ss-uri-gen-chatbot-telegram
```

```console
$ # Method 2: Set the bot token as an environment variable.
$ export TELEGRAM_BOT_TOKEN="1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy"
$ # Start the bot.
$ ss-uri-gen-chatbot-telegram
```

```console
$ # Method 3: Start the bot and pass the bot token as an argument.
$ ss-uri-gen-chatbot-telegram --bot-token "1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy"
```

## License

- This project is licensed under [GPLv3](LICENSE).
- The icons are from [Material Design Icons](https://materialdesignicons.com/) and are licensed under the [Pictogrammers Free License](https://dev.materialdesignicons.com/license).
- [`System.CommandLine`](https://github.com/dotnet/command-line-api) is licensed under the MIT license.
- [`Telegram.Bot`](https://github.com/TelegramBots/Telegram.Bot) and [`Telegram.Bot.Extensions.Polling`](https://github.com/TelegramBots/Telegram.Bot.Extensions.Polling) are licensed under the MIT license.

© 2025 database64128
