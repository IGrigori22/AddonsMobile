using StardewModdingAPI;

namespace AddonsMobile.Framework
{
    // ═══════════════════════════════════════════════════════════════════════════
    // EVENT ARGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Event args untuk button registration
    /// </summary>
    public class ButtonRegisteredEventArgs : EventArgs
    {
        public ModKeyButton Button { get; }
        public bool IsUpdate { get; }

        public ButtonRegisteredEventArgs(ModKeyButton button, bool isUpdate)
        {
            Button = button;
            IsUpdate = isUpdate;
        }
    }

    /// <summary>
    /// Event args untuk button unregistration
    /// </summary>
    public class ButtonUnregisteredEventArgs : EventArgs
    {
        public string UniqueId { get; }
        public string ModId { get; }

        public ButtonUnregisteredEventArgs(string uniqueId, string modId)
        {
            UniqueId = uniqueId;
            ModId = modId;
        }
    }

    /// <summary>
    /// Event args untuk button triggered
    /// </summary>
    public class ButtonTriggeredEventArgs : EventArgs
    {
        public ModKeyButton Button { get; }
        public bool WasProgrammatic { get; }

        public ButtonTriggeredEventArgs(ModKeyButton button, bool wasProgrammatic)
        {
            Button = button;
            WasProgrammatic = wasProgrammatic;
        }
    }

    /// <summary>
    /// Event args untuk toggle state change
    /// </summary>
    public class ButtonToggledEventArgs : EventArgs
    {
        public ModKeyButton Button { get; }
        public bool NewState { get; }

