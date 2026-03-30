using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IDialogBoxService<TViewModel> where TViewModel : IDialogViewModel
    {
        public Task<TViewModel> ShowAsync();
    }
}
