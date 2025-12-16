using AddonsMobile.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AddonsMobile.API
{
    public interface IMobileAddonsAPI
    {
        string Version { get; }
        bool IsMobilePlatform { get; }

        bool RegisterButton(
            string uniqueId, string modId, string displayName, string description,
            KeyCategory category, Action onPressed,
            Texture2D iconTexture = null, Rectangle? iconSourceRect = null,
            int priority = 0, string originalKeybind = null);

        IButtonBuilder CreateButton(string uniqueId, string modId);
        bool UnregisterButton(string uniqueId);
        void UnregisterAllFromMod(string modId);
        bool SetButtonEnabled(string uniqueId, bool enabled);
        bool TriggerButton(string uniqueId);
        int GetRegisteredButtonCount();
        bool IsButtonRegistered(string uniqueId);
        void RefreshUI();
        void SetVisible(bool visible);
    }

    public interface IButtonBuilder
    {
        IButtonBuilder WithDisplayName(string name);
        IButtonBuilder WithDescription(string description);
        IButtonBuilder WithCategory(KeyCategory category);
        IButtonBuilder WithIcon(Texture2D texture, Rectangle? sourceRect = null);
        IButtonBuilder WithTintColor(Color color);
        IButtonBuilder WithPriority(int priority);
        IButtonBuilder WithCooldown(int milliseconds);
        IButtonBuilder WithVisibilityCondition(Func<bool> condition);
        IButtonBuilder WithOriginalKeybind(string keybind);
        IButtonBuilder OnPressed(Action action);
        IButtonBuilder OnHeld(Action action);
        bool Register();
    }
}