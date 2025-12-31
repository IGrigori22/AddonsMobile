using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.Internal.Core;
using AddonsMobile.Menus.Components;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace AddonsMobile.Menus
{
    /// <summary>
    /// Menu dashboard untuk melihat dan mengelola semua button yang terdaftar.
    /// Layout: Left panel (tabs + navigation) + Right panel (content)
    /// </summary>
    public class GeneralMenu : IClickableMenu
    {
        #region Constants

        private const int BorderPadding = 32;
        private const int PanelPadding = 16;
        private const int PanelSpacing = 16;
        private const float LeftPanelRatio = 0.28f;
        private const float RightPanelRatio = 0.72f;

        private const int GridItemWidth = ButtonGridItem.ItemWidth;
        private const int GridItemHeight = ButtonGridItem.ItemHeight;
        private const int GridSpacing = 10;

        private const int ScrollSpeed = 30;
        private const int DragThreshold = 10;

        private const int TabHeight = 44;
        private const int TabSpacing = 4;
        private const int NavItemHeight = 36;
        private const int NavItemSpacing = 6;

        #endregion

        #region Layout Bounds

        private Rectangle _leftPanelBounds;
        private Rectangle _rightPanelBounds;
        private Rectangle _contentAreaBounds;
        private Rectangle _tabAreaBounds;
        private Rectangle _navAreaBounds;

        #endregion

        #region Tab System

        /// <summary>Tab yang sedang aktif</summary>
        private MenuTab _currentTab = MenuTab.Dashboard;

        /// <summary>List semua tombol tab</summary>
        private readonly List<ClickableComponent> _tabButtons = new();

        /// <summary>Label untuk setiap tab</summary>
        private readonly Dictionary<MenuTab, string> _tabLabels = new()
        {
            { MenuTab.Dashboard, "Dashboard" },
            { MenuTab.Settings, "Settings" },
            { MenuTab.Debug, "Debug" },
            { MenuTab.About, "About" }
        };

        /// <summary>Icon untuk setiap tab (menggunakan emoji sebagai placeholder)</summary>
        private readonly Dictionary<MenuTab, string> _tabIcons = new()
        {
            { MenuTab.Dashboard, "📊" },
            { MenuTab.Settings, "⚙" },
            { MenuTab.Debug, "🔧" },
            { MenuTab.About, "ℹ" }
        };

        /// <summary>Index tab yang sedang di-hover (-1 jika tidak ada)</summary>
        private int _hoveredTabIndex = -1;

        #endregion

        #region Navigation Items (per Tab)

        private readonly List<ClickableComponent> _navItems = new();
        private ClickableComponent? _hoveredNavItem = null;
        private int _navScrollOffset = 0;
        private int _navMaxScrollOffset = 0;

        // Dashboard tab specific
        private KeyCategory? _activeFilter = null;
        private SortMode _currentSort = SortMode.Priority;

        private enum SortMode
        {
            Priority,
            Name,
            Category,
            Mod
        }

        #endregion

        #region Grid State (Dashboard Tab)

        private readonly List<ButtonGridItem> _gridItems = new();
        private ButtonGridItem? _hoveredItem = null;
        private int _gridColumns = 1;

        #endregion

        #region Scroll State

        private int _scrollOffset = 0;
        private int _maxScrollOffset = 0;
        private int _totalContentHeight = 0;

        #endregion

        #region Drag State

        private bool _isDragging = false;
        private Vector2 _dragStartPosition;
        private int _lastTouchY = 0;
        private bool _dragThresholdMet = false;

        #endregion

        #region Dependencies

        private readonly KeyRegistry _registry;
        private readonly ModConfig _config;
        private readonly IMonitor _monitor;
        private List<ModKeyButton> _allButtonsCache = new();
        private DashboardStats _stats = null!;

        #endregion

        #region Constructor

        public GeneralMenu(KeyRegistry registry, ModConfig config, IMonitor monitor)
            : base(0, 0, 800, 600, showUpperRightCloseButton: true)
        {
            _registry = registry ?? throw new InvalidOperationException("Registry not initialized");
            _config = config ?? throw new InvalidOperationException("Config not initialized");
            _monitor = monitor;

            CalculateMenuDimensions();
            InitializePanels();
            SetupCloseButton();
            InitializeTabButtons();
            InitializeNavItems();
            CalculateGridColumns();
            RefreshGridItems();

            Game1.playSound("bigSelect");
        }

        #endregion

        #region Initialization

        private void CalculateMenuDimensions()
        {
            int viewportWidth = Game1.uiViewport.Width;
            int viewportHeight = Game1.uiViewport.Height;

            bool isMobile = Game1.options.gamepadControls || viewportWidth < 1280;
            float widthRatio = isMobile ? 0.95f : 0.85f;
            float heightRatio = isMobile ? 0.92f : 0.80f;

            width = Math.Clamp((int)(viewportWidth * widthRatio), 750, 1400);
            height = Math.Clamp((int)(viewportHeight * heightRatio), 500, 900);

            xPositionOnScreen = Math.Max(0, (viewportWidth - width) / 2);
            yPositionOnScreen = Math.Max(0, (viewportHeight - height) / 2);
        }

        private void InitializePanels()
        {
            int innerX = xPositionOnScreen + BorderPadding;
            int innerY = yPositionOnScreen + BorderPadding;
            int innerWidth = width - (BorderPadding * 2);
            int innerHeight = height - (BorderPadding * 2);

            int leftWidth = (int)(innerWidth * LeftPanelRatio) - (PanelSpacing / 2);
            int rightWidth = (int)(innerWidth * RightPanelRatio) - (PanelSpacing / 2);

            _leftPanelBounds = new Rectangle(innerX, innerY, leftWidth, innerHeight);
            _rightPanelBounds = new Rectangle(_leftPanelBounds.Right + PanelSpacing, innerY, rightWidth, innerHeight);

            // Tab area (top of left panel)
            int tabCount = _tabLabels.Count;
            int tabAreaHeight = (TabHeight * tabCount) + (TabSpacing * (tabCount - 1)) + PanelPadding * 2;
            _tabAreaBounds = new Rectangle(
                _leftPanelBounds.X,
                _leftPanelBounds.Y,
                _leftPanelBounds.Width,
                tabAreaHeight
            );

            // Navigation area (below tabs)
            _navAreaBounds = new Rectangle(
                _leftPanelBounds.X + PanelPadding,
                _tabAreaBounds.Bottom + PanelPadding,
                _leftPanelBounds.Width - (PanelPadding * 2),
                _leftPanelBounds.Height - tabAreaHeight - PanelPadding * 2
            );

            // Content area (right panel)
            int titleHeight = 60;
            _contentAreaBounds = new Rectangle(
                _rightPanelBounds.X + PanelPadding,
                _rightPanelBounds.Y + PanelPadding + titleHeight,
                _rightPanelBounds.Width - (PanelPadding * 2),
                _rightPanelBounds.Height - (PanelPadding * 2) - titleHeight
            );
        }

        private void SetupCloseButton()
        {
            int size = 48;
            upperRightCloseButton = new ClickableTextureComponent(
                new Rectangle(xPositionOnScreen + width - size - 8, yPositionOnScreen - size / 4, size, size),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );
        }

        private void InitializeTabButtons()
        {
            _tabButtons.Clear();

            int y = _tabAreaBounds.Y + PanelPadding;
            int tabWidth = _tabAreaBounds.Width - PanelPadding * 2;

            foreach (MenuTab tab in Enum.GetValues<MenuTab>())
            {
                _tabButtons.Add(new ClickableComponent(
                    new Rectangle(_tabAreaBounds.X + PanelPadding, y, tabWidth, TabHeight),
                    tab.ToString(),
                    _tabLabels[tab]
                )
                {
                    myID = (int)tab
                });

                y += TabHeight + TabSpacing;
            }
        }

        private void InitializeNavItems()
        {
            _navItems.Clear();
            _navScrollOffset = 0;

            switch (_currentTab)
            {
                case MenuTab.Dashboard:
                    InitializeDashboardNav();
                    break;
                case MenuTab.Settings:
                    InitializeSettingsNav();
                    break;
                case MenuTab.Debug:
                    InitializeDebugNav();
                    break;
                case MenuTab.About:
                    InitializeAboutNav();
                    break;
            }

            CalculateNavScrollHeight();
        }

        private void InitializeDashboardNav()
        {
            int y = _navAreaBounds.Y;
            int itemWidth = _navAreaBounds.Width;

            // ═══ FILTER SECTION ═══
            AddNavLabel("─ Filter ─", ref y, itemWidth);

            // "All" filter
            AddNavItem("filter_all", "All Buttons", ref y, itemWidth);

            // Category filters
            foreach (var category in KeyCategoryExtensions.GetAllCategories())
            {
                AddNavItem($"filter_{category}", category.GetDisplayName(), ref y, itemWidth, (int)category);
            }

            y += NavItemSpacing * 2;

            // ═══ SORT SECTION ═══
            AddNavLabel("─ Sort By ─", ref y, itemWidth);

            foreach (SortMode mode in Enum.GetValues<SortMode>())
            {
                AddNavItem($"sort_{mode}", mode.ToString(), ref y, itemWidth);
            }

            y += NavItemSpacing * 2;

            // ═══ ACTIONS SECTION ═══
            AddNavLabel("─ Actions ─", ref y, itemWidth);

            AddNavItem("action_refresh", "↻ Refresh", ref y, itemWidth);
            AddNavItem("action_reset", "⟲ Reset States", ref y, itemWidth);
        }

        private void InitializeSettingsNav()
        {
            int y = _navAreaBounds.Y;
            int itemWidth = _navAreaBounds.Width;

            AddNavLabel("─ Display ─", ref y, itemWidth);
            AddNavItem("setting_button_size", $"Button Size: {_config.MenuButtonSize}", ref y, itemWidth);
            AddNavItem("setting_show_labels", $"Show Labels: {(_config.ShowButtonLabels ? "Yes" : "No")}", ref y, itemWidth);

            y += NavItemSpacing * 2;

            AddNavLabel("─ Behavior ─", ref y, itemWidth);
            AddNavItem("setting_auto_hide", $"Auto Hide: {(_config.AutoHideInEvents ? "Yes" : "No")}", ref y, itemWidth);
            AddNavItem("setting_drag_indicator", $"Drag Indicator: {(_config.DragShowIndicator ? "Yes" : "No")}", ref y, itemWidth);

            y += NavItemSpacing * 2;

            AddNavLabel("─ Position ─", ref y, itemWidth);
            AddNavItem("setting_reset_pos", "Reset FAB Position", ref y, itemWidth);

            y += NavItemSpacing * 2;

            AddNavLabel("─ Advanced ─", ref y, itemWidth);
            AddNavItem("setting_verbose", $"Verbose Log: {(_config.DebugVerboseLogging ? "Yes" : "No")}", ref y, itemWidth);
        }

        private void InitializeDebugNav()
        {
            int y = _navAreaBounds.Y;
            int itemWidth = _navAreaBounds.Width;

            AddNavLabel("─ Registry ─", ref y, itemWidth);
            AddNavItem("debug_count", $"Total Buttons: {_registry.Count}", ref y, itemWidth);
            AddNavItem("debug_mods", $"Registered Mods: {_registry.ModCount}", ref y, itemWidth);

            y += NavItemSpacing * 2;

            AddNavLabel("─ Actions ─", ref y, itemWidth);
            AddNavItem("debug_refresh", "Force Refresh", ref y, itemWidth);
            AddNavItem("debug_clear", "Clear All Buttons", ref y, itemWidth);
            AddNavItem("debug_diagnostics", "Show Diagnostics", ref y, itemWidth);

            y += NavItemSpacing * 2;

            AddNavLabel("─ Export ─", ref y, itemWidth);
            AddNavItem("debug_export_log", "Export to Log", ref y, itemWidth);
        }

        private void InitializeAboutNav()
        {
            int y = _navAreaBounds.Y;
            int itemWidth = _navAreaBounds.Width;

            AddNavLabel("─ Links ─", ref y, itemWidth);
            AddNavItem("about_nexus", "Nexus Mods", ref y, itemWidth);
            AddNavItem("about_github", "GitHub", ref y, itemWidth);
            AddNavItem("about_discord", "Discord", ref y, itemWidth);

            y += NavItemSpacing * 2;

            AddNavLabel("─ Support ─", ref y, itemWidth);
            AddNavItem("about_report", "Report Bug", ref y, itemWidth);
            AddNavItem("about_donate", "Support Dev", ref y, itemWidth);
        }

        private void AddNavLabel(string text, ref int y, int width)
        {
            _navItems.Add(new ClickableComponent(
                new Rectangle(_navAreaBounds.X, y, width, NavItemHeight),
                "label",
                text
            ));
            y += NavItemHeight + NavItemSpacing;
        }

        private void AddNavItem(string name, string label, ref int y, int width, int id = -1)
        {
            var item = new ClickableComponent(
                new Rectangle(_navAreaBounds.X, y, width, NavItemHeight),
                name,
                label
            );
            if (id >= 0) item.myID = id;
            _navItems.Add(item);
            y += NavItemHeight + NavItemSpacing;
        }

        private void CalculateNavScrollHeight()
        {
            if (_navItems.Count == 0)
            {
                _navMaxScrollOffset = 0;
                return;
            }

            int lastItemBottom = _navItems.Last().bounds.Bottom;
            int totalNavHeight = lastItemBottom - _navAreaBounds.Y;
            _navMaxScrollOffset = Math.Max(0, totalNavHeight - _navAreaBounds.Height);
        }

        private void CalculateGridColumns()
        {
            int availableWidth = _contentAreaBounds.Width - 20;
            _gridColumns = Math.Max(1, (availableWidth + GridSpacing) / (GridItemWidth + GridSpacing));
        }

        private void RefreshGridItems()
        {
            _gridItems.Clear();
            _hoveredItem = null;
            _scrollOffset = 0;

            var allButtons = _registry.GetAllButtonsIncludingHidden()?.ToList();
            if (allButtons == null || allButtons.Count == 0)
            {
                _allButtonsCache = new List<ModKeyButton>();
                UpdateStats();
                CalculateContentHeight();
                return;
            }

            _allButtonsCache = allButtons;

            // Apply filter
            var filtered = _activeFilter.HasValue
                ? allButtons.Where(b => b.Category == _activeFilter.Value)
                : allButtons;

            // Apply sort
            var sorted = _currentSort switch
            {
                SortMode.Priority => filtered.OrderByDescending(b => b.Priority)
                                            .ThenBy(b => b.DisplayName),
                SortMode.Name => filtered.OrderBy(b => b.DisplayName),
                SortMode.Category => filtered.OrderBy(b => b.Category.GetSortOrder())
                                            .ThenBy(b => b.DisplayName),
                SortMode.Mod => filtered.OrderBy(b => b.ModId)
                                       .ThenBy(b => b.DisplayName),
                _ => filtered.OrderByDescending(b => b.Priority)
            };

            var sortedList = sorted.ToList();

            // Create grid items
            int startY = _contentAreaBounds.Y + 150;

            for (int i = 0; i < sortedList.Count; i++)
            {
                int col = i % _gridColumns;
                int row = i / _gridColumns;

                int itemX = _contentAreaBounds.X + (col * (GridItemWidth + GridSpacing));
                int itemY = startY + (row * (GridItemHeight + GridSpacing));

                var bounds = new Rectangle(itemX, itemY, GridItemWidth, GridItemHeight);
                _gridItems.Add(new ButtonGridItem(sortedList[i], bounds, row, col));
            }

            UpdateStats();
            CalculateContentHeight();
        }

        private void UpdateStats()
        {
            if (_allButtonsCache.Count == 0)
            {
                _stats = new DashboardStats();
                return;
            }

            _stats = new DashboardStats
            {
                TotalButtons = _allButtonsCache.Count,
                VisibleButtons = _allButtonsCache.Count(b => b.ShouldShow()),
                HiddenButtons = _allButtonsCache.Count(b => !b.ShouldShow()),
                TotalMods = _allButtonsCache.Select(b => b.ModId).Distinct().Count(),
                CategoryCounts = _allButtonsCache
                    .GroupBy(b => b.Category)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private void CalculateContentHeight()
        {
            int rows = (_gridItems.Count + _gridColumns - 1) / _gridColumns;
            _totalContentHeight = 150 + (rows * (GridItemHeight + GridSpacing)) + 50;
            _maxScrollOffset = Math.Max(0, _totalContentHeight - _contentAreaBounds.Height);
            _scrollOffset = Math.Clamp(_scrollOffset, 0, _maxScrollOffset);
        }

        #endregion

        #region Tab Switching

        private void SwitchTab(MenuTab newTab)
        {
            if (_currentTab == newTab)
                return;

            _currentTab = newTab;
            _scrollOffset = 0;
            _navScrollOffset = 0;

            InitializeNavItems();

            // Refresh content based on tab
            if (newTab == MenuTab.Dashboard)
            {
                RefreshGridItems();
            }

            Game1.playSound("smallSelect");
        }

        #endregion

        #region Update

        public override void update(GameTime time)
        {
            base.update(time);

            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            // Update tab hover
            _hoveredTabIndex = -1;
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (_tabButtons[i].containsPoint(mouseX, mouseY))
                {
                    _hoveredTabIndex = i;
                    break;
                }
            }

            // Update nav items hover
            _hoveredNavItem = null;
            if (_navAreaBounds.Contains(mouseX, mouseY))
            {
                foreach (var navItem in _navItems)
                {
                    var adjusted = new Rectangle(
                        navItem.bounds.X,
                        navItem.bounds.Y - _navScrollOffset,
                        navItem.bounds.Width,
                        navItem.bounds.Height
                    );

                    if (adjusted.Contains(mouseX, mouseY) && navItem.name != "label")
                    {
                        _hoveredNavItem = navItem;
                        break;
                    }
                }
            }

            // Update grid items hover (Dashboard tab only)
            if (_currentTab == MenuTab.Dashboard)
            {
                _hoveredItem = null;
                foreach (var item in _gridItems)
                {
                    var adjusted = new Rectangle(
                        item.bounds.X,
                        item.bounds.Y - _scrollOffset,
                        item.bounds.Width,
                        item.bounds.Height
                    );

                    bool inViewport = adjusted.Bottom > _contentAreaBounds.Y &&
                                      adjusted.Top < _contentAreaBounds.Bottom;

                    if (inViewport &&
                        _contentAreaBounds.Contains(mouseX, mouseY) &&
                        adjusted.Contains(mouseX, mouseY))
                    {
                        _hoveredItem = item;
                    }

                    item.Update(time, mouseX, inViewport ? mouseY + _scrollOffset : -1000);
                }
            }
        }

        #endregion

        #region Input

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (_dragThresholdMet)
            {
                _isDragging = false;
                _dragThresholdMet = false;
                return;
            }

            // Check tab buttons
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (_tabButtons[i].containsPoint(x, y))
                {
                    SwitchTab((MenuTab)i);
                    return;
                }
            }

            // Check navigation items
            if (_navAreaBounds.Contains(x, y))
            {
                HandleNavClick(x, y, playSound);
                return;
            }

            // Check content area (tab-specific)
            if (_contentAreaBounds.Contains(x, y))
            {
                HandleContentClick(x, y, playSound);
                return;
            }
        }

        private void HandleNavClick(int x, int y, bool playSound)
        {
            foreach (var navItem in _navItems)
            {
                var adjusted = new Rectangle(
                    navItem.bounds.X,
                    navItem.bounds.Y - _navScrollOffset,
                    navItem.bounds.Width,
                    navItem.bounds.Height
                );

                if (!adjusted.Contains(x, y) || navItem.name == "label")
                    continue;

                ProcessNavItemClick(navItem, playSound);
                return;
            }
        }

        private void ProcessNavItemClick(ClickableComponent navItem, bool playSound)
        {
            switch (_currentTab)
            {
                case MenuTab.Dashboard:
                    ProcessDashboardNavClick(navItem, playSound);
                    break;
                case MenuTab.Settings:
                    ProcessSettingsNavClick(navItem, playSound);
                    break;
                case MenuTab.Debug:
                    ProcessDebugNavClick(navItem, playSound);
                    break;
                case MenuTab.About:
                    ProcessAboutNavClick(navItem, playSound);
                    break;
            }
        }

        private void ProcessDashboardNavClick(ClickableComponent navItem, bool playSound)
        {
            // Filters
            if (navItem.name == "filter_all")
            {
                _activeFilter = null;
                RefreshGridItems();
                if (playSound) Game1.playSound("smallSelect");
                return;
            }

            if (navItem.name.StartsWith("filter_"))
            {
                string categoryName = navItem.name.Replace("filter_", "");
                if (Enum.TryParse<KeyCategory>(categoryName, out var category))
                {
                    _activeFilter = category;
                    RefreshGridItems();
                    if (playSound) Game1.playSound("smallSelect");
                }
                return;
            }

            // Sort
            if (navItem.name.StartsWith("sort_"))
            {
                string sortName = navItem.name.Replace("sort_", "");
                if (Enum.TryParse<SortMode>(sortName, out var mode))
                {
                    _currentSort = mode;
                    RefreshGridItems();
                    if (playSound) Game1.playSound("smallSelect");
                }
                return;
            }

            // Actions
            if (navItem.name == "action_refresh")
            {
                RefreshGridItems();
                if (playSound) Game1.playSound("newArtifact");
                return;
            }

            if (navItem.name == "action_reset")
            {
                _registry.ResetAllStates();
                RefreshGridItems();
                if (playSound) Game1.playSound("coin");
                Game1.addHUDMessage(new HUDMessage("All button states reset!", HUDMessage.achievement_type));
                return;
            }
        }

        private void ProcessSettingsNavClick(ClickableComponent navItem, bool playSound)
        {
            switch (navItem.name)
            {
                case "setting_button_size":
                    // Cycle button size (e.g., 48 → 56 → 64 → 48)
                    _config.MenuButtonSize = _config.MenuButtonSize switch
                    {
                        48 => 56,
                        56 => 64,
                        64 => 72,
                        _ => 48
                    };
                    InitializeNavItems(); // Refresh to show new value
                    if (playSound) Game1.playSound("smallSelect");
                    break;

                case "setting_show_labels":
                    _config.ShowButtonLabels = !_config.ShowButtonLabels;
                    InitializeNavItems();
                    if (playSound) Game1.playSound("smallSelect");
                    break;

                case "setting_auto_hide":
                    _config.AutoHideInEvents = !_config.AutoHideInEvents;
                    InitializeNavItems();
                    if (playSound) Game1.playSound("smallSelect");
                    break;

                case "setting_drag_indicator":
                    _config.DragShowIndicator = !_config.DragShowIndicator;
                    InitializeNavItems();
                    if (playSound) Game1.playSound("smallSelect");
                    break;

                case "setting_reset_pos":
                    // TODO: Reset FAB position
                    if (playSound) Game1.playSound("coin");
                    Game1.addHUDMessage(new HUDMessage("FAB position reset!", HUDMessage.achievement_type));
                    break;

                case "setting_verbose":
                    _config.DebugVerboseLogging = !_config.DebugVerboseLogging;
                    InitializeNavItems();
                    if (playSound) Game1.playSound("smallSelect");
                    break;
            }

            // Save config after changes
            StaticReferenceHolder.Helper.WriteConfig(_config);
        }

        private void ProcessDebugNavClick(ClickableComponent navItem, bool playSound)
        {
            switch (navItem.name)
            {
                case "debug_refresh":
                    RefreshGridItems();
                    InitializeNavItems();
                    if (playSound) Game1.playSound("newArtifact");
                    break;

                case "debug_clear":
                    // Warning: This clears ALL buttons!
                    // In real implementation, add confirmation dialog
                    if (playSound) Game1.playSound("trashcan");
                    Game1.addHUDMessage(new HUDMessage("This action is disabled in release.", HUDMessage.error_type));
                    break;

                case "debug_diagnostics":
                    string diagnostics = _registry.GetDiagnostics();
                    _monitor?.Log(diagnostics, LogLevel.Info);
                    if (playSound) Game1.playSound("coin");
                    Game1.addHUDMessage(new HUDMessage("Diagnostics logged to SMAPI console.", HUDMessage.newQuest_type));
                    break;

                case "debug_export_log":
                    ExportButtonsToLog();
                    if (playSound) Game1.playSound("coin");
                    break;
            }
        }

        private void ProcessAboutNavClick(ClickableComponent navItem, bool playSound)
        {
            // In real implementation, open URLs
            switch (navItem.name)
            {
                case "about_nexus":
                case "about_github":
                case "about_discord":
                case "about_report":
                case "about_donate":
                    if (playSound) Game1.playSound("smallSelect");
                    Game1.addHUDMessage(new HUDMessage("Links will be implemented.", HUDMessage.newQuest_type));
                    break;
            }
        }

        private void HandleContentClick(int x, int y, bool playSound)
        {
            if (_currentTab != MenuTab.Dashboard)
                return;

            foreach (var item in _gridItems)
            {
                var adjusted = new Rectangle(
                    item.bounds.X,
                    item.bounds.Y - _scrollOffset,
                    item.bounds.Width,
                    item.bounds.Height
                );

                if (!adjusted.Contains(x, y))
                    continue;

                if (adjusted.Y < _contentAreaBounds.Y ||
                    adjusted.Bottom > _contentAreaBounds.Bottom)
                    continue;

                var button = item.ButtonData;

                if (button.CanPress())
                {
                    bool success = _registry.TriggerButton(button.UniqueId, isProgrammatic: false, logAction: true);
                    if (playSound) Game1.playSound(success ? "coin" : "cancel");
                }
                else
                {
                    if (playSound) Game1.playSound("cancel");
                }

                return;
            }
        }

        private void ExportButtonsToLog()
        {
            _monitor?.Log("═══ BUTTON REGISTRY EXPORT ═══", LogLevel.Info);

            foreach (var button in _allButtonsCache)
            {
                _monitor?.Log(
                    $"[{button.Category}] {button.DisplayName} ({button.UniqueId}) " +
                    $"- Mod: {button.ModId}, Type: {button.Type}, Priority: {button.Priority}",
                    LogLevel.Info
                );
            }

            _monitor?.Log($"═══ Total: {_allButtonsCache.Count} buttons ═══", LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage("Exported to SMAPI console.", HUDMessage.newQuest_type));
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            // Scroll content area
            if (_contentAreaBounds.Contains(mouseX, mouseY) && _maxScrollOffset > 0)
            {
                _scrollOffset = Math.Clamp(
                    _scrollOffset - Math.Sign(direction) * ScrollSpeed,
                    0,
                    _maxScrollOffset
                );
                return;
            }

            // Scroll nav area
            if (_navAreaBounds.Contains(mouseX, mouseY) && _navMaxScrollOffset > 0)
            {
                _navScrollOffset = Math.Clamp(
                    _navScrollOffset - Math.Sign(direction) * ScrollSpeed,
                    0,
                    _navMaxScrollOffset
                );
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);

            if (!_contentAreaBounds.Contains(x, y) || _maxScrollOffset <= 0)
                return;

            if (!_isDragging)
            {
                _isDragging = true;
                _dragStartPosition = new Vector2(x, y);
                _lastTouchY = y;
                _dragThresholdMet = false;
            }
            else
            {
                if (!_dragThresholdMet)
                {
                    float distance = Vector2.Distance(_dragStartPosition, new Vector2(x, y));
                    if (distance >= DragThreshold)
                    {
                        _dragThresholdMet = true;
                    }
                }

                if (_dragThresholdMet)
                {
                    int delta = _lastTouchY - y;
                    _scrollOffset = Math.Clamp(_scrollOffset + delta, 0, _maxScrollOffset);
                    _lastTouchY = y;
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            _isDragging = false;
            _dragThresholdMet = false;
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

            switch (key)
            {
                case Keys.Escape:
                    exitThisMenu();
                    break;

                case Keys.Down:
                case Keys.S:
                    _scrollOffset = Math.Clamp(_scrollOffset + ScrollSpeed, 0, _maxScrollOffset);
                    break;

                case Keys.Up:
                case Keys.W:
                    _scrollOffset = Math.Clamp(_scrollOffset - ScrollSpeed, 0, _maxScrollOffset);
                    break;

                case Keys.Home:
                    _scrollOffset = 0;
                    break;

                case Keys.End:
                    _scrollOffset = _maxScrollOffset;
                    break;

                case Keys.F5:
                    RefreshGridItems();
                    InitializeNavItems();
                    Game1.playSound("newArtifact");
                    break;

                case Keys.Tab:
                    // Cycle through tabs
                    int nextTab = ((int)_currentTab + 1) % _tabLabels.Count;
                    SwitchTab((MenuTab)nextTab);
                    break;

                case Keys.D1:
                    SwitchTab(MenuTab.Dashboard);
                    break;
                case Keys.D2:
                    SwitchTab(MenuTab.Settings);
                    break;
                case Keys.D3:
                    SwitchTab(MenuTab.Debug);
                    break;
                case Keys.D4:
                    SwitchTab(MenuTab.About);
                    break;
            }
        }

        #endregion

        #region Drawing

        public override void draw(SpriteBatch b)
        {
            // Dim background
            b.Draw(
                Game1.fadeToBlackRect,
                Game1.graphics.GraphicsDevice.Viewport.Bounds,
                Color.Black * 0.75f
            );

            // Main menu box
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

            DrawLeftPanel(b);
            DrawRightPanel(b);

            // Draw scrollbar if needed
            if (_currentTab == MenuTab.Dashboard && _maxScrollOffset > 0)
            {
                DrawScrollbar(b, _contentAreaBounds, _scrollOffset, _maxScrollOffset, _totalContentHeight);
            }

            // Close button
            base.draw(b);

            // Tooltips (last!)
            DrawTooltips(b);

            // Mouse cursor
            drawMouse(b);
        }

        private void DrawLeftPanel(SpriteBatch b)
        {
            // Panel background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                _leftPanelBounds.X - 4,
                _leftPanelBounds.Y - 4,
                _leftPanelBounds.Width + 8,
                _leftPanelBounds.Height + 8,
                Color.White * 0.5f,
                1f,
                drawShadow: false
            );

            // Draw tab buttons
            DrawTabButtons(b);

            // Draw divider
            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    _leftPanelBounds.X + PanelPadding,
                    _tabAreaBounds.Bottom,
                    _leftPanelBounds.Width - PanelPadding * 2,
                    2
                ),
                Game1.textColor * 0.3f
            );

            // Draw navigation items with clipping
            DrawNavItemsWithClipping(b);
        }

        private void DrawTabButtons(SpriteBatch b)
        {
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            for (int i = 0; i < _tabButtons.Count; i++)
            {
                var tab = _tabButtons[i];
                MenuTab tabEnum = (MenuTab)i;
                bool isActive = _currentTab == tabEnum;
                bool isHovered = _hoveredTabIndex == i;

                // Background
                //Color bgColor;
                if (isActive)
                {
                    // Active tab - Border tebal dengan berwara hijau
                    IClickableMenu.drawTextureBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),  // Diambil daei bawaan stardew valley
                        tab.bounds.X,
                        tab.bounds.Y,
                        tab.bounds.Width,
                        tab.bounds.Height,
                        new Color(80, 120, 80),  // Hijau gelap
                        1f,
                        drawShadow: false
                    );
                }
                else if (isHovered)
                {
                    // Hovered tab - Border dengan highlight
                    IClickableMenu.drawTextureBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        tab.bounds.X,
                        tab.bounds.Y,
                        tab.bounds.Width,
                        tab.bounds.Height,
                        Color.White * 0.4f,
                        1f,
                        drawShadow: false
                    );
                }
                else
                {
                    // Normal tab - Border subtle
                    IClickableMenu.drawTextureBox(
                        b,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        tab.bounds.X,
                        tab.bounds.Y,
                        tab.bounds.Width,
                        tab.bounds.Height,
                        Color.White * 0.15f,
                        1f,
                        drawShadow: false
                    );
                }

                //b.Draw(Game1.staminaRect, tab.bounds, bgColor);

                // Active indicator (left border)
                if (isActive)
                {
                    b.Draw(
                        Game1.staminaRect,
                        new Rectangle(tab.bounds.X, tab.bounds.Y, 4, tab.bounds.Height),
                        Color.LimeGreen
                    );
                }

                // Icon
                string icon = _tabIcons.GetValueOrDefault(tabEnum, "•");
                var iconSize = Game1.smallFont.MeasureString(icon);
                b.DrawString(
                    Game1.smallFont,
                    icon,
                    new Vector2(tab.bounds.X + 12, tab.bounds.Y + (tab.bounds.Height - iconSize.Y) / 2),
                    isActive ? Color.White : Color.Gray
                );

                // Label
                string label = _tabLabels[tabEnum];
                var labelSize = Game1.smallFont.MeasureString(label);
                Utility.drawTextWithShadow(
                    b,
                    label,
                    Game1.smallFont,
                    new Vector2(
                        tab.bounds.X + 40,
                        tab.bounds.Y + (tab.bounds.Height - labelSize.Y) / 2
                    ),
                    isActive ? Color.White : (isHovered ? Color.LightGray : Color.Gray)
                );

                // Keyboard shortcut hint
                string shortcut = $"{i + 1}";
                var shortcutSize = Game1.tinyFont.MeasureString(shortcut);
                b.DrawString(
                    Game1.tinyFont,
                    shortcut,
                    new Vector2(
                        tab.bounds.Right - shortcutSize.X - 8,
                        tab.bounds.Y + (tab.bounds.Height - shortcutSize.Y) / 2
                    ),
                    Color.Gray * 0.5f
                );
            }
        }

        private void DrawNavItemsWithClipping(SpriteBatch b)
        {
            var origScissor = b.GraphicsDevice.ScissorRectangle;
            var origRasterizer = b.GraphicsDevice.RasterizerState;

            b.End();

            b.GraphicsDevice.ScissorRectangle = _navAreaBounds;
            var scissorState = new RasterizerState { ScissorTestEnable = true };

            b.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                scissorState
            );

            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            foreach (var navItem in _navItems)
            {
                var adjusted = new Rectangle(
                    navItem.bounds.X,
                    navItem.bounds.Y - _navScrollOffset,
                    navItem.bounds.Width,
                    navItem.bounds.Height
                );

                if (adjusted.Bottom < _navAreaBounds.Y || adjusted.Top > _navAreaBounds.Bottom)
                    continue;

                DrawNavItem(b, navItem, adjusted, mouseX, mouseY);
            }

            b.End();

            b.GraphicsDevice.ScissorRectangle = origScissor;
            b.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                origRasterizer
            );
        }

        private void DrawNavItem(SpriteBatch b, ClickableComponent navItem, Rectangle bounds, int mouseX, int mouseY)
        {
            bool isLabel = navItem.name == "label";
            bool isHovered = bounds.Contains(mouseX, mouseY) && !isLabel;
            bool isActive = IsNavItemActive(navItem);

            // Background
            Color bgColor;
            if (isLabel)
            {
                bgColor = Color.Transparent;
            }
            else if (isActive)
            {
                bgColor = new Color(60, 90, 60) * 0.9f;
            }
            else if (isHovered)
            {
                bgColor = Color.White * 0.12f;
            }
            else
            {
                bgColor = Color.Transparent;
            }

            if (bgColor != Color.Transparent)
            {
                b.Draw(Game1.staminaRect, bounds, bgColor);
            }

            // Active indicator
            if (isActive && !isLabel)
            {
                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(bounds.X, bounds.Y, 3, bounds.Height),
                    Color.LimeGreen
                );
            }

            // Text
            Color textColor = isLabel
                ? Game1.textColor * 0.5f
                : (isActive ? Color.White : (isHovered ? Color.LightGray : Game1.textColor * 0.9f));

            float scale = isLabel ? 0.85f : 0.95f;

            var textSize = Game1.smallFont.MeasureString(navItem.label) * scale;
            var textPos = new Vector2(
                bounds.X + (isLabel ? (bounds.Width - textSize.X) / 2 : 14),
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            Utility.drawTextWithShadow(
                b,
                navItem.label,
                Game1.smallFont,
                textPos,
                textColor,
                scale
            );

            // Category indicator (Dashboard tab, filter items)
            if (_currentTab == MenuTab.Dashboard &&
                navItem.name.StartsWith("filter_") &&
                navItem.name != "filter_all")
            {
                DrawCategoryIndicator(b, navItem, bounds);
            }
        }

        private void DrawCategoryIndicator(SpriteBatch b, ClickableComponent navItem, Rectangle bounds)
        {
            string categoryName = navItem.name.Replace("filter_", "");
            if (!Enum.TryParse<KeyCategory>(categoryName, out var category))
                return;

            // Color dot
            Color catColor = category.GetThemeColor();
            b.Draw(
                Game1.staminaRect,
                new Rectangle(bounds.Right - 22, bounds.Y + (bounds.Height - 10) / 2, 10, 10),
                catColor
            );

            // Count
            if (_stats.CategoryCounts != null &&
                _stats.CategoryCounts.TryGetValue(category, out int count))
            {
                string countText = count.ToString();
                var countSize = Game1.tinyFont.MeasureString(countText);
                b.DrawString(
                    Game1.tinyFont,
                    countText,
                    new Vector2(bounds.Right - 35 - countSize.X, bounds.Y + (bounds.Height - countSize.Y) / 2),
                    Game1.textColor * 0.5f
                );
            }
        }

        private bool IsNavItemActive(ClickableComponent navItem)
        {
            if (_currentTab == MenuTab.Dashboard)
            {
                if (navItem.name == "filter_all" && !_activeFilter.HasValue)
                    return true;

                if (navItem.name.StartsWith("filter_") && _activeFilter.HasValue)
                {
                    string categoryName = navItem.name.Replace("filter_", "");
                    return categoryName == _activeFilter.Value.ToString();
                }

                if (navItem.name.StartsWith("sort_"))
                {
                    string sortName = navItem.name.Replace("sort_", "");
                    return sortName == _currentSort.ToString();
                }
            }

            return false;
        }

        private void DrawRightPanel(SpriteBatch b)
        {
            // Panel background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                _rightPanelBounds.X - 4,
                _rightPanelBounds.Y - 4,
                _rightPanelBounds.Width + 8,
                _rightPanelBounds.Height + 8,
                Color.White * 0.4f,
                1f,
                drawShadow: false
            );

            // Title
            string title = GetPanelTitle();
            Utility.drawTextWithShadow(
                b,
                title,
                Game1.dialogueFont,
                new Vector2(_rightPanelBounds.X + PanelPadding, _rightPanelBounds.Y + PanelPadding),
                Game1.textColor
            );

            // Divider
            b.Draw(
                Game1.staminaRect,
                new Rectangle(
                    _rightPanelBounds.X + PanelPadding,
                    _rightPanelBounds.Y + PanelPadding + 45,
                    _rightPanelBounds.Width - PanelPadding * 2,
                    4
                ),
                Game1.textColor * 0.3f
            );

            // Content based on active tab
            switch (_currentTab)
            {
                case MenuTab.Dashboard:
                    DrawDashboardContent(b);
                    break;
                case MenuTab.Settings:
                    DrawSettingsContent(b);
                    break;
                case MenuTab.Debug:
                    DrawDebugContent(b);
                    break;
                case MenuTab.About:
                    DrawAboutContent(b);
                    break;
            }
        }

        private string GetPanelTitle()
        {
            return _currentTab switch
            {
                MenuTab.Dashboard when _activeFilter.HasValue =>
                    $"Dashboard - {_activeFilter.Value.GetDisplayName()}",
                MenuTab.Dashboard => "Dashboard - All Buttons",
                MenuTab.Settings => "Settings",
                MenuTab.Debug => "Debug Tools",
                MenuTab.About => "About AddonsMobile",
                _ => _currentTab.ToString()
            };
        }

        private void DrawDashboardContent(SpriteBatch b)
        {
            var origScissor = b.GraphicsDevice.ScissorRectangle;
            var origRasterizer = b.GraphicsDevice.RasterizerState;

            b.End();

            b.GraphicsDevice.ScissorRectangle = _contentAreaBounds;
            var scissorState = new RasterizerState { ScissorTestEnable = true };

            b.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                scissorState
            );

            int contentY = _contentAreaBounds.Y - _scrollOffset;

            // Stats header
            DrawStatsHeader(b, _contentAreaBounds.X, contentY);

            // Section header
            string sectionTitle = _activeFilter.HasValue
                ? $"{_activeFilter.Value.GetDisplayName()} ({_gridItems.Count})"
                : $"All Buttons ({_gridItems.Count})";
            DrawSectionHeader(b, sectionTitle, _contentAreaBounds.X, contentY + 100);

            // Grid items
            DrawGridItems(b);

            b.End();

            b.GraphicsDevice.ScissorRectangle = origScissor;
            b.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                origRasterizer
            );
        }

        private void DrawSettingsContent(SpriteBatch b)
        {
            int y = _contentAreaBounds.Y + 20;
            int x = _contentAreaBounds.X + 20;

            DrawSectionInContent(b, "Current Configuration", ref y, x);

            string[] configLines = {
                $"Button Size: {_config.MenuButtonSize}px",
                $"Show Button Labels: {(_config.ShowButtonLabels ? "Enabled" : "Disabled")}",
                $"Auto-Hide in Events: {(_config.AutoHideInEvents ? "Enabled" : "Disabled")}",
                $"Show Drag Indicator: {(_config.DragShowIndicator ? "Enabled" : "Disabled")}",
                $"Verbose Logging: {(_config.DebugVerboseLogging ? "Enabled" : "Disabled")}",
            };

            foreach (var line in configLines)
            {
                Utility.drawTextWithShadow(b, "• " + line, Game1.smallFont, new Vector2(x, y), Game1.textColor);
                y += 28;
            }

            y += 20;
            DrawSectionInContent(b, "Instructions", ref y, x);

            Utility.drawTextWithShadow(
                b,
                "Use the navigation panel on the left\nto adjust settings. Changes are\nsaved automatically.",
                Game1.smallFont,
                new Vector2(x, y),
                Game1.textColor * 0.8f
            );
        }

        private void DrawDebugContent(SpriteBatch b)
        {
            int y = _contentAreaBounds.Y + 20;
            int x = _contentAreaBounds.X + 20;

            DrawSectionInContent(b, "Registry Status", ref y, x);

            string[] debugLines = {
                $"Total Registered: {_registry.Count}",
                $"Registered Mods: {_registry.ModCount}",
                $"Active Categories: {_registry.ActiveCategories.Count()}",
                $"Visible Buttons: {_stats.VisibleButtons}",
                $"Hidden Buttons: {_stats.HiddenButtons}",
            };

            foreach (var line in debugLines)
            {
                Utility.drawTextWithShadow(b, "• " + line, Game1.smallFont, new Vector2(x, y), Game1.textColor);
                y += 28;
            }

            y += 20;
            DrawSectionInContent(b, "Categories Breakdown", ref y, x);

            if (_stats.CategoryCounts != null)
            {
                foreach (var (category, count) in _stats.CategoryCounts.OrderBy(kv => kv.Key.GetSortOrder()))
                {
                    Color catColor = category.GetThemeColor();
                    b.Draw(Game1.staminaRect, new Rectangle(x, y + 5, 12, 12), catColor);
                    Utility.drawTextWithShadow(
                        b,
                        $"  {category.GetDisplayName()}: {count}",
                        Game1.smallFont,
                        new Vector2(x + 16, y),
                        Game1.textColor
                    );
                    y += 26;
                }
            }
        }

        private void DrawAboutContent(SpriteBatch b)
        {
            int y = _contentAreaBounds.Y + 20;
            int x = _contentAreaBounds.X + 20;

            // Title
            Utility.drawTextWithShadow(
                b,
                "AddonsMobile",
                Game1.dialogueFont,
                new Vector2(x, y),
                new Color(100, 200, 100)
            );
            y += 50;

            // Version
            string version = StaticReferenceHolder.Manifest?.Version?.ToString() ?? "Unknown";
            Utility.drawTextWithShadow(b, $"Version: {version}", Game1.smallFont, new Vector2(x, y), Game1.textColor);
            y += 30;

            // Author
            string author = StaticReferenceHolder.Manifest?.Author ?? "Unknown";
            Utility.drawTextWithShadow(b, $"Author: {author}", Game1.smallFont, new Vector2(x, y), Game1.textColor);
            y += 40;

            DrawSectionInContent(b, "Description", ref y, x);

            string description = StaticReferenceHolder.Manifest?.Description ?? "Mobile-friendly button system for Stardew Valley mods.";
            Utility.drawTextWithShadow(b, description, Game1.smallFont, new Vector2(x, y), Game1.textColor * 0.9f);
            y += 60;

            DrawSectionInContent(b, "Features", ref y, x);

            string[] features = {
                "• Floating Action Button (FAB) for quick access",
                "• Radial menu for registered buttons",
                "• Support for Momentary, Toggle, and Hold buttons",
                "• Category-based organization",
                "• Full API for mod developers"
            };

            foreach (var feature in features)
            {
                Utility.drawTextWithShadow(b, feature, Game1.smallFont, new Vector2(x, y), Game1.textColor * 0.8f);
                y += 26;
            }
        }

        private void DrawSectionInContent(SpriteBatch b, string title, ref int y, int x)
        {
            Utility.drawTextWithShadow(b, title, Game1.smallFont, new Vector2(x, y), Color.Black);
            y += 24;
            b.Draw(Game1.staminaRect, new Rectangle(x, y, 200, 2), Color.Gold * 0.7f);
            y += 12;
        }

        private void DrawStatsHeader(SpriteBatch b, int x, int y)
        {
            int bgWidth = _contentAreaBounds.Width - 20;
            var bg = new Rectangle(x, y, bgWidth, 85);
            b.Draw(Game1.staminaRect, bg, Color.White * 0.1f);

            int boxWidth = (bg.Width - 50) / 4;
            int boxHeight = 60;
            int boxY = y + 12;
            int spacing = 10;

            DrawStatBox(b, x + spacing, boxY, boxWidth, boxHeight,
                "Total", _stats.TotalButtons.ToString(), new Color(100, 150, 220));

            DrawStatBox(b, x + spacing + boxWidth + spacing, boxY, boxWidth, boxHeight,
                "Visible", _stats.VisibleButtons.ToString(), new Color(100, 200, 100));

            DrawStatBox(b, x + spacing + (boxWidth + spacing) * 2, boxY, boxWidth, boxHeight,
                "Hidden", _stats.HiddenButtons.ToString(), new Color(200, 100, 100));

            DrawStatBox(b, x + spacing + (boxWidth + spacing) * 3, boxY, boxWidth, boxHeight,
                "Mods", _stats.TotalMods.ToString(), new Color(180, 130, 200));
        }

        private void DrawStatBox(SpriteBatch b, int x, int y, int w, int h, string label, string value, Color color)
        {
            IClickableMenu.drawTextureBox(
                b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                x, y, w, h, color * 0.3f, 1f, false
            );

            var valSize = Game1.dialogueFont.MeasureString(value);
            Utility.drawTextWithShadow(b, value, Game1.dialogueFont,
                new Vector2(x + (w - valSize.X) / 2, y + 8), color);

            var lblSize = Game1.smallFont.MeasureString(label);
            Utility.drawTextWithShadow(b, label, Game1.smallFont,
                new Vector2(x + (w - lblSize.X) / 2, y + h - lblSize.Y - 6), Game1.textColor * 0.7f);
        }

        private void DrawSectionHeader(SpriteBatch b, string text, int x, int y)
        {
            Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(x + 10, y + 8), Game1.textColor);
            var size = Game1.smallFont.MeasureString(text);
            b.Draw(Game1.staminaRect, new Rectangle(x + 10, y + (int)size.Y + 10, (int)size.X, 2), Game1.textColor * 0.3f);
        }

        private void DrawGridItems(SpriteBatch b)
        {
            if (_gridItems.Count == 0)
            {
                string emptyText = _activeFilter.HasValue
                    ? $"No buttons in {_activeFilter.Value.GetDisplayName()} category."
                    : "No buttons registered.\nPress F5 to refresh.";

                Utility.drawTextWithShadow(b, emptyText, Game1.smallFont,
                    new Vector2(_contentAreaBounds.X + 20, _contentAreaBounds.Y + 180 - _scrollOffset),
                    Game1.textColor * 0.8f);
                return;
            }

            foreach (var item in _gridItems)
            {
                item.Draw(b, _scrollOffset);
            }
        }

        private void DrawScrollbar(SpriteBatch b, Rectangle area, int offset, int maxOffset, int totalHeight)
        {
            int scrollbarWidth = 12;
            int scrollbarX = area.Right - scrollbarWidth;
            int scrollbarHeight = area.Height;

            b.Draw(Game1.staminaRect,
                new Rectangle(scrollbarX, area.Y, scrollbarWidth, scrollbarHeight),
                Color.Gray * 0.3f);

            float ratio = (float)area.Height / totalHeight;
            int thumbHeight = Math.Max(30, (int)(scrollbarHeight * ratio));
            float scrollRatio = maxOffset > 0 ? (float)offset / maxOffset : 0;
            int thumbY = area.Y + (int)((scrollbarHeight - thumbHeight) * scrollRatio);

            b.Draw(Game1.staminaRect,
                new Rectangle(scrollbarX + 2, thumbY, scrollbarWidth - 4, thumbHeight),
                Color.White * 0.8f);
        }

        private void DrawTooltips(SpriteBatch b)
        {
            // Grid item tooltip
            if (_hoveredItem != null && _currentTab == MenuTab.Dashboard)
            {
                string tooltipText = _hoveredItem.GetTooltipText();
                if (!string.IsNullOrEmpty(tooltipText))
                {
                    ButtonInfoTooltip.DrawAtCursor(b, tooltipText);
                }
                return;
            }

            // Tab tooltip
            if (_hoveredTabIndex >= 0)
            {
                MenuTab tab = (MenuTab)_hoveredTabIndex;
                string tooltip = tab switch
                {
                    MenuTab.Dashboard => "View and interact with registered buttons",
                    MenuTab.Settings => "Configure mod settings",
                    MenuTab.Debug => "Developer and debugging tools",
                    MenuTab.About => "Information about this mod",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(tooltip))
                {
                    IClickableMenu.drawHoverText(b, tooltip, Game1.smallFont);
                }
                return;
            }

            // Nav item tooltip
            if (_hoveredNavItem != null)
            {
                string tooltip = GetNavItemTooltip(_hoveredNavItem);
                if (!string.IsNullOrEmpty(tooltip))
                {
                    IClickableMenu.drawHoverText(b, tooltip, Game1.smallFont);
                }
            }
        }

        private string GetNavItemTooltip(ClickableComponent navItem)
        {
            if (navItem.name == "filter_all")
                return "Show all registered buttons";

            if (navItem.name.StartsWith("filter_"))
            {
                string categoryName = navItem.name.Replace("filter_", "");
                if (Enum.TryParse<KeyCategory>(categoryName, out var category))
                {
                    return category.GetDescription();
                }
            }

            if (navItem.name.StartsWith("sort_"))
                return $"Sort buttons by {navItem.label}";

            return navItem.name switch
            {
                "action_refresh" => "Reload button list from registry (F5)",
                "action_reset" => "Reset all toggle and hold button states",
                "setting_button_size" => "Change the size of buttons in FAB menu",
                "setting_show_labels" => "Toggle button label visibility",
                "setting_auto_hide" => "Hide FAB during events/cutscenes",
                "setting_drag_indicator" => "Show indicator while dragging FAB",
                "setting_reset_pos" => "Reset FAB to default position",
                "setting_verbose" => "Enable detailed logging to SMAPI console",
                "debug_refresh" => "Force refresh registry data",
                "debug_clear" => "Remove all registered buttons (dangerous!)",
                "debug_diagnostics" => "Print registry diagnostics to console",
                "debug_export_log" => "Export button list to SMAPI log",
                _ => ""
            };
        }

        #endregion

        #region Resize

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            CalculateMenuDimensions();
            InitializePanels();
            SetupCloseButton();
            InitializeTabButtons();
            InitializeNavItems();
            CalculateGridColumns();
            RefreshGridItems();
        }

        #endregion

        #region Cleanup

        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
            Game1.playSound("bigDeSelect");
        }

        #endregion
    }

    /// <summary>
    /// Stats untuk dashboard.
    /// </summary>
    public class DashboardStats
    {
        public int TotalButtons { get; set; }
        public int VisibleButtons { get; set; }
        public int HiddenButtons { get; set; }
        public int TotalMods { get; set; }
        public Dictionary<KeyCategory, int>? CategoryCounts { get; set; }
    }
}