using StardewModdingAPI;

namespace NoShirt
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        { 
            var noShirt = new NoShirt(helper, Monitor);
        }
    }
}