using Facepunch;
using Newtonsoft.Json;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using Oxide.Core;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System;
using Time = UnityEngine.Time;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Discord Events", "MON@H", "1.2.11")]
    [Description("Displays events to a discord channel")]
    class DiscordEvents : CovalencePlugin
    {
        #region Variables

        [PluginReference] private Plugin AntiSpamNames, BetterChatMute, PersonalHeli, PersonalHeliExtended;

        private Hash<DiscordMessage, string> _queueMessages = new Hash<DiscordMessage, string>();
        private Hash<DiscordMessage, string> _queueProcessing = new Hash<DiscordMessage, string>();
        private Hash<DiscordMessage, string> _queueErrorProcessing = new Hash<DiscordMessage, string>();
        private Hash<uint, float> _lastEntities = new Hash<uint, float>();
        private bool _isConnectionOK = true;
        private bool _retrying = false;
        private DiscordMessage _lastMessage;
        private string _lastUrl;

        private UFilterStoredData _uFilterStoredData;

        private class UFilterStoredData
        {
            public List<string> Profanities = new List<string>();
        }

        private readonly List<Regex> _regexTags = new List<Regex>
        {
            new Regex("<color=.+?>", RegexOptions.Compiled),
            new Regex("<size=.+?>", RegexOptions.Compiled)
        };

        private readonly List<string> _tags = new List<string>
        {
            "</color>",
            "</size>",
            "<i>",
            "</i>",
            "<b>",
            "</b>"
        };

        private enum EventType
        {
            Bradley,
            CargoPlane,
            CargoShip,
            Chat,
            ChatTeam,
            Chinook,
            Christmas,
            DangerousTreasures,
            Death,
            DeathNotes,
            Duel,
            Easter,
            Halloween,
            Helicopter,
            LockedCrate,
            None,
            PlayerConnected,
            PlayerConnectedInfo,
            PlayerDisconnected,
            RaidableBases,
            SantaSleigh,
            SupplyDrop,
            SupplySignal
        }

        #endregion Variables

        #region Initialization

        private void Init()
        {
            UnsubscribeDisabled();
        }

        private void OnServerInitialized(bool isStartup)
        {
            if (isStartup && _configData.ServerStateSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts("Server is online again!");
                }

                SendMessage(Lang("Initialized"), _configData.ServerStateSettings.WebhookURL);
            }

            if (_configData.GlobalSettings.UseUFilter)
            {
                _uFilterStoredData = Interface.Oxide.DataFileSystem.ReadObject<UFilterStoredData>("UFilter");
            }
        }

        private void OnServerShutdown()
        {
            if (_configData.ServerStateSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts("Server is shutting down!");
                }

                DiscordMessage discordMessage = new DiscordMessage(Lang("Shutdown"));
                SendDiscordMessage(_configData.ServerStateSettings.WebhookURL, discordMessage);
            }
        }

        #endregion Initialization

        #region Configuration

        private ConfigData _configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Global settings")]
            public GlobalSettings GlobalSettings = new GlobalSettings();

            [JsonProperty(PropertyName = "Bradley settings")]
            public EventSettings BradleySettings = new EventSettings();

            [JsonProperty(PropertyName = "Cargo Ship settings")]
            public EventSettings CargoShipSettings = new EventSettings();

            [JsonProperty(PropertyName = "Cargo Plane settings")]
            public EventSettings CargoPlaneSettings = new EventSettings();

            [JsonProperty(PropertyName = "Chat settings")]
            public EventSettings ChatSettings = new EventSettings();

            [JsonProperty(PropertyName = "Chat (Team) settings")]
            public EventSettings ChatTeamSettings = new EventSettings();

            [JsonProperty(PropertyName = "CH47 Helicopter settings")]
            public EventSettings ChinookSettings = new EventSettings();

            [JsonProperty(PropertyName = "Christmas settings")]
            public EventSettings ChristmasSettings = new EventSettings();

            [JsonProperty(PropertyName = "Clan settings")]
            public EventSettings ClanSettings = new EventSettings();

            [JsonProperty(PropertyName = "Dangerous Treasures settings")]
            public EventSettings DangerousTreasuresSettings = new EventSettings();

            [JsonProperty(PropertyName = "Duel settings")]
            public EventSettings DuelSettings = new EventSettings();

            [JsonProperty(PropertyName = "Easter settings")]
            public EventSettings EasterSettings = new EventSettings();

            [JsonProperty(PropertyName = "Hackable Locked Crate settings")]
            public EventSettings LockedCrateSettings = new EventSettings();

            [JsonProperty(PropertyName = "Halloween settings")]
            public EventSettings HalloweenSettings = new EventSettings();

            [JsonProperty(PropertyName = "Helicopter settings")]
            public EventSettings HelicopterSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player death settings")]
            public EventSettings PlayerDeathSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player DeathNotes settings")]
            public EventSettings PlayerDeathNotesSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player connect advanced info settings")]
            public EventSettings PlayerConnectedInfoSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player connect settings")]
            public EventSettings PlayerConnectedSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player disconnect settings")]
            public EventSettings PlayerDisconnectedSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player Respawned settings")]
            public EventSettings PlayerRespawnedSettings = new EventSettings();

            [JsonProperty(PropertyName = "Raidable Bases settings")]
            public EventSettings RaidableBasesSettings = new EventSettings();

            [JsonProperty(PropertyName = "Rcon command settings")]
            public EventSettings RconCommandSettings = new EventSettings();

            [JsonProperty(PropertyName = "Rcon connection settings")]
            public EventSettings RconConnectionSettings = new EventSettings();

            [JsonProperty(PropertyName = "SantaSleigh settings")]
            public EventSettings SantaSleighSettings = new EventSettings();

            [JsonProperty(PropertyName = "Server messages settings")]
            public EventSettings ServerMessagesSettings = new EventSettings();

            [JsonProperty(PropertyName = "Server state settings")]
            public EventSettings ServerStateSettings = new EventSettings();

            [JsonProperty(PropertyName = "Supply Drop settings")]
            public EventSettings SupplyDropSettings = new EventSettings();

            [JsonProperty(PropertyName = "User Banned settings")]
            public EventSettings UserBannedSettings = new EventSettings();

            [JsonProperty(PropertyName = "User Kicked settings")]
            public EventSettings UserKickedSettings = new EventSettings();

            [JsonProperty(PropertyName = "User Muted settings")]
            public EventSettings UserMutedSettings = new EventSettings();

            [JsonProperty(PropertyName = "User Name Updated settings")]
            public EventSettings UserNameUpdateSettings = new EventSettings();
        }

        private class GlobalSettings
        {
            [JsonProperty(PropertyName = "Log to console?")]
            public bool LoggingEnabled = false;

            [JsonProperty(PropertyName = "Use AntiSpamNames plugin on chat messages")]
            public bool UseAntiSpamNames = false;

            [JsonProperty(PropertyName = "Use UFilter plugin on chat messages")]
            public bool UseUFilter = false;

            [JsonProperty(PropertyName = "Hide admin connect/disconnect messages")]
            public bool HideAdmin = false;

            [JsonProperty(PropertyName = "Hide NPC death messages")]
            public bool HideNPC = false;

            [JsonProperty(PropertyName = "Replacement string for tags")]
            public string TagsReplacement = "`";

            [JsonProperty(PropertyName = "Queue interval (1 message per ? seconds)")]
            public float QueueInterval = 1.5f;

            [JsonProperty(PropertyName = "Sleep interval (if connection error)")]
            public float SleepInterval = 60f;

            [JsonProperty(PropertyName = "RCON command blacklist", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> RCONCommandBlacklist = new List<string>()
            {
                "playerlist",
                "status"
            };
        }

        private class EventSettings
        {
            [JsonProperty(PropertyName = "WebhookURL")]
            public string WebhookURL = "";

            [JsonProperty(PropertyName = "Enabled?")]
            public bool Enabled = false;
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _configData = Config.ReadObject<ConfigData>();
                if (_configData == null)
                {
                    LoadDefaultConfig();
                    SaveConfig();
                }
            }
            catch
            {
                PrintError("The configuration file is corrupted");
                LoadDefaultConfig();
                SaveConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            _configData = new ConfigData();
        }

        protected override void SaveConfig() => Config.WriteObject(_configData);

        #endregion Configuration

        #region Localization

        private string Lang(string key, string userIDString = null, params object[] args)
        {
            try
            {
                return string.Format(lang.GetMessage(key, this, userIDString).Replace("{time}", DateTime.Now.ToShortTimeString()), args);
            }
            catch (Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception:\n{ex}");
                throw;
            }
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Bradley"] = ":dagger: {time} Bradley spawned `{0}`",
                ["CargoPlane"] = ":airplane: {time} Cargo Plane incoming `{0}`",
                ["CargoShip"] = ":ship: {time} Cargo Ship incoming `{0}`",
                ["Chat"] = ":speech_left: {time} **{0}**: {1}",
                ["ChatTeam"] = ":busts_in_silhouette: {time} **{0}**: {1}",
                ["Chinook"] = ":helicopter: {time} Chinook 47 incoming `{0}`",
                ["Christmas"] = ":christmas_tree: {time} Christmas event started",
                ["ClanCreated"] = ":family_mwgb: {time} **{0}** clan was created",
                ["ClanDisbanded"] = ":family_mwgb: {time} **{0}** clan was disbanded",
                ["DangerousTreasuresEnded"] = ":pirate_flag: {time} Dangerous Treasures event at `{0}` is ended",
                ["DangerousTreasuresStarted"] = ":pirate_flag: {time} Dangerous Treasures started at `{0}`",
                ["Death"] = ":skull: {time} `{0}` died",
                ["DeathNotes"] = ":skull_crossbones: {time} {0}",
                ["Duel"] = ":crossed_swords: {time} `{0}` has defeated `{1}` in a duel",
                ["Easter"] = ":egg: {time} Easter event started",
                ["EasterWinner"] = ":egg: {time} Easter event ended. The winner is `{0}`",
                ["Halloween"] = ":jack_o_lantern: {time} Halloween event started",
                ["HalloweenWinner"] = ":jack_o_lantern: {time} Halloween event ended. The winner is `{0}`",
                ["Helicopter"] = ":dagger: {time} Helicopter incoming `{0}`",
                ["Initialized"] = ":ballot_box_with_check: {time} Server is online again!",
                ["LockedCrate"] = ":package: {time} Codelocked crate is here `{0}`",
                ["PersonalHelicopter"] = ":dagger: {time} Personal Helicopter incoming `{0}`",
                ["PlayerConnected"] = ":white_check_mark: {time} {0} connected",
                ["PlayerConnectedInfo"] = ":detective: {time} {0} connected. SteamID: `{1}` IP: `{2}`",
                ["PlayerDisconnected"] = ":x: {time} {0} disconnected ({1})",
                ["PlayerRespawned"] = ":baby_symbol: {time} `{0}` has been spawned at `{1}`",
                ["RaidableBaseEnded"] = ":homes: {time} {1} Raidable Base at `{0}` is ended",
                ["RaidableBaseStarted"] = ":homes: {time} {1} Raidable Base spawned at `{0}`",
                ["RconCommand"] = ":satellite: {time} RCON command `{0}` is run from `{1}`",
                ["RconConnection"] = ":satellite: {time} RCON connection is opened from `{0}`",
                ["SantaSleigh"] = ":santa: {time} SantaSleigh Event started",
                ["ServerMessage"] = ":desktop: {time} `{0}`",
                ["Shutdown"] = ":stop_sign: {time} Server is shutting down!",
                ["SupplyDrop"] = ":parachute: {time} SupplyDrop incoming at `{0}`",
                ["SupplyDropLanded"] = ":gift: {time} SupplyDrop landed at `{0}`",
                ["SupplySignal"] = ":firecracker: {time} SupplySignal was thrown by `{0}` at `{1}`",
                ["UserBanned"] = ":no_entry: {time} Player `{0}` SteamID: `{1}` IP: `{2}` was banned: `{3}`",
                ["UserKicked"] = ":hiking_boot: {time} Player `{0}` SteamID: `{1}` was kicked: `{2}`",
                ["UserMuted"] = ":mute: {time} `{0}` was muted by `{1}` for `{2}` (`{3}`)",
                ["UserNameUpdated"] = ":label: {time} `{0}` changed name to `{1}` SteamID: `{2}`",
                ["UserUnbanned"] = ":ok: {time} Player `{0}` SteamID: `{1}` IP: `{2}` was unbanned",
                ["UserUnmuted"] = ":speaker:: {time} `{0}` was unmuted `{1}`",

                ["Easy"] = "Easy",
                ["Medium"] = "Medium",
                ["Hard"] = "Hard",
                ["Expert"] = "Expert",
                ["Nightmare"] = "Nightmare"
            }, this);
        }

        #endregion Localization

        #region Events Hooks

        private void OnEntitySpawned(BaseHelicopter entity)
        {
            NextTick(() => {
                HandleEntity(entity);
            });
        }

        private void OnEntitySpawned(BradleyAPC entity) => HandleEntity(entity);

        private void OnEntitySpawned(CargoPlane entity) => HandleEntity(entity);

        private void OnEntitySpawned(CargoShip entity) => HandleEntity(entity);

        private void OnEntitySpawned(CH47HelicopterAIController entity) => HandleEntity(entity);

        private void OnEntitySpawned(EggHuntEvent entity) => HandleEntity(entity);

        private void OnEntitySpawned(HackableLockedCrate entity) => HandleEntity(entity);

        private void OnEntitySpawned(SantaSleigh entity) => HandleEntity(entity);

        private void OnEntitySpawned(SupplyDrop entity) => HandleEntity(entity);

        private void OnEntitySpawned(XMasRefill entity) => HandleEntity(entity);

        private void OnEntityDeath(BasePlayer player, HitInfo info)
        {
            if (player == null || info == null)
            {
                return;
            }

            if (_configData.GlobalSettings.HideNPC && (player.IsNpc || !player.userID.IsSteamId()))
            {
                return;
            }

            if (_configData.PlayerDeathSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{player.displayName} died.");
                }

                SendMessage(Lang("Death", null, ReplaceChars(player.displayName)), _configData.PlayerDeathSettings.WebhookURL);
            }
        }

        private void OnDeathNotice(Dictionary<string, object> data, string message)
        {
            if (_configData.PlayerDeathNotesSettings.Enabled)
            {
                SendMessage(Lang("DeathNotes", null, StripRustTags(Formatter.ToPlaintext(message))), _configData.PlayerDeathNotesSettings.WebhookURL);
            }
        }

        private void OnEntityKill(EggHuntEvent entity)
        {
            if (entity == null)
            {
                return;
            }

            List<EggHuntEvent.EggHunter> winners = entity.GetTopHunters();
            string winner;
            if (winners.Count > 0)
            {
                winner = ReplaceChars(winners[0].displayName);
            }
            else
            {
                winner = "No winner";
            }

            bool isHalloween = entity is HalloweenHunt;
            if (isHalloween)
            {
                if (_configData.HalloweenSettings.Enabled)
                {
                    if (_configData.GlobalSettings.LoggingEnabled)
                    {
                        Puts("Halloween Hunt Event has ended. The winner is " + winner);
                    }

                    SendMessage(Lang("HalloweenWinner", null, winner), _configData.HalloweenSettings.WebhookURL);
                }
            }
            else
            {
                if (_configData.EasterSettings.Enabled)
                {
                    if (_configData.GlobalSettings.LoggingEnabled)
                    {
                        Puts("Egg Hunt Event has ended. The winner is " + winner);
                    }

                    SendMessage(Lang("EasterWinner", null, winner), _configData.EasterSettings.WebhookURL);
                }
            }
        }

        private void OnExplosiveThrown(BasePlayer player, SupplySignal entity) => HandleSupplySignal(player, entity);

        private void OnExplosiveDropped(BasePlayer player, SupplySignal entity) => HandleSupplySignal(player, entity);

        private void OnRconConnection(IPAddress ip)
        {
            if (_configData.RconConnectionSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"RCON connection is opened from {ip}");
                }

                SendMessage(Lang("RconConnection", null, ip.ToString()), _configData.RconConnectionSettings.WebhookURL);
            }
        }

        private void OnRconCommand(IPAddress ip, string command, string[] args)
        {
            if (_configData.RconCommandSettings.Enabled)
            {
                foreach (string rconCommand in _configData.GlobalSettings.RCONCommandBlacklist)
                {
                    if (command.ToLower().Equals(rconCommand.ToLower()))
                    {
                        return;
                    }
                }

                for (int i = 0; i < args.Length; i++)
                {
                    command += $" {args[i]}";
                }

                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"RCON command {command} is run from {ip}");
                }

                SendMessage(Lang("RconCommand", null, command, ip), _configData.RconCommandSettings.WebhookURL);
            }
        }

        private void OnSupplyDropLanded(SupplyDrop entity)
        {
            if (entity == null || IsEntityInList(entity.net.ID))
            {
                return;
            }

            if (_configData.SupplyDropSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts("SupplyDrop landed at " + GetGridPosition(entity.transform.position));
                }

                SendMessage(Lang("SupplyDropLanded", null, GetGridPosition(entity.transform.position)), _configData.SupplyDropSettings.WebhookURL);
                _lastEntities.Add(entity.net.ID, Time.realtimeSinceStartup + 60);
            }
        }

        private void OnDuelistDefeated(BasePlayer attacker, BasePlayer victim)
        {
            if (attacker == null || victim == null)
            {
                return;
            }

            if (_configData.DuelSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{attacker.displayName} has defeated {victim.displayName} in a duel");
                }

                SendMessage(Lang("Duel", null, ReplaceChars(attacker.displayName), ReplaceChars(victim.displayName)), _configData.DuelSettings.WebhookURL);
            }
        }

        private void OnRaidableBaseStarted(Vector3 raidPos, int difficulty)
        {
            HandleRaidableBase(raidPos, difficulty, "RaidableBaseStarted");
        }
        private void OnRaidableBaseEnded(Vector3 raidPos, int difficulty)
        {
            HandleRaidableBase(raidPos, difficulty, "RaidableBaseEnded");
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (_configData.PlayerConnectedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player {player.displayName} connected.");
                }

                if (!_configData.GlobalSettings.HideAdmin || !player.IsAdmin)
                {
                    SendMessage(Lang("PlayerConnected", null, ReplaceChars(player.displayName)), _configData.PlayerConnectedSettings.WebhookURL);
                }
            }

            if (_configData.PlayerConnectedInfoSettings.Enabled)
            {
                SendMessage(Lang("PlayerConnectedInfo", null, ReplaceChars(player.displayName), player.UserIDString, player.net.connection.ipaddress.Split(':')[0]), _configData.PlayerConnectedInfoSettings.WebhookURL);
            }
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null)
            {
                return;
            }

            if (_configData.PlayerDisconnectedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player {player.displayName} disconnected ({reason}).");
                }

                if (!_configData.GlobalSettings.HideAdmin || !player.IsAdmin)
                {
                    SendMessage(Lang("PlayerDisconnected", null, ReplaceChars(player.displayName), reason), _configData.PlayerDisconnectedSettings.WebhookURL);
                }
            }
        }

        private void OnPlayerChat(BasePlayer player, string message, ConVar.Chat.ChatChannel channel)
        {
            if (player == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (BetterChatMute != null && BetterChatMute.IsLoaded)
            {
                if (BetterChatMute.Call<bool>("API_IsMuted", player.IPlayer))
                {
                    return;
                }
            }

            if (_configData.GlobalSettings.UseAntiSpamNames && AntiSpamNames != null && AntiSpamNames.IsLoaded)
            {
                message = AntiSpamNames.Call<string>("GetClearText", message);
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
            }

            if (_configData.GlobalSettings.UseUFilter && _uFilterStoredData.Profanities.Count > 0)
            {
                StringBuilder sb = new StringBuilder(message);
                foreach (string profanity in _uFilterStoredData.Profanities)
                {
                    sb.Replace(profanity, new string('＊', profanity.Length));
                }

                message = sb.ToString();
            }

            message = ReplaceChars(message);

            if (channel == ConVar.Chat.ChatChannel.Global && _configData.ChatSettings.Enabled)
            {
                SendMessage(Lang("Chat", null, ReplaceChars(player.displayName), message), _configData.ChatSettings.WebhookURL);
            }

            if (channel == ConVar.Chat.ChatChannel.Team && _configData.ChatTeamSettings.Enabled)
            {
                SendMessage(Lang("ChatTeam", null, ReplaceChars(player.displayName), message), _configData.ChatTeamSettings.WebhookURL);
            }
        }

        void OnPlayerRespawned(BasePlayer player)
        {
            if (_configData.PlayerRespawnedSettings.Enabled && !string.IsNullOrWhiteSpace(player?.displayName))
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{player.displayName} has been spawned at {GetGridPosition(player.transform.position)}");
                }

                SendMessage(Lang("PlayerRespawned", null, player.displayName, GetGridPosition(player.transform.position)), _configData.PlayerRespawnedSettings.WebhookURL);
            }
        }

        private void OnDangerousEventStarted(Vector3 containerPos)
        {
            HandleDangerousTreasures(containerPos, "DangerousTreasuresStarted");
        }
        private void OnDangerousEventEnded(Vector3 containerPos)
        {
            HandleDangerousTreasures(containerPos, "DangerousTreasuresEnded");
        }

        private void OnUserKicked(IPlayer player, string reason)
        {
            if (_configData.UserKickedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player {player.Name} ({player.Id}) was kicked ({reason})");
                }

                SendMessage(Lang("UserKicked", null, ReplaceChars(player.Name), player.Id, ReplaceChars(reason)), _configData.UserKickedSettings.WebhookURL);
            }
        }

        private void OnUserBanned(string name, string id, string ipAddress, string reason)
        {
            if (_configData.UserBannedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player {name} ({id}) at {ipAddress} was banned: {reason}");
                }

                SendMessage(Lang("UserBanned", null, ReplaceChars(name), id, ipAddress, ReplaceChars(reason)), _configData.UserBannedSettings.WebhookURL);
            }
        }

        private void OnUserUnbanned(string name, string id, string ipAddress)
        {
            if (_configData.UserBannedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player {name} ({id}) at {ipAddress} was unbanned");
                }

                SendMessage(Lang("UserUnbanned", null, ReplaceChars(name), id, ipAddress), _configData.UserBannedSettings.WebhookURL);
            }
        }

        private void OnBetterChatMuted(IPlayer target, IPlayer initiator, string reason)
        {
            if (_configData.UserMutedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{target.Name} was muted by {initiator.Name} for ever ({reason})");
                }

                SendMessage(Lang("UserMuted", null, ReplaceChars(target.Name), ReplaceChars(initiator.Name), "ever", ReplaceChars(reason)), _configData.UserMutedSettings.WebhookURL);
            }
        }

        private void OnBetterChatMuteExpired(IPlayer player)
        {
            if (_configData.UserMutedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{player.Name} was unmuted by SERVER");
                }

                SendMessage(Lang("UserUnmuted", null, ReplaceChars(player.Name), "SERVER"), _configData.UserMutedSettings.WebhookURL);
            }
        }

        private void OnBetterChatTimeMuted(IPlayer target, IPlayer initiator, TimeSpan time, string reason)
        {
            if (_configData.UserMutedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{target.Name} was muted by {initiator.Name} for {time.ToShortString()} ({reason})");
                }

                SendMessage(Lang("UserMuted", null, ReplaceChars(target.Name), ReplaceChars(initiator.Name), time.ToShortString(), ReplaceChars(reason)), _configData.UserMutedSettings.WebhookURL);
            }
        }

        private void OnBetterChatUnmuted(IPlayer target, IPlayer initiator)
        {
            if (_configData.UserMutedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{target.Name} was unmuted by {initiator.Name}");
                }

                SendMessage(Lang("UserUnmuted", null, ReplaceChars(target.Name), ReplaceChars(initiator.Name)), _configData.UserMutedSettings.WebhookURL);
            }
        }

        private void OnUserNameUpdated(string id, string oldName, string newName)
        {
            if (_configData.UserNameUpdateSettings.Enabled && !oldName.Equals(newName) && !oldName.Equals("Unnamed"))
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player name changed from {oldName} to {newName} for ID {id}");
                }

                SendMessage(Lang("UserNameUpdated", null, ReplaceChars(oldName), ReplaceChars(newName), id), _configData.UserNameUpdateSettings.WebhookURL);
            }
        }

        private void OnClanCreate(string tag)
        {
            if (_configData.ClanSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{tag} clan was created");
                }

                SendMessage(Lang("ClanCreated", null, ReplaceChars(tag)), _configData.ClanSettings.WebhookURL);
            }
        }

        private void OnClanDisbanded(string tag)
        {
            if (_configData.ClanSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{tag} clan was disbanded");
                }

                SendMessage(Lang("ClanDisbanded", null, ReplaceChars(tag)), _configData.ClanSettings.WebhookURL);
            }
        }

        private void OnServerMessage(string message, string name, string color, ulong id)
        {
            if (_configData.ServerMessagesSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"ServerMessage: {message}");
                }

                SendMessage(Lang("ServerMessage", null, message), _configData.ServerMessagesSettings.WebhookURL);
            }
        }

        #endregion Events Hooks

        #region Methods

        private string ReplaceChars(string text)
        {
            StringBuilder sb = new StringBuilder(text);
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }
            else
            {
                sb.Replace("*", "＊");
                sb.Replace("`", "'");
                sb.Replace("_", "＿");
                sb.Replace("~", "～");
                sb.Replace("@here", "here");
                sb.Replace("@everyone", "everyone");
                return sb.ToString();
            }
        }

        private void SendMessage(string message, string webhookUrl)
        {

            if (string.IsNullOrWhiteSpace(message))
            {
                PrintError("SendMessage: message is null or empty!");
                return;
            }

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                PrintError("SendMessage: webhookUrl is null or empty!");
                return;
            }

            DiscordMessage discordMessage = new DiscordMessage(message);

            _queueMessages.Add(discordMessage, webhookUrl);

            HandleQueue();
        }

        private void HandleQueue()
        {
            if (!_isConnectionOK)
            {
                if (!_retrying)
                {
                    PrintError($"HandleQueue: Connection is NOT OK! Retrying in {_configData.GlobalSettings.SleepInterval} seconds. Messages in queue: {_queueMessages.Count}");
                    _retrying = true;
                    timer.Once(_configData.GlobalSettings.SleepInterval, () =>
                    {
                        SendDiscordMessage(_lastUrl, _lastMessage);
                        _retrying = false;
                        HandleQueue();
                    });
                }

                return;
            }

            if (_queueProcessing.Count > 0)
            {
                return;
            }

            if (_queueErrorProcessing.Count > 0)
            {
                _queueProcessing = _queueErrorProcessing;
                _queueErrorProcessing = new Hash<DiscordMessage, string> ();
            }
            else
            {
                _queueProcessing = _queueMessages;
                _queueMessages = new Hash<DiscordMessage, string> ();
            }

            timer.Repeat(_configData.GlobalSettings.QueueInterval, _queueProcessing.Count, () =>
            {
                foreach(KeyValuePair<DiscordMessage, string> message in _queueProcessing)
                {
                    SendDiscordMessage(message.Value, message.Key);
                    _queueProcessing.Remove(message.Key);
                    if (!_isConnectionOK)
                    {
                        _queueErrorProcessing = _queueProcessing;
                        _queueProcessing = new Hash<DiscordMessage, string> ();
                    }

                    break;
                }

                if (_queueMessages.Count > 0 && _queueProcessing.Count == 0)
                {
                    HandleQueue();
                }
            });
        }

        private void HandleEntity(BaseEntity baseEntity)
        {
            if (baseEntity == null)
            {
                return;
            }

            EventType eventType = GetEventTypeFromEntity(baseEntity);
            if (eventType == EventType.None)
            {
                PrintError("HandleEntity: eventType == EventType.None ->" + baseEntity.ShortPrefabName);
                return;
            }

            EventSettings eventSettengs = GetEventSettings(eventType);
            if (eventSettengs == null)
            {
                PrintError("HandleEntity: eventSettings == null");
                return;
            }

            if (eventSettengs.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts(eventType.ToString());
                }

                if (eventType == EventType.Helicopter)
                {
                    if (PersonalHeli != null && PersonalHeli.IsLoaded)
                    {
                        if (PersonalHeli.Call<bool>("IsPersonal", baseEntity))
                        {
                            if (_configData.GlobalSettings.LoggingEnabled)
                            {
                                Puts("Personal Helicopter spawned at " + GetGridPosition(baseEntity.transform.position));
                            }

                            SendMessage(Lang("PersonalHelicopter", null, GetGridPosition(baseEntity.transform.position)), eventSettengs.WebhookURL);
                            return;
                        }
                    }

                    if (PersonalHeliExtended != null && PersonalHeliExtended.IsLoaded)
                    {
                        if (PersonalHeliExtended.Call<bool>("IsPersonal", baseEntity))
                        {
                            if (_configData.GlobalSettings.LoggingEnabled)
                            {
                                Puts("Personal Helicopter spawned at " + GetGridPosition(baseEntity.transform.position));
                            }

                            SendMessage(Lang("PersonalHelicopter", null, GetGridPosition(baseEntity.transform.position)), eventSettengs.WebhookURL);
                            return;
                        }
                    }
                }

                SendMessage(Lang(eventType.ToString(), null, GetGridPosition(baseEntity.transform.position)), eventSettengs.WebhookURL);
            }
        }

        private void HandleSupplySignal(BasePlayer player, SupplySignal entity)
        {
            if (_configData.SupplyDropSettings.Enabled)
            {
                NextTick(() =>
                {
                    if (player != null && entity != null)
                    {
                        if (_configData.GlobalSettings.LoggingEnabled)
                        {
                            Puts($"SupplySignal was thrown by {player.displayName} at {GetGridPosition(entity.transform.position)}");
                        }

                        SendMessage(Lang("SupplySignal", null, ReplaceChars(player.displayName), GetGridPosition(entity.transform.position)), _configData.SupplyDropSettings.WebhookURL);
                    }
                });
            }
        }

        private void HandleRaidableBase(Vector3 raidPos, int difficulty, string langKey)
        {
            if (raidPos == null)
            {
                PrintError($"{langKey}: raidPos == null");
                return;
            }

            if (_configData.RaidableBasesSettings.Enabled)
            {
                string difficultyString;
                switch (difficulty)
                {
                    case 0:
                        difficultyString = "Easy";
                        break;
                    case 1:
                        difficultyString = "Medium";
                        break;
                    case 2:
                        difficultyString = "Hard";
                        break;
                    case 3:
                        difficultyString = "Expert";
                        break;
                    case 4:
                        difficultyString = "Nightmare";
                        break;
                    default:
                        PrintError($"{langKey}: Unknown difficulty: {difficulty}");
                        return;
                }

                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts(difficultyString + " Raidable Base at " + GetGridPosition(raidPos) + " is " + (langKey == "RaidableBaseStarted" ? "spawned" : "ended"));
                }

                SendMessage(Lang(langKey, null, GetGridPosition(raidPos), Lang(difficultyString)), _configData.RaidableBasesSettings.WebhookURL);
            }
        }

        private void HandleDangerousTreasures(Vector3 containerPos, string langKey)
        {
            if (containerPos == null)
            {
                PrintError($"{langKey}: containerPos == null");
                return;
            }

            if (_configData.DangerousTreasuresSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts("Dangerous Treasures at " + GetGridPosition(containerPos) + " is " + (langKey == "DangerousTreasuresStarted" ? "spawned" : "ended"));
                }

                SendMessage(Lang(langKey, null, GetGridPosition(containerPos)), _configData.DangerousTreasuresSettings.WebhookURL);
            }
        }

        #endregion Methods

        #region Helpers

        private void UnsubscribeDisabled()
        {
            if (!_configData.UserMutedSettings.Enabled)
            {
                Unsubscribe(nameof(OnBetterChatMuted));
                Unsubscribe(nameof(OnBetterChatMuteExpired));
                Unsubscribe(nameof(OnBetterChatTimeMuted));
                Unsubscribe(nameof(OnBetterChatUnmuted));
            }

            if (!_configData.ClanSettings.Enabled)
            {
                Unsubscribe(nameof(OnClanCreate));
                Unsubscribe(nameof(OnClanDisbanded));
            }

            if (!_configData.DangerousTreasuresSettings.Enabled)
            {
                Unsubscribe(nameof(OnDangerousEventEnded));
                Unsubscribe(nameof(OnDangerousEventStarted));
            }

            if (!_configData.PlayerDeathNotesSettings.Enabled)
            {
                Unsubscribe(nameof(OnDeathNotice));
            }

            if (!_configData.PlayerDeathSettings.Enabled)
            {
                Unsubscribe(nameof(OnEntityDeath));
            }

            if (!_configData.EasterSettings.Enabled &&
                !_configData.HalloweenSettings.Enabled)
            {
                Unsubscribe(nameof(OnEntityKill));
            }

            if (!_configData.BradleySettings.Enabled &&
                !_configData.CargoPlaneSettings.Enabled &&
                !_configData.CargoShipSettings.Enabled &&
                !_configData.ChinookSettings.Enabled &&
                !_configData.ChristmasSettings.Enabled &&
                !_configData.EasterSettings.Enabled &&
                !_configData.HalloweenSettings.Enabled &&
                !_configData.HelicopterSettings.Enabled &&
                !_configData.LockedCrateSettings.Enabled &&
                !_configData.SantaSleighSettings.Enabled &&
                !_configData.SupplyDropSettings.Enabled)
            {
                Unsubscribe(nameof(OnEntitySpawned));
            }

            if (!_configData.SupplyDropSettings.Enabled)
            {
                Unsubscribe(nameof(OnExplosiveDropped));
                Unsubscribe(nameof(OnExplosiveThrown));
                Unsubscribe(nameof(OnSupplyDropLanded));
            }

            if (!_configData.ServerMessagesSettings.Enabled)
            {
                Unsubscribe(nameof(OnServerMessage));
            }

            if (!_configData.PlayerConnectedSettings.Enabled &&
                !_configData.PlayerConnectedInfoSettings.Enabled)
            {
                Unsubscribe(nameof(OnPlayerConnected));
            }

            if (!_configData.ChatSettings.Enabled &&
                !_configData.ChatTeamSettings.Enabled)
            {
                Unsubscribe(nameof(OnPlayerChat));
            }

            if (!_configData.PlayerDisconnectedSettings.Enabled)
            {
                Unsubscribe(nameof(OnPlayerDisconnected));
            }

            if (!_configData.PlayerRespawnedSettings.Enabled)
            {
                Unsubscribe(nameof(OnPlayerRespawned));
            }

            if (!_configData.RaidableBasesSettings.Enabled)
            {
                Unsubscribe(nameof(OnRaidableBaseEnded));
                Unsubscribe(nameof(OnRaidableBaseStarted));
            }

            if (!_configData.RconCommandSettings.Enabled)
            {
                Unsubscribe(nameof(OnRconCommand));
            }

            if (!_configData.RconConnectionSettings.Enabled)
            {
                Unsubscribe(nameof(OnRconConnection));
            }

            if (!_configData.ServerMessagesSettings.Enabled)
            {
                Unsubscribe(nameof(OnServerMessage));
            }

            if (!_configData.UserBannedSettings.Enabled)
            {
                Unsubscribe(nameof(OnUserBanned));
                Unsubscribe(nameof(OnUserUnbanned));
            }

            if (!_configData.UserKickedSettings.Enabled)
            {
                Unsubscribe(nameof(OnUserKicked));
            }

            if (!_configData.UserNameUpdateSettings.Enabled)
            {
                Unsubscribe(nameof(OnUserNameUpdated));
            }
        }

        private static EventType GetEventTypeFromEntity(BaseEntity baseEntity)
        {
            if (baseEntity is BaseHelicopter) return EventType.Helicopter;
            if (baseEntity is BradleyAPC) return EventType.Bradley;
            if (baseEntity is CargoPlane) return EventType.CargoPlane;
            if (baseEntity is CargoShip) return EventType.CargoShip;
            if (baseEntity is HackableLockedCrate) return EventType.LockedCrate;
            if (baseEntity is SupplyDrop) return EventType.SupplyDrop;
            if (baseEntity is SupplySignal) return EventType.SupplyDrop;
            if (baseEntity is CH47HelicopterAIController) return EventType.Chinook;
            if (baseEntity is SantaSleigh) return EventType.SantaSleigh;
            if (baseEntity is HalloweenHunt) return EventType.Halloween;
            if (baseEntity is EggHuntEvent) return EventType.Easter;
            if (baseEntity is XMasRefill) return EventType.Christmas;

            return EventType.None;
        }

        private EventSettings GetEventSettings(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Bradley: return _configData.BradleySettings;
                case EventType.CargoPlane: return _configData.CargoPlaneSettings;
                case EventType.CargoShip: return _configData.CargoShipSettings;
                case EventType.Chinook: return _configData.ChinookSettings;
                case EventType.Christmas: return _configData.ChristmasSettings;
                case EventType.DangerousTreasures: return _configData.DangerousTreasuresSettings;
                case EventType.Death: return _configData.PlayerDeathSettings;
                case EventType.DeathNotes: return _configData.PlayerDeathNotesSettings;
                case EventType.Easter: return _configData.EasterSettings;
                case EventType.Halloween: return _configData.HalloweenSettings;
                case EventType.Helicopter: return _configData.HelicopterSettings;
                case EventType.LockedCrate: return _configData.LockedCrateSettings;
                case EventType.PlayerConnected: return _configData.PlayerConnectedSettings;
                case EventType.PlayerConnectedInfo: return _configData.PlayerConnectedInfoSettings;
                case EventType.PlayerDisconnected: return _configData.PlayerDisconnectedSettings;
                case EventType.RaidableBases: return _configData.RaidableBasesSettings;
                case EventType.SantaSleigh: return _configData.SantaSleighSettings;
                case EventType.SupplyDrop: return _configData.SupplyDropSettings;
                default:
                    PrintError($"GetEventSettings: Unknown EventType: {eventType}");
                    return null;
            }
        }

        private bool IsEntityInList(uint networkId)
        {
            if (_lastEntities.Count > 0)
            {
                List<uint> entitiesToRemove = Pool.GetList<uint>();

                foreach (KeyValuePair<uint, float> entity in _lastEntities)
                {
                    if (entity.Value > Time.realtimeSinceStartup)
                    {
                        entitiesToRemove.Add(entity.Key);
                    }
                }

                foreach (uint entity in entitiesToRemove)
                {
                    _lastEntities.Remove(entity);
                }

                Pool.FreeList(ref entitiesToRemove);

                if (_lastEntities.ContainsKey(networkId))
                {
                    return true;
                }
            }

            return false;
        }

        private string StripRustTags(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            foreach (string tag in _tags)
            {
                text = text.Replace(tag, _configData.GlobalSettings.TagsReplacement);
            }

            foreach (Regex regexTag in _regexTags)
            {
                text = regexTag.Replace(text, _configData.GlobalSettings.TagsReplacement);
            }

            return text;
        }

        private string GetGridPosition(Vector3 pos) => PhoneController.PositionToGridCoord(pos);

        #endregion Helpers

        #region Discord Embed

        #region Send Embed Methods
        /// <summary>
        /// Headers when sending an embeded message
        /// </summary>
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>()
        {
            {"Content-Type", "application/json"}
        };

        /// <summary>
        /// Sends the DiscordMessage to the specified webhook url
        /// </summary>
        /// <param name="url">Webhook url</param>
        /// <param name="message">Message being sent</param>
        private void SendDiscordMessage(string url, DiscordMessage message)
        {
            _lastMessage = message;
            _lastUrl = url;
            webrequest.Enqueue(url, message.ToJson(), SendDiscordMessageCallback, this, RequestMethod.POST, _headers);
        }

        /// <summary>
        /// Callback when sending the embed if any errors occured
        /// </summary>
        /// <param name="code">HTTP response code</param>
        /// <param name="message">Response message</param>
        private void SendDiscordMessageCallback(int code, string message)
        {
            switch (code)
            {
                case 204:
                    _isConnectionOK = true;
                    break;
                case 429:
                    _isConnectionOK = false;
                    PrintError("You are being rate limited. To avoid this try to increase queue interval in your config file.");
                    break;
                default:
                    _isConnectionOK = false;
                    PrintError($"SendDiscordMessageCallback: code = {code} message = {message}");
                    break;
            }
        }
        #endregion Send Embed Methods

        #region Embed Classes

        private class DiscordMessage
        {
            /// <summary>
            /// The name of the user sending the message changing this will change the webhook bots name
            /// </summary>
            [JsonProperty("username")]
            private string Username { get; set; }

            /// <summary>
            /// The avatar url of the user sending the message changing this will change the webhook bots avatar
            /// </summary>
            [JsonProperty("avatar_url")]
            private string AvatarUrl { get; set; }

            /// <summary>
            /// String only content to be sent
            /// </summary>
            [JsonProperty("content")]
            private string Content { get; set; }

            public DiscordMessage(string content, string username = null, string avatarUrl = null)
            {
                Content = content;
                Username = username;
                AvatarUrl = avatarUrl;
            }

            /// <summary>
            /// Adds string content to the message
            /// </summary>
            /// <param name="content"></param>
            /// <returns></returns>
            public DiscordMessage AddContent(string content)
            {
                Content = content;
                return this;
            }

            /// <summary>
            /// Changes the username and avatar image for the bot sending the message
            /// </summary>
            /// <param name="username">username to change</param>
            /// <param name="avatarUrl">avatar img url to change</param>
            /// <returns>This</returns>
            public DiscordMessage AddSender(string username, string avatarUrl)
            {
                Username = username;
                AvatarUrl = avatarUrl;
                return this;
            }

            /// <summary>
            /// Returns message as JSON to be sent in the web request
            /// </summary>
            /// <returns></returns>
            public string ToJson() => JsonConvert.SerializeObject(this, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
        }
        #endregion Embed Classes

        #endregion Discord Embed
    }
}