using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;
using SimplyShirtless.frameworks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SimplyShirtless.extensibility;

public class ContentPatcher
{
    private static IModHelper _helper;
    private static IManifest _modManifest;
    private static IMonitor _monitor;
    private CurrentShirtToken _shirtToken = new CurrentShirtToken();
    
    public ContentPatcher(IModHelper helper, IManifest modManifest, IMonitor monitor, SimplyShirtless simplyShirtless)
    {
        _helper = helper;
        _monitor = monitor;
        _modManifest = modManifest;
        _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        //_helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
    }

    // private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    // {
    //     _shirtToken.UpdateContext();
    // }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var contentPatcherApi = _helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
        
        if (contentPatcherApi is null)
        {
            _monitor.Log(I18n.DisabledCp(), LogLevel.Info);
            return;
        }
        
        contentPatcherApi.RegisterToken(_modManifest, "CurrentShirt", _shirtToken);
    }
    
    internal class CurrentShirtToken
    {
        public string _currentShirt = "";
        //public bool Disable;

        public bool AllowsInput()
        {
            return true;
        }

        public bool UpdateContext()
        {
            var oldShirt = this._currentShirt;
            this._currentShirt = GetPlayerShirtId(this);
            if (this._currentShirt == oldShirt) return false;
            //InvalidateAssets();
            return true;
        }

        public bool IsReady()
        {
            return Context.IsWorldReady && this._currentShirt != "";
        }

        public IEnumerable<string> GetValues(string input)
        {
            string shirt = input ?? this._currentShirt;
            
            if (string.IsNullOrWhiteSpace(shirt))
                yield break;
            
            this._currentShirt = shirt;
            yield return this._currentShirt;
        }
    }

    private static string GetPlayerShirtId(CurrentShirtToken currentShirtToken)
    {
        var result = Game1.player?.shirtItem.Value?.ItemId 
                     ?? SaveGame.loaded?.player?.shirtItem.Value?.ItemId 
                     ?? "null";
        _monitor.Log("ShirtID: " + result, LogLevel.Error);
        _monitor.Log("Current Shirt: " + currentShirtToken._currentShirt, LogLevel.Error);
        return result;
    }

    // private static void InvalidateAssets()
    // {
    //     _monitor.Log("Assets invalidated", LogLevel.Error);
    //     _helper.GameContent.InvalidateCache("Strings/StringsFromCSFiles");
    // }
}