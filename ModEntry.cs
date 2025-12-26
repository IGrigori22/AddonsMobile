using AddonsMobile.API;
using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.UI;
using AddonsMobile.UI.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace AddonsMobile
{
    public sealed class ModEntry : Mod
    {
        // ════════════════════════════════════════════════════════════════
        // STATIC PROPERTIES
        // ════════════════════════════════════════════════════════════════

        internal static ModConfig Config { get; private set; } = null!;
        internal static IModHelper SHelper { get; private set; } = null!;
        internal static IMonitor SMonitor { get; private set; } = null!;
        internal static IManifest SManifest { get; private set; } = null!;
        internal static KeyRegistry Registry { get; private set; } = null!;

        // ════════════════════════════════════════════════════════════════
        // INSTANCE FIELDS
        // ════════════════════════════════════════════════════════════════

        private MobileButtonManager _buttonManager = null!;
        private MobileAddonsAPI _addonsAPI = null!;
        private ConsoleCommandHandler _consoleCommands = null!;

        private bool _isInitialized;

        // ════════════════════════════════════════════════════════════════
        // ENTRY POINT
        // ════════════════════════════════════════════════════════════════

        public override void Entry(IModHelper helper)
        {
            try
            {
                // Step 1: Setup static references
                InitializeStaticReferences(helper);

                // Step 2: Load configuration
                LoadConfiguration();

                // Step 3: Initialize core systems
                InitializeCoreComponents();

                // Step 4: Register event handlers
                RegisterEventHandlers(helper);

                // Step 5: Devlopment tools (debug only)
                InitializeDebugTools(helper);

                _isInitialized = true;
                Monitor.Log("✓ AddonsMobile initialized successfully", LogLevel.Info);
                LogSystemInfo();
            }
            catch (Exception ex)
            {
                _isInitialized = false;

                Monitor.Log($"✗ Failed to initialize AddonsMobile: {ex.Message}", LogLevel.Error);
                Monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);
                throw; // Re-throw untuk memberitahu SMAPI bahwa mod gagal load
            }
        }

        /// <summary>
        /// Expose API untuk mod lain
        /// </summary>
        public override object? GetApi()
        {
            if (_addonsAPI == null)
            {
                Monitor.Log("⚠ API requested before initialization complete", LogLevel.Warn);
                return null;
            }

            Monitor.Log("API instance provided to external mod", LogLevel.Trace);
            return _addonsAPI;
        }

        // ═══════════════════════════════════════════════════════════
        // INITIALIZATION METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Inisialisasi static references yang digunakan di seluruh mod.
        /// </summary>
        private void InitializeStaticReferences(IModHelper helper)
        {
            SHelper = helper ?? throw new ArgumentNullException(nameof(helper));
            SMonitor = Monitor ?? throw new InvalidOperationException("Monitor not available");
            SManifest = ModManifest ?? throw new InvalidOperationException("ModManifest not available");

            Monitor.Log("Static references initialized", LogLevel.Trace);
        }

        /// <summary>
        /// Load dan validasi konfigurasi mod.
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                Config = Helper.ReadConfig<ModConfig>() ?? new ModConfig();
                Config.Validate();
                Monitor.Log("Configuration loaded and validated", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Config error, using defaults: {ex.Message}", LogLevel.Warn);
                Config = new ModConfig();
            }
        }

        /// <summary>
        /// Inisialisasi komponen-komponen inti mod.
        /// Urutan penting: Registry → ButtonManager → API
        /// </summary>
        private void InitializeCoreComponents()
        {
            // Registry harus dibuat pertama karena digunakan oleh komponen lain
            Registry = new KeyRegistry(Monitor);
            Monitor.Log("KeyRegistry initialized", LogLevel.Trace);

            // ButtonManager mengelola tampilan dan interaksi button
            _buttonManager = new MobileButtonManager(Helper, Monitor);
            Monitor.Log("MobileButtonManager initialized", LogLevel.Trace);

            // API menghubungkan mod lain dengan sistem button
            _addonsAPI = new MobileAddonsAPI(Registry, _buttonManager, Monitor);
            Monitor.Log("MobileAddonsAPI initialized", LogLevel.Trace);

            // Console command handler untuk debugging
            _consoleCommands = new ConsoleCommandHandler(Registry, _buttonManager, Monitor);
            Monitor.Log("ConsoleCommandHandler initialized", LogLevel.Trace);
        }

        /// <summary>
        /// Register semua event handler untuk lifecycle mod.
        /// </summary>
        private void RegisterEventHandlers(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched -= OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle -= OnReturnedToTitle;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            Monitor.Log("Event handlers registered", LogLevel.Trace);
        }

        /// <summary>
        /// Inisialisasi tool untuk development (hanya di debug mode).
        /// </summary>
        private void InitializeDebugTools(IModHelper helper)
        {
#if DEBUG
            try
            {
                TextureGenerator.ExportTexturesToFile(helper, Monitor);
                Monitor.Log("Debug tools initialized", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Debug tools failed: {ex.Message}", LogLevel.Warn);
            }
#endif
        }

        /// <summary>
        /// Log informasi sistem untuk debugging.
        /// </summary>
        private void LogSystemInfo()
        {
            Monitor.Log($"Platform: {Constants.TargetPlatform}", LogLevel.Debug);
            Monitor.Log($"SMAPI Version: {Constants.ApiVersion}", LogLevel.Debug);
            Monitor.Log($"Game Version: {Constants.MinimumGameVersion}", LogLevel.Debug);
        }

        // ════════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ════════════════════════════════════════════════════════════════

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                SetupConfigMenu();
                RegisterConsoleCommands();

                Monitor.Log("Post-Launch setup Completed", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Error during game launch: {ex.Message}", LogLevel.Error);
                Monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);
            }            
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                // Sebaiknya verify game state
                if (!Context.IsWorldReady)
                {
                    Monitor.Log("World not ready yet, skipping button setup", LogLevel.Warn);
                    return;
                }

                if (_buttonManager == null || Registry == null)
                {
                    Monitor.Log("✗ Core components not initialized", LogLevel.Error);
                    return;
                }

                // Update posisi berdasarkan config/saved state
                _buttonManager.UpdatePosition();

                // Refresh button list dari registry
                _buttonManager.RefreshButtons();

                // Tampilkan button manager
                _buttonManager.SetVisible(true);

                int buttonCount = Registry.Count;
                Monitor.Log($"✓ Save loaded with {buttonCount} registered button(s)", LogLevel.Info);

                if (buttonCount == 0)
                {
                    Monitor.Log("⚠ No buttons registered. Check if addon mods are loaded.", LogLevel.Warn);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"✗ Error during save load: {ex.Message}", LogLevel.Error);
                Monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);
            }
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                _buttonManager.SetVisible(false);
                Monitor.Log("Returned to title - button manager hidden", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"✗ Error during return to title: {ex.Message}", LogLevel.Error);
                Monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Integration Setup
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Setup integrasi dengan Generic Mod Config Menu.
        /// </summary>
        private void SetupConfigMenu()
        {
            try
            {
                GenericModConfigMenu.Register(Helper, Monitor, Config, ModManifest);
                Monitor.Log("Generic Mod Config Menu integration registered", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to setup GMCM: {ex.Message}", LogLevel.Warn);
            }
        }

        /// <summary>
        /// Register console commands untuk debugging.
        /// </summary>
        private void RegisterConsoleCommands()
        {
            if (_consoleCommands == null)
            {
                Monitor.Log("Console commands handler not available", LogLevel.Warn);
                return;
            }

            _consoleCommands.RegisterAll(Helper.ConsoleCommands);
            Monitor.Log("Console commands registered", LogLevel.Debug);
        }

        /// <summary>
        /// Reset posisi FAB ke default
        /// </summary>
        public void ResetButtonPosition()
        {
            if (_buttonManager == null)
            {
                Monitor.Log("Cannot reset: ButtonManager not initialized", LogLevel.Warn);
                return;
            }

            try
            {
                _buttonManager.ResetPosition();
                Monitor.Log("FAB position reset to default", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to reset button position: {ex.Message}", LogLevel.Error);
            }
        }        
    }
}