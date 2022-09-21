using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.DiscordObjects;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Discord Events", "MON@H", "0.0.7")]
    [Description("Displays events to a discord channel")]
    internal class DiscordEvents : CovalencePlugin
    {
        #region Class Fields
        [PluginReference] private readonly Plugin DiscordCore, AutomatedEvents, PersonalHeli, RaidableBases;
        private PluginConfig _pluginConfig;

        private bool _init;
        private Dictionary<uint, DateTime> _lastEntities = new Dictionary<uint, DateTime>();
        private float _half;
        #endregion

        #region Setup & Loading
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["AirDropIncoming"] = ":parachute: Airdrop incoming to `{0}`",
                ["AirDropLanded"] = ":gift: Airdrop landed to `{0}`",
                ["Bradley"] = ":crossed_swords: Bradley spawned",
                ["CargoPlane"] = ":airplane: Cargo Plane incoming",
                ["CargoShip"] = ":ship: Cargo Ship incoming",
                ["Chinook"] = ":helicopter: Chinook 47 incoming",
                ["Christmas"] = ":christmas_tree: Christmas event started",
                ["Easter"] = ":egg: Easter event started",
                ["Halloween"] = ":mage: Halloween event started",
                ["Helicopter"] = ":crossed_swords: Helicopter incoming",
                ["Initialized"] = ":ballot_box_with_check: Server is online again!",
                ["LockedCrate"] = ":package: Codelocked crate is here (`{0}`)",
                ["PersonalHelicopter"] = ":crossed_swords: Personal Helicopter incoming",
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
            if (DiscordCore == null)
            {
                PrintError("Missing plugin dependency DiscordCore: https://umod.org/plugins/discord-core");
                return;
            }

            OnDiscordCoreReady();
            _half = World.Size / 2.0f;
        }

        private void OnServerInitialized(bool isStartup)
        {
            SendChatToChannel(Lang("Initialized"));
        }

        private void OnDiscordCoreReady()
        {
            if (!(DiscordCore?.Call<bool>("IsReady") ?? false))
            {
                return;
            }

            Channel channel = DiscordCore.Call<Channel>("GetChannel", _pluginConfig.EventsChannel);
            if (channel == null)
            {
                PrintError($"Failed to find a channel with the name or id {_pluginConfig.EventsChannel} in the discord");
            }

            _init = true;
            Subscribe(nameof(OnAutoEventTriggered));
            Subscribe(nameof(OnRaidableBaseStarted));
            Subscribe(nameof(OnRaidableBaseEnded));
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
                    SendChatToChannel(Lang("AirDropIncoming", null, GetGridPosition(eventEntity.transform.position)));
                    return;
            }
            Puts("AutoEvent: " + eventTypeStr);
            SendChatToChannel(Lang(eventTypeStr.Replace(" ", string.Empty)));
        }

        private void OnEntitySpawned(BradleyAPC entity)
        {
            SendChatToChannel(Lang("Bradley"));
        }

        private void OnEntitySpawned(BaseHelicopter entity)
        {
            NextTick(() => {
                if (PersonalHeli != null && PersonalHeli.IsLoaded)
                {
                    if (PersonalHeli.Call<bool>("IsPersonal", entity))
                    {
                        SendChatToChannel(Lang("PersonalHelicopter"));
                        return;
                    }
                }

                SendChatToChannel(Lang("Helicopter"));
            });
        }

        private void OnEntitySpawned(CH47Helicopter entity)
        {
            SendChatToChannel(Lang("Chinook"));
        }

        private void OnEntitySpawned(HackableLockedCrate entity)
        {
            SendChatToChannel(Lang("LockedCrate", null, GetGridPosition(entity.transform.position)));
        }

        private void OnEntitySpawned(SupplyDrop entity)
        {
            SendChatToChannel(Lang("AirDropIncoming", null, GetGridPosition(entity.transform.position)));
        }

        private void OnSupplyDropLanded(SupplyDrop entity)
        {
            if (entity == null) return;
            if (IsEntityInList(entity.net.ID)) return;

            SendChatToChannel(Lang("AirDropLanded", null, GetGridPosition(entity.transform.position)));
            _lastEntities.Add(entity.net.ID, DateTime.Now.Add(TimeSpan.FromSeconds(30)));
        }

        private void OnRaidableBaseStarted(Vector3 raidPos, int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    SendChatToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang("Easy")));
                    return;
                case 2:
                    SendChatToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang("Medium")));
                    return;
                case 3:
                    SendChatToChannel(Lang("RaidableBaseStarted", null, GetGridPosition(raidPos), Lang("Hard")));
                    return;
            }
        }
        private void OnRaidableBaseEnded(Vector3 raidPos, int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    SendChatToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang("Easy")));
                    return;
                case 2:
                    SendChatToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang("Medium")));
                    return;
                case 3:
                    SendChatToChannel(Lang("RaidableBaseEnded", null, GetGridPosition(raidPos), Lang("Hard")));
                    return;
            }
        }

        void OnServerShutdown()
        {
            SendChatToChannel(Lang("Shutdown"));
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

        private void SendChatToChannel(string message)
        {
            if (!_init) return;
            DiscordCore.Call("SendMessageToChannel", _pluginConfig.EventsChannel, message);
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
            [DefaultValue("Events")]
            [JsonProperty("Events Channel Name or Id")]
            public string EventsChannel { get; set; }
        }
        #endregion
    }
}
