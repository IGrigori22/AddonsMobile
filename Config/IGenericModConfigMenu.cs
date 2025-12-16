using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddonsMobile.Config
{
    public interface IGenericModConfigMenuApi
    {
        /// <summary>Register a mod whose config can be edited through the UI.</summary>
        void Register(
            IManifest mod,
            Action reset,
            Action save,
            bool titleScreenOnly = false  // ← INI YANG HILANG!
        );

        /// <summary>Add a section title.</summary>
        void AddSectionTitle(
            IManifest mod,
            Func<string> text,
            Func<string> tooltip = null
        );

        /// <summary>Add a paragraph of text.</summary>
        void AddParagraph(
            IManifest mod,
            Func<string> text
        );

        /// <summary>Add a boolean option.</summary>
        void AddBoolOption(
            IManifest mod,
            Func<bool> getValue,
            Action<bool> setValue,
            Func<string> name,
            Func<string> tooltip = null,
            string fieldId = null
        );

        /// <summary>Add an integer option.</summary>
        void AddNumberOption(
            IManifest mod,
            Func<int> getValue,
            Action<int> setValue,
            Func<string> name,
            Func<string> tooltip = null,
            int? min = null,
            int? max = null,
            int? interval = null,
            Func<int, string> formatValue = null,
            string fieldId = null
        );

        /// <summary>Add a float option.</summary>
        void AddNumberOption(
            IManifest mod,
            Func<float> getValue,
            Action<float> setValue,
            Func<string> name,
            Func<string> tooltip = null,
            float? min = null,
            float? max = null,
            float? interval = null,
            Func<float, string> formatValue = null,
            string fieldId = null
        );

        void AddTextOption(
            IManifest mod, 
            Func<string> getValue, 
            Action<string> setValue, 
            Func<string> name, 
            Func<string> tooltip = null, 
            string[] allowedValues = null, 
            Func<string, string> formatAllowedValue = null, 
            string fieldId = null
        );
    }
}
