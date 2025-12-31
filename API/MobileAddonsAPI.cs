using AddonsMobile.Framework;
using AddonsMobile.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using FrameworkEvents = AddonsMobile.Framework.Events.ButtonEventArgs;

namespace AddonsMobile.API
{
    /// <summary>
    /// Implementasi API untuk mod lain.
    /// </summary>
    public sealed class MobileAddonsAPI : IMobileAddonsAPI, IDisposable
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly KeyRegistry _registry;
        private readonly MobileButtonManager _buttonManager;
        private readonly IMonitor _monitor;
        private readonly object _lockObject = new object();
        private bool _disposed = false;

        private const string API_VERSION = "1.0.0";

        // ═══════════════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        public string ApiVersion => API_VERSION;

        public bool IsMobilePlatform => Constants.TargetPlatform == GamePlatform.Android;

        public bool IsVisible => _buttonManager?.IsVisible ?? false;

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        public event EventHandler<ButtonRegisteredEventArgs>? ButtonRegistered;
        public event EventHandler<ButtonUnregisteredEventArgs>? ButtonUnregistered;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        public MobileAddonsAPI(KeyRegistry registry, MobileButtonManager buttonManager, IMonitor monitor)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _buttonManager = buttonManager ?? throw new ArgumentNullException(nameof(buttonManager));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));

            // Subscribe to registry events
            _registry.ButtonRegistered += OnRegistryButtonRegistered;
            _registry.ButtonUnregistered += OnRegistryButtonUnregistered;

            _monitor.Log($"MobileAddonsAPI v{API_VERSION} initialized", LogLevel.Trace);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // REGISTRATION - SIMPLE
        // ═══════════════════════════════════════════════════════════════════════════

        public bool RegisterSimpleButton(
            string uniqueId,
            string modId,
            string displayName,
            Action onPress,
            KeyCategory category = KeyCategory.Miscellaneous)
        {
            ThrowIfDisposed();

            try
            {
                ValidateBasicParams(uniqueId, modId, displayName);

                if (onPress == null)
                    throw new ArgumentNullException(nameof(onPress), "onPress action cannot be null");

                lock (_lockObject)
                {
                    var button = ModKeyButton.CreateBuilder(uniqueId, displayName)
                        .WithModId(modId)
                        .WithCategory(category)
                        .OnPress(onPress)
                        .Build();

                    bool result = _registry.RegisterButton(button);

                    if (result)
                    {
                        _monitor.Log($"API: Registered simple button '{uniqueId}' from mod '{modId}'", LogLevel.Debug);
                        RefreshUI();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Failed to register simple button '{uniqueId}': {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // REGISTRATION - BUILDER
        // ═══════════════════════════════════════════════════════════════════════════

        public IButtonBuilder CreateButton(string uniqueId, string modId)
        {
            ThrowIfDisposed();

            try
            {
                ValidateBasicParams(uniqueId, modId, null);

                _monitor.Log($"API: Creating button builder for '{uniqueId}' from mod '{modId}'", LogLevel.Trace);

                return new ApiButtonBuilder(uniqueId, modId, _registry, _buttonManager, _monitor, _lockObject);
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Failed to create button builder: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════════════

        public bool UnregisterButton(string uniqueId)
        {
            ThrowIfDisposed();

            try
            {
                if (string.IsNullOrWhiteSpace(uniqueId))
                {
                    _monitor.Log("API: Cannot unregister button - uniqueId is empty", LogLevel.Warn);
                    return false;
                }

                lock (_lockObject)
                {
                    bool result = _registry.UnregisterButton(uniqueId);

                    if (result)
                    {
                        _monitor.Log($"API: Unregistered button '{uniqueId}'", LogLevel.Debug);
                        RefreshUI();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error unregistering button '{uniqueId}': {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public int UnregisterAllFromMod(string modId)
        {
            ThrowIfDisposed();

            try
            {
                if (string.IsNullOrWhiteSpace(modId))
                {
                    _monitor.Log("API: Cannot unregister - modId is empty", LogLevel.Warn);
                    return 0;
                }

                lock (_lockObject)
                {
                    int count = _registry.UnregisterAllFromMod(modId);

                    if (count > 0)
                    {
                        _monitor.Log($"API: Unregistered {count} button(s) from mod '{modId}'", LogLevel.Info);
                        RefreshUI();
                    }

                    return count;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error unregistering buttons from mod '{modId}': {ex.Message}", LogLevel.Error);
                return 0;
            }
        }

        public bool SetButtonEnabled(string uniqueId, bool enabled)
        {
            ThrowIfDisposed();

            try
            {
                lock (_lockObject)
                {
                    bool result = _registry.SetEnabled(uniqueId, enabled);

                    if (result)
                    {
                        _monitor.Log($"API: Set button '{uniqueId}' enabled={enabled}", LogLevel.Debug);
                        RefreshUI();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error setting button enabled state: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool SetToggleState(string uniqueId, bool toggled, bool invokeCallback = false)
        {
            ThrowIfDisposed();

            try
            {
                lock (_lockObject)
                {
                    bool result = _registry.SetToggleState(uniqueId, toggled, invokeCallback);

                    if (result)
                    {
                        _monitor.Log($"API: Set toggle state for '{uniqueId}' to {toggled}", LogLevel.Debug);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error setting toggle state: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // TRIGGERING
        // ═══════════════════════════════════════════════════════════════════════════

        public bool TriggerButton(string uniqueId)
        {
            ThrowIfDisposed();

            try
            {
                return _registry.TriggerButton(uniqueId, isProgrammatic: true, logAction: true);
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error triggering button '{uniqueId}': {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // QUERIES
        // ═══════════════════════════════════════════════════════════════════════════

        public bool IsButtonRegistered(string uniqueId)
        {
            ThrowIfDisposed();

            try
            {
                return _registry.HasButton(uniqueId);
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error checking button registration: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public int GetRegisteredButtonCount()
        {
            ThrowIfDisposed();

            try
            {
                return _registry.Count;
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error getting button count: {ex.Message}", LogLevel.Error);
                return 0;
            }
        }

        public int GetButtonCountForMod(string modId)
        {
            ThrowIfDisposed();

            try
            {
                if (string.IsNullOrWhiteSpace(modId))
                    return 0;

                return _registry.GetButtonsByMod(modId).Count();
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error getting button count for mod: {ex.Message}", LogLevel.Error);
                return 0;
            }
        }

        public bool IsVersionCompatible(string minimumVersion)
        {
            try
            {
                var current = new Version(API_VERSION);
                var minimum = new Version(minimumVersion);
                return current >= minimum;
            }
            catch
            {
                _monitor.Log($"Invalid version format: {minimumVersion}", LogLevel.Warn);
                return false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // UI CONTROL
        // ═══════════════════════════════════════════════════════════════════════════

        public void RefreshUI()
        {
            if (_disposed) return;

            try
            {
                _buttonManager?.RefreshButtons();
                _monitor.Log("API: UI refreshed", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error refreshing UI: {ex.Message}", LogLevel.Error);
            }
        }

        public void SetVisible(bool visible)
        {
            ThrowIfDisposed();

            try
            {
                _buttonManager?.SetVisible(visible);
                _monitor.Log($"API: Set visibility to {visible}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error setting visibility: {ex.Message}", LogLevel.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════════════════════

        private void OnRegistryButtonRegistered(object? sender, FrameworkEvents.ButtonRegisteredEventArgs e)
        {
            if (_disposed) return;

            try
            {
                ButtonRegistered?.Invoke(this, new ButtonRegisteredEventArgs(
                    e.Button.UniqueId,
                    e.Button.ModId,
                    e.Button.DisplayName,
                    e.IsUpdate
                ));
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error in ButtonRegistered event: {ex.Message}", LogLevel.Error);
            }
        }

        private void OnRegistryButtonUnregistered(object? sender, FrameworkEvents.ButtonUnregisteredEventArgs e)
        {
            if (_disposed) return;

            try
            {
                ButtonUnregistered?.Invoke(this, new ButtonUnregisteredEventArgs(
                    e.UniqueId,
                    e.ModId
                ));
            }
            catch (Exception ex)
            {
                _monitor.Log($"API: Error in ButtonUnregistered event: {ex.Message}", LogLevel.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // VALIDATION & DISPOSAL
        // ═══════════════════════════════════════════════════════════════════════════

        private void ValidateBasicParams(string uniqueId, string modId, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
                throw new ArgumentException("uniqueId cannot be empty", nameof(uniqueId));

            if (string.IsNullOrWhiteSpace(modId))
                throw new ArgumentException("modId cannot be empty", nameof(modId));

            if (displayName != null && string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("displayName cannot be empty if provided", nameof(displayName));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MobileAddonsAPI));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Unsubscribe from events
                _registry.ButtonRegistered -= OnRegistryButtonRegistered;
                _registry.ButtonUnregistered -= OnRegistryButtonUnregistered;

                _monitor.Log("MobileAddonsAPI disposed", LogLevel.Trace);
            }

            _disposed = true;
        }

        ~MobileAddonsAPI()
        {
            Dispose(false);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // API BUTTON BUILDER (FIXED VERSION)
    // ═══════════════════════════════════════════════════════════════════════════

    internal sealed class ApiButtonBuilder : IButtonBuilder
    {
        private readonly KeyRegistry _registry;
        private readonly MobileButtonManager _buttonManager;
        private readonly IMonitor _monitor;
        private readonly object _lockObject;

        // Stored properties
        private readonly string _uniqueId;
        private readonly string _modId;
        private string _displayName;
        private string? _description;
        private KeyCategory _category = KeyCategory.Miscellaneous;
        private int _priority = 0;
        private string? _keybind;
        private Texture2D? _iconTexture;
        private Rectangle? _iconSourceRect;
        private Color _normalColor = Color.White;
        private Color? _toggledColor;
        private ButtonType _buttonType = ButtonType.Momentary;
        private int _cooldownMs = 0;
        private Func<bool>? _visibilityCondition;
        private Func<bool>? _enabledCondition;
        private Action? _onPress;
        private Action<float>? _onHold;
        private Action? _onRelease;
        private Action<bool>? _onToggle;

        public ApiButtonBuilder(
            string uniqueId,
            string modId,
            KeyRegistry registry,
            MobileButtonManager buttonManager,
            IMonitor monitor,
            object lockObject)
        {
            _uniqueId = uniqueId;
            _modId = modId;
            _displayName = uniqueId; // Default
            _registry = registry;
            _buttonManager = buttonManager;
            _monitor = monitor;
            _lockObject = lockObject;
        }

        // Basic Properties
        public IButtonBuilder WithDisplayName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _displayName = name;
            return this;
        }

        public IButtonBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public IButtonBuilder WithCategory(KeyCategory category)
        {
            _category = category;
            return this;
        }

        public IButtonBuilder WithPriority(int priority)
        {
            _priority = Math.Clamp(priority, 0, 1000);
            return this;
        }

        public IButtonBuilder WithKeybind(string keybind)
        {
            _keybind = keybind;
            return this;
        }

        // Visual
        public IButtonBuilder WithIcon(Texture2D texture, Rectangle? sourceRect = null)
        {
            _iconTexture = texture;
            _iconSourceRect = sourceRect;
            return this;
        }

        public IButtonBuilder WithTint(Color normalColor, Color? toggledColor = null)
        {
            _normalColor = normalColor;
            _toggledColor = toggledColor;
            return this;
        }

        // Behavior
        public IButtonBuilder WithType(ButtonType type)
        {
            _buttonType = type;
            return this;
        }

        public IButtonBuilder WithCooldown(int milliseconds)
        {
            _cooldownMs = Math.Max(0, milliseconds);
            return this;
        }

        public IButtonBuilder WithVisibilityCondition(Func<bool> condition)
        {
            _visibilityCondition = condition;
            return this;
        }

        public IButtonBuilder WithEnabledCondition(Func<bool> condition)
        {
            _enabledCondition = condition;
            return this;
        }

        // Actions
        public IButtonBuilder OnPress(Action action)
        {
            _onPress = action;
            return this;
        }

        public IButtonBuilder OnHold(Action<float> action)
        {
            _onHold = action;
            return this;
        }

        public IButtonBuilder OnRelease(Action action)
        {
            _onRelease = action;
            return this;
        }

        public IButtonBuilder OnToggle(Action<bool> action)
        {
            _onToggle = action;
            return this;
        }

        // Finalization
        public bool Register()
        {
            try
            {
                ValidateConfiguration();

                lock (_lockObject)
                {
                    var builder = ModKeyButton.CreateBuilder(_uniqueId, _displayName)
                        .WithModId(_modId)
                        .WithCategory(_category)
                        .WithPriority(_priority)
                        .WithType(_buttonType)
                        .WithCooldown(_cooldownMs)
                        .WithTint(_normalColor, _toggledColor);

                    if (!string.IsNullOrEmpty(_description))
                        builder.WithDescription(_description);

                    if (!string.IsNullOrEmpty(_keybind))
                        builder.WithKeybind(_keybind);

                    if (_iconTexture != null)
                        builder.WithIcon(_iconTexture, _iconSourceRect);

                    if (_visibilityCondition != null)
                        builder.WithVisibilityCondition(_visibilityCondition);

                    if (_enabledCondition != null)
                        builder.WithEnabledCondition(_enabledCondition);

                    if (_onPress != null)
                        builder.OnPress(_onPress);

                    if (_onHold != null)
                        builder.OnHold(_onHold);

                    if (_onRelease != null)
                        builder.OnRelease(_onRelease);

                    if (_onToggle != null)
                        builder.OnToggle(_onToggle);

                    var button = builder.Build();
                    bool result = _registry.RegisterButton(button);

                    if (result)
                    {
                        _monitor.Log($"API Builder: Registered '{_uniqueId}' ({_displayName})", LogLevel.Debug);
                        _buttonManager?.RefreshButtons();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"API Builder: Registration failed for '{_uniqueId}' - {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        private void ValidateConfiguration()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_displayName))
                errors.Add("DisplayName is required");

            bool hasAction = _onPress != null || _onToggle != null || _onHold != null;
            if (!hasAction)
            {
                _monitor.Log($"Warning: Button '{_uniqueId}' has no action defined", LogLevel.Warn);
            }

            // Type-specific validation
            switch (_buttonType)
            {
                case ButtonType.Toggle when _onToggle == null && _onPress == null:
                    errors.Add("Toggle button requires OnToggle() or OnPress()");
                    break;
                case ButtonType.Hold when _onHold == null:
                    errors.Add("Hold button requires OnHold()");
                    break;
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Button validation failed for '{_uniqueId}': {string.Join(", ", errors)}");
            }
        }
    }
}