using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SimplyShirtless.frameworks
{
    public class CreateMenu
    {
        private readonly IModHelper _helper;
        private readonly IManifest _modManifest;
        private readonly IMonitor _monitor;
        private readonly SimplyShirtless _simplyShirtless;
        private ModConfig _config;
        
        public CreateMenu(IModHelper helper, IManifest modManifest, IMonitor monitor, SimplyShirtless simplyShirtless, ModConfig config)
        {
            _monitor = monitor;
            _helper = helper;
            _config = config;
            _modManifest = modManifest;
            _simplyShirtless = simplyShirtless;
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        
        [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenuApi =
                _helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenuApi is null)
            {
                _monitor.Log(I18n.DisabledGmcm(), LogLevel.Info);
                return;
            }

            configMenuApi.Register
            (
                mod: _modManifest,
                reset: () => _config = new ModConfig(),
                save: CommitConfig,
                titleScreenOnly: false
            );
            
            configMenuApi.AddBoolOption
            (
                mod: _modManifest,
                name: () => I18n.TitleSimplyShirtless(),
                tooltip: () => I18n.TooltipSimplyShirtless(),
                getValue: () => _config.ModToggle,
                setValue: value => _config.ModToggle = value
            );
            
            configMenuApi.AddBoolOption
            (
                mod: _modManifest,
                name: () => I18n.TitleFemaleSprite(),
                tooltip: () => I18n.TooltipFemaleSprite(),
                getValue: () => _config.FemaleToggle,
                setValue: value => _config.FemaleToggle = value
            );

            configMenuApi.AddTextOption
            (
                mod: _modManifest,
                name: () => I18n.TitleSprite(),
                tooltip: () => I18n.TooltipSprite(),
                getValue: () => _config.TextureOption.ToString(),
                setValue: value => _config.TextureOption = int.Parse(value),
                allowedValues: new[] { "0", "1" },
                formatAllowedValue: value => FormatAllowedValues(value)
            );
            
            configMenuApi.AddTextOption
            (
                mod: _modManifest,
                name: () => I18n.TitleBikiniColor(),
                tooltip: () => I18n.TooltipBikiniColor(),
                getValue: () => _config.BikiniColor,
                setValue: value => _config.BikiniColor = value
            );
            
            configMenuApi.AddBoolOption
            (
                mod: _modManifest,
                name: () => I18n.TitleMultiplayer(),
                tooltip: () => I18n.TooltipMultiplayer(),
                getValue: () => _config.MultiplayerToggle,
                setValue: value => _config.MultiplayerToggle = value
            );
            
            configMenuApi.AddTextOption
            (
                mod: _modManifest,
                name: () => I18n.TitleMultiplayerSprite(),
                tooltip: () => I18n.TooltipMultiplayerSprite(),
                getValue: () => _config.MultiplayerTexture.ToString(),
                setValue: value => _config.MultiplayerTexture = int.Parse(value),
                allowedValues: new[] { "0", "1" },
                formatAllowedValue: value => FormatAllowedValues(value)
            );
        }

        private void CommitConfig()
        {
            _helper.WriteConfig(_config);
            _simplyShirtless.InvalidateAssets();
            _simplyShirtless.ValidateBikiniColor();
            SimplyShirtless.ConvertBikiniColor(_config.BikiniColor);
        }

        private static string FormatAllowedValues(string value)
        {
            return value switch {
                "0" => I18n.Flat(),
                "1" => I18n.Toned(),
                //"2" => I18n.Sculpted(),
                _ => value
            };
        }
    }
}