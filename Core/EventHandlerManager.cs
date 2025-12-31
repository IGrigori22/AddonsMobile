using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.Framework.Data;
using AddonsMobile.Integration;
using AddonsMobile.UI;
using MonoMod.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Net.Http.Headers;

namespace AddonsMobile.Core
{
    internal sealed class EventHandlerManager
    {
        #region Fields
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly Func<bool> _isInitializedCheck;

        private MobileButtonManager _mobileButtonManager = null!;
        private MobileAddonsAPI _addonsAPI = null!;
        private ConsoleCommandHandler _consoleCommands = null!;
        private KeyRegistry _registry = null!;
        private ModConfig _config = null!;
        private IManifest _manifest = null!;

        private bool _isRegistered;
        #endregion

        #region Constructor
        public EventHandlerManager(IModHelper helper, IMonitor monitor, Func<bool> isInitializedCheck)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _isInitializedCheck = isInitializedCheck ?? throw new ArgumentNullException(nameof(isInitializedCheck));
        }
        #endregion

        #region Configuration
        public void SetComponents(MobileButtonManager buttonManager, MobileAddonsAPI addonsAPI, ConsoleCommandHandler consoleCommands, KeyRegistry registry, ModConfig config, IManifest manifest)
        {
            _mobileButtonManager = buttonManager ?? throw new ArgumentNullException(nameof(buttonManager));
            _addonsAPI = addonsAPI ?? throw new ArgumentNullException(nameof(addonsAPI));
            _consoleCommands = consoleCommands ?? throw new ArgumentNullException(nameof(consoleCommands));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));

            _monitor.Log("EventHandlerManager components configured", LogLevel.Trace);
        }
        #endregion

        #region Registration
        public void RegisterAll()
        {
            if (_isRegistered)
            {
                _monitor.Log("Event handlers already registered, skipping", LogLevel.Trace);
                return;
            }

            UnregisteredAll();

            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            _helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            _helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            _helper.Events.Display.WindowResized += OnWindowResized;

            _isRegistered = true;
            _monitor.Log("Event handlers registered", LogLevel.Trace);
        }
        private void UnregisteredAll()
        {
            _helper.Events.GameLoop.GameLaunched -= OnGameLaunched;
            _helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
            _helper.Events.GameLoop.ReturnedToTitle -= OnReturnedToTitle;
            _helper.Events.Display.WindowResized -= OnWindowResized;

            _isRegistered = false;
            _monitor.Log("Event handlers unregistered", LogLevel.Trace);
        }
        #endregion

        #region Event Handler
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (!_isInitializedCheck()) return;
            try
            {
                SetupConfigMenu();
                RegisterConsoleCommands();
                RegisterInternalButtons();

                _monitor.Log("Post-launch setup completed", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error during game launch: {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace ?? "No Stack trace", LogLevel.Trace);
            }
        }
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (!_isInitializedCheck()) return;

            try
            {
                if (!ValidateWorldReady()) return;
                if (!ValidateComponents()) return;

                _mobileButtonManager!.UpdatePosition();
                _mobileButtonManager.RefreshButtons();
                _mobileButtonManager.SetVisible(true);

                LogButtonStatus();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error during save load: {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);
            }
        }
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            if (!_isInitializedCheck()) return;

            try
            {
                _mobileButtonManager?.SetVisible(false);
                _monitor.Log("Returned to title - button manager hidden", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error during return to title: {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);
            }
        }
        #endregion

        #region Event Display Handler
        private void OnWindowResized(object? sender, WindowResizedEventArgs e)
        {
            if (!_isInitializedCheck()) return;

            try
            {
                _mobileButtonManager?.UpdatePosition();
                _monitor.Log($"Window resized to {e.NewSize.X}x{e.NewSize.Y}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error on window resize: {ex.Message}", LogLevel.Error);
            }
        }
        #endregion

        #region Setup Helpers
        private void RegisterInternalButtons()
        {
            if (_addonsAPI == null)
            {
                _monitor.Log("Cannot register internal buttons: API not initialized", LogLevel.Warn);
                return;
            }

            try
            {
                InternalButtonRegistry.RegisterAll(_addonsAPI, _registry, _config, _helper, _monitor);
                _monitor.Log("Internal buttons registered", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to register internal buttons: {ex.Message}", LogLevel.Error);
            }

            _addonsAPI.RefreshUI();
        }

        private void RegisterConsoleCommands()
        {
            if (_consoleCommands == null)
            {
                _monitor.Log("Console commands handler not available", LogLevel.Warn);
                return;
            }

            _consoleCommands.RegisterAll(_helper.ConsoleCommands);
            _monitor.Log("Console commands registered", LogLevel.Debug);
        }

        private void SetupConfigMenu()
        {
            if (_config == null || _manifest == null)
            {
                _monitor.Log("Cannot setup Generic mod config menu: config or manifest not set", LogLevel.Warn);
                return;
            }

            try
            {
                GenericModConfigMenu.Register(_helper, _monitor, _config, _manifest);
                _monitor.Log("Generic mod config menu integration registered", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to setup GMCM: {ex.Message}", LogLevel.Error);
            }
        }
        #endregion

        #region Validation Helpers
        private bool ValidateComponents()
        {
            if (_mobileButtonManager == null || _registry == null)
            {
                _monitor.Log("Core components not initialized", LogLevel.Error);
                return false;
            }
            return true;
        }

        private bool ValidateWorldReady()
        {
            if (!Context.IsWorldReady)
            {
                _monitor.Log("World not ready yet, skipping button setup", LogLevel.Warn);
                return false;
            }
            return true;
        }
        #endregion

        #region Logging
        private void LogButtonStatus()
        {
            if (_registry == null) return;

            int buttonCount = _registry.Count;
            _monitor.Log($"Save loaded with {buttonCount} registered button(s)", LogLevel.Info);

            if (buttonCount == 0)
            {
                _monitor.Log("No Buttons registered. Check if addon mods are loaded", LogLevel.Warn);
            }
        } 
        #endregion






    }
}
