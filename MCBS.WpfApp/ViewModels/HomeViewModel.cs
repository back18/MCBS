using MCBS.WpfApp.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.ViewModels
{
    public class HomeViewModel
    {
        public HomeViewModel(PageOneWayNavigateCommand pageOneWayNavigateCommand)
        {
            ArgumentNullException.ThrowIfNull(pageOneWayNavigateCommand, nameof(pageOneWayNavigateCommand));

            PageOneWayNavigateCommand = pageOneWayNavigateCommand;
        }

        public PageOneWayNavigateCommand PageOneWayNavigateCommand { get; }
    }
}
