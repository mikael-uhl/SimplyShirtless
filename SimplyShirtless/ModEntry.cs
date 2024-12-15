using System.Collections.Generic;
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
            var simplyShirtless = new SimplyShirtless(helper, Monitor, _config);
            _ = new CreateMenu(helper, ModManifest, Monitor, simplyShirtless, _config);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch
            (
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.ShirtHasSleeves)),
                prefix: new HarmonyMethod(typeof(SimplyShirtless), nameof(SimplyShirtless.ShirtHasSleeves_Prefix))
            );
        }
    }
}