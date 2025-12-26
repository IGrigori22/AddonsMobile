using AddonsMobile.Framework.Conflicts;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Mendeteksi dan menyelesaikan konflik antar button.
    /// </summary>
    public class ConflictResolver
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly KeyRegistry _registry;
        private readonly IMonitor _monitor;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        public ConflictResolver(KeyRegistry registry, IMonitor monitor)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // DETECTION METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Scan semua button dan deteksi semua jenis konflik.
        /// </summary>
        public ConflictDetectionResult DetectAllConflicts()
        {
            var result = new ConflictDetectionResult
            {
                DetectedAt = DateTime.UtcNow
            };

            var allButtons = _registry.GetAllButtonsIncludingHidden().ToList();
            result.TotalButtonsScanned = allButtons.Count;

            _monitor.Log($"Scanning {allButtons.Count} buttons for conflicts...", LogLevel.Debug);

            // Deteksi berbagai jenis konflik
            result.Conflicts.AddRange(DetectKeybindConflicts(allButtons));
            result.Conflicts.AddRange(DetectNameConflicts(allButtons));
            result.Conflicts.AddRange(DetectIdConflicts(allButtons));
            result.Conflicts.AddRange(DetectPriorityConflicts(allButtons));

            _monitor.Log($"Conflict scan complete: {result.Conflicts.Count} conflict(s) found",
                result.HasCriticalConflicts ? LogLevel.Error :
                result.HasErrors ? LogLevel.Warn :
                LogLevel.Debug);

            return result;
        }

        /// <summary>
        /// Deteksi konflik keybind original.
        /// </summary>
        private List<ConflictInfo> DetectKeybindConflicts(List<ModKeyButton> buttons)
        {
            var conflicts = new List<ConflictInfo>();

            var keybindGroups = buttons
                .Where(b => !string.IsNullOrWhiteSpace(b.OriginalKeybind))
                .GroupBy(b => b.OriginalKeybind.ToUpperInvariant())
                .Where(g => g.Count() > 1);

            foreach (var group in keybindGroups)
            {
                var buttonList = group.ToList();
                var severity = DetermineSeverity(buttonList);

                var conflict = new ConflictInfo
                {
                    Type = ConflictType.DuplicateKeybind,
                    Severity = severity,
                    Description = $"Multiple buttons mapped to keybind '{group.Key}'",
                    ConflictingButtons = buttonList,
                    CanAutoResolve = severity <= ConflictSeverity.Warning
                };

                // Generate suggestions
                conflict.Suggestions.Add($"Consider using different keybinds for these {buttonList.Count} buttons");
                conflict.Suggestions.Add("Users can only use one button at a time with this keybind");

                // Jika bisa auto-resolve, beri priority suggestion
                if (conflict.CanAutoResolve)
                {
                    var highestPriority = buttonList.OrderByDescending(b => b.Priority).First();
                    conflict.Suggestions.Add($"Auto-resolve: Prioritize '{highestPriority.DisplayName}' (highest priority: {highestPriority.Priority})");
                }

                conflicts.Add(conflict);

                _monitor.Log(
                    $"Keybind conflict: '{group.Key}' used by {buttonList.Count} button(s)",
                    severity >= ConflictSeverity.Error ? LogLevel.Warn : LogLevel.Debug
                );
            }

            return conflicts;
        }

        /// <summary>
        /// Deteksi konflik display name.
        /// </summary>
        private List<ConflictInfo> DetectNameConflicts(List<ModKeyButton> buttons)
        {
            var conflicts = new List<ConflictInfo>();

            var nameGroups = buttons
                .GroupBy(b => b.DisplayName.ToUpperInvariant())
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                var buttonList = group.ToList();

                // Name conflict biasanya warning, kecuali dari mod yang sama
                var sameMod = buttonList.All(b => b.ModId == buttonList[0].ModId);
                var severity = sameMod ? ConflictSeverity.Error : ConflictSeverity.Warning;

                var conflict = new ConflictInfo
                {
                    Type = ConflictType.DuplicateName,
                    Severity = severity,
                    Description = $"Duplicate display name '{group.First().DisplayName}'",
                    ConflictingButtons = buttonList,
                    CanAutoResolve = !sameMod
                };

                if (sameMod)
                {
                    conflict.Suggestions.Add("Same mod registered multiple buttons with identical names - this is likely a bug");
                }
                else
                {
                    conflict.Suggestions.Add("Different mods using same display name - users might be confused");
                    conflict.Suggestions.Add("Consider adding mod prefix to display names");
                }

                conflicts.Add(conflict);
            }

            return conflicts;
        }

        /// <summary>
        /// Deteksi konflik unique ID (seharusnya tidak mungkin, tapi tetap check).
        /// </summary>
        private List<ConflictInfo> DetectIdConflicts(List<ModKeyButton> buttons)
        {
            var conflicts = new List<ConflictInfo>();

            var idGroups = buttons
                .GroupBy(b => b.UniqueId)
                .Where(g => g.Count() > 1);

            foreach (var group in idGroups)
            {
                var buttonList = group.ToList();

                var conflict = new ConflictInfo
                {
                    Type = ConflictType.DuplicateId,
                    Severity = ConflictSeverity.Critical,
                    Description = $"CRITICAL: Duplicate unique ID '{group.Key}'",
                    ConflictingButtons = buttonList,
                    CanAutoResolve = false
                };

                conflict.Suggestions.Add("This should never happen - registry validation failed");
                conflict.Suggestions.Add("One of these buttons will be ignored");
                conflict.Suggestions.Add("Contact mod authors to fix unique ID collision");

                conflicts.Add(conflict);

                _monitor.Log($"CRITICAL: Duplicate ID '{group.Key}' detected!", LogLevel.Error);
            }

            return conflicts;
        }

        /// <summary>
        /// Deteksi konflik priority (optional check).
        /// </summary>
        private List<ConflictInfo> DetectPriorityConflicts(List<ModKeyButton> buttons)
        {
            var conflicts = new List<ConflictInfo>();

            // Check for suspiciously high priorities dari mod yang sama
            var modGroups = buttons
                .GroupBy(b => b.ModId)
                .Where(g => g.Any(b => b.Priority > 900));

            foreach (var group in modGroups)
            {
                var highPriorityButtons = group.Where(b => b.Priority > 900).ToList();

                if (highPriorityButtons.Count > 0)
                {
                    var conflict = new ConflictInfo
                    {
                        Type = ConflictType.PriorityConflict,
                        Severity = ConflictSeverity.Info,
                        Description = $"Mod '{group.Key}' using very high priorities (>900)",
                        ConflictingButtons = highPriorityButtons,
                        CanAutoResolve = false
                    };

                    conflict.Suggestions.Add("High priorities should be reserved for critical buttons");
                    conflict.Suggestions.Add("Consider using priorities 0-100 for normal buttons");

                    conflicts.Add(conflict);
                }
            }

            return conflicts;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // RESOLUTION METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Attempt to auto-resolve conflicts yang bisa diresolve.
        /// </summary>
        public ConflictResolutionResult AutoResolveConflicts(ConflictDetectionResult detectionResult)
        {
            var result = new ConflictResolutionResult();

            var resolvableConflicts = detectionResult.Conflicts
                .Where(c => c.CanAutoResolve)
                .ToList();

            _monitor.Log($"Attempting to auto-resolve {resolvableConflicts.Count} conflict(s)...", LogLevel.Info);

            foreach (var conflict in resolvableConflicts)
            {
                try
                {
                    bool resolved = false;

                    switch (conflict.Type)
                    {
                        case ConflictType.DuplicateKeybind:
                            resolved = ResolveKeybindConflict(conflict, result);
                            break;

                        case ConflictType.DuplicateName:
                            resolved = ResolveNameConflict(conflict, result);
                            break;

                        default:
                            _monitor.Log($"No auto-resolve strategy for {conflict.Type}", LogLevel.Debug);
                            break;
                    }

                    if (resolved)
                    {
                        result.ResolvedConflicts.Add(conflict);
                    }
                    else
                    {
                        result.FailedConflicts.Add(conflict);
                    }
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error resolving conflict: {ex.Message}", LogLevel.Error);
                    result.FailedConflicts.Add(conflict);
                }
            }

            _monitor.Log(
                $"Auto-resolve complete: {result.ResolvedConflicts.Count} resolved, {result.FailedConflicts.Count} failed",
                result.IsFullyResolved ? LogLevel.Info : LogLevel.Warn
            );

            return result;
        }

        /// <summary>
        /// Resolve keybind conflict dengan prioritization.
        /// </summary>
        private bool ResolveKeybindConflict(ConflictInfo conflict, ConflictResolutionResult result)
        {
            // Strategy: Disable lower priority buttons
            var sorted = conflict.ConflictingButtons.OrderByDescending(b => b.Priority).ToList();
            var winner = sorted.First();

            for (int i = 1; i < sorted.Count; i++)
            {
                var loser = sorted[i];
                loser.IsEnabled = false;

                result.ActionsToken.Add($"Disabled '{loser.DisplayName}' ({loser.ModId}) - lower priority than '{winner.DisplayName}'");
                _monitor.Log($"Conflict resolution: Disabled '{loser.DisplayName}' due to keybind conflict", LogLevel.Debug);
            }

            return true;
        }

        /// <summary>
        /// Resolve name conflict dengan annotation.
        /// </summary>
        private bool ResolveNameConflict(ConflictInfo conflict, ConflictResolutionResult result)
        {
            // Strategy: Tidak bisa auto-resolve name (perlu mod update)
            // Hanya log suggestion
            result.ActionsToken.Add($"Name conflict for '{conflict.ConflictingButtons[0].DisplayName}' - manual intervention needed");

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // REPORTING
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Log comprehensive conflict report.
        /// </summary>
        public void LogConflictReport(ConflictDetectionResult detectionResult)
        {
            if (!detectionResult.HasAnyConflicts)
            {
                _monitor.Log("✓ No conflicts detected", LogLevel.Info);
                return;
            }

            _monitor.Log("╔═══════════════════════════════════════════════════════╗", LogLevel.Warn);
            _monitor.Log("║          BUTTON CONFLICT REPORT                       ║", LogLevel.Warn);
            _monitor.Log("╚═══════════════════════════════════════════════════════╝", LogLevel.Warn);
            _monitor.Log($"Scanned: {detectionResult.TotalButtonsScanned} buttons", LogLevel.Info);
            _monitor.Log($"Total Conflicts: {detectionResult.Conflicts.Count}", LogLevel.Warn);

            // Group by severity
            foreach (ConflictSeverity severity in Enum.GetValues(typeof(ConflictSeverity)))
            {
                var conflicts = detectionResult.GetBySeverity(severity).ToList();
                if (conflicts.Count == 0) continue;

                LogLevel level = severity switch
                {
                    ConflictSeverity.Critical => LogLevel.Error,
                    ConflictSeverity.Error => LogLevel.Error,
                    ConflictSeverity.Warning => LogLevel.Warn,
                    _ => LogLevel.Info
                };

                _monitor.Log($"\n[{severity}] {conflicts.Count} conflict(s):", level);

                foreach (var conflict in conflicts)
                {
                    _monitor.Log($"  • {conflict.GetSummary()}", level);

                    foreach (var suggestion in conflict.Suggestions)
                    {
                        _monitor.Log($"    → {suggestion}", LogLevel.Info);
                    }
                }
            }

            _monitor.Log("═══════════════════════════════════════════════════════", LogLevel.Warn);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Determine severity berdasarkan context.
        /// </summary>
        private ConflictSeverity DetermineSeverity(List<ModKeyButton> conflictingButtons)
        {
            // Jika semua dari mod yang sama -> Error (likely bug)
            if (conflictingButtons.All(b => b.ModId == conflictingButtons[0].ModId))
                return ConflictSeverity.Error;

            // Jika ada yang critical function -> Warning
            if (conflictingButtons.Any(b => b.Category == KeyCategory.Cheats))
                return ConflictSeverity.Warning;

            // Default -> Info
            return ConflictSeverity.Info;
        }
    }
}