using AddonsMobile.Config;
using StardewModdingAPI;


namespace AddonsMobile.Internal.Core
{
    /// <summary>
    /// Mengelola loading, validasi, dan penyimpanan konfigurasi mod.
    /// </summary>
    internal class ConfigurationManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        public ModConfig Config { get; private set; } = null!;
        public bool IsLoadedFromFile { get; private set; }
        public bool IsValidated { get; private set; }

        public event Action<ModConfig>? OnConfigChanged;
        public event Action? OnConfigReset;

        public ConfigurationManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        #region Loading
        public ModConfig Load()
        {
            try
            {
                Config = _helper.ReadConfig<ModConfig>();

                if (Config == null)
                {
                    _monitor.Log("Config file returned null, using defaults", LogLevel.Debug);
                    Config = new ModConfig();
                    IsLoadedFromFile = false;
                }
                else
                {
                    IsLoadedFromFile = true;
                    _monitor.Log("Configuration loaded from file", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to load config: {ex.Message}", LogLevel.Warn);
                _monitor.Log("Using default configuration", LogLevel.Info);

                Config = new ModConfig();
                IsLoadedFromFile = false;
            }
            return Config;
        }

        public ModConfig LoadAndValidate()
        {
            Load();
            Validate();
            return Config;
        }
        #endregion

        #region Validation
        private bool Validate()
        {
            if (Config == null)
            {
                _monitor.Log("Cannot validate: Config is null", LogLevel.Warn);
                return false;
            }

            try
            {
                Config.Validate();

                IsValidated = true;
                _monitor.Log("Configuration validated successfully", LogLevel.Debug);

                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Validation error: {ex.Message}", LogLevel.Error);
                IsValidated = false;
                return false;
            }
        } 
        #endregion


        #region Saving
        /// <summary>
        /// Simpan konfigurasi ke file.
        /// </summary>
        public void Save()
        {
            if (Config == null)
            {
                _monitor.Log("Cannot save: Config is null", LogLevel.Warn);
                return;
            }

            try
            {
                _helper.WriteConfig(Config);
                _monitor.Log("Configuration saved", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to save config: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Simpan konfigurasi dan notify listeners.
        /// </summary>
        public void SaveAndNotify()
        {
            Save();
            OnConfigChanged?.Invoke(Config);
        }
        #endregion

        #region Reset
        /// <summary>
        /// Reset konfigurasi ke nilai default.
        /// </summary>
        /// <param name="saveToFile">Jika true, simpan ke file setelah reset</param>
        public void Reset(bool saveToFile = true)
        {
            _monitor.Log("Resetting configuration to defaults", LogLevel.Info);

            Config = new ModConfig();
            IsLoadedFromFile = false;
            IsValidated = true;

            if (saveToFile)
            {
                Save();
            }

            OnConfigReset?.Invoke();
            OnConfigChanged?.Invoke(Config);
        }

        /// <summary>
        /// Reset hanya posisi FAB.
        /// </summary>
        public void ResetPosition(bool saveToFile = true)
        {
            if (Config == null) return;

            Config.ResetPosition();
            _monitor.Log("FAB position reset to default", LogLevel.Info);

            if (saveToFile)
            {
                SaveAndNotify();
            }
        }

        /// <summary>
        /// Reset hanya appearance settings.
        /// </summary>
        public void ResetAppearance(bool saveToFile = true)
        {
            if (Config == null) return;

            Config.ResetAppearance();
            _monitor.Log("Appearance settings reset to default", LogLevel.Info);

            if (saveToFile)
            {
                SaveAndNotify();
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Update nilai konfigurasi tertentu.
        /// </summary>
        /// <param name="updateAction">Action untuk mengupdate config</param>
        /// <param name="autoSave">Jika true, otomatis simpan setelah update</param>
        public void Update(Action<ModConfig> updateAction, bool autoSave = true)
        {
            if (Config == null)
            {
                _monitor.Log("Cannot update: Config is null", LogLevel.Warn);
                return;
            }

            try
            {
                updateAction(Config);
                Config.Validate(); // Re-validate after update

                _monitor.Log("Configuration updated", LogLevel.Trace);

                if (autoSave)
                {
                    SaveAndNotify();
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to update config: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Update posisi button.
        /// </summary>
        public void UpdateFABPosition(int x, int y, bool autoSave = true)
        {
            Update(config =>
            {
                config.FabPositionX = Math.Max(0, x);
                config.FabPositionY = Math.Max(0, y);
            }, autoSave);
        }

        /// <summary>
        /// Toggle visibility button.
        /// </summary>
        public void ToggleButtonVisibility(string buttonId, bool autoSave = true)
        {
            if (string.IsNullOrEmpty(buttonId)) return;

            Update(config =>
            {
                config.ToggleButtonVisibility(buttonId);
            }, autoSave);
        } 
        #endregion

    }
}
