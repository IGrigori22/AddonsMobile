using AddonsMobile.Framework.Events;
using AddonsMobile.Framework.Validation;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Registry pusat untuk semua button yang didaftarkan.
    /// Thread-safe dan mendukung event-driven architecture.
    /// </summary>
    public class KeyRegistry
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // PRIVATE FIELDS
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly Dictionary<string, ModKeyButton> _registeredButtons;
        private readonly Dictionary<string, List<string>> _modButtons;
        private readonly Dictionary<KeyCategory, List<string>> _categoryButtons;
        private readonly IMonitor _monitor;
        private readonly object _lock;

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Dipanggil saat button baru didaftarkan atau diupdate</summary>
        public event EventHandler<ButtonEventArgs.ButtonRegisteredEventArgs> ButtonRegistered;

        /// <summary>Dipanggil saat button di-unregister</summary>
        public event EventHandler<ButtonEventArgs.ButtonUnregisteredEventArgs> ButtonUnregistered;

        /// <summary>Dipanggil saat button di-trigger</summary>
        public event EventHandler<ButtonEventArgs.ButtonTriggeredEventArgs> ButtonTriggered;

        /// <summary>Dipanggil saat toggle button berubah state</summary>
        public event EventHandler<ButtonEventArgs.ButtonToggledEventArgs> ButtonToggled;

        /// <summary>Dipanggil saat registry berubah</summary>
        public event EventHandler<ButtonEventArgs.RegistryChangedEventArgs> RegistryChanged;

        // ═══════════════════════════════════════════════════════════════════════════
        // PUBLIC PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Jumlah button yang terdaftar</summary>
        public int Count => ExecuteThreadSafe(() => _registeredButtons.Count);

        /// <summary>Jumlah mod yang mendaftar button</summary>
        public int ModCount => ExecuteThreadSafe(() => _modButtons.Count);

        /// <summary>Apakah registry kosong</summary>
        public bool IsEmpty => Count == 0;

        /// <summary>Semua kategori yang memiliki button (sorted)</summary>
        public IEnumerable<KeyCategory> ActiveCategories
        {
            get
            {
                return ExecuteThreadSafe(() =>
                    _categoryButtons
                        .Where(kv => kv.Value.Count > 0)
                        .Select(kv => kv.Key)
                        .OrderBy(c => c.GetSortOrder())
                        .ToList()
                );
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public KeyRegistry(IMonitor monitor)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _lock = new object();

            _registeredButtons = new Dictionary<string, ModKeyButton>();
            _modButtons = new Dictionary<string, List<string>>();
            _categoryButtons = new Dictionary<KeyCategory, List<string>>();

            InitializeCategoryDictionary();

            _monitor.Log("KeyRegistry initialized", LogLevel.Trace);
        }

        /// <summary>
        /// Pre-initialize semua kategori untuk menghindari null checks.
        /// </summary>
        private void InitializeCategoryDictionary()
        {
            foreach (KeyCategory category in Enum.GetValues<KeyCategory>())
            {
                _categoryButtons[category] = new List<string>();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // REGISTRATION METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mendaftarkan button baru atau update button yang sudah ada.
        /// </summary>
        /// <returns>True jika berhasil, false jika validasi gagal</returns>
        public bool RegisterButton(ModKeyButton button)
        {
            // Validasi
            var (isValid, errorMessage) = ButtonValidator.Validate(button);
            if (!isValid)
            {
                ButtonValidator.LogValidationError(_monitor, button, errorMessage);
                return false;
            }

            bool isUpdate;
            ModKeyButton? oldButton = null;

            // Registration dengan thread safety
            lock (_lock)
            {
                isUpdate = _registeredButtons.TryGetValue(button.UniqueId, out oldButton);

                if (isUpdate)
                {
                    HandleButtonUpdate(button, oldButton);
                }
                else
                {
                    HandleButtonRegistration(button);
                }

                _registeredButtons[button.UniqueId] = button;
            }

            // Logging
            LogButtonRegistration(button, isUpdate);

            // Fire events (outside lock to prevent deadlock)
            RaiseButtonRegisteredEvent(button, isUpdate);
            RaiseRegistryChangedEvent(
                isUpdate ? ButtonEventArgs.RegistryChangeType.ButtonUpdated
                         : ButtonEventArgs.RegistryChangeType.ButtonAdded
            );

            return true;
        }

        /// <summary>
        /// Handle logic untuk update button yang sudah ada.
        /// </summary>
        private void HandleButtonUpdate(ModKeyButton newButton, ModKeyButton oldButton)
        {
            // Remove dari kategori lama jika berbeda
            if (oldButton.Category != newButton.Category)
            {
                _categoryButtons[oldButton.Category].Remove(newButton.UniqueId);
                _categoryButtons[newButton.Category].Add(newButton.UniqueId);
            }

            _monitor.Log($"Updating existing button '{newButton.UniqueId}'", LogLevel.Debug);
        }

        /// <summary>
        /// Handle logic untuk registrasi button baru.
        /// </summary>
        private void HandleButtonRegistration(ModKeyButton button)
        {
            // Track per mod
            if (!_modButtons.ContainsKey(button.ModId))
            {
                _modButtons[button.ModId] = new List<string>();
            }
            _modButtons[button.ModId].Add(button.UniqueId);

            // Track per category
            _categoryButtons[button.Category].Add(button.UniqueId);
        }

        /// <summary>
        /// Log informasi registrasi button.
        /// </summary>
        private void LogButtonRegistration(ModKeyButton button, bool isUpdate)
        {
            string action = isUpdate ? "Updated" : "Registered";
            _monitor.Log(
                $"✓ {action} button '{button.DisplayName}' ({button.UniqueId}) " +
                $"from '{button.ModId}' in category '{button.Category}'",
                LogLevel.Debug
            );
        }

        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Menghapus registrasi button berdasarkan unique ID.
        /// </summary>
        /// <returns>True jika berhasil dihapus, false jika button tidak ditemukan</returns>
        public bool UnregisterButton(string uniqueId)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                _monitor.Log("Cannot unregister button: UniqueId is empty", LogLevel.Error);
                return false;
            }

            ModKeyButton? button;

            lock (_lock)
            {
                // Cari button
                if (!_registeredButtons.TryGetValue(uniqueId, out button))
                {
                    _monitor.Log($"Button '{uniqueId}' not found in registry", LogLevel.Trace);
                    return false;
                }

                // Remove dari semua tracking
                _registeredButtons.Remove(uniqueId);

                if (_modButtons.TryGetValue(button.ModId, out var modButtonList))
                {
                    modButtonList.Remove(uniqueId);
                    if (modButtonList.Count == 0)
                    {
                        _modButtons.Remove(button.ModId);
                    }
                }

                _categoryButtons[button.Category].Remove(uniqueId);
            }

            _monitor.Log($"✓ Unregistered button '{uniqueId}'", LogLevel.Debug);

            // Fire events
            RaiseButtonUnregisteredEvent(uniqueId, button.ModId);
            RaiseRegistryChangedEvent(ButtonEventArgs.RegistryChangeType.ButtonRemoved);

            return true;
        }

        /// <summary>
        /// Menghapus semua button dari mod tertentu.
        /// </summary>
        /// <returns>Jumlah button yang dihapus</returns>
        public int UnregisterAllFromMod(string modId)
        {
            if (string.IsNullOrWhiteSpace(modId))
            {
                _monitor.Log("Cannot unregister mod: ModId is empty", LogLevel.Error);
                return 0;
            }

            List<(string uniqueId, ModKeyButton button)> removedButtons = new();

            lock (_lock)
            {
                if (!_modButtons.TryGetValue(modId, out var buttonIds) || buttonIds.Count == 0)
                {
                    _monitor.Log($"No buttons registered for mod '{modId}'", LogLevel.Trace);
                    return 0;
                }

                // Collect buttons to remove
                foreach (var id in buttonIds.ToList())
                {
                    if (_registeredButtons.TryGetValue(id, out var button))
                    {
                        _categoryButtons[button.Category].Remove(id);
                        _registeredButtons.Remove(id);
                        removedButtons.Add((id, button));
                    }
                }

                _modButtons.Remove(modId);
            }

            // Fire events for each removed button
            foreach (var (uniqueId, _) in removedButtons)
            {
                RaiseButtonUnregisteredEvent(uniqueId, modId);
            }

            if (removedButtons.Count > 0)
            {
                _monitor.Log($"✓ Unregistered {removedButtons.Count} button(s) from mod '{modId}'", LogLevel.Info);
                RaiseRegistryChangedEvent(ButtonEventArgs.RegistryChangeType.ModUnregistered);
            }

            return removedButtons.Count;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // QUERY METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mendapatkan semua button yang visible (ShouldShow = true).
        /// Hasil sudah di-sort berdasarkan priority dan kategori.
        /// </summary>
        public IEnumerable<ModKeyButton> GetAllButtons()
        {
            return ExecuteThreadSafe(() =>
                _registeredButtons.Values
                    .Where(b => b.ShouldShow())
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.Category.GetSortOrder())
                    .ThenBy(b => b.DisplayName)
                    .ToList()
            );
        }

        /// <summary>
        /// Mendapatkan SEMUA button termasuk yang hidden.
        /// Berguna untuk debugging dan admin tools.
        /// </summary>
        public IEnumerable<ModKeyButton> GetAllButtonsIncludingHidden()
        {
            return ExecuteThreadSafe(() =>
                _registeredButtons.Values
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.Category.GetSortOrder())
                    .ThenBy(b => b.DisplayName)
                    .ToList()
            );
        }

        /// <summary>
        /// Mendapatkan button berdasarkan kategori (visible only).
        /// </summary>
        public IEnumerable<ModKeyButton> GetButtonsByCategory(KeyCategory category)
        {
            return ExecuteThreadSafe(() =>
            {
                if (!_categoryButtons.TryGetValue(category, out var buttonIds))
                    return Enumerable.Empty<ModKeyButton>();

                return buttonIds
                    .Select(id => _registeredButtons.GetValueOrDefault(id))
                    .Where(b => b != null && b.ShouldShow())
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.DisplayName)
                    .ToList();
            });
        }

        /// <summary>
        /// Mendapatkan button berdasarkan mod (visible only).
        /// </summary>
        public IEnumerable<ModKeyButton> GetButtonsByMod(string modId)
        {
            if (string.IsNullOrWhiteSpace(modId))
                return Enumerable.Empty<ModKeyButton>();

            return ExecuteThreadSafe(() =>
            {
                if (!_modButtons.TryGetValue(modId, out var buttonIds))
                    return Enumerable.Empty<ModKeyButton>();

                return buttonIds
                    .Select(id => _registeredButtons.GetValueOrDefault(id))
                    .Where(b => b != null && b.ShouldShow())
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.DisplayName)
                    .ToList();
            });
        }

        /// <summary>
        /// Mendapatkan button tertentu berdasarkan unique ID.
        /// </summary>
        /// <returns>Button jika ditemukan, null jika tidak ada</returns>
        public ModKeyButton? GetButton(string uniqueId)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                return null;

            return ExecuteThreadSafe(() =>
                _registeredButtons.GetValueOrDefault(uniqueId)
            );
        }

        /// <summary>
        /// Cek apakah button dengan ID tertentu terdaftar.
        /// </summary>
        public bool HasButton(string uniqueId)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                return false;

            return ExecuteThreadSafe(() =>
                _registeredButtons.ContainsKey(uniqueId)
            );
        }

        /// <summary>
        /// Mendapatkan statistik jumlah button per kategori.
        /// </summary>
        public Dictionary<KeyCategory, int> GetButtonCountByCategory()
        {
            return ExecuteThreadSafe(() =>
                _categoryButtons.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Count(id =>
                        _registeredButtons.TryGetValue(id, out var btn) && btn.ShouldShow())
                )
            );
        }

        /// <summary>
        /// Mendapatkan statistik jumlah button per mod.
        /// </summary>
        public Dictionary<string, int> GetButtonCountByMod()
        {
            return ExecuteThreadSafe(() =>
                _modButtons.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Count(id =>
                        _registeredButtons.TryGetValue(id, out var btn) && btn.ShouldShow())
                )
            );
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // TRIGGER METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Trigger button berdasarkan unique ID.
        /// </summary>
        /// <param name="uniqueId">ID button yang akan di-trigger</param>
        /// <param name="isProgrammatic">True jika trigger dari code, bukan user</param>
        /// <param name="logAction">True untuk log action ke console</param>
        /// <returns>True jika berhasil, false jika gagal atau tidak bisa di-trigger</returns>
        public bool TriggerButton(string uniqueId, bool isProgrammatic = false, bool logAction = false)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                _monitor.Log("Cannot trigger button: UniqueId is empty", LogLevel.Error);
                return false;
            }

            ModKeyButton button = GetButton(uniqueId);

            // Validasi button
            if (button == null)
            {
                if (logAction)
                    _monitor.Log($"Cannot trigger '{uniqueId}': button not found", LogLevel.Warn);
                return false;
            }

            if (!button.ShouldShow())
            {
                if (logAction)
                    _monitor.Log($"Cannot trigger '{uniqueId}': button is hidden", LogLevel.Debug);
                return false;
            }

            if (!button.CanPress())
            {
                if (logAction)
                    _monitor.Log($"Cannot trigger '{uniqueId}': cooldown active", LogLevel.Debug);
                return false;
            }

            // Execute button action
            try
            {
                bool wasToggled = button.IsToggled;
                button.ExecutePress();

                // Fire events
                var triggerEvent = new ButtonEventArgs.ButtonTriggeredEventArgs(button, isProgrammatic);
                RaiseButtonTriggeredEvent(triggerEvent);

                // Fire toggle event if state changed
                if (button.Type == ButtonType.Toggle && button.IsToggled != wasToggled)
                {
                    RaiseButtonToggledEvent(button, button.IsToggled, wasToggled);
                }

                if (logAction)
                {
                    string source = isProgrammatic ? " (programmatic)" : "";
                    _monitor.Log($"✓ Triggered button '{uniqueId}'{source}", LogLevel.Info);
                }

                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"✗ Error triggering button '{uniqueId}': {ex.Message}", LogLevel.Error);
                _monitor.Log(ex.StackTrace, LogLevel.Trace);
                return false;
            }
        }

        /// <summary>
        /// Update semua hold buttons (dipanggil setiap frame).
        /// </summary>
        public void UpdateHoldButtons(float deltaTime)
        {
            List<ModKeyButton> holdButtons = ExecuteThreadSafe(() =>
                _registeredButtons.Values
                    .Where(b => b.Type == ButtonType.Hold && b.IsBeingHeld)
                    .ToList()
            );

            foreach (var button in holdButtons)
            {
                try
                {
                    button.ExecuteHold(deltaTime);
                }
                catch (Exception ex)
                {
                    _monitor.Log($"✗ Error in hold action for '{button.UniqueId}': {ex.Message}", LogLevel.Error);
                    button.IsBeingHeld = false;
                }
            }
        }

        /// <summary>
        /// Release semua hold buttons (biasanya saat menu ditutup).
        /// </summary>
        public void ReleaseAllHoldButtons()
        {
            List<ModKeyButton> holdButtons = ExecuteThreadSafe(() =>
                _registeredButtons.Values
                    .Where(b => b.Type == ButtonType.Hold && b.IsBeingHeld)
                    .ToList()
            );

            foreach (var button in holdButtons)
            {
                try
                {
                    button.ExecuteRelease();
                }
                catch (Exception ex)
                {
                    _monitor.Log($"✗ Error releasing button '{button.UniqueId}': {ex.Message}", LogLevel.Error);
                }
            }

            if (holdButtons.Count > 0)
            {
                _monitor.Log($"Released {holdButtons.Count} hold button(s)", LogLevel.Debug);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STATE MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Set toggle state untuk button tertentu.
        /// </summary>
        public bool SetToggleState(string uniqueId, bool toggled, bool invokeAction = false)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                return false;

            ModKeyButton button = GetButton(uniqueId);

            if (button == null)
            {
                _monitor.Log($"Cannot set toggle state: button '{uniqueId}' not found", LogLevel.Warn);
                return false;
            }

            if (button.Type != ButtonType.Toggle)
            {
                _monitor.Log($"Cannot set toggle state: button '{uniqueId}' is not a toggle button", LogLevel.Warn);
                return false;
            }

            bool oldState = button.IsToggled;

            if (oldState == toggled)
                return true; // Already in desired state

            button.SetToggleState(toggled, invokeAction);

            // Fire event
            RaiseButtonToggledEvent(button, toggled, oldState);

            _monitor.Log($"Toggle state changed for '{uniqueId}': {oldState} → {toggled}", LogLevel.Debug);

            return true;
        }

        /// <summary>
        /// Set enabled state untuk button tertentu.
        /// </summary>
        public bool SetEnabled(string uniqueId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                return false;

            ModKeyButton button = GetButton(uniqueId);

            if (button == null)
                return false;

            lock (_lock)
            {
                button.IsEnabled = enabled;
            }

            _monitor.Log($"Button '{uniqueId}' enabled state: {enabled}", LogLevel.Debug);

            return true;
        }

        /// <summary>
        /// Reset semua button states ke default.
        /// </summary>
        public void ResetAllStates()
        {
            List<ModKeyButton> buttons = ExecuteThreadSafe(() =>
                _registeredButtons.Values.ToList()
            );

            foreach (var button in buttons)
            {
                try
                {
                    button.ResetState();
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error resetting state for '{button.UniqueId}': {ex.Message}", LogLevel.Error);
                }
            }

            _monitor.Log($"Reset state for {buttons.Count} button(s)", LogLevel.Debug);
            RaiseRegistryChangedEvent(ButtonEventArgs.RegistryChangeType.StateReset);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENT HELPERS
        // ═══════════════════════════════════════════════════════════════════════════

        private void RaiseButtonRegisteredEvent(ModKeyButton button, bool isUpdate)
        {
            try
            {
                ButtonRegistered?.Invoke(this, new ButtonEventArgs.ButtonRegisteredEventArgs(button, isUpdate));
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error in ButtonRegistered event handler: {ex.Message}", LogLevel.Error);
            }
        }

        private void RaiseButtonUnregisteredEvent(string uniqueId, string modId)
        {
            try
            {
                ButtonUnregistered?.Invoke(this, new ButtonEventArgs.ButtonUnregisteredEventArgs(uniqueId, modId));
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error in ButtonUnregistered event handler: {ex.Message}", LogLevel.Error);
            }
        }

        private void RaiseButtonTriggeredEvent(ButtonEventArgs.ButtonTriggeredEventArgs args)
        {
            try
            {
                ButtonTriggered?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error in ButtonTriggered event handler: {ex.Message}", LogLevel.Error);
            }
        }

        private void RaiseButtonToggledEvent(ModKeyButton button, bool newState, bool oldState)
        {
            try
            {
                ButtonToggled?.Invoke(this, new ButtonEventArgs.ButtonToggledEventArgs(button, newState, oldState));
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error in ButtonToggled event handler: {ex.Message}", LogLevel.Error);
            }
        }

        private void RaiseRegistryChangedEvent(ButtonEventArgs.RegistryChangeType changeType)
        {
            try
            {
                var args = new ButtonEventArgs.RegistryChangedEventArgs(changeType, Count, ModCount);
                RegistryChanged?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error in RegistryChanged event handler: {ex.Message}", LogLevel.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // THREAD SAFETY HELPERS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Execute function dengan thread safety.
        /// </summary>
        private T ExecuteThreadSafe<T>(Func<T> func)
        {
            lock (_lock)
            {
                return func();
            }
        }

        /// <summary>
        /// Execute action dengan thread safety.
        /// </summary>
        private void ExecuteThreadSafe(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // DEBUG & DIAGNOSTICS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mendapatkan informasi diagnostik registry.
        /// </summary>
        public string GetDiagnostics()
        {
            return ExecuteThreadSafe(() =>
            {
                int totalButtons = _registeredButtons.Count;
                int visibleButtons = _registeredButtons.Values.Count(b => b.ShouldShow());
                int totalMods = _modButtons.Count;
                int activeCategories = _categoryButtons.Count(kv => kv.Value.Count > 0);

                return $"Registry Diagnostics:\n" +
                       $"  Total Buttons: {totalButtons} ({visibleButtons} visible)\n" +
                       $"  Total Mods: {totalMods}\n" +
                       $"  Active Categories: {activeCategories}";
            });
        }
    }
}