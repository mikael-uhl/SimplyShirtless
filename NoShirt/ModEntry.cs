using StardewModdingAPI;
using HarmonyLib;
using StardewValley;

namespace NoShirt
{ 
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        { 
            var noShirt = new NoShirt(helper, Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetShirtExtraData)),
                prefix: new HarmonyMethod(typeof(NoShirt), nameof(NoShirt.GetShirtData_Prefix))
            );
        }
    }
}