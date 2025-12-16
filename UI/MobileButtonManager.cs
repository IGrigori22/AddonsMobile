using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.UI.Animation;
using AddonsMobile.UI.Components;
using AddonsMobile.UI.Data;
using AddonsMobile.UI.Handlers;
using AddonsMobile.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Mods;

namespace AddonsMobile.UI
{
    /// <summary>
    /// Main coordinator class untuk Mobile Button UI
    /// </summary>
    public sealed class MobileButtonManager : IDisposable
    {
        // ============================================
        // Dependencies
        // ============================================
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly KeyRegistry _registry;
        private readonly ModConfig _config;

        // ============================================
        // Components
        // ============================================
        private readonly PositionManager _positionManager;
        private readonly AnimationController _animator;
        private readonly InputHandler _inputHandler;
        private readonly DrawingHelpers _drawingHelpers;
        private readonly TextureManager _textureManager;
        private readonly FabRenderer _fabRenderer;
        private readonly MenuBarRenderer _menuBarRenderer;

        // ============================================
        // State
        // ============================================
        private bool _isVisible = false;
        private bool _wasInteractable = false;
        private List<ModKeyButton> _currentButtons = new();
        private readonly List<Rectangle> _menuButtonBounds = new();
        private Rectangle _menuBarBounds;

        // ============================================
        // Pending Callback System (★ BARU)
        // ============================================
        private string? _pendingButtonId;
        private int _pendingCallbackDelay;
        private const int CALLBACK_DELAY_TICKS = 2;  // ~32ms delay

        // ============================================
        // Viewport tracking
        // ============================================
        private int _lastViewportWidth;
        private int _lastViewportHeight;

        // ============================================
        // Constants
        // ============================================
        private const int MENU_BAR_PADDING = 12;
        private const int MENU_BUTTON_MARGIN = 8;

        // ============================================
        // Constructor
        // ============================================
        public MobileButtonManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _registry = ModEntry.Registry;
            _config = ModEntry.Config;

            // Initialize components
            _positionManager = new PositionManager(helper, monitor, _config);
            _animator = new AnimationController();
            _inputHandler = new InputHandler(_helper, _monitor, _config);
            _drawingHelpers = new DrawingHelpers();
            _textureManager = new TextureManager(helper, monitor);

            _textureManager.LoadAllTextures();

            _fabRenderer = new FabRenderer(_config, _drawingHelpers, _textureManager);
            _menuBarRenderer = new MenuBarRenderer(_config, _drawingHelpers, _textureManager);

            // Wire up events
            _inputHandler.OnFabTapped += HandleFabTapped;
            _inputHandler.OnDragUpdate += HandleDragUpdate;
            _inputHandler.OnDragEnd += HandleDragEnd;
            _inputHandler.OnButtonPressed += HandleButtonPressed;
            _inputHandler.OnOutsideTapped += HandleOutsideTapped;

            // Register SMAPI events
            _helper.Events.Input.ButtonPressed += OnButtonPressed;
            _helper.Events.Input.ButtonReleased += OnButtonReleased;
            _helper.Events.Input.CursorMoved += OnCursorMoved;
            _helper.Events.Display.RenderedStep += OnRenderedStep;
            _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            _helper.Events.Display.WindowResized += OnWindowResized;

            _lastViewportWidth = Game1.uiViewport.Width;
            _lastViewportHeight = Game1.uiViewport.Height;

            _monitor.Log("MobileButtonManager initialized", LogLevel.Trace);
        }

        // ============================================
        // Public Methods
        // ============================================

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            
            if (visible)
            {
                _positionManager.UpdatePosition();
                UpdateMenuBarBounds();
                RefreshButtons();
                _wasInteractable = CanInteract();
            }
            else
            {
                _animator.Reset();
                _inputHandler.ResetState();
                _wasInteractable = false;
                CancelPendingCallback();  // ★ TAMBAH
            }

