using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using WPFLocalizeExtension.Engine;

namespace MCBS.WpfApp.Helpers
{
    public static class LanguageHelper
    {
        public const string DEFAULT_LANGUAGE = "zh-CN";

        private static Properties.Settings Settings => Properties.Settings.Default;

        private static LocalizeDictionary LocalizeDictionary => LocalizeDictionary.Instance;

        public static void Initialize()
        {
            string language = Settings.Language;
            try
            {
                LocalizeDictionary.Culture = CultureInfo.GetCultureInfo(language);
            }
            catch
            {
                LocalizeDictionary.Culture = CultureInfo.GetCultureInfo(DEFAULT_LANGUAGE);
            }

            LocalizeDictionary.PropertyChanged += LocalizeDictionary_PropertyChanged;
        }

        public static CultureInfo Culture
        {
            get
            {
                return LocalizeDictionary.Culture;
            }
            set
            {
                if (!LocalizeDictionary.Culture.Equals(value))
                    LocalizeDictionary.Culture = value;
            }
        }

        private static void LocalizeDictionary_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Culture))
            {
                Settings.Language = LocalizeDictionary.Culture.Name;
                Settings.Save();
            }
        }
    }
}
