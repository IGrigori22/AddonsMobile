using AddonsMobile.API;
using AddonsMobile.Config;
using AddonsMobile.Internal.Core;
using AddonsMobile.UI;
using StardewModdingAPI;

namespace AddonsMobile
{
    public sealed class ModEntry : Mod
    {

        #region Instance Fields
        private ConfigurationManager _configManager = null!;
        private CoreInitializer _coreInitializer = null!;
        private EventHandlerManager _eventManager = null!;

        private bool _isInitialized; 
        #endregion


        #region Components Accessors (Shortcuts)
        private MobileButtonManager ButtonManager => _coreInitializer.ButtonManager;
        private MobileAddonsAPI AddonsAPI => _coreInitializer.AddonsAPI;
        #endregion

        #region Entry Point
        public override void Entry(IModHelper helper)
        {
            try
            {
                // Step 1: Setup static references
                InitializeStaticReferences(helper);

                // Step 2: Load configuration
                InitializeConfiguration();

                // Step 3: Initialize core systems
                InitializeCoreComponents();

                // Step 4: Setup and Register event handler
                InitializeEventHandlers();

                // Step 5: Final validation
                FinalizeInitialization();

                // Step 6: Devlopment tools (debug only)
                InitializeDebugTools();

                _isInitialized = true;

                Monitor.Log("✓ AddonsMobile initialized successfully", LogLevel.Info);
                StaticReferenceHolder.LogSystemInfo();
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
            if (!_isInitialized)
            {
                Monitor.Log("API requested before initialization complete", LogLevel.Warn);
                return null;
            }

            if (AddonsAPI == null)
            {
                Monitor.Log("⚠ API is null", LogLevel.Warn);
                return null;
            }

            Monitor.Log("API instance provided to external mod", LogLevel.Trace);
            return AddonsAPI;
        }
        #endregion

        #region Initialization Steps
        /// <summary>
        /// Inisialisasi static references yang digunakan di seluruh mod.
        /// </summary>
        private void InitializeStaticReferences(IModHelper helper)
        {
            StaticReferenceHolder.InitializeCore(helper, Monitor, ModManifest);

            if (!StaticReferenceHolder.ValidateCore())
            {
                throw new InvalidOperationException("Failed to initialize static references");
            }

            Monitor.Log("Static references initialized", LogLevel.Trace);
        }

        private void InitializeConfiguration()
        {
            _configManager = new ConfigurationManager(Helper, Monitor);

            _configManager.OnConfigChanged += OnConfigurartionChanged;
            _configManager.OnConfigReset += OnConfigurationReset;

            var config = _configManager.LoadAndValidate();

            StaticReferenceHolder.SetConfig(config);

            // Validate dengan config
            if (!StaticReferenceHolder.ValidateWithConfig())
            {
                throw new InvalidOperationException("Failed to validate configuration");
            }


            Monitor.Log("Configuration initialized", LogLevel.Trace);
        }

        private void InitializeCoreComponents()
        {
            _coreInitializer = new CoreInitializer(Helper, Monitor);

            if (!_coreInitializer.InitializeAll())
            {
                throw new InvalidOperationException("Core components initialization failed");
            }

            if (!_coreInitializer.ValidateComponents())
            {
                throw new InvalidOperationException("Core components validation failed");
            }

            Monitor.Log("Core components initialized", LogLevel.Trace);
        }

        private void InitializeEventHandlers()
        {
            // Buat event manager dengan refrensi ke status inisialisasi
            _eventManager = new EventHandlerManager(
                Helper,
                Monitor,
                isInitializedCheck: () => _isInitialized
            );

            // Set komponen yang dibutuhkan untuk event handlers
            _eventManager.SetComponents(
                buttonManager: _coreInitializer.ButtonManager,
                addonsAPI: _coreInitializer.AddonsAPI,
                consoleCommands: _coreInitializer.ConsoleCommands,
                registry: _coreInitializer.Registry,
                config: _configManager.Config,
                manifest: ModManifest
            );

            _eventManager.RegisterAll();

            Monitor.Log("Event handler manager initialized", LogLevel.Trace);
        }

        /// <summary>
        /// Step 5: Final validation - pastikan semua sudah siap.
        /// </summary>
        private void FinalizeInitialization()
        {
            // Validate ALL references
            if (!StaticReferenceHolder.ValidateAll())
            {
                throw new InvalidOperationException("Final validation failed");
            }

            // Mark as fully initialized
            StaticReferenceHolder.MarkAsFullyInitialized();

            Monitor.Log("Initialization finalized", LogLevel.Trace);
        }

        /// <summary>
        /// Inisialisasi tool untuk development (hanya di debug mode).
        /// </summary>
        private void InitializeDebugTools()
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
        #endregion
        #region Event Callbacks
        private void OnConfigurartionChanged(ModConfig newConfig)
        {
            Monitor.Log("Configuration changed, updating components", LogLevel.Debug);

            StaticReferenceHolder.SetConfig(newConfig);

            ButtonManager?.UpdatePosition();
        }

        private void OnConfigurationReset()
        {
            Monitor.Log("Configuration reset to defaults", LogLevel.Info);
            ButtonManager?.ResetPosition();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Reset posisi FAB ke default
        /// </summary>
        public void ResetButtonPosition()
        {
            if (ButtonManager == null)
            {
                Monitor.Log("Cannot reset: ButtonManager not initialized", LogLevel.Warn);
                return;
            }

            try
            {
                ButtonManager.ResetPosition();
                _configManager.Reset(saveToFile: true);
                Monitor.Log("FAB position reset to default", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to reset button position: {ex.Message}", LogLevel.Error);
            }
        }

        public void UpdateButtonPosition(int x, int y)
        {
            _configManager?.UpdateFABPosition(x, y, autoSave: true);
        }

        public void ReloadConfiguration()
        {
            if (_configManager == null)
            {
                Monitor.Log("Cannot reload: ConfigManager not initialized", LogLevel.Warn);
                return;
            }

            try
            {
                _configManager.LoadAndValidate();
                StaticReferenceHolder.SetConfig(_configManager.Config);
                Monitor.Log("Cenfiguration reloaded", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to reload config: {ex.Message}", LogLevel.Error);
            }
        } 
        #endregion
    }
}