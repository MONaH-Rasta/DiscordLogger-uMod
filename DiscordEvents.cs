using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using Time = UnityEngine.Time;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Discord Events", "MON@H", "0.2.1")]
    [Description("Displays events to a discord channel")]
    internal class DiscordEvents : CovalencePlugin
    {
        #region Class Fields

        [PluginReference] private Plugin PersonalHeli, RaidableBases;

        private Dictionary<uint, float> _lastEntities = new Dictionary<uint, float>();

        private enum EventType
        {
            Bradley,
            CargoPlane,
            CargoShip,
            Chinook,
            Christmas,
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
            SupplyDrop
        }

        #endregion Class Fields

        #region Initialization

        private void OnServerInitialized(bool isStartup)
        {
            UnsubscribeDisabled();

            if (isStartup && _configData.ServerStateSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts("Server is online again!");
                }

                SendMsgToChannel(Lang("Initialized"), _configData.ServerStateSettings.WebhookURL);
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

                SendMsgToChannel(Lang("Shutdown"), _configData.ServerStateSettings.WebhookURL);
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

            [JsonProperty(PropertyName = "CH47 Helicopter settings")]
            public EventSettings ChinookSettings = new EventSettings();

            [JsonProperty(PropertyName = "Christmas settings")]
            public EventSettings ChristmasSettings = new EventSettings();

            [JsonProperty(PropertyName = "Easter settings")]
            public EventSettings EasterSettings = new EventSettings();

            [JsonProperty(PropertyName = "Hackable Locked Crate settings")]
            public EventSettings LockedCrateSettings = new EventSettings();

            [JsonProperty(PropertyName = "Halloween settings")]
            public EventSettings HalloweenSettings = new EventSettings();

            [JsonProperty(PropertyName = "Helicopter settings")]
            public EventSettings HelicopterSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player connect advanced info settings")]
            public EventSettings PlayerConnectedInfoSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player connect settings")]
            public EventSettings PlayerConnectedSettings = new EventSettings();

            [JsonProperty(PropertyName = "Player disconnect settings")]
            public EventSettings PlayerDisconnectedSettings = new EventSettings();

            [JsonProperty(PropertyName = "Raidable Bases settings")]
            public EventSettings RaidableBasesSettings = new EventSettings();

            [JsonProperty(PropertyName = "SantaSleigh settings")]
            public EventSettings SantaSleighSettings = new EventSettings();

            [JsonProperty(PropertyName = "Server state settings")]
            public EventSettings ServerStateSettings = new EventSettings();

            [JsonProperty(PropertyName = "Supply Drop settings")]
            public EventSettings SupplyDropSettings = new EventSettings();
        }

        private class GlobalSettings
        {
            [JsonProperty(PropertyName = "Log to console?")]
            public bool LoggingEnabled = true;
        }

        private class EventSettings
        {
            [JsonProperty(PropertyName = "WebhookURL")]
            public string WebhookURL = "";

            [JsonProperty(PropertyName = "Enabled?")]
            public bool Enabled = true;
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

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Bradley"] = ":crossed_swords: Bradley spawned `{0}`",
                ["CargoPlane"] = ":airplane: Cargo Plane incoming `{0}`",
                ["CargoShip"] = ":ship: Cargo Ship incoming `{0}`",
                ["Chinook"] = ":helicopter: Chinook 47 incoming `{0}`",
                ["Christmas"] = ":christmas_tree: Christmas event started",
                ["Easter"] = ":egg: Easter event started",
                ["EasterWinner"] = ":egg: Easter event ended. The winner is `{0}`",
                ["Halloween"] = ":mage: Halloween event started",
                ["HalloweenWinner"] = ":mage: Halloween event ended. The winner is `{0}`",
                ["Helicopter"] = ":crossed_swords: Helicopter incoming `{0}`",
                ["Initialized"] = ":ballot_box_with_check: Server is online again!",
                ["LockedCrate"] = ":package: Codelocked crate is here `{0}`",
                ["PersonalHelicopter"] = ":crossed_swords: Personal Helicopter incoming `{0}`",
                ["PlayerConnected"] = ":white_check_mark: {0} connected",
                ["PlayerConnectedInfo"] = ":detective: {0} connected. SteamID: `{1}` IP: `{2}`",
                ["PlayerDisconnected"] = ":x: {0} disconnected ({1})",
                ["RaidableBaseEnded"] = ":homes: {1} Raidable Base at `{0}` is ended",
                ["RaidableBaseStarted"] = ":homes: {1} Raidable Base spawned at `{0}`",
                ["SantaSleigh"] = ":santa: SantaSleigh Event started",
                ["Shutdown"] = ":stop_sign: Server is shutting down!",
                ["SupplyDrop"] = ":parachute: SupplyDrop incoming at `{0}`",
                ["SupplyDropLanded"] = ":gift: SupplyDrop landed at `{0}`",

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
                handleEntity(entity);
            });
        }

        private void OnEntitySpawned(BradleyAPC entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(CargoPlane entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(CargoShip entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(CH47HelicopterAIController entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(EggHuntEvent entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(HackableLockedCrate entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(SantaSleigh entity)
        {
            handleEntity(entity);
        }
        private void OnEntitySpawned(SupplyDrop entity)
        {
            handleEntity(entity);
        }

        private void OnEntitySpawned(XMasRefill entity)
        {
            handleEntity(entity);
        }

        private void OnEntityKill(EggHuntEvent entity)
        {
            if (entity == null)
            {
                return;
            }

            var winners = entity.GetTopHunters();
            string winner;
            if (winners.Count > 0)
            {
                winner = winners[0].displayName;
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

                    SendMsgToChannel(Lang("HalloweenWinner", null, winner), _configData.HalloweenSettings.WebhookURL);
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

                    SendMsgToChannel(Lang("EasterWinner", null, winner), _configData.EasterSettings.WebhookURL);
                }
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

                SendMsgToChannel(Lang("SupplyDropLanded", null, GetGridPosition(entity.transform.position)), _configData.SupplyDropSettings.WebhookURL);
                _lastEntities.Add(entity.net.ID, Time.realtimeSinceStartup + 60);
            }
        }

        private void OnRaidableBaseStarted(Vector3 raidPos, int difficulty)
        {
            if (raidPos == null)
            {
                PrintError("OnRaidableBaseStarted: raidPos == null");
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
                        PrintError($"OnRaidableBaseStarted: Unknown difficulty: {difficulty}");
                        return;
                }

                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{difficultyString} " + " Raidable Base spawned at " + GetGridPosition(raidPos));
                }

                SendMsgToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang(difficultyString)), _configData.RaidableBasesSettings.WebhookURL);
            }
        }
        private void OnRaidableBaseEnded(Vector3 raidPos, int difficulty)
        {
            if (raidPos == null)
            {
                PrintError("OnRaidableBaseStarted: raidPos == null");
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
                        PrintError($"OnRaidableBaseStarted: Unknown difficulty: {difficulty}");
                        return;
                }

                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"{difficultyString} Raidable Base at {GetGridPosition(raidPos)} ended");
                }

                SendMsgToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang(difficultyString)), _configData.RaidableBasesSettings.WebhookURL);
            }
        }

        void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || !player.IsConnected)
            {
                return;
            }

            if (_configData.PlayerConnectedSettings.Enabled)
            {
                if (_configData.GlobalSettings.LoggingEnabled)
                {
                    Puts($"Player {player.displayName} connected.");
                }

                SendMsgToChannel(Lang("PlayerConnected", null, player.displayName), _configData.PlayerConnectedSettings.WebhookURL);
            }

            if (_configData.PlayerConnectedInfoSettings.Enabled)
            {
                SendMsgToChannel(Lang("PlayerConnectedInfo", null, player.displayName, player.UserIDString, player.net.connection.ipaddress.Split(':')[0]), _configData.PlayerConnectedInfoSettings.WebhookURL);
            }
        }

        void OnPlayerDisconnected(BasePlayer player, string reason)
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

                SendMsgToChannel(Lang("PlayerDisconnected", null, player.displayName, reason), _configData.PlayerConnectedSettings.WebhookURL);
            }
        }

        #endregion Events Hooks

        #region Helpers

        private void handleEntity(BaseEntity baseEntity)
        {
            if (baseEntity == null)
            {
                return;
            }

            EventType eventType = GetEventTypeFromEntity(baseEntity);
            if (eventType == EventType.None)
            {
                PrintError("handleEntity: eventType == EventType.None ->" + baseEntity.ShortPrefabName);
                return;                
            }

            var eventSettengs = GetEventSettings(eventType);
            if (eventSettengs == null)
            {
                PrintError("handleEntity: eventSettengs == null");
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

                            SendMsgToChannel(Lang("PersonalHelicopter", null, GetGridPosition(baseEntity.transform.position)), eventSettengs.WebhookURL);
                            return;
                        }
                    }
                }

                SendMsgToChannel(Lang(eventType.ToString(), null, GetGridPosition(baseEntity.transform.position)), eventSettengs.WebhookURL);
            }
        }

        private void UnsubscribeDisabled()
        {
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

            if (!_configData.PlayerConnectedSettings.Enabled &&
                !_configData.PlayerConnectedInfoSettings.Enabled)
            {
                Unsubscribe(nameof(OnPlayerConnected));
            }

            if (!_configData.PlayerDisconnectedSettings.Enabled)
            {
                Unsubscribe(nameof(OnPlayerDisconnected));
            }

            if (!_configData.RaidableBasesSettings.Enabled)
            {
                Unsubscribe(nameof(OnRaidableBaseEnded));
                Unsubscribe(nameof(OnRaidableBaseStarted));
            }

            if (!_configData.SupplyDropSettings.Enabled)
            {
                Unsubscribe(nameof(OnSupplyDropLanded));
            }
        }

        private static EventType GetEventTypeFromEntity(BaseEntity baseEntity)
        {
            if (baseEntity is BaseHelicopter) return EventType.Helicopter;
            if (baseEntity is BradleyAPC) return EventType.Bradley;
            if (baseEntity is CargoPlane) return EventType.CargoPlane;
            if (baseEntity is CargoShip) return EventType.CargoShip;
            if (baseEntity is HalloweenHunt) return EventType.Halloween;
            if (baseEntity is EggHuntEvent) return EventType.Easter;
            if (baseEntity is HackableLockedCrate) return EventType.LockedCrate;
            if (baseEntity is SantaSleigh) return EventType.SantaSleigh;
            if (baseEntity is SupplyDrop) return EventType.SupplyDrop;
            if (baseEntity is XMasRefill) return EventType.Christmas;
            var controller = baseEntity as CH47HelicopterAIController;
            if (controller != null && controller.landingTarget == Vector3.zero) return EventType.Chinook;
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
            if (_lastEntities != null)
            {
                foreach (var entity in _lastEntities)
                {
                    if (entity.Value < Time.realtimeSinceStartup)
                    {
                        _lastEntities.Remove(entity.Key);
                    }
                }

                if (_lastEntities.ContainsKey(networkId))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetGridPosition(Vector3 pos)
        {
            const float gridCellSize = 146.3f;
            
            int maxGridSize = Mathf.FloorToInt(World.Size / gridCellSize) - 1;
            float halfWorldSize = World.Size / 2f;
            int xGrid = Mathf.Clamp(Mathf.FloorToInt((pos.x + halfWorldSize) / gridCellSize),0, maxGridSize);
            int zGrid = Mathf.Clamp(maxGridSize - Mathf.FloorToInt((pos.z + halfWorldSize) / gridCellSize),0, maxGridSize);
            
            string extraA = string.Empty;
            if (xGrid > 26)
            {
                extraA = $"{(char) ('A' + (xGrid / 26 - 1))}";
            }
            
            return $"{extraA}{(char) ('A' + xGrid % 26)}{zGrid.ToString()}";
        }

        private void SendMsgToChannel(string message, string webhookUrl)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrEmpty(message))
            {
                PrintError("SendMsgToChannel: message is null or empty!");
                return;
            }

            if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrEmpty(webhookUrl))
            {
                PrintError("SendMsgToChannel: webhookUrl is null or empty!");
                return;
            }

            DiscordMessage discordMessage = new DiscordMessage(message);
            
            SendDiscordMessage(webhookUrl, discordMessage);
        }

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
            string json = message.ToJson();
            webrequest.Enqueue(url, json, SendDiscordMessageCallback, this, RequestMethod.POST, _headers);
        }

        /// <summary>
        /// Callback when sending the embed if any errors occured
        /// </summary>
        /// <param name="code">HTTP response code</param>
        /// <param name="message">Response message</param>
        private void SendDiscordMessageCallback(int code, string message)
        {
            if (code != 204)
            {
                PrintError(message);
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