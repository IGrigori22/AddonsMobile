using AddonsMobile.Framework.Data;

namespace AddonsMobile.Framework.Events
{
    /// <summary>
    /// Event arguments untuk button registry events.
    /// Memisahkan event args dari registry untuk better organization.
    /// </summary>
    public class ButtonEventArgs
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // REGISTRATION EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Event args untuk button registration/update.
        /// </summary>
        public class ButtonRegisteredEventArgs : EventArgs
        {
            /// <summary>Button yang didaftarkan</summary>
            public ModKeyButton Button { get; }

            /// <summary>True jika ini adalah update dari button yang sudah ada</summary>
            public bool IsUpdate { get; }

            /// <summary>Timestamp kapan button didaftarkan</summary>
            public DateTime Timestamp { get; }

            public ButtonRegisteredEventArgs(ModKeyButton button, bool isUpdate)
            {
                Button = button ?? throw new ArgumentNullException(nameof(button));
                IsUpdate = isUpdate;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Event args untuk button unregistration.
        /// </summary>
        public class ButtonUnregisteredEventArgs : EventArgs
        {
            /// <summary>Unique ID button yang dihapus</summary>
            public string UniqueId { get; }

            /// <summary>Mod ID pemilik button</summary>
            public string ModId { get; }

            /// <summary>Timestamp kapan button dihapus</summary>
            public DateTime Timestamp { get; }

            public ButtonUnregisteredEventArgs(string uniqueId, string modId)
            {
                UniqueId = uniqueId ?? throw new ArgumentNullException(nameof(uniqueId));
                ModId = modId ?? throw new ArgumentNullException(nameof(modId));
                Timestamp = DateTime.UtcNow;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // INTERACTION EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Event args untuk button trigger/press.
        /// </summary>
        public class ButtonTriggeredEventArgs : EventArgs
        {
            /// <summary>Button yang di-trigger</summary>
            public ModKeyButton Button { get; }

            /// <summary>True jika trigger dari code (bukan user interaction)</summary>
            public bool WasProgrammatic { get; }

            /// <summary>Timestamp kapan button di-trigger</summary>
            public DateTime Timestamp { get; }

            /// <summary>Apakah trigger berhasil</summary>
            public bool Success { get; set; }

            public ButtonTriggeredEventArgs(ModKeyButton button, bool wasProgrammatic)
            {
                Button = button ?? throw new ArgumentNullException(nameof(button));
                WasProgrammatic = wasProgrammatic;
                Timestamp = DateTime.UtcNow;
                Success = true;
            }
        }

        /// <summary>
        /// Event args untuk toggle state change.
        /// </summary>
        public class ButtonToggledEventArgs : EventArgs
        {
            /// <summary>Button yang berubah state</summary>
            public ModKeyButton Button { get; }

            /// <summary>State baru (on/off)</summary>
            public bool NewState { get; }

            /// <summary>State sebelumnya</summary>
            public bool OldState { get; }

            /// <summary>Timestamp kapan toggle terjadi</summary>
            public DateTime Timestamp { get; }

            public ButtonToggledEventArgs(ModKeyButton button, bool newState, bool oldState)
            {
                Button = button ?? throw new ArgumentNullException(nameof(button));
                NewState = newState;
                OldState = oldState;
                Timestamp = DateTime.UtcNow;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STATE CHANGE EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Event args untuk registry state change.
        /// </summary>
        public class RegistryChangedEventArgs : EventArgs
        {
            /// <summary>Tipe perubahan</summary>
            public RegistryChangeType ChangeType { get; }

            /// <summary>Jumlah button setelah perubahan</summary>
            public int TotalButtons { get; }

            /// <summary>Jumlah mod yang terdaftar</summary>
            public int TotalMods { get; }

            /// <summary>Timestamp perubahan</summary>
            public DateTime Timestamp { get; }

            public RegistryChangedEventArgs(RegistryChangeType changeType, int totalButtons, int totalMods)
            {
                ChangeType = changeType;
                TotalButtons = totalButtons;
                TotalMods = totalMods;
                Timestamp = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Tipe perubahan registry
        /// </summary>
        public enum RegistryChangeType
        {
            ButtonAdded,
            ButtonUpdated,
            ButtonRemoved,
            ModUnregistered,
            StateReset
        }
    }
}