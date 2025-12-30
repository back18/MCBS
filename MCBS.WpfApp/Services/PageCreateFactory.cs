using System;
using System.Collections.Generic;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Services
{
    public class PageCreateFactory : IPageCreateFactory
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

            return _pages[pageType];
        }

        public Page CreatePage(Type pageType)
        {
            object obj = Activator.CreateInstance(pageType) ?? throw new InvalidCastException($"Unable to cast object of type {pageType} to type {PAGE_TYPE}.");
            Page page = (Page)obj;

            _pages[pageType] = page;
            return page;
        }

        public Page CreatePage(Type pageType, params object?[]? args)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            object obj = Activator.CreateInstance(pageType, args) ?? throw new InvalidCastException($"Unable to cast object of type {pageType} to type {PAGE_TYPE}.");
            Page page = (Page)obj;

            _pages[pageType] = page;
            return page;
        }

        public Page GetOrCreatePage(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            if (_pages.TryGetValue(pageType, out var page))
                return page;

            object obj = Activator.CreateInstance(pageType) ?? throw new InvalidCastException($"Unable to cast object of type {pageType} to type {PAGE_TYPE}.");
            page = (Page)obj;

            _pages.Add(pageType, page);
            return page;
        }

        public Page GetOrCreatePage(Type pageType, params object?[]? args)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            if (_pages.TryGetValue(pageType, out var page))
                return page;

            object obj = Activator.CreateInstance(pageType, args) ?? throw new InvalidCastException($"Unable to cast object of type {pageType} to type {PAGE_TYPE}.");
            page = (Page)obj;

            _pages.Add(pageType, page);
            return page;
        }
    }
}
