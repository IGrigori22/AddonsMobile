namespace AddonsMobile.Config
{
    /// <summary>
    /// Style background untuk FAB (Floating Action Button).
    /// </summary>
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

    /// <summary>
    /// Action yang dijalankan saat double-tap pada FAB.
    /// </summary>
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

    /// <summary>
    /// Action yang dijalankan saat long-press pada FAB.
    /// </summary>
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

    /// <summary>
    /// Posisi anchor untuk menu bar.
    /// </summary>
    public enum MenuAnchorPosition
    {
        Auto,
        Left,
        Right,
        Top,
        Bottom
    }
}