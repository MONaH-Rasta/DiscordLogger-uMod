# DiscordLogger
Logs events to Discord channels using webhooks

Simple plugin to display server events in a Discord channels using [Webhooks](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks). Set `Default WebhookURL`, enable events you want and you are ready to go. If you need messages about some specific events to be sent to another channels - set different `WebhookURL` for these events.
* Supports filtering nicknames and chat messages to avoid impersonation, spam and profanities. Just install [**Anti Spam**](https://umod.org/plugins/anti-spam) and [**UFilter**](https://umod.org/plugins/ufilter) plugins and activate them in the config file.

## Configuration

```json
{
  "Global settings": {
    "Log to console?": false,
    "Use AntiSpam plugin on chat messages": false,
    "Use UFilter plugin on chat messages": false,
    "Hide admin connect/disconnect messages": false,
    "Hide NPC death messages": false,
    "Replacement string for tags": "`",
    "Queue interval (1 message per ? seconds)": 1.0,
    "Queue cooldown if connection error (seconds)": 60.0,
    "Default WebhookURL": "",
    "RCON command blacklist": [
      "playerlist",
      "status"
    ]
  },
  "Admin Hammer settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Admin Radar settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Bradley settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Cargo Ship settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Cargo Plane settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Chat settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Chat (Team) settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "CH47 Helicopter settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Christmas settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Clan settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Dangerous Treasures settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Duel settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Godmode settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Easter settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Error settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Hackable Locked Crate settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Halloween settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Helicopter settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "NTeleportation settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Permissions settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Player death settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Player DeathNotes settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Player connect advanced info settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Player connect settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Player disconnect settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Player Respawned settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Private Messages settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Raidable Bases settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Rcon command settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Rcon connection settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Rust Kits settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "SantaSleigh settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Server messages settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Server state settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Supply Drop settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Teams settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "User Banned settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "User Kicked settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "User Muted settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "User Name Updated settings": {
    "WebhookURL": "",
    "Enabled?": false
  },
  "Vanish settings": {
    "WebhookURL": "",
    "Enabled?": false
  }
}
```

## Localization

```json
{
  "Event.Bradley": ":dagger: {time} Bradley spawned `{0}`",
  "Event.CargoPlane": ":airplane: {time} Cargo Plane incoming `{0}`",
  "Event.CargoShip": ":ship: {time} Cargo Ship incoming `{0}`",
  "Event.Chat": ":speech_left: {time} **{0}**: {1}",
  "Event.ChatTeam": ":busts_in_silhouette: {time} **{0}**: {1}",
  "Event.Chinook": ":helicopter: {time} Chinook 47 incoming `{0}`",
  "Event.Christmas": ":christmas_tree: {time} Christmas event started",
  "Event.Death": ":skull: {time} `{0}` died",
  "Event.Easter": ":egg: {time} Easter event started",
  "Event.EasterWinner": ":egg: {time} Easter event ended. The winner is `{0}`",
  "Event.Error": ":octagonal_sign: {time}\n{0}",
  "Event.Halloween": ":jack_o_lantern: {time} Halloween event started",
  "Event.HalloweenWinner": ":jack_o_lantern: {time} Halloween event ended. The winner is `{0}`",
  "Event.Helicopter": ":dagger: {time} Helicopter incoming `{0}`",
  "Event.Initialized": ":ballot_box_with_check: {time} Server is online again!",
  "Event.LockedCrate": ":package: {time} Codelocked crate is here `{0}`",
  "Event.PlayerConnected": ":white_check_mark: {time} {0} connected",
  "Event.PlayerConnectedInfo": ":detective: {time} {0} connected. SteamID: `{1}` IP: `{2}`",
  "Event.PlayerDisconnected": ":x: {time} {0} disconnected ({1})",
  "Event.PlayerRespawned": ":baby_symbol: {time} `{0}` has been spawned at `{1}`",
  "Event.RconCommand": ":satellite: {time} RCON command `{0}` is run from `{1}`",
  "Event.RconConnection": ":satellite: {time} RCON connection is opened from `{0}`",
  "Event.Team": ":family_man_girl_boy: {time} Team was `{0}`\n{1}",
  "Event.SantaSleigh": ":santa: {time} SantaSleigh Event started",
  "Event.ServerMessage": ":desktop: {time} `{0}`",
  "Event.Shutdown": ":stop_sign: {time} Server is shutting down!",
  "Event.SupplyDrop": ":parachute: {time} SupplyDrop incoming at `{0}`",
  "Event.SupplyDropLanded": ":gift: {time} SupplyDrop landed at `{0}`",
  "Event.SupplySignal": ":firecracker: {time} SupplySignal was thrown by `{0}` at `{1}`",
  "Event.UserBanned": ":no_entry: {time} Player `{0}` SteamID: `{1}` IP: `{2}` was banned: `{3}`",
  "Event.UserKicked": ":hiking_boot: {time} Player `{0}` SteamID: `{1}` was kicked: `{2}`",
  "Event.UserMuted": ":mute: {time} `{0}` was muted by `{1}` for `{2}` (`{3}`)",
  "Event.UserNameUpdated": ":label: {time} `{0}` changed name to `{1}` SteamID: `{2}`",
  "Event.UserUnbanned": ":ok: {time} Player `{0}` SteamID: `{1}` IP: `{2}` was unbanned",
  "Event.UserUnmuted": ":speaker: {time} `{0}` was unmuted `{1}`",
  "Format.Created": "created",
  "Format.Day": "day",
  "Format.Days": "days",
  "Format.Disbanded": "disbanded",
  "Format.Easy": "Easy",
  "Format.Expert": "Expert",
  "Format.Hard": "Hard",
  "Format.Hour": "hour",
  "Format.Hours": "hours",
  "Format.Medium": "Medium",
  "Format.Minute": "minute",
  "Format.Minutes": "minutes",
  "Format.Nightmare": "Nightmare",
  "Format.Second": "second",
  "Format.Updated": "updated",
  "Permission.GroupCreated": ":family: {time} Group `{0}` has been created",
  "Permission.GroupDeleted": ":family: {time} Group `{0}` has been deleted",
  "Permission.UserGroupAdded": ":family: {time} `{0}` `{1}` is added to group `{2}`",
  "Permission.UserGroupRemoved": ":family: {time} `{0}` `{1}` is removed from group `{2}`",
  "Permission.UserPermissionGranted": ":key: {time} `{0}` `{1}` is granted `{2}`",
  "Permission.UserPermissionRevoked": ":key: {time} `{0}` `{1}` is revoked `{2}`",
  "Plugin.AdminHammerOff": ":hammer: {time} AdminHammer enabled by `{0}`",
  "Plugin.AdminHammerOn": ":hammer: {time} AdminHammer disabled by `{0}`",
  "Plugin.AdminRadarOff": ":compass: {time} Admin Radar enabled by `{0}`",
  "Plugin.AdminRadarOn": ":compass: {time} Admin Radar disabled by `{0}`",
  "Plugin.ClanCreated": ":family_mwgb: {time} **{0}** clan was created",
  "Plugin.ClanDisbanded": ":family_mwgb: {time} **{0}** clan was disbanded",
  "Plugin.DangerousTreasuresEnded": ":pirate_flag: {time} Dangerous Treasures event at `{0}` is ended",
  "Plugin.DangerousTreasuresStarted": ":pirate_flag: {time} Dangerous Treasures started at `{0}`",
  "Plugin.DeathNotes": ":skull_crossbones: {time} {0}",
  "Plugin.Duel": ":crossed_swords: {time} `{0}` has defeated `{1}` in a duel",
  "Plugin.GodmodeOff": ":angel: {time} Godmode disabled for `{0}`",
  "Plugin.GodmodeOn": ":angel: {time} Godmode enabled for `{0}`",
  "Plugin.NTeleportation": ":cyclone: {time} `{0}` teleported from `{1}` `{2}` to `{3}` `{4}`",
  "Plugin.PersonalHelicopter": ":dagger: {time} Personal Helicopter incoming `{0}`",
  "Plugin.PrivateMessage": ":envelope: {time} PM from `{0}` to `{1}`: {2}",
  "Plugin.RaidableBaseEnded": ":homes: {time} {1} Raidable Base at `{0}` is ended",
  "Plugin.RaidableBaseStarted": ":homes: {time} {1} Raidable Base spawned at `{0}`",
  "Plugin.RustKits": ":shopping_bags: {time} `{0}` redeemed a kit `{1}`",
  "Plugin.TimedGroupAdded": ":timer: {time} `{0}` `{1}` is added to `{2}` for {3}",
  "Plugin.TimedGroupExtended": ":timer: {time} `{0}` `{1}` timed group `{2}` is extended to {3}",
  "Plugin.TimedPermissionExtended": ":timer: {time} `{0}` `{1}` timed permission `{2}` is extended to {3}",
  "Plugin.TimedPermissionGranted": ":timer: {time} `{0}` `{1}` is granted `{2}` for {3}",
  "Plugin.VanishOff": ":ghost: {time} Vanish: Disabled for `{0}`",
  "Plugin.VanishOn": ":ghost: {time} Vanish: Enabled for `{0}`"
}
```

## Developer API

### DiscordSendMessage

Used to send message to discord channel. If `webhookUrl` is not set - message will be sent to `Default WebhookURL`. `stripTags` is used to replace all tags like `<color></color>` etc.

```csharp
void DiscordSendMessage(string message, string webhookUrl, bool stripTags = false)
```

## Credits

* [**MJSU**](https://umod.org/user/MJSU) many thanks for all help
* [**Arainrr**](https://umod.org/user/Arainrr) thank you for help