        public ButtonToggledEventArgs(ModKeyButton button, bool newState)
        {
            Button = button;
            NewState = newState;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGISTRY
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Registry pusat untuk semua tombol yang didaftarkan
    /// </summary>
    public class KeyRegistry
    {
        // ═══════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════

        private readonly Dictionary<string, ModKeyButton> _registeredButtons = new();
        private readonly Dictionary<string, List<string>> _modButtons = new();
        private readonly Dictionary<KeyCategory, List<string>> _categoryButtons = new();
        private readonly IMonitor _monitor;
        private readonly object _lock = new(); // Thread safety

        // ═══════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════

        /// <summary>Dipanggil saat button baru didaftarkan</summary>
        public event EventHandler<ButtonRegisteredEventArgs> ButtonRegistered;

        /// <summary>Dipanggil saat button di-unregister</summary>
        public event EventHandler<ButtonUnregisteredEventArgs> ButtonUnregistered;

        /// <summary>Dipanggil saat button di-trigger</summary>
        public event EventHandler<ButtonTriggeredEventArgs> ButtonTriggered;

        /// <summary>Dipanggil saat toggle button berubah state</summary>
        public event EventHandler<ButtonToggledEventArgs> ButtonToggled;

        /// <summary>Dipanggil saat registry berubah (add/remove)</summary>
        public event EventHandler RegistryChanged;

        // ═══════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>Jumlah button yang terdaftar</summary>
        public int Count
        {
            get { lock (_lock) { return _registeredButtons.Count; } }
        }

        /// <summary>Jumlah mod yang mendaftar button</summary>
        public int ModCount
        {
            get { lock (_lock) { return _modButtons.Count; } }
        }

        /// <summary>Semua kategori yang memiliki button</summary>
        public IEnumerable<KeyCategory> ActiveCategories
        {
            get
            {
                lock (_lock)
                {
                    return _categoryButtons
                        .Where(kv => kv.Value.Count > 0)
                        .Select(kv => kv.Key)
                        .OrderBy(c => c.GetSortOrder())
                        .ToList();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════

        public KeyRegistry(IMonitor monitor)
        {
            _monitor = monitor;

            // Pre-initialize category dictionary
            foreach (KeyCategory category in Enum.GetValues<KeyCategory>())
            {
                _categoryButtons[category] = new List<string>();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // REGISTRATION METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mendaftarkan tombol baru
        /// </summary>
        public bool RegisterButton(ModKeyButton button)
        {
            // Validasi
            if (string.IsNullOrEmpty(button.UniqueId))
            {
                _monitor.Log("Failed to register button: UniqueId is empty", LogLevel.Error);
                return false;
            }

            if (string.IsNullOrEmpty(button.ModId))
            {
                _monitor.Log($"Failed to register button '{button.UniqueId}': ModId is empty", LogLevel.Error);
                return false;
            }

            if (!button.HasAnyAction)
            {
                _monitor.Log($"Failed to register button '{button.UniqueId}': No action defined", LogLevel.Error);
                return false;
            }

            bool isUpdate;

            lock (_lock)
            {
                // Cek duplikat
                isUpdate = _registeredButtons.ContainsKey(button.UniqueId);

                if (isUpdate)
                {
                    // Update existing - remove from old category first
                    var oldButton = _registeredButtons[button.UniqueId];
                    _categoryButtons[oldButton.Category].Remove(button.UniqueId);

                    _monitor.Log($"Button '{button.UniqueId}' already registered. Updating.", LogLevel.Debug);
                }
                else
                {
                    // Track per mod
                    if (!_modButtons.ContainsKey(button.ModId))
                        _modButtons[button.ModId] = new List<string>();

                    _modButtons[button.ModId].Add(button.UniqueId);
                }

                // Register/Update
                _registeredButtons[button.UniqueId] = button;

                // Track per category
                _categoryButtons[button.Category].Add(button.UniqueId);
            }

            _monitor.Log($"Registered button '{button.DisplayName}' ({button.UniqueId}) " +
                        $"from mod '{button.ModId}' in category '{button.Category}'", LogLevel.Debug);

            // Fire events
            ButtonRegistered?.Invoke(this, new ButtonRegisteredEventArgs(button, isUpdate));
            RegistryChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Menghapus registrasi tombol
        /// </summary>
        public bool UnregisterButton(string uniqueId)
        {
            ModKeyButton button;

            lock (_lock)
            {
                if (!_registeredButtons.TryGetValue(uniqueId, out button))
                    return false;

                _registeredButtons.Remove(uniqueId);

                if (_modButtons.ContainsKey(button.ModId))
                    _modButtons[button.ModId].Remove(uniqueId);

                _categoryButtons[button.Category].Remove(uniqueId);
            }

            _monitor.Log($"Unregistered button '{uniqueId}'", LogLevel.Debug);

            ButtonUnregistered?.Invoke(this, new ButtonUnregisteredEventArgs(uniqueId, button.ModId));
            RegistryChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Menghapus semua tombol dari mod tertentu
        /// </summary>
        public void UnregisterAllFromMod(string modId)
        {
            List<string> removedIds = new();

            lock (_lock)
            {
                if (!_modButtons.TryGetValue(modId, out var buttonIds))
                    return;

                foreach (var id in buttonIds.ToList())
                {
                    if (_registeredButtons.TryGetValue(id, out var button))
                    {
                        _categoryButtons[button.Category].Remove(id);
                        _registeredButtons.Remove(id);
                        removedIds.Add(id);
                    }
                }

                _modButtons.Remove(modId);
            }

            foreach (var id in removedIds)
            {
                ButtonUnregistered?.Invoke(this, new ButtonUnregisteredEventArgs(id, modId));
            }

            if (removedIds.Count > 0)
            {
                _monitor.Log($"Unregistered {removedIds.Count} buttons from mod '{modId}'", LogLevel.Debug);
                RegistryChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // QUERY METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mendapatkan semua tombol yang terdaftar (visible only)
        /// </summary>
        public IEnumerable<ModKeyButton> GetAllButtons()
        {
            lock (_lock)
            {
                return _registeredButtons.Values
                    .Where(b => b.ShouldShow())
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.Category.GetSortOrder())
                    .ThenBy(b => b.DisplayName)
                    .ToList();
            }
        }

        /// <summary>
        /// Mendapatkan SEMUA tombol (termasuk hidden)
        /// </summary>
        public IEnumerable<ModKeyButton> GetAllButtonsIncludingHidden()
        {
            lock (_lock)
            {
                return _registeredButtons.Values
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.Category.GetSortOrder())
                    .ToList();
            }
        }

        /// <summary>
        /// Mendapatkan tombol berdasarkan kategori
        /// </summary>
        public IEnumerable<ModKeyButton> GetButtonsByCategory(KeyCategory category)
        {
            lock (_lock)
            {
                return _categoryButtons[category]
                    .Select(id => _registeredButtons.GetValueOrDefault(id))
                    .Where(b => b != null && b.ShouldShow())
                    .OrderByDescending(b => b.Priority)
                    .ThenBy(b => b.DisplayName)
                    .ToList();
            }
        }

        /// <summary>
        /// Mendapatkan tombol berdasarkan mod
        /// </summary>
        public IEnumerable<ModKeyButton> GetButtonsByMod(string modId)
        {
            lock (_lock)
            {
                if (!_modButtons.TryGetValue(modId, out var buttonIds))
                    return Enumerable.Empty<ModKeyButton>();

                return buttonIds
                    .Select(id => _registeredButtons.GetValueOrDefault(id))
                    .Where(b => b != null && b.ShouldShow())
                    .OrderByDescending(b => b.Priority)
                    .ToList();
            }
        }

        /// <summary>
        /// Mendapatkan tombol tertentu
        /// </summary>
        public ModKeyButton GetButton(string uniqueId)
        {
            lock (_lock)
            {
                return _registeredButtons.GetValueOrDefault(uniqueId);
            }
        }

        /// <summary>
        /// Mendapatkan jumlah button per kategori
        /// </summary>
        public Dictionary<KeyCategory, int> GetButtonCountByCategory()
        {
            lock (_lock)
            {
                return _categoryButtons.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Count(id =>
                        _registeredButtons.TryGetValue(id, out var btn) && btn.ShouldShow())
                );
            }
        }

        // ═══════════════════════════════════════════════════════════
        // TRIGGER METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Trigger tombol berdasarkan ID
        /// </summary>
        public bool TriggerButton(string uniqueId, bool isProgrammatic = false)
        {
            ModKeyButton button;

            lock (_lock)
            {
                button = _registeredButtons.GetValueOrDefault(uniqueId);
            }

            if (button == null || !button.ShouldShow())
            {
                _monitor.Log($"Cannot trigger '{uniqueId}': button not found or hidden", LogLevel.Trace);
                return false;
            }

            if (!button.CanPress())
            {
                _monitor.Log($"Cannot trigger '{uniqueId}': cooldown active", LogLevel.Trace);
                return false;
            }

            try
            {
                bool wasToggled = button.IsToggled;
                button.ExecutePress();

                // Fire events
                ButtonTriggered?.Invoke(this, new ButtonTriggeredEventArgs(button, isProgrammatic));

                if (button.Type == ButtonType.Toggle && button.IsToggled != wasToggled)
                {
                    ButtonToggled?.Invoke(this, new ButtonToggledEventArgs(button, button.IsToggled));
                }

                _monitor.Log($"Triggered button '{uniqueId}'" +
                    (isProgrammatic ? " (programmatic)" : ""), LogLevel.Trace);
                return true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Error triggering button '{uniqueId}': {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Update hold state untuk button
        /// </summary>
        public void UpdateHoldButtons(float deltaTime)
        {
            List<ModKeyButton> holdButtons;

            lock (_lock)
            {
                holdButtons = _registeredButtons.Values
                    .Where(b => b.Type == ButtonType.Hold && b.IsBeingHeld)
                    .ToList();
            }

            foreach (var button in holdButtons)
            {
                try
                {
                    button.ExecuteHold(deltaTime);
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error in hold action for '{button.UniqueId}': {ex.Message}", LogLevel.Error);
                    button.IsBeingHeld = false;
                }
            }
        }

        /// <summary>
        /// Release semua hold button
        /// </summary>
        public void ReleaseAllHoldButtons()
        {
            List<ModKeyButton> holdButtons;

            lock (_lock)
            {
                holdButtons = _registeredButtons.Values
                    .Where(b => b.Type == ButtonType.Hold && b.IsBeingHeld)
                    .ToList();
            }

            foreach (var button in holdButtons)
            {
                try
                {
                    button.ExecuteRelease();
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error releasing button '{button.UniqueId}': {ex.Message}", LogLevel.Error);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // STATE MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Set toggle state untuk button
        /// </summary>
        public bool SetToggleState(string uniqueId, bool toggled)
        {
            ModKeyButton button;

            lock (_lock)
            {
                button = _registeredButtons.GetValueOrDefault(uniqueId);
            }

            if (button == null || button.Type != ButtonType.Toggle)
                return false;

            bool oldState = button.IsToggled;
            button.SetToggleState(toggled, true);

            if (oldState != toggled)
            {
                ButtonToggled?.Invoke(this, new ButtonToggledEventArgs(button, toggled));
            }

            return true;
        }

        /// <summary>
        /// Set enabled state untuk button
        /// </summary>
        public bool SetEnabled(string uniqueId, bool enabled)
        {
            lock (_lock)
            {
                var button = _registeredButtons.GetValueOrDefault(uniqueId);
                if (button == null) return false;

                button.IsEnabled = enabled;
                return true;
            }
        }

        /// <summary>
        /// Reset semua button states
        /// </summary>
        public void ResetAllStates()
        {
            lock (_lock)
            {
                foreach (var button in _registeredButtons.Values)
                {
                    button.ResetState();
                }
            }
        }
    }
}