using CommunityToolkit.Mvvm.ComponentModel;
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
        }

        [ObservableProperty]
        public partial string Language { get; set; }
    }
}