            _monitor.Log($"FAB visibility: {visible}", LogLevel.Trace);
        }

        public void UpdatePosition()
        {
            _positionManager.UpdatePosition();
            UpdateMenuBarBounds();

            _lastViewportWidth = Game1.uiViewport.Width;
            _lastViewportHeight = Game1.uiViewport.Height;
        }

        public void RefreshButtons()
        {
            _currentButtons = _registry.GetAllButtons().ToList();
            UpdateMenuBarBounds();

            if (_config.VerboseLogging)
            {
                _monitor.Log($"Refreshed: {_currentButtons.Count} buttons", LogLevel.Debug);
            }
        }

        public void ResetPosition()
        {
            _positionManager.ResetPosition();
            UpdateMenuBarBounds();
        }

        public int ButtonCount => _currentButtons.Count;
        public bool IsVisible => _isVisible;
        public bool IsExpanded => _animator.IsExpanded;

        // ============================================
        // CanInteract
        // ============================================

        private bool CanInteract()
        {
            if (!_isVisible) return false;
            if (!Context.IsWorldReady) return false;
            if (Game1.activeClickableMenu != null) return false;
            if (Game1.eventUp && _config.AutoHideInEvents) return false;
            return true;
        }

        private void ResetInteractionState()
        {
            _inputHandler.ResetState();
            _animator.SetExpanded(false);
            CancelPendingCallback();  // ★ TAMBAH
        }

        // ============================================
        // SMAPI Input Event Handlers (★ UPDATED)
        // ============================================

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!CanInteract()) return;
            if (e.Button != SButton.MouseLeft) return;

            Vector2 screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            int x = (int)screenPixels.X;
            int y = (int)screenPixels.Y;

            var fabBounds = _positionManager.FabBounds;

            // Check FAB
            if (fabBounds.Contains(x, y))
            {
                _inputHandler.OnFabPressed(screenPixels);
                return;
            }

            // Check menu buttons
            if (_animator.IsExpanded && _animator.ExpandProgress > 0.5f)
            {
                for (int i = 0; i < _menuButtonBounds.Count && i < _currentButtons.Count; i++)
                {
                    if (_menuButtonBounds[i].Contains(x, y))
                    {
                        var button = _currentButtons[i];
                        if (button.CanPress())
                        {
                            _inputHandler.OnMenuButtonPressed(button.UniqueId);
                        }
                        return;
                    }
                }

                if (_menuBarBounds.Contains(x, y))
                {
                    _helper.Input.Suppress(e.Button);
                    return;
                }

                // Outside tap // ★ SUPPRESS
                _inputHandler.OnOutsidePressed();
            }
        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft) return;

            if (_inputHandler.IsHeldDown)
            {
                _inputHandler.OnFabReleased();
            }
        }

        private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
            if (!_inputHandler.IsHeldDown) return;

            Vector2 newPos = e.NewPosition.GetScaledScreenPixels();
            _inputHandler.UpdatePosition(newPos);
        }

        // ============================================
        // Internal Event Handlers (★ UPDATED)
        // ============================================

        private void HandleFabTapped()
        {
            bool newExpanded = !_animator.IsExpanded;
            _animator.SetExpanded(newExpanded);
            _animator.StartGearRotation();

            if (newExpanded)
            {
                RefreshButtons();
                CalculateTargetMenuBarWidth();
            }

            Game1.playSound("smallSelect");
        }

        private void HandleDragUpdate(Vector2 screenPos)
        {
            _positionManager.UpdateFromDrag(screenPos);
            UpdateMenuBarBounds();

            if (_animator.IsExpanded)
            {
                _animator.SetExpanded(false);
            }
        }

        private void HandleDragEnd()
        {
            _positionManager.SavePositionData();
            Game1.playSound("stoneStep");
        }

        /// <summary>
        /// ★ UPDATED: Queue button callback instead of immediate execution
        /// </summary>
        private void HandleButtonPressed(string buttonId)
        {
            // Queue untuk eksekusi di frame berikutnya
            _pendingButtonId = buttonId;
            _pendingCallbackDelay = CALLBACK_DELAY_TICKS;
            
            if (_config.VerboseLogging)
            {
                _monitor.Log($"Button '{buttonId}' queued (delay: {CALLBACK_DELAY_TICKS} ticks)", LogLevel.Debug);
            }
        }

        private void HandleOutsideTapped()
        {
            _animator.SetExpanded(false);
            Game1.playSound("smallSelect");
        }

        // ============================================
        // Pending Callback System (★ BARU)
        // ============================================

        /// <summary>
        /// Process pending button callback setelah delay
        /// </summary>
        private void ProcessPendingCallback()
        {
            if (_pendingButtonId == null)
                return;

            // Countdown
            if (_pendingCallbackDelay > 0)
            {
                _pendingCallbackDelay--;
                return;
            }

            // Delay selesai - execute!
            string buttonId = _pendingButtonId;
            _pendingButtonId = null;

            // Pastikan masih bisa interact
            if (!CanInteract())
            {
                if (_config.VerboseLogging)
                {
                    _monitor.Log($"Skipped callback for '{buttonId}' - cannot interact", LogLevel.Debug);
                }
                return;
            }

            if (_config.VerboseLogging)
            {
                _monitor.Log($"Executing callback for '{buttonId}'", LogLevel.Debug);
            }

            // Trigger button
            _registry.TriggerButton(buttonId);
        }

        /// <summary>
        /// Cancel pending callback
        /// </summary>
        private void CancelPendingCallback()
        {
            if (_pendingButtonId != null)
            {
                if (_config.VerboseLogging)
                {
                    _monitor.Log($"Cancelled callback for '{_pendingButtonId}'", LogLevel.Debug);
                }
                _pendingButtonId = null;
                _pendingCallbackDelay = 0;
            }
        }

        // ============================================
        // Update Loop (★ UPDATED)
        // ============================================

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!_isVisible || !Context.IsWorldReady) return;

            // ★ Process pending callback FIRST
            ProcessPendingCallback();

            // Polling: Cek mouse state
            if (_inputHandler.IsHeldDown)
            {
                var cursorPos = _helper.Input.GetCursorPosition().GetScaledScreenPixels();
                _inputHandler.UpdatePosition(cursorPos);
            }

            // Reset state jika tidak bisa interact
            if (!CanInteract() && _inputHandler.IsHeldDown)
            {
                _inputHandler.ResetState();
                CancelPendingCallback();  // ★ TAMBAH
            }

            // Viewport check
            if (e.IsMultipleOf(30))
            {
                if (Game1.uiViewport.Width != _lastViewportWidth ||
                    Game1.uiViewport.Height != _lastViewportHeight)
                {
                    UpdatePosition();
                }
            }

            // Update animations
            float deltaTime = (float)(Game1.currentGameTime?.ElapsedGameTime.TotalSeconds ?? 0.016f);
            _animator.Update(deltaTime, _inputHandler.IsDragging);

            // Auto hide in events
            if (_config.AutoHideInEvents && Game1.eventUp)
            {
                _animator.Reset();
                _inputHandler.ResetState();
                CancelPendingCallback();  // ★ TAMBAH
            }
        }

        private void OnWindowResized(object? sender, WindowResizedEventArgs e)
        {
            UpdatePosition();

            if (_config.VerboseLogging)
            {
                _monitor.Log($"Window resized to {e.NewSize.X}x{e.NewSize.Y}", LogLevel.Debug);
            }
        }

        // ============================================
        // Rendering (tidak berubah)
        // ============================================

        private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
        {
            switch (e.Step)
            {
                case RenderSteps.Overlays:
                    RenderFabAndMenu(e.SpriteBatch);
                    break;
                default:
                    break;
            }
        }

        private void RenderFabAndMenu(SpriteBatch spriteBatch)
        {
            if (!_isVisible || !Context.IsWorldReady) return;
            if (Game1.activeClickableMenu != null) return;
            if (Game1.eventUp && _config.AutoHideInEvents) return;

            _drawingHelpers.EnsureTexture();
            if (!_drawingHelpers.IsReady) return;

            if (_animator.MenuBarOpacity > 0.01f && _currentButtons.Count > 0)
            {
                _menuBarRenderer.Draw(
                    spriteBatch,
                    _menuBarBounds,
                    _positionManager.Position,
                    _animator.MenuBarWidth,
                    _animator.MenuBarOpacity,
                    _animator.ExpandProgress,
                    _menuButtonBounds,
                    _currentButtons
                );
            }

            _fabRenderer.Draw(
                spriteBatch,
                _positionManager.FabBounds,
                _animator.GearRotation,
                _inputHandler.IsDragging,
                _inputHandler.IsHeldDown,
                _animator.IsExpanded,
                _registry.Count
            );

            if (_inputHandler.IsDragging && _config.ShowDragIndicator)
            {
                _fabRenderer.DrawDragIndicator(spriteBatch, _positionManager.FabBounds);
            }

            if (_config.VerboseLogging)
            {
                DrawDebugInfo(spriteBatch);
            }
        }

        private void DrawDebugInfo(SpriteBatch b)
        {
            if (!_drawingHelpers.IsReady) return;

            Color fabDebugColor = _inputHandler.IsDragging ? Color.Green * 0.8f : Color.Red * 0.8f;
            var fabBounds = _positionManager.FabBounds;

            _drawingHelpers.DrawRectangle(b, new Rectangle(fabBounds.X, fabBounds.Y, fabBounds.Width, 2), fabDebugColor);
            _drawingHelpers.DrawRectangle(b, new Rectangle(fabBounds.X, fabBounds.Bottom - 2, fabBounds.Width, 2), fabDebugColor);
            _drawingHelpers.DrawRectangle(b, new Rectangle(fabBounds.X, fabBounds.Y, 2, fabBounds.Height), fabDebugColor);
            _drawingHelpers.DrawRectangle(b, new Rectangle(fabBounds.Right - 2, fabBounds.Y, 2, fabBounds.Height), fabDebugColor);

            if (_animator.IsExpanded && _menuBarBounds.Width > 0)
            {
                Color menuDebugColor = Color.Cyan * 0.8f;
                _drawingHelpers.DrawRectangle(b, new Rectangle(_menuBarBounds.X, _menuBarBounds.Y, _menuBarBounds.Width, 2), menuDebugColor);
                _drawingHelpers.DrawRectangle(b, new Rectangle(_menuBarBounds.X, _menuBarBounds.Bottom - 2, _menuBarBounds.Width, 2), menuDebugColor);
                _drawingHelpers.DrawRectangle(b, new Rectangle(_menuBarBounds.X, _menuBarBounds.Y, 2, _menuBarBounds.Height), menuDebugColor);
                _drawingHelpers.DrawRectangle(b, new Rectangle(_menuBarBounds.Right - 2, _menuBarBounds.Y, 2, _menuBarBounds.Height), menuDebugColor);

                foreach (var btnBounds in _menuButtonBounds)
                {
                    Color btnDebugColor = Color.Yellow * 0.8f;
                    _drawingHelpers.DrawRectangle(b, new Rectangle(btnBounds.X, btnBounds.Y, btnBounds.Width, 1), btnDebugColor);
                    _drawingHelpers.DrawRectangle(b, new Rectangle(btnBounds.X, btnBounds.Bottom - 1, btnBounds.Width, 1), btnDebugColor);
                    _drawingHelpers.DrawRectangle(b, new Rectangle(btnBounds.X, btnBounds.Y, 1, btnBounds.Height), btnDebugColor);
                    _drawingHelpers.DrawRectangle(b, new Rectangle(btnBounds.Right - 1, btnBounds.Y, 1, btnBounds.Height), btnDebugColor);
                }
            }
        }

        // ============================================
        // Helper Methods (tidak berubah)
        // ============================================

        private void CalculateTargetMenuBarWidth()
        {
            if (_currentButtons.Count == 0)
            {
                _animator.SetTargetMenuBarWidth(0f);
                return;
            }

            int buttonSize = _config.MenuButtonSize;
            int totalButtonsWidth = _currentButtons.Count * buttonSize;
            int totalMarginsWidth = (_currentButtons.Count - 1) * MENU_BUTTON_MARGIN;

            float targetWidth = totalButtonsWidth + totalMarginsWidth + (MENU_BAR_PADDING * 2);
            _animator.SetTargetMenuBarWidth(targetWidth);
        }

        private void UpdateMenuBarBounds()
        {
            _menuButtonBounds.Clear();

            if (_currentButtons.Count == 0)
            {
                _menuBarBounds = Rectangle.Empty;
                return;
            }

            int buttonSize = _config.MenuButtonSize;
            int menuBarHeight = buttonSize + (MENU_BAR_PADDING * 2);

            CalculateTargetMenuBarWidth();

            var position = _positionManager.Position;
            int screenWidth = Game1.uiViewport.Width;
            float targetWidth = _animator.TargetMenuBarWidth;

            int menuBarX;
            bool showOnLeft = (position.X + _config.ButtonSize + targetWidth + 15) > screenWidth;

            if (showOnLeft)
            {
                menuBarX = (int)position.X - (int)targetWidth - 15;
            }
            else
            {
                menuBarX = (int)position.X + _config.ButtonSize + 15;
            }

            int menuBarY = (int)position.Y + (_config.ButtonSize - menuBarHeight) / 2;

            menuBarX = Math.Max(PositionManager.SCREEN_EDGE_PADDING, menuBarX);
            menuBarY = MathHelper.Clamp(
                menuBarY,
                PositionManager.SCREEN_EDGE_PADDING,
                Game1.uiViewport.Height - menuBarHeight - PositionManager.SCREEN_EDGE_PADDING
            );

            _menuBarBounds = new Rectangle(
                menuBarX,
                menuBarY,
                (int)targetWidth,
                menuBarHeight
            );

            int buttonX = menuBarX + MENU_BAR_PADDING;
            int buttonY = menuBarY + MENU_BAR_PADDING;

            for (int i = 0; i < _currentButtons.Count; i++)
            {
                _menuButtonBounds.Add(new Rectangle(
                    buttonX + i * (buttonSize + MENU_BUTTON_MARGIN),
                    buttonY,
                    buttonSize,
                    buttonSize
                ));
            }
        }

        // ============================================
        // Dispose
        // ============================================

        public void Dispose()
        {
            _inputHandler.OnFabTapped -= HandleFabTapped;
            _inputHandler.OnDragUpdate -= HandleDragUpdate;
            _inputHandler.OnDragEnd -= HandleDragEnd;
            _inputHandler.OnButtonPressed -= HandleButtonPressed;
            _inputHandler.OnOutsideTapped -= HandleOutsideTapped;

            _helper.Events.Input.ButtonPressed -= OnButtonPressed;
            _helper.Events.Input.ButtonReleased -= OnButtonReleased;
            _helper.Events.Input.CursorMoved -= OnCursorMoved;
            _helper.Events.Display.RenderedStep -= OnRenderedStep;
            _helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            _helper.Events.Display.WindowResized -= OnWindowResized;

            _drawingHelpers.Dispose();

            _monitor.Log("MobileButtonManager disposed", LogLevel.Trace);
        }
    }
}