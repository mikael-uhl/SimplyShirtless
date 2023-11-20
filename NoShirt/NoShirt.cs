using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

namespace NoShirt
{
    public class NoShirt
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle ShoulderArea = new(136, 416, 8, 32);
        
        private readonly Texture2D _blankRectangle;
        
        private int _skinColor;

        public NoShirt(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _blankRectangle = _helper.ModContent.Load<Texture2D>("assets/blank.png");
            _helper.Events.Content.AssetRequested += this.RemoveShirt;
            _helper.Events.Content.AssetRequested += this.RemoveShoulder;
        }
        
        private bool IsAssetShirts(AssetRequestedEventArgs assetRequestEvent)
        {
            return assetRequestEvent.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts");
        }
        
        public static bool GetShirtData_Prefix(Farmer __instance, ref List<string> __result)
        {
            if (__result == null)
            {
                __result = new List<string>();
            }

            if (__instance.shirt.Value == -1)
            {
                __result.Clear();
                __result.Add("Sleeveless");
                return false;
            }
            return true;
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
    }
}
