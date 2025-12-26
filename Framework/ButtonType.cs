namespace AddonsMobile.Framework
{
    /// <summary>
    /// Tipe behavior untuk button interaction.
    /// </summary>
    public enum ButtonType
    {
        /// <summary>
        /// Trigger sekali saat tap/press.
        /// Action dijalankan sekali kemudian selesai.
        /// </summary>
        Momentary = 0,

        /// <summary>
        /// Toggle on/off state.
        /// Setiap press akan flip state antara on dan off.
        /// </summary>
        Toggle = 1,

        /// <summary>
        /// Aktif selama di-hold/tekan.
        /// Action dijalankan continuous selama button ditekan.
        /// </summary>
        Hold = 2
    }
}