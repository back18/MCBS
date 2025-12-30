using System;
using System.Collections.Generic;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Services
{
    public interface IPageCreateFactory
    {
        public void Register(Page page);

        public Page GetPage(Type pageType);

        public Page CreatePage(Type pageType);

        public Page CreatePage(Type pageType, params object?[]? args);

        public Page GetOrCreatePage(Type pageType);

        public Page GetOrCreatePage(Type pageType, params object?[]? args);
    }
}
