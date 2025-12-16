using Microsoft.Xna.Framework;

namespace AddonsMobile.Framework
{
    /// <summary>
    /// Kategori untuk mengelompokkan button
    /// </summary>
    public enum KeyCategory
    {
        Menu,
        Farming,
        Tools,
        Cheats,
        Information,
        Social,
        Inventory,
        Teleport,
        Miscellaneous
    }

    /// <summary>
    /// Extension methods untuk KeyCategory
    /// </summary>
    public static class KeyCategoryExtensions
    {
        /// <summary>
        /// Mendapatkan display name yang user-friendly
        /// </summary>
        public static string GetDisplayName(this KeyCategory category)
        {
            return category switch
            {
                KeyCategory.Menu => "Menu & UI",
                KeyCategory.Farming => "Farming",
                KeyCategory.Tools => "Tools",
                KeyCategory.Cheats => "Cheats",
                KeyCategory.Information => "Info",
                KeyCategory.Social => "Social",
                KeyCategory.Inventory => "Inventory",
                KeyCategory.Teleport => "Teleport",
                KeyCategory.Miscellaneous => "Misc",
                _ => category.ToString()
            };
        }

        /// <summary>
        /// Mendapatkan icon source rect dari spritesheet
        /// Asumsi: spritesheet dengan 9 icons, 16x16 px each, horizontal
        /// </summary>
        public static Rectangle GetIconSourceRect(this KeyCategory category, int iconSize = 16)
        {
            int index = (int)category;
            return new Rectangle(index * iconSize, 0, iconSize, iconSize);
        }

        /// <summary>
        /// Mendapatkan warna tema untuk kategori
        /// </summary>
        public static Color GetThemeColor(this KeyCategory category)
        {
            return category switch
            {
                KeyCategory.Menu => new Color(100, 149, 237),       // Cornflower Blue
                KeyCategory.Farming => new Color(34, 139, 34),      // Forest Green
                KeyCategory.Tools => new Color(184, 134, 11),       // Dark Goldenrod
                KeyCategory.Cheats => new Color(220, 20, 60),       // Crimson
                KeyCategory.Information => new Color(65, 105, 225), // Royal Blue
                KeyCategory.Social => new Color(255, 105, 180),     // Hot Pink
                KeyCategory.Inventory => new Color(210, 105, 30),   // Chocolate
                KeyCategory.Teleport => new Color(138, 43, 226),    // Blue Violet
                KeyCategory.Miscellaneous => new Color(128, 128, 128), // Gray
                _ => Color.White
            };
        }

        /// <summary>
        /// Mendapatkan urutan sorting
        /// </summary>
        public static int GetSortOrder(this KeyCategory category)
        {
            return category switch
            {
                KeyCategory.Menu => 0,
                KeyCategory.Inventory => 1,
                KeyCategory.Tools => 2,
                KeyCategory.Farming => 3,
                KeyCategory.Social => 4,
                KeyCategory.Teleport => 5,
                KeyCategory.Information => 6,
                KeyCategory.Cheats => 7,
                KeyCategory.Miscellaneous => 99,
                _ => 50
            };
        }
    }
}