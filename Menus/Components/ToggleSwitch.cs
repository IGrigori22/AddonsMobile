using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AddonsMobile.Menus.Components
{
    /*
    ╔═══════════════════════════════════════════════════════════════════════════════╗
    ║                              TOGGLE SWITCH                                     ║
    ║                                                                               ║
    ║  Komponen UI toggle switch dengan style vanilla Stardew Valley.               ║
    ║  Touch-friendly untuk penggunaan mobile.                                      ║
    ║                                                                               ║
    ║  Visual:                                                                      ║
    ║  ┌──────────────────────────────────────────────────────────────────────┐    ║
    ║  │                                                                      │    ║
    ║  │   OFF State:  ┌─────────┐      ON State:   ┌─────────┐              │    ║
    ║  │               │ ●       │                  │       ● │              │    ║
    ║  │               └─────────┘                  └─────────┘              │    ║
    ║  │               (Gray)                       (Green)                  │    ║
    ║  │                                                                      │    ║
    ║  └──────────────────────────────────────────────────────────────────────┘    ║
    ╚═══════════════════════════════════════════════════════════════════════════════╝
    */

    /// <summary>
    /// A toggle switch UI component with vanilla Stardew Valley styling.
    /// </summary>
    public class ToggleSwitch
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTANTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Default width of the toggle switch</summary>
        private const int DEFAULT_WIDTH = 60;

        /// <summary>Default height of the toggle switch</summary>
        private const int DEFAULT_HEIGHT = 32;

        /// <summary>Padding inside the track</summary>
        private const int TRACK_PADDING = 4;

        /// <summary>Animation speed (0.0 - 1.0 per frame)</summary>
        private const float ANIMATION_SPEED = 0.15f;

        // ═══════════════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Bounds of the toggle switch.
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// Current state of the toggle (on/off).
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary>
        /// Whether the toggle is enabled (can be interacted with).
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether the toggle is currently hovered.
        /// </summary>
        public bool IsHovered { get; private set; }

        /// <summary>
        /// Optional label text.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Unique identifier for this toggle.
        /// </summary>
        public string Id { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // COLORS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Track color when OFF</summary>
        public Color TrackColorOff { get; set; } = new Color(100, 100, 100);

        /// <summary>Track color when ON</summary>
        public Color TrackColorOn { get; set; } = new Color(80, 180, 80);

        /// <summary>Thumb (knob) color</summary>
        public Color ThumbColor { get; set; } = Color.White;

        /// <summary>Border color</summary>
        public Color BorderColor { get; set; } = new Color(60, 60, 60);

        /// <summary>Disabled overlay color</summary>
        public Color DisabledColor { get; set; } = new Color(0, 0, 0, 100);

        // ═══════════════════════════════════════════════════════════════════════════
        // ANIMATION
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Current animation progress (0.0 = left/off, 1.0 = right/on).
        /// </summary>
        private float _animationProgress;

        /// <summary>
        /// Target animation progress.
        /// </summary>
        private float _targetProgress;

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Event fired when toggle state changes.
        /// </summary>
        public event EventHandler<ToggleChangedEventArgs>? ValueChanged;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a new toggle switch.
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="initialState">Initial on/off state</param>
        /// <param name="width">Width (optional)</param>
        /// <param name="height">Height (optional)</param>
        public ToggleSwitch(
            string id,
            int x,
            int y,
            bool initialState = false,
            int width = DEFAULT_WIDTH,
            int height = DEFAULT_HEIGHT)
        {
            Id = id;
            Bounds = new Rectangle(x, y, width, height);
            IsOn = initialState;

            // Set initial animation state
            _animationProgress = initialState ? 1.0f : 0.0f;
            _targetProgress = _animationProgress;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Updates the toggle switch position.
        /// </summary>
        public void SetPosition(int x, int y)
        {
            Bounds = new Rectangle(x, y, Bounds.Width, Bounds.Height);
        }

        /// <summary>
        /// Updates the toggle switch size.
        /// </summary>
        public void SetSize(int width, int height)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y, width, height);
        }

        /// <summary>
        /// Sets the toggle state without triggering the event.
        /// </summary>
        public void SetState(bool isOn, bool animate = true)
        {
            if (IsOn == isOn)
                return;

            IsOn = isOn;
            _targetProgress = isOn ? 1.0f : 0.0f;

            if (!animate)
            {
                _animationProgress = _targetProgress;
            }
        }

        /// <summary>
        /// Sets the toggle state and triggers the event.
        /// </summary>
        public void SetStateWithEvent(bool isOn, bool animate = true)
        {
            bool previousState = IsOn;

            if (previousState == isOn)
                return;

            SetState(isOn, animate);
            OnValueChanged(previousState, isOn);
        }

        /// <summary>
        /// Toggles the current state.
        /// </summary>
        public void Toggle()
        {
            if (!Enabled)
                return;

            bool previousState = IsOn;
            IsOn = !IsOn;
            _targetProgress = IsOn ? 1.0f : 0.0f;

            // Play sound
            Game1.playSound("drumkit6");

            OnValueChanged(previousState, IsOn);
        }

        /// <summary>
        /// Updates the toggle animation.
        /// Call this every frame.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Animate towards target
            if (Math.Abs(_animationProgress - _targetProgress) > 0.001f)
            {
                if (_animationProgress < _targetProgress)
                {
                    _animationProgress = Math.Min(_targetProgress, _animationProgress + ANIMATION_SPEED);
                }
                else
                {
                    _animationProgress = Math.Max(_targetProgress, _animationProgress - ANIMATION_SPEED);
                }
            }
        }

        /// <summary>
        /// Handles hover state.
        /// </summary>
        public void PerformHover(int x, int y)
        {
            IsHovered = Bounds.Contains(x, y) && Enabled;
        }

        /// <summary>
        /// Handles click input.
        /// </summary>
        /// <returns>True if the toggle was clicked</returns>
        public bool ReceiveClick(int x, int y)
        {
            if (!Enabled)
                return false;

            if (Bounds.Contains(x, y))
            {
                Toggle();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws the toggle switch.
        /// </summary>
        public void Draw(SpriteBatch b)
        {
            // Calculate colors based on animation progress
            Color trackColor = Color.Lerp(TrackColorOff, TrackColorOn, _animationProgress);

            if (IsHovered && Enabled)
            {
                trackColor = Color.Lerp(trackColor, Color.White, 0.2f);
            }

            // ─────────────── DRAW TRACK ───────────────

            // Track background (rounded rectangle simulation with texture box)
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                Bounds.X,
                Bounds.Y,
                Bounds.Width,
                Bounds.Height,
                trackColor,
                1f,
                drawShadow: false
            );

            // ─────────────── DRAW THUMB ───────────────

            int thumbSize = Bounds.Height - (TRACK_PADDING * 2);
            int thumbTravel = Bounds.Width - thumbSize - (TRACK_PADDING * 2);
            int thumbX = Bounds.X + TRACK_PADDING + (int)(thumbTravel * _animationProgress);
            int thumbY = Bounds.Y + TRACK_PADDING;

            // Thumb shadow
            b.Draw(
                Game1.staminaRect,
                new Rectangle(thumbX + 2, thumbY + 2, thumbSize, thumbSize),
                Color.Black * 0.3f
            );

            // Thumb
            Color currentThumbColor = ThumbColor;
            if (IsHovered && Enabled)
            {
                currentThumbColor = Color.Lerp(ThumbColor, Color.Yellow, 0.1f);
            }

            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                thumbX,
                thumbY,
                thumbSize,
                thumbSize,
                currentThumbColor,
                1f,
                drawShadow: false
            );

            // ─────────────── DRAW DISABLED OVERLAY ───────────────

            if (!Enabled)
            {
                b.Draw(
                    Game1.staminaRect,
                    Bounds,
                    DisabledColor
                );
            }

            // ─────────────── DRAW LABEL ───────────────

            if (!string.IsNullOrEmpty(Label))
            {
                Vector2 labelPos = new Vector2(
                    Bounds.Right + 12,
                    Bounds.Y + (Bounds.Height - Game1.smallFont.MeasureString(Label).Y) / 2
                );

                Color labelColor = Enabled ? Game1.textColor : Game1.textColor * 0.5f;

                Utility.drawTextWithShadow(
                    b,
                    Label,
                    Game1.smallFont,
                    labelPos,
                    labelColor
                );
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        private void OnValueChanged(bool previousValue, bool newValue)
        {
            ValueChanged?.Invoke(this, new ToggleChangedEventArgs(Id, previousValue, newValue));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // EVENT ARGS
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Event arguments for toggle state changes.
    /// </summary>
    public class ToggleChangedEventArgs : EventArgs
    {
        /// <summary>Toggle identifier</summary>
        public string Id { get; }

        /// <summary>Previous state</summary>
        public bool PreviousValue { get; }

        /// <summary>New state</summary>
        public bool NewValue { get; }

        public ToggleChangedEventArgs(string id, bool previousValue, bool newValue)
        {
            Id = id;
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}