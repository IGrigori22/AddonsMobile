namespace AddonsMobile.Framework.Conflicts
{
    /// <summary>
    /// Severity level untuk konflik.
    /// </summary>
    public enum ConflictSeverity
    {
        /// <summary>Info saja, tidak perlu action</summary>
        Info = 0,

        /// <summary>Warning, mungkin perlu perhatian</summary>
        Warning = 1,

        /// <summary>Error, harus diselesaikan</summary>
        Error = 2,

        /// <summary>Critical, bisa menyebabkan crash</summary>
        Critical = 3
    }

    /// <summary>
    /// Tipe konflik yang terdeteksi.
    /// </summary>
    public enum ConflictType
    {
        /// <summary>Duplikasi keybind original</summary>
        DuplicateKeybind,

        /// <summary>Duplikasi display name</summary>
        DuplicateName,

        /// <summary>Duplikasi unique ID</summary>
        DuplicateId,

        /// <summary>Prioritas bertabrakan</summary>
        PriorityConflict,

        /// <summary>Kategori tidak konsisten</summary>
        CategoryMismatch
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CONFLICT INFO MODELS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Data konflik yang terdeteksi.
    /// </summary>
    public class ConflictInfo
    {
        /// <summary>Tipe konflik</summary>
        public ConflictType Type { get; set; }

        /// <summary>Severity level</summary>
        public ConflictSeverity Severity { get; set; }

        /// <summary>Deskripsi konflik</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Button-button yang terlibat dalam konflik</summary>
        public List<ModKeyButton> ConflictingButtons { get; set; } = new();

        /// <summary>Suggestion untuk menyelesaikan konflik</summary>
        public List<string> Suggestions { get; set; } = new();

        /// <summary>Apakah konflik bisa di-auto-resolve</summary>
        public bool CanAutoResolve { get; set; }

        /// <summary>Timestamp deteksi</summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Mendapatkan summary text untuk logging.
        /// </summary>
        public string GetSummary()
        {
            string buttonList = string.Join(", ", ConflictingButtons.Select(b => $"{b.DisplayName} ({b.ModId})"));
            return $"[{Severity}] {Type}: {Description} | Affected: {buttonList}";
        }
    }

    /// <summary>
    /// Hasil dari conflict detection.
    /// </summary>
    public class ConflictDetectionResult
    {
        /// <summary>Semua konflik yang ditemukan</summary>
        public List<ConflictInfo> Conflicts { get; set; } = new();

        /// <summary>Waktu deteksi</summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Total button yang di-scan</summary>
        public int TotalButtonsScanned { get; set; }

        /// <summary>Apakah ada konflik critical</summary>
        public bool HasCriticalConflicts => Conflicts.Any(c => c.Severity == ConflictSeverity.Critical);

        /// <summary>Apakah ada konflik error</summary>
        public bool HasErrors => Conflicts.Any(c => c.Severity >= ConflictSeverity.Error);

        /// <summary>Apakah ada konflik sama sekali</summary>
        public bool HasAnyConflicts => Conflicts.Count > 0;

        /// <summary>
        /// Mendapatkan konflik berdasarkan severity.
        /// </summary>
        public IEnumerable<ConflictInfo> GetBySeverity(ConflictSeverity severity)
        {
            return Conflicts.Where(c => c.Severity == severity);
        }

        /// <summary>
        /// Mendapatkan konflik berdasarkan type.
        /// </summary>
        public IEnumerable<ConflictInfo> GetByType(ConflictType type)
        {
            return Conflicts.Where(c => c.Type == type);
        }
    }

    /// <summary>
    /// Hasil dari conflict resolution.
    /// </summary>
    public class ConflictResolutionResult
    {
        /// <summary>Konflik yang berhasil diresolve</summary>
        public List<ConflictInfo> ResolvedConflicts { get; set; } = new();

        /// <summary>Konflik yang gagal diresolve</summary>
        public List<ConflictInfo> FailedConflicts { get; set; } = new();

        /// <summary>Actions yang diambil untuk resolve</summary>
        public List<string> ActionsToken { get; set; } = new();

        /// <summary>Apakah semua berhasil diresolve</summary>
        public bool IsFullyResolved => FailedConflicts.Count == 0;

        /// <summary>Success rate percentage</summary>
        public double SuccessRate
        {
            get
            {
                int total = ResolvedConflicts.Count + FailedConflicts.Count;
                return total == 0 ? 100.0 : (ResolvedConflicts.Count / (double)total) * 100.0;
            }
        }
    }
}