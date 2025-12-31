using AddonsMobile.Config;
using AddonsMobile.Framework.Data;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddonsMobile.Core
{
    internal static class StaticReferenceHolder
    {
        public static ModConfig Config { get; private set; } = null!;
        public static IModHelper Helper { get; private set; } = null!;
        public static IMonitor Monitor { get; private set; } = null!;
        public static IManifest Manifest { get; private set; } = null!;
        public static KeyRegistry Registry { get; private set; } = null!;
        public static bool IsFullyInitialized { get; private set; }
        public static bool IsCoreInitialized { get; private set; }

        public static void InitializeCore(IModHelper helper, IMonitor monitor, IManifest manifest)
        {
            Helper = helper ?? throw new ArgumentNullException(nameof(helper));
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));

            IsCoreInitialized = true;

            Monitor.Log("Core static reference initialized", LogLevel.Trace);
        }

        public static void SetConfig(ModConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Monitor?.Log("Config reference set", LogLevel.Trace);
        }

        public static void SetRegistry(KeyRegistry registry)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            Monitor?.Log("Registry reference set", LogLevel.Trace);
        }

        public static void MarkAsFullyInitialized()
        {
            IsFullyInitialized = true;
            Monitor?.Log("All static references marked as initialized", LogLevel.Trace);
        }

        public static bool ValidateCore()
        {
            var issues = new List<string>();

            if (Helper == null)
                issues.Add("Helper is null");

            if (Monitor == null)
                issues.Add("Monitor is null");

            if (Manifest == null)
                issues.Add("Manifest is null");

            if (issues.Count > 0)
            {
                foreach (var issue in issues)
                {
                    if (Monitor != null)
                        Monitor.Log($"⚠ Core reference issue: {issue}", LogLevel.Warn);
                    else
                        Console.WriteLine($"[AddonsMobile] Core reference issue: {issue}");
                }
                return false;
            }

            Monitor?.Log("All static references validated", LogLevel.Trace);
            return true;
        }

        /// <summary>
        /// Validasi SEMUA references termasuk Config.
        /// Dipanggil setelah config di-load.
        /// </summary>
        public static bool ValidateWithConfig()
        {
            if (!ValidateCore())
                return false;

            if (Config == null)
            {
                Monitor?.Log("⚠ Config is null", LogLevel.Warn);
                return false;
            }

            Monitor?.Log("✓ Static references with config validated", LogLevel.Trace);
            return true;
        }

        public static bool ValidateAll()
        {
            if (!ValidateWithConfig())
                return false;

            if (Registry == null)
            {
                Monitor?.Log("Registry is null (optional but recommended", LogLevel.Debug);
                return false;
            }

            Monitor?.Log("All static references (including optional) validated", LogLevel.Trace);
            return true;
        }

        public static void LogSystemInfo()
        {
            if (Monitor == null) return;

            Monitor.Log($"╔══════════════════════════════════════", LogLevel.Debug);
            Monitor.Log($"║ System Information", LogLevel.Debug);
            Monitor.Log($"╠══════════════════════════════════════", LogLevel.Debug);
            Monitor.Log($"║ Mod: {Manifest?.Name} v{Manifest?.Version}", LogLevel.Debug);
            Monitor.Log($"║ Platform: {Constants.TargetPlatform}", LogLevel.Debug);
            Monitor.Log($"║ Game: {Constants.MinimumGameVersion}", LogLevel.Debug);
            Monitor.Log($"║ SMAPI: {Constants.ApiVersion}", LogLevel.Debug);
            Monitor.Log($"║ OS: {Environment.OSVersion}", LogLevel.Debug);
            Monitor.Log($"╚══════════════════════════════════════", LogLevel.Debug);
        }

        public static void Reset()
        {
            Config = null!;
            Helper = null!;
            Monitor = null!;
            Manifest = null!;
            Registry = null!;
            IsFullyInitialized = false;
        }
    }
}
