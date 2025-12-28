using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern;
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
        }

        [ObservableProperty]
        public partial string Language { get; set; }

        [ObservableProperty]
        public partial ElementTheme AppTheme { get; set; }

        partial void OnAppThemeChanged(ElementTheme value)
        {
            ThemeHelper.AppTheme = value;
        }
    }
}
