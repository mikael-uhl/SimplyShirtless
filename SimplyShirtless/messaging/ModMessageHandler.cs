using SimplyShirtless.frameworks;
using StardewModdingAPI;

namespace SimplyShirtless.messaging
{
    public class ModMessageHandler
    {
        private readonly IModHelper _helper;

        public ModMessageHandler(IModHelper helper)
        {
            _helper = helper;
            _helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
        }

        public void SendConfig<T>(T config)
        {
            _helper.Multiplayer.SendMessage(
                config,
                messageType: "ConfigSync",
                modIDs: new[] { _helper.ModRegistry.ModID }
            );
        }

        private void OnModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != _helper.ModRegistry.ModID || e.Type != "ConfigSync") return;
            var receivedConfig = e.ReadAs<ModConfig>();
            ProcessConfig(receivedConfig);
        }

        private void ProcessConfig(ModConfig config)
        {
            // Logic to handle received configuration
            // InvalidateAssets() here, to update online players assets
        }

        public void Dispose()
        {
            _helper.Events.Multiplayer.ModMessageReceived -= OnModMessageReceived;
        }
    }
}