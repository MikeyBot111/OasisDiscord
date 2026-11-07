# OasisDiscord

> **⚠️ Archived – old project, no longer maintained.**
> Written in 2023 for the **Oasis** Unturned servers. It's kept here for reference only. It may not work with current versions of Unturned or RocketMod.

A RocketMod plugin for **Unturned** that connects the game server to **Discord webhooks**. It handles player reports and sends server events to a log channel.

## What it does

- **`/report <message>`**: players can send a report straight to the admin team's Discord channel (with an optional cooldown)
- **Server log** (optional): sends these events to a Discord channel as embeds:
  - player joined (name, SteamID, IP and country, looked up through ip-api.com)
  - player disconnected
  - player died (killer, cause, body part)
  - chat messages

## Commands & permissions

| Command | Permission | Description |
|---|---|---|
| `/report <message>` | `OasisDiscord.report` | Send a report to the admins |

## Config

| Key | Default | Description |
|---|---|---|
| `ReportWebhook` | `""` | Discord webhook URL for reports |
| `ReportCooldown` | `false` | Turn the report cooldown on or off |
| `ReportCooldownSeconds` | `30` | Cooldown length |
| `ChatLogEnabled` | `false` | Turn on the join/leave/death/chat log |
| `ChatLogWebhook` | `""` | Discord webhook URL for the log |

No webhook URLs are included in this repo. You set your own in the plugin config on the server.

## Tech

- C# / .NET Framework 4.8
- RocketMod, Newtonsoft.Json
- `lib/` holds the reference assemblies needed to build

## Build

Open `OasisDiscord.sln` in Visual Studio and build it. Then copy the DLL into `Rocket/Plugins`.

## Part of the Oasis project

[OasisEssential](https://github.com/MikeyBot111/OasisEssential) · [OasisTeleport](https://github.com/MikeyBot111/OasisTeleport) · [OasisKits](https://github.com/MikeyBot111/OasisKits) · [OasisRanks](https://github.com/MikeyBot111/OasisRanks) · [OasisVault](https://github.com/MikeyBot111/OasisVault) · [OasisDiscord](https://github.com/MikeyBot111/OasisDiscord) · [OasisCustomCommands](https://github.com/MikeyBot111/OasisCustomCommands) · [WebsiteOasis](https://github.com/MikeyBot111/WebsiteOasis) · [oasis-server-discord](https://github.com/MikeyBot111/oasis-server-discord)
