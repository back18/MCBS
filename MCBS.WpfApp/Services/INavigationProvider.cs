using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace MCBS.WpfApp.Services
{
    public interface INavigationProvider
    {
        public NavigationService NavigationService { get; }
    }
}
