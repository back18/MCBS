using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Helpers
{
    public class ThemeHelper
    {
        public const string THEME_LIGHT = nameof(ElementTheme.Light);
        public const string THEME_DARK = nameof(ElementTheme.Dark);

        private static Properties.Settings Settings => Properties.Settings.Default;

        public static void Initialize()
        {
            ThemeManager.SetRequestedTheme(App.Current.MainWindow, AppTheme);
        }

        public static ElementTheme ParseTheme(string themeName)
        {
            return themeName switch
            {
                THEME_LIGHT => ElementTheme.Light,
                THEME_DARK => ElementTheme.Dark,
                _ => ElementTheme.Light,
            };
        }

        public static ElementTheme AppTheme
        {
            get
            {
                return ParseTheme(Settings.AppTheme);
            }
            set
            {
                ThemeManager.SetRequestedTheme(App.Current.MainWindow, value);
                Settings.AppTheme = value.ToString();
                Settings.Save();
            }
        }
    }
}
