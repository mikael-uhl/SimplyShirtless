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
        private static ModConfig _config;
        private static IMonitor _monitor;
        private readonly string _baldTarget;
        private readonly string _hairyTarget;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle ShoulderArea = new(136, 416, 8, 32);

        public SimplyShirtless(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _config = config;
            _monitor = monitor;
            _baldTarget = "Characters/Farmer/farmer_base_bald";
            _hairyTarget = "Characters/Farmer/farmer_base";
            
            helper.Events.Content.AssetRequested += this.RemoveShirt;
            helper.Events.Content.AssetRequested += this.ReplaceTorso;
        }
        
        /// <summary>
        /// Prefix method rewriting shirt's extra data in the Farmer class.
        /// Adds "Sleeveless" to the shirt's extra data if shirt is unequipped and player is not creating a farmer.
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
            if (!_config.ModToggle || __instance.shirtItem.Value != null || __instance.shirt.Value >= 0) return true;
            __result ??= new List<string>();
            __result.Add("Sleeveless");
            return false;
        }

        private void RemoveShirt(object sender, AssetRequestedEventArgs e)
        {
            if (!_config.ModToggle) return;
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
            if (!_config.ModToggle) return;
            if (IsAssetTarget(e, _hairyTarget))
            {
                e.LoadFromModFile<Texture2D>(GetModdedTorso(), AssetLoadPriority.Medium);
            } else if (IsAssetTarget(e, _baldTarget))
            {
                e.LoadFromModFile<Texture2D>(GetModdedTorso(isBald: true), AssetLoadPriority.Medium);
            }
        }
        
        /// <summary>
        /// Retrieves the path for the modded torso image based on the chosen sprite option.
        /// </summary>
        /// <param name="isBald">Specifies whether the required sprite should be bald.</param>
        /// <returns>
        /// Returns the file path for the torso image corresponding to the selected sprite option.
        /// Defaults to the Toned sprite if the chosen sprite option is unavailable.
        /// </returns>
        private string GetModdedTorso(bool isBald = false)
        {
            var bald = "";
            if (isBald) bald = "_bald";
            if (_config.Sprite == 0) return $"assets/male/flat{bald}.png";
            if (_config.Sprite == 1) return $"assets/male/toned{bald}.png";
            if (_config.Sprite == 2) return $"assets/male/sculpted{bald}.png";

            _monitor.Log("Chosen Sprite option not available. Defaulting to Toned.", LogLevel.Warn);
            return $"assets/male/toned{bald}.png";
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
