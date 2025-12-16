namespace AddonsMobile.UI.Data
{
    /// <summary>
    /// Data untuk menyimpan posisi FAB yang sudah di-drag
    /// </summary>
    public class ButtonPositionData
    {
        public float PositionXPercent { get; set; } = -1f;
        public float PositionYPercent { get; set; } = -1f;
        public long LastSaved { get; set; } = 0;

        public bool HasSavedPosition => PositionXPercent >= 0 && PositionYPercent >= 0;
    }
}