using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace SimplyShirtless
{ 
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        { 
            _ = new SimplyShirtless(helper, Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetShirtExtraData)),
                prefix: new HarmonyMethod(typeof(SimplyShirtless), nameof(SimplyShirtless.GetShirtData_Prefix))
            );
        }
    }
}