using AddonsMobile.Config;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace AddonsMobile.UI.Handlers
{
    public class InputHandler
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;

        // State
        private DateTime _lastKeyDownTime;
        private bool _isMoveKeyboard = false;
        private bool _isHeldDown = false;

        private const int START_DRAG_TIME = 150;

        // Events
        public event Action? OnFabTapped;
        public event Action<Vector2>? OnDragUpdate;
        public event Action? OnDragEnd;
        public event Action<string>? OnButtonPressed;
        public event Action? OnOutsideTapped;

        // Properties
        public bool IsHeldDown => _isHeldDown;
        public bool IsDragging => _isMoveKeyboard;

        public InputHandler(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;
        }

        public void ResetState()
        {
            _isHeldDown = false;
            _isMoveKeyboard = false;
            Game1.freezeControls = false;
        }

        public void OnFabPressed(Vector2 position)
        {
            _lastKeyDownTime = DateTime.Now;
            _isHeldDown = true;
            _isMoveKeyboard = false;

            // ★ LANGSUNG freeze saat press - seperti VirtualKeyboard
            Game1.freezeControls = true;

            _monitor.Log("FAB pressed", LogLevel.Debug);
        }

        // ★ Update posisi - TIDAK cek release di sini
        public void UpdatePosition(Vector2 cursorPosition)
        {
            if (!_isHeldDown) return;
            if (!_config.EnableDragging) return;

            var offset = DateTime.Now - _lastKeyDownTime;

            // ★ DEBUG: Log setiap 100ms
            _monitor.Log($"[HOLD] Time={offset.TotalMilliseconds:F0}ms", LogLevel.Debug);

            if (offset.TotalMilliseconds >= START_DRAG_TIME)
            {
                if (!_isMoveKeyboard)
                {
                    _isMoveKeyboard = true;
                    Game1.freezeControls = true;
                    Game1.playSound("dwop");
                    _monitor.Log("FAB drag started", LogLevel.Debug);
                }

                OnDragUpdate?.Invoke(cursorPosition);
            }
        }

        // ★ Dipanggil dari ButtonReleased event
        public void OnFabReleased()
        {
            if (!_isHeldDown) return;

            var holdTime = DateTime.Now - _lastKeyDownTime;
            _monitor.Log($"FAB released after {holdTime.TotalMilliseconds:F0}ms - IsDrag={_isMoveKeyboard}", LogLevel.Debug);

            if (!_isMoveKeyboard)
            {
                OnFabTapped?.Invoke();
                _monitor.Log("FAB tapped", LogLevel.Debug);
            }
            else
            {
                OnDragEnd?.Invoke();
                _monitor.Log("FAB drag ended", LogLevel.Debug);
            }

            _isMoveKeyboard = false;
            _isHeldDown = false;
            Game1.freezeControls = false;
        }

        public void OnMenuButtonPressed(string buttonId)
        {
            OnButtonPressed?.Invoke(buttonId);
        }

        public void OnOutsidePressed()
        {
            OnOutsideTapped?.Invoke();
        }
    }
}