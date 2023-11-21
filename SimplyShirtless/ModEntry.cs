using HarmonyLib;
using SimplyShirtless.frameworks;
using StardewModdingAPI;
using StardewValley;

namespace SimplyShirtless
{ 
    internal sealed class ModEntry : Mod
    {
        private ModConfig Config;
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            _ = new SimplyShirtless(helper, Monitor, Config);
            _ = new CreateMenu(helper, ModManifest, Monitor, Config);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetShirtExtraData)),
                prefix: new HarmonyMethod(typeof(SimplyShirtless), nameof(SimplyShirtless.GetShirtExtraData_Prefix))
            );
        }
    }
}