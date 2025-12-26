using AddonsMobile.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AddonsMobile.API
{
    /// <summary>
    /// Public API untuk mod lain mendaftarkan custom buttons ke AddonsMobile.
    /// Version: 1.0.0
    /// </summary>
    public interface IMobileAddonsAPI
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Versi API saat ini (Semantic Versioning).
        /// Format: "major.minor.patch"
        /// </summary>
        string ApiVersion { get; }

        /// <summary>
        /// Apakah game berjalan di platform mobile (Android).
        /// </summary>
        bool IsMobilePlatform { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // BUTTON REGISTRATION (Simple)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cara cepat register button sederhana (momentary type).
        /// Untuk konfigurasi advanced, gunakan CreateButton() builder.
        /// </summary>
        /// <param name="uniqueId">Unique ID (recommended: "YourModId.ButtonName")</param>
        /// <param name="modId">Your mod's unique ID</param>
        /// <param name="displayName">Nama yang ditampilkan di UI</param>
        /// <param name="onPress">Action saat button ditekan</param>
        /// <param name="category">Kategori button (default: Miscellaneous)</param>
        /// <returns>True jika berhasil register, false jika gagal</returns>
        bool RegisterSimpleButton(
            string uniqueId,
            string modId,
            string displayName,
            Action onPress,
            KeyCategory category = KeyCategory.Miscellaneous
        );

        // ═══════════════════════════════════════════════════════════════════════════
        // BUTTON REGISTRATION (Builder Pattern)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Membuat builder untuk konfigurasi button yang kompleks.
        /// Recommended untuk semua use case.
        /// </summary>
        /// <param name="uniqueId">Unique ID (recommended: "YourModId.ButtonName")</param>
        /// <param name="modId">Your mod's unique ID</param>
        /// <returns>Button builder instance</returns>
        /// <example>
        /// api.CreateButton("MyMod.OpenMenu", "MyMod")
        ///    .WithDisplayName("Open Menu")
        ///    .WithCategory(KeyCategory.Menu)
        ///    .OnPress(() => OpenMyMenu())
        ///    .Register();
        /// </example>
        IButtonBuilder CreateButton(string uniqueId, string modId);

        // ═══════════════════════════════════════════════════════════════════════════
        // BUTTON MANAGEMENT
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unregister button berdasarkan unique ID.
        /// </summary>
        /// <returns>True jika berhasil, false jika button tidak ditemukan</returns>
        bool UnregisterButton(string uniqueId);

        /// <summary>
        /// Unregister semua button dari mod tertentu.
        /// Biasanya dipanggil saat mod di-unload.
        /// </summary>
        /// <param name="modId">Mod ID yang akan dihapus semua buttonnya</param>
        /// <returns>Jumlah button yang dihapus</returns>
        int UnregisterAllFromMod(string modId);

        /// <summary>
        /// Enable/disable button secara runtime.
        /// </summary>
        /// <returns>True jika berhasil, false jika button tidak ditemukan</returns>
        bool SetButtonEnabled(string uniqueId, bool enabled);

        /// <summary>
        /// Set toggle state untuk toggle button.
        /// </summary>
        /// <param name="uniqueId">Button ID</param>
        /// <param name="toggled">State baru (on/off)</param>
        /// <param name="invokeCallback">Apakah trigger OnToggle callback</param>
        /// <returns>True jika berhasil</returns>
        bool SetToggleState(string uniqueId, bool toggled, bool invokeCallback = false);

        // ═══════════════════════════════════════════════════════════════════════════
        // BUTTON TRIGGERING
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Trigger button secara programmatic (simulate press).
        /// </summary>
        /// <returns>True jika berhasil trigger, false jika gagal/cooldown</returns>
        bool TriggerButton(string uniqueId);

        // ═══════════════════════════════════════════════════════════════════════════
        // QUERIES
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cek apakah button dengan ID tertentu sudah terdaftar.
        /// </summary>
        bool IsButtonRegistered(string uniqueId);

        /// <summary>
        /// Mendapatkan jumlah total button yang terdaftar.
        /// </summary>
        int GetRegisteredButtonCount();

        /// <summary>
        /// Mendapatkan jumlah button yang terdaftar dari mod tertentu.
        /// </summary>
        int GetButtonCountForMod(string modId);

        // ═══════════════════════════════════════════════════════════════════════════
        // UI CONTROL
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Refresh UI untuk update tampilan button (setelah bulk changes).
        /// </summary>
        void RefreshUI();

        /// <summary>
        /// Show/hide semua button UI.
        /// </summary>
        void SetVisible(bool visible);

        /// <summary>
        /// Apakah button UI sedang visible.
        /// </summary>
        bool IsVisible { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENTS (Optional - for advanced mods)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Event dipanggil saat button berhasil didaftarkan.
        /// </summary>
        event EventHandler<ButtonRegisteredEventArgs> ButtonRegistered;

        /// <summary>
        /// Event dipanggil saat button di-unregister.
        /// </summary>
        event EventHandler<ButtonUnregisteredEventArgs> ButtonUnregistered;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BUTTON BUILDER INTERFACE
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Fluent builder interface untuk membuat button dengan konfigurasi lengkap.
    /// </summary>
    public interface IButtonBuilder
    {
        // ═══════════════════════════════════════════════════════════
        // BASIC PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>Set nama tampilan button</summary>
        IButtonBuilder WithDisplayName(string name);

        /// <summary>Set deskripsi untuk tooltip (opsional)</summary>
        IButtonBuilder WithDescription(string description);

        /// <summary>Set kategori button</summary>
        IButtonBuilder WithCategory(KeyCategory category);

        /// <summary>Set priority (0-1000, higher = more important)</summary>
        IButtonBuilder WithPriority(int priority);

        /// <summary>Set keybind original untuk dokumentasi</summary>
        IButtonBuilder WithKeybind(string keybind);

        // ═══════════════════════════════════════════════════════════
        // VISUAL CUSTOMIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>Set custom icon texture</summary>
        IButtonBuilder WithIcon(Texture2D texture, Rectangle? sourceRect = null);

        /// <summary>Set warna tint</summary>
        IButtonBuilder WithTint(Color normalColor, Color? toggledColor = null);

        // ═══════════════════════════════════════════════════════════
        // BEHAVIOR
        // ═══════════════════════════════════════════════════════════

        /// <summary>Set button type (Momentary/Toggle/Hold)</summary>
        IButtonBuilder WithType(ButtonType type);

        /// <summary>Set cooldown antara press (milliseconds)</summary>
        IButtonBuilder WithCooldown(int milliseconds);

        /// <summary>Set kondisi visibility (null = always visible)</summary>
        IButtonBuilder WithVisibilityCondition(Func<bool> condition);

        /// <summary>Set kondisi enabled (null = always enabled)</summary>
        IButtonBuilder WithEnabledCondition(Func<bool> condition);

        // ═══════════════════════════════════════════════════════════
        // ACTIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Set action saat button ditekan.
        /// - Momentary: Execute once
        /// - Toggle: Execute on every toggle
        /// - Hold: Execute on hold start
        /// </summary>
        IButtonBuilder OnPress(Action action);

        /// <summary>
        /// Set action saat button di-hold (ButtonType.Hold only).
        /// Parameter: deltaTime in seconds
        /// </summary>
        IButtonBuilder OnHold(Action<float> action);

        /// <summary>
        /// Set action saat button dilepas setelah hold (ButtonType.Hold only).
        /// </summary>
        IButtonBuilder OnRelease(Action action);

        /// <summary>
        /// Set action saat toggle state berubah (ButtonType.Toggle only).
        /// Parameter: newState (true = ON, false = OFF)
        /// </summary>
        IButtonBuilder OnToggle(Action<bool> action);

        // ═══════════════════════════════════════════════════════════
        // FINALIZATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Register button ke registry.
        /// Throws exception jika validation gagal.
        /// </summary>
        /// <returns>True jika berhasil</returns>
        bool Register();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // EVENT ARGS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Event args untuk button registration.
    /// </summary>
    public class ButtonRegisteredEventArgs : EventArgs
    {
        public string UniqueId { get; }
        public string ModId { get; }
        public string DisplayName { get; }
        public bool IsUpdate { get; }

        public ButtonRegisteredEventArgs(string uniqueId, string modId, string displayName, bool isUpdate)
        {
            UniqueId = uniqueId;
            ModId = modId;
            DisplayName = displayName;
            IsUpdate = isUpdate;
        }
    }

    /// <summary>
    /// Event args untuk button unregistration.
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
}