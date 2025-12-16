using AddonsMobile.Config;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace AddonsMobile.UI.Data
{
    /// <summary>
    /// Mengelola posisi FAB (save/load/update)
    /// </summary>
    public class PositionManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ModConfig _config;

        private ButtonPositionData _positionData;

        private const string POSITION_DATA_FILE = "button-position.json";
        public const int SCREEN_EDGE_PADDING = 20;

        public ButtonPositionData Data => _positionData;
        public Vector2 Position { get; private set; }
        public Rectangle FabBounds { get; private set; }

        public PositionManager(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _config = config;

            LoadPositionData();
        }

        public void LoadPositionData()
        {
            try
            {
                _positionData = _helper.Data.ReadJsonFile<ButtonPositionData>(POSITION_DATA_FILE)
                                ?? new ButtonPositionData();

                if (!_positionData.HasSavedPosition)
                {
                    _positionData.PositionXPercent = _config.ButtonPositionX;
                    _positionData.PositionYPercent = _config.ButtonPositionY;
                    _monitor.Log("Using default position from config", LogLevel.Trace);
                }
                else
                {
                    _monitor.Log($"Loaded saved position: ({_positionData.PositionXPercent:F1}%, {_positionData.PositionYPercent:F1}%)", LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to load position data: {ex.Message}", LogLevel.Warn);
                _positionData = new ButtonPositionData
                {
                    PositionXPercent = _config.ButtonPositionX,
                    PositionYPercent = _config.ButtonPositionY
                };
            }
        }

        public void SavePositionData()
        {
            try
            {
                _positionData.LastSaved = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _helper.Data.WriteJsonFile(POSITION_DATA_FILE, _positionData);

                if (_config.VerboseLogging)
                {
                    _monitor.Log($"Saved button position: ({_positionData.PositionXPercent:F1}%, {_positionData.PositionYPercent:F1}%)", LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to save position data: {ex.Message}", LogLevel.Warn);
            }
        }

        public void ResetPosition()
        {
            _positionData.PositionXPercent = _config.ButtonPositionX;
            _positionData.PositionYPercent = _config.ButtonPositionY;
            _positionData.LastSaved = 0;

            try
            {
                _helper.Data.WriteJsonFile<ButtonPositionData>(POSITION_DATA_FILE, null);
            }
            catch { }

            UpdatePosition();
            Game1.playSound("dialogueCharacter");
            _monitor.Log($"Position reset to default: ({_config.ButtonPositionX}%, {_config.ButtonPositionY}%)", LogLevel.Info);
        }

        // ══════════════════════════════════════════════════════════════════
        // NEW HELPER METHOD: Get safe bounds considering safe area config
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Menghitung batas aman untuk FAB dengan mempertimbangkan safe area
        /// </summary>
        private (int minX, int maxX, int minY, int maxY) GetSafeBounds(int screenWidth, int screenHeight)
        {
            int minX = SCREEN_EDGE_PADDING + _config.SafeAreaLeft;
            int maxX = screenWidth - _config.ButtonSize - SCREEN_EDGE_PADDING - _config.SafeAreaRight;
            int minY = SCREEN_EDGE_PADDING + _config.SafeAreaTop;
            int maxY = screenHeight - _config.ButtonSize - SCREEN_EDGE_PADDING - _config.SafeAreaBottom;

            // Pastikan max tidak lebih kecil dari min
            maxX = Math.Max(minX, maxX);
            maxY = Math.Max(minY, maxY);

            return (minX, maxX, minY, maxY);
        }

        // ══════════════════════════════════════════════════════════════════
        // MODIFIED: UpdatePosition now uses safe area
        // ══════════════════════════════════════════════════════════════════

        public void UpdatePosition()
        {
            int screenWidth = Game1.uiViewport.Width;
            int screenHeight = Game1.uiViewport.Height;

            if (screenWidth <= 0 || screenHeight <= 0)
            {
                screenWidth = 1280;
                screenHeight = 720;
            }

            // Calculate position from percentage
            Position = new Vector2(
                screenWidth * (_positionData.PositionXPercent / 100f) - _config.ButtonSize / 2f,
                screenHeight * (_positionData.PositionYPercent / 100f) - _config.ButtonSize / 2f
            );

            // ← CHANGED: Now uses safe area bounds
            var (minX, maxX, minY, maxY) = GetSafeBounds(screenWidth, screenHeight);

            Position = new Vector2(
                MathHelper.Clamp(Position.X, minX, maxX),
                MathHelper.Clamp(Position.Y, minY, maxY)
            );

            FabBounds = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                _config.ButtonSize,
                _config.ButtonSize
            );

            if (_config.VerboseLogging)
            {
                _monitor.Log($"FAB position: ({Position.X:F0}, {Position.Y:F0}) " +
                            $"SafeArea: L={_config.SafeAreaLeft} R={_config.SafeAreaRight} " +
                            $"T={_config.SafeAreaTop} B={_config.SafeAreaBottom}", LogLevel.Debug);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // FIXED: UpdateFromDrag - consistent coordinate system
        // ══════════════════════════════════════════════════════════════════

        public void UpdateFromDrag(Vector2 screenPos)
        {
            // ← CHANGED: Use Utility.ModifyCoordinatesForUIScale for consistency
            // Previously was mixing viewport and uiViewport incorrectly
            Vector2 scaledPos = Utility.ModifyCoordinatesForUIScale(screenPos);

            int screenWidth = Game1.uiViewport.Width;
            int screenHeight = Game1.uiViewport.Height;

            // Center FAB ke posisi touch
            Vector2 newPos = new Vector2(
                scaledPos.X - _config.ButtonSize / 2f,
                scaledPos.Y - _config.ButtonSize / 2f
            );

            // ← CHANGED: Now uses safe area bounds
            var (minX, maxX, minY, maxY) = GetSafeBounds(screenWidth, screenHeight);

            newPos.X = MathHelper.Clamp(newPos.X, minX, maxX);
            newPos.Y = MathHelper.Clamp(newPos.Y, minY, maxY);

            // Update position
            Position = newPos;

            // Update bounds
            FabBounds = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                _config.ButtonSize,
                _config.ButtonSize
            );

            // Update position data untuk disimpan nanti
            float centerX = Position.X + _config.ButtonSize / 2f;
            float centerY = Position.Y + _config.ButtonSize / 2f;

            _positionData.PositionXPercent = MathHelper.Clamp(
                (centerX / screenWidth) * 100f, 5f, 95f);
            _positionData.PositionYPercent = MathHelper.Clamp(
                (centerY / screenHeight) * 100f, 5f, 95f);
        }
    }
}