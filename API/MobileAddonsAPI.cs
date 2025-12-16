using AddonsMobile.API;
using AddonsMobile.Framework;
using AddonsMobile.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace AddonsMobile.API
{
    public sealed class MobileAddonsAPI : IMobileAddonsAPI
    {
        private readonly KeyRegistry _registry;
        private readonly MobileButtonManager _buttonManager;
        private readonly IMonitor _monitor;

        public string Version => "1.0.0";
        public bool IsMobilePlatform => Constants.TargetPlatform == GamePlatform.Android;

        public MobileAddonsAPI(KeyRegistry registry, MobileButtonManager buttonManager, IMonitor monitor)
        {
            _registry = registry;
            _buttonManager = buttonManager;
            _monitor = monitor;
        }

        public bool RegisterButton(
            string uniqueId, string modId, string displayName, string description,
            KeyCategory category, Action onPressed,
            Texture2D iconTexture = null, Rectangle? iconSourceRect = null,
            int priority = 0, string originalKeybind = null)
        {
            var button = new ModKeyButton
            {
                UniqueId = uniqueId,
                ModId = modId,
                DisplayName = displayName,
                Description = description,
                Category = category,
                OnPressed = onPressed,
                IconTexture = iconTexture,
                IconSourceRect = iconSourceRect,
                Priority = priority,
                OriginalKeybind = originalKeybind
            };

            return _registry.RegisterButton(button);
        }

        public IButtonBuilder CreateButton(string uniqueId, string modId)
        {
            return new ButtonBuilder(uniqueId, modId, _registry, _monitor);
        }

        public bool UnregisterButton(string uniqueId) => _registry.UnregisterButton(uniqueId);
        public void UnregisterAllFromMod(string modId) => _registry.UnregisterAllFromMod(modId);

        public bool SetButtonEnabled(string uniqueId, bool enabled)
        {
            var button = _registry.GetButton(uniqueId);
            if (button == null) return false;
            button.IsEnabled = enabled;
            return true;
        }

        public bool TriggerButton(string uniqueId) => _registry.TriggerButton(uniqueId);
        public int GetRegisteredButtonCount() => _registry.Count;
        public bool IsButtonRegistered(string uniqueId) => _registry.GetButton(uniqueId) != null;
        public void RefreshUI() => _buttonManager.RefreshButtons();
        public void SetVisible(bool visible) => _buttonManager.SetVisible(visible);
    }

    internal sealed class ButtonBuilder : IButtonBuilder
    {
        private readonly ModKeyButton _button;
        private readonly KeyRegistry _registry;
        private readonly IMonitor _monitor;

        public ButtonBuilder(string uniqueId, string modId, KeyRegistry registry, IMonitor monitor)
        {
            _button = new ModKeyButton { UniqueId = uniqueId, ModId = modId };
            _registry = registry;
            _monitor = monitor;
        }

        public IButtonBuilder WithDisplayName(string name) { _button.DisplayName = name; return this; }
        public IButtonBuilder WithDescription(string description) { _button.Description = description; return this; }
        public IButtonBuilder WithCategory(KeyCategory category) { _button.Category = category; return this; }
        public IButtonBuilder WithIcon(Texture2D texture, Rectangle? sourceRect = null)
        {
            _button.IconTexture = texture;
            _button.IconSourceRect = sourceRect;
            return this;
        }
        public IButtonBuilder WithTintColor(Color color) { _button.TintColor = color; return this; }
        public IButtonBuilder WithPriority(int priority) { _button.Priority = priority; return this; }
        public IButtonBuilder WithCooldown(int milliseconds) { _button.PressCooldown = milliseconds; return this; }
        public IButtonBuilder WithVisibilityCondition(Func<bool> condition) { _button.VisibilityCondition = condition; return this; }
        public IButtonBuilder WithOriginalKeybind(string keybind) { _button.OriginalKeybind = keybind; return this; }
        public IButtonBuilder OnPressed(Action action) { _button.OnPressed = action; return this; }
        public IButtonBuilder OnHeld(Action action) { _button.OnHeld = action; return this; }

        public bool Register()
        {
            if (string.IsNullOrEmpty(_button.DisplayName))
                _button.DisplayName = _button.UniqueId;

            return _registry.RegisterButton(_button);
        }
    }
}