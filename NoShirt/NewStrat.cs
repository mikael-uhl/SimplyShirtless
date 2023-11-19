using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using StardewValley.Objects;

namespace NoShirt
{
    public class NewStrat
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle ShoulderArea = new(136, 416, 8, 32);
        
        private readonly Texture2D _blankRectangle;
        private readonly Texture2D _skinColorsTexture;
        private readonly Texture2D _chestTexture;
        private readonly Texture2D _originalColorsTexture;
        
        private int _skinColor;

        public NewStrat(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _skinColorsTexture = _helper.ModContent.Load<Texture2D>("assets/skinColors.png");
            _originalColorsTexture = _helper.ModContent.Load<Texture2D>("assets/originalColors.png");
            _chestTexture = _helper.ModContent.Load<Texture2D>("assets/chest.png");
            _blankRectangle = _helper.ModContent.Load<Texture2D>("assets/blank.png");
            _helper.Events.Content.AssetRequested += this.RemoveShirt;
            _helper.Events.Content.AssetRequested += this.RemoveShoulder;
            _helper.Events.Content.AssetRequested += this.ReplaceSleeves;
            _helper.Events.GameLoop.UpdateTicked += this.CheckSkinChange;
            _helper.Events.GameLoop.SaveLoaded += this.OnSaveLoad;
            _helper.Events.GameLoop.SaveLoaded += this.OnSaveLoad;
        }
        
        private bool IsAssetShirts(AssetRequestedEventArgs assetRequestEvent)
        {
            return assetRequestEvent.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts");
        }
        
        //protected static bool _loadedData;
        
        // internal static bool LoadDataPrefix(Clothing __instance, bool initialize_color)
        // {
        //     try
        //     {
        //         FieldInfo loadedDataField = typeof(Clothing).GetField("_loadedData");
        //         if (loadedDataField != null)
        //         {
        //             loadedDataField.SetValue(__instance, true);
        //             return true;
        //         }
        //         return true;
        //     }
        //     catch (Exception ex)
        //     {
        //         return true;
        //     }
        // }
        
        public static bool GetShirtData_Prefix(Farmer __instance, ref List<string> __result)
        {
            if (__result == null)
            {
                __result = new List<string>(); // Ensure __result is initialized
            }

            if (__instance.shirt.Value == -1)
            {
                __result.Clear();
                __result.Add("Sleeveless"); // Adding the "Sleeveless" property
                return false; // Skip original method
            }
            return true; // Continue with original method
        }

