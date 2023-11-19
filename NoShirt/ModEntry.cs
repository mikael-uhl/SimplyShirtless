using NVorbis;
using StardewModdingAPI;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_5;
using HarmonyLib;
using StardewValley;
using StardewValley.Objects;

namespace NoShirt
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        { 
            //var noShirt = new NoShirt(helper, Monitor);
            var bareChest = new NewStrat(helper, Monitor);
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetShirtExtraData)),
                prefix: new HarmonyMethod(typeof(NewStrat), nameof(NewStrat.GetShirtData_Prefix))
            );
        }
    }
}