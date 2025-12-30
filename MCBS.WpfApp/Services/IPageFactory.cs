using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Services
{
    public interface IPageFactory
    {
        public void Register(Page page);

        public Page GetPage(Type pageType);

        public TPage GetPage<TPage>() where TPage : Page, new();

        public Page GetNewPage(Type pageType);

        public TPage GetNewPage<TPage>() where TPage : Page, new();

        public bool TryGetPage(Type pageType, [MaybeNullWhen(false)] out Page page);

        public bool TryGetPage<TPage>([MaybeNullWhen(false)] out TPage page) where TPage : Page, new();

        public bool TryGetNewPage(Type pageType, [MaybeNullWhen(false)] out Page page);

        public bool TryGetNewPage<TPage>([MaybeNullWhen(false)] out TPage page) where TPage : Page, new();
    }
}
