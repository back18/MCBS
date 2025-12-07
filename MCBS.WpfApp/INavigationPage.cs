using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace MCBS.WpfApp
{
    public interface INavigationPage
    {
        public NavigationService NavigationService { get; }

        public INavigationPage? GetParentPage();

        public Type GetParentPageType();
    }
}
