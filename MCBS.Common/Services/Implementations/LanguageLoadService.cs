using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class LanguageLoadService : ILanguageLoadService
    {
        public LanguageLoadService(ILanguageTemplateParser languageTemplateParser)
        {
            ArgumentNullException.ThrowIfNull(languageTemplateParser, nameof(languageTemplateParser));

            _languageTemplateParser = languageTemplateParser;
        }

        private readonly ILanguageTemplateParser _languageTemplateParser;

        public LanguageManager LoadLanguage(ResourceEntryManager resourceEntryManager, string language)
        {
            ArgumentNullException.ThrowIfNull(resourceEntryManager, nameof(resourceEntryManager));
            ArgumentException.ThrowIfNullOrEmpty(language, nameof(language));

            if (!resourceEntryManager.LanguageEntries.ContainsKey(language))
                throw new ArgumentException($"The specified language '{language}' does not exist in the resource entry manager.", nameof(language));

            Dictionary<string, Dictionary<string, LanguageTemplate>> languageTemplates = [];
            foreach (var item in resourceEntryManager.LanguageEntries)
            {
                string languageName = item.Key;
                Dictionary<string, LanguageTemplate> templates = _languageTemplateParser.ParseLanguageTemplates(item.Value);

                languageTemplates.Add(languageName, templates);
            }

            return LanguageManager.LoadInstance(new(languageTemplates, language));
        }
    }
}
