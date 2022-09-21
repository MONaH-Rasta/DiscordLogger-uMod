using Newtonsoft.Json;
using Oxide.Core.Plugins;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Discord Events", "MON@H", "0.1.5")]
    [Description("Displays events to a discord channel")]
    internal class DiscordEvents : CovalencePlugin
    {
        #region Class Fields
        [PluginReference] private Plugin DiscordMessages, AutomatedEvents, PersonalHeli, RaidableBases;
        private PluginConfig _pluginConfig;
        private Dictionary<uint, DateTime> _lastEntities = new Dictionary<uint, DateTime>();
        private float _half;
        private List<string> _queue = new List<string>();
        #endregion

        #region Setup & Loading
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["AirDropIncoming"] = ":parachute: Airdrop incoming to `{0}`",
                ["AirDropLanded"] = ":gift: Airdrop landed to `{0}`",
                ["Bradley"] = ":crossed_swords: Bradley spawned `{0}`",
                ["CargoPlane"] = ":airplane: Cargo Plane incoming `{0}`",
                ["CargoShip"] = ":ship: Cargo Ship incoming `{0}`",
                ["Chinook"] = ":helicopter: Chinook 47 incoming `{0}`",
                ["Christmas"] = ":christmas_tree: Christmas event started",
                ["Easter"] = ":egg: Easter event started",
                ["Halloween"] = ":mage: Halloween event started",
                ["Helicopter"] = ":crossed_swords: Helicopter incoming `{0}`",
                ["Initialized"] = ":ballot_box_with_check: Server is online again!",
                ["LockedCrate"] = ":package: Codelocked crate is here `{0}`",
                ["PersonalHelicopter"] = ":crossed_swords: Personal Helicopter incoming `{0}`",
                ["RaidableBaseEnded"] = ":homes: {1} Raidable Base at `{0}` is ended",
                ["RaidableBaseStarted"] = ":homes: {1} Raidable Base spawned at `{0}`",
                ["SantaSleigh"] = ":santa: SantaSleigh Event started",
                ["Shutdown"] = ":stop_sign: Server is shutting down!",

                ["Easy"] = "Easy",
                ["Medium"] = "Medium",
                ["Hard"] = "Hard",
            }, this);
        }
        
        private void OnServerInitialized()
        {
            if (DiscordMessages == null || !DiscordMessages.IsLoaded)
            {
                PrintError("Missing plugin dependency DiscordMessages: https://umod.org/plugins/discord-messages");
                return;
            }

            if (string.IsNullOrEmpty(_pluginConfig.WebhookURL))
            {
                PrintError("WebhookURL is null or emply, set it correctly and then restart plugin");
                return;
            }

            _half = World.Size / 2.0f;
            Subscribe(nameof(OnAutoEventTriggered));
            Subscribe(nameof(OnRaidableBaseStarted));
            Subscribe(nameof(OnRaidableBaseEnded));
        }

        private void OnServerInitialized(bool isStartup)
        {
            timer.Once(5f, () => SendMsgToChannel(Lang("Initialized")));
        }

        private void OnPluginLoaded(Plugin plugin)
        {
            switch (plugin.Title)
            {
                case "AutomatedEvents":
                    {
                        AutomatedEvents = plugin;
                        break;
                    }
                case "DiscordMessages":
                    {
                        DiscordMessages = plugin;
                        break;
                    }
                case "PersonalHeli":
                    {
                        PersonalHeli = plugin;
                        break;
                    }
                case "RaidableBases":
                    {
                        RaidableBases = plugin;
                        break;
                    }
            }
        }

        private void OnPluginUnloaded(Plugin plugin)
        {
            switch (plugin.Title)
            {
                case "AutomatedEvents":
                    {
                        AutomatedEvents = null;
                        break;
                    }
                case "DiscordMessages":
                    {
                        DiscordMessages = null;
                        break;
                    }
                case "PersonalHeli":
                    {
                        PersonalHeli = null;
                        break;
                    }
                case "RaidableBases":
                    {
                        RaidableBases = null;
                        break;
                    }
            }
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Loading Default Config");
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
            _pluginConfig = AdditionalConfig(Config.ReadObject<PluginConfig>());
            Config.WriteObject(_pluginConfig);
        }

        private PluginConfig AdditionalConfig(PluginConfig config)
        {
            return config;
        }
        #endregion

        #region Event Hook
        private void OnAutoEventTriggered(string eventTypeStr, BaseEntity eventEntity, bool once)
        {
            if (eventTypeStr == null || eventEntity == null)
                return;

            switch (eventTypeStr)
            {
                case "AirDrop":
                case "Bradley":
                case "Cargo Ship":
                case "Chinook":
                case "Helicopter":
                    return;
                case "FancyDrop":
                    SendMsgToChannel(Lang("AirDropIncoming", null, GetGridPosition(eventEntity.transform.position)));
                    return;
            }
            Puts("AutoEvent: " + eventTypeStr);
            if (eventEntity != null)
            {
                SendMsgToChannel(Lang(eventTypeStr.Replace(" ", string.Empty), null, GetGridPosition(eventEntity.transform.position)));
            }
            else
            {
                SendMsgToChannel(Lang(eventTypeStr.Replace(" ", string.Empty)));
            }
        }

        private void OnEntitySpawned(BradleyAPC entity)
        {
            SendMsgToChannel(Lang("Bradley", null, GetGridPosition(entity.transform.position)));
        }

        private void OnEntitySpawned(BaseHelicopter entity)
        {
            NextTick(() => {
                if (PersonalHeli != null && PersonalHeli.IsLoaded)
                {
                    if (PersonalHeli.Call<bool>("IsPersonal", entity))
                    {
                        SendMsgToChannel(Lang("PersonalHelicopter", null, GetGridPosition(entity.transform.position)));
                        return;
                    }
                }

                SendMsgToChannel(Lang("Helicopter", null, GetGridPosition(entity.transform.position)));
            });
        }

        private void OnEntitySpawned(CH47Helicopter entity)
        {
            SendMsgToChannel(Lang("Chinook", null, GetGridPosition(entity.transform.position)));
        }

        private void OnEntitySpawned(HackableLockedCrate entity)
        {
            SendMsgToChannel(Lang("LockedCrate", null, GetGridPosition(entity.transform.position)));
        }

        private void OnEntitySpawned(SupplyDrop entity)
        {
            SendMsgToChannel(Lang("AirDropIncoming", null, GetGridPosition(entity.transform.position)));
        }

        private void OnSupplyDropLanded(SupplyDrop entity)
        {
            if (entity == null) return;
            if (IsEntityInList(entity.net.ID)) return;

            SendMsgToChannel(Lang("AirDropLanded", null, GetGridPosition(entity.transform.position)));
            _lastEntities.Add(entity.net.ID, DateTime.Now.Add(TimeSpan.FromSeconds(30)));
        }

        private void OnRaidableBaseStarted(Vector3 raidPos, int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    SendMsgToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang("Easy")));
                    return;
                case 2:
                    SendMsgToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang("Medium")));
                    return;
                case 3:
                    SendMsgToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang("Hard")));
                    return;
            }
        }
        private void OnRaidableBaseEnded(Vector3 raidPos, int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    SendMsgToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang("Easy")));
                    return;
                case 2:
                    SendMsgToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang("Medium")));
                    return;
                case 3:
                    SendMsgToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang("Hard")));
                    return;
            }
        }

        void OnServerShutdown()
        {
            SendMsgToChannel(Lang("Shutdown"));
        }

        #endregion

        #region Helpers

        private bool IsEntityInList(uint networkID)
        {
            if (_lastEntities != null)
            {
                foreach (var entity in _lastEntities)
                {
                    if (entity.Value < DateTime.Now)
                    {
                        _lastEntities.Remove(entity.Key);
                        break;
                    }
                }

                if (_lastEntities.ContainsKey(networkID))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetGridPosition(Vector3 pos)
        {
            const float gridCellSize = 146.3f;
            
            int xGrid = (int) Mathf.Floor((pos.x + _half) / gridCellSize);
            int zGrid = (int) Mathf.Floor((_half - pos.z - 100) / gridCellSize);
            if (xGrid < 0)
            {
                xGrid = 0;
            }

            string extraA = string.Empty;
            if (xGrid / 26 > 0)
            {
                extraA = $"{(char) ('A' + (xGrid - 26) / 26)}";
            }

            if (zGrid < 0)
            {
                zGrid = 0;
            }

            xGrid %= 26;
            return $"{extraA}{(char) ('A' + xGrid)}{zGrid}";
        }

        private void SendMsgToChannel(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (string.IsNullOrEmpty(message)) return;
            if (string.IsNullOrEmpty(_pluginConfig.WebhookURL))
            {
                PrintError("WebhookURL is null or emply, set it correctly and then restart plugin");
                return;
            }
            if (DiscordMessages == null || !DiscordMessages.IsLoaded)
            {
                _queue.Add(message);
                return;
            }

            for (var i = 0; i < _queue.Count; i++)
            {
                var msg = _queue[i];
                DiscordMessages?.Call("API_SendTextMessage", _pluginConfig.WebhookURL, msg, false, this);
                _queue.Remove(msg);
            }

            DiscordMessages?.Call("API_SendTextMessage", _pluginConfig.WebhookURL, message, false, this);
        }

        private string Lang(string key, BasePlayer player = null, params object[] args)
        {
            try
            {
                return string.Format(lang.GetMessage(key, this, player?.UserIDString), args);
            }
            catch (Exception ex)
            {
                PrintError($"Lang Key '{key}' threw exception\n:{ex}");
                throw;
            }
        }
        #endregion

        #region Classes
        private class PluginConfig
        {
            [DefaultValue("")]
            [JsonProperty("Events WebhookURL")]
            public string WebhookURL { get; set; }
        }
        #endregion
    }
}