using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Helpers.Styles;
using System;
using System.Collections.Generic;
using System.Text;
using WPFLocalizeExtension.Engine;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class ApplicationSettingsViewModel : ObservableValidator
    {
        public ApplicationSettingsViewModel()
        {
            Language = LocalizeDictionary.Instance.Culture.Name;
            AppTheme = ThemeHelper.AppTheme;
            WindowBackdrop = BackdropHelper.WindowBackdrop;
        }

        [ObservableProperty]
        public partial string Language { get; set; }

        [ObservableProperty]
        public partial ElementTheme AppTheme { get; set; }

        [ObservableProperty]
        public partial BackdropType WindowBackdrop { get; set; }

        partial void OnAppThemeChanged(ElementTheme value)
        {
            ThemeHelper.AppTheme = value;
        }

        partial void OnWindowBackdropChanged(BackdropType value)
        {
            BackdropHelper.WindowBackdrop = value;
        }
    }
}
