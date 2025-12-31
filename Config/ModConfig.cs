using AddonsMobile.Internal.Core;
using Microsoft.Xna.Framework;

namespace AddonsMobile.Config
{
    /// <summary>
    /// Konfigurasi utama untuk AddonsMobile mod.
    /// Semua nilai akan divalidasi dan di-clamp ke range yang valid.
    /// </summary>
    public sealed class ModConfig : IValidatable
    {
        // ════════════════════════════════════════════════════════════════
        // CONSTANTS (Validation Ranges)
        // ════════════════════════════════════════════════════════════════

        private static class Limits
        {
            // Position
            public const float MinPosition = 5f;
            public const float MaxPosition = 95f;

            // Size
            public const int MinButtonSize = 40;
            public const int MaxButtonSize = 100;
            public const int MinMenuButtonSize = 30;
            public const int MaxMenuButtonSize = 80;

            // Spacing & Padding
            public const int MinSpacing = 4;
            public const int MaxSpacing = 24;
            public const int MinPadding = 4;
            public const int MaxPadding = 24;

            // Opacity
            public const float MinOpacity = 0.3f;
            public const float MaxOpacity = 1.0f;

            // Animation
            public const int MinAnimationDuration = 50;
            public const int MaxAnimationDuration = 500;
            public const int MinAutoCollapseDelay = 500;
            public const int MaxAutoCollapseDelay = 5000;

            // Drag
            public const float MinDragThreshold = 5f;
            public const float MaxDragThreshold = 50f;

            // Layout
            public const int MinButtonsPerRow = 3;
            public const int MaxButtonsPerRow = 10;
        }

        // ════════════════════════════════════════════════════════════════
        // GENERAL SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Apakah mod aktif.
        /// </summary>
        public bool ModEnabled { get; set; } = true;

