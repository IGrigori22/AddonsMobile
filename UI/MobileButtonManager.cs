using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.UI.Animation;
using AddonsMobile.UI.Components;
using AddonsMobile.UI.Data;
using AddonsMobile.UI.Debug;
using AddonsMobile.UI.Handlers;
using AddonsMobile.UI.Layout;
using AddonsMobile.UI.Rendering;
using AddonsMobile.UI.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Mods;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using TileRectangle = xTile.Dimensions.Rectangle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddonsMobile.UI
{
    /// <summary>
    /// Main coordinator untuk Mobile Button UI system.
    /// Orchestrates antara components tapi tidak melakukan heavy lifting sendiri.
    /// </summary>
    public sealed class MobileButtonManager : IDisposable
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // DEPENDENCIES
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly KeyRegistry _registry;
        private readonly ModConfig _config;

        // ═══════════════════════════════════════════════════════════════════════════
        // COMPONENTS
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly PositionManager _positionManager;
        private readonly AnimationController _animator;
        private readonly InputHandler _inputHandler;
        private readonly DrawingHelpers _drawingHelpers;
        private readonly TextureManager _textureManager;
        private readonly FabRenderer _fabRenderer;
        private readonly MenuBarRenderer _menuBarRenderer;
        private readonly MenuBarLayoutCalculator _layoutCalculator;
        private readonly ButtonCallbackQueue _callbackQueue;
        private readonly DebugRenderer _debugRenderer;

        // ═══════════════════════════════════════════════════════════════════════════
        // STATE
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _isVisible;
        private List<ModKeyButton> _currentButtons;
        private MenuBarLayout _currentLayout;
        private ViewportState _viewportState;

        // ═══════════════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        public bool IsVisible => _isVisible;
        public bool IsExpanded => _animator.IsExpanded;
        public int ButtonCount => _currentButtons.Count;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        public MobileButtonManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _registry = ModEntry.Registry;
            _config = ModEntry.Config;

            // Initialize components
            _positionManager = new PositionManager(helper, monitor, _config);
            _animator = new AnimationController();
            _inputHandler = new InputHandler(helper, monitor, _config);
            _drawingHelpers = new DrawingHelpers();
            _textureManager = new TextureManager(helper, monitor);
            _layoutCalculator = new MenuBarLayoutCalculator(_config);
            _callbackQueue = new ButtonCallbackQueue(monitor, delayTicks: 2);
            _debugRenderer = new DebugRenderer(_drawingHelpers);

            _textureManager.LoadAllTextures();

            _fabRenderer = new FabRenderer(_config, _drawingHelpers, _textureManager);
            _menuBarRenderer = new MenuBarRenderer(_config, _drawingHelpers, _textureManager);

            // Initialize state
            _currentButtons = new List<ModKeyButton>();
            _currentLayout = new MenuBarLayout();
            _viewportState = new ViewportState(Game1.uiViewport);

            // Wire up events
            WireUpEvents();

            _monitor.Log("MobileButtonManager initialized", LogLevel.Trace);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENT WIRING
        // ═══════════════════════════════════════════════════════════════════════════

        private void WireUpEvents()
        {
            // Input handler events
            _inputHandler.OnFabTapped += HandleFabTapped;
            _inputHandler.OnDragUpdate += HandleDragUpdate;
            _inputHandler.OnDragEnd += HandleDragEnd;
            _inputHandler.OnButtonPressed += HandleButtonPressed;
            _inputHandler.OnOutsideTapped += HandleOutsideTapped;

            // SMAPI events
            _helper.Events.Input.ButtonPressed += OnButtonPressed;
            _helper.Events.Input.ButtonReleased += OnButtonReleased;
            _helper.Events.Input.CursorMoved += OnCursorMoved;
            _helper.Events.Display.RenderedStep += OnRenderedStep;
            _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            _helper.Events.Display.WindowResized += OnWindowResized;
        }

        private void UnwireEvents()
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
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═══════════════════════════════════════════════════════════════════════════

        public void SetVisible(bool visible)
        {
            _isVisible = visible;

            if (visible)
            {
                _positionManager.UpdatePosition();
                RefreshButtons();
                RecalculateLayout();
            }
            else
            {
                ResetAllState();
            }

            _monitor.Log($"Visibility changed: {visible}", LogLevel.Trace);
        }

        public void UpdatePosition()
        {
            _positionManager.UpdatePosition();
            RecalculateLayout();
            _viewportState.Update(Game1.uiViewport);
        }

        public void RefreshButtons()
        {
            _currentButtons = _registry.GetAllButtons().ToList();
            RecalculateLayout();

            if (_config.VerboseLogging)
            {
                _monitor.Log($"Refreshed: {_currentButtons.Count} button(s)", LogLevel.Debug);
            }
        }

        public void ResetPosition()
        {
            _positionManager.ResetPosition();
            RecalculateLayout();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SMAPI EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════════════════════

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!CanInteract() || e.Button != SButton.MouseLeft)
                return;

            Vector2 screenPos = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
            int x = (int)screenPos.X;
            int y = (int)screenPos.Y;

            // Check FAB
            if (_positionManager.FabBounds.Contains(x, y))
            {
                _inputHandler.OnFabPressed(screenPos);
                return;
            }

            // Check menu (if expanded)
            if (_animator.IsExpanded && _animator.ExpandProgress > 0.5f)
            {
                if (HandleMenuInteraction(x, y))
                    return;

                // Outside tap
                _inputHandler.OnOutsidePressed();
            }
        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft && _inputHandler.IsHeldDown)
            {
                _inputHandler.OnFabReleased();
            }
        }

        private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
            if (_inputHandler.IsHeldDown)
            {
                Vector2 newPos = e.NewPosition.GetScaledScreenPixels();
                _inputHandler.UpdatePosition(newPos);
            }
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!_isVisible || !Context.IsWorldReady)
                return;

            // Process pending callbacks FIRST
            ProcessCallbackQueue();

            // Update input polling
            UpdateInputPolling();

            // Check for viewport changes
            if (e.IsMultipleOf(30) && _viewportState.HasChanged(Game1.uiViewport))
            {
                UpdatePosition();
            }

            // Update animations
            UpdateAnimations();

            // Auto-hide in events
            HandleAutoHide();
        }

        private void OnWindowResized(object? sender, WindowResizedEventArgs e)
        {
            UpdatePosition();

            if (_config.VerboseLogging)
            {
                _monitor.Log($"Window resized: {e.NewSize.X}x{e.NewSize.Y}", LogLevel.Debug);
            }
        }

        private void OnRenderedStep(object? sender, RenderedStepEventArgs e)
        {
            if (e.Step == RenderSteps.Overlays)
            {
                RenderUI(e.SpriteBatch);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // INPUT HANDLERS
        // ═══════════════════════════════════════════════════════════════════════════

        private void HandleFabTapped()
        {
            bool newExpanded = !_animator.IsExpanded;
            _animator.SetExpanded(newExpanded);
            _animator.StartGearRotation();

            if (newExpanded)
            {
                RefreshButtons();
                float targetWidth = _layoutCalculator.CalculateTargetWidth(_currentButtons.Count);
                _animator.SetTargetMenuBarWidth(targetWidth);
            }

            Game1.playSound("smallSelect");
        }

        private void HandleDragUpdate(Vector2 screenPos)
        {
            _positionManager.UpdateFromDrag(screenPos);
            RecalculateLayout();

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

        private void HandleButtonPressed(string buttonId)
        {
            _callbackQueue.Enqueue(buttonId);
        }

        private void HandleOutsideTapped()
        {
            _animator.SetExpanded(false);
            Game1.playSound("smallSelect");
        }

        /// <summary>
        /// Handle interaction dengan menu bar dan buttons.
        /// </summary>
        /// <returns>True jika handled</returns>
        private bool HandleMenuInteraction(int x, int y)
        {
            // Check menu buttons
            for (int i = 0; i < _currentLayout.ButtonBounds.Count && i < _currentButtons.Count; i++)
            {
                if (_currentLayout.ButtonBounds[i].Contains(x, y))
                {
                    var button = _currentButtons[i];
                    if (button.CanPress())
                    {
                        _inputHandler.OnMenuButtonPressed(button.UniqueId, SButton.MouseLeft);
                        return true;
                    }
                }
            }

            // Check menu bar bounds
            if (_currentLayout.MenuBarBounds.Contains(x, y))
            {
                _helper.Input.Suppress(SButton.MouseLeft);
                return true;
            }

            return false;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // UPDATE LOGIC
        // ═══════════════════════════════════════════════════════════════════════════

        private void ProcessCallbackQueue()
        {
            string? readyButtonId = _callbackQueue.Update();

            if (readyButtonId != null)
            {
                if (CanInteract())
                {
                    _registry.TriggerButton(readyButtonId, isProgrammatic: false, logAction: _config.VerboseLogging);
                }
                else if (_config.VerboseLogging)
                {
                    _monitor.Log($"Skipped callback '{readyButtonId}' - cannot interact", LogLevel.Debug);
                }
            }
        }

        private void UpdateInputPolling()
        {
            if (_inputHandler.IsHeldDown)
            {
                var cursorPos = _helper.Input.GetCursorPosition().GetScaledScreenPixels();
                _inputHandler.UpdatePosition(cursorPos);
            }

            if (!CanInteract() && _inputHandler.IsHeldDown)
            {
                ResetAllState();
            }
        }

        private void UpdateAnimations()
        {
            float deltaTime = (float)(Game1.currentGameTime?.ElapsedGameTime.TotalSeconds ?? 0.016f);
            _animator.Update(deltaTime, _inputHandler.IsDragging);
        }

        private void HandleAutoHide()
        {
            if (_config.AutoHideInEvents && Game1.eventUp)
            {
                ResetAllState();
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // RENDERING
        // ═══════════════════════════════════════════════════════════════════════════

        private void RenderUI(SpriteBatch spriteBatch)
        {
            if (!ShouldRender())
                return;

            _drawingHelpers.EnsureTexture();
            if (!_drawingHelpers.IsReady)
                return;

            // Render menu bar
            if (_animator.MenuBarOpacity > 0.01f && _currentButtons.Count > 0)
            {
                RenderMenuBar(spriteBatch);
            }

            // Render FAB
            RenderFab(spriteBatch);

            // Render debug (if enabled)
            if (_config.VerboseLogging)
            {
                RenderDebugInfo(spriteBatch);
            }
        }

        private void RenderMenuBar(SpriteBatch b)
        {
            _menuBarRenderer.Draw(
                b,
                _currentLayout.MenuBarBounds,
                _positionManager.Position,
                _animator.MenuBarWidth,
                _animator.MenuBarOpacity,
                _animator.ExpandProgress,
                _currentLayout.ButtonBounds,
                _currentButtons
            );
        }

        private void RenderFab(SpriteBatch b)
        {
            _fabRenderer.Draw(
                b,
                _positionManager.FabBounds,
                _animator.GearRotation,
                _inputHandler.IsDragging,
                _inputHandler.IsHeldDown,
                _animator.IsExpanded,
                _registry.Count
            );

            if (_inputHandler.IsDragging && _config.ShowDragIndicator)
            {
                _fabRenderer.DrawDragIndicator(b, _positionManager.FabBounds);
            }
        }

        private void RenderDebugInfo(SpriteBatch b)
        {
            _debugRenderer.DrawFabBounds(b, _positionManager.FabBounds, _inputHandler.IsDragging);

            if (_animator.IsExpanded && !_currentLayout.MenuBarBounds.IsEmpty)
            {
                _debugRenderer.DrawMenuBarBounds(b, _currentLayout.MenuBarBounds);
                _debugRenderer.DrawButtonBounds(b, _currentLayout.ButtonBounds);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════════════════

        private bool CanInteract()
        {
            if (!_isVisible) return false;
            if (!Context.IsWorldReady) return false;
            if (Game1.activeClickableMenu != null) return false;
            if (Game1.eventUp && _config.AutoHideInEvents) return false;
            return true;
        }

        private bool ShouldRender()
        {
            if (!_isVisible) return false;
            if (!Context.IsWorldReady) return false;
            if (Game1.activeClickableMenu != null) return false;
            if (Game1.eventUp && _config.AutoHideInEvents) return false;
            return true;
        }

        private void RecalculateLayout()
        {
            _currentLayout = _layoutCalculator.Calculate(
                _positionManager.Position,
                _config.ButtonSize,
                _currentButtons.Count,
                _animator.MenuBarWidth,
                Game1.uiViewport.Width,
                Game1.uiViewport.Height
            );
        }

        private void ResetAllState()
        {
            _animator.Reset();
            _inputHandler.ResetState();
            _callbackQueue.Cancel();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // DISPOSE
        // ═══════════════════════════════════════════════════════════════════════════

        public void Dispose()
        {
            UnwireEvents();
            _drawingHelpers.Dispose();

            _monitor.Log("MobileButtonManager disposed", LogLevel.Trace);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // VIEWPORT STATE TRACKER
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Helper class untuk track viewport changes.
    /// </summary>
    internal class ViewportState
    {
        private int _width;
        private int _height;

        public ViewportState(TileRectangle viewport)
        {
            _width = viewport.Width;
            _height = viewport.Height;
        }

        public void Update(TileRectangle viewport)
        {
            _width = viewport.Width;
            _height = viewport.Height;
        }

        public bool HasChanged(TileRectangle viewport)
        {
            return viewport.Width != _width || viewport.Height != _height;
        }
    }
}