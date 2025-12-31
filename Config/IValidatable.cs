namespace AddonsMobile.Config
{
    /// <summary>
    /// Interface untuk object yang bisa memvalidasi dirinya sendiri.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validasi dan perbaiki nilai yang tidak valid.
        /// </summary>
        void Validate();
    }
}