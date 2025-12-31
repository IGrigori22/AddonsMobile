using Microsoft.Xna.Framework;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Kategori untuk mengelompokkan button.
    /// </summary>
    public enum KeyCategory
    {
        Menu = 0,
        Farming = 1,
        Tools = 2,
        Cheats = 3,
        Information = 4,
        Social = 5,
        Inventory = 6,
        Teleport = 7,
        Miscellaneous = 99
    }

    /// <summary>
    /// Metadata untuk kategori.
    /// </summary>
    public class CategoryMetadata
    {
        public KeyCategory Category { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Color ThemeColor { get; set; }
        public int SortOrder { get; set; }
        public Rectangle IconSourceRect { get; set; }
    }

    /// <summary>
    /// Extension methods dan utilities untuk KeyCategory.
    /// </summary>
    public static class KeyCategoryExtensions
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // METADATA REGISTRY
        // ═══════════════════════════════════════════════════════════════════════════

        private static readonly Dictionary<KeyCategory, CategoryMetadata> _metadata = new()
        {
            [KeyCategory.Menu] = new()
            {
                Category = KeyCategory.Menu,
                DisplayName = "Menu & UI",
                Description = "Open menus and UI elements",
                ThemeColor = new Color(100, 149, 237),
                SortOrder = 0,
                IconSourceRect = new Rectangle(0, 0, 16, 16)
            },
            [KeyCategory.Inventory] = new()
            {
                Category = KeyCategory.Inventory,
                DisplayName = "Inventory",
                Description = "Inventory management and item actions",
                ThemeColor = new Color(210, 105, 30),
                SortOrder = 1,
                IconSourceRect = new Rectangle(16, 0, 16, 16)
            },
            [KeyCategory.Tools] = new()
            {
                Category = KeyCategory.Tools,
                DisplayName = "Tools",
                Description = "Tool-related actions",
                ThemeColor = new Color(184, 134, 11),
                SortOrder = 2,
                IconSourceRect = new Rectangle(32, 0, 16, 16)
            },
            [KeyCategory.Farming] = new()
            {
                Category = KeyCategory.Farming,
                DisplayName = "Farming",
                Description = "Farming and harvesting actions",
                ThemeColor = new Color(34, 139, 34),
                SortOrder = 3,
                IconSourceRect = new Rectangle(48, 0, 16, 16)
            },
            [KeyCategory.Social] = new()
            {
                Category = KeyCategory.Social,
                DisplayName = "Social",
                Description = "NPC interactions and relationships",
                ThemeColor = new Color(255, 105, 180),
                SortOrder = 4,
                IconSourceRect = new Rectangle(64, 0, 16, 16)
            },
            [KeyCategory.Teleport] = new()
            {
                Category = KeyCategory.Teleport,
                DisplayName = "Teleport",
                Description = "Fast travel and teleportation",
                ThemeColor = new Color(138, 43, 226),
                SortOrder = 5,
                IconSourceRect = new Rectangle(80, 0, 16, 16)
            },
            [KeyCategory.Information] = new()
            {
                Category = KeyCategory.Information,
                DisplayName = "Information",
                Description = "Display information and stats",
                ThemeColor = new Color(65, 105, 225),
                SortOrder = 6,
                IconSourceRect = new Rectangle(96, 0, 16, 16)
            },
            [KeyCategory.Cheats] = new()
            {
                Category = KeyCategory.Cheats,
                DisplayName = "Cheats",
                Description = "Cheat and debug functions",
                ThemeColor = new Color(220, 20, 60),
                SortOrder = 7,
                IconSourceRect = new Rectangle(112, 0, 16, 16)
            },
            [KeyCategory.Miscellaneous] = new()
            {
                Category = KeyCategory.Miscellaneous,
                DisplayName = "Miscellaneous",
                Description = "Other uncategorized actions",
                ThemeColor = new Color(128, 128, 128),
                SortOrder = 99,
                IconSourceRect = new Rectangle(128, 0, 16, 16)
            }
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // EXTENSION METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Mendapatkan metadata lengkap untuk kategori.
        /// </summary>
        public static CategoryMetadata GetMetadata(this KeyCategory category)
        {
            return _metadata.TryGetValue(category, out var metadata)
                ? metadata
                : _metadata[KeyCategory.Miscellaneous];
        }

        /// <summary>
        /// Mendapatkan display name yang user-friendly.
        /// </summary>
        public static string GetDisplayName(this KeyCategory category)
        {
            return category.GetMetadata().DisplayName;
        }

        /// <summary>
        /// Mendapatkan description.
        /// </summary>
        public static string GetDescription(this KeyCategory category)
        {
            return category.GetMetadata().Description;
        }

        /// <summary>
        /// Mendapatkan icon source rect dari spritesheet.
        /// </summary>
        public static Rectangle GetIconSourceRect(this KeyCategory category)
        {
            return category.GetMetadata().IconSourceRect;
        }

        /// <summary>
        /// Mendapatkan warna tema untuk kategori.
        /// </summary>
        public static Color GetThemeColor(this KeyCategory category)
        {
            return category.GetMetadata().ThemeColor;
        }

        /// <summary>
        /// Mendapatkan urutan sorting.
        /// </summary>
        public static int GetSortOrder(this KeyCategory category)
        {
            return category.GetMetadata().SortOrder;
        }

        /// <summary>
        /// Mendapatkan semua kategori yang terdefinisi.
        /// </summary>
        public static IEnumerable<KeyCategory> GetAllCategories()
        {
            return Enum.GetValues<KeyCategory>()
                .OrderBy(c => c.GetSortOrder());
        }

        /// <summary>
        /// Parse string ke KeyCategory (case-insensitive).
        /// </summary>
        public static bool TryParse(string categoryName, out KeyCategory category)
        {
            if (Enum.TryParse<KeyCategory>(categoryName, ignoreCase: true, out category))
                return true;

            // Try matching by display name
            foreach (var kvp in _metadata)
            {
                if (kvp.Value.DisplayName.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    category = kvp.Key;
                    return true;
                }
            }

            category = KeyCategory.Miscellaneous;
            return false;
        }

        /// <summary>
        /// Validate apakah kategori valid.
        /// </summary>
        public static bool IsValid(this KeyCategory category)
        {
            return Enum.IsDefined(typeof(KeyCategory), category);
        }
    }
}