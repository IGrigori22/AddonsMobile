using AddonsMobile.Framework;
using StardewModdingAPI;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Mendeteksi dan menyelesaikan konflik antar tombol
    /// </summary>
    public class ConflictResolver
    {
        private readonly KeyRegistry _registry;
        private readonly IMonitor _monitor;

        public ConflictResolver(KeyRegistry registry, IMonitor monitor)
        {
            _registry = registry;
            _monitor = monitor;
        }

        /// <summary>
        /// Data konflik yang terdeteksi
        /// </summary>
        public class ConflictInfo
        {
            public string OriginalKeybind { get; set; } = string.Empty;
            public List<ModKeyButton> ConflictingButtons { get; set; } = new();
        }

        /// <summary>
        /// Mendeteksi semua konflik keybind original
        /// </summary>
        public List<ConflictInfo> DetectKeybindConflicts()
        {
            var conflicts = new List<ConflictInfo>();
            var keybindGroups = _registry.GetAllButtons()
                .Where(b => !string.IsNullOrEmpty(b.OriginalKeybind))
                .GroupBy(b => b.OriginalKeybind!.ToUpperInvariant())
                .Where(g => g.Count() > 1);

            foreach (var group in keybindGroups)
            {
                conflicts.Add(new ConflictInfo
                {
                    OriginalKeybind = group.Key,
                    ConflictingButtons = group.ToList()
                });

                _monitor.Log(
                    $"Keybind conflict detected for '{group.Key}': " +
                    string.Join(", ", group.Select(b => b.DisplayName)),
                    LogLevel.Warn
                );
            }

            return conflicts;
        }

        /// <summary>
        /// Mendeteksi duplikasi nama display
        /// </summary>
        public List<(string Name, List<ModKeyButton> Buttons)> DetectNameConflicts()
        {
            return _registry.GetAllButtons()
                .GroupBy(b => b.DisplayName.ToUpperInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => (g.First().DisplayName, g.ToList()))
                .ToList();
        }

        /// <summary>
        /// Log semua konflik yang terdeteksi
        /// </summary>
        public void LogAllConflicts()
        {
            var keybindConflicts = DetectKeybindConflicts();
            var nameConflicts = DetectNameConflicts();

            if (keybindConflicts.Count == 0 && nameConflicts.Count == 0)
            {
                _monitor.Log("No conflicts detected.", LogLevel.Info);
                return;
            }

            _monitor.Log($"=== Conflict Report ===", LogLevel.Warn);

            if (keybindConflicts.Count > 0)
            {
                _monitor.Log($"Keybind Conflicts: {keybindConflicts.Count}", LogLevel.Warn);
                foreach (var conflict in keybindConflicts)
                {
                    _monitor.Log($"  [{conflict.OriginalKeybind}]:", LogLevel.Warn);
                    foreach (var button in conflict.ConflictingButtons)
                    {
                        _monitor.Log($"    - {button.DisplayName} ({button.ModId})", LogLevel.Warn);
                    }
                }
            }

            if (nameConflicts.Count > 0)
            {
                _monitor.Log($"Name Conflicts: {nameConflicts.Count}", LogLevel.Warn);
                foreach (var (name, buttons) in nameConflicts)
                {
                    _monitor.Log($"  \"{name}\" used by: {string.Join(", ", buttons.Select(b => b.ModId))}", LogLevel.Warn);
                }
            }
        }
    }
}