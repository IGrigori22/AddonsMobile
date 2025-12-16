using Microsoft.Xna.Framework;

namespace AddonsMobile.Config
{
    public sealed class ModConfig
    {
        // ════════════════════════════════════════════════════════════════
        // POSITION
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Posisi X default dalam persentase (0-100)
        /// </summary>
        public float ButtonPositionX { get; set; } = 95f;

        /// <summary>
        /// Posisi Y default dalam persentase (0-100)
        /// </summary>
        public float ButtonPositionY { get; set; } = 50f;

        // ════════════════════════════════════════════════════════════════
        // SAFE AREA (untuk notch dan navigation bar)
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Margin atas untuk safe area (pixels)
        /// Berguna untuk menghindari notch
        /// </summary>
        public int SafeAreaTop { get; set; } = 0;

        /// <summary>
        /// Margin bawah untuk safe area (pixels)
        /// Berguna untuk menghindari navigation bar
        /// </summary>
        public int SafeAreaBottom { get; set; } = 0;

        /// <summary>
        /// Margin kiri untuk safe area (pixels)
        /// </summary>
        public int SafeAreaLeft { get; set; } = 0;

        /// <summary>
        /// Margin kanan untuk safe area (pixels)
        /// </summary>
        public int SafeAreaRight { get; set; } = 0;

        // ════════════════════════════════════════════════════════════════
        // DRAG SETTINGS
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan fitur drag
        /// </summary>
        public bool EnableDragging { get; set; } = true;

        /// <summary>
        /// Jarak minimum sebelum drag dimulai (pixels)
        /// </summary>
        public float DragThreshold { get; set; } = 15f;

        /// <summary>
        /// Tampilkan indikator saat dragging
        /// </summary>
        public bool ShowDragIndicator { get; set; } = true;

