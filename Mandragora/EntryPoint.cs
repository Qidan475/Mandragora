﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using HarmonyLib;
using Mandragora.Controllers;
using Mandragora.Managers;

namespace Mandragora
{
    public class EntryPoint : Plugin<PluginConfig>
    {
        public override string Name => "Mandragora";
        public override string Author => "dark7eamplar.69420";
        public override Version Version { get; } = new Version(1, 1, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 5, 0);

        public CachedLayerMask WallLayerMask { get; private set; }
        public OverwatchFeaturesManager OWManager { get; private set; }

        public static EntryPoint Instance { get; private set; }

        private Harmony _harmony;
        private IDisposable[] _controllers;

        public override void OnEnabled()
        {
            Instance = this;
            _harmony = new Harmony($"{Author}.{Prefix}");
            GlobalPatchProcessor.PatchAll(_harmony, out var failed);
            if (failed > 0)
                Log.Error("failed patching");

            WallLayerMask = new CachedLayerMask(Config.BacktrackLinecastWallLayerMasks);
            OWManager = new OverwatchFeaturesManager();
            _controllers = new IDisposable[]
            {
                new HunterAtlasMapFixController(),
                new AdminWallhackController(),
                new AdminAntiflashController(),
            };

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _controllers.Do(x => x.Dispose());
            GlobalPatchProcessor.UnpatchAll(_harmony.Id);
            Instance = null;

            base.OnDisabled();
        }
    }

    public class PluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;

        [Description("Default base-game value is 0.15. You can tweak it by setting the value lower, so it will be stricter. Min value - 0")]
        public float BacktrackForecastOverride { get; set; } = 0.15f;
        public float BacktrackMinVelocityRequiredToCheckForecast { get; set; } = 0.3f;
        [Description("The main thing that protects from this type of exploit. It should not be that expensive, but you can try to disable it (in features_killswitch), if it is, and set forecast_override to zero")]
        public string[] BacktrackLinecastWallLayerMasks { get; set; } = new string[]
        {
            "Default",
            "Door",
            "Glass"
        };

        [Description("Permissions that are required for admin to enable a wallhack or antiflash in overwatch (that is visible only to that admin)")]
        public PlayerPermissions[] OverwatchCmdsRequiredPermissions { get; set; } =
        {
            PlayerPermissions.LongTermBanning,
            PlayerPermissions.Overwatch,
        };

        [Description("Effects that are being disabled when antiflash is active")]
        public List<EffectType> AntiflashEffects { get; set; } = new List<EffectType>()
        {
            EffectType.Blurred,
            EffectType.Flashed,
        };

        [Description("Quick way to disable certain features, in case the plugin starts behaving")]
        public Dictionary<PluginFeature, bool> FeaturesKillswitch { get; set; } = new Dictionary<PluginFeature, bool>()
        {
            [PluginFeature.BacktrackForecastPatch] = false,
            [PluginFeature.BacktrackForecastOverride] = false,
            [PluginFeature.BacktrackForecastLinecastCheck] = false,

            [PluginFeature.Scp1344OutOfRangeVisibilityFix] = false,

            [PluginFeature.AdminWallhackCmd] = false,
            [PluginFeature.AdminAntiflashCmd] = false,

            [PluginFeature.HunterAtlasSurfaceFix] = false,
        };
    }
}
