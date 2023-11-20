using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimplyShirtless
{
    public class SimplyShirtless
    {
        private readonly IMonitor _monitor;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle ShoulderArea = new(136, 416, 8, 32);
        
        private readonly IRawTextureData _blankRectangle;
        
        public SimplyShirtless(IModHelper helper, IMonitor monitor)
        {
            _monitor = monitor;
            _blankRectangle = helper.ModContent.Load<IRawTextureData>("assets/blank.png");
            helper.Events.Content.AssetRequested += this.RemoveShirt;
            helper.Events.Content.AssetRequested += this.RemoveShoulder;
        }
        
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static bool GetShirtData_Prefix(Farmer __instance, ref List<string> __result)
        {
            if (__instance.shirtItem.Value != null) return true;
            __result ??= new List<string>();
            
            if (!__result.Contains("Sleeveless")) __result.Add("Sleeveless");
            
            __result.Clear();
            __result.Add("Sleeveless");
            return false;
        }
        private void RemoveShirt(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                editor.PatchImage(_blankRectangle, targetArea: ShirtArea);
            });
        }
        
        private void RemoveShoulder(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                editor.PatchImage(_blankRectangle, targetArea: ShoulderArea);
            });
        }
        
        private static bool IsAssetShirts(AssetRequestedEventArgs assetRequestEvent)
        {
            return assetRequestEvent.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts");
        }
    }
}
