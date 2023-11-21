using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimplyShirtless.frameworks;

namespace SimplyShirtless
{
    public class SimplyShirtless
    {
        private readonly IModHelper _helper;
        private ModConfig _config;
        private readonly bool _isModEnabled;
        private readonly IMonitor _monitor;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle ShoulderArea = new(136, 416, 8, 32);

        public SimplyShirtless(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _config = helper.ReadConfig<ModConfig>();
            _monitor = monitor;
            _helper.Events.Content.AssetRequested += this.RemoveShirt;
            helper.Events.Content.AssetRequested += this.ReplaceTorso;
            _isModEnabled = _config.ModToggle;
        }
        
        /// <summary>
        /// Prefix method rewriting shirt's extra data in the Farmer class.
        /// Adds "Sleeveless" to the shirt's extra data if shirt is absent and fallback activates.
        /// This is necessary since the fallback shirt never has the extra data found in Data/ClothingInformation.json
        /// (even after manually writing). Hence this forcing method.
        /// See stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony
        /// </summary>
        /// <param name="__instance">The Farmer instance provided by Harmony.</param>
        /// <param name="__result">The resulting list of extra data for the shirt provided by Harmony.</param>
        /// <returns>Returns whether to skip the original method (false) or continue executing it (true).</returns>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names provided by Harmony")]
        public static bool GetShirtExtraData_Prefix(Farmer __instance, ref List<string> __result)
        {
            if (__instance.shirtItem.Value != null) return true;
            __result ??= new List<string>();
            __result.Add("Sleeveless");
            return false;
        }

        private void RemoveShirt(object sender, AssetRequestedEventArgs e)
        {
            if (!_isModEnabled) return;
            if (!IsAssetTarget(e, "Characters/Farmer/shirts")) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                editor.PatchImage(NewBlankTexture(), targetArea: ShirtArea);
                editor.PatchImage(NewBlankTexture(), targetArea: ShoulderArea);
            });
        }
        
        private void ReplaceTorso(object sender, AssetRequestedEventArgs e)
        {
            if (!_isModEnabled) return;
            if (!IsAssetTarget(e, "Characters/Farmer/farmer_base")) return;
            e.LoadFromModFile<Texture2D>("assets/flat.png", AssetLoadPriority.Medium);
        }

        private static bool IsAssetTarget(AssetRequestedEventArgs e, string target)
        {
            return e.NameWithoutLocale.IsEquivalentTo(target);
        }

        /// <summary>
        /// Generates a blank Texture2D of specified width and height with transparent pixels.
        /// </summary>
        /// <param name="width">The width of the generated blank texture.</param>
        /// <param name="height">The height of the generated blank texture.</param>
        /// <returns>Returns a Texture2D instance representing a blank rectangle filled with transparent pixels.</returns>
        private static Texture2D NewBlankTexture(int width = 8, int height = 32)
        {
            var blankTexture = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            Array.Fill(data, Color.Transparent);
            blankTexture.SetData(data);
            return blankTexture;
        }

    }
}