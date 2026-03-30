using iNKORE.UI.WPF.Modern.Controls;

namespace MCBS.WpfApp.Services
{
    public interface IInstancePageFactory
    {
        public Page CreateEmpty();

        public Page Create(string instanceName, INavigationProvider navigationProvider);
    }
}