        /// <summary>
        /// Vibrasi saat interaksi (Android)
        /// </summary>
        public bool HapticFeedback { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // SIZE
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Ukuran FAB (pixels)
        /// </summary>
        public int ButtonSize { get; set; } = 64;

        /// <summary>
        /// Ukuran button di menu (pixels)
        /// </summary>
        public int MenuButtonSize { get; set; } = 56;

        /// <summary>
        /// Jarak antar button (pixels)
        /// </summary>
        public int ButtonSpacing { get; set; } = 8;

        /// <summary>
        /// Maksimum button per baris
        /// </summary>
        public int MaxButtonsPerRow { get; set; } = 6;

        /// <summary>
        /// Padding dalam menu bar (pixels)
        /// </summary>
        public int MenuBarPadding { get; set; } = 12;

        // ════════════════════════════════════════════════════════════════
        // APPEARANCE
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Opacity button (0.0 - 1.0)
        /// </summary>
        public float ButtonOpacity { get; set; } = 1f;

        /// <summary>
        /// Tampilkan label di bawah button
        /// </summary>
        public bool ShowButtonLabels { get; set; } = false;

        /// <summary>
        /// Style background FAB
        /// </summary>
        public FabBackgroundStyle FabBackground { get; set; } = FabBackgroundStyle.CircleDark;

        /// <summary>
        /// Tampilkan shadow di bawah FAB
        /// </summary>
        public bool ShowFabShadow { get; set; } = true;

        /// <summary>
        /// Ubah warna saat dragging
        /// </summary>
        public bool ShowDragColor { get; set; } = true;

        /// <summary>
        /// Tampilkan badge jumlah button di FAB
        /// </summary>
        public bool ShowButtonCountBadge { get; set; } = true;

        /// <summary>
        /// Tampilkan tooltip saat button di-hover (atau long press)
        /// </summary>
        public bool ShowTooltips { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // BEHAVIOR
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sembunyikan otomatis saat event/cutscene
        /// </summary>
        public bool AutoHideInEvents { get; set; } = true;

        /// <summary>
        /// Durasi animasi (ms)
        /// </summary>
        public int AnimationDuration { get; set; } = 200;

        /// <summary>
        /// Auto-collapse menu setelah button di-press
        /// </summary>
        public bool AutoCollapseAfterPress { get; set; } = false;

        /// <summary>
        /// Delay sebelum auto-collapse (ms)
        /// </summary>
        public int AutoCollapseDelay { get; set; } = 1500;

        /// <summary>
        /// Aktifkan double-tap untuk action khusus
        /// </summary>
        public bool EnableDoubleTap { get; set; } = true;

        /// <summary>
        /// Action untuk double-tap pada FAB
        /// </summary>
        public DoubleTapAction FabDoubleTapAction { get; set; } = DoubleTapAction.ToggleAllButtons;

        /// <summary>
        /// Action untuk long-press pada FAB
        /// </summary>
        public LongPressAction FabLongPressAction { get; set; } = LongPressAction.ResetPosition;

        // ════════════════════════════════════════════════════════════════
        // CATEGORY FILTER
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan tab kategori
        /// </summary>
        public bool EnableCategoryTabs { get; set; } = false;

        /// <summary>
        /// Kategori default yang ditampilkan (null = semua)
        /// </summary>
        public string DefaultCategory { get; set; } = null;

        /// <summary>
        /// Sembunyikan kategori kosong
        /// </summary>
        public bool HideEmptyCategories { get; set; } = true;

        // ════════════════════════════════════════════════════════════════
        // DEBUG
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aktifkan logging verbose
        /// </summary>
        public bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Tampilkan debug bounds
        /// </summary>
        public bool ShowDebugBounds { get; set; } = false;

        // ════════════════════════════════════════════════════════════════
        // COMPUTED PROPERTIES
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Total safe area Rectangle
        /// </summary>
        public Rectangle GetSafeAreaMargins()
        {
            return new Rectangle(SafeAreaLeft, SafeAreaTop, SafeAreaRight, SafeAreaBottom);
        }

        /// <summary>
        /// Validasi dan clamp values
        /// </summary>
        public void Validate()
        {
            ButtonPositionX = Math.Clamp(ButtonPositionX, 5f, 95f);
            ButtonPositionY = Math.Clamp(ButtonPositionY, 5f, 95f);

            ButtonSize = Math.Clamp(ButtonSize, 40, 100);
            MenuButtonSize = Math.Clamp(MenuButtonSize, 30, 80);
            ButtonSpacing = Math.Clamp(ButtonSpacing, 4, 20);
            MaxButtonsPerRow = Math.Clamp(MaxButtonsPerRow, 3, 10);
            MenuBarPadding = Math.Clamp(MenuBarPadding, 4, 24);

            ButtonOpacity = Math.Clamp(ButtonOpacity, 0.3f, 1f);
            AnimationDuration = Math.Clamp(AnimationDuration, 100, 500);
            AutoCollapseDelay = Math.Clamp(AutoCollapseDelay, 500, 5000);

            DragThreshold = Math.Clamp(DragThreshold, 5f, 50f);

            SafeAreaTop = Math.Max(0, SafeAreaTop);
            SafeAreaBottom = Math.Max(0, SafeAreaBottom);
            SafeAreaLeft = Math.Max(0, SafeAreaLeft);
            SafeAreaRight = Math.Max(0, SafeAreaRight);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // ENUMS
    // ════════════════════════════════════════════════════════════════════

    public enum FabBackgroundStyle
    {
        None,
        CircleDark,
        CircleLight,
        RoundedSquare,
        Wood,
        Stone,
        Metal,
        StardewStyle,
        GradientBlue,
        GradientGreen,
        GradientSunset
    }

    public enum DoubleTapAction
    {
        /// <summary>Tidak ada action</summary>
        None,

        /// <summary>Toggle visibility semua button</summary>
        ToggleAllButtons,

        /// <summary>Buka config menu</summary>
        OpenConfig,

        /// <summary>Cycle ke kategori berikutnya</summary>
        NextCategory,

        /// <summary>Collapse menu dan sembunyikan FAB</summary>
        HideFab
    }

    public enum LongPressAction
    {
        /// <summary>Tidak ada action</summary>
        None,

        /// <summary>Reset posisi ke default</summary>
        ResetPosition,

        /// <summary>Buka config menu</summary>
        OpenConfig,

        /// <summary>Toggle drag mode</summary>
        ToggleDragMode,

        /// <summary>Sembunyikan FAB</summary>
        HideFab
    }
}