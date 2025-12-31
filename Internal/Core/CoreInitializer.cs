using AddonsMobile.API;
using AddonsMobile.Framework;
using AddonsMobile.UI;
using StardewModdingAPI;

namespace AddonsMobile.Internal.Core
{
    /// <summary>
    /// Menangani inisialisasi komponen-komponen inti mod
    /// Lebih rapi karena terpisah dari ModEntry
    /// </summary>
    internal sealed class CoreInitializer
    {
        #region Fields
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor; 
        #endregion

        #region Initialized Components (Result)
        public KeyRegistry Registry { get; private set; } = null!;
        public MobileButtonManager ButtonManager { get; private set; } = null!;
        public MobileAddonsAPI AddonsAPI { get; private set; } = null!;
        public ConsoleCommandHandler ConsoleCommands { get; private set; } = null!;
        
        /// <summary>
        /// Menunjukan apakah semua komponen berhasil diinisialisasi
        /// </summary>
        public bool IsInitialized { get; private set; } 
        #endregion

        #region Constructor
        public CoreInitializer(IModHelper helper, IMonitor monitor)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        } 
        #endregion


        /// <summary>
        /// Inisialisasi komponen inti
        /// Registry --> ButtonManager --> AddonsAPI --> ConsoleCommands    
        /// </summary>
        /// <returns></returns>
        public bool InitializeAll()
        {
            try
            {
                InitializeRegistry();
                StaticReferenceHolder.SetRegistry(Registry);
                InitializeButtonManager();
                InitializeAddonsAPI();
                InitializeConsoleCommands();

                IsInitialized = true;
                _monitor.Log("All core components initialized", LogLevel.Debug);

                return true;
            }
            catch (Exception ex)
            {
                IsInitialized = false;
                _monitor.Log($"Core initialization failed: {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace ?? "No stack trace", LogLevel.Trace);

                return false;
            }
        }

        public bool ValidateComponents()
        {
            var issues = new List<string>();

            if (Registry == null)
            {
                issues.Add("Registry is null");
            }

            if (ButtonManager == null)
            {
                issues.Add("ButtonManager is null");
            }

            if (AddonsAPI == null)
            {
                issues.Add("AddonsAPI is null");
            }

            if (ConsoleCommands == null)
            {
                issues.Add("ConsoleCommands is null");
            }

            if (issues.Count > 0)
            {
                foreach (var issue in issues)
                {
                    _monitor.Log($"Validation issue: {issue}", LogLevel.Warn);
                }

                return false;
            }

            _monitor.Log("All components validated", LogLevel.Trace);
            return true;
        }

        private void InitializeRegistry()
        {
            Registry = new KeyRegistry(_monitor);
            _monitor.Log("KeyRegistry initialized", LogLevel.Trace);
        }

        private void InitializeButtonManager()
        {
            // Verifikasi Registry sudah tersedia di StaticReferenceHolder
            if (StaticReferenceHolder.Registry == null)
            {
                throw new InvalidOperationException(
                    "Registry must be set in StaticReferenceHolder before ButtonManager initialization");
            }

            ButtonManager = new MobileButtonManager(_helper, _monitor);
            _monitor.Log("MobileButtonManager initialized", LogLevel.Trace);
        }

        private void InitializeAddonsAPI()
        {
            if (Registry == null) throw new InvalidOperationException("Registry must be initialized before AddonsAPI");
            if (ButtonManager == null) throw new InvalidOperationException("ButtonManager must be initialized before AddonsAPI");

            AddonsAPI = new MobileAddonsAPI(Registry, ButtonManager, _monitor);
            _monitor.Log("MobileAddonsAPI initialized", LogLevel.Trace);
        }

        private void InitializeConsoleCommands()
        {
            if (Registry == null) throw new InvalidOperationException("Registry must be initialized before ConsoleCommands");

            if (ButtonManager == null) throw new InvalidOperationException("ButtonManager must be initialized before ConsoleCommands");

            ConsoleCommands = new ConsoleCommandHandler(Registry, ButtonManager, _monitor);
            _monitor.Log("ConsoleCommandHandler initialized", LogLevel.Trace);
        }
    }
}
