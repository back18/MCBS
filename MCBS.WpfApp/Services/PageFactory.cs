using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public class PageFactory : IPageFactory
    {
        private static readonly Type PAGE_TYPE = typeof(Page);

        private readonly Dictionary<Type, Page> _pages = [];

        public void Register(Page page)
        {
            ArgumentNullException.ThrowIfNull(page, nameof(page));

            _pages.Add(page.GetType(), page);
        }

        public Page GetPage(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            if (_pages.TryGetValue(pageType, out var page))
                return page;

            object obj = Activator.CreateInstance(pageType) ?? throw new InvalidCastException($"Unable to cast object of type {pageType} to type {PAGE_TYPE}.");
            page = (Page)obj;

            _pages.Add(pageType, page);
            return page;
        }

        public TPage GetPage<TPage>() where TPage : Page, new()
        {
            Type pageType = typeof(TPage);
            if (_pages.TryGetValue(pageType, out var page) && page is TPage tpage)
                return tpage;

            tpage = new TPage();
            _pages.Add(pageType, tpage);
            return tpage;
        }

        public Page GetNewPage(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            object obj = Activator.CreateInstance(pageType) ?? throw new InvalidCastException($"Unable to cast object of type {pageType} to type {PAGE_TYPE}.");
            Page page = (Page)obj;

            _pages[pageType] = page;
            return page;
        }

        public TPage GetNewPage<TPage>() where TPage : Page, new()
        {
            Type pageType = typeof(TPage);
            TPage tpage = new();

            _pages.Add(pageType, tpage);
            return tpage;
        }

        public bool TryGetPage(Type pageType, [MaybeNullWhen(false)] out Page page)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            if (_pages.TryGetValue(pageType, out page))
                return true;

            if (pageType.IsAbstract || !PAGE_TYPE.IsAssignableFrom(pageType))
                goto err;

            ConstructorInfo? constructor = pageType.GetConstructor(Type.EmptyTypes);
            if (constructor is null)
                goto err;

            object? obj;
            try
            {
                obj = constructor.Invoke(null);
            }
            catch
            {
                goto err;
            }

            if (obj is not Page newPage)
                goto err;

            _pages.Add(pageType, newPage);
            page = newPage;
            return true;

            err:
            page = null;
            return false;
        }

        public bool TryGetPage<TPage>([MaybeNullWhen(false)] out TPage page) where TPage : Page, new()
        {
            Type pageType = typeof(TPage);
            if (_pages.TryGetValue(pageType, out var newPage) && newPage is TPage tpage)
            {
                page = tpage;
                return true;
            }

            try
            {
                tpage = new TPage();
            }
            catch
            {
                page = null;
                return false;
            }

            _pages.Add(pageType, tpage);
            page = tpage;
            return true;
        }

        public bool TryGetNewPage(Type pageType, [MaybeNullWhen(false)] out Page page)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            if (pageType.IsAbstract || !PAGE_TYPE.IsAssignableFrom(pageType))
                goto err;

            ConstructorInfo? constructor = pageType.GetConstructor(Type.EmptyTypes);
            if (constructor is null)
                goto err;

            object? obj;
            try
            {
                obj = constructor.Invoke(null);
            }
            catch
            {
                goto err;
            }

            if (obj is not Page newPage)
                goto err;

            _pages[pageType] = newPage;
            page = newPage;
            return true;

            err:
            page = null;
            return false;
        }

        public bool TryGetNewPage<TPage>([MaybeNullWhen(false)] out TPage page) where TPage : Page, new()
        {
            Type pageType = typeof(TPage);

            TPage tpage;
            try
            {
                tpage = new TPage();
            }
            catch
            {
                page = null;
                return false;
            }

            _pages[pageType] = tpage;
            page = tpage;
            return true;
        }
    }
}
