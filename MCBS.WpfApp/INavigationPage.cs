using System;
using System.Collections.Generic;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp
{
    public interface INavigationPage
    {
        public Page GetParentPage();
    }
}
