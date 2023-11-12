using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace NoShirt
{
    public class NoShirt
    {
        private readonly IModHelper _helper;
        private static readonly Rectangle ShirtArea = new(8, 416, 8, 32);
        private static readonly Rectangle SleevesArea = new(136, 416, 8, 29);
        
        private readonly Texture2D _skinColorsTexture;
        private readonly Texture2D _sleevesTexture;
        private readonly Texture2D _originalColorsTexture;

        private List<Farmer> _farmersInLoadMenu = new List<Farmer>();
        
        private int _skinColor;
        private readonly IMonitor _monitor;

        public NoShirt(IModHelper helper, IMonitor monitor)
        {
            _monitor = monitor;
            _helper = helper;
            _skinColorsTexture = _helper.ModContent.Load<Texture2D>("assets/skinColors.png");
            _sleevesTexture = _helper.ModContent.Load<Texture2D>("assets/sleeves.png");
            _originalColorsTexture = _helper.ModContent.Load<Texture2D>("assets/originalColors.png");
            _helper.Events.Content.AssetRequested += this.RemoveShirt;
            _helper.Events.Content.AssetRequested += this.RemoveSleeve;
            _helper.Events.GameLoop.UpdateTicked += this.CheckSkinChange;
            _helper.Events.GameLoop.SaveLoaded += this.OnSaveLoad;
            _helper.Events.Display.MenuChanged += this.IsLoadMenuActive;
        }
        
        private void IsLoadMenuActive(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is LoadGameMenu)) return;
            // Armazena a lista de fazendeiros renderizados no menu de carregar jogo
            _farmersInLoadMenu = Game1.getAllFarmers().ToList();
            foreach (Farmer farmer in _farmersInLoadMenu)
            {
                int skinColor = GetValidSkinColor(farmer.skin.Value);
                
                ReplaceSleeveColors(_sleevesTexture, _originalColorsTexture, _skinColorsTexture, skinColor);

                _monitor.Log("Height: " + farmer.Sprite.SpriteHeight + "\n" + "Width: " + farmer.Sprite.SpriteWidth, LogLevel.Alert);
            }
        }
        private bool IsColorMatch(Color colorA, Color colorB, int tolerance)
        {
            int r = colorA.R - colorB.R;
            int g = colorA.G - colorB.G;
            int b = colorA.B - colorB.B;
            int a = colorA.A - colorB.A;

            return (r * r + g * g + b * b + a * a) <= tolerance * tolerance;
        }
        
        private bool IsAssetShirts(AssetRequestedEventArgs assetRequestEvent)
        {
            return assetRequestEvent.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts");
        }

        private void RemoveShirt(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();

                Texture2D blankRectangle = new Texture2D(Game1.graphics.GraphicsDevice, 8, 32);
                editor.PatchImage(blankRectangle, targetArea: ShirtArea);
            });
        }

        private void RemoveSleeve(object sender, AssetRequestedEventArgs e)
        {
            if (!IsAssetShirts(e)) return;
            e.Edit(asset =>
            {
                var editor = asset.AsImage();
                int currentSkinColor = GetValidSkinColor(Game1.player.skin.Value);
                ReplaceSleeveColors(_sleevesTexture, _originalColorsTexture, _skinColorsTexture, currentSkinColor);
                editor.PatchImage(_sleevesTexture, targetArea: SleevesArea);
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
        /// Replace the colors on the sleeves texture with the replacement colors
        /// </summary>
        /// <param name="sleeves">The sleeves texture to modify</param>
        /// <param name="originalColors">The texture containing the original colors</param>
        /// <param name="skinColors">The texture containing the replacement colors</param>
        /// <param name="skinValue">The index of the replacement colors in the skinColors texture</param>
        private void ReplaceSleeveColors(Texture2D sleeves, Texture2D originalColors, Texture2D skinColors, int skinValue)
        {
            Color[] pixels = GetDataFromTexture(sleeves);
            Color[] originalData = GetDataFromTexture(originalColors);
            Color[] replacementColors = GetReplacementColors(skinColors, skinValue);
            ReplaceColors(pixels, originalData, replacementColors);
            sleeves.SetData(pixels);
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
        
        /// <summary>
        /// Replace the colors in a pixels array with replacement colors based on original colors
        /// </summary>
        /// <param name="pixels">The pixels array to modify</param>
        /// <param name="originalData">The original colors to replace</param>
        /// <param name="replacementColors">The colors to replace the original colors with</param>
        private void ReplaceColors(Color[] pixels, Color[] originalData, Color[] replacementColors)
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                int index = Array.IndexOf(originalData, pixels[i]);
                if (index != -1)
                    pixels[i] = replacementColors[index % 3];
            }
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
