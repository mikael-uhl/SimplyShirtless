using HarmonyLib;
using SimplyShirtless.frameworks;
using StardewModdingAPI;
using StardewValley;

namespace SimplyShirtless
{ 
    internal sealed class ModEntry : Mod
    {
        private ModConfig _config;
        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            _ = new SimplyShirtless(helper, Monitor, _config);
            _ = new CreateMenu(helper, ModManifest, Monitor, _config);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch
            (
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetShirtExtraData)),
                prefix: new HarmonyMethod(typeof(SimplyShirtless), nameof(SimplyShirtless.GetShirtExtraData_Prefix))
            );
        }
    }
}