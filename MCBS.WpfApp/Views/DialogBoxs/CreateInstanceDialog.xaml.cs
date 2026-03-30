using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.ViewModels.DialogBoxs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCBS.WpfApp.Views.DialogBoxs
{
    /// <summary>
    /// CreateInstanceDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CreateInstanceDialog : ContentDialog
    {
        public CreateInstanceDialog(CreateInstanceViewModel createInstanceViewModel)
        {
            ArgumentNullException.ThrowIfNull(createInstanceViewModel, nameof(createInstanceViewModel));
            DataContext = createInstanceViewModel;

            InitializeComponent();
        }
    }
}
