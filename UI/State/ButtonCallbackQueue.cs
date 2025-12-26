using StardewModdingAPI;
using System;

namespace AddonsMobile.UI.State
{
    /// <summary>
    /// State machine untuk button callback dengan delay execution.
    /// Mencegah race condition antara menu closing dan callback execution.
    /// </summary>
    public sealed class ButtonCallbackQueue
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTANTS
        // ═══════════════════════════════════════════════════════════════════════════

        private const int DEFAULT_DELAY_TICKS = 2; // ~32ms
        private const int MAX_DELAY_TICKS = 10;    // ~160ms max

        // ═══════════════════════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly IMonitor _monitor;
        private string? _pendingButtonId;
        private int _delayRemaining;
        private readonly int _delayTicks;

        // ═══════════════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Apakah ada callback yang pending</summary>
        public bool HasPending => _pendingButtonId != null;

        /// <summary>Button ID yang sedang pending</summary>
        public string? PendingButtonId => _pendingButtonId;

        /// <summary>Sisa delay dalam ticks</summary>
        public int RemainingDelay => _delayRemaining;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        public ButtonCallbackQueue(IMonitor monitor, int delayTicks = DEFAULT_DELAY_TICKS)
        {
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _delayTicks = Math.Clamp(delayTicks, 1, MAX_DELAY_TICKS);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enqueue button callback untuk eksekusi delayed.
        /// </summary>
        /// <param name="buttonId">Unique ID button</param>
        /// <returns>True jika berhasil enqueue, false jika sudah ada pending</returns>
        public bool Enqueue(string buttonId)
        {
            if (string.IsNullOrWhiteSpace(buttonId))
            {
                _monitor.Log("Cannot enqueue empty button ID", LogLevel.Warn);
                return false;
            }

            if (HasPending)
            {
                _monitor.Log($"Callback queue busy, replacing '{_pendingButtonId}' with '{buttonId}'", LogLevel.Debug);
            }

            _pendingButtonId = buttonId;
            _delayRemaining = _delayTicks;

            _monitor.Log($"Enqueued callback: '{buttonId}' (delay: {_delayTicks} ticks)", LogLevel.Trace);
            return true;
        }

        /// <summary>
        /// Update queue dan cek apakah callback siap dieksekusi.
        /// </summary>
        /// <returns>Button ID jika siap, null jika masih delay atau tidak ada pending</returns>
        public string? Update()
        {
            if (!HasPending)
                return null;

            // Countdown
            if (_delayRemaining > 0)
            {
                _delayRemaining--;
                return null;
            }

            // Ready to execute
            string buttonId = _pendingButtonId!;
            _pendingButtonId = null;
            _delayRemaining = 0;

            _monitor.Log($"Callback ready: '{buttonId}'", LogLevel.Trace);
            return buttonId;
        }

        /// <summary>
        /// Cancel pending callback.
        /// </summary>
        /// <returns>True jika ada yang di-cancel</returns>
        public bool Cancel()
        {
            if (!HasPending)
                return false;

            string cancelledId = _pendingButtonId!;
            _pendingButtonId = null;
            _delayRemaining = 0;

            _monitor.Log($"Cancelled callback: '{cancelledId}'", LogLevel.Debug);
            return true;
        }

        /// <summary>
        /// Reset queue ke state awal.
        /// </summary>
        public void Reset()
        {
            _pendingButtonId = null;
            _delayRemaining = 0;
        }
    }
}