using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Data model untuk custom button yang didaftarkan oleh mod.
    /// Immutable setelah registration (kecuali runtime state).
    /// </summary>
    public sealed class ModKeyButton
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // IDENTITY PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        public string UniqueId { get; private set; } = string.Empty;
        public string ModId { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public KeyCategory Category { get; private set; } = KeyCategory.Miscellaneous;

        // ═══════════════════════════════════════════════════════════════════════════
        // VISUAL PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        public Texture2D? IconTexture { get; private set; }
        public Rectangle? IconSourceRect { get; private set; }
        public Color TintColor { get; private set; } = Color.White;
        public Color ToggledTintColor { get; private set; } = Color.LightGreen;
        public Color DisabledTintColor { get; private set; } = Color.Gray;

        // ═══════════════════════════════════════════════════════════════════════════
        // BEHAVIOR PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        public ButtonType Type { get; private set; } = ButtonType.Momentary;
        public int Priority { get; private set; } = 0;
        public int PressCooldown { get; private set; } = 250;
        public string? OriginalKeybind { get; private set; }

        // ═══════════════════════════════════════════════════════════════════════════
        // CONDITIONS (PREDICATES)
        // ═══════════════════════════════════════════════════════════════════════════

        public Func<bool>? VisibilityCondition { get; private set; }
        public Func<bool>? EnabledCondition { get; private set; }

        // ═══════════════════════════════════════════════════════════════════════════
        // ACTIONS (CALLBACKS)
        // ═══════════════════════════════════════════════════════════════════════════

        public Action? OnPress { get; private set; }
        public Action<float>? OnHold { get; private set; }
        public Action? OnRelease { get; private set; }
        public Action<bool>? OnToggle { get; private set; }

        // ═══════════════════════════════════════════════════════════════════════════
        // RUNTIME STATE (Mutable, Thread-Safe Access)
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly object _stateLock = new object();
        private DateTime _lastPressed = DateTime.MinValue;
        private bool _isToggled = false;
        private bool _isBeingHeld = false;
        private float _holdDuration = 0f;
        private bool _isEnabled = true;

        internal DateTime LastPressed
        {
            get { lock (_stateLock) return _lastPressed; }
            set { lock (_stateLock) _lastPressed = value; }
        }

        public bool IsToggled
        {
            get { lock (_stateLock) return _isToggled; }
            internal set { lock (_stateLock) _isToggled = value; }
        }

        public bool IsBeingHeld
        {
            get { lock (_stateLock) return _isBeingHeld; }
            internal set { lock (_stateLock) _isBeingHeld = value; }
        }

        public float HoldDuration
        {
            get { lock (_stateLock) return _holdDuration; }
            internal set { lock (_stateLock) _holdDuration = value; }
        }

        public bool IsEnabled
        {
            get { lock (_stateLock) return _isEnabled; }
            set { lock (_stateLock) _isEnabled = value; }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // COMPUTED PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Apakah button bisa di-press sekarang (cooldown & condition check).
        /// </summary>
        public bool CanPress()
        {
            var now = DateTime.UtcNow;

            lock (_stateLock)
            {
                if (!_isEnabled)
                    return false;

                // Check enabled condition (bisa throw, handle di luar)
                try
                {
                    if (EnabledCondition != null && !EnabledCondition())
                        return false;
                }
                catch
                {
                    return false; // Condition error = disabled
                }

                double msSinceLastPress = (now - _lastPressed).TotalMilliseconds;
                return msSinceLastPress >= PressCooldown;
            }
        }

        /// <summary>
        /// Apakah button harus ditampilkan (pure visibility check).
        /// Disabled buttons masih visible (grayed out).
        /// </summary>
        public bool ShouldShow()
        {
            try
            {
                return VisibilityCondition?.Invoke() ?? true;
            }
            catch
            {
                return false; // Visibility condition error = hide
            }
        }

        /// <summary>
        /// Apakah button visible DAN enabled untuk interaksi.
        /// </summary>
        public bool IsInteractable()
        {
            if (!ShouldShow())
                return false;

            lock (_stateLock)
            {
                if (!_isEnabled)
                    return false;

                try
                {
                    return EnabledCondition?.Invoke() ?? true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Warna tint aktif berdasarkan state saat ini.
        /// </summary>
        public Color GetCurrentTintColor()
        {
            lock (_stateLock)
            {
                if (!_isEnabled)
                    return DisabledTintColor;

                // Check enabled condition
                try
                {
                    if (EnabledCondition != null && !EnabledCondition())
                        return DisabledTintColor;
                }
                catch
                {
                    return DisabledTintColor;
                }

                if (Type == ButtonType.Toggle && _isToggled)
                    return ToggledTintColor;

                return TintColor;
            }
        }

        public bool HasAnyAction => OnPress != null || OnHold != null || OnToggle != null;

        public string GetKeybindText()
        {
            return string.IsNullOrWhiteSpace(OriginalKeybind)
                ? "Not bound"
                : OriginalKeybind;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR (Private - gunakan Builder)
        // ═══════════════════════════════════════════════════════════════════════════

        private ModKeyButton() { }

        // ═══════════════════════════════════════════════════════════════════════════
        // EXECUTION METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Execute press action berdasarkan button type.
        /// </summary>
        /// <returns>True jika berhasil execute</returns>
        public bool ExecutePress()
        {
            var now = DateTime.UtcNow;

            if (!CanPress())
                return false;

            // Capture actions before lock for safer invocation
            Action? pressAction;
            Action<bool>? toggleAction;

            lock (_stateLock)
            {
                _lastPressed = now;
                pressAction = OnPress;
                toggleAction = OnToggle;

                switch (Type)
                {
                    case ButtonType.Momentary:
                        // Execute outside lock
                        break;

                    case ButtonType.Toggle:
                        bool previousState = _isToggled;
                        _isToggled = !_isToggled;

                        try
                        {
                            // Must invoke inside lock to maintain state consistency
                            toggleAction?.Invoke(_isToggled);
                            pressAction?.Invoke();
                        }
                        catch
                        {
                            // Rollback on error
                            _isToggled = previousState;
                            throw;
                        }
                        return true;

                    case ButtonType.Hold:
                        _isBeingHeld = true;
                        _holdDuration = 0f;
                        break;
                }
            }

            // Execute outside lock for Momentary and Hold (initial press)
            try
            {
                pressAction?.Invoke();
                return true;
            }
            catch
            {
                // Reset hold state on error
                if (Type == ButtonType.Hold)
                {
                    lock (_stateLock)
                    {
                        _isBeingHeld = false;
                        _holdDuration = 0f;
                    }
                }
                throw;
            }
        }

        /// <summary>
        /// Execute hold action (dipanggil setiap frame saat di-hold).
        /// </summary>
        /// <param name="deltaTime">Delta time dalam detik</param>
        public void ExecuteHold(float deltaTime)
        {
            if (Type != ButtonType.Hold)
                return;

            Action<float>? holdAction;

            lock (_stateLock)
            {
                if (!_isBeingHeld)
                    return;

                _holdDuration += deltaTime;
                holdAction = OnHold;
            }

            // Invoke outside lock to prevent deadlock
            try
            {
                holdAction?.Invoke(deltaTime);
            }
            catch
            {
                ExecuteRelease();
                throw;
            }
        }

        /// <summary>
        /// Execute release action (untuk ButtonType.Hold).
        /// </summary>
        public void ExecuteRelease()
        {
            if (Type != ButtonType.Hold)
                return;

            Action? releaseAction;
            bool wasHeld;

            lock (_stateLock)
            {
                wasHeld = _isBeingHeld;
                _isBeingHeld = false;
                _holdDuration = 0f;
                releaseAction = OnRelease;
            }

            if (wasHeld)
            {
                releaseAction?.Invoke();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STATE MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Set toggle state secara programmatic (untuk ButtonType.Toggle).
        /// </summary>
        public void SetToggleState(bool toggled, bool invokeCallback = true)
        {
            if (Type != ButtonType.Toggle)
                return;

            Action<bool>? toggleAction = null;
            bool stateChanged;

            lock (_stateLock)
            {
                stateChanged = _isToggled != toggled;
                _isToggled = toggled;

                if (stateChanged && invokeCallback)
                {
                    toggleAction = OnToggle;
                }
            }

            // Invoke outside lock
            if (toggleAction != null)
            {
                toggleAction.Invoke(toggled);
            }
        }

        /// <summary>
        /// Reset semua runtime state ke default.
        /// </summary>
        public void ResetState()
        {
            lock (_stateLock)
            {
                _lastPressed = DateTime.MinValue;
                _isToggled = false;
                _isBeingHeld = false;
                _holdDuration = 0f;
                // Note: _isEnabled tidak di-reset karena controlled by user
            }
        }

        /// <summary>
        /// Force release jika sedang held (untuk cleanup).
        /// </summary>
        public void ForceRelease()
        {
            if (Type != ButtonType.Hold)
                return;

            bool wasHeld;
            lock (_stateLock)
            {
                wasHeld = _isBeingHeld;
                _isBeingHeld = false;
                _holdDuration = 0f;
            }

            // Tidak invoke OnRelease karena ini force release
            // Jika perlu invoke, gunakan ExecuteRelease()
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // BUILDER PATTERN
        // ═══════════════════════════════════════════════════════════════════════════

        public static Builder CreateBuilder(string uniqueId, string displayName)
        {
            return new Builder(uniqueId, displayName);
        }

        public class Builder
        {
            private readonly ModKeyButton _button;
            private bool _isBuilt = false;

            internal Builder(string uniqueId, string displayName)
            {
                if (string.IsNullOrWhiteSpace(uniqueId))
                    throw new ArgumentException("UniqueId cannot be empty", nameof(uniqueId));

                if (string.IsNullOrWhiteSpace(displayName))
                    throw new ArgumentException("DisplayName cannot be empty", nameof(displayName));

                _button = new ModKeyButton
                {
                    UniqueId = uniqueId.Trim(),
                    DisplayName = displayName.Trim()
                };
            }

            private void ThrowIfBuilt()
            {
                if (_isBuilt)
                    throw new InvalidOperationException("Button has already been built. Create a new builder.");
            }

            // Identity
            public Builder WithModId(string modId)
            {
                ThrowIfBuilt();
                _button.ModId = modId ?? throw new ArgumentNullException(nameof(modId));
                return this;
            }

            public Builder WithDescription(string? description)
            {
                ThrowIfBuilt();
                _button.Description = description ?? string.Empty;
                return this;
            }

            public Builder WithCategory(KeyCategory category)
            {
                ThrowIfBuilt();
                _button.Category = category;
                return this;
            }

            // Visual
            public Builder WithIcon(Texture2D? texture, Rectangle? sourceRect = null)
            {
                ThrowIfBuilt();
                _button.IconTexture = texture;
                _button.IconSourceRect = sourceRect;
                return this;
            }

            public Builder WithTint(Color normal, Color? toggled = null, Color? disabled = null)
            {
                ThrowIfBuilt();
                _button.TintColor = normal;
                _button.ToggledTintColor = toggled ?? Color.LightGreen;
                _button.DisabledTintColor = disabled ?? Color.Gray;
                return this;
            }

            // Behavior
            public Builder WithType(ButtonType type)
            {
                ThrowIfBuilt();
                _button.Type = type;
                return this;
            }

            public Builder WithPriority(int priority)
            {
                ThrowIfBuilt();
                _button.Priority = Math.Clamp(priority, 0, 1000);
                return this;
            }

            public Builder WithCooldown(int milliseconds)
            {
                ThrowIfBuilt();
                _button.PressCooldown = Math.Max(0, milliseconds);
                return this;
            }

            public Builder WithKeybind(string? keybind)
            {
                ThrowIfBuilt();
                _button.OriginalKeybind = keybind;
                return this;
            }

            // Conditions
            public Builder WithVisibilityCondition(Func<bool>? condition)
            {
                ThrowIfBuilt();
                _button.VisibilityCondition = condition;
                return this;
            }

            public Builder WithEnabledCondition(Func<bool>? condition)
            {
                ThrowIfBuilt();
                _button.EnabledCondition = condition;
                return this;
            }

            // Actions - now accept nullable
            public Builder OnPress(Action? action)
            {
                ThrowIfBuilt();
                _button.OnPress = action;
                return this;
            }

            public Builder OnHold(Action<float>? action)
            {
                ThrowIfBuilt();
                _button.OnHold = action;
                return this;
            }

            public Builder OnRelease(Action? action)
            {
                ThrowIfBuilt();
                _button.OnRelease = action;
                return this;
            }

            public Builder OnToggle(Action<bool>? action)
            {
                ThrowIfBuilt();
                _button.OnToggle = action;
                return this;
            }

            // Build
            public ModKeyButton Build()
            {
                ThrowIfBuilt();

                // Validation
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(_button.ModId))
                    errors.Add("ModId must be set (use WithModId())");

                if (!_button.HasAnyAction)
                    errors.Add("At least one action must be defined (OnPress, OnHold, or OnToggle)");

                // Type-specific validation
                switch (_button.Type)
                {
                    case ButtonType.Hold when _button.OnHold == null:
                        errors.Add("Hold button requires OnHold action");
                        break;

                    case ButtonType.Toggle when _button.OnToggle == null && _button.OnPress == null:
                        errors.Add("Toggle button requires OnToggle or OnPress action");
                        break;
                }

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Button validation failed for '{_button.UniqueId}':\n- " +
                        string.Join("\n- ", errors));
                }

                _isBuilt = true;
                return _button;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // CLONING
        // ═══════════════════════════════════════════════════════════════════════════

        public ModKeyButton Clone()
        {
            var cloned = new ModKeyButton
            {
                UniqueId = UniqueId,
                ModId = ModId,
                DisplayName = DisplayName,
                Description = Description,
                Category = Category,
                IconTexture = IconTexture,
                IconSourceRect = IconSourceRect,
                TintColor = TintColor,
                ToggledTintColor = ToggledTintColor,
                DisabledTintColor = DisabledTintColor,
                Type = Type,
                Priority = Priority,
                PressCooldown = PressCooldown,
                OriginalKeybind = OriginalKeybind,
                VisibilityCondition = VisibilityCondition,
                EnabledCondition = EnabledCondition,
                OnPress = OnPress,
                OnHold = OnHold,
                OnRelease = OnRelease,
                OnToggle = OnToggle
            };

            // Copy runtime state
            lock (_stateLock)
            {
                cloned._lastPressed = _lastPressed;
                cloned._isToggled = _isToggled;
                cloned._isBeingHeld = _isBeingHeld;
                cloned._holdDuration = _holdDuration;
                cloned._isEnabled = _isEnabled;
            }

            return cloned;
        }

        public ModKeyButton CloneMetadataOnly()
        {
            return new ModKeyButton
            {
                UniqueId = UniqueId,
                ModId = ModId,
                DisplayName = DisplayName,
                Description = Description,
                Category = Category,
                IconTexture = IconTexture,
                IconSourceRect = IconSourceRect,
                TintColor = TintColor,
                ToggledTintColor = ToggledTintColor,
                DisabledTintColor = DisabledTintColor,
                Type = Type,
                Priority = Priority,
                PressCooldown = PressCooldown,
                OriginalKeybind = OriginalKeybind
                // Actions dan conditions tidak di-copy
            };
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // OVERRIDES
        // ═══════════════════════════════════════════════════════════════════════════

        public override string ToString()
        {
            string state = Type switch
            {
                ButtonType.Toggle => $"[{(IsToggled ? "ON" : "OFF")}]",
                ButtonType.Hold => IsBeingHeld ? "[HELD]" : "",
                _ => ""
            };
            return $"[{Type}] {DisplayName} ({UniqueId}) {state} - {Category}";
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(UniqueId ?? string.Empty);
        }

        public override bool Equals(object? obj)
        {
            return obj is ModKeyButton other &&
                   string.Equals(UniqueId, other.UniqueId, StringComparison.OrdinalIgnoreCase);
        }
    }
}