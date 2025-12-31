using AddonsMobile.Framework.Data;
using AddonsMobile.UI;
using StardewModdingAPI;

namespace AddonsMobile
{
    /// <summary>
    /// Handler untuk console commands debugging.
    /// Memisahkan logic console commands dari ModEntry untuk better organization.
    /// </summary>
    internal class ConsoleCommandHandler
    {
        private readonly KeyRegistry _registry;
        private readonly MobileButtonManager _buttonManager;
        private readonly IMonitor _monitor;

        public ConsoleCommandHandler(KeyRegistry registry, MobileButtonManager buttonManager, IMonitor monitor)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _buttonManager = buttonManager ?? throw new ArgumentNullException(nameof(buttonManager));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        /// <summary>
        /// Register semua console commands.
        /// </summary>
        public void RegisterAll(ICommandHelper commandHelper)
        {
            RegisterResetCommand(commandHelper);
            RegisterListCommand(commandHelper);
            RegisterToggleCommand(commandHelper);
            RegisterTriggerCommand(commandHelper);
        }

        // ════════════════════════════════════════════════════════════════
        // COMMAND: addons_reset
        // ════════════════════════════════════════════════════════════════

        private void RegisterResetCommand(ICommandHelper commandHelper)
        {
            commandHelper.Add(
                "addons_reset",
                "Reset FAB position to default.\n\nUsage: addons_reset",
                (name, args) => HandleResetCommand()
            );
        }

        private void HandleResetCommand()
        {
            try
            {
                _buttonManager.ResetPosition();
                _monitor.Log("✓ FAB position reset to default", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"✗ Failed to reset position: {ex.Message}", LogLevel.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // COMMAND: addons_list
        // ════════════════════════════════════════════════════════════════

        private void RegisterListCommand(ICommandHelper commandHelper)
        {
            commandHelper.Add(
                "addons_list",
                "List all registered buttons with their status.\n\nUsage: addons_list",
                (name, args) => HandleListCommand()
            );
        }

        private void HandleListCommand()
        {
            try
            {
                var buttons = _registry.GetAllButtonsIncludingHidden();
                int totalCount = _registry.Count;
                int visibleCount = 0;

                _monitor.Log($"╔═══ Registered Buttons ({totalCount}) ═══╗", LogLevel.Info);

                foreach (var button in buttons)
                {
                    bool isVisible = button.ShouldShow();
                    if (isVisible) visibleCount++;

                    string status = isVisible ? "✓" : "✗";
                    string statusText = isVisible ? "VISIBLE" : "HIDDEN";

                    _monitor.Log(
                        $"║ [{status}] {button.DisplayName,-20} │ {statusText,-8} │ {button.UniqueId}",
                        LogLevel.Info
                    );
                    _monitor.Log(
                        $"║     ↳ Mod: {button.ModId}",
                        LogLevel.Info
                    );
                }

                _monitor.Log($"╚═══ Visible: {visibleCount}/{totalCount} ═══╝", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"✗ Failed to list buttons: {ex.Message}", LogLevel.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // COMMAND: addons_toggle
        // ════════════════════════════════════════════════════════════════

        private void RegisterToggleCommand(ICommandHelper commandHelper)
        {
            commandHelper.Add(
                "addons_toggle",
                "Toggle FAB visibility on/off.\n\nUsage: addons_toggle",
                (name, args) => HandleToggleCommand()
            );
        }

        private void HandleToggleCommand()
        {
            try
            {
                bool newState = !_buttonManager.IsVisible;
                _buttonManager.SetVisible(newState);

                string stateText = newState ? "SHOWN" : "HIDDEN";
                _monitor.Log($"✓ FAB visibility: {stateText}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _monitor.Log($"✗ Failed to toggle visibility: {ex.Message}", LogLevel.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // COMMAND: addons_trigger
        // ════════════════════════════════════════════════════════════════

        private void RegisterTriggerCommand(ICommandHelper commandHelper)
        {
            commandHelper.Add(
                "addons_trigger",
                "Manually trigger a button by its unique ID.\n\n" +
                "Usage: addons_trigger <uniqueId>\n" +
                "Example: addons_trigger MyMod.MyButton",
                (name, args) => HandleTriggerCommand(args)
            );
        }

        private void HandleTriggerCommand(string[] args)
        {
            if (args.Length == 0)
            {
                _monitor.Log("✗ Missing button ID", LogLevel.Error);
                _monitor.Log("Usage: addons_trigger <uniqueId>", LogLevel.Info);
                _monitor.Log("Tip: Use 'addons_list' to see available button IDs", LogLevel.Info);
                return;
            }

            string buttonId = args[0];

            try
            {
                bool result = _registry.TriggerButton(buttonId, true);

                if (result)
                {
                    _monitor.Log($"✓ Successfully triggered button '{buttonId}'", LogLevel.Info);
                }
                else
                {
                    _monitor.Log($"✗ Failed to trigger button '{buttonId}'", LogLevel.Error);
                    _monitor.Log("Possible reasons:", LogLevel.Info);
                    _monitor.Log("  • Button ID not found", LogLevel.Info);
                    _monitor.Log("  • Button condition not met (ShouldShow = false)", LogLevel.Info);
                    _monitor.Log("  • Button action threw an exception", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"✗ Error triggering button: {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace, LogLevel.Trace);
            }
        }
    }
}