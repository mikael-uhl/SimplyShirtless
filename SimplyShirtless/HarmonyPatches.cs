using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimplyShirtless.frameworks;
using StardewModdingAPI;
using StardewValley;

namespace SimplyShirtless;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Names provided by Harmony")]

public class HarmonyPatches
{
    private static IModHelper _helper;
    private static IMonitor _monitor;
    private readonly ModConfig _config;
    private SimplyShirtless _simplyShirtless;
    private static readonly Texture2D blankShirt = NewBlankTexture(256, 8);
    
    public HarmonyPatches(IModHelper helper, IMonitor monitor, ModConfig config, SimplyShirtless simplyShirtless)
    {
        _helper = helper;
        _monitor = monitor;
        _config = config;
        _simplyShirtless = simplyShirtless;
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
                if (ShouldDisableShirt())
                {
                    __result = false;
                    return;
                }
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
}