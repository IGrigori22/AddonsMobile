using StardewModdingAPI;
using AddonsMobile.API;
using System;

namespace AddonsMobile.Framework.Validation
{
    /// <summary>
    /// Validator untuk button registration.
    /// Memisahkan validation logic dari registry.
    /// </summary>
    internal static class ButtonValidator
    {
        /// <summary>
        /// Validasi button sebelum registration.
        /// </summary>
        /// <returns>Tuple (isValid, errorMessage)</returns>
        public static (bool isValid, string errorMessage) Validate(ModKeyButton button)
        {
            if (button == null)
                return (false, "Button is null");

            // Validasi UniqueId
            if (string.IsNullOrWhiteSpace(button.UniqueId))
                return (false, "UniqueId cannot be empty");

            if (button.UniqueId.Length > 100)
                return (false, $"UniqueId too long (max 100 chars): '{button.UniqueId}'");

            // Validasi ModId
            if (string.IsNullOrWhiteSpace(button.ModId))
                return (false, $"ModId cannot be empty for button '{button.UniqueId}'");

            // Validasi DisplayName
            if (string.IsNullOrWhiteSpace(button.DisplayName))
                return (false, $"DisplayName cannot be empty for button '{button.UniqueId}'");

            // Validasi Action
            if (!button.HasAnyAction)
                return (false, $"Button '{button.UniqueId}' has no action defined");

            // Validasi Type-specific
            if (button.Type == ButtonType.Hold)
            {
                if (button.OnHold == null)
                    return (false, $"Hold button '{button.UniqueId}' must have OnHold action");
            }
            else if (button.Type == ButtonType.Toggle)
            {
                if (button.OnPress == null)
                    return (false, $"Toggle button '{button.UniqueId}' must have OnPress action");
            }

            // Validasi Priority range
            if (button.Priority < 0 || button.Priority > 1000)
                return (false, $"Priority must be between 0-1000 for button '{button.UniqueId}'");

            return (true, null);
        }

        /// <summary>
        /// Log validation error dengan format yang konsisten.
        /// </summary>
        public static void LogValidationError(IMonitor monitor, ModKeyButton button, string errorMessage)
        {
            string buttonInfo = button != null
                ? $"'{button.DisplayName}' ({button.UniqueId})"
                : "(unknown)";

            monitor.Log($"✗ Button validation failed for {buttonInfo}: {errorMessage}", LogLevel.Error);
        }
    }
}