        /// <summary>
        /// Tampilkan FAB dan buttons di layar.
        /// </summary>
        public bool ShowButtons { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // FAB POSITION (Floating Action Button)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Posisi X FAB dalam persentase layar (5-95).
        /// </summary>
        public float FabPositionX { get; set; } = 95f;

        /// <summary>
        /// Posisi Y FAB dalam persentase layar (5-95).
        /// </summary>
        public float FabPositionY { get; set; } = 50f;

        // ════════════════════════════════════════════════════════════════
        // FAB APPEARANCE
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Ukuran FAB dalam pixels (40-100).
        /// </summary>
        public int FabSize { get; set; } = 64;

        /// <summary>
        /// Opacity FAB (0.3-1.0).
        /// </summary>
        public float FabOpacity { get; set; } = 1.0f;

        /// <summary>
        /// Style background FAB.
        /// </summary>
        public FabBackgroundStyle FabBackground { get; set; } = FabBackgroundStyle.CircleDark;

        /// <summary>
        /// Tampilkan shadow di bawah FAB.
        /// </summary>
        public bool FabShowShadow { get; set; } = true;

        /// <summary>
        /// Tampilkan badge jumlah button di FAB.
        /// </summary>
        public bool FabShowBadge { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // MENU BAR SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Ukuran button dalam menu bar (30-80 pixels).
        /// </summary>
        public int MenuButtonSize { get; set; } = 56;

        /// <summary>
        /// Jarak antar button dalam menu bar (4-24 pixels).
        /// </summary>
        public int MenuButtonSpacing { get; set; } = 8;

        /// <summary>
        /// Padding dalam menu bar (4-24 pixels).
        /// </summary>
        public int MenuPadding { get; set; } = 12;

        /// <summary>
        /// Opacity button dalam menu bar (0.3-1.0).
        /// </summary>
        public float MenuButtonOpacity { get; set; } = 1.0f;

        /// <summary>
        /// Maksimum button per baris (3-10).
        /// </summary>
        public int MaxButtonsPerRow { get; set; } = 6;

        /// <summary>
        /// Tampilkan label di bawah button.
        /// </summary>
        public bool ShowButtonLabels { get; set; } = false;

        /// <summary>
        /// Tampilkan tooltip saat hover/long press.
        /// </summary>
        public bool ShowTooltips { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // DRAG SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan fitur drag FAB.
        /// </summary>
        public bool DragEnabled { get; set; } = true;

        /// <summary>
        /// Jarak minimum sebelum drag dimulai (5-50 pixels).
        /// </summary>
        public float DragThreshold { get; set; } = 15f;

        /// <summary>
        /// Tampilkan indikator visual saat dragging.
        /// </summary>
        public bool DragShowIndicator { get; set; } = true;

        /// <summary>
        /// Ubah warna FAB saat dragging.
        /// </summary>
        public bool DragShowColor { get; set; } = true;

        /// <summary>
        /// Haptic feedback saat interaksi (Android).
        /// </summary>
        public bool HapticFeedback { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // GESTURE SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan double-tap gesture.
        /// </summary>
        public bool DoubleTapEnabled { get; set; } = true;

        /// <summary>
        /// Action untuk double-tap pada FAB.
        /// </summary>
        public DoubleTapAction DoubleTapAction { get; set; } = DoubleTapAction.ToggleAllButtons;

        /// <summary>
        /// Action untuk long-press pada FAB.
        /// </summary>
        public LongPressAction LongPressAction { get; set; } = LongPressAction.ResetPosition;

        // ════════════════════════════════════════════════════════════════
        // ANIMATION SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Durasi animasi dalam milliseconds (50-500).
        /// </summary>
        public int AnimationDuration { get; set; } = 200;

        /// <summary>
        /// Auto-collapse menu setelah button di-press.
        /// </summary>
        public bool AutoCollapse { get; set; } = false;

        /// <summary>
        /// Delay sebelum auto-collapse dalam milliseconds (500-5000).
        /// </summary>
        public int AutoCollapseDelay { get; set; } = 1500;

        // ════════════════════════════════════════════════════════════════
        // BEHAVIOR SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sembunyikan otomatis saat event/cutscene.
        /// </summary>
        public bool AutoHideInEvents { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // CATEGORY SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan tab kategori.
        /// </summary>
        public bool CategoriesEnabled { get; set; } = false;

        /// <summary>
        /// Kategori default yang ditampilkan (kosong = semua).
        /// </summary>
        public string DefaultCategory { get; set; } = string.Empty;

        /// <summary>
        /// Sembunyikan kategori kosong.
        /// </summary>
        public bool HideEmptyCategories { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // SAFE AREA (untuk notch dan navigation bar)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Margin atas untuk safe area dalam pixels (untuk notch).
        /// </summary>
        public int SafeAreaTop { get; set; } = 0;

        /// <summary>
        /// Margin bawah untuk safe area dalam pixels (untuk navigation bar).
        /// </summary>
        public int SafeAreaBottom { get; set; } = 0;

        /// <summary>
        /// Margin kiri untuk safe area dalam pixels.
        /// </summary>
        public int SafeAreaLeft { get; set; } = 0;

        /// <summary>
        /// Margin kanan untuk safe area dalam pixels.
        /// </summary>
        public int SafeAreaRight { get; set; } = 0;

        // ════════════════════════════════════════════════════════════════
        // HIDDEN BUTTONS (User Preferences)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// List UniqueId button yang disembunyikan oleh user.
        /// </summary>
        public List<string> HiddenButtonIds { get; set; } = new();

        // ════════════════════════════════════════════════════════════════
        // DEBUG SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan debug mode (extra logging + visual bounds).
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Tampilkan button IDs pada hover (untuk development).
        /// </summary>
        public bool DebugShowButtonIds { get; set; } = false;

        /// <summary>
        /// Tampilkan debug bounds/hitbox.
        /// </summary>
        public bool DebugShowBounds { get; set; } = false;

        /// <summary>
        /// Verbose logging (sangat detail).
        /// </summary>
        public bool DebugVerboseLogging { get; set; } = false;

        // ════════════════════════════════════════════════════════════════
        // COMPUTED PROPERTIES
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Get safe area sebagai Rectangle (Left, Top, Right, Bottom).
        /// </summary>
        public Rectangle GetSafeAreaMargins()
        {
            return new Rectangle(SafeAreaLeft, SafeAreaTop, SafeAreaRight, SafeAreaBottom);
        }

        /// <summary>
        /// Cek apakah button dengan ID tertentu disembunyikan.
        /// </summary>
        public bool IsButtonHidden(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
                return false;

            return HiddenButtonIds?.Contains(buttonId) ?? false;
        }

        /// <summary>
        /// Sembunyikan button berdasarkan ID.
        /// </summary>
        public void HideButton(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
                return;

            HiddenButtonIds ??= new List<string>();

            if (!HiddenButtonIds.Contains(buttonId))
            {
                HiddenButtonIds.Add(buttonId);
            }
        }

        /// <summary>
        /// Tampilkan button berdasarkan ID.
        /// </summary>
        public void ShowButton(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
                return;

            HiddenButtonIds?.Remove(buttonId);
        }

        /// <summary>
        /// Toggle visibility button berdasarkan ID.
        /// </summary>
        public void ToggleButtonVisibility(string buttonId)
        {
            if (IsButtonHidden(buttonId))
                ShowButton(buttonId);
            else
                HideButton(buttonId);
        }

        // ════════════════════════════════════════════════════════════════
        // VALIDATION (IValidatable Implementation)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Validasi dan clamp semua nilai ke range yang valid.
        /// </summary>
        public void Validate()
        {
            // Position
            FabPositionX = Math.Clamp(FabPositionX, Limits.MinPosition, Limits.MaxPosition);
            FabPositionY = Math.Clamp(FabPositionY, Limits.MinPosition, Limits.MaxPosition);

            // FAB Size & Appearance
            FabSize = Math.Clamp(FabSize, Limits.MinButtonSize, Limits.MaxButtonSize);
            FabOpacity = Math.Clamp(FabOpacity, Limits.MinOpacity, Limits.MaxOpacity);

            // Menu Bar
            MenuButtonSize = Math.Clamp(MenuButtonSize, Limits.MinMenuButtonSize, Limits.MaxMenuButtonSize);
            MenuButtonSpacing = Math.Clamp(MenuButtonSpacing, Limits.MinSpacing, Limits.MaxSpacing);
            MenuPadding = Math.Clamp(MenuPadding, Limits.MinPadding, Limits.MaxPadding);
            MenuButtonOpacity = Math.Clamp(MenuButtonOpacity, Limits.MinOpacity, Limits.MaxOpacity);
            MaxButtonsPerRow = Math.Clamp(MaxButtonsPerRow, Limits.MinButtonsPerRow, Limits.MaxButtonsPerRow);

            // Animation
            AnimationDuration = Math.Clamp(AnimationDuration, Limits.MinAnimationDuration, Limits.MaxAnimationDuration);
            AutoCollapseDelay = Math.Clamp(AutoCollapseDelay, Limits.MinAutoCollapseDelay, Limits.MaxAutoCollapseDelay);

            // Drag
            DragThreshold = Math.Clamp(DragThreshold, Limits.MinDragThreshold, Limits.MaxDragThreshold);

            // Safe Area (tidak boleh negatif)
            SafeAreaTop = Math.Max(0, SafeAreaTop);
            SafeAreaBottom = Math.Max(0, SafeAreaBottom);
            SafeAreaLeft = Math.Max(0, SafeAreaLeft);
            SafeAreaRight = Math.Max(0, SafeAreaRight);

            // Nullable/Reference types
            HiddenButtonIds ??= new List<string>();
            DefaultCategory ??= string.Empty;
        }

        // ════════════════════════════════════════════════════════════════
        // RESET METHODS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Reset posisi FAB ke default.
        /// </summary>
        public void ResetPosition()
        {
            FabPositionX = 95f;
            FabPositionY = 50f;
        }

        /// <summary>
        /// Reset semua appearance settings ke default.
        /// </summary>
        public void ResetAppearance()
        {
            FabSize = 64;
            FabOpacity = 1.0f;
            FabBackground = FabBackgroundStyle.CircleDark;
            FabShowShadow = true;
            FabShowBadge = true;

            MenuButtonSize = 56;
            MenuButtonSpacing = 8;
            MenuPadding = 12;
            MenuButtonOpacity = 1.0f;
            MaxButtonsPerRow = 6;
            ShowButtonLabels = false;
            ShowTooltips = true;
        }

        /// <summary>
        /// Reset semua behavior settings ke default.
        /// </summary>
        public void ResetBehavior()
        {
            DragEnabled = true;
            DragThreshold = 15f;
            DragShowIndicator = true;
            DragShowColor = true;
            HapticFeedback = true;

            DoubleTapEnabled = true;
            DoubleTapAction = DoubleTapAction.ToggleAllButtons;
            LongPressAction = LongPressAction.ResetPosition;

            AnimationDuration = 200;
            AutoCollapse = false;
            AutoCollapseDelay = 1500;
            AutoHideInEvents = true;
        }

        /// <summary>
        /// Reset safe area ke default (0).
        /// </summary>
        public void ResetSafeArea()
        {
            SafeAreaTop = 0;
            SafeAreaBottom = 0;
            SafeAreaLeft = 0;
            SafeAreaRight = 0;
        }

        /// <summary>
        /// Reset hidden buttons (tampilkan semua).
        /// </summary>
        public void ResetHiddenButtons()
        {
            HiddenButtonIds?.Clear();
        }

        /// <summary>
        /// Reset semua settings ke default.
        /// </summary>
        public void ResetAll()
        {
            // General
            ModEnabled = true;
            ShowButtons = true;

            // Position & Appearance
            ResetPosition();
            ResetAppearance();

            // Behavior
            ResetBehavior();

            // Categories
            CategoriesEnabled = false;
            DefaultCategory = string.Empty;
            HideEmptyCategories = true;

            // Safe Area
            ResetSafeArea();

            // Hidden Buttons
            ResetHiddenButtons();

            // Debug
            DebugMode = false;
            DebugShowButtonIds = false;
            DebugShowBounds = false;
            DebugVerboseLogging = false;
        }
    }
}