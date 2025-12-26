using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace AddonsMobile.Menus
{
    /// <summary>
    /// Menu custom dengan layout split: Kiri 30% dan Kanan 70%.
    /// </summary>
    public class ConfigurationMenu : IClickableMenu
    {
        /*********
        ** Konstanta Layout
        *********/
        private const int MENU_WIDTH = 1200;
        private const int MENU_HEIGHT = 800;
        private const float LEFT_PANEL_RATIO = 0.30f;  // 30%
        private const float RIGHT_PANEL_RATIO = 0.70f; // 70%
        private const int PANEL_SPACING = 16; // Jarak antara panel kiri dan kanan
        private const int PANEL_PADDING = 16; // Padding dalam panel

        /*********
        ** Panel Rectangles
        *********/
        public Rectangle LeftPanelBounds;
        public Rectangle RightPanelBounds;

        /*********
        ** Komponen UI - Panel Kiri
        *********/
        private List<ClickableTextureComponent> _leftPanelButtons;
        private string _leftPanelTitle = "Menu Kiri";

        /*********
        ** Komponen UI - Panel Kanan
        *********/
        private ClickableTextureComponent _rightPanelButton;
        private string _rightPanelTitle = "Konten Kanan";
        private string _rightPanelContent = "Ini adalah area konten utama.\nKamu bisa menampilkan informasi detail di sini.\n\nPanel kanan memiliki 70% dari lebar menu.";

        /*********
        ** Hover & State
        *********/
        private string _hoverText;
        private string _hoverTitle;
        private int _selectedLeftIndex = 0;

        /*********
        ** Konstruktor
        *********/
        public ConfigurationMenu()
            : base(
                  x: Game1.uiViewport.Width / 2 - MENU_WIDTH / 2,
                  y: Game1.uiViewport.Height / 2 - MENU_HEIGHT / 2,
                  width: MENU_WIDTH,
                  height: MENU_HEIGHT,
                  showUpperRightCloseButton: true
              )
        {
            InitializeLayout();
            InitializeComponents();

            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        /*********
        ** Inisialisasi Layout
        *********/
        private void InitializeLayout()
        {
            // Hitung lebar panel
            int totalPanelWidth = width - (PANEL_PADDING * 3) - PANEL_SPACING;
            int leftPanelWidth = (int)(totalPanelWidth * LEFT_PANEL_RATIO);
            int rightPanelWidth = (int)(totalPanelWidth * RIGHT_PANEL_RATIO);

            // Panel Kiri (30%)
            LeftPanelBounds = new Rectangle(
                xPositionOnScreen + PANEL_PADDING,
                yPositionOnScreen + 80, // Beri ruang untuk title
                leftPanelWidth,
                height - 80 - PANEL_PADDING
            );

            // Panel Kanan (70%)
            RightPanelBounds = new Rectangle(
                LeftPanelBounds.X + LeftPanelBounds.Width + PANEL_SPACING,
                yPositionOnScreen + 80,
                rightPanelWidth,
                height - 80 - PANEL_PADDING
            );
        }

        /*********
        ** Inisialisasi Komponen
        *********/
        private void InitializeComponents()
        {
            // ===== PANEL KIRI: Daftar Tombol =====
            _leftPanelButtons = new List<ClickableTextureComponent>();

            int buttonHeight = 72;
            int buttonSpacing = 8;
            int buttonY = LeftPanelBounds.Y + PANEL_PADDING;

            // Contoh: 5 tombol di panel kiri
            for (int i = 0; i < 5; i++)
            {
                Rectangle bounds = new Rectangle(
                    LeftPanelBounds.X + PANEL_PADDING,
                    buttonY,
                    LeftPanelBounds.Width - (PANEL_PADDING * 2),
                    buttonHeight
                );

                var button = new ClickableTextureComponent(
                    name: $"LeftButton{i}",
                    bounds: bounds,
                    label: $"Opsi {i + 1}",
                    hoverText: $"Klik untuk melihat detail Opsi {i + 1}",
                    texture: Game1.mouseCursors,
                    sourceRect: new Rectangle(256, 256, 10, 10), // Kotak putih polos
                    scale: 4f
                )
                {
                    myID = 100 + i,
                    downNeighborID = (i < 4) ? 100 + i + 1 : -1,
                    upNeighborID = (i > 0) ? 100 + i - 1 : -1,
                    rightNeighborID = 200 // ID tombol di panel kanan
                };

                _leftPanelButtons.Add(button);
                buttonY += buttonHeight + buttonSpacing;
            }

            // ===== PANEL KANAN: Contoh Tombol Aksi =====
            Rectangle rightButtonBounds = new Rectangle(
                RightPanelBounds.X + RightPanelBounds.Width / 2 - 100,
                RightPanelBounds.Y + RightPanelBounds.Height - 100,
                200,
                64
            );

            _rightPanelButton = new ClickableTextureComponent(
                name: "RightActionButton",
                bounds: rightButtonBounds,
                label: null,
                hoverText: "Tombol Aksi",
                texture: Game1.mouseCursors,
                sourceRect: new Rectangle(294, 428, 21, 11), // Tombol hijau
                scale: 4f
            )
            {
                myID = 200,
                leftNeighborID = 100 + _selectedLeftIndex
            };
        }

        /*********
        ** Navigasi Gamepad
        *********/
        public override void populateClickableComponentList()
        {
            allClickableComponents = new List<ClickableComponent>();

            foreach (var button in _leftPanelButtons)
            {
                allClickableComponents.Add(button);
            }

            allClickableComponents.Add(_rightPanelButton);

            if (upperRightCloseButton != null)
                allClickableComponents.Add(upperRightCloseButton);
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = _leftPanelButtons[0];
            snapCursorToCurrentSnappedComponent();
        }

        public override bool areGamePadControlsImplemented()
        {
            return true;
        }

        /*********
        ** Input - Mouse
        *********/
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            // Klik tombol di panel kiri
            for (int i = 0; i < _leftPanelButtons.Count; i++)
            {
                if (_leftPanelButtons[i].containsPoint(x, y))
                {
                    if (playSound)
                        Game1.playSound("smallSelect");

                    _selectedLeftIndex = i;
                    OnLeftButtonClicked(i);
                    return;
                }
            }

            // Klik tombol di panel kanan
            if (_rightPanelButton.containsPoint(x, y))
            {
                if (playSound)
                    Game1.playSound("bigSelect");

                OnRightButtonClicked();
                return;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            // Klik kanan untuk menutup
            if (readyToClose())
                exitThisMenu();
        }

        /*********
        ** Input - Keyboard/Gamepad
        *********/
        public override void receiveKeyPress(Keys key)
        {
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
            {
                exitThisMenu();
                return;
            }

            if (Game1.options.snappyMenus && Game1.options.gamepadControls)
            {
                applyMovementKey(key);
            }

            base.receiveKeyPress(key);
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);

            if (b == Buttons.A && currentlySnappedComponent != null)
            {
                // Tombol di panel kiri
                for (int i = 0; i < _leftPanelButtons.Count; i++)
                {
                    if (currentlySnappedComponent == _leftPanelButtons[i])
                    {
                        _selectedLeftIndex = i;
                        OnLeftButtonClicked(i);
                        return;
                    }
                }

                // Tombol di panel kanan
                if (currentlySnappedComponent == _rightPanelButton)
                {
                    OnRightButtonClicked();
                }
            }
        }

        /*********
        ** Hover
        *********/
        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            _hoverText = null;
            _hoverTitle = null;

            // Hover tombol kiri
            foreach (var button in _leftPanelButtons)
            {
                if (button.containsPoint(x, y))
                {
                    button.scale = 4.1f;
                    _hoverText = button.hoverText;
                }
                else
                {
                    button.scale = 4.0f;
                }
            }

            // Hover tombol kanan
            if (_rightPanelButton.containsPoint(x, y))
            {
                _rightPanelButton.scale = 4.2f;
                _hoverText = _rightPanelButton.hoverText;
            }
            else
            {
                _rightPanelButton.scale = 4.0f;
            }
        }

        /*********
        ** Update
        *********/
        public override void update(GameTime time)
        {
            base.update(time);
        }

        /*********
        ** Draw
        *********/
        public override void draw(SpriteBatch b)
        {
            // Background overlay gelap
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(
                    Game1.fadeToBlackRect,
                    Game1.graphics.GraphicsDevice.Viewport.Bounds,
                    Color.Black * 0.6f
                );
            }

            // ===== BACKGROUND MENU UTAMA =====
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                Color.White,
                1f,
                drawShadow: true
            );

            // ===== JUDUL MENU UTAMA =====
            DrawMenuTitle(b);

            // ===== PANEL KIRI (30%) =====
            DrawLeftPanel(b);

            // ===== PANEL KANAN (70%) =====
            DrawRightPanel(b);

            // ===== TOMBOL CLOSE =====
            base.draw(b);

            // ===== HOVER TOOLTIP =====
            if (!string.IsNullOrEmpty(_hoverText))
            {
                IClickableMenu.drawHoverText(
                    b,
                    _hoverText,
                    Game1.smallFont,
                    0, 0, -1,
                    _hoverTitle
                );
            }

            // ===== CURSOR =====
            drawMouse(b);
        }

        private void DrawMenuTitle(SpriteBatch b)
        {
            string title = "Split Layout Menu";
            SpriteFont titleFont = Game1.dialogueFont;
            Vector2 titleSize = titleFont.MeasureString(title);
            Vector2 titlePos = new Vector2(
                xPositionOnScreen + width / 2 - titleSize.X / 2,
                yPositionOnScreen + 20
            );
            Utility.drawTextWithShadow(b, title, titleFont, titlePos, Game1.textColor);
        }

        private void DrawLeftPanel(SpriteBatch b)
        {
            // Background panel kiri
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                LeftPanelBounds.X,
                LeftPanelBounds.Y,
                LeftPanelBounds.Width,
                LeftPanelBounds.Height,
                Color.LightBlue * 0.3f,
                1f,
                drawShadow: false
            );

            // Judul panel kiri
            Vector2 leftTitlePos = new Vector2(
                LeftPanelBounds.X + PANEL_PADDING,
                LeftPanelBounds.Y - 40
            );
            Utility.drawTextWithShadow(b, _leftPanelTitle, Game1.smallFont, leftTitlePos, Game1.textColor);

            // Gambar tombol-tombol
            for (int i = 0; i < _leftPanelButtons.Count; i++)
            {
                var button = _leftPanelButtons[i];

                // Background tombol
                Color buttonColor = (i == _selectedLeftIndex) ? Color.Gold : Color.White;
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60),
                    button.bounds.X,
                    button.bounds.Y,
                    button.bounds.Width,
                    button.bounds.Height,
                    buttonColor * 0.7f,
                    1f,
                    drawShadow: false
                );

                // Label tombol
                Vector2 labelSize = Game1.smallFont.MeasureString(button.label);
                Vector2 labelPos = new Vector2(
                    button.bounds.X + button.bounds.Width / 2 - labelSize.X / 2,
                    button.bounds.Y + button.bounds.Height / 2 - labelSize.Y / 2
                );
                Utility.drawTextWithShadow(b, button.label, Game1.smallFont, labelPos,
                    (i == _selectedLeftIndex) ? Color.Yellow : Game1.textColor);

                // Indikator selected
                if (i == _selectedLeftIndex)
                {
                    b.Draw(
                        Game1.mouseCursors,
                        new Vector2(button.bounds.X - 20, button.bounds.Y + button.bounds.Height / 2 - 16),
                        new Rectangle(421, 459, 12, 8),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        3f,
                        SpriteEffects.None,
                        0.9f
                    );
                }
            }
        }

        private void DrawRightPanel(SpriteBatch b)
        {
            // Background panel kanan
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                RightPanelBounds.X,
                RightPanelBounds.Y,
                RightPanelBounds.Width,
                RightPanelBounds.Height,
                Color.LightGreen * 0.3f,
                1f,
                drawShadow: false
            );

            // Judul panel kanan
            Vector2 rightTitlePos = new Vector2(
                RightPanelBounds.X + PANEL_PADDING,
                RightPanelBounds.Y - 40
            );
            string dynamicTitle = $"{_rightPanelTitle} - Opsi {_selectedLeftIndex + 1}";
            Utility.drawTextWithShadow(b, dynamicTitle, Game1.smallFont, rightTitlePos, Game1.textColor);

            // Konten panel kanan
            Vector2 contentPos = new Vector2(
                RightPanelBounds.X + PANEL_PADDING,
                RightPanelBounds.Y + PANEL_PADDING
            );

            string displayContent = $"{_rightPanelContent}\n\nOpsi yang dipilih: {_selectedLeftIndex + 1}";

            Utility.drawTextWithShadow(
                b,
                displayContent,
                Game1.smallFont,
                contentPos,
                Game1.textColor,
                1f,
                -1f,
                2, 2
            );

            // Tombol aksi di panel kanan
            _rightPanelButton.draw(b);

            // Label tombol
            string buttonLabel = "Eksekusi";
            Vector2 buttonLabelSize = Game1.smallFont.MeasureString(buttonLabel);
            Vector2 buttonLabelPos = new Vector2(
                _rightPanelButton.bounds.X + _rightPanelButton.bounds.Width / 2 - buttonLabelSize.X / 2,
                _rightPanelButton.bounds.Y + _rightPanelButton.bounds.Height / 2 - buttonLabelSize.Y / 2
            );
            Utility.drawTextWithShadow(b, buttonLabel, Game1.smallFont, buttonLabelPos, Color.White);
        }

        /*********
        ** Event Handlers
        *********/
        private void OnLeftButtonClicked(int index)
        {
            _selectedLeftIndex = index;
            _rightPanelContent = $"Kamu memilih Opsi {index + 1}.\n\nIni adalah konten detail untuk opsi tersebut.\n\nPanel kanan dapat menampilkan informasi yang berbeda tergantung pilihan di panel kiri.";

            Game1.addHUDMessage(new HUDMessage($"Opsi {index + 1} dipilih", HUDMessage.achievement_type));
        }

        private void OnRightButtonClicked()
        {
            Game1.addHUDMessage(new HUDMessage($"Aksi dieksekusi untuk Opsi {_selectedLeftIndex + 1}!", HUDMessage.newQuest_type));

            // Contoh: tutup menu setelah aksi
            // exitThisMenu();
        }

        /*********
        ** Lifecycle
        *********/
        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
        }

        public override bool readyToClose()
        {
            return true;
        }
    }
}