        private void ReplaceShirtOld(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                int currentSkinColor = GetValidSkinColor(Game1.player?.skin?.Value ?? 0);

                Texture2D shirtTextureCopy = new Texture2D(Game1.graphics.GraphicsDevice, _chestTexture.Width, _chestTexture.Height);
                Color[] shirtDataCopy = new Color[_chestTexture.Width * _chestTexture.Height];
                _chestTexture.GetData(shirtDataCopy);
                shirtTextureCopy.SetData(shirtDataCopy);
                ReplaceShirtColors(shirtTextureCopy, _originalColorsTexture, _skinColorsTexture, currentSkinColor);
                editor.PatchImage(shirtTextureCopy, targetArea: ShirtArea);
            });
        }
        
        private void ReplaceShirt(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                int currentSkinColor = GetValidSkinColor(Game1.player?.skin?.Value ?? 0);

                Texture2D shirtTexture = _helper.ModContent.Load<Texture2D>("assets/chest.png");
                Color[] modifiedPixels = GetModifiedShirtPixels(shirtTexture, _originalColorsTexture, _skinColorsTexture, currentSkinColor);
        
                Texture2D modifiedShirtTexture = new Texture2D(Game1.graphics.GraphicsDevice, shirtTexture.Width, shirtTexture.Height);
                modifiedShirtTexture.SetData(modifiedPixels);
        
                editor.PatchImage(modifiedShirtTexture, targetArea: ShirtArea);
            });
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
        
        private void ReplaceShoulder(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                int currentSkinColor = GetValidSkinColor(Game1.player?.skin?.Value ?? 0);

                Texture2D shoulderTexture = _helper.ModContent.Load<Texture2D>("assets/shoulder.png");
                Color[] modifiedPixels = GetModifiedShirtPixels(shoulderTexture, _originalColorsTexture, _skinColorsTexture, currentSkinColor);
        
                Texture2D modifiedShoulderTexture = new Texture2D(Game1.graphics.GraphicsDevice, shoulderTexture.Width, shoulderTexture.Height);
                modifiedShoulderTexture.SetData(modifiedPixels);
                
                editor.PatchImage(modifiedShoulderTexture, targetArea: ShoulderArea);
            });
        }

        private void ReplaceSleeves(object sender, AssetRequestedEventArgs e)
        {
            if(!e.NameWithoutLocale.IsEquivalentTo("Data/ClothingInformation")) return;
            e.Edit(asset =>
            {
                var editor = asset.AsDictionary<int, string>();
                editor.Data[-2] = "Shirt/Shirt/A wearable shirt./0/-1/50/255 255 255/false/Shirt/Sleeveless";
                editor.Data[1209] = 
                    "Sports Shirt/Sports Shirt/Knock it out of the park with this classic look./209/-1/50/255 0 0/false/Shirt/Sleeveless";
                _monitor.Log(editor.Data[1209], LogLevel.Error);
                _monitor.Log(editor.Data[-2], LogLevel.Error);
            });
        }
        
        private Color[] GetModifiedShirtPixels(Texture2D shirtTexture, Texture2D originalColors, Texture2D skinColors, int skinValue)
        {
            Color[] pixels = new Color[shirtTexture.Width * shirtTexture.Height];
            shirtTexture.GetData(pixels);
            Color[] originalData = GetDataFromTexture(originalColors);
            Color[] replacementColors = GetReplacementColors(skinColors, skinValue);
            ReplaceColors(pixels, originalData, replacementColors);
            return pixels;
        }
        
        /// <summary>
        /// Get the valid skin color for the player, adjusting the value if it's out of bounds
        /// </summary>
        /// <param name="skinColor">The current player's skin color</param>
        /// <returns>The valid skin color</returns>
        private int GetValidSkinColor(int skinColor)
        {
            const int minSkinValue = 0;
            int maxSkinValue = _skinColorsTexture.Height - 1;
            return skinColor < minSkinValue ? maxSkinValue : skinColor > maxSkinValue ? minSkinValue : skinColor;
        }
        
        /// <summary>
        /// Replace the colors on the shirt texture with the replacement colors
        /// </summary>
        /// <param name="shirt">The shirt texture to modify</param>
        /// <param name="originalColors">The texture containing the original colors</param>
        /// <param name="skinColors">The texture containing the replacement colors</param>
        /// <param name="skinValue">The index of the replacement colors in the skinColors texture</param>
        private void ReplaceShirtColors(Texture2D shirt, Texture2D originalColors, Texture2D skinColors, int skinValue)
        {
            Color[] pixels = GetDataFromTexture(shirt);
            Color[] originalData = GetDataFromTexture(originalColors);
            Color[] replacementColors = GetReplacementColors(skinColors, skinValue);
            ReplaceColors(pixels, originalData, replacementColors);
            shirt.SetData(pixels);
        }
        
        /// <summary>
        /// Get the color data from a texture
        /// </summary>
        /// <param name="texture">The texture to get the color data from</param>
        /// <returns>The color data of the texture</returns>
        private Color[] GetDataFromTexture(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            return data;
        }
        
        /// <summary>
        /// Get the replacement colors from a skin color texture
        /// </summary>
        /// <param name="skinColors">The skin color texture to get the replacement colors from</param>
        /// <param name="skinValue">The index of the replacement colors in the skinColors texture</param>
        /// <returns>The replacement colors</returns>
        private Color[] GetReplacementColors(Texture2D skinColors, int skinValue)
        {
            Color[] skinData = GetDataFromTexture(skinColors);
            Color[] replacementColors = new Color[3];
            Array.Copy(skinData, skinValue * 3, replacementColors, 0, 3);
            return replacementColors;
        }
        
        private void ReplaceColors(Color[] pixels, Color[] originalData, Color[] replacementColors)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                // Skip replacement for transparent pixels
                if (pixels[i].A == 0) continue;
                for (int j = 0; j < originalData.Length; j++)
                {
                    if (AreColorsEqual(pixels[i], originalData[j]))
                    {
                        pixels[i] = replacementColors[j % 3];
                        break;
                    }
                }
            }
        }
        
        private bool AreColorsEqual(Color color1, Color color2)
        {
            return color1.R == color2.R && color1.G == color2.G && color1.B == color2.B && color1.A == color2.A;
        }

        private void OnSaveLoad(object sender, SaveLoadedEventArgs e)
        {
            _skinColor = GetValidSkinColor(Game1.player.skin.Value);
            _helper.GameContent.InvalidateCache("Data/ClothingInformation");
            _monitor.Log(Game1.player.GetShirtIndex().ToString(), LogLevel.Error);
            _monitor.Log(string.Concat(Game1.player.GetShirtExtraData()), LogLevel.Error);
        }
        
        private void CheckSkinChange(object sender, UpdateTickedEventArgs e)
        {
            if (_skinColor == Game1.player.skin.Value) return;
            _skinColor = Game1.player.skin.Value;
            _helper.GameContent.InvalidateCache("Characters/Farmer/shirts");
            _helper.GameContent.InvalidateCache("Data/ClothingInformation");
            _monitor.Log(Game1.player.GetShirtIndex().ToString(), LogLevel.Error);
            _monitor.Log(string.Concat(Game1.player.GetShirtExtraData()), LogLevel.Error);
        }
    }
}
