using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SimplyShirtless.frameworks
{
    public class CreateMenu
    {
        //TODO: translate this whole thing into Simply Shirtless,
        //maybe ignore i18n at first and just make the menu work in-game, single language;
        private readonly IModHelper _helper;
        private readonly IManifest _modManifest;
        private readonly IMonitor _monitor;
        private  ModConfig _config;
        
        public CreateMenu(IModHelper helper, IManifest modManifest, IMonitor monitor) {
            _monitor = monitor;
            _helper = helper;
            _config = helper.ReadConfig<ModConfig>();
            _modManifest = modManifest;
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenuApi =
                _helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenuApi is null)
            {
                _monitor.Log(I18n.DisabledGmcm(), LogLevel.Info);
                return;
            }

            configMenuApi.Register(
                mod: _modManifest,
                reset: () => _config = new ModConfig(),
                save: CommitConfig,
                titleScreenOnly: false
            );
            
            configMenuApi.AddBoolOption(
                mod: _modManifest,
                name: () => I18n.TitleEdnaldo(),
                tooltip: () => I18n.TooltipEdnaldo(),
                getValue: () => _config.EdnaldoPereiraToggle,
                setValue: value => _config.EdnaldoPereiraToggle = value
            );
            
            configMenuApi.AddBoolOption(
                mod: _modManifest,
                name: () => I18n.TitlePalmirinha(),
                tooltip: () => I18n.TooltipPalmirinha(),
                getValue: () => _config.PalmirinhaToggle,
                setValue: value => _config.PalmirinhaToggle = value
            );
            
            configMenuApi.AddBoolOption(
                mod: _modManifest,
                name: () => I18n.TitleGloboRural(),
                tooltip: () => I18n.TooltipGloboRural(),
                getValue: () => _config.GloboRuralToggle,
                setValue: value => _config.GloboRuralToggle = value
            );
            
            configMenuApi.AddBoolOption(
                mod: _modManifest,
                name: () => I18n.TitleMarciaSensitiva(),
                tooltip: () => I18n.TooltipMarciaSensitiva(),
                getValue: () => _config.MarciaSensitivaToggle,
                setValue: value => _config.MarciaSensitivaToggle = value
            );
        }
        
        private void CommitConfig()
        {
            _helper.WriteConfig(_config);
        }
    }
}