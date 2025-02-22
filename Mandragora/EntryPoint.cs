using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using HarmonyLib;

namespace Mandragora
{
    public class EntryPoint : Plugin<PluginConfig>
    {
        public override string Name => "Mandragora";
        public override string Author => "dark7eamplar.69420";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(9, 5, 0);

        public static CachedLayerMask WallLayerMask { get; private set; }

        public static EntryPoint Instance { get; private set; }

        private Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;
            _harmony = new Harmony($"{Author}.{Name}");
            GlobalPatchProcessor.PatchAll(_harmony, out var failed);
            if (failed > 0)
                Log.Error("failed patching");

            WallLayerMask = new CachedLayerMask(Config.LinecastWallLayerMasks);
            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Instance = null;
            GlobalPatchProcessor.UnpatchAll(_harmony.Id);

            base.OnDisabled();
        }
    }

    public class PluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;

        [Description("Default base-game value is 0.15. You can tweak it by setting the value lower, so it will be stricter. Min value - 0")]
        public float ForecastOverride { get; set; } = 0.15f;
        public float MinVelocityRequiredToCheckForecast { get; set; } = 0.3f;
        [Description("The main thing that protects from this type of exploit. It should not be that expensive, but you can try to disable it, if it is, and set forecast_override to zero")]
        public bool LinecastCheck { get; set; } = true;
        public string[] LinecastWallLayerMasks { get; set; } = new string[]
        {
            "Default",
            "Door",
            "Glass"
        };
    }
}
