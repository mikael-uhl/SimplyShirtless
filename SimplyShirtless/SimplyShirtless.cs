using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SimplyShirtless.extensibility;
using SimplyShirtless.frameworks;

namespace SimplyShirtless
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names provided by Harmony")]
    public class SimplyShirtless
    {
        private static IModHelper _helper;
        private static IMonitor _monitor;
        private readonly ModConfig _config;
        private static readonly Texture2D blankShirt = NewBlankTexture(256, 8);
        private static Color bikiniColor;
        //private static CurrentShirtToken _shirtToken;
        private enum TorsoSprite
        {
            Flat,
            Toned,
            Debug,
        }

        private readonly Dictionary<string, string> _patchedAssets = new()
        {
            { "hairy", "Characters/Farmer/farmer_base" },
            { "bald", "Characters/Farmer/farmer_base_bald" },
            { "hairy_girl", "Characters/Farmer/farmer_girl_base" },
            { "bald_girl", "Characters/Farmer/farmer_girl_base_bald" },
        };
        
        public SimplyShirtless(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;
            //_shirtToken = shirtToken;
            
            helper.Events.Content.AssetRequested += ReplaceTorso;
            helper.Events.GameLoop.SaveLoaded += InvalidateAssets;
        }
        
        public static void GetShirtColor_Postfix(ref Color __result, Farmer __instance)
        {
            try
            {
                if (!IsModEnabled() || __instance.IsMale) return;

                if (__instance.shirtItem.Value == null)
                    __result = bikiniColor;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetDisplayShirt_Postfix)} while coloring the shirt: " +
                             $"Please report at nexusmods.com/stardewvalley/mods/19282?tab=posts:\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Prefix method that overrides the behavior of checking if a shirt has sleeves in the Farmer class.
        /// Returns (__result is) <c>false</c> if no shirt is equipped, indicating the shirt has no sleeves.
        /// If a shirt is equipped, it replicates the original method's behavior (i.e., <c>__result</c> is not <c>false</c>).
        /// This is necessary since the fallback shirt is hardcoded to always have sleeves.
        /// See <a href="https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Harmony">Stardew Valley Wiki: Harmony</a>.
        /// </summary>
        /// <param name="__instance">The Farmer instance provided by Harmony.</param>
        /// <param name="__result">The resulting boolean indicating whether the shirt has sleeves or not.</param>
        /// <returns>Returns whether to skip the original method (false) or continue executing it (true).</returns>
        public static void ShirtHasSleeves_Postfix(ref bool __result, Farmer __instance)
        {
            try
            {
                if (!IsModEnabled()) return;
                // if (ShouldDisableShirt())
                // {
                //     __result = false;
                //     return;
                // }
                if (ShouldForceSleeves(__instance))
                {
                    __result = true;
                    return;
                }

                var id = __instance.IsOverridingShirt(out var overrideId)
                    ? overrideId
                    : __instance.shirtItem?.Value?.ItemId;

                __result = id != null && Game1.shirtData.TryGetValue(id, out var shirtData) && shirtData.HasSleeves;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShirtHasSleeves_Postfix)} while removing the sleeves. " +
                             $"Please report at nexusmods.com/stardewvalley/mods/19282?tab=posts:\n{ex}", LogLevel.Error);
            }
        }
        
        //TODO: add support for Content Packs that replaces the body texture, allowing for easy custom textures
        //maybe add a "Compatibility Mode" that enables an extra texture free of asset replacements
        //alternatively, simply disable the replacements when this mode is active without an extra texture option

        /// <summary>
        /// Postfix method that either overrides the fallback shirt texture with a blank asset (male),
        /// or simply returns a different shirt id (female) when no shirt is equipped and the farmer is
        /// either the local player or an online player with multiplayer settings enabled.
        /// </summary>
        /// <param name="__instance">The Farmer instance provided by Harmony.</param>
        /// <param name="texture">The texture of the shirt to be displayed.</param>
        /// <param name="spriteIndex">The sprite index of the shirt to be displayed.</param>
        public static void GetDisplayShirt_Postfix(Farmer __instance, ref Texture2D texture, ref int spriteIndex)
        {
            try
            {
                if (!IsModEnabled() || __instance.IsOverridingShirt(out _) || __instance.shirtItem.Value != null) 
                    return;

                if (Game1.hasLoadedGame && !__instance.IsLocalPlayer &&
                    (!Game1.IsMultiplayer || !IsMultiplayerEnabled() || __instance.IsLocalPlayer)) return;
            
                texture = __instance.IsMale ? blankShirt : FarmerRenderer.shirtsTexture;
                spriteIndex = __instance.IsMale ? 0 : 299;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetDisplayShirt_Postfix)} while replacing the shirt texture: " +
                             $"Please report at nexusmods.com/stardewvalley/mods/19282?tab=posts:\n{ex}", LogLevel.Error);
            }
        }
        
        private void ReplaceTorso(object sender, AssetRequestedEventArgs e)
        {
            if (!IsModEnabled()) return;
            //_monitor.Log($"TextureOption: {_config.TextureOption}", LogLevel.Error);
            if (_config.TextureOption == 2) return;
            //TODO: fix mod always overriding all the asset options regardless of current farmer
            foreach (var (id, asset) in _patchedAssets)
            {
                if (IsAssetTarget(e, asset))
                {
                    e.LoadFromModFile<Texture2D>(GetModdedTorso(id), AssetLoadPriority.High + 5);
                }
            }
        }
        
        /// <summary>
        /// Retrieves the path for the modded torso image based on the chosen single player sprite option and
        /// multiplayer option.
        /// </summary>
        /// <param name="key">The index of the patched asset to be modded in _patchedAssets.</param>
        /// <returns>
        /// Returns the file path for the torso image corresponding to the selected sprite option.
        /// Defaults to the Toned sprite if the chosen sprite option is unavailable.
        /// </returns>
        private string GetModdedTorso(string key)
        {
            var bald = key.Contains("bald") ? "_bald" : "";
            var sex = key.Contains("girl") ? "female" : "male";
            
            var torsoOption = GetTorsoOption(_config.TextureOption, bald, sex);
            if (torsoOption != null) return torsoOption;
            
            _monitor.Log("Chosen Sprite option not available. Defaulting to Toned.", LogLevel.Warn);
            return GetTorsoOption(1, bald, sex);
        }
        
        /// <summary>
        /// Retrieves the torso option based on the specified sprite index, bald setting and farmer sex.
        /// </summary>
        /// <param name="spriteIndex">The index representing the chosen torso sprite.</param>
        /// <param name="bald">Specifies whether the required sprite should be bald.</param>
        /// <param name="sex">Specifies a male or female sprite.</param>
        /// <returns>
        /// Returns the file path for the torso image corresponding to the selected sprite index and bald setting.
        /// Returns null if the chosen sprite index is unavailable.
        /// </returns>
        private static string GetTorsoOption(int spriteIndex, string bald, string sex)
        {
            return (TorsoSprite)spriteIndex switch
            {
                TorsoSprite.Flat => $"assets/{sex}/flat{bald}.png",
                TorsoSprite.Toned => $"assets/{sex}/toned{bald}.png",
                _ => null
            };
        }
        
        /// <summary>
        /// Ensures that when multiplayer is disabled and an online player has no shirt, they are forced to have a shirt
        /// with sleeves. This allows the local player to remain without sleeves, while online players are still forced
        /// to have sleeves when shirtless.
        /// </summary>
        /// <param name="farmer">The Farmer instance to check the conditions for.</param>
        /// <returns><c>true</c> if the farmer should be forced to have sleeves; otherwise, <c>false</c>.</returns>
        private static bool ShouldForceSleeves(Farmer farmer)
        {
            return !farmer.IsLocalPlayer 
                   && !IsMultiplayerEnabled() 
                   && farmer.shirtItem?.Value == null 
                   && Game1.hasLoadedGame;
        }

        private static bool IsAssetTarget(AssetRequestedEventArgs e, string target)
        {
            return e.NameWithoutLocale.IsEquivalentTo(target);
        }

        private static bool IsModEnabled()
        {
            return _helper.ReadConfig<ModConfig>().ModToggle;
        }
        
        private static bool IsMultiplayerEnabled()
        {
            return _helper.ReadConfig<ModConfig>().MultiplayerToggle;
        }
        
        private static string GetBikiniColor()
        {
            return _helper.ReadConfig<ModConfig>().BikiniColor;
        }

        // private static bool ShouldDisableShirt()
        // {
        //     return _shirtToken.Disable;
        // }

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
        
        public static void ConvertBikiniColor(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length != 6)
            {
                bikiniColor = Color.White;
                _monitor.Log(I18n.ColorError(), LogLevel.Warn);
            }
            else bikiniColor = new Color(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16),
                255
            );
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