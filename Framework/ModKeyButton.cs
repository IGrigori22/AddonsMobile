using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Tipe behavior button
    /// </summary>
    public enum ButtonType
    {
        /// <summary>Trigger sekali saat tap</summary>
        Momentary,

        /// <summary>Toggle on/off</summary>
        Toggle,

        /// <summary>Aktif selama di-hold</summary>
        Hold
    }

    /// <summary>
    /// Data model untuk button yang didaftarkan
    /// </summary>
    public sealed class ModKeyButton
    {
        // ═══════════════════════════════════════════════════════════
        // IDENTITY
        // ═══════════════════════════════════════════════════════════

        /// <summary>ID unik untuk button ini</summary>
        public string UniqueId { get; set; } = string.Empty;

        /// <summary>ID mod yang mendaftarkan button</summary>
        public string ModId { get; set; } = string.Empty;

        /// <summary>Nama yang ditampilkan</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Deskripsi untuk tooltip</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Kategori button</summary>
        public KeyCategory Category { get; set; } = KeyCategory.Miscellaneous;

        // ═══════════════════════════════════════════════════════════
        // VISUAL
        // ═══════════════════════════════════════════════════════════

        /// <summary>Texture icon (null = gunakan default)</summary>
        public Texture2D IconTexture { get; set; }

        /// <summary>Source rectangle untuk spritesheet</summary>
        public Rectangle? IconSourceRect { get; set; }

        /// <summary>Warna tint untuk icon</summary>
        public Color TintColor { get; set; } = Color.White;

        /// <summary>Warna tint saat toggle ON</summary>
        public Color ToggledTintColor { get; set; } = Color.LightGreen;

        // ═══════════════════════════════════════════════════════════
        // BEHAVIOR
        // ═══════════════════════════════════════════════════════════

        /// <summary>Tipe button</summary>
        public ButtonType Type { get; set; } = ButtonType.Momentary;

        /// <summary>Priority (higher = more important)</summary>
        public int Priority { get; set; } = 0;

        /// <summary>Apakah button enabled</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Cooldown antara press (ms)</summary>
        public int PressCooldown { get; set; } = 250;

        /// <summary>Keybind original (untuk dokumentasi)</summary>
        public string OriginalKeybind { get; set; }

        // ═══════════════════════════════════════════════════════════
        // CONDITIONS & ACTIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>Kondisi untuk menampilkan button</summary>
        public Func<bool> VisibilityCondition { get; set; }

        /// <summary>Kondisi untuk enable button</summary>
        public Func<bool> EnabledCondition { get; set; }

        /// <summary>Action saat button di-tap</summary>
        public Action OnPressed { get; set; }

        /// <summary>Action saat button di-hold (setiap frame)</summary>
        public Action OnHeld { get; set; }

        /// <summary>Action saat button di-release (setelah hold)</summary>
        public Action OnReleased { get; set; }

        /// <summary>Action saat toggle state berubah</summary>
        public Action<bool> OnToggled { get; set; }

        // ═══════════════════════════════════════════════════════════
        // RUNTIME STATE (Internal)
        // ═══════════════════════════════════════════════════════════

        /// <summary>Waktu terakhir di-press</summary>
        internal DateTime LastPressed { get; set; } = DateTime.MinValue;

        /// <summary>Toggle state (untuk ButtonType.Toggle)</summary>
        internal bool IsToggled { get; set; } = false;

        /// <summary>Apakah sedang di-hold</summary>
        internal bool IsBeingHeld { get; set; } = false;

        /// <summary>Durasi hold dalam detik</summary>
        internal float HoldDuration { get; set; } = 0f;

        // ═══════════════════════════════════════════════════════════
        // COMPUTED PROPERTIES
        // ═══════════════════════════════════════════════════════════

        /// <summary>Apakah bisa di-press (cooldown check)</summary>
        public bool CanPress()
        {
            if (!IsEnabled) return false;
            if (EnabledCondition != null && !EnabledCondition()) return false;
            return (DateTime.Now - LastPressed).TotalMilliseconds >= PressCooldown;
        }

        /// <summary>Apakah harus ditampilkan</summary>
        public bool ShouldShow()
        {
            if (!IsEnabled) return false;
            return VisibilityCondition?.Invoke() ?? true;
        }

        /// <summary>Warna aktif berdasarkan state</summary>
        public Color GetCurrentTintColor()
        {
            if (Type == ButtonType.Toggle && IsToggled)
                return ToggledTintColor;
            return TintColor;
        }

        /// <summary>Apakah ada action yang bisa dijalankan</summary>
        public bool HasAnyAction => OnPressed != null || OnHeld != null || OnToggled != null;

        // ═══════════════════════════════════════════════════════════
        // METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Execute press action berdasarkan button type
        /// </summary>
        /// <returns>True jika berhasil execute</returns>
        public bool ExecutePress()
        {
            if (!CanPress()) return false;

            LastPressed = DateTime.Now;

            switch (Type)
            {
                case ButtonType.Momentary:
                    OnPressed?.Invoke();
                    break;

                case ButtonType.Toggle:
                    IsToggled = !IsToggled;
                    OnToggled?.Invoke(IsToggled);
                    OnPressed?.Invoke(); // Tetap panggil OnPressed juga
                    break;

                case ButtonType.Hold:
                    IsBeingHeld = true;
                    HoldDuration = 0f;
                    OnPressed?.Invoke();
                    break;
            }

            return true;
        }

        /// <summary>
        /// Execute hold action (dipanggil setiap frame saat di-hold)
        /// </summary>
        public void ExecuteHold(float deltaTime)
        {
            if (Type != ButtonType.Hold || !IsBeingHeld) return;

            HoldDuration += deltaTime;
            OnHeld?.Invoke();
        }

        /// <summary>
        /// Execute release action
        /// </summary>
        public void ExecuteRelease()
        {
            if (Type != ButtonType.Hold || !IsBeingHeld) return;

            IsBeingHeld = false;
            OnReleased?.Invoke();
            HoldDuration = 0f;
        }

        /// <summary>
        /// Set toggle state secara programmatic
        /// </summary>
        public void SetToggleState(bool toggled, bool triggerCallback = true)
        {
            if (Type != ButtonType.Toggle) return;

            bool changed = IsToggled != toggled;
            IsToggled = toggled;

            if (changed && triggerCallback)
                OnToggled?.Invoke(toggled);
        }

        /// <summary>
        /// Reset semua runtime state
        /// </summary>
        public void ResetState()
        {
            LastPressed = DateTime.MinValue;
            IsToggled = false;
            IsBeingHeld = false;
            HoldDuration = 0f;
        }

        /// <summary>
        /// Clone button (tanpa actions)
        /// </summary>
        public ModKeyButton CloneWithoutActions()
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
                Type = Type,
                Priority = Priority,
                IsEnabled = IsEnabled,
                PressCooldown = PressCooldown,
                OriginalKeybind = OriginalKeybind
            };
        }
    }
}