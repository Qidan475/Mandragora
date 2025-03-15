using System;
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
using UnityEngine;

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
                new RealClientPositionController(),
                new PickingThroughWallFixController(),
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

        public List<CheckableRoom> CheckableRoomPositions { get; set; } = new List<CheckableRoom>()
        {
            new CheckableRoom()
            {
                Room = RoomType.HczHid,
                BoundsStart = new Vector3(1.5f, 3.9f, -6.1f),
                BoundsEnd = new Vector3(7.0f, 8.5f, 2.6f),
                LinecastPoint = new Vector3(2.3f, 5.2f, 0.85f),
            },
            new CheckableRoom()
            {
                Room = RoomType.HczArmory,
                BoundsStart = new Vector3(-0.2f, -1.4f, -2f),
                BoundsEnd = new Vector3(4.0f, 3.9f, 2.4f),
                LinecastPoint = new Vector3(0.25f, 1.2f, 0),
            },
            new CheckableRoom()
            {
                Room = RoomType.Hcz096,
                BoundsStart = new Vector3(0.3f, -1.2f, 1.9f),
                BoundsEnd = new Vector3(-3.7f, 3.8f, -2f),
                LinecastPoint = new Vector3(-2.85f, 1.25f, 0),
            },
            new CheckableRoom()
            {
                Room = RoomType.LczArmory,
                BoundsStart = new Vector3(-1.0f, -0.85f, -4.5f),
                BoundsEnd = new Vector3(5.8f, 6f, 4.75f),
                LinecastPoint = new Vector3(0.6f, 0.65f, 0),
            },
            new CheckableRoom()
            {
                Room = RoomType.Lcz914,
                BoundsStart = new Vector3(4.0f, -0.75f, 3.7f),
                BoundsEnd = new Vector3(5.7f, 3.05f, 6.7f),
                LinecastPoint = new Vector3(4.3f, 0.3f, 4.2f),
            },
            new CheckableRoom()
            {
                Room = RoomType.Lcz914,
                BoundsStart = new Vector3(4.0f, -0.75f, -7.05f),
                BoundsEnd = new Vector3(5.7f, 3.05f, -3.85f),
                LinecastPoint = new Vector3(4.3f, 0.3f, -6.65f),
            },
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
            [PluginFeature.RealClientPosCmd] = false,

            [PluginFeature.HunterAtlasSurfaceFix] = false,
        };
    }
}
