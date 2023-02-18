using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using ServerSync;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BetterBosses
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class CreatureManagerModTemplatePlugin : BaseUnityPlugin
    {
        internal const string ModName = "BetterBosses";
        internal const string ModVersion = "1.1.0";
        internal const string Author = "NoobMods";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource CreatureManagerModTemplateLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        [HarmonyPatch(typeof(Character), "SetupMaxHealth")]
        public class SetupMaxHealth_Patch
        {
            private static bool Prefix(Character __instance)
            {
                if ((UnityEngine.Object)(object)__instance != null && !__instance.IsPlayer() && (UnityEngine.Object)(object)Player.m_localPlayer != null)
                {
                    float scale = 1.0f;
                    if (__instance.m_name == "$enemy_eikthyr")
                    {
                        scale = bossHealthMultiplierEikthyr.Value;
                    }
                    if (__instance.m_name == "$enemy_gdking")
                    {
                        scale = bossHealthMultiplierElder.Value;
                    }
                    if (__instance.m_name == "$enemy_bonemass")
                    {
                        scale = bossHealthMultiplierBonemass.Value;
                    }
                    else if (__instance.m_name == "$enemy_dragon")
                    {
                        scale = bossHealthMultiplierModer.Value;
                    }
                    else if (__instance.m_name == "$enemy_goblinking")
                    {
                        scale = bossHealthMultiplierYagluth.Value;
                    }
                    else if (__instance.m_name == "$enemy_seekerqueen")
                    {
                        scale = bossHealthMultiplierSeekerQueen.Value;
                    }
                    else
                    {
                        scale = mobsHealthMultiplier.Value;
                    }

                    float health = __instance.m_health * scale;
                    health = ((health > __instance.m_health) ? health : __instance.m_health);

                    MonoBehaviour.print(__instance.name +  " new health: " + health);

                    __instance.SetMaxHealth(health);
                    return false;
                }
                return true;
            }
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            bossHealthMultiplierEikthyr     = config("2 - Bosses", "1 - Eikthyr Health Multiplier"      , 1.0f, new ConfigDescription("Increases the health of Moder with this multiplier"      , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));
            bossHealthMultiplierElder       = config("2 - Bosses", "2 - The Elder Health Multiplier"    , 1.0f, new ConfigDescription("Increases the health of the Elder with this multiplier"  , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));
            bossHealthMultiplierBonemass    = config("2 - Bosses", "3 - Bonemass Health Multiplier"     , 1.0f, new ConfigDescription("Increases the health of Bonemass with this multiplier"   , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));
            bossHealthMultiplierModer       = config("2 - Bosses", "4 - Moder Health Multiplier"        , 1.0f, new ConfigDescription("Increases the health of Moder with this multiplier"      , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));
            bossHealthMultiplierYagluth     = config("2 - Bosses", "5 - Yagluth Health Multiplier"      , 1.0f, new ConfigDescription("Increases the health of Moder with this multiplier"      , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));
            bossHealthMultiplierSeekerQueen = config("2 - Bosses", "6 - The Queen Health Multiplier"    , 1.0f, new ConfigDescription("Increases the health of the Queen with this multiplier"  , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));
            mobsHealthMultiplier            = config("3 - Mobs",   "Mobs Health Multiplier"             , 1.0f, new ConfigDescription("Increases the health of the mobs with this multiplier"   , (AcceptableValueBase)(object)new AcceptableValueRange<float>(1.0f, 20.0f), Array.Empty<object>()));

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                CreatureManagerModTemplateLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                CreatureManagerModTemplateLogger.LogError($"There was an issue loading your {ConfigFileName}");
                CreatureManagerModTemplateLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked          = null!;
        private static ConfigEntry<float> bossHealthMultiplierEikthyr   = null!;
        private static ConfigEntry<float> bossHealthMultiplierElder     = null!;
        private static ConfigEntry<float> bossHealthMultiplierBonemass  = null!;
        private static ConfigEntry<float> bossHealthMultiplierModer     = null!;
        private static ConfigEntry<float> bossHealthMultiplierYagluth   = null!;
        private static ConfigEntry<float> bossHealthMultiplierSeekerQueen = null!;
        private static ConfigEntry<float> mobsHealthMultiplier          = null!;   

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        #endregion
    }
}