using iNKORE.UI.WPF.Modern.Controls.Helpers;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Helpers
{
    public static class BackdropHelper
    {
        public const string BACKDROP_NONE = nameof(BackdropType.None);
        public const string BACKDROP_MICA = nameof(BackdropType.Mica);
        public const string BACKDROP_TABBED = nameof(BackdropType.Tabbed);
        public const string BACKDROP_ACRYLIC = nameof(BackdropType.Acrylic);

        private static Properties.Settings Settings => Properties.Settings.Default;

        public static void Initialize()
        {
            WindowHelper.SetSystemBackdropType(App.Current.MainWindow, WindowBackdrop);
        }

        public static BackdropType ParseBackdrop(string backdropName)
        {
            return backdropName switch
            {
                BACKDROP_MICA => BackdropType.Mica,
                BACKDROP_TABBED => BackdropType.Tabbed,
                BACKDROP_ACRYLIC => BackdropType.Acrylic,
                _ => BackdropType.None,
            };
        }

        public static BackdropType WindowBackdrop
        {
            get
            {
                return ParseBackdrop(Settings.WindowBackdrop);
            }
            set
            {
                WindowHelper.SetSystemBackdropType(App.Current.MainWindow, value);
                Settings.WindowBackdrop = value.ToString();
                Settings.Save();
            }
        }
    }
}
