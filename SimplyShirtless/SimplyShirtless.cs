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
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names provided by Harmony")]
    public class SimplyShirtless
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;
        private readonly ModConfig _config;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle ShoulderArea = new(136, 416, 8, 32);
        private enum TorsoSprite
        {
            Flat,
            Toned,
            //Sculpted,
        }

        private readonly Dictionary<string, string> _patchedAssets = new()
        {
            { "shirts", "Characters/Farmer/shirts" },
            { "hairy", "Characters/Farmer/farmer_base" },
            { "bald", "Characters/Farmer/farmer_base_bald" }
        };

        public SimplyShirtless(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;

            helper.Events.Content.AssetRequested += RemoveShirt;
            helper.Events.Content.AssetRequested += ReplaceTorso;
            helper.Events.GameLoop.SaveLoaded += InvalidateAssets;
        }

        /// <summary>
        /// Prefix method that overrides the behavior of checking if a shirt has sleeves in the Farmer class.
        /// Returns <c>false</c> if no shirt is equipped, indicating the shirt has no sleeves.
        /// If a shirt is equipped, it replicates the original method's behavior (i.e., <c>__result</c> is not <c>false</c>).
        /// This is necessary since the fallback shirt is hardcoded to always have sleeves.
        /// See <a href="https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony">Stardew Valley Wiki: Harmony</a>.
        /// </summary>
        /// <param name="__instance">The Farmer instance provided by Harmony.</param>
        /// <param name="__result">The resulting boolean indicating whether the shirt has sleeves or not.</param>
        /// <returns>Returns whether to skip the original method (false) or continue executing it (true).</returns>
        public static bool ShirtHasSleeves_Prefix(ref bool __result, Farmer __instance)
        {
            if (!IsModEnabled()) return true;
            string id = __instance.IsOverridingShirt(out var overrideId)
                ? overrideId
                : __instance.shirtItem?.Value?.ItemId;

            __result = id != null && Game1.shirtData.TryGetValue(id, out var shirtData) && shirtData.HasSleeves;
            return false;
        }

        private void RemoveShirt(object sender, AssetRequestedEventArgs e)
        {
            if (!IsModEnabled()) return;
            if (!IsAssetTarget(e, _patchedAssets["shirts"])) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                editor.PatchImage(NewBlankTexture(), targetArea: ShirtArea);
                editor.PatchImage(NewBlankTexture(), targetArea: ShoulderArea);
            });
        }
        
        private void ReplaceTorso(object sender, AssetRequestedEventArgs e)
        {
            if (!IsModEnabled()) return;
            var isTargetHairy = IsAssetTarget(e, _patchedAssets["hairy"]);
            var isTargetBald = IsAssetTarget(e, _patchedAssets["bald"]);

            if (isTargetHairy)
                e.LoadFromModFile<Texture2D>(GetModdedTorso(), AssetLoadPriority.Medium);
            if (isTargetBald)
                e.LoadFromModFile<Texture2D>(GetModdedTorso(isBald: true), AssetLoadPriority.Medium);
        }
        
        /// <summary>
        /// Retrieves the path for the modded torso image based on the chosen single player sprite option and
        /// multiplayer option.
        /// </summary>
        /// <param name="isBald">Specifies whether the required sprite should be bald.</param>
        /// <param name="forMultiplayer">Specifies whether the required sprite is for multiplayer.</param>
        /// <returns>
        /// Returns the file path for the torso image corresponding to the selected sprite option.
        /// Defaults to the Toned sprite if the chosen sprite option is unavailable.
        /// </returns>
        private string GetModdedTorso(bool isBald = false, bool forMultiplayer = false)
        {
            var bald = isBald ? "_bald" : "";
            var torsoOption =
                GetTorsoOption(forMultiplayer ? _config.MultiplayerSprite : _config.Sprite, bald);
            if (torsoOption != null) return torsoOption;
            
            _monitor.Log("Chosen Sprite option not available. Defaulting to Toned.", LogLevel.Warn);
            return GetTorsoOption(1, bald);
        }
        
        /// <summary>
        /// Retrieves the torso option based on the specified sprite index and bald setting.
        /// </summary>
        /// <param name="spriteIndex">The index representing the chosen torso sprite.</param>
        /// <param name="bald">Specifies whether the required sprite should be bald.</param>
        /// <returns>
        /// Returns the file path for the torso image corresponding to the selected sprite index and bald setting.
        /// Returns null if the chosen sprite index is unavailable.
        /// </returns>
        private static string GetTorsoOption(int spriteIndex, string bald)
        {
            return (TorsoSprite)spriteIndex switch
            {
                TorsoSprite.Flat => $"assets/male/flat{bald}.png",
                TorsoSprite.Toned => $"assets/male/toned{bald}.png",
                _ => null
            };
        }

        private static bool IsAssetTarget(AssetRequestedEventArgs e, string target)
        {
            return e.NameWithoutLocale.IsEquivalentTo(target);
        }

        private static bool IsModEnabled()
        {
            return _helper.ReadConfig<ModConfig>().ModToggle;
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

        /// <summary>
        /// Invalidates the cache for the target assets in the `_patchedAssets` list. Used to reload or update textures.
        /// </summary>
        public void InvalidateAssets(object sender = null, SaveLoadedEventArgs e = null)
        {
            foreach (var asset in _patchedAssets.Values)
            {
                _helper.GameContent.InvalidateCache(asset);
            }
        }
    }
}