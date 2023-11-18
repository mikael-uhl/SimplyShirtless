using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoShirt
{
    public class NewStrat
    {
        private readonly IModHelper _helper;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        
        private readonly Texture2D _skinColorsTexture;
        private readonly Texture2D _chestTexture;
        private readonly Texture2D _originalColorsTexture;
        
        private int _skinColor;

        public NewStrat(IModHelper helper)
        {
            _helper = helper;
            _skinColorsTexture = _helper.ModContent.Load<Texture2D>("assets/skinColors.png");
            _originalColorsTexture = _helper.ModContent.Load<Texture2D>("assets/originalColors.png");
            _chestTexture = _helper.ModContent.Load<Texture2D>("assets/chest.png");
            _helper.Events.Content.AssetRequested += this.ReplaceShirt;
            _helper.Events.GameLoop.UpdateTicked += this.CheckSkinChange;
            _helper.Events.GameLoop.SaveLoaded += this.OnSaveLoad;
        }
        
        private bool IsAssetShirts(AssetRequestedEventArgs assetRequestEvent)
        {
            return assetRequestEvent.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts");
        }

        private void ReplaceShirt(object sender, AssetRequestedEventArgs e)
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
                if (pixels[i].A == 0)
                {
                    continue;
                }
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
            _helper.GameContent.InvalidateCache("Characters/Farmer/shirts");
        }
        
        private void CheckSkinChange(object sender, UpdateTickedEventArgs e)
        {
            if (_skinColor == Game1.player.skin.Value) return;
            _skinColor = Game1.player.skin.Value;
            _helper.GameContent.InvalidateCache("Characters/Farmer/shirts");
        }
    }